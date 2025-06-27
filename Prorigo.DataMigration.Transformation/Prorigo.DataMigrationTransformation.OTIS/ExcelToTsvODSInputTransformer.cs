using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prorigo.Plm.DataMigration.Transformer;
using System.Collections.Generic;
using Prorigo.Plm.DataMigration.IO;
using OfficeOpenXml;
using Prorigo.Plm.DataMigration.Utilities;
using Prorigo.Plm.DataMigration.Transformer.Metrics;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    class ExcelToTsvODSInputTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExcelToTsvODSInputTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private const string Input = "Input";


        public ExcelToTsvODSInputTransformer(IConfiguration configuration, ILogger<ExcelToTsvODSInputTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var Configuration = _configuration.GetSection("ExcelToTsvODSInput");
            _processAreaDataPath = Configuration.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = Configuration.GetValue<long>("ObjectCountPerFile");

        }
        public void Transform(string LicenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");
            if (isLicenValid)
            {
                TransformExcelFiles(Path.Combine(_processAreaDataPath, "BOM Files", "ODS"));
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
            _migrationDiagnostics.LogTransformTypeStartTime(directoryName, "Input");
            _migrationDiagnostics.LogTransformTypeStatus(directoryName, "Input", TransformStatus.InProgress);

            TransformFile(directoryName);

            _migrationDiagnostics.LogTransformTypeStatus(directoryName, "Input", TransformStatus.Completed);
            _migrationDiagnostics.LogTransformTypeEndTime(directoryName, "Input");
        }
        private void TransformFile(string directoryName)
        {
            //var path = (Path.Combine(directoryName, Input));
            var excelFiles = Directory.EnumerateFiles(directoryName)
                        .Where(file => file.EndsWith(".xlsm", StringComparison.OrdinalIgnoreCase)
                || file.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase));

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            foreach (var excelFile in excelFiles)
            {
                using (var package = new ExcelPackage(new FileInfo(excelFile)))
                {
                    List<string[]> inputDataRows = new List<string[]>();

                    var worksheets = package.Workbook.Worksheets;
                    var ODSNo = String.Empty;
                    foreach (var worksheet in worksheets)
                    {
                        if (worksheet.Name == "Parameters")
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

                                ODSNo = worksheetName.Cells[1, 10].Text;
                                if (ODSNo == "")
                                    ODSNo = worksheetName.Cells[3, 4].Text;
                                else if (ODSNo == "")
                                    ODSNo = worksheetName.Cells[1, 9].Text;

                                List<int> inOutIndexes = new List<int>();
                                for (int row = startRow; row <= endRow; row++)
                                {
                                    var val = worksheetName.Cells[row, 2].Text;
                                    if (val == "Input parameters") inOutIndexes.Add(row + 2);
                                    else if (val == "Output parameters") inOutIndexes.Add(row - 1);
                                    else
                                    {
                                        val = worksheetName.Cells[row, 1].Text;
                                        if (val == "Input parameters") inOutIndexes.Add(row + 2);
                                        else if (val == "Output parameters") inOutIndexes.Add(row - 1);
                                    }                                    
                                }

                                string[] inputColumnHeaders = new string[endCol];
                                List<int> inputValidColumnIndexes = new List<int>();

                                //Input
                                for (int row = inOutIndexes[0] - 1; row <= inOutIndexes[1] - 1; row++)
                                {
                                    if (row == inOutIndexes[0] - 1)
                                    {
                                        for (int col = 1; col <= endCol - 3; col++)
                                        {
                                            var val = worksheetName.Cells[row, col].Text.TrimEnd().TrimStart();
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
                                            cleanRow.Add((val ?? string.Empty).TrimEnd().TrimStart().Replace("\t", "|t|").Replace("\n", "|n|").Replace("\r", "|r|"));
                                        }
                                        cleanRow.Add(ODSNo);
                                        inputDataRows.Add(cleanRow.ToArray());
                                    }

                                }
                                GenerateTsv(ODSNo, worksheet.Name, inputDataRows);

                            }
                        }
                    }
                }
            }
        }
        private void GenerateTsv(string processType, string sheetName, List<string[]> dataRows)
        {

            var inputDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "BOM Files", "ODS"), _objectCountPerFile)
            {
                FileBaseName = processType,
                TypeName = "Input",
                FileExtension = "tsv"
            };

            using (inputDataFileWriter)
            {
                if (inputDataFileWriter.HeaderRow == null)
                    inputDataFileWriter.HeaderRow = "Rev\tItem\tParameter\tDescription\tParameterType\tUOM\tValueList\tODSNO\n";

                foreach (var dataRow in dataRows)
                {
                    if (string.IsNullOrEmpty(dataRow[1]))
                        continue;

                    var newDataFormatDataRow = string.Join("\t", dataRow) + "\n";
                    inputDataFileWriter.WriteRow(newDataFormatDataRow);
                }
            }
        }
    }
}