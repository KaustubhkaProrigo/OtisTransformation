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
    class OtisSODSSelectionSetTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisSODSSelectionSetTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;

        private const string SODSSelection = "SODSSelection";
        private const string SODSCalculationSheet = "SODSCalculationSheet";

        public OtisSODSSelectionSetTransformer(IConfiguration configuration, ILogger<OtisSODSSelectionSetTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var SODSSelectionSetSection = _configuration.GetSection("OtisSODSSelectionSet");
            _processAreaDataPath = SODSSelectionSetSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = SODSSelectionSetSection.GetValue<long>("ObjectCountPerFile");
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
                TransformFiles(transformName, SODSSelection);
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

            var SODSDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "SODS"));
            var SODSEntities = SODSDataReader.ReadAllEntities<OtisSODSEntity>(SODSCalculationSheet, "*.TSV");

            var ExpressionDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "SODS"));
            var ExpressionEntities = ExpressionDataReader.ReadAllEntities<SODSExpressionEntity>(SODSSelection, "*.tsv");


            var ExpressionDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "SODS" ), _objectCountPerFile)
            {
                FileBaseName = $"SODSSelectionSet_MetaData",
                TypeName = $"SODS_CS_SelectionTable",
                FileExtension = "tsv"
            };
            var failedFilesDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "SODS", "SODS_CS_SelectionTable"), _objectCountPerFile)
            {
                FileBaseName = $"TR_SODS_MetaData",
                TypeName = "FailedSODS_MetaData",
                HeaderRow = "SODSNumber\tErrorDescription\n",
                FileExtension = "tsv"
            };

            var SODS = SODSEntities.ToDictionary(e => e.ITEM_NUMBER, e => e.ID);

            int objectCount = 0, failedObjectCount = 0;
            using (failedFilesDataFileWriter)
            {
                using (ExpressionDataFileWriter)
                {
                    if (ExpressionDataFileWriter.HeaderRow == null)
                        ExpressionDataFileWriter.HeaderRow = "ConnectionId\tCONFIG_ID\tKEYED_NAME\tCREATED_ON\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tIS_CURRENT\tMAJOR_REV\tSTATE\tIS_RELEASED\tNOT_LOCKABLE\tGENERATION\tPERMISSION_ID\tSOURCE_ID\tOTS_CONDITION_TABLE\tOTS_ID\tOTS_SSNo\tDescription\tTableType\tRELATED_ID\n";

                    foreach (var ExpressionEntity in ExpressionEntities)
                    {
                        var SODS_Number = ExpressionEntity.SODSNO;
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
                        var OTS_SSNO = ExpressionEntity.SSNo;
                        var Description = ExpressionEntity.description;
                        var RelatedID = "";
                        var SourceID = "";
                        var TableType = "X";

                        if (SODS.ContainsKey(SODS_Number))
                        {
                            SourceID = SODS[SODS_Number];
                        }
                        else
                        {
                            failedFilesDataFileWriter.WriteRow($"{SODS_Number}\tMissing SODS_Number\n");
                            continue;
                        }

                        ExpressionDataFileWriter.WriteRow($"{ConnectionId}\t{ConfigId}\t{KEYED_NAME}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{IS_CURRENT}\t{MAJOR_REV}\t{STATE}\t{IS_RELEASED}\t{NOT_LOCKABLE}\t{GENERATION}\t{PERMISSION_ID}\t{SourceID}\t{OTS_CONDITION_TABLE}\t{OTS_ID}\t{OTS_SSNO}\t{Description}\t{TableType}\t{RelatedID}\n");

                    }
                    objectCount++;

                }
            }
           

            _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.Completed, objectCount, failedObjectCount);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, typeName);
        }
    }
}

