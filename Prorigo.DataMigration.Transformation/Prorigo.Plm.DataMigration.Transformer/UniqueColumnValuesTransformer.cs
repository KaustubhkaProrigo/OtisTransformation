using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using Prorigo.Plm.DataMigration.Utilities;

namespace Prorigo.Plm.DataMigration.Transformer
{
    public class UniqueColumnValuesTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReorderColumnsTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly string _columnHeader;

        public UniqueColumnValuesTransformer(IConfiguration configuration, ILogger<ReorderColumnsTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var uniqueColumnValuesSection = _configuration.GetSection("UniqueColumnValues");
            _processAreaDataPath = uniqueColumnValuesSection.GetSection("ProcessAreaDataPath").Value;
            _columnHeader = uniqueColumnValuesSection.GetValue<string>("ColumnHeader");
        }

        public void Transform(string LicenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            //License key
            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");
            if (isLicenValid)
            {
                var className = this.GetType().Name;
                var transformName = className.Substring(0, className.IndexOf("Transformer"));

                 TransformDataFiles(transformName);

            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }

        private void TransformDataFiles(string transformName)
        {
            var typeName = Path.GetFileName(_processAreaDataPath);

            _migrationDiagnostics.LogTransformTypeStartTime(transformName, typeName);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.InProgress);

            var columnIndex = -1;
            int objectCount = 0;
            var uniqueValues = new HashSet<string>();

            var dataFiles = Directory.GetFiles(_processAreaDataPath, "*.tsv");
            using (var streamWriter = new StreamWriter(Path.Combine(_processAreaDataPath, $"{_columnHeader}-UniqueColumnValues.tsv")))
            {
                foreach (var dataFile in dataFiles)
                {
                    var dataLines = File.ReadAllLines(dataFile);

                    var columnHeadersLine = dataLines[0];
                    if (columnIndex < 0)
                    {
                        var columnHeaders = columnHeadersLine.Split('\t').ToList();
                        columnIndex = columnHeaders.IndexOf(_columnHeader);
                    }
            
                    for (int i = 1; i < dataLines.Length; i++)
                    {
                        var dataLine = dataLines[i];
                        var rowCellValues = dataLine.Split('\t');

                        var columnValue = rowCellValues[columnIndex];
                        if (!uniqueValues.Contains(columnValue))
                        {
                            streamWriter.WriteLine(columnValue);
                            objectCount++;

                            uniqueValues.Add(columnValue);
                        }
                    }
                }
            }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.Completed, objectCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, typeName);
        }
    }
}
