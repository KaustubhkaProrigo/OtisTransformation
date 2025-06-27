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
using System.Configuration;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    class OtisPartToCalculationSheetRelationshipTransformer : IDataTransformer
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisPartToCalculationSheetRelationshipTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;
        private string[] _processType;

        string CalculationSheetRelation = "CalculationSheetToPartRelationship";
        string EBOM = "TR_Conv_CSVToExl_EBOMTemplate";
        string PART = "Part";
        public OtisPartToCalculationSheetRelationshipTransformer(IConfiguration configuration, ILogger<OtisPartToCalculationSheetRelationshipTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var CalculationSheetToPartSection = _configuration.GetSection("OtisPartToCalculationSheetRelationship");
            _processAreaDataPath = CalculationSheetToPartSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = CalculationSheetToPartSection.GetValue<long>("ObjectCountPerFile");
            _processType = CalculationSheetToPartSection.GetSection("ProcessType").Get<string[]>();
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
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, CalculationSheetRelation);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, CalculationSheetRelation, TransformStatus.InProgress);

            var CalculationSheetToPartWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, PART), _objectCountPerFile)
            {
                FileBaseName = $"TR{processtype}_PartToCalculationSheetRelationship",
                TypeName = $"{processtype}PartToCalculationSheet",
                FileExtension = "tsv",
            };

            var MissingPartWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, PART), _objectCountPerFile)
            {
                FileBaseName = $"Missing{processtype}_PARTToCalSheet",
                TypeName = $"Missing{processtype}_PARTToCalSheet",
                FileExtension = "tsv",
            };

            var PartReader = new TypeDataFileReader(_processAreaDataPath);
            var PartEntities = PartReader.ReadAllEntities<OtisPartTransformedEntity>(PART, "*.tsv"); //Read Part Entities

            Dictionary<string, string> PartEntityMap = new Dictionary<string, string>();
            foreach (var PartEntity in PartEntities)
            {
                if (!PartEntityMap.ContainsKey(PartEntity.keyed_name))
                {
                    PartEntityMap.Add(PartEntity.keyed_name, PartEntity.id);
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

            var EBOMReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "BOM Files", processtype));
            var EBOMEntities = EBOMReader.ReadAllEntities<OtisEBOMEntity>(EBOM, "*.tsv"); //Read EBOM Entities

            HashSet<(string Drawing_Number, string Source_Part_Number)> DrawingNoPartNoPair = new HashSet<(string, string)>();

            long successCount = 0;

            using (CalculationSheetToPartWriter)
            {
                using (MissingPartWriter)
                {

                    foreach (var EBOMEntity in EBOMEntities)
                {
                    if (CalculationSheetToPartWriter.HeaderRow == null)
                        CalculationSheetToPartWriter.HeaderRow = "ConnectionId\tCONFIG_ID\tFromFilename\tToFilename\tSourceID\tRelatedId\tCREATED_ON\tCREATED_BY_ID\tPERMISSION_ID\tIS_CURRENT\tIS_RELEASED\tMAJOR_REV\tNOT_LOCKABLE\tGENERATION\n";

                        if (MissingPartWriter.HeaderRow == null)
                            MissingPartWriter.HeaderRow = "DrawingNo\tPart\tError Description\n";

                        if (!DrawingNoPartNoPair.Contains((EBOMEntity.Drawing_Number, EBOMEntity.Source_Part_Number)))
                        {
                            if (CalculationSheetEntityMap.ContainsKey(EBOMEntity.Drawing_Number) && (PartEntityMap.ContainsKey(EBOMEntity.Source_Part_Number)))
                            {
                                var ConnectionId = TransformerUtils.GetNewArasGuid();
                                var CONFIG_ID = ConnectionId;
                                var FromFilename = EBOMEntity.Source_Part_Number; 
                                var ToFilename = EBOMEntity.Drawing_Number; 
                                var SourceID = PartEntityMap[EBOMEntity.Source_Part_Number]; 
                                var RelatedId = CalculationSheetEntityMap[EBOMEntity.Drawing_Number]; 
                                //var FromFilename = EBOMEntity.Drawing_Number;
                                //var ToFilename = EBOMEntity.Source_Part_Number;
                                //var SourceID = CalculationSheetEntityMap[EBOMEntity.Drawing_Number];
                                //var RelatedId = PartEntityMap[EBOMEntity.Source_Part_Number];

                                var CREATED_ON = DateTime.Now.ToString();
                                var CREATED_BY_ID = "Data Migration";
                                var PERMISSION_ID = "95475AE006E7415794BDC93808DC04D2";
                                var IS_CURRENT = "1";
                                var IS_RELEASED = "1";
                                var MAJOR_REV = "A";
                                var NOT_LOCKABLE = "0";
                                var GENERATION = "1";

                                DrawingNoPartNoPair.Add((EBOMEntity.Drawing_Number, EBOMEntity.Source_Part_Number));
                                CalculationSheetToPartWriter.WriteRow($"{ConnectionId}\t{CONFIG_ID}\t{FromFilename}\t{ToFilename}\t{SourceID}\t{RelatedId}\t{CREATED_ON}\t{CREATED_BY_ID}\t{PERMISSION_ID}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{NOT_LOCKABLE}\t{GENERATION}\n");
                                successCount++;
                            }
                            else 
                            {
                                var FromFilename = EBOMEntity.Drawing_Number;
                                var ToFilename = EBOMEntity.Source_Part_Number;
                                if (!CalculationSheetEntityMap.ContainsKey(EBOMEntity.Drawing_Number))
                                {
                                    if (!PartEntityMap.ContainsKey(EBOMEntity.Source_Part_Number))
                                    {
                                        MissingPartWriter.WriteRow($"{FromFilename}\t{ToFilename}\tMissng Drawing And Part\n");
                                        successCount++;
                                    }
                                    MissingPartWriter.WriteRow($"{FromFilename}\t{ToFilename}\tMissng Drawing\n");
                                    successCount++;
                                }            
                                MissingPartWriter.WriteRow($"{FromFilename}\t{ToFilename}\tMissng Part\n");
                                successCount++;
                            }
                        }
                        }
                    }
            }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, CalculationSheetRelation, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, CalculationSheetRelation);
        }
    }
}
