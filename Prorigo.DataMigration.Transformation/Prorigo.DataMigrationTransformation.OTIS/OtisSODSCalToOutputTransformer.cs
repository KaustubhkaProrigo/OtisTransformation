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
    class OtisSODSCalToOutputTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisSODSCalToOutputTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;

        string CalculationSheet = "CalculationSheet";
        string SODSOutput = "SODSOutput";
        public OtisSODSCalToOutputTransformer(IConfiguration configuration, ILogger<OtisSODSCalToOutputTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var CalculationSheetToOutputSection = _configuration.GetSection("OtisSODSCalToOutput");
            _processAreaDataPath = CalculationSheetToOutputSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = CalculationSheetToOutputSection.GetValue<long>("ObjectCountPerFile");
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
            var SODSToOutputWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "SODS"), _objectCountPerFile)
            {
                FileBaseName = $"SODSCalSheetToOutputRel",
                TypeName = $"SODSCalSheetToOutputRel",
                FileExtension = "tsv",
            };

            var MissingRecordWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "SODS", "SODSCalSheetToOutputRel"), _objectCountPerFile)
            {
                FileBaseName = $"MissingSODSCalSheetToOutputRel",
                TypeName = $"MissingSODSCalSheetToOutputRel",
                FileExtension = "tsv",
            };

            var ParameterReader = new TypeDataFileReader(_processAreaDataPath);
            var ParameterEntities = ParameterReader.ReadAllEntities<OtisParameterTransformedEntity>("Parameter", "*.tsv"); //Read Parameter Entities

            Dictionary<string, string> ParameterEntityMap = new Dictionary<string, string>();
            foreach (var ParameterEntity in ParameterEntities)
            {
                if (ParameterEntity.classification == "Internal" && !ParameterEntityMap.ContainsKey(ParameterEntity.keyed_name))//Internal
                {
                    ParameterEntityMap.Add(ParameterEntity.keyed_name, ParameterEntity.id);
                }
            }

            var CalculationSheetReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "SODS"));
            var CalculationSheetEntities = CalculationSheetReader.ReadAllEntities<OtisSODSEntity>("SODSCalculationSheet", "*.tsv"); //Read CalculationSheet Entities

            Dictionary<string, string> CalculationSheetEntityMap = new Dictionary<string, string>();
            foreach (var CalculationSheetEntity in CalculationSheetEntities)
            {
                if (!CalculationSheetEntityMap.ContainsKey(CalculationSheetEntity.KEYED_NAME))
                {
                    CalculationSheetEntityMap.Add(CalculationSheetEntity.KEYED_NAME, CalculationSheetEntity.ID);
                }
            }

            var OutputReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath,"BOM Files", "SODS"));
            var OutputEntities = OutputReader.ReadAllEntities<SODSOutputEntity>(SODSOutput, "*.tsv"); //Read Output Entities

            var ExpressionReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "SODS"));
            var ExpressionEntities = ExpressionReader.ReadAllEntities<SODSConditionExpressionEntity>("SODSCS_OutputConditionExpression", "*.tsv"); //Read Expression Entitie


            Dictionary<string, (string, string, string)> ExpressionEntityMap = new Dictionary<string, (string, string, string)>();
            foreach (var ExpressionEntity in ExpressionEntities)
            {
                if (!ExpressionEntityMap.ContainsKey(ExpressionEntity.SODSNo + "|" + ExpressionEntity.ParameterName))
                {
                    ExpressionEntityMap.Add(ExpressionEntity.SODSNo + "|" + ExpressionEntity.ParameterName, (ExpressionEntity.ConditionExpression, ExpressionEntity.isExpression, ExpressionEntity.ConditionTable));
                }
            }


            long successCount = 0;

            using (SODSToOutputWriter)
            {
                using (MissingRecordWriter)
                {
                    foreach (var OutputEntity in OutputEntities)
                    {
                        if (SODSToOutputWriter.HeaderRow == null)
                            SODSToOutputWriter.HeaderRow = "ConnectionId\tCONFIG_ID\tFromFilename\tToFilename\tSourceID\tRelatedID\tCREATED_ON\tCREATED_BY_ID\tPERMISSION_ID\tIS_CURRENT\tIS_RELEASED\tMAJOR_REV\tNOT_LOCKABLE\tGENERATION\tCondition_Expression\tConditionTable\tIs_Expression\tAllowed_Ranges\tSort_Order\n";

                        if (MissingRecordWriter.HeaderRow == null)
                            MissingRecordWriter.HeaderRow = "SODSNo\tParameter\tError Description\n";

                        if (CalculationSheetEntityMap.ContainsKey(OutputEntity.SODSNO))
                        {
                            var ConnectionId = TransformerUtils.GetNewArasGuid();
                            var CONFIG_ID = ConnectionId;
                            var SourceID = CalculationSheetEntityMap[OutputEntity.SODSNO];

                            string RelatedId = null;

                            string parameterPrefix = OutputEntity.ParameterName;
                            int lastDigitIndex = parameterPrefix.Length - 1;

                            while (lastDigitIndex >= 0 && Char.IsDigit(parameterPrefix[lastDigitIndex]))
                            {
                                lastDigitIndex--;
                            }
                            parameterPrefix = parameterPrefix.Substring(0, lastDigitIndex + 1);

                            if (ParameterEntityMap.ContainsKey(parameterPrefix))
                            {
                                RelatedId = ParameterEntityMap[parameterPrefix];
                            }

                            if (RelatedId == null)
                            {
                                Console.WriteLine($"Parameter not found: {parameterPrefix}");
                                MissingRecordWriter.WriteRow($"{OutputEntity.SODSNO}\t{parameterPrefix}\tMissing Parameter\n");
                                continue;//add loger
                            }

                            var CREATED_ON = DateTime.Now.ToString();
                            var CREATED_BY_ID = "Data Migration";
                            var PERMISSION_ID = "95475AE006E7415794BDC93808DC04D2";
                            var IS_CURRENT = "1";
                            var IS_RELEASED = "1";
                            var MAJOR_REV = "A";
                            var NOT_LOCKABLE = "0";
                            var GENERATION = "1";
                            var Is_Expression = string.Empty;
                            var Is_Condition_Expression = string.Empty;
                            var ConditionTable = string.Empty;
                            if (ExpressionEntityMap.ContainsKey(OutputEntity.SODSNO + "|" + OutputEntity.ParameterName))
                            {
                                (Is_Condition_Expression, Is_Expression, ConditionTable) = ExpressionEntityMap[OutputEntity.SODSNO + "|" + OutputEntity.ParameterName];
                            }
                            SODSToOutputWriter.WriteRow($"{ConnectionId}\t{CONFIG_ID}\t{OutputEntity.SODSNO}\t{parameterPrefix}\t{SourceID}\t{RelatedId}\t{CREATED_ON}\t{CREATED_BY_ID}\t{PERMISSION_ID}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{NOT_LOCKABLE}\t{GENERATION}\t{Is_Condition_Expression}\t{ConditionTable}\t{Is_Expression}\t{OutputEntity.ValueListorValueRange}\t{OutputEntity.ID}\n");
                        }
                        else
                        {
                            MissingRecordWriter.WriteRow($"{OutputEntity.SODSNO}\t{OutputEntity.ParameterName}\tMissing SODSno\n");
                            successCount++;
                        }
                    }
                }
                successCount++;
            }
            _migrationDiagnostics.LogTransformTypeStatus(transformName, CalculationSheet, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, CalculationSheet);
        }
    }
}