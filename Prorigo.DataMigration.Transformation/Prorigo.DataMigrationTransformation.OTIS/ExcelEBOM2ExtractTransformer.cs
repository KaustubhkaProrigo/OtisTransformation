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
//using ClosedXML.Excel;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    class ExcelEBOM2ExtractTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExcelEBOM2ExtractTransformer> _logger;
        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly string EBOMExcel;


        public ExcelEBOM2ExtractTransformer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<ExcelEBOM2ExtractTransformer> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;

            var Configuration = _configuration.GetSection("ExcelEBOM2Extract");
            _processAreaDataPath = Configuration.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = Configuration.GetValue<long>("ObjectCountPerFile");
            EBOMExcel = Configuration.GetValue<string>("ExcelFileName");


        }
        public void Transform(string LicenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");
            if (isLicenValid)
            {
                TransformExcelFiles(_processAreaDataPath, EBOMExcel);
            }

            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }
        private void TransformExcelFiles(string directoryName, string fileName)
        {
            string filePath = Path.Combine(directoryName, fileName);
            if (File.Exists(filePath))
            {
                TransformFile(filePath);
            }
            else
            {
                Console.WriteLine($"File '{fileName}' not found in '{directoryName}'");
            }
        }


        private void TransformFile(string fileNameWithPath)
        {
            //var sourceHeader = "Part No.";
            //var relatedHeader = "Part Number";

            //var outputLines = new List<string>();
            //bool headerWritten = false;

            //using (var workbook = new XLWorkbook(fileNameWithPath))
            //{

            //    foreach (var worksheet in workbook.Worksheets)
            //    {
            //        Console.WriteLine($"Checking sheet: {worksheet.Name}");
            //        var usedRange = worksheet.RangeUsed();
            //        if (usedRange == null) continue;

            //        bool headerFound = false;
            //        int headerRowNumber = 0;
            //        int colSource = -1;
            //        int colRelated = -1;
            //        List<int> validColumns = new List<int>();

            //        foreach (var row in usedRange.Rows())
            //        {
            //            var rowValues = row.Cells().Select(c => c.GetString()?.Trim()).ToList();

            //            if (rowValues.Any(c => string.Equals(c, sourceHeader, StringComparison.OrdinalIgnoreCase)) &&
            //                rowValues.Any(c => string.Equals(c, relatedHeader, StringComparison.OrdinalIgnoreCase)))
            //            {
            //                headerFound = true;
            //                headerRowNumber = row.RowNumber();

            //                // Get column indexes of the source and related headers
            //                colSource = row.Cells().FirstOrDefault(c => string.Equals(c.GetString()?.Trim(), sourceHeader, StringComparison.OrdinalIgnoreCase))?.Address.ColumnNumber ?? -1;
            //                colRelated = row.Cells().FirstOrDefault(c => string.Equals(c.GetString()?.Trim(), relatedHeader, StringComparison.OrdinalIgnoreCase))?.Address.ColumnNumber ?? -1;

            //                Console.WriteLine($"✅ Header found at row {headerRowNumber} (Source: col {colSource}, Related: col {colRelated})");

            //                // Build valid columns and headerLine (just once for first sheet)
            //                var headerCells = row.Cells().ToList();
            //                var headerLine = new List<string>();

            //                foreach (var cell in headerCells)
            //                {
            //                    int colIndex = cell.Address.ColumnNumber;
            //                    string name = cell.GetString()?.Trim() ?? "";

            //                    if (!string.IsNullOrEmpty(name))
            //                    {
            //                        if (colIndex == colSource) name = "SourcePart";
            //                        else if (colIndex == colRelated) name = "Related_Part";

            //                        validColumns.Add(colIndex);

            //                        // Only add header row once globally
            //                        if (!headerWritten)
            //                            headerLine.Add(name);
            //                    }
            //                }

            //                if (!headerWritten)
            //                {
            //                    outputLines.Add(string.Join("\t", headerLine));
            //                    headerWritten = true;
            //                }

            //                break; // Done finding header
            //            }
            //        }

            //        if (!headerFound || colSource == -1 || colRelated == -1)
            //        {
            //            Console.WriteLine("❌ Required headers not found in this sheet.");
            //            continue;
            //        }

            //        // Get the data rows after header
            //        var dataRows = usedRange.RowsUsed()
            //            .Where(r => r.RowNumber() > headerRowNumber)
            //            .Where(r => r.Cells().Any(c => !string.IsNullOrWhiteSpace(c.GetString())));

            //        string lastSource = "";

            //        foreach (var row in dataRows)
            //        {
            //            var lineValues = new List<string>();
            //            var cells = row.Cells().ToList();

            //            foreach (int colIndex in validColumns)
            //            {
            //                var cell = cells.FirstOrDefault(c => c.Address.ColumnNumber == colIndex);
            //                string value = cell?.GetString()?.Trim() ?? "";

            //                // Fill-down logic
            //                if (colIndex == colSource)
            //                {
            //                    if (string.IsNullOrEmpty(value))
            //                        value = lastSource;
            //                    else
            //                        lastSource = value;
            //                }

            //                lineValues.Add(value);
            //            }

            //            outputLines.Add(string.Join("\t", lineValues));
            //        }
            //    }

            //}

            //string outputFileName = Path.GetFileNameWithoutExtension(fileNameWithPath) + ".tsv";
            //string outputPath = Path.Combine(_processAreaDataPath, outputFileName);
            //File.WriteAllLines(outputPath, outputLines);
        }

        // Write the final output


    }

}

