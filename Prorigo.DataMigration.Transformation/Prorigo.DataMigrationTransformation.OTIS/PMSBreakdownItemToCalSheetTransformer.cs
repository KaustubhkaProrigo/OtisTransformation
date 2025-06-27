using System;
using System.Collections.Generic;
using System.IO;
using Prorigo.Plm.DataMigration.Utilities;

using Prorigo.Plm.DataMigration.Transformer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using Prorigo.DataMigrationTransformation.OTIS.Entities;
using System.Linq;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    internal class PMSBreakdownItemToCalSheetTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PMSBreakdownItemToCalSheetTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;

        string BreakDownToPMSCal = "BreakDownToPMSCal";
        public PMSBreakdownItemToCalSheetTransformer(IConfiguration configuration, ILogger<PMSBreakdownItemToCalSheetTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var BreakDownToPMSCal = _configuration.GetSection("PMSBreakdownItemToCalSheet");
            _processAreaDataPath = BreakDownToPMSCal.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = BreakDownToPMSCal.GetValue<long>("ObjectCountPerFile");
        }

        public void Transform(string LicenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");
            if (isLicenValid)
            {
                var className = this.GetType().Name;
                var transformName = className.Substring(0, className.IndexOf("Transformer"));

                DefaultValueAdder(transformName);
            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }

        public void DefaultValueAdder(string transformName)
        {
            var BreakDownToPMSCalSheetWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "PMS"), _objectCountPerFile)
            {
                FileBaseName = $"TRBreakDownItemToPMSCalSheet",
                TypeName = $"BreakDownItemToPMSCalSheet",
                FileExtension = "tsv",
            };

            var MissingRecordWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "PMS", "BreakDownItemToPMSCalSheet"), _objectCountPerFile)
            {
                FileBaseName = "TRBreakDownItemToPMSCalSheet",
                TypeName = "MissingBreakDownItemToPMSCalSheet",
                FileExtension = "tsv",
            };

            var ProductDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "Product"));
            var ProductEntities = ProductDataReader.ReadAllEntities<OtisBreakdownProductEntity>("BreakdownItem_Product", "*.tsv");

            var PMSDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "PMS"));
            var PMSEntities = PMSDataReader.ReadAllEntities<OtisCalculationSheetEntity>("CalSheet_PMS", "*.tsv");

            Dictionary<string, string> PMSEntityMap = new Dictionary<string, string>();
            foreach (var PMSEntity in PMSEntities)
            {
                if (!PMSEntityMap.ContainsKey(PMSEntity.KEYED_NAME))
                {
                    PMSEntityMap.Add(PMSEntity.KEYED_NAME, PMSEntity.Id);
                }
            }

            long successCount = 0;

            using (BreakDownToPMSCalSheetWriter)
            {
                using (MissingRecordWriter)
                {
                    foreach (var entity in ProductEntities)
                    {

                        if (BreakDownToPMSCalSheetWriter.HeaderRow == null)
                            BreakDownToPMSCalSheetWriter.HeaderRow = "ConnectionId\tCONFIG_ID\tKEYED_NAME\tFromFilename\tToFilename\tSourceID\tRelatedID\tCREATED_ON\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tPERMISSION_ID\tIS_CURRENT\tIS_RELEASED\tMAJOR_REV\tNOT_LOCKABLE\tGENERATION\tSTATE\n";

                        if (MissingRecordWriter.HeaderRow == null)
                            MissingRecordWriter.HeaderRow = "ProductNumber\tPMSNO\tError Description\n";

                        if (PMSEntityMap.ContainsKey(entity.KEYED_NAME))
                        {
                            var ConnectionId = TransformerUtils.GetNewArasGuid();
                            var CONFIG_ID = ConnectionId;
                            var KEYED_NAME = ConnectionId;
                            var FromFilename = entity.KEYED_NAME;
                            var ToFilename = entity.KEYED_NAME;
                            var SourceID = entity.ID;
                            var RelatedId = PMSEntityMap[entity.KEYED_NAME];
                            var CREATED_ON = DateTime.Now.ToString();
                            var CREATED_BY_ID = "Data Migration";
                            var MODIFIED_ON = DateTime.Now.ToString();
                            var MODIFIED_BY_ID = "Data Migration";
                            var PERMISSION_ID = "95475AE006E7415794BDC93808DC04D2";
                            var IS_CURRENT = "1";
                            var IS_RELEASED = "1";
                            var STATE = "Released";
                            var MAJOR_REV = "A";
                            var NOT_LOCKABLE = "0";
                            var GENERATION = "1";

                            BreakDownToPMSCalSheetWriter.WriteRow($"{ConnectionId}\t{CONFIG_ID}\t{KEYED_NAME}\t{FromFilename}\t{ToFilename}\t{SourceID}\t{RelatedId}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{PERMISSION_ID}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{NOT_LOCKABLE}\t{GENERATION}\t{STATE}\n");
                            successCount++;
                        }
                        else
                        {
                            var FromFilename = entity.KEYED_NAME;
                            var ToFilename = entity.KEYED_NAME;

                            MissingRecordWriter.WriteRow($"{FromFilename}\t{ToFilename}\tMissing Product And PMS\n");
                            continue;
                            
                        }
                    }

                }
            }
            _migrationDiagnostics.LogTransformTypeStatus(transformName, BreakDownToPMSCal, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, BreakDownToPMSCal);
        }
    }
}