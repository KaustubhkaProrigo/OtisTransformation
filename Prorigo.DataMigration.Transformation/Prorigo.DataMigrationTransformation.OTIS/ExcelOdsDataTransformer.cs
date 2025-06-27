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
    class ExcelOdsDataTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExcelOdsDataTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly long _objectCountPerFile;
        private readonly string _processAreaDataPath;
        private readonly List<string> _expectedHeaders;

        private const string ODS = "ODS";
        public ExcelOdsDataTransformer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<ExcelOdsDataTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;


            var configSection = _configuration.GetSection("ExcelOdsData");

            _processAreaDataPath = configSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = configSection.GetValue<long>("ObjectCountPerFile");

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
                TransformFiles(ODS, transformName);
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

            _expectedHeaders.Add("Drawing Number");

            //string[] excelFiles = Directory.GetFiles(_processAreaDataPath, "*.xlsx");
            string[] excelFiles = Directory
                                    .EnumerateFiles(Path.Combine(_processAreaDataPath, "BOM Files","ODS"))
                                    .Where(file => file.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                                                   file.EndsWith(".xlsm", StringComparison.OrdinalIgnoreCase))
                                    .ToArray();


            var outputLines = new List<string>();
            bool headerWritten = false;

            // Loop through each file
            foreach (string file in excelFiles)
            {
                string inputPath = Path.Combine(file);

                var outputFolder = Path.Combine(_processAreaDataPath, "BOM Files", "ODS");
                Directory.CreateDirectory(outputFolder);

                string outputPath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension("ODSData") + ".tsv");

                using var workbook = new XLWorkbook(inputPath);
                string lastKnownOdsNo = null;

                foreach (var sheet in workbook.Worksheets)
                {
                    Console.WriteLine($"Processing sheet: {sheet.Name}");
                    var usedRange = sheet.RangeUsed();
                    if (usedRange == null) continue;

                    var rows = usedRange.Rows().ToList();
                    if (rows.Count == 0) continue;

                    string odsNo = ExtractODSNO(rows);

                    odsNo = ExtractODSNO(rows) ?? lastKnownOdsNo;

                    if (string.IsNullOrEmpty(odsNo))
                    {
                        Console.WriteLine("ODS No not found and no previous ODS No available. Skipping sheet.");
                        continue;
                    }

                    lastKnownOdsNo = odsNo;

                    var headerInfo = LocateHeaderRow(rows);
                    if (headerInfo == null)
                    {
                        Console.WriteLine("Valid header row not found. Skipping sheet.");
                        continue;
                    }

                    var (headerRowIndex, headers, headerMap) = headerInfo.Value;

                    if (!_expectedHeaders.Any(h => h.Equals("ODSNO", StringComparison.OrdinalIgnoreCase)))//insert ods as header at 0 pos
                    {
                        _expectedHeaders.Insert(0, "ODSNO");
                    }

                    if (!headerWritten)
                    {
                        outputLines.Add(string.Join("\t", _expectedHeaders.Take(_expectedHeaders.Count - 1)));
                        headerWritten = true;
                    }

                    for (int i = headerRowIndex + 1; i < rows.Count; i++)
                    {
                        var row = rows[i];
                        if (row.CellsUsed().All(c => string.IsNullOrWhiteSpace(c.GetString()))) continue;

                        string type = GetCellValue(row, headerMap, "Type")?.ToLower();

                        var values = _expectedHeaders.ToDictionary(
                            h => h,
                            h => h.Equals("ODSNO", StringComparison.OrdinalIgnoreCase) ? odsNo : GetCellValue(row, headerMap, h)
                        );

                        if (type == "x")
                        {
                            int condCol1 = headerMap["Condition"];
                            int condCol2 = condCol1 + 1;

                            var childRows = CollectChildRows(rows, i + 1, headerMap);
                            var sectionOutput = ProcessTypeX(values, childRows, condCol1, condCol2, headerMap);
                            outputLines.AddRange(sectionOutput);
                        }
                        else if (type == "y")
                        {
                            int condCol1 = headerMap["Condition"];
                            int condCol2 = condCol1 + 1;
                            var childRows = CollectChildRows(rows, i + 1, headerMap);
                            var sectionOutput = ProcessTypeY(values, childRows);
                            outputLines.AddRange(sectionOutput);
                        }
                        else if (type == "xy")
                        {
                            int condCol1 = headerMap["Condition"];
                            int condCol2 = condCol1 + 1;

                            var childRows = CollectChildRows(rows, i + 1, headerMap);
                            var parentHeaders = headerMap.Keys.ToList();

                            var sectionOutput = ProcessTypeXY(values, childRows);
                            outputLines.AddRange(sectionOutput);
                        }
                        else if (type == "" && !string.IsNullOrWhiteSpace(values["ID"].ToString()))
                        {
                            string Drawing = values["Drawing Number"].ToString();
                            if (Drawing == "")
                                Drawing = values["P / N: Part Number"].ToString();

                            string finalLine = FormatOutputRow(values, Drawing);
                            outputLines.Add(finalLine);
                        }

                    }
                    _expectedHeaders.Remove("ODSNO");
                    _expectedHeaders.Remove("Drawing Number");


                }

                File.WriteAllLines(outputPath, outputLines);
                Console.WriteLine($"Transformation complete. Output written to: {outputPath}");

                _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.Completed);
                _migrationDiagnostics.LogTransformTypeEndTime(transformName, typeName);
            }
        }

        private string ExtractODSNO(List<IXLRangeRow> rows)
        {
            foreach (var row in rows)
            {
                var cells = row.Cells().ToList();
                for (int i = 0; i < cells.Count - 1; i++)
                {
                    if (cells[i].GetString().Trim().Equals("ODS No:", StringComparison.OrdinalIgnoreCase))
                        return cells[i + 1].GetString().Trim();
                }
            }
            return null;
        }

        private (int index, List<string> headers, Dictionary<string, int> map)? LocateHeaderRow(List<IXLRangeRow> rows)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                var headers = rows[i].Cells().Select(c => c.GetString().Trim()).ToList();
                if (_expectedHeaders.All(h => headers.Contains(h, StringComparer.OrdinalIgnoreCase)))
                {
                    var map = _expectedHeaders.ToDictionary(
                        h => h,
                        h => headers.FindIndex(x => x.Equals(h, StringComparison.OrdinalIgnoreCase)) + 1
                    );
                    return (i, headers, map);
                }
            }
            return null;
        }//headers

        private List<IXLRangeRow> CollectChildRows(List<IXLRangeRow> allRows, int startIndex, Dictionary<string, int> headerMap)
        {
            var result = new List<IXLRangeRow>();
            string currentId = null;

            for (int i = startIndex; i < allRows.Count; i++)
            {
                var row = allRows[i];

                string id = row.Cell(headerMap["ID"]).GetString().Trim();
                if (!string.IsNullOrEmpty(currentId) && currentId != id)
                    break;

                result.Add(row);
                currentId = id;
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
            if (!string.IsNullOrEmpty(val1)) parts.Add(val1.Replace("\t", "|t|").Replace("\n", "|n|").Replace("\r", "|r|").Trim());
            if (!string.IsNullOrEmpty(val2)) parts.Add(val2.Replace("\t", "|t|").Replace("\n", "|n|").Replace("\r", "|r|").Trim());

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

        private string FormatOutputRow(Dictionary<string, string> parentRow, string condition)
        {
            return $"{parentRow.GetValueOrDefault("ODSNO", "")}\t" +
                   $"{parentRow.GetValueOrDefault("ID", "")}\t" +
                   $"{parentRow.GetValueOrDefault("Remark", "")}\t" +
                   $"{condition}\t" +
                   $"{parentRow.GetValueOrDefault("Table Type", "")}\t" +
                   $"{parentRow.GetValueOrDefault("Type", "")}\t" +
            $"{parentRow.GetValueOrDefault("Qty", "")}";

        }

        private List<string> ProcessTypeX(
            Dictionary<string, string> parentRow,
            List<IXLRangeRow> childRows,
            int conditionCol1,
            int conditionCol2,
            Dictionary<string, int> headerMap)
        {
            var output = new List<string>();
            string currentId = null;
            var sectionBuffer = new List<string>();

            foreach (var child in childRows)
            {
                if (IsRowBlank(child))
                    continue;

                string childId = child.Cell(headerMap["ID"]).GetString().Trim();
                if (!string.IsNullOrEmpty(currentId) && childId != currentId)
                {
                    output.AddRange(sectionBuffer);
                    sectionBuffer.Clear();
                }

                string condition = BuildConditionString(child, conditionCol1, conditionCol2);

                if (string.IsNullOrWhiteSpace(condition))
                    continue;

                string finalLine = FormatOutputRow(parentRow, condition);
                sectionBuffer.Add(finalLine);

                currentId = childId;
            }

            if (sectionBuffer.Count > 0)
                output.AddRange(sectionBuffer);

            return output;
        }

        private List<string> ProcessTypeY(
                    Dictionary<string, string> parentRow,
                    List<IXLRangeRow> childRows)
        {
            var output = new List<string>();

            if (childRows == null || childRows.Count == 0)
                return output;

            bool IsBlankRow(IXLRangeRow row) =>
                row.Cells().All(cell => string.IsNullOrWhiteSpace(cell.GetString()));

            var sections = new List<List<IXLRangeRow>>();
            var currentSection = new List<IXLRangeRow>();

            foreach (var row in childRows)
            {
                if (IsBlankRow(row))
                {
                    if (currentSection.Count > 0)
                    {
                        sections.Add(new List<IXLRangeRow>(currentSection));
                        currentSection.Clear();
                    }
                }
                else
                {
                    currentSection.Add(row);
                }
            }
            if (currentSection.Count > 0)
                sections.Add(currentSection);

            for (int sectionIndex = 0; sectionIndex < sections.Count; sectionIndex++)
            {
                var section = sections[sectionIndex];
                if (section.Count == 0)
                    continue;

                var fieldNames = section.Select(r => r.Cell(1).GetString().Trim()).ToList();
                int maxDataColumns = section.Max(r => r.LastCellUsed()?.Address.ColumnNumber ?? 2);

                for (int col = 2; col <= maxDataColumns; col++)
                {
                    var conditionParts = new List<string>();

                    for (int rowIdx = 0; rowIdx < section.Count; rowIdx++)
                    {
                        var row = section[rowIdx];
                        string fieldName = fieldNames[rowIdx];

                        if (!string.IsNullOrWhiteSpace(fieldName))
                            break;

                        string cellValue = row.Cell(col).GetString().Trim().Replace("\t", "|t|").Replace("\n", "|n|").Replace("\r", "|r|");//trim tab and \n

                        if (!string.IsNullOrEmpty(cellValue))
                        {
                            conditionParts.Add($"{fieldName} {cellValue}");
                        }
                    }

                    if (conditionParts.Count == 0)
                        continue;

                    string conditionGroup = string.Join("|", conditionParts);
                    string outputLine = $"{FormatOutputRow(parentRow, conditionGroup)}";
                    output.Add(outputLine);
                }
            }

            return output;
        }

        private List<string> ProcessTypeXY(
                Dictionary<string, string> parentRow,
                List<IXLRangeRow> childRows)
        {
            var output = new List<string>();
            if (childRows == null || childRows.Count == 0) return output;

            var sections = new List<List<IXLRangeRow>>();
            var currentSection = new List<IXLRangeRow>();

            foreach (var row in childRows)
            {
                if (IsRowBlank(row))
                {
                    if (currentSection.Count > 0)
                    {
                        sections.Add(new List<IXLRangeRow>(currentSection));
                        currentSection.Clear();
                    }
                }
                else
                {

                    currentSection.Add(row);
                }
            }
            if (currentSection.Count > 0)
                sections.Add(currentSection);

            foreach (var section in sections)
            {
                const string pnLabel = "PN";
                int pnRowIdx = -1, pnColIdx = -1;

                // Locate PN cell
                for (int r = 0; r < section.Count && pnRowIdx == -1; r++)
                {
                    foreach (var cell in section[r].CellsUsed())
                    {
                        if (cell.GetString().Trim().Equals(pnLabel, StringComparison.OrdinalIgnoreCase))
                        {
                            pnRowIdx = r;
                            pnColIdx = cell.Address.ColumnNumber;
                            break;
                        }
                    }
                }

                if (pnRowIdx <= 0 || pnColIdx == -1) return output;

                int mrtColIdx = pnColIdx - 2;
                var headerRow = section[pnRowIdx];
                var xHeaders = Enumerable.Range(1, mrtColIdx)
                                         .Select(c => headerRow.Cell(c).GetString().Trim())
                                         .Where(v => !string.IsNullOrEmpty(v)).ToList();

                var yHeaders = new List<string>();
                for (int r = pnRowIdx - 1; r >= 0; r--)
                {
                    var val = section[r].Cell(mrtColIdx).GetString().Trim();
                    if (!string.IsNullOrEmpty(val)) yHeaders.Add(val);
                }
                yHeaders.Reverse();

                string pnHeader = headerRow.Cell(pnColIdx).GetString().Trim();
                string qtHeader = headerRow.Cell(pnColIdx + 1).GetString().Trim();

                var yValues = new Dictionary<int, string>();

                int emptyRows = 0;

                for (int rowIdx = pnRowIdx - 1; rowIdx >= 0 && emptyRows < 2; rowIdx--)
                {

                    var row = section[rowIdx];
                    bool hasData = false;
                    int emptyCols = 0;

                    for (int col = pnColIdx; emptyCols < 2; col++)
                    {
                        string yval = row.Cell(col).GetString().Trim();
                        if (string.IsNullOrEmpty(yval))
                        {
                            emptyCols++;
                        }
                        else
                        {
                            emptyCols = 0;
                            hasData = true;
                            yValues[col] = yValues.ContainsKey(col) ? yval + "|" + yValues[col] : yval;
                        }

                    }

                    if (!hasData) emptyRows++;
                    else emptyRows = 0;

                }

                bool headerWritten = false;

                foreach (var kvp in yValues)
                {
                    int pnCol = kvp.Key;
                    int qtCol = pnCol + 1;
                    string ytype = kvp.Value;

                    // Check if QT column exists and is valid
                    bool hasQtColumn = section[pnRowIdx].CellCount() > qtCol &&
                                       string.Equals(section[pnRowIdx].Cell(qtCol).GetString().Trim(), "QT", StringComparison.OrdinalIgnoreCase);

                    for (int rowIdx = pnRowIdx + 1; rowIdx < section.Count; rowIdx++)
                    {
                        var row = section[rowIdx];
                        string pnVal = row.Cell(pnCol).GetString().Trim();

                        if (string.IsNullOrEmpty(pnVal)) continue;

                        string qtVal = hasQtColumn ? row.Cell(qtCol).GetString().Trim() : "";
                        var xValues = Enumerable.Range(1, mrtColIdx)
                            .Select(col => row.Cell(col).GetString().Trim())
                            .Where(val => !string.IsNullOrEmpty(val)).ToList();

                        //var parts = new List<string>();
                        //parts.AddRange(xValues);
                        //parts.Add(ytype);
                        //parts.Add(pnVal);
                        var parts = new List<string>();
                        parts.AddRange(xValues.Select(val => (val ?? string.Empty).Replace("\t", "|t|").Replace("\n", "|n|").Replace("\r", "|r|")));
                        parts.Add((ytype ?? string.Empty).Replace("\t", "|t|").Replace("\n", "|n|").Replace("\r", "|r|"));
                        parts.Add((pnVal ?? string.Empty).Replace("\t", "|t|").Replace("\n", "|n|").Replace("\r", "|r|"));


                        if (hasQtColumn)
                            parts.Add(qtVal); // Only add QT value if it exists

                        var condition = string.Join(" | ", parts);

                        if (!headerWritten)
                        {
                            var headerParts = new List<string>();
                            headerParts.AddRange(xHeaders);
                            headerParts.AddRange(yHeaders);
                            headerParts.Add(pnHeader); // Always include PN header

                            if (hasQtColumn)
                                headerParts.Add(qtHeader); // Only include QT header if it exists

                            var conditionHeader = string.Join(" | ", headerParts);
                            output.Add(FormatOutputRow(parentRow, conditionHeader));

                            headerWritten = true;
                        }

                        output.Add(FormatOutputRow(parentRow, condition));

                    }

                }
            }
            return output;
        }

        private void ReTransformFiles(string transformName)
        {
            var ODSDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath,"BOM Files"));
            var ODSEntities = ODSDataReader.ReadAllEntities<ODSDataExtractEntity>("ODS", "*.tsv");

            var ODSDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath,"BOM Files"), _objectCountPerFile)
            {
                FileBaseName = $"TR_ODSData_MetaData",
                TypeName = $"ODS",
                HeaderRow = "ODSNo\tID\tRemark\tCondition\tTable Type\tType\tQty\tExpressionID\n",
                FileExtension = "tsv"
            };

            var groupedByOdsNo = ODSEntities.GroupBy(e => e.ODSNo);
            //int ExpId = 10001;
            var seenFirstRowForId = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (ODSDataFileWriter)
            {
                foreach (var group in groupedByOdsNo)
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

                            ODSDataFileWriter.WriteRow($"{entity.ODSNo}\t{entity.ID}\t{entity.Remark}\t{entity.Condition}\t{entity.TableType}\t{entity.Type}\t{entity.QT}\t{expIdValue}\n");
                            if (index > 0 || !hasMultipleRows)
                                ExpId++;
                        }
                    }
                }
            }



            var DirectoryInfo = new DirectoryInfo(Path.Combine(_processAreaDataPath, "BOM Files","ODS"));
            var originalFiles = DirectoryInfo.GetFiles("*.tsv").Where(f => !f.Name.StartsWith("TR_", StringComparison.OrdinalIgnoreCase));

            foreach (FileInfo file in originalFiles)
            {
                file.Delete();
            }

        }
    }
}
