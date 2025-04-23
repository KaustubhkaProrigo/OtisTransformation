using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Utilities;
using Prorigo.Plm.DataMigration.Transformer.Metrics;

namespace Prorigo.Plm.DataMigration.Transformer
{
    public class ReorderColumnsTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReorderColumnsTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly string _columnHeaders;
        private readonly string _typeName;

        private const string MODIFIED_FOLDER = "Modified";

        public ReorderColumnsTransformer(IConfiguration configuration, ILogger<ReorderColumnsTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var reorderColumnsSection = _configuration.GetSection("ReorderColumns");
            _processAreaDataPath = reorderColumnsSection.GetSection("ProcessAreaDataPath").Value;
            _objectCountPerFile = reorderColumnsSection.GetValue<long>("ObjectCountPerFile");
            _columnHeaders = reorderColumnsSection.GetValue<string>("ColumnHeaders");
            _typeName = reorderColumnsSection.GetValue<string>("Type");
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

            var dataColumnHeaders = _columnHeaders.Split(',');
            Directory.CreateDirectory(Path.Combine(_processAreaDataPath, $"{MODIFIED_FOLDER}_{_typeName}"));

            var modifiedColumnHeadersLine = string.Join('\t', dataColumnHeaders);
            Dictionary<string, int> columnNameColumnIndexMap = null;

            int objectCount = 0;
            var dataFiles = Directory.GetFiles(Path.Combine(_processAreaDataPath, _typeName));
            foreach (var dataFile in dataFiles)
            {
                var fileName = Path.GetFileName(dataFile);
                var dataLines = File.ReadAllLines(dataFile);

                var columnHeadersLine = dataLines[0];
                if (columnNameColumnIndexMap == null)
                {
                    var columnHeaders = columnHeadersLine.Split('\t').ToList();

                    columnNameColumnIndexMap = new Dictionary<string, int>();
                    foreach (var dataColumnHeader in dataColumnHeaders)
                        columnNameColumnIndexMap[dataColumnHeader] = columnHeaders.IndexOf(dataColumnHeader);
                }

                using (var streamWriter = new StreamWriter(Path.Combine(_processAreaDataPath, $"{MODIFIED_FOLDER}_{_typeName}", fileName)))
                {
                    streamWriter.WriteLine(modifiedColumnHeadersLine);

                    for (int i = 1; i < dataLines.Length; i++)
                    {
                        var dataLine = dataLines[i];
                        var rowCellValues = dataLine.Split('\t');

                        var modifiedRowCellValues = new string[dataColumnHeaders.Length];
                        for (int j = 0; j < dataColumnHeaders.Length; j++)
                        {
                            var dataColumnHeader = dataColumnHeaders[j];
                            var columnIndex = columnNameColumnIndexMap[dataColumnHeader];
                            modifiedRowCellValues[j] = rowCellValues[columnIndex];
                        }

                        streamWriter.WriteLine(string.Join('\t', modifiedRowCellValues));
                        objectCount++;
                    }
                }
            }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.Completed, objectCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, typeName);
        }
    }
}
