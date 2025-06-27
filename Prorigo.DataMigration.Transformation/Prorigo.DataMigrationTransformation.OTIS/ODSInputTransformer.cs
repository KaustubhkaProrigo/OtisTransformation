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
    class ODSInputTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ODSInputTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private const string Input = "Input";
        private const string ODS = "ODS";
        private const string Otis_Parameter = "Otis_Parameter";

        public ODSInputTransformer(IConfiguration configuration, ILogger<ODSInputTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var ODSInputSection = _configuration.GetSection("ODSInput");
            _processAreaDataPath = ODSInputSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = ODSInputSection.GetValue<long>("ObjectCountPerFile");
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
                TransformFiles(transformName, Input);
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

            var ODSInputDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "BOM Files", ODS));
            var ODSInputEntities = ODSInputDataReader.ReadAllEntities<ODSInputEntity>(Input, "*.tsv");

            var ODSDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, ODS));
            var ODSEntities = ODSDataReader.ReadAllEntities<ODSEntity>("CalSheet_ODS", "*.tsv");

            var ParameterReader = new TypeDataFileReader(_processAreaDataPath);
            var ParameterEntities = ParameterReader.ReadAllEntities<OtisParameterTransformedEntity>("Parameter", "*.tsv");

            List<string> encounteredInputParameterNames = new List<string>();

            var ODSINPUTFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, ODS), _objectCountPerFile)
            {
                FileBaseName = $"TR_ODSCalInput_MetaData",
                TypeName = "ODSCalInput_MetaData",
                FileExtension = "tsv"
            };

            var failedFilesDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, ODS), _objectCountPerFile)
            {
                FileBaseName = $"TR_FailedParameter_MetaData",
                TypeName = "MissingODSCalSheetToInputRelationship",
                HeaderRow = "Parameter_Name\tODSNumber\tErrorDescription\n",
                FileExtension = "tsv"
            };

            var BreakDown = ODSEntities.ToDictionary(e => e.ITEM_NUMBER, e => e.ID);
            var Parameter = ParameterEntities
                            .Where(e => e.classification == "Global")
                            .ToDictionary(e => e.item_number, e => e.id);

            int objectCount = 0, failedObjectCount = 0;

            using (failedFilesDataFileWriter)
            {
                using (ODSINPUTFileWriter)
                {
                    if (ODSINPUTFileWriter.HeaderRow == null)
                        ODSINPUTFileWriter.HeaderRow = "ConnectionId\tCONFIG_ID\tSourceID\tRelatedId\tCREATED_ON\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tPermissionId\tIS_CURRENT\tIS_RELEASED\tMAJOR_REV\tNOT_LOCKABLE\tGENERATION\tSTATE\tValueList\tSort_Order\n";

                    foreach (var ODSInputEntity in ODSInputEntities)
                    {
                        var ODS_Number = ODSInputEntity.ODSNo;
                        var ParameterName = ODSInputEntity.Parameter_Name;
                        var ConnectionId = TransformerUtils.GetNewArasGuid();
                        var ConfigId = ConnectionId;

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
                        var ValueList = ODSInputEntity.Value;
                        var SourceID = "";
                        var RelatedID = "";

                        if (BreakDown.ContainsKey(ODS_Number) && Parameter.ContainsKey(ParameterName))
                        {
                            SourceID = BreakDown[ODS_Number];
                            RelatedID = Parameter[ParameterName];
                        }
                        else
                        {
                            if (!BreakDown.ContainsKey(ODS_Number)) 
                            {
                                if (!Parameter.ContainsKey(ParameterName))
                                {
                                    failedFilesDataFileWriter.WriteRow($"{ParameterName}\t{ODS_Number}\tMissing ODS And Parameter\n");
                                    continue;
                                }
                                failedFilesDataFileWriter.WriteRow($"{ParameterName}\t{ODS_Number}\tMissing ODS\n");
                                continue;
                            }
                            else 
                            {
                                failedFilesDataFileWriter.WriteRow($"{ParameterName}\t{ODS_Number}\tMissing Parameter\n");
                                continue;
                            }                            
                        }
                        
                        ODSINPUTFileWriter.WriteRow($"{ConnectionId}\t{ConfigId}\t{SourceID}\t{RelatedID}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{PermissionId}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{NOT_LOCKABLE}\t{GENERATION}\t{STATE}\t{ValueList}\t{ODSInputEntity.Item}\n");
                        objectCount++;
                    }

                }
            }


            _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.Completed, objectCount, failedObjectCount);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, typeName);
        }
    }
}