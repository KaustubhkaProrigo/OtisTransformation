using System;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Prorigo.Plm.DataMigration.Utilities;
using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using Prorigo.Plm.DataMigration.IO;
using Prorigo.DataMigrationTransformation.OTIS.Entities;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    class Excel_SODSDataTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Excel_SODSDataTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _excelFolderName;
        private readonly long _objectCountPerFile;
        private readonly string _processAreaDataPath;
        private readonly List<string> _expectedHeaders;

        private const string SODS = "SODS";
        public Excel_SODSDataTransformer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<Excel_SODSDataTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;


            var configSection = _configuration.GetSection("Excel_SODSData");

            _processAreaDataPath = configSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = configSection.GetValue<long>("ObjectCountPerFile");
            //_excelFileName = configSection.GetValue<string>("ExcelFileName");
            _excelFolderName = configSection.GetValue<string>("ExcelFolderName");

            var headerRow = configSection.GetValue<string>("HeaderRow") ?? "";
            _expectedHeaders = headerRow
                .Split('\t')
                .Select(h => h.Trim())
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .ToList();
        }
        public void Transform(string licenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            //License key
            bool isLicenValid = LicenseUtils.ValidateLicenKey(licenseKey, "", "DMF");
            if (isLicenValid)
            {
                var className = this.GetType().Name;
                var transformName = className.Substring(0, className.IndexOf("Transformer"));
                TransformFiles(SODS, transformName);
                ReTransformFiles(transformName);
            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

        }

        private void TransformFiles(string typeName, string transformName)
        {
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, typeName);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.InProgress);

            string[] excelFiles = Directory.GetFiles(Path.Combine(_processAreaDataPath, "BOM Files", _excelFolderName), "*.xlsx");


            if (!_expectedHeaders.Any(h => h.Equals("SODSNO", StringComparison.OrdinalIgnoreCase)))
            {
                _expectedHeaders.Insert(0, "SODSNO");
            }

            var objectCountPerFile = 0;
            var writtenSodsNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            Dictionary<string, string> sodsNoToDescription = new Dictionary<string, string>();

            // Loop through each file
            foreach (string file in excelFiles)
            {
                var outputLines = new List<string>(); // 
                bool headerWritten = false;

                string inputPath = file; // 

                var outputFolder = Path.Combine(_processAreaDataPath, "BOM Files", "SODS");
                Directory.CreateDirectory(outputFolder);

                string outputFileName = Path.GetFileNameWithoutExtension(file) + ".tsv";
                string outputPath = Path.Combine(outputFolder, outputFileName);


                string lastKnownOdsNo = null;
                using var workbook = new XLWorkbook(inputPath);
                foreach (var sheet in workbook.Worksheets)
                {

                    Console.WriteLine($"Processing sheet: {sheet.Name}");
                    var usedRange = sheet.RangeUsed();
                    if (usedRange == null || !usedRange.Rows().Any()) continue;


                    var rows = usedRange.Rows().ToList();

                    string description;
                    string sodsNo = ExtractSODSNODesp(rows, out description) ?? lastKnownOdsNo;

                    if (!string.IsNullOrWhiteSpace(sodsNo))
                    {
                        if (!sodsNoToDescription.ContainsKey(sodsNo))
                            sodsNoToDescription.Add(sodsNo, description);
                    }


                    lastKnownOdsNo = sodsNo;

                    var headerInfo = LocateHeaderRow(rows);
                    if (headerInfo == null)
                    {
                        Console.WriteLine("Valid header row not found. Skipping sheet.");
                        continue;
                    }

                    var (headerRowIndex, headers, headerMap) = headerInfo.Value;


                    // Build output headers: _expectedHeaders + custom fields like "CONDITION"
                    var outputHeaders = new List<string>(_expectedHeaders);


                    for (int i = headerRowIndex + 1; i < rows.Count;)
                    {
                        var row = rows[i];
                        if (IsRowBlank(row))
                        {
                            i++;
                            continue;
                        }

                        //string type = GetCellValue(row, headerMap, "Type")?.ToLower();
                        var values = _expectedHeaders.ToDictionary(
                            h => h,
                            h => h.Equals("SODSNO", StringComparison.OrdinalIgnoreCase) ? sodsNo : GetCellValue(row, headerMap, h)
                        );

                        int condCol1 = headerMap["Condition"];
                        int condCol2 = condCol1 + 1;

                        var childRows = CollectChildRows(rows, i, headerMap);
                        var sectionOutput = ProcessTypeX(values, childRows, condCol1, condCol2, headerMap, outputHeaders);
                        if (!headerWritten)
                        {
                            outputLines.Add(string.Join("\t", outputHeaders));
                            headerWritten = true;
                        }

                        if (sectionOutput.Count > 0)
                        {
                            outputLines.AddRange(sectionOutput);
                        }
                        else
                        {
                            string defaultLine = FormatOutputRow(values, "", outputHeaders);
                            outputLines.Add(defaultLine);
                        }

                        i += childRows.Count; // Skip processed rows
                    }

                }

                File.WriteAllLines(outputPath, outputLines);

                Console.WriteLine($"Transformation complete. Output written to: {outputPath}");

                _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.Completed);
                _migrationDiagnostics.LogTransformTypeEndTime(transformName, typeName);



            }
            // After processing sheets:
            WriteSODSCalculationSheet(sodsNoToDescription);
            WriteBreakdownItemToCalculationSheet(sodsNoToDescription);
        }

        private (int index, List<string> headers, Dictionary<string, int> map)? LocateHeaderRow(List<IXLRangeRow> rows)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                var headers = rows[i].Cells().Select(c => c.GetString().Trim()).ToList();

                // Remove SODSNO if present in _expectedHeaders for matching
                var expectedHeadersFiltered = _expectedHeaders
                    .Where(h => !h.Equals("SODSNO", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var headersFiltered = headers
                    .Where(h => !h.Equals("SODSNO", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (expectedHeadersFiltered.All(h => headersFiltered.Contains(h, StringComparer.OrdinalIgnoreCase)))
                {
                    // Build map based on actual header row positions
                    var map = _expectedHeaders.ToDictionary(
                        h => h,
                        h =>
                        {
                            int index = headers.FindIndex(x => x.Equals(h, StringComparison.OrdinalIgnoreCase));
                            return index >= 0 ? index + 1 : -1; // +1 for 1-based index like Excel
                        }
                    );

                    return (i, headers, map);
                }
            }

            return null;
        }


        private List<IXLRangeRow> CollectChildRows(List<IXLRangeRow> allRows, int startIndex, Dictionary<string, int> headerMap)
        {
            var result = new List<IXLRangeRow>();
            string currentId = null;

            for (int i = startIndex; i < allRows.Count; i++)
            {
                var row = allRows[i];

                if (IsRowBlank(row))
                    continue; // Skip completely blank rows

                var cell = row.Cell(headerMap["ID"]);
                string id = cell.GetString().Trim();

                if (string.IsNullOrEmpty(currentId))
                {
                    currentId = id; // First valid row sets the ID
                }
                else if (!string.IsNullOrEmpty(id) && !string.Equals(currentId, id, StringComparison.OrdinalIgnoreCase))
                {
                    break; // Stop if new ID is different
                }

                // If current row's ID is blank, set it to currentId for consistency
                if (string.IsNullOrEmpty(id))
                {
                    cell.Value = currentId;
                }

                result.Add(row);
            }

            return result;
        }

        private string GetCellValue(IXLRangeRow row, Dictionary<string, int> headerMap, string key)
        {
            return (headerMap.TryGetValue(key, out int index) && index > 0)
                ? row.Cell(index).GetString().Trim()
                : "";
        }



        private bool IsRowBlank(IXLRangeRow row)
        {
            return row.Cells().All(c => string.IsNullOrWhiteSpace(c.GetString()));
        }


        private string BuildConditionString(IXLRangeRow row, int col1, int col2)
        {
            var val1 = row.Cell(col1).GetString().Trim();
            var val2 = row.Cell(col2).GetString().Trim();

            var parts = new List<string>();
            if (!string.IsNullOrEmpty(val1)) parts.Add(val1);
            if (!string.IsNullOrEmpty(val2)) parts.Add(val2);

            int nextCol = col2 + 1;
            int blankCount = 0;

            while (row.LastCellUsed()?.Address.ColumnNumber >= nextCol && blankCount < 2)
            {
                string value = row.Cell(nextCol).GetString().Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    parts.Add(value);
                    blankCount = 0;
                }
                else blankCount++;

                nextCol++;
            }

            return string.Join("|", parts);
        }

        private string FormatOutputRow(
            Dictionary<string, string> rowData,
            string condition,
            List<string> outputHeaders)
        {
            return string.Join("\t", outputHeaders.Select(header =>
            {
                if (header.Equals("CONDITION", StringComparison.OrdinalIgnoreCase))
                    return condition?.Trim() ?? "";

                if (rowData.TryGetValue(header, out var value))
                    return value?.Trim() ?? "";

                return "";
            }));
        }

        private List<string> ProcessTypeX(
            Dictionary<string, string> parentRow,
            List<IXLRangeRow> childRows,
            int conditionCol1,
            int conditionCol2,
            Dictionary<string, int> headerMap,
            List<string> outputHeaders)
        {
            var output = new List<string>();
            string lastKnownId = parentRow.GetValueOrDefault("ID", "");

            foreach (var child in childRows)
            {
                if (IsRowBlank(child))
                    continue;

                string rawId = child.Cell(headerMap["ID"]).GetString().Trim();
                string childId = string.IsNullOrEmpty(rawId) ? lastKnownId : rawId;
                lastKnownId = childId; // update only if new ID found

                string condition = BuildConditionString(child, conditionCol1, conditionCol2);
                if (string.IsNullOrWhiteSpace(condition))
                    continue;

                string finalLine = FormatOutputRow(parentRow, condition, outputHeaders);

                output.Add(finalLine);
            }

            return output;
        }


        private void WriteSODSCalculationSheet(Dictionary<string, string> sodsNoToDescription)
        {
            using var SODSCalculationSheetWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, _excelFolderName), _objectCountPerFile)
            {
                FileBaseName = "SODSCalculationSheet",
                TypeName = "SODSCalculationSheet",
                FileExtension = "tsv"
            };

            if (SODSCalculationSheetWriter.HeaderRow == null)
            {
                // Add DESCRIPTION header column
                SODSCalculationSheetWriter.HeaderRow = "Id\tARAS_UNIQUENESS_HELPER\tCONFIG_ID\tITEM_NUMBER\tOTS_Name\tKEYED_NAME\tDESCRIPTION\tCREATED_ON\tCREATED_BY_ID\tPERMISSION_ID\tIS_CURRENT\tIS_RELEASED\tMAJOR_REV\tCLASSIFICATION\tNOT_LOCKABLE\tGENERATION\tSTATE\tCURRENT_STATE\n";
            }

            foreach (var kvp in sodsNoToDescription)
            {
                var sodsNo = kvp.Key;
                var description = kvp.Value ?? "";

                var Id = TransformerUtils.GetNewArasGuid();
                var ARAS_UNIQUENESS_HELPER = Id;
                var CONFIG_ID = Id;
                var ITEM_NUMBER = sodsNo;
                var KEYED_NAME = ITEM_NUMBER;
                var CREATED_ON = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var CREATED_BY_ID = "Data Migration";
                var PERMISSION_ID = "95475AE006E7415794BDC93808DC04D2";
                var IS_CURRENT = "1";
                var IS_RELEASED = "1";
                var MAJOR_REV = "A";
                var CLASSIFICATION = "SODS";
                var NOT_LOCKABLE = "0";
                var GENERATION = "1";
                var STATE = "Released";
                var CURRENT_STATE = "95475AE006E7415794BDC93808DC04D2";
                var OTS_Name = ITEM_NUMBER;

                SODSCalculationSheetWriter.WriteRow(
                    $"{Id}\t{ARAS_UNIQUENESS_HELPER}\t{CONFIG_ID}\t{ITEM_NUMBER}\t{OTS_Name}\t{KEYED_NAME}\t{description}\t{CREATED_ON}\t{CREATED_BY_ID}\t{PERMISSION_ID}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{CLASSIFICATION}\t{NOT_LOCKABLE}\t{GENERATION}\t{STATE}\t{CURRENT_STATE}\n");
            }

            SODSCalculationSheetWriter.Dispose();
        }
        private void WriteBreakdownItemToCalculationSheet(Dictionary<string, string> sodsNoToDescription)
        {
            using var BreabkdonItemToCalculationSheetWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, _excelFolderName), _objectCountPerFile)
            {
                FileBaseName = "VM_BreakdownItemToCalculationSheet",
                TypeName = "VM_BreakdownItemToCalculationSheet",
                FileExtension = "tsv"
            };
            var failedFilesDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "SODS", "VM_BreakdownItemToCalculationSheet"), _objectCountPerFile)
            {
                FileBaseName = $"TR_Failed_BreakdownItemToCalculationSheet_MetaData",
                TypeName = $"TR_Failed_BreakdownItemToCalculationSheet_MetaData",
                HeaderRow = "ProductNumber\tSODSNumber\tErrorDescription\n",
                FileExtension = "tsv"
            };
            var ProductDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "Product"));
            var ProductEntities = ProductDataReader.ReadAllEntities<OtisBreakdownProductEntity>("BreakdownItem_Product", "*.tsv");

            var SODSDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath,"SODS"));
            var SODSEntities = SODSDataReader.ReadAllEntities<OtisSODSEntity>("SODSCalculationSheet","*.tsv");

            var Product = ProductEntities.ToDictionary(e => e.ITEM_NUMBER, e => e.ID);
            var SODS = SODSEntities.ToDictionary(e => e.ITEM_NUMBER, e => e.ID);
            if (BreabkdonItemToCalculationSheetWriter.HeaderRow == null)
            {
                BreabkdonItemToCalculationSheetWriter.HeaderRow = "ConnectionId\tCONFIG_ID\tKEYED_NAME\tCREATED_ON\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tIS_CURRENT\tMAJOR_REV\tSTATE\tIS_RELEASED\tNOT_LOCKABLE\tGENERATION\tPERMISSION_ID\tSOURCE_ID\tRELATED_ID\n";
            }
            foreach (var kvp in sodsNoToDescription)
            {
                var SourceID = "";
                var RelatedID = "";
                var sodsNo = kvp.Key; // RelatedID
                if (!sodsNo.Contains("-"))
                    continue; // Skip processing if ID does not contain '-'
                string ProductNO = sodsNo.Split('-')[0]; // SourceID

                if (SODS.ContainsKey(sodsNo) && Product.ContainsKey(ProductNO))
                {
                    RelatedID = SODS[sodsNo];
                    SourceID = Product[ProductNO];
                }
                else
                {
                    if (!SODS.ContainsKey(sodsNo))
                    {
                        if (!Product.ContainsKey(ProductNO))
                        {
                            failedFilesDataFileWriter.WriteRow($"{ProductNO}\t{sodsNo}\tMissing Product And SODS\n");
                            continue;
                        }
                        failedFilesDataFileWriter.WriteRow($"{ProductNO}\t{sodsNo}\tMissing SODS\n");
                        continue;
                    }
                    else
                    {
                        failedFilesDataFileWriter.WriteRow($"{ProductNO}\t{sodsNo}\tMissing Product\n");
                        continue;
                    }
                }
                var ConnectionId = TransformerUtils.GetNewArasGuid();
                var ConfigId = ConnectionId;
                var KEYED_NAME = ConnectionId;
                var PERMISSION_ID = "95475AE006E7415794BDC93808DC04D2";
                var CREATED_ON = DateTime.Now;
                var CREATED_BY_ID = "Data Migration";
                var MODIFIED_ON = DateTime.Now.ToString();
                var MODIFIED_BY_ID = "Data Migration";
                var IS_RELEASED = "1";
                var STATE = "Released";
                var IS_CURRENT = "1";
                var MAJOR_REV = "A";
                var NOT_LOCKABLE = "0";
                var GENERATION = "1";
                BreabkdonItemToCalculationSheetWriter.WriteRow($"{ConnectionId}\t{ConfigId}\t{KEYED_NAME}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{IS_CURRENT}\t{MAJOR_REV}\t{STATE}\t{IS_RELEASED}\t{NOT_LOCKABLE}\t{GENERATION}\t{PERMISSION_ID}\t{SourceID}\t{RelatedID}\n");
            }
            BreabkdonItemToCalculationSheetWriter.Dispose();
        }
        private string ExtractSODSNODesp(List<IXLRangeRow> rows, out string description)
        {
            description = null;
            foreach (var row in rows)
            {
                var cells = row.Cells().ToList();
                foreach (var cell in cells)
                {
                    if (cell.GetString().Trim().Equals("SODS", StringComparison.OrdinalIgnoreCase))
                    {
                        int rowNumber = cell.Address.RowNumber;
                        int colIndex = cell.Address.ColumnNumber;
                        var worksheet = cell.Worksheet;

                        for (int j = colIndex + 1; j <= row.LastCellUsed().Address.ColumnNumber; j++)
                        {
                            var targetCell = worksheet.Cell(rowNumber, j);
                            string value = targetCell.GetString().Trim();

                            if (!string.IsNullOrEmpty(value))
                            {
                                var aboveCell = worksheet.Cell(rowNumber - 1, j);
                                string aboveText = aboveCell.GetString().Trim();

                                if (aboveText.Equals("SODS Number", StringComparison.OrdinalIgnoreCase))
                                {
                                    description = worksheet.Cell(rowNumber, j + 7).GetString().Trim(); // 8th cell to right as you want
                                    return value;
                                }

                            }
                        }
                    }
                }
            }
            return null;
        }

        private void ReTransformFiles(string transformName)
        {
            var ODSDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "BOM Files"));
            var ODSEntities = ODSDataReader.ReadAllEntities<SODSDataExtractEntity>("SODS", "*.tsv");

            var SODSDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "BOM Files"), _objectCountPerFile)
            {
                FileBaseName = $"TR_SODSData",
                TypeName = $"SODS",
                HeaderRow = "SODSNO\tID\tSS No.\tDescription\tCondition\tODS No.\tQty.\tExpressionID\n",
                FileExtension = "tsv"
            };
            var groupedBySodsNo = ODSEntities.GroupBy(e => e.SODSNO);

            using (SODSDataFileWriter)
            {
                foreach (var group in groupedBySodsNo)
                {
                    int ExpId = 10001;

                    // Group further by ID within each SODSNO section
                    var groupedById = group.GroupBy(e => e.ID?.Trim() ?? "");

                    foreach (var idGroup in groupedById)
                    {
                        bool hasMultipleRows = idGroup.Count() > 1;

                        foreach (var (entity, index) in idGroup.Select((e, i) => (e, i)))
                        {
                            // Only skip ExpId for first row if there are multiple rows
                            string expIdValue = (index == 0 && hasMultipleRows) ? "" : ExpId.ToString();

                            SODSDataFileWriter.WriteRow($"{entity.SODSNO}\t{entity.ID}\t{entity.SS_No}\t{entity.Description}\t{entity.Condition}\t{entity.ODS_No}\t{entity.Qty}\t{expIdValue}\n");

                            if (index > 0 || !hasMultipleRows)
                                ExpId++;
                        }
                    }
                }
            
                    var DirectoryInfo = new DirectoryInfo(Path.Combine(_processAreaDataPath,"BOM Files", "SODS"));
                var originalFiles = DirectoryInfo.GetFiles("*.tsv").Where(f => !f.Name.StartsWith("TR_", StringComparison.OrdinalIgnoreCase));
   
                foreach (FileInfo file in originalFiles)
                {
                    file.Delete();
                }


            }


        }
    }
}
