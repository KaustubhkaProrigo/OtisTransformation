using System;
using System.IO;
using OfficeOpenXml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using Prorigo.Plm.DataMigration.Transformer;
using System.Collections.Generic;
using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Utilities;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    public class ExcelToTsvParameterTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExcelToTsvParameterTransformer> _logger;
        private readonly string _processAreaDataPath;
        private readonly string _tsvRowDelimiter;
        private readonly string _tsvColumnDelimiter;
        private readonly long _objectCountPerFile;
        private static readonly string TAB_REPLACEMENT = "|t|";
        private static readonly string NEWLINE_REPLACEMENT = "|n|";
        private static readonly string RETURN_REPLACEMENT = "|r|";

        public ExcelToTsvParameterTransformer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<ExcelToTsvParameterTransformer> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;

            var Configuration = _configuration.GetSection("ExcelToTsvParameter");
            _processAreaDataPath = Configuration.GetValue<string>("ProcessAreaDataPath");
            _tsvRowDelimiter = Configuration.GetValue<string>("TsvRowDelimiter");
            _tsvColumnDelimiter = Configuration.GetValue<string>("TsvColumnDelimiter");
            _objectCountPerFile = Configuration.GetValue<long>("ObjectCountPerFile");
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
        private void TransformFile(string fileNameWithPath)
        {
            var fileExtension = Path.GetExtension(fileNameWithPath);
            if (!fileExtension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;//

            using (var package = new ExcelPackage(new FileInfo(fileNameWithPath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault(); //package.Workbook.Worksheets["EBOM Template"]
                if (worksheet != null)
                {
                    int startRow = int.MaxValue;
                    int endRow = 1;
                    int startCol = int.MaxValue;
                    int endCol = 1;

                    for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                    {
                        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                        {
                            var cellValue = worksheet.Cells[row, col].Text;

                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                startRow = Math.Min(startRow, row);
                                endRow = Math.Max(endRow, row);
                                startCol = Math.Min(startCol, col);
                                endCol = Math.Max(endCol, col);
                            }
                        }
                    }

                    // var tsvContent = "";

                    List<string[]> dataRows = new List<string[]>();

                    string[] columnHeaders = new string[endCol];

                    for (int row = 2; row <= endRow; row++)
                    {
                        if (row == 2)
                        {
                            for (int col = 0; col < endCol; col++)
                            {
                                columnHeaders[col] = worksheet.Cells[row, col + 1].Value.ToString();
                            }
                        }

                        else
                        {
                            string[] dataRow = new string[endCol];
                            for (int i = 0; i < endCol; i++)
                            {
                                dataRow[i] = worksheet.Cells[row, i + 1].Value == null ? "" : worksheet.Cells[row, i + 1].Value.ToString();
                            }
                            dataRows.Add(dataRow);
                        }
                    }

                    GenerateTsv(fileNameWithPath, columnHeaders, dataRows);

                    //if (!string.IsNullOrWhiteSpace(tsvContent))
                    //{                       
                    //    var tsvFileName = Path.ChangeExtension(fileNameWithPath, ".tsv");
                    //    File.WriteAllText(tsvFileName, tsvContent);
                    //}
                }
            }
        }

        private void GenerateTsv(string fileNameWithPath, string[] columnHeaders, List<string[]> dataRows)
        {
            var fileNameWithChangedExtension = Path.ChangeExtension(fileNameWithPath, ".tsv");

            var customReportDataFileWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"Conv_CSVToExl_Parameter",
                TypeName = "Conv_CSVToExl_Parameter",
                FileExtension = "tsv"
            };

            var objectCountPerFile = 0;
            using (customReportDataFileWriter)
            {
                var headerDataRow = string.Join("\t", columnHeaders) + "\n";
                if (customReportDataFileWriter.HeaderRow == null)
                    customReportDataFileWriter.HeaderRow = headerDataRow;

                foreach (var dataRow in dataRows)
                {
                    var datFormatDataRow = string.Join("»«", dataRow) + "◘";
                    var tsvFormatDataRow = datFormatDataRow.Replace("\t", TAB_REPLACEMENT).Replace("\n", NEWLINE_REPLACEMENT).Replace("\r", RETURN_REPLACEMENT);
                    var tsvDataRow = tsvFormatDataRow.Replace("»«", "\t").Replace("◘", "\n");
                    customReportDataFileWriter.WriteRow(tsvDataRow);

                    objectCountPerFile++;
                }
            }
        }
    }
}

