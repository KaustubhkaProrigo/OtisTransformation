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
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Data;


namespace Prorigo.DataMigrationTransformation.OTIS
{
    class ExcelToTsvSODSOutputTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExcelToTsvSODSOutputTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly List<string> _expectedHeaders;


        private const string SODS = "SODS";
        public ExcelToTsvSODSOutputTransformer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<ExcelToTsvSODSOutputTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;


            var configSection = _configuration.GetSection("ExcelToTsvSODSOutput");
            _processAreaDataPath = configSection.GetValue<string>("ProcessAreaDataPath");

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

            string[] excelFiles = Directory.GetFiles(Path.Combine(_processAreaDataPath,"BOM Files", "SODS"), "*.xlsx");

            if (!_expectedHeaders.Any(h => h.Equals("SODSNO", StringComparison.OrdinalIgnoreCase)))
            {
                _expectedHeaders.Insert(0, "SODSNO");
            }
            var writtenSodsNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string file in excelFiles)
            {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
                var outputLines = new List<string>();
                bool headerWritten = false;

                string inputPath = Path.Combine(file);

                var outputFolder = Path.Combine(_processAreaDataPath,"BOM Files", "SODS" ,"SODSOutput");
                Directory.CreateDirectory(outputFolder);

                string outputFileName = Path.GetFileNameWithoutExtension(file) + ".tsv";
                string outputPath = Path.Combine(outputFolder, outputFileName);

                using var workbook = new XLWorkbook(inputPath);
                string lastKnownsodsNo = null;

                foreach (var sheet in workbook.Worksheets)
                {
                    Console.WriteLine($"Processing sheet: {sheet.Name}");
                    var usedRange = sheet.RangeUsed();
                    if (usedRange == null || !usedRange.Rows().Any()) continue;

                    var rows = usedRange.Rows().ToList();
                    string sodsNo = ExtractSODSNO(rows) ?? lastKnownsodsNo;

                    lastKnownsodsNo = sodsNo;

                    var headerInfo = LocateHeaderRow(rows);
                    if (headerInfo == null)
                    {
                        Console.WriteLine("Valid header row not found. Skipping sheet.");
                        continue;
                    }

                    var (headerRowIndex, headers, headerMap) = headerInfo.Value;
                    var outputHeaders = new List<string>(_expectedHeaders);
                    for (int i = headerRowIndex + 1; i < rows.Count; i++)
                    {
                        var row = rows[i];
                        var childRows = CollectChildRows(rows, i, headerMap);

                        int condCol1 = headerMap["Condition / Formula Detail"];
                        int condCol2 = condCol1 + 2;
                        int valueCol = condCol2 + 17;

                        var allRowsToCheck = new List<IXLRangeRow> { row };
                        allRowsToCheck.AddRange(childRows);

                        bool allRowsBlank = allRowsToCheck.All(r =>
                            Enumerable.Range(1, valueCol)
                                      .All(col => string.IsNullOrWhiteSpace(r.Cell(col).GetString()))
                        );

                        if (allRowsBlank)
                        {
                            i += childRows.Count;
                            continue;
                        }

                        var values = _expectedHeaders.ToDictionary(
                            h => h,
                            h => h.Equals("SODSNO", StringComparison.OrdinalIgnoreCase)
                                ? sodsNo
                                : GetCellValue(row, headerMap, h)
                        );

                        var childRowPairs = childRows
                        .Select(child => (Row: child, ValueColValue: child.Cell(valueCol).GetString().Trim()))
                        .ToList();

                        var sectionOutput = ProcessTypeX(values, childRowPairs, condCol1, condCol2, headerMap, outputHeaders);

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
                        i += childRows.Count;
                    }
                }

                File.WriteAllLines(outputPath, outputLines);
                Console.WriteLine($"Transformation complete. Output written to: {outputPath}");

                _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.Completed);
                _migrationDiagnostics.LogTransformTypeEndTime(transformName, typeName);
            }
        }

        private string ExtractSODSNO(List<IXLRangeRow> rows)
        {
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
                                    return value;
                                }
                            }
                        }
                    }
                }
            }
            return null; ;
        }

        private (int index, List<string> headers, Dictionary<string, int> map)? LocateHeaderRow(List<IXLRangeRow> rows)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                var headers = rows[i].Cells().Select(c => c.GetString().Trim()).ToList();

                var expectedHeadersFiltered = _expectedHeaders
                    .Where(h => !h.Equals("SODSNO", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var headersFiltered = headers
                    .Where(h => !h.Equals("SODSNO", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (expectedHeadersFiltered.All(h => headersFiltered.Contains(h, StringComparer.OrdinalIgnoreCase)))
                {
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
        }//headers

        private List<IXLRangeRow> CollectChildRows(List<IXLRangeRow> allRows, int startIndex, Dictionary<string, int> headerMap)
        {
            var result = new List<IXLRangeRow>();
            string currentId = null;

            for (int i = startIndex; i < allRows.Count; i++)
            {
                var row = allRows[i];

                if (IsRowBlank(row))
                    continue; 

                var cell = row.Cell(headerMap["ID"]);
                string id = cell.GetString().Trim();

                if (string.IsNullOrEmpty(currentId))
                {
                    currentId = id; 
                }
                else if (!string.IsNullOrEmpty(id) && !string.Equals(currentId, id, StringComparison.OrdinalIgnoreCase))
                {
                    break; 
                }

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
        private string FormatOutputRow(Dictionary<string, string> values, string condition, List<string> outputHeaders, string valueColValue = "")
        {
            var line = outputHeaders.Select((h, index) =>
            {
                if (h.Equals("Condition / Formula Detail", StringComparison.OrdinalIgnoreCase))
                    return condition;

                if (index == outputHeaders.Count - 1 && !string.IsNullOrWhiteSpace(valueColValue))
                    return valueColValue;

                 return values.TryGetValue(h, out var val) ? val : "";
                
            });
            return string.Join("\t", line);
        }

        private List<string> ProcessTypeX(Dictionary<string, string> parentRow, List<(IXLRangeRow Row, string ValueColValue)> childRowPairs, int conditionCol1,
        int conditionCol2, Dictionary<string, int> headerMap, List<string> outputHeaders)
        {
            var output = new List<string>();
            string lastKnownId = parentRow.GetValueOrDefault("ID", "");

            foreach (var (child, valueColValue) in childRowPairs)
            {
                bool isChildRowBlank = _expectedHeaders.All(h =>
                {
                    if (!headerMap.TryGetValue(h, out int colIndex) || colIndex <= 0)
                        return true;

                    string cellValue = child.Cell(colIndex).GetString().Trim();
                    return string.IsNullOrWhiteSpace(cellValue);

                });

                if (isChildRowBlank)
                    continue;

                string condition = BuildConditionString(child, conditionCol1, conditionCol2);
                if (string.IsNullOrWhiteSpace(condition))
                    continue;

                string outputLine = FormatOutputRow(parentRow, condition, outputHeaders, valueColValue);
                output.Add(outputLine);
            }
            return output;
        }
    }
}
