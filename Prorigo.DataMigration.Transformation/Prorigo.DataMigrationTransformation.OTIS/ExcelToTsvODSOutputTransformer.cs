using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    internal class ExcelToTsvODSOutputTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExcelToTsvODSOutputTransformer> _logger;
        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly List<string> _fileTypes;

        public ExcelToTsvODSOutputTransformer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<ExcelToTsvODSOutputTransformer> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;

            var Configuration = _configuration.GetSection("ExcelToTsvODSOutput");
            _processAreaDataPath = Configuration.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = Configuration.GetValue<long>("ObjectCountPerFile");
            _fileTypes = Configuration.GetSection("ProcessType").Get<List<string>>();

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
            foreach (var processType in _fileTypes)
            {
                string fileTypePath = Path.Combine(directoryName, "BOM Files", processType);
                if (Directory.Exists(fileTypePath))
                {
                    var excelFiles = Directory.GetFiles(fileTypePath, "*.xlsx");
                    foreach (var excelFile in excelFiles)
                    {
                        TransformFile(excelFile);
                    }
                }
            }
        }
        private void TransformFile(string fileNameWithPath)
        {
            var fileExtension = Path.GetExtension(fileNameWithPath);
            if (!fileExtension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(fileNameWithPath)))
            {
                List<string[]> outputDataRows = new List<string[]>();

                var worksheets = package.Workbook.Worksheets;
                var ODSNo = String.Empty;
                foreach (var worksheet in worksheets)
                {
                    if (worksheet.Name.Contains("Parameters"))
                    {
                        var worksheetName = package.Workbook.Worksheets[worksheet.Name];

                        if (worksheetName != null)
                        {
                            int startRow = int.MinValue;
                            int endRow = int.MaxValue;
                            int startCol = int.MinValue;
                            int endCol = int.MaxValue;

                            if (worksheet.Dimension != null)
                            {
                                startRow = worksheet.Dimension.Start.Row;
                                endRow = worksheet.Dimension.End.Row;
                                startCol = worksheet.Dimension.Start.Column;
                                endCol = worksheet.Dimension.End.Column;
                            }


                            var inCell = worksheetName.Cells[6, 2].Text;
                            var outCell = worksheetName.Cells[15, 2].Text;

                            if (inCell == "" && outCell == "")
                                continue;

                            ODSNo = worksheetName.Cells[1, 10].Text;
                            if(ODSNo == "")
                                ODSNo = worksheetName.Cells[3, 4].Text;
                            else if(ODSNo == "")
                                ODSNo = worksheetName.Cells[1,9].Text;

                            ODSNo = ODSNo.Replace("\t", "|t|").Replace("\n", "|n|").Replace("\r", "|r|").TrimEnd().TrimStart();



                            List<int> inOutIndexes = new List<int>();
                            for (int row = startRow; row <= endRow; row++)
                            {
                                var val = worksheetName.Cells[row, 2].Text;
                                if (val == "Output parameters") inOutIndexes.Add(row);
                                else
                                {
                                    val = worksheetName.Cells[row, 1].Text;
                                    if (val == "Output parameters") inOutIndexes.Add(row - 1);
                                }
                            }

                            string[] outputColumnHeaders = new string[endCol];
                            List<int> outputValidColumnIndexes = new List<int>();

                            string lastId = string.Empty;
                            string lastParameterName = string.Empty;

                            for (int row = inOutIndexes[0] + 1; row <= endRow; row++)
                            {
                                bool IdAndParameter = false;
                                bool isRowEmpty = true;

                                for (int col = 1; col <= endCol; col++)
                                {
                                    var val = worksheetName.Cells[row, col].Text;
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        isRowEmpty = false;
                                        break;
                                    }
                                }

                                if (isRowEmpty)
                                {
                                    lastId = string.Empty;
                                    lastParameterName = string.Empty;
                                }

                                if (row == inOutIndexes[0] + 1)
                                {
                                    for (int col = 1; col <= endCol; col++)
                                    {
                                        var val = worksheetName.Cells[row, col].Text;
                                        if (!string.IsNullOrEmpty(val))
                                        {
                                            outputColumnHeaders[col - 1] = val;
                                            outputValidColumnIndexes.Add(col - 1);
                                        }
                                    }

                                    for (int col = outputValidColumnIndexes[outputValidColumnIndexes.Count - 1]; col <= endCol; col++)
                                    {
                                        outputValidColumnIndexes.Add(col + 1);
                                    }
                                }
                                else
                                {
                                    List<string> cleanRow = new List<string>();
                                    foreach (int colIndex in outputValidColumnIndexes)
                                    {
                                        var val = worksheetName.Cells[row, colIndex + 1].Text;
                                        val = val.Replace("\t", "|T|").Replace("\n", "|N|").Replace("\r", "|R|").TrimEnd().TrimStart();

                                        if (colIndex == 7 && !string.IsNullOrEmpty(val))
                                        {
                                            IdAndParameter = true;
                                        }

                                        if (colIndex == 1)
                                        {
                                            if (!string.IsNullOrEmpty(val))
                                            {
                                                lastId = val;
                                            }

                                            if (!isRowEmpty && (IdAndParameter || !string.IsNullOrEmpty(lastId)))
                                            {
                                                cleanRow.Add(lastId);
                                            }
                                            else
                                            {
                                                cleanRow.Add(string.Empty);
                                            }
                                        }

                                        else if (colIndex == 2)
                                        {
                                            if (!string.IsNullOrEmpty(val))
                                            {
                                                lastParameterName = val;
                                            }
                                            if (!isRowEmpty && (IdAndParameter || !string.IsNullOrEmpty(lastParameterName)))
                                            {
                                                cleanRow.Add(lastParameterName);
                                            }
                                            else
                                            {
                                                cleanRow.Add(string.Empty);
                                            }
                                        }
                                        else
                                        {
                                            cleanRow.Add(val ?? string.Empty);
                                        }
                                    }
                                    outputDataRows.Add(cleanRow.ToArray());
                                }
                            }
                            GenerateTsv(fileNameWithPath, outputDataRows, outputColumnHeaders, ODSNo);
                        }
                    }
                }
            }
        }
        private void GenerateTsv(string fileNameWithPath, List<string[]> outputDataRows, string[] outputColumnHeaders, string ODSNo)
        {
            string excelFileName = Path.GetFileNameWithoutExtension(fileNameWithPath);
            string sheetTsvFileName = $"{excelFileName}.tsv";

            var outputDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath,"BOM Files","ODS"), _objectCountPerFile)
            {
                FileBaseName = sheetTsvFileName,
                TypeName = "OutputParameters",
                FileExtension = "tsv"
            };

            FormulaToTsv(outputDataFileWriter, outputDataRows, outputColumnHeaders, ODSNo);
        }

        private void FormulaToTsv(TypeDataFileWriter fileWriter, List<string[]> dataRows, string[] outputColumnHeaders, string ODSNo)
        {
            using (fileWriter)
            {
                outputColumnHeaders = outputColumnHeaders.Where(header => !string.IsNullOrWhiteSpace(header)).ToArray();

                if (fileWriter.HeaderRow == null)
                    fileWriter.HeaderRow = "ODSNo\tRev\tID\tParameter\tDescription\tParameterType\tUOM\tOutput\tInput\n";

                foreach (var dataRow in dataRows)
                {
                    if(string.IsNullOrEmpty(dataRow[1]) && string.IsNullOrEmpty(dataRow[2]) && string.IsNullOrEmpty(dataRow[7]))
                        continue;

                    var newDataFormatDataRow = dataRow
                    .Select((value, index) =>
                    {
                        if (index <= 7 && string.IsNullOrWhiteSpace(value))
                            return null;

                        return value;
                    })
                    .Where(value => value != "" || value == null)
                    .ToArray();

                    var newDataDataRow = ODSNo;
                    if (newDataFormatDataRow.Length == 8)
                    {
                        newDataDataRow = newDataDataRow + "\t" + string.Join("\t", newDataFormatDataRow) + "\n";
                    }
                    else
                    {
                        var mergedString = string.Join("|", newDataFormatDataRow.Skip(7));
                        newDataFormatDataRow = newDataFormatDataRow.Take(7)
                                             .Concat(new string[] { mergedString })
                                             .ToArray();
                        newDataDataRow = newDataDataRow + "\t" + string.Join("\t", newDataFormatDataRow) + "\n";
                    }
                    fileWriter.WriteRow(newDataDataRow);
                }                
             }
         }
    }
}