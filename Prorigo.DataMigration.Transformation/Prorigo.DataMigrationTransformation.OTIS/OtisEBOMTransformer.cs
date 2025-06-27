using System;
using System.Collections.Generic;
using System.Text;
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
    public class OtisEBOMTransformer : IDataTransformer
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisEBOMTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private string[] _processType;
        private readonly IConfigurationSection _typesConfigSection;

        string EBOM = "TR_Conv_CSVToExl_EBOMTemplate";
        string PART = "Part";
        public OtisEBOMTransformer(IConfiguration configuration, ILogger<OtisEBOMTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var CadStructureValidationSection = _configuration.GetSection("OtisEBOM");
            _processAreaDataPath = CadStructureValidationSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = CadStructureValidationSection.GetValue<long>("ObjectCountPerFile");
            _processType = CadStructureValidationSection.GetSection("ProcessType").Get<string[]>();
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
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, EBOM);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, EBOM, TransformStatus.InProgress);

            var EBOMWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "Part"), _objectCountPerFile)
            {
                FileBaseName = $"TR{processtype}_PARTBOM",
                TypeName = $"{processtype}_PARTBOM",
                FileExtension = "tsv",
            };

            var MissingPartWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "Part"), _objectCountPerFile)
            {
                FileBaseName = $"Missing{processtype}_PART",
                TypeName = $"Missing{processtype}_PART",
                FileExtension = "tsv",
            };

            var PartReader = new TypeDataFileReader(_processAreaDataPath);
            var PartEntities = PartReader.ReadAllEntities<OtisPartTransformedEntity>(PART, "*.tsv");

            var EBOMReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "BOM Files", processtype));
            var EBOMEntities = EBOMReader.ReadAllEntities<OtisEBOMEntity>(EBOM, "*.tsv");

            //Dictionary<string, string> PartEntityMap = new Dictionary<string, string>();

            //foreach (var PartEntity in PartEntities)
            //{
            //    if (!PartEntityMap.ContainsKey(PartEntity.keyed_name))
            //    {
            //        PartEntityMap.Add(PartEntity.keyed_name, PartEntity.id);
            //    }
            //}

            var PartEntityMap = PartEntities.ToDictionary(e => e.item_number.ToUpper(), e => e.id);

            long successCount = 0;

            using (EBOMWriter)
            {
                using (MissingPartWriter)
                {
                    foreach (var EBOMEntity in EBOMEntities)
                    {
                        if (EBOMWriter.HeaderRow == null)
                            EBOMWriter.HeaderRow = "ConnectionId\tCONFIG_ID\tFromFilename\tToFilename\tSOURCE_ID\tRELATED_ID\tCREATED_ON\tCREATED_BY_ID\tPERMISSION_ID\tIS_CURRENT\tIS_RELEASED\tMAJOR_REV\tNOT_LOCKABLE\tGENERATION\tDescription\tQT\tUOM\tDrawing_Number\n";

                        if (MissingPartWriter.HeaderRow == null)
                            MissingPartWriter.HeaderRow = "Source_PartNumber\tRelated_PartNumber\tDrawing_Number\tErrorDescription\n";

                        if (PartEntityMap.ContainsKey(EBOMEntity.Source_Part_Number.ToUpper()) && PartEntityMap.ContainsKey(EBOMEntity.Related_Part_Number.ToUpper()))
                        {
                            var ConnectionId = TransformerUtils.GetNewArasGuid();
                            var CONFIG_ID = ConnectionId;
                            var FromFilename = EBOMEntity.Source_Part_Number;
                            var ToFilename = EBOMEntity.Related_Part_Number;
                            var SOURCE_ID = PartEntityMap[EBOMEntity.Source_Part_Number.ToUpper()];
                            var RELATED_ID = PartEntityMap[EBOMEntity.Related_Part_Number.ToUpper()];
                            var CREATED_ON = DateTime.Now.ToString();
                            var CREATED_BY_ID = "Data Migration";
                            var PERMISSION_ID = "95475AE006E7415794BDC93808DC04D2";
                            var IS_CURRENT = "1";
                            var IS_RELEASED = "1";
                            var MAJOR_REV = "A";
                            var NOT_LOCKABLE = "0";
                            var GENERATION = "1";
                            var Description = EBOMEntity.Description;
                            var QT = EBOMEntity.QT;
                            var UOM = EBOMEntity.UOM;
                            var Drawing_Number = EBOMEntity.Drawing_Number;

                            EBOMWriter.WriteRow($"{ConnectionId}\t{CONFIG_ID}\t{FromFilename}\t{ToFilename}\t{SOURCE_ID}\t{RELATED_ID}\t{CREATED_ON}\t{CREATED_BY_ID}\t{PERMISSION_ID}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{NOT_LOCKABLE}\t{GENERATION}\t{Description}\t{QT}\t{UOM}\t{Drawing_Number}\n");
                            successCount++;
                        }
                        else if (!PartEntityMap.ContainsKey(EBOMEntity.Source_Part_Number) && !PartEntityMap.ContainsKey(EBOMEntity.Related_Part_Number))
                        {
                            var Source_PartNumber = EBOMEntity.Source_Part_Number;
                            var Related_PartNumber = EBOMEntity.Related_Part_Number;
                            var Drawing_Number = EBOMEntity.Drawing_Number;
                            var ErrorDescription = "Source_Part_Number and Related_Part_Number are Missing";
                            MissingPartWriter.WriteRow($"{Source_PartNumber}\t{Related_PartNumber}\t{Drawing_Number}\t{ErrorDescription}\n");
                            successCount++;
                        }
                        else if (!PartEntityMap.ContainsKey(EBOMEntity.Source_Part_Number) && PartEntityMap.ContainsKey(EBOMEntity.Related_Part_Number))
                        {
                            var Source_PartNumber = EBOMEntity.Source_Part_Number;
                            var Related_PartNumber = EBOMEntity.Related_Part_Number;
                            var Drawing_Number = EBOMEntity.Drawing_Number;
                            var ErrorDescription = "Source_Part_Number is Missing";
                            MissingPartWriter.WriteRow($"{Source_PartNumber}\t{Related_PartNumber}\t{Drawing_Number}\t{ErrorDescription}\n");
                            successCount++;
                        }
                        else if (PartEntityMap.ContainsKey(EBOMEntity.Source_Part_Number) && !PartEntityMap.ContainsKey(EBOMEntity.Related_Part_Number))
                        {
                            var Source_PartNumber = EBOMEntity.Source_Part_Number;
                            var Related_PartNumber = EBOMEntity.Related_Part_Number;
                            var Drawing_Number = EBOMEntity.Drawing_Number;
                            var ErrorDescription = "Related_Part_Number is Missing";
                            MissingPartWriter.WriteRow($"{Source_PartNumber}\t{Related_PartNumber}\t{Drawing_Number}\t{ErrorDescription}\n");
                            successCount++;
                        }
                    }
                }
            }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, EBOM, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, EBOM);
        }
    }
}
