using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Engineering;
using Prorigo.DataMigrationTransformation.OTIS.Entities;
using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.OtisDataTransformer;
using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using Prorigo.Plm.DataMigration.Utilities;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    public class OtisPartTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisPartTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private string _classification;
        private readonly IConfigurationSection _typesConfigSection;

        private const string OtisPart = "Conv_ExcelToTSV_PartTemplate";

        public OtisPartTransformer(IConfiguration configuration, ILogger<OtisPartTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var CADValidationSection = _configuration.GetSection("OtisPart");
            _processAreaDataPath = CADValidationSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = CADValidationSection.GetValue<long>("ObjectCountPerFile");
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
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, OtisPart);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, OtisPart, TransformStatus.InProgress);

            var OtisPartWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"Otis_Part",
                TypeName = "Part",
                FileExtension = "tsv",
            };

            var OtisEntityReader = new TypeDataFileReader(_processAreaDataPath);
            var OtisEntities = OtisEntityReader.ReadAllEntities<OTSPartEntity>(OtisPart);

            var OtisGroups = OtisEntities
                           .GroupBy(entity => new { entity.Part_Number })
                           .ToList();


            long successCount = 0;

            using (OtisPartWriter)
            {
                foreach (var OtisGroup in OtisGroups)
                {
                    if (OtisPartWriter.HeaderRow == null)
                    {
                        OtisPartWriter.HeaderRow = "ARAS_UNIQUENESS_HELPER\tid\tconfig_id\tkeyed_name\titem_number\tname\tdescription\tclassification\tpart_type\tsub_type\tots_uom\teffectivity_date\tots_assembly_mode\tunit\tots_material\tweight\tots_serviceable\tots_us_jurisdiction\tots_us_eccn\tots_us_category\tots_us_rationale\tots_us_source\tots_us_classifieremail\tots_us_date_classified\tots_chinese_description\tots_french_description\tots_japanese_description\tcreated_by_id\tcreated_on\tcurrent_state\tgeneration\tis_current\tis_released\tmajor_rev\tminor_rev\tmodified_on\tpermission_id\tMAKE_BUY\tstate\tots_revision\n";
                    }

                    var firstEntity = OtisGroup.First();
                    firstEntity.ID = TransformerUtils.GetNewArasGuid();
                    firstEntity.CONFIG_ID = firstEntity.ID;
                    firstEntity.ARAS_UNIQUENESS_HELPER = "";
                    firstEntity.KEYED_NAME = firstEntity.Part_Number;
                    firstEntity.Weight = string.IsNullOrEmpty(firstEntity.Weight) ? "0.00" : firstEntity.Weight;
                    firstEntity.Effectivity_Date = "12/31/2099";
                    firstEntity.CREATED_BY_ID = "Data Migration";
                    firstEntity.CREATED_ON = DateTime.Now.ToString();
                    firstEntity.CURRENT_STATE = "42BB3B183A7748C3B4AA9D33F7BF70AC";
                    firstEntity.GENERATION = "1";
                    firstEntity.IS_CURRENT = "1";
                    firstEntity.IS_RELEASED = "1";
                    firstEntity.MAJOR_REV = "A";
                    firstEntity.MINOR_REV = "1";
                    firstEntity.MODIFIED_ON = DateTime.Now.ToString();
                    firstEntity.PERMISSION_ID = "95475AE006E7415794BDC93808DC04D2";
                    firstEntity.STATE = "Released";
                    //firstEntity.Ots_Image = "";


                    OtisPartWriter.WriteRow(firstEntity.DataRow);
                   
                }
                successCount++;
            }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, OtisPart, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, OtisPart);
        }
    }
}
