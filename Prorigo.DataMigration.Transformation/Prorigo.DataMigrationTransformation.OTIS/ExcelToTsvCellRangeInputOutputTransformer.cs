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
    public class ExcelToTsvCellRangeInputOutputTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExcelToTsvCellRangeInputOutputTransformer> _logger;

        private readonly string _processAreaDataPath;
        private readonly string _datRowDelimiter;
        private readonly string _datColumnDelimiter;
        private readonly string _tsvRowDelimiter;
        private readonly string _tsvColumnDelimiter;
        private readonly long _objectCountPerFile;
        private static readonly string TAB_REPLACEMENT = "|t|";
        private static readonly string NEWLINE_REPLACEMENT = "|n|";
        private static readonly string RETURN_REPLACEMENT = "|r|";

        public ExcelToTsvCellRangeInputOutputTransformer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<ExcelToTsvCellRangeInputOutputTransformer> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;

            var CSVToTsvSection = _configuration.GetSection("ExcelToTsvCellRangeInputOutput");
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
                ExcelWorksheet worksheet = package.Workbook.Worksheets["EX3-Input & Output(Semi-Para)"];

                var rangesToProcess = new List<(string Name, string Range , string OutputFileName)>
                {
                    ("Table1", "A5:N16", "Input.tsv"),
                    ("Table2", "A21:N31", "Output.tsv")
                };

                string directory = Path.GetDirectoryName(filePath);
                //int fileCounter = 1;

                foreach (var (name, rangeAddress, OutputFileName) in rangesToProcess)
                {
                    var fullRange = worksheet.Cells[rangeAddress];
                    var nonEmptyCols = GetNonEmptyColumnIndices(fullRange);

                    string tsv = ConvertRangeToTsv(worksheet, fullRange, nonEmptyCols, _tsvColumnDelimiter, _tsvRowDelimiter);
                    //string outputFilePath = Path.Combine(directory, $"output_{fileCounter++}.tsv");
                    string outputFilePath = Path.Combine(directory, OutputFileName);

                    File.WriteAllText(outputFilePath, tsv);
                    Console.WriteLine($"TSV file saved to: {outputFilePath}");
                }
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

        static string ConvertRangeToTsv(ExcelWorksheet worksheet, ExcelRange range, List<int> nonEmptyCols, string columnDelimiter, string rowDelimiter)
        {
            StringBuilder tsv = new StringBuilder();

            for (int row = range.Start.Row; row <= range.End.Row; row++)
            {
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