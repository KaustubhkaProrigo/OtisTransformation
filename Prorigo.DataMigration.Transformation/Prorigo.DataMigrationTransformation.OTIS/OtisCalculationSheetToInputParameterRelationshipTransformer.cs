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

namespace Prorigo.DataMigrationTransformation.OTIS
{
    class OtisCalculationSheetToInputParameterRelationshipTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisCalculationSheetToInputParameterRelationshipTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;
        private string[] _processType;

        string CalculationSheet = "CalculationSheet";
        public OtisCalculationSheetToInputParameterRelationshipTransformer(IConfiguration configuration, ILogger<OtisCalculationSheetToInputParameterRelationshipTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var CalculationSheetToInputSection = _configuration.GetSection("OtisCalculationSheetToInputParameterRelationship");
            _processAreaDataPath = CalculationSheetToInputSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = CalculationSheetToInputSection.GetValue<long>("ObjectCountPerFile");
            _processType = CalculationSheetToInputSection.GetSection("ProcessType").Get<string[]>();
        }

        public void Transform(string LicenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");
            if (isLicenValid)
            {
                var className = this.GetType().Name;
                var transformName = className.Substring(0, className.IndexOf("Transformer"));
                foreach (var processtype in _processType)
                {
                    DefaultValueAdder(transformName, processtype);
                }
            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }

        public void DefaultValueAdder(string transformName, string processtype)
        {
            var CalculationSheetToInputWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, $"{processtype}_CalculationSheet"), _objectCountPerFile)
            {
                FileBaseName = $"TR{processtype}_CalculationSheetToInputRelationship",
                TypeName = $"CalculationSheetToInput",
                FileExtension = "tsv",
            };

            var MissingRecordWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, $"{processtype}_CalculationSheet"), _objectCountPerFile)
            {
                FileBaseName = $"Missing{processtype}CalculationSheetToInputRelationship",
                TypeName = $"Missing{processtype}CalculationSheetToInput",
                FileExtension = "tsv",
            };

            var ParameterReader = new TypeDataFileReader(_processAreaDataPath);
            var ParameterEntities = ParameterReader.ReadAllEntities<OtisParameterTransformedEntity>("Parameter", "*.tsv"); //Read Parameter Entities

            Dictionary<string, string> ParameterEntityMap = new Dictionary<string, string>();
            foreach (var ParameterEntity in ParameterEntities)
            {
                if (!ParameterEntityMap.ContainsKey(ParameterEntity.keyed_name))
                {
                    ParameterEntityMap.Add(ParameterEntity.keyed_name, ParameterEntity.id);
                }
            }

            var CalculationSheetReader = new TypeDataFileReader(_processAreaDataPath);
            var CalculationSheetEntities = CalculationSheetReader.ReadAllEntities<OtisCalculationSheetEntity>($"{processtype}_CalculationSheet", "*.tsv"); //Read CalculationSheet Entities

            Dictionary<string, string> CalculationSheetEntityMap = new Dictionary<string, string>();
            foreach (var CalculationSheetEntity in CalculationSheetEntities)
            {
                if (!CalculationSheetEntityMap.ContainsKey(CalculationSheetEntity.KEYED_NAME))
                {
                    CalculationSheetEntityMap.Add(CalculationSheetEntity.KEYED_NAME, CalculationSheetEntity.Id);
                }
            }

            var InputReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "InputParameters"));
            var InputEntities = InputReader.ReadAllEntities<OtisInputOutputEntity>(processtype, "*.tsv"); //Read Input Entities

            long successCount = 0;

            using (CalculationSheetToInputWriter)
            {
                using (MissingRecordWriter)
                {
                    foreach (var InputEntity in InputEntities)
                    {
                        if (CalculationSheetToInputWriter.HeaderRow == null)
                            CalculationSheetToInputWriter.HeaderRow = "ConnectionId\tCONFIG_ID\tFromFilename\tToFilename\tSourceID\tRelatedId\tCREATED_ON\tCREATED_BY_ID\tPERMISSION_ID\tIS_CURRENT\tIS_RELEASED\tMAJOR_REV\tNOT_LOCKABLE\tGENERATION\tAllowed_Ranges\tSort_Order\n";

                        if (MissingRecordWriter.HeaderRow == null)
                            MissingRecordWriter.HeaderRow = "DrawingNo\tParameter\tError Description\n";

                        if (CalculationSheetEntityMap.ContainsKey(InputEntity.DrawingNo) && (ParameterEntityMap.ContainsKey(InputEntity.Parameter_Name)))
                        {
                            var ConnectionId = TransformerUtils.GetNewArasGuid();
                            var CONFIG_ID = ConnectionId;
                            var FromFilename = InputEntity.DrawingNo;
                            var ToFilename = InputEntity.Parameter_Name;
                            var SourceID = CalculationSheetEntityMap[InputEntity.DrawingNo];
                            var RelatedId = ParameterEntityMap[InputEntity.Parameter_Name];
                            var CREATED_ON = DateTime.Now.ToString();
                            var CREATED_BY_ID = "Data Migration";
                            var PERMISSION_ID = "95475AE006E7415794BDC93808DC04D2";
                            var IS_CURRENT = "1";
                            var IS_RELEASED = "1";
                            var MAJOR_REV = "A";
                            var NOT_LOCKABLE = "0";
                            var GENERATION = "1";

                            CalculationSheetToInputWriter.WriteRow($"{ConnectionId}\t{CONFIG_ID}\t{FromFilename}\t{ToFilename}\t{SourceID}\t{RelatedId}\t{CREATED_ON}\t{CREATED_BY_ID}\t{PERMISSION_ID}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{NOT_LOCKABLE}\t{GENERATION}\t{InputEntity.ValueList_or_ValueRange}\t{InputEntity.ID}\n");
                            successCount++;
                        }
                        else 
                        {
                            var FromFilename = InputEntity.DrawingNo;
                            var ToFilename = InputEntity.Parameter_Name;
                            if (!CalculationSheetEntityMap.ContainsKey(InputEntity.DrawingNo))
                            {
                                if (!ParameterEntityMap.ContainsKey(InputEntity.Parameter_Name))
                                {
                                    MissingRecordWriter.WriteRow($"{FromFilename}\t{ToFilename}\tMissing Drawing And Parameter\n");
                                    successCount++;
                                }
                                else 
                                {
                                    MissingRecordWriter.WriteRow($"{FromFilename}\t{ToFilename}\tMissing Drawing\n");
                                    successCount++;
                                }                                
                            }
                            else 
                            {
                                MissingRecordWriter.WriteRow($"{FromFilename}\t{ToFilename}\tMissing Parameter\n");
                                successCount++;
                            }                            
                        }
                    }
                }
            }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, CalculationSheet, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, CalculationSheet);
        }
    }
}
