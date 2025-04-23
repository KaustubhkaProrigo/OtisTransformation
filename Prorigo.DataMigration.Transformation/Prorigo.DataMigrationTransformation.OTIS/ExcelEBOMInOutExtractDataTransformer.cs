using DocumentFormat.OpenXml.Spreadsheet;
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
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    public class ExcelEBOMInOutExtractDataTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExcelEBOMInOutExtractDataTransformer> _logger;
        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;

        public ExcelEBOMInOutExtractDataTransformer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<ExcelEBOMInOutExtractDataTransformer> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;

            var Configuration = _configuration.GetSection("ExcelEBOMInOutExtractData");
            _processAreaDataPath = Configuration.GetValue<string>("ProcessAreaDataPath");
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
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(fileNameWithPath)))
            {
                List<string[]> inputDataRows = new List<string[]>();
                List<string[]> outputDataRows = new List<string[]>();
                List<string[]> formulaDataRows = new List<string[]>();

                var worksheets = package.Workbook.Worksheets;
                var DrawingNo = String.Empty;
                foreach (var worksheet in worksheets)
                {
                    if (worksheet.Name.Contains("Input & Output"))
                    {
                        var worksheetName = package.Workbook.Worksheets[worksheet.Name];
                        

                        if (worksheetName != null)
                        {
                            int startRow = int.MinValue;
                            int endRow = int.MaxValue;
                            int startCol = int.MinValue;
                            int endCol = int.MaxValue;

                            //foreach (var cell in worksheetName.Cells)
                            //{
                            //    //if (!string.IsNullOrWhiteSpace(cell.Text))
                            //    //{
                            //    //    startRow = Math.Min(startRow, cell.Start.Row);
                            //    //    endRow = Math.Max(endRow, cell.Start.Row);
                            //    //    startCol = Math.Min(startCol, cell.Start.Column);
                            //    //    endCol = Math.Max(endCol, cell.Start.Column);
                            //    //}
                            //}


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

                            DrawingNo = worksheetName.Cells[2, 1].Text;

                            List<int> inOutIndexes = new List<int>();
                            for (int row = startRow; row <= endRow; row++)
                            {
                                var val = worksheetName.Cells[row, 1].Text;
                                if (val == "Input") inOutIndexes.Add(row + 2);
                                else if (val == "Output") inOutIndexes.Add(row - 1);
                                else if (val == "Formula & Linkup") inOutIndexes.Add(row - 1);
                            }


                            string[] inputColumnHeaders = new string[endCol];
                            string[] outputColumnHeaders = new string[endCol];
                            string[] formulaColumnHeaders = new string[endCol];
                            List<int> inputValidColumnIndexes = new List<int>();
                            List<int> outputValidColumnIndexes = new List<int>();
                            List<int> formulaValidColumnIndexes = new List<int>();

                            //Input
                            for (int row = inOutIndexes[0]; row <= inOutIndexes[1]; row++)
                            {
                                if (row == inOutIndexes[0])
                                {
                                    for (int col = 1; col <= endCol; col++)
                                    {
                                        var val = worksheetName.Cells[row, col].Text;
                                        if (!string.IsNullOrEmpty(val))
                                        {
                                            inputColumnHeaders[col - 1] = val;
                                            inputValidColumnIndexes.Add(col - 1);
                                        }
                                    }
                                }
                                else
                                {
                                    List<string> cleanRow = new List<string>();
                                    foreach (int colIndex in inputValidColumnIndexes)
                                    {
                                        var val = worksheetName.Cells[row, colIndex + 1].Text;
                                        cleanRow.Add(val ?? string.Empty);
                                    }
                                    cleanRow.Add(DrawingNo);
                                    inputDataRows.Add(cleanRow.ToArray());
                                }
                            }

                            //Output
                            for (int row = inOutIndexes[1] + 3; row <= inOutIndexes[2]; row++)
                            {
                                if (row == inOutIndexes[1] + 3)
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
                                }
                                else
                                {
                                    List<string> cleanRow = new List<string>();
                                    foreach (int colIndex in outputValidColumnIndexes)
                                    {
                                        var val = worksheetName.Cells[row, colIndex + 1].Text;
                                        cleanRow.Add(val ?? string.Empty);
                                    }
                                    cleanRow.Add(DrawingNo);
                                    outputDataRows.Add(cleanRow.ToArray());
                                }
                            }

                            //Formula & Linkup
                            for (int row = inOutIndexes[2] + 2; row <= endRow; row++)
                            {
                                if (row == inOutIndexes[2] + 2)
                                {
                                    for (int col = 1; col <= endCol; col++)
                                    {
                                        var val = worksheetName.Cells[row, col].Text;
                                        if (!string.IsNullOrEmpty(val))
                                        {
                                            formulaColumnHeaders[col - 1] = val;
                                            formulaValidColumnIndexes.Add(col - 1);
                                        }
                                    }
                                    for (int col = formulaValidColumnIndexes[formulaValidColumnIndexes.Count - 1]; col <= endCol; col++)
                                    {
                                        formulaValidColumnIndexes.Add(col+1);
                                    }
                                }
                                else
                                {
                                    List<string> cleanRow = new List<string>();
                                    foreach (int colIndex in formulaValidColumnIndexes)
                                    {
                                        var val = worksheetName.Cells[row, colIndex + 1].Text;
                                        cleanRow.Add(val ?? string.Empty);
                                    }
                                    //cleanRow.Add(DrawingNo);
                                    formulaDataRows.Add(cleanRow.ToArray());
                                }
                            }

                            GenerateTsv(inputDataRows, outputDataRows, formulaDataRows, formulaColumnHeaders);
                        }
                    }
                }
            }
        }
        private void GenerateTsv(List<string[]> inputDataRows, List<string[]> outputDataRows, List<string[]> formulaDataRows, string[] formulaColumnHeaders)
        {
            var inputDataFileWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"Conv_ExlToTSV_InputTemplate",
                TypeName = "Conv_ExlToTSV_InputTemplate",
                FileExtension = "tsv"
            };
            var outputDataFileWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"Conv_ExlToTSV_OutputTemplate",
                TypeName = "Conv_ExlToTSV_OutputTemplate",
                FileExtension = "tsv"
            }; 
            var formulaDataFileWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"Conv_ExlToTSV_FormulaTemplate",
                TypeName = "Conv_ExlToTSV_FormulaTemplate",
                FileExtension = "tsv"
            };

            WriteToTsv(inputDataFileWriter, inputDataRows);
            WriteToTsv(outputDataFileWriter, outputDataRows);
            FormulaToTsv(formulaDataFileWriter, formulaDataRows, formulaColumnHeaders);
        }

        private void WriteToTsv(TypeDataFileWriter fileWriter, List<string[]> dataRows)
        {
            using (fileWriter)
            {
                if (fileWriter.HeaderRow == null)
                    fileWriter.HeaderRow = "ID\tParameter_Name\tParameter_Description\tParameter_Type\tUOM\tValue List or Value Range\tDrawingNo\n";

                foreach (var dataRow in dataRows)
                {
                    if (string.IsNullOrEmpty(dataRow[1]))
                        continue;
                    
                    var newDataFormatDataRow = string.Join("\t", dataRow) + "\n";
                    fileWriter.WriteRow(newDataFormatDataRow);
                }
            }
        }
        private void FormulaToTsv(TypeDataFileWriter fileWriter, List<string[]> dataRows, string[] formulaColumnHeaders)
        {
            using (fileWriter)
            {
                formulaColumnHeaders = formulaColumnHeaders.Where(header => !string.IsNullOrWhiteSpace(header)).ToArray();

                //if (fileWriter.HeaderRow == null)
                //    fileWriter.HeaderRow = string.Join("\t", formulaColumnHeaders) + "\n";
                //if (fileWriter.HeaderRow == null)
                //    fileWriter.HeaderRow = string.Join("\t", formulaColumnHeaders[0], formulaColumnHeaders[1], formulaColumnHeaders[2], formulaColumnHeaders[7]) + "\n";
                if (fileWriter.HeaderRow == null)
                    fileWriter.HeaderRow = "ID\tPropertyName\tFormula\tCondition\tDrawingNo\n";

                foreach (var dataRow in dataRows)
                {
                    if (string.IsNullOrEmpty(dataRow[1]) && string.IsNullOrEmpty(dataRow[7]))
                        continue;

                    var newDataFormatDataRow = dataRow
                    .Select((value, index) =>
                    {
                        
                        if (index <= 2 && string.IsNullOrWhiteSpace(value))
                            return null;

                        return value;
                    })
                    .Where(value => value != "" || value == null)  
                    .ToArray();

                    var newDataDataRow = string.Empty;
                    if (newDataFormatDataRow.Length == 3)
                    {
                        newDataDataRow = string.Join("\t", newDataFormatDataRow) +"\t"+ "\n";
                    }
                    else
                    {
                        var mergedString = string.Join("|", newDataFormatDataRow.Skip(3));
                        newDataFormatDataRow = newDataFormatDataRow.Take(3)
                                             .Concat(new string[] { mergedString})
                                             .ToArray();
                        newDataDataRow = string.Join("\t", newDataFormatDataRow) + "\n";


                    }

                    fileWriter.WriteRow(newDataDataRow);

                }
            }
        }
    }
}
