using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;
using Prorigo.Plm.DataMigration.IO;
using Microsoft.Extensions.Configuration;
using Prorigo.Plm.DataMigration.Transformer;

using Prorigo.Plm.DataMigration.Utilities;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using Prorigo.DataMigrationTransformation.OTIS.Entities;
using System.IO;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    class OtisSODSExpressionsTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisSODSExpressionsTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private const string SODS = "SODS";

        public OtisSODSExpressionsTransformer(IConfiguration configuration, ILogger<OtisSODSExpressionsTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var SODSSection = _configuration.GetSection("OtisSODSExpressions");
            _processAreaDataPath = SODSSection.GetSection("ProcessAreaDataPath").Value;
            _objectCountPerFile = SODSSection.GetValue<long>("ObjectCountPerFile");
        }
        public void Transform(string LicenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            //License key 
            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");
            if (isLicenValid)
            {
                TransformSubDirectories(_processAreaDataPath);
            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }
        private void TransformSubDirectories(string directoryName)
        {
            _migrationDiagnostics.LogTransformTypeStartTime(directoryName, "SODS");
            _migrationDiagnostics.LogTransformTypeStatus(directoryName, "SODS", TransformStatus.InProgress);

            var SODS_ODSExpressionFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "SODS"), _objectCountPerFile)
            {
                FileBaseName = $"SODSExpressions",
                TypeName = "SODSSelection",
                FileExtension = "tsv",
                HeaderRow = "SODSNo\tID\tSS No.\tDescription\tConditionExpression\n"
            };

            var OtisODSConditionExpDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath,"BOM Files"));
            var OtisODSConditionExpEntities = OtisODSConditionExpDataReader.ReadAllEntities<Product_ODSEntity>(SODS, "*.tsv");

            var path = Path.Combine(_processAreaDataPath, "SODS");
           
           // Group entire dataset by SODSNO first
           var groupedBySODSNO = OtisODSConditionExpEntities
                .Where(e => !string.IsNullOrWhiteSpace(e.SODSNO))
                .GroupBy(e => e.SODSNO.Trim())
                .ToDictionary(g => g.Key, g => g.ToList());
            
            string outputFolder = Path.Combine(path, "SODSSelection");
            Directory.CreateDirectory(outputFolder);

            using (SODS_ODSExpressionFileWriter)
            {
                foreach (var kvp in groupedBySODSNO)
                {
                    string sodsNo = kvp.Key;
                    var entities = kvp.Value;

                    var groupedByID = entities
                        .GroupBy(e => e.ID?.Trim() ?? string.Empty)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    string outputPath = Path.Combine(outputFolder, $"{sodsNo}_expression.tsv");


                    foreach (var group in groupedByID.Values)
                    {
                        if (group.Count < 2) continue; // skip if less than header + data row

                        var headerRow = group[0];
                        //var headers = headerRow.Condition?.Split('|').Select(h => h.Trim()).ToArray();

                        var headers = headerRow.Condition?.Split('|').Select(h => string.Equals(h.Trim(), "ODS", StringComparison.OrdinalIgnoreCase) ? "ITEM" : h.Trim()).ToArray();
                        if (headers == null || headers.Length == 0) continue;

                        var rows = group.Skip(1)
                            .Where(e => !string.IsNullOrWhiteSpace(e.Condition))
                            .Select(e =>
                            {
                                var values = e.Condition.Split('|').Select(v => v.Trim()).ToArray();
                                var kv = headers.Zip(values, (k, v) => $"\"{k}\" : {(int.TryParse(v, out _) ? v : $"\"{v}\"")}").ToList();

                                kv.Add($"\"QTY\" : {e.QT}");
                                kv.Add($"\"ID\" : {e.ExpressionID}");

                                return "{" + string.Join(", ", kv) + "}";
                            });

                        var conditionExpression = "[" + string.Join(",", rows) + "]";

                        SODS_ODSExpressionFileWriter.WriteRow($"{headerRow.SODSNO}\t{headerRow.ID}\t{headerRow.SS_No}\t{headerRow.Description}\t{conditionExpression}\n");

                    }

                }



            }

        }
    }
}