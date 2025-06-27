using System;
using System.IO;
using System.Linq;


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using Prorigo.DataMigrationTransformation.OTIS.Entities;

using Prorigo.Plm.DataMigration.Utilities;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    class ODSSelectionSetTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ODSSelectionSetTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private const string Selection = "Selection";
        private const string ODS_Data = "ODS";

        public ODSSelectionSetTransformer(IConfiguration configuration, ILogger<ODSSelectionSetTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var ODSSelectionSetSection = _configuration.GetSection("ODSSelectionSet");
            _processAreaDataPath = ODSSelectionSetSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = ODSSelectionSetSection.GetValue<long>("ObjectCountPerFile");
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
                TransformFiles(transformName, Selection);
            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }

        private void TransformFiles(string typeName, string transformName)
        {
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, typeName);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.InProgress);

            var ODSDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath,ODS_Data));
            var ODSEntities = ODSDataReader.ReadAllEntities<ODSEntity>("CalSheet_ODS", "*.tsv");

            var ExpressionDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, ODS_Data));
            var ExpressionEntities = ExpressionDataReader.ReadAllEntities<ODSExpressionEntity>(Selection);


            var ExpressionDataFileWriter = new TypeDataFileWriter((Path.Combine(_processAreaDataPath,ODS_Data)), _objectCountPerFile)
            {
                FileBaseName = $"ODSSelectionSet",
                TypeName = $"ODS_CS_SelectionTable",
                FileExtension = "tsv"
            };
            var failedFilesDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, ODS_Data), _objectCountPerFile)
            {
                FileBaseName = $"TR_ODS_MetaData",
                TypeName = "FailedODS_CS_SelectionTable",
                HeaderRow = "ODSNumber\n",
                FileExtension = "tsv"
            };

            var ODS = ODSEntities.ToDictionary(e => e.ITEM_NUMBER, e => e.ID);

            int objectCount = 0, failedObjectCount = 0;
            using (ExpressionDataFileWriter)
            {
                using (failedFilesDataFileWriter)
                {
                    if (ExpressionDataFileWriter.HeaderRow == null)
                        ExpressionDataFileWriter.HeaderRow = "ConnectionId\tCONFIG_ID\tKEYED_NAME\tCREATED_ON\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tIS_CURRENT\tMAJOR_REV\tSTATE\tIS_RELEASED\tNOT_LOCKABLE\tGENERATION\tPERMISSION_ID\tSOURCE_ID\tOTS_CONDITION_TABLE\tOTS_ID\tOTS_NAME\tOTS_TABLE_TYPE\tRELATED_ID\n";

                    foreach (var ExpressionEntity in ExpressionEntities)
                    {

                        var ODS_Number = ExpressionEntity.ODSNO;
                        var ConnectionId = TransformerUtils.GetNewArasGuid();
                        var ConfigId = ConnectionId;
                        var KEYED_NAME = ConnectionId;
                        var PERMISSION_ID = "95475AE006E7415794BDC93808DC04D2";
                        var CREATED_ON = DateTime.Now;
                        var CREATED_BY_ID = "Data Migration";
                        var MODIFIED_ON = DateTime.Now.ToString();
                        var MODIFIED_BY_ID = "Data Migration";
                        var IS_RELEASED = "1";
                        var STATE = "Released";
                        var IS_CURRENT = "1";
                        var MAJOR_REV = "A";
                        var NOT_LOCKABLE = "0";
                        var GENERATION = "1";
                        var OTS_CONDITION_TABLE = ExpressionEntity.condition;
                        var OTS_ID = ExpressionEntity.ID;
                        var OTS_NAME = ExpressionEntity.Remark; // Remark
                        var OTS_TABLE_TYPE = ExpressionEntity.Type;
                        var RelatedID = "";
                        var SourceID = "";

                        if (ODS.ContainsKey(ODS_Number))
                        {
                            SourceID = ODS[ODS_Number];
                        }
                        else
                        {
                            failedFilesDataFileWriter.WriteRow($"{ODS_Number}\n");
                            continue;
                        }

                        ExpressionDataFileWriter.WriteRow($"{ConnectionId}\t{ConfigId}\t{KEYED_NAME}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{IS_CURRENT}\t{MAJOR_REV}\t{STATE}\t{IS_RELEASED}\t{NOT_LOCKABLE}\t{GENERATION}\t{PERMISSION_ID}\t{SourceID}\t{OTS_CONDITION_TABLE}\t{OTS_ID}\t{OTS_NAME}\t{OTS_TABLE_TYPE}\t{RelatedID}\n");

                    }
                    objectCount++;
                }
            }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.Completed, objectCount, failedObjectCount);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, typeName);
        }
    }
}