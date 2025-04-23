using System;
using System.IO;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;
using Prorigo.Plm.DataMigration.Transformer;
using System.Collections.Generic;
using Prorigo.Plm.DataMigration.IO;
using OfficeOpenXml;
using Prorigo.Plm.DataMigration.Utilities;




namespace Prorigo.DataMigrationTransformation.OTIS
{
    public class ExcelToTsvStartCellValueInputOutputTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExcelToTsvStartCellValueInputOutputTransformer> _logger;

        private readonly string _processAreaDataPath;
        private readonly string _datRowDelimiter;
        private readonly string _datColumnDelimiter;
        private readonly string _tsvRowDelimiter;
        private readonly string _tsvColumnDelimiter;
        private readonly long _objectCountPerFile;
        private static readonly string TAB_REPLACEMENT = "|t|";
        private static readonly string NEWLINE_REPLACEMENT = "|n|";
        private static readonly string RETURN_REPLACEMENT = "|r|";

        public ExcelToTsvStartCellValueInputOutputTransformer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<ExcelToTsvStartCellValueInputOutputTransformer> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;

            var CSVToTsvSection = _configuration.GetSection("ExcelToTsvStartCellValueInputOutput");
            _processAreaDataPath = CSVToTsvSection.GetValue<string>("ProcessAreaDataPath");
            _tsvRowDelimiter = CSVToTsvSection.GetValue<string>("TsvRowDelimiter");
            _tsvColumnDelimiter = CSVToTsvSection.GetValue<string>("TsvColumnDelimiter");
            _objectCountPerFile = CSVToTsvSection.GetValue<long>("ObjectCountPerFile");
            _datRowDelimiter = CSVToTsvSection.GetValue<string>("DatRowDelimiter");
            _datColumnDelimiter = CSVToTsvSection.GetValue<string>("DatColumnDelimiter");

        }
        public void Transform(string LicenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");
            if (isLicenValid)
            {
                TransformExcelFiles(_processAreaDataPath);
            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }
        private void TransformExcelFiles(string directoryName)
        {
            var excelFiles = Directory.GetFiles(directoryName, "*.xlsx");
            foreach (var excelFile in excelFiles)
            {
                TransformFile(excelFile);
            }
        }

        private void TransformFile(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
                var inputData = new List<string>();
                var outputData = new List<string>();
                bool inputHeaderWritten = false;
                bool outputHeaderWritten = false;

                foreach (var worksheet in package.Workbook.Worksheets
                                        .Where(ws => ws.Name.IndexOf("input", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               ws.Name.IndexOf("output", StringComparison.OrdinalIgnoreCase) >= 0))

                {
                    var cellMatches = FindCellsWithValue(worksheet, "Parameter Name");

                    foreach (var matchCell in cellMatches)
                    {
                        string type = GetSectionType(worksheet, matchCell);

                        if (string.IsNullOrEmpty(type)) continue;

                        var dataRange = GetDataRangeFrom(matchCell);
                        var range = worksheet.Cells[dataRange];
                        var nonEmptyCols = GetNonEmptyColumnIndices(range);
                        string tsv = ConvertRangeToTsv(worksheet, range, nonEmptyCols, _tsvColumnDelimiter, _tsvRowDelimiter,
                            (type == "input" && inputHeaderWritten) || (type == "output" && outputHeaderWritten));

                        if (type == "input")
                        {
                            if (!inputHeaderWritten) inputHeaderWritten = true;
                            inputData.Add(tsv);
                        }
                        else if (type == "output")
                        {
                            if (!outputHeaderWritten) outputHeaderWritten = true;
                            outputData.Add(tsv);
                        }
                    }
                }

                string directory = Path.GetDirectoryName(filePath);
                File.WriteAllText(Path.Combine(directory, "Input.tsv"), string.Join(_tsvRowDelimiter, inputData));
                File.WriteAllText(Path.Combine(directory, "Output.tsv"), string.Join(_tsvRowDelimiter, outputData));

                Console.WriteLine("Generated Input.tsv and Output.tsv");
            }
        }

        private List<int> GetNonEmptyColumnIndices(ExcelRange range)
        {
            var worksheet = range.Worksheet;

            int startRow = range.Start.Row;
            int endRow = range.End.Row;
            int startCol = range.Start.Column;
            int endCol = range.End.Column;

            List<int> nonEmptyColumns = new List<int>();

            for (int col = startCol; col <= endCol; col++)
            {
                for (int row = startRow; row <= endRow; row++)
                {
                    var value = worksheet.Cells[row, col].Value;
                    if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                    {
                        nonEmptyColumns.Add(col);
                        break;
                    }
                }
            }

            return nonEmptyColumns;
        }
        private List<ExcelRangeBase> FindCellsWithValue(ExcelWorksheet worksheet, string value)
        {
            var matches = new List<ExcelRangeBase>();

            foreach (var cell in worksheet.Cells)
            {
                if (cell.Value != null && cell.Value.ToString().Trim().Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(cell);
                }
            }

            return matches;
        }

        private string GetSectionType(ExcelWorksheet worksheet, ExcelRangeBase cell)
        {
            int rowAbove = cell.Start.Row - 2;
            if (rowAbove <= 0) return null;

            for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
            {
                var val = worksheet.Cells[rowAbove, col].Value?.ToString().ToLower();
                if (val?.Contains("input") == true) return "input";
                if (val?.Contains("output") == true) return "output";
            }

            return null;
        }

        private string GetDataRangeFrom(ExcelRangeBase startCell)
        {
            int startRow = startCell.Start.Row;
            int endRow = startRow;

           
            var worksheet = startCell.Worksheet;
            while (true)
            {
                bool hasData = false;
                for (int col = startCell.Start.Column; col <= worksheet.Dimension.End.Column; col++)
                {
                    if (worksheet.Cells[endRow + 1, col].Value != null &&
                        !string.IsNullOrWhiteSpace(worksheet.Cells[endRow + 1, col].Text))
                    {
                        hasData = true;
                        break;
                    }
                }

                if (!hasData) break;
                endRow++;
            }

            int startCol = startCell.Start.Column;
            int endCol = worksheet.Dimension.End.Column;

            return ExcelCellBase.GetAddress(startRow, startCol, endRow, endCol);
        }

        private static string ConvertRangeToTsv(ExcelWorksheet worksheet, ExcelRange range, List<int> nonEmptyCols,
            string columnDelimiter, string rowDelimiter, bool skipHeader)
        {
            StringBuilder tsv = new StringBuilder();

            for (int row = range.Start.Row; row <= range.End.Row; row++)
            {
                if (skipHeader && row == range.Start.Row)
                    continue;

                List<string> rowValues = new List<string>();

                foreach (int col in nonEmptyCols)
                {
                    var val = worksheet.Cells[row, col].Value;
                    rowValues.Add(val?.ToString()?.Replace("\t", TAB_REPLACEMENT)
                                                 .Replace("\n", NEWLINE_REPLACEMENT)
                                                 .Replace("\r", RETURN_REPLACEMENT) ?? string.Empty);
                }

                tsv.Append(string.Join(columnDelimiter, rowValues));
                if (row < range.End.Row)
                {
                    tsv.Append(rowDelimiter);
                }
            }

            return tsv.ToString();
        }

    }
}
