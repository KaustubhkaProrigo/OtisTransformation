using System;
using System.IO;
using OfficeOpenXml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using Prorigo.Plm.DataMigration.Transformer;
using System.Collections.Generic;
using Prorigo.Plm.DataMigration.IO;
using System.Data;
using Prorigo.Plm.DataMigration.Utilities;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    public class ExcelEBOMExtractDataTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExcelEBOMExtractDataTransformer> _logger;
        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private string[] _processType;

        public ExcelEBOMExtractDataTransformer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<ExcelEBOMExtractDataTransformer> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;

            var Configuration = _configuration.GetSection("ExcelEBOMExtractData");
            _processAreaDataPath = Configuration.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = Configuration.GetValue<long>("ObjectCountPerFile");
            _processType = Configuration.GetSection("ProcessType").Get<string[]>();

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
            foreach (var processType in _processType)
            {
                string fileTypePath = Path.Combine(directoryName, "BOM Files", processType);
                if (Directory.Exists(fileTypePath))
                {
                    var excelFiles = Directory.GetFiles(fileTypePath, "*.xlsx");
                    foreach (var excelFile in excelFiles)
                    {
                        TransformFile(excelFile, processType);
                    }
                }
            }
            //var excelFiles = Directory.GetFiles(directoryName, "*.xlsx");
            //foreach (var excelFile in excelFiles)
            //{
            //    TransformFile(excelFile);
            //}
        }

        private void TransformFile(string fileNameWithPath, string processType)
        {
            var fileExtension = Path.GetExtension(fileNameWithPath);
            if (!fileExtension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(fileNameWithPath)))
            {
                List<string[]> dataRows = new List<string[]>();

                var worksheets = package.Workbook.Worksheets;
                foreach (var worksheet in worksheets)
                {
                    if (worksheet.Name.Contains("EBOM"))
                    {
                        var worksheetName = package.Workbook.Worksheets[worksheet.Name];
                        if (worksheetName != null)
                        {

                            int startRow = int.MaxValue;
                            int endRow = int.MinValue;
                            int startCol = int.MaxValue;
                            int endCol = int.MinValue;

                            foreach (var cell in worksheetName.Cells)
                            {
                                if (!string.IsNullOrWhiteSpace(cell.Text))
                                {
                                    startRow = Math.Min(startRow, cell.Start.Row);
                                    endRow = Math.Max(endRow, cell.Start.Row);
                                    startCol = Math.Min(startCol, cell.Start.Column);
                                    endCol = Math.Max(endCol, cell.Start.Column);
                                }
                            }


                            if (endRow == 3)
                                continue;

                            string cellVal = worksheetName.Cells[3, 2].Value.ToString();
                            string DCellVal = worksheetName.Cells[1, 3].Value.ToString();
                            if (cellVal != "Part No.")
                                continue;

                            string[] columnHeaders = new string[endCol];
                            List<int> validColumnIndexes = new List<int>();

                            var partNum = String.Empty;
                            for (int row = 3; row <= endRow; row++)
                            {
                                if (row == 3)
                                {
                                    for (int col = 0; col < endCol; col++)
                                    {
                                        var val = worksheetName.Cells[row, col + 1].Value;

                                        if (val != null)
                                        {
                                            columnHeaders[col] = val.ToString();
                                            validColumnIndexes.Add(col);
                                        }
                                    }
                                    columnHeaders = validColumnIndexes.Select(index => columnHeaders[index]).ToArray();
                                }
                                else
                                {
                                    List<string> cleanRow = new List<string>();
                                    foreach (int colIndex in validColumnIndexes)
                                    {
                                        var val = worksheetName.Cells[row, colIndex + 1].Value;
                                        string cellValue = val?.ToString() ?? "";
                                        cellValue = cellValue.Replace("\t", "|t|").Replace("\n", "|n|").Replace("\r", "|r|").TrimEnd().TrimStart();

                                        if (colIndex == 1)
                                        {
                                            cleanRow.Add(DCellVal ?? "");
                                            if (!string.IsNullOrEmpty(cellValue))
                                            {
                                                partNum = cellValue;
                                            }
                                            else
                                            {
                                                cleanRow.Add(partNum ?? "");
                                                continue;
                                            }
                                        }

                                        cleanRow.Add(cellValue);
                                    }
                                    dataRows.Add(cleanRow.ToArray());

                                }
                            }

                        }
                        GenerateTsv(processType, fileNameWithPath, worksheet.Name, dataRows);
                    }
                }

            }
        }

        private void GenerateTsv(string processType, string fileNameWithPath, string sheetName, List<string[]> dataRows)
        {

            string excelFileName = Path.GetFileNameWithoutExtension(fileNameWithPath);
            string sheetTsvFileName = $"{excelFileName}_{processType}.tsv";

            var customReportDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "BOM Files", processType), _objectCountPerFile)
            {
                FileBaseName = sheetTsvFileName,
                TypeName = "TR_Conv_CSVToExl_EBOMTemplate",
                FileExtension = "tsv"
            };

            var objectCountPerFile = 0;
            using (customReportDataFileWriter)
            {
                //var headerDataRow = string.Join("\t", columnHeaders) + "\n";
                if (customReportDataFileWriter.HeaderRow == null)
                    customReportDataFileWriter.HeaderRow = "Source_Part_Number\tRelated_Part_Number\tDescription\tQT\tUOM\tDrawing_Number\n";

                foreach (var dataRow in dataRows)
                {
                    if (dataRow.Length < 11)
                        continue;

                    var val = dataRow[5];
                    if (String.IsNullOrEmpty(val) || val.Contains("...") || val.Contains("…"))
                    {
                        continue;
                    }

                    string newDatFormatDataRow = string.Empty;

                    if (processType == "Fixed" || processType == "Parametric")
                    {
                        newDatFormatDataRow = $"{dataRow[2]}\t{dataRow[6]}\t{dataRow[7]}\t{dataRow[9]}\t{dataRow[10]}\t{dataRow[1]}\n";
                    }

                    else if (processType == "Semi-Para (Smart)")
                    {
                        newDatFormatDataRow = $"{dataRow[2]}\t{dataRow[5]}\t{dataRow[6]}\t{dataRow[8]}\t{dataRow[9]}\t{dataRow[1]}\n";
                    }

                    //var newDatFormatDataRow = $"{dataRow[2]}\t{dataRow[5]}\t{dataRow[6]}\t{dataRow[8]}\t{dataRow[9]}\t{dataRow[1]}\n"; //semi-para EBOM
                    //var newDatFormatDataRow = $"{dataRow[2]}\t{dataRow[6]}\t{dataRow[7]}\t{dataRow[9]}\t{dataRow[10]}\t{dataRow[1]}\n"; //fixed, parametric EBOM
                    //var newDatFormatDataRow = $"{dataRow[1]}\t{dataRow[5]}\t{dataRow[6]}\t{dataRow[8]}\t{dataRow[9]}\n";
                    customReportDataFileWriter.WriteRow(newDatFormatDataRow);

                    objectCountPerFile++;
                }
            }
        }
    }
}

