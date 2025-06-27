using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using Prorigo.DataMigrationTransformation.OTIS.Entities;

using Prorigo.Plm.DataMigration.Utilities;
namespace Prorigo.DataMigrationTransformation.OTIS
{
    class OtisSODSInputTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisSODSInputTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private const string SODSInput = "SODSInput";
        private const string SODS = "SODS";
        private const string Otis_Parameter = "Parameter";

        public OtisSODSInputTransformer(IConfiguration configuration, ILogger<OtisSODSInputTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var SODSInputSection = _configuration.GetSection("OtisSODSInput");
            _processAreaDataPath = SODSInputSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = SODSInputSection.GetValue<long>("ObjectCountPerFile");
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
                TransformFiles(_processAreaDataPath, transformName);
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

            var path = Path.Combine(_processAreaDataPath,"BOM Files", SODS);

            var SODSInputDataReader = new TypeDataFileReader(path);
            var SODSInputEntities = SODSInputDataReader.ReadAllEntities<SODSInputEntity>(SODSInput);

            var SODSDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath,"SODS"));
            var SODSEntities = SODSDataReader.ReadAllEntities<OtisSODSEntity>("SODSCalculationSheet");

            var ParameterReader = new TypeDataFileReader(_processAreaDataPath);
            var ParameterEntities = ParameterReader.ReadAllEntities<OtisParameterTransformedEntity>(Otis_Parameter,"*.tsv");

            var SODSINPUTFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, SODS), _objectCountPerFile)
            {
                FileBaseName = $"TR_SODSInput_MetaData",
                TypeName = "SODS_CS_InputData",
                FileExtension = "tsv"
            };
            var failedFilesDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath,"SODS", "SODS_CS_InputData"), _objectCountPerFile)
            {
                FileBaseName = $"TR_FailedParameter_MetaData",
                TypeName = "FailedParameter_MetaData",
                HeaderRow = "Parameter_Name\tSODSNumber\tErrorDescription\n",
                FileExtension = "tsv"
            };

            var BreakDown = SODSEntities.ToDictionary(e => e.ITEM_NUMBER, e => e.ID);
            var Parameter = ParameterEntities
                            .Where(e => e.classification == "Global")
                            .ToDictionary(e => e.item_number, e => e.id);

            int objectCount = 0, failedObjectCount = 0;

            using (failedFilesDataFileWriter)
            {
                using (SODSINPUTFileWriter)
                {
                    if (SODSINPUTFileWriter.HeaderRow == null)
                        SODSINPUTFileWriter.HeaderRow = "ConnectionId\tCONFIG_ID\tID\tSourceID\tRelatedId\tCREATED_ON\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tPermissionId\tIS_CURRENT\tIS_RELEASED\tMAJOR_REV\tNOT_LOCKABLE\tGENERATION\tSTATE\tValueList\n";

                    foreach (var SODSInputEntity in SODSInputEntities)
                    {
                        var SODS_Number = SODSInputEntity.SODSNO;
                        var ParameterName = SODSInputEntity.ParameterName;
                        var ConnectionId = TransformerUtils.GetNewArasGuid();
                        var ConfigId = ConnectionId;
                        var ID = SODSInputEntity.ID;
                        var PermissionId = "95475AE006E7415794BDC93808DC04D2";
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
                        var ValueList = SODSInputEntity.ValueList;
                        var SourceID = "";
                        var RelatedID = "";

                        if (Parameter.ContainsKey(ParameterName) && BreakDown.ContainsKey(SODS_Number))
                        {
                            SourceID = BreakDown[SODS_Number];
                            RelatedID = Parameter[ParameterName];
                        }
                        else
                        {
                            if (!BreakDown.ContainsKey(SODS_Number))
                            {
                                if (!Parameter.ContainsKey(ParameterName))
                                {
                                    failedFilesDataFileWriter.WriteRow($"{ParameterName}\t{SODS_Number}\tMissing SODS And Parameter\n");
                                    continue;
                                }
                                failedFilesDataFileWriter.WriteRow($"{ParameterName}\t{SODS_Number}\tMissing SODS\n");
                                continue;
                            }
                            else
                            {
                                failedFilesDataFileWriter.WriteRow($"{ParameterName}\t{SODS_Number}\tMissing Parameter\n");
                                continue;
                            }
                        }
                        
                        SODSINPUTFileWriter.WriteRow($"{ConnectionId}\t{ConfigId}\t{ID}\t{SourceID}\t{RelatedID}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{PermissionId}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{NOT_LOCKABLE}\t{GENERATION}\t{STATE}\t{ValueList}\n");
                        objectCount++;

                    }

                }
            }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.Completed, objectCount, failedObjectCount);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, typeName);
        }
    }
}