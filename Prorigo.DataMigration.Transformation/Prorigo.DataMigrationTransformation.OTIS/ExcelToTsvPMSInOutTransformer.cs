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
    internal class ExcelToTsvPMSInOutTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExcelToTsvPMSInOutTransformer> _logger;
        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;

        public ExcelToTsvPMSInOutTransformer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<ExcelToTsvPMSInOutTransformer> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;

            var Configuration = _configuration.GetSection("ExcelToTsvPMSInOut");
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
            string fileTypePath = Path.Combine(directoryName, "BOM Files", "PMS");
            if (Directory.Exists(fileTypePath))
            {
                var excelFiles = Directory.GetFiles(fileTypePath, "*.xlsx");
                foreach (var excelFile in excelFiles)
                {
                    TransformFile(excelFile);
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
                List<string[]> inputDataRows = new List<string[]>();
                List<string[]> outputDataRows = new List<string[]>();
                //<string[]> formulaDataRows = new List<string[]>();

                var worksheets = package.Workbook.Worksheets;
                var PMSNumber = String.Empty;
                foreach (var worksheet in worksheets)
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
                    
                        var inCell = worksheetName.Cells[3, 2].Text;
                        var outCell = worksheetName.Cells[144,2].Text;
                    
                        if (inCell == "" && outCell == "")
                            continue;
                    
                        PMSNumber = worksheetName.Cells[2, 5].Text;
                        PMSNumber = PMSNumber.Replace("\t", "|t|").Replace("\n", "|n|").Replace("\r", "|r|").TrimEnd().TrimStart();
                        PMSNumber = PMSNumber.Split('-')[0];

                        List<int> inOutIndexes = new List<int>();
                        for (int row = startRow; row <= endRow; row++)
                        {
                            var val = worksheetName.Cells[row, 2].Text;
                            if (val == "Input") inOutIndexes.Add(row + 2);
                            else if (val == "Output") inOutIndexes.Add(row - 2);
                            else if (val == "SODS information")inOutIndexes.Add(row - 3);
                        }
                                        
                        string[] inputColumnHeaders = new string[endCol];
                        string[] outputColumnHeaders = new string[endCol];
                        List<int> inputValidColumnIndexes = new List<int>();
                        List<int> outputValidColumnIndexes = new List<int>();
                    
                        //Input
                        for (int row = inOutIndexes[0]; row <= inOutIndexes[1]; row++)
                        {
                            if (row == inOutIndexes[0])
                            {
                                for (int col = 2; col <= endCol; col++)
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
                                    cleanRow.Add((val ?? string.Empty).Replace("\t", "|t|").Replace("\n", "|n|").Replace("\r", "|r|").TrimEnd().TrimStart());
                                }
                                cleanRow.Add(PMSNumber);
                                inputDataRows.Add(cleanRow.ToArray());
                            }
                        }
                    
                        //Output
                        string lastId = string.Empty;
                        string lastParameterName = string.Empty;
                        string lastParameterDescription = string.Empty;
                        string lastParameterType = string.Empty;
                        string lastUOM = string.Empty;
                        string lastValueListorValueRange = string.Empty;
                    
                        for (int row = inOutIndexes[1] + 4; row <= inOutIndexes[2]; row++)
                        {
                            bool IdAndParameter = false;
                            bool isRowEmpty = true;
                    
                            for (int col = 2; col <= endCol; col++)
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
                                lastParameterDescription =string.Empty;
                                lastParameterType = string.Empty;
                                lastUOM= string.Empty;
                                lastValueListorValueRange = string.Empty;
                            }
                    
                            if (row == inOutIndexes[1] + 4)
                            {
                                for (int col = 2; col <= endCol; col++)
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
                                else if (colIndex == 4)
                                {
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        lastParameterDescription = val;
                                    }
                                    if (!isRowEmpty && (IdAndParameter || !string.IsNullOrEmpty(lastParameterDescription)))
                                    {
                                        cleanRow.Add(lastParameterDescription);
                                    }
                                    else
                                    {
                                        cleanRow.Add(string.Empty);
                                    }
                                }
                                else if (colIndex == 11)
                                {
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        lastParameterType = val;
                                    }
                                    if (!isRowEmpty && (IdAndParameter || !string.IsNullOrEmpty(lastParameterType)))
                                    {
                                        cleanRow.Add(lastParameterType);
                                    }
                                    else
                                    {
                                        cleanRow.Add(string.Empty);
                                    }
                                }
                                else if (colIndex == 13)
                                {
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        lastUOM = val;
                                    }
                                    if (!isRowEmpty && (IdAndParameter || !string.IsNullOrEmpty(lastUOM)))
                                    {
                                        cleanRow.Add(lastUOM);
                                    }
                                    else
                                    {
                                        cleanRow.Add(string.Empty);
                                    }
                                }
                                else if (colIndex == 14)
                                {
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        lastValueListorValueRange = val;
                                    }
                                    if (!isRowEmpty && (IdAndParameter || !string.IsNullOrEmpty(lastValueListorValueRange)))
                                    {
                                        cleanRow.Add(lastValueListorValueRange);
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
                        GenerateTsv(fileNameWithPath, inputDataRows, outputDataRows, outputColumnHeaders, PMSNumber);
                    }                    
                }
            }
        }
        private void GenerateTsv(string fileNameWithPath, List<string[]> inputDataRows, List<string[]> outputDataRows,  string[] outputColumnHeaders, string PMSNumber)
        {
            string excelFileName = Path.GetFileNameWithoutExtension(fileNameWithPath);
            string sheetTsvFileName = $"{excelFileName}.tsv";

            var inputDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "BOM Files", "PMS"), _objectCountPerFile)
            {
                FileBaseName = sheetTsvFileName,
                TypeName = "Input",
                FileExtension = "tsv"
            };
            var outputDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "BOM Files", "PMS"), _objectCountPerFile)
            {
                FileBaseName = sheetTsvFileName,
                TypeName = "Output",
                FileExtension = "tsv"
            };

            WriteToTsv(inputDataFileWriter, inputDataRows);
            FormulaToTsv(outputDataFileWriter, outputDataRows, outputColumnHeaders, PMSNumber);
        }

        private void WriteToTsv(TypeDataFileWriter fileWriter, List<string[]> dataRows)
        {
            using (fileWriter)
            {
                if (fileWriter.HeaderRow == null)
                    fileWriter.HeaderRow = "ID\tParameterName\tParameterDescription\tParameterType\tUOM\tValueListorValueRange\tPMSNumber\n";

                foreach (var dataRow in dataRows)
                {
                    if (string.IsNullOrEmpty(dataRow[1]))
                        continue;

                    var newDataFormatDataRow = string.Join("\t", dataRow) + "\n";
                    fileWriter.WriteRow(newDataFormatDataRow);
                }
            }
        }
        private void FormulaToTsv(TypeDataFileWriter fileWriter, List<string[]> dataRows, string[] formulaColumnHeaders, string PMSNumber)
        {
            using (fileWriter)
            {
                formulaColumnHeaders = formulaColumnHeaders.Where(header => !string.IsNullOrWhiteSpace(header)).ToArray();

                if (fileWriter.HeaderRow == null)
                    fileWriter.HeaderRow = "PMSNumber\tID\tParameterName\tParameterDescription\tParameterType\tUOM\tValueListorValueRange\tFormula\tProductRangeValueVerification\tCondition\n";

                foreach (var dataRow in dataRows)
                {
                    if (string.IsNullOrEmpty(dataRow[1]) && string.IsNullOrEmpty(dataRow[2]))
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

                    var newDataDataRow = PMSNumber;
                    if (newDataFormatDataRow.Length == 7)
                    {
                        newDataDataRow = newDataDataRow + "\t" + string.Join("\t", newDataFormatDataRow) + "\t" + "\n";
                    }
                    else
                    {
                        var mergedString = string.Join("|", newDataFormatDataRow.Skip(8));
                        newDataFormatDataRow = newDataFormatDataRow.Take(8)
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

