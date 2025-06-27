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
    class ODSExpressionsTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ODSExpressionsTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private const string ODS = "ODS";
        private const string Selection = "Selection";

        public ODSExpressionsTransformer(IConfiguration configuration, ILogger<ODSExpressionsTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var ODSSection = _configuration.GetSection("ODSExpressions");
            _processAreaDataPath = ODSSection.GetSection("ProcessAreaDataPath").Value;
            _objectCountPerFile = ODSSection.GetValue<long>("ObjectCountPerFile");
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
            _migrationDiagnostics.LogTransformTypeStartTime(directoryName, "ODS");
            _migrationDiagnostics.LogTransformTypeStatus(directoryName, "ODS", TransformStatus.InProgress);

            var path = Path.Combine(_processAreaDataPath, ODS);
            var OtisODSConditionExpDataWriter = new TypeDataFileWriter(path, _objectCountPerFile)
            {
                FileBaseName = $"ODS_Expression",
                TypeName = Selection,
                FileExtension = "tsv",
                HeaderRow = "ODSNo\tID\tRemark\tType\tConditionExpression\n"
            };

            var OtisODSConditionExpDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "BOM Files"));
            var OtisODSConditionExpEntities = OtisODSConditionExpDataReader.ReadAllEntities<ODSPartEntity>(ODS, "*.tsv");
            var groupedByOdsNo = OtisODSConditionExpEntities
                               .GroupBy(e => new { e.ODSNo, e.ID })
                               .GroupBy(g => g.Key.ODSNo);

            using (OtisODSConditionExpDataWriter)
            {
                foreach (var idGroup in groupedByOdsNo)
                {
                    int ExpressionID = 10001;
                    foreach (var groups in idGroup)
                    {
                        var odsNo = idGroup.Key;
                        var ID = "";
                        var remark = "";
                        var type = "";
                        string condition = "";

                        var headerCondition = (groups.FirstOrDefault()?.Condition ?? "");
                        Func<string, Dictionary<int, string>> getHeaders = condition =>
                                 condition
                                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                                .Select((val, idx) => new { idx, val = val.Trim() })
                                .ToDictionary(x => x.idx, x => x.val);

                        var headers = getHeaders(headerCondition);

                        foreach (var entity in groups)
                        {
                            if (string.IsNullOrEmpty(entity.Condition))
                                continue;

                            if (headerCondition.Contains(entity.Condition) || entity.Condition.Contains(headerCondition))
                            {
                                headerCondition = entity.Condition;
                                headers = getHeaders(headerCondition);
                                continue;
                            }

                            var values = entity.Condition.Split('|');
                            var conditionValues = new List<string>();
                            var QTY = entity.QT;


                            headers = headers.ToDictionary(
                                 kvp => kvp.Key,
                                 kvp => string.Equals(kvp.Value, "PN", StringComparison.OrdinalIgnoreCase) ? "ITEM" : kvp.Value
                                 );


                            for (int i = 0; i < values.Length && i < headers.Count; i++)
                            {
                                var value = values[i].Trim();
                                conditionValues.Add($"\"{headers[i]}\" : \"{value}\"");

                            }

                            var currentHeaders = new Dictionary<int, string>(headers);
                            bool qtAdded = currentHeaders.Values.Contains("QT");

                            if (!qtAdded)
                            {
                                int newIndex = currentHeaders.Keys.Max() + 1;
                                currentHeaders[newIndex] = "QTY";
                                conditionValues.Add($"\"QTY\" : \"{QTY}\"");
                            }

                            currentHeaders[currentHeaders.Keys.Max() + 1] = "ID";
                            conditionValues.Add($"\"ID\" : \"{ExpressionID}\"");

                            if (conditionValues.Count > 0)
                                condition = condition + "{" + string.Join(", ", conditionValues) + "},";

                            ID = entity.ID;
                            remark = entity.Remark;
                            type = entity.Type;
                            ExpressionID++;
                        }
                        if (!string.IsNullOrEmpty(ID) && !string.IsNullOrEmpty(remark))
                            OtisODSConditionExpDataWriter.WriteRow($"{odsNo}\t{ID}\t{remark}\t{type}\t[{condition.TrimEnd(',')}]\n");
                    }
                }
            }

            _migrationDiagnostics.LogTransformTypeStatus(path, Selection, TransformStatus.Completed);
            _migrationDiagnostics.LogTransformTypeEndTime(path, Selection);
        }

    }
}

