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
    class ODSCalToOutputParameterTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ODSCalToOutputParameterTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;

        string CalculationSheet = "CalculationSheet";
        public ODSCalToOutputParameterTransformer(IConfiguration configuration, ILogger<ODSCalToOutputParameterTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var CalculationSheetToOutputSection = _configuration.GetSection("ODSCalToOutputParameter");
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
            var ODSToOutputWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "ODS"), _objectCountPerFile)
            {
                FileBaseName = $"ODSCalSheetToOutputRelationship",
                TypeName = $"ODSCalSheetToOutputRelationship",
                FileExtension = "tsv",
            };

            var MissingRecordWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "ODS"), _objectCountPerFile)
            {
                FileBaseName = $"MissingODSCalSheetToOutputRelationship",
                TypeName = $"MissingODSCalSheetToOutputRelationship",
                FileExtension = "tsv",
            };

            var ParameterReader = new TypeDataFileReader(_processAreaDataPath);
            var ParameterEntities = ParameterReader.ReadAllEntities<OtisParameterTransformedEntity>("Parameter", "*.tsv"); //Read Parameter Entities

            Dictionary<string, string> ParameterEntityMap = new Dictionary<string, string>();
            foreach (var ParameterEntity in ParameterEntities)
            {
                if (ParameterEntity.classification == "Internal" && !ParameterEntityMap.ContainsKey(ParameterEntity.keyed_name))
                {
                    ParameterEntityMap.Add(ParameterEntity.keyed_name, ParameterEntity.id);
                }
            }

            var ODSReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "ODS"));
            var ODSEntities = ODSReader.ReadAllEntities<OtisODSCalculationSheetEntity>("CalSheet_ODS", "*.tsv"); //Read CalculationSheet Entities

            Dictionary<string, string> ODSEntityMap = new Dictionary<string, string>();
            foreach (var ODSEntity in ODSEntities)
            {
                if (!ODSEntityMap.ContainsKey(ODSEntity.KEYED_NAME))
                {
                    ODSEntityMap.Add(ODSEntity.KEYED_NAME, ODSEntity.ID);
                }
            }

            var OutputReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath,"BOM Files","ODS"));
            var OutputEntities = OutputReader.ReadAllEntities<ODSOutputEntity>("OutputParameters", "*.tsv"); //Read Output Entities

            var ExpressionReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "ODS"));
            var ExpressionEntities = ExpressionReader.ReadAllEntities<ODSConditionExpressionEntity>("ODSConditionExpression", "*.tsv"); //Read Expression Entitie


            Dictionary<string, (string, string, string)> ExpressionEntityMap = new Dictionary<string, (string, string, string)>();
            foreach (var ExpressionEntity in ExpressionEntities)
            {
                if (!ExpressionEntityMap.ContainsKey(ExpressionEntity.ODSNo + "|" + ExpressionEntity.Parameter))
                {
                    ExpressionEntityMap.Add(ExpressionEntity.ODSNo + "|" + ExpressionEntity.Parameter, (ExpressionEntity.ConditionExpression, ExpressionEntity.isExpression, ExpressionEntity.ConditionTable));
                }
            }

            long successCount = 0;

            using (ODSToOutputWriter)
            {
                using (MissingRecordWriter)
                {
                    foreach (var OutputEntity in OutputEntities)
                    {
                        if (ODSToOutputWriter.HeaderRow == null)
                            ODSToOutputWriter.HeaderRow = "ConnectionId\tCONFIG_ID\tFromFilename\tToFilename\tSourceID\tRelatedID\tCREATED_ON\tCREATED_BY_ID\tPERMISSION_ID\tIS_CURRENT\tIS_RELEASED\tMAJOR_REV\tNOT_LOCKABLE\tGENERATION\tCondition_Expression\tConditionTable\tIs_Expression\tSortOrder\n";

                        if (MissingRecordWriter.HeaderRow == null)
                            MissingRecordWriter.HeaderRow = "ODSNo\tParameter\tErrorDescription\n";

                        if (ODSEntityMap.ContainsKey(OutputEntity.ODSNo))
                        {
                            var ConnectionId = TransformerUtils.GetNewArasGuid();
                            var CONFIG_ID = ConnectionId;
                            var FromFilename = OutputEntity.ODSNo;
                            string ToFilename = OutputEntity.Parameter;
                            var SourceID = ODSEntityMap[OutputEntity.ODSNo];

                            string RelatedId = null;

                            string parameterPrefix = OutputEntity.Parameter;
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
                                MissingRecordWriter.WriteRow($"{FromFilename}\t{ToFilename}\tMissing Parameter\n");
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
                            if (ExpressionEntityMap.ContainsKey(OutputEntity.ODSNo + "|" + OutputEntity.Parameter))
                            {
                                (Is_Condition_Expression, Is_Expression, ConditionTable) = ExpressionEntityMap[OutputEntity.ODSNo + "|" + OutputEntity.Parameter];
                            }

                            ODSToOutputWriter.WriteRow($"{ConnectionId}\t{CONFIG_ID}\t{FromFilename}\t{ToFilename}\t{SourceID}\t{RelatedId}\t{CREATED_ON}\t{CREATED_BY_ID}\t{PERMISSION_ID}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{NOT_LOCKABLE}\t{GENERATION}\t{Is_Condition_Expression}\t{ConditionTable}\t{Is_Expression}\t{OutputEntity.ID}\n");
                        }
                        else
                        {
                            string FromFilename = OutputEntity.ODSNo;
                            string ToFilename = OutputEntity.Parameter;

                            MissingRecordWriter.WriteRow($"{FromFilename}\t{ToFilename}\tMissing ODS No\n");
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
