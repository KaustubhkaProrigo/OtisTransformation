using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Engineering;
using Prorigo.DataMigrationTransformation.OTIS.Entities;
using Prorigo.Plm.DataMigration.OtisDataTransformer;
using Prorigo.Plm.DataMigration.Utilities;
using System.Security.Cryptography;


namespace Prorigo.DataMigrationTransformation.OTIS
{
    public class OtisBranchPlantTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisBranchPlantTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;
        private const string Ots_BranchPlant = "Conv_ExcelToTSV_BranchPlantTemplate";
        private const string OtisBranchPlantLocation = "Conv_ExcelToTSV_BranchPlantLocationTemplate";



        public OtisBranchPlantTransformer(IConfiguration configuration, ILogger<OtisBranchPlantTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var CADValidationSection = _configuration.GetSection("OtisBranchPlant");
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
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, Ots_BranchPlant);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, Ots_BranchPlant, TransformStatus.InProgress);

            var Ots_BranchPlantItemWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"TR_Ots_BranchPlant",
                TypeName = "BranchPlant",
                FileExtension = "tsv",
            };

            var Otis_BranchPlantLocationRelWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "BranchPlant"), _objectCountPerFile)
            {
                FileBaseName = $"Otis_BranchPlantLocation",
                TypeName = "BranchPlantLocationRelationship",
                FileExtension = "tsv",
            };


            var OtsBranchPlantItemReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "BranchPlant"));
            var OtsBranchPlantItemEntities = OtsBranchPlantItemReader.ReadAllEntities<OtisBranchPlantEntity>(Ots_BranchPlant, "*.tsv");

            var OtisBranchPlantLocationEntityReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "BranchPlantLocation"));
            var OtisBranchPlantLocationEntities = OtisBranchPlantLocationEntityReader.ReadAllEntities<OtisBranchPlantLocationEntity>(OtisBranchPlantLocation, "*.tsv");

            //var OtisGroups = OtisBranchPlantLocationEntities
            //                   .GroupBy(entity => new { entity.Branch_Plant})
            //                   .ToList();

            Dictionary<string, string> BranchPlantMap = new Dictionary<string, string>();

            long successCount = 0;


            using (Otis_BranchPlantLocationRelWriter)
            {
                using (Ots_BranchPlantItemWriter)
                {
                    foreach (var BranchPlantEntity in OtsBranchPlantItemEntities)
                    {
                        if (Ots_BranchPlantItemWriter.HeaderRow == null)
                        {
                            Ots_BranchPlantItemWriter.HeaderRow = "id\tKeyed_name\tName\tDescription\tCompany_Code\tCompany_Code_Description\tCONFIG_ID\tCREATED_ON" +
                             "\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tCURRENT_STATE\tPERMISSION_ID\tSTATE\tIS_CURRENT\tMAJOR_REV\tMINOR_REV\tIS_RELEASED\tNOT_LOCKABLE\tGENERATION\tNEW_VERSION\n";
                        }

                        
                        BranchPlantEntity.id = TransformerUtils.GetNewArasGuid();
                        BranchPlantEntity.Keyed_name = BranchPlantEntity.Name;
                        BranchPlantEntity.CREATED_ON = DateTime.Now.ToString();
                        BranchPlantEntity.CREATED_BY_ID = "Data Migration";
                        BranchPlantEntity.MODIFIED_ON = DateTime.Now.ToString();
                        BranchPlantEntity.MODIFIED_BY_ID = "Data Migration";
                        BranchPlantEntity.CURRENT_STATE = "";
                        BranchPlantEntity.STATE = "";
                        BranchPlantEntity.IS_CURRENT = "1";
                        BranchPlantEntity.MAJOR_REV = "A";
                        BranchPlantEntity.MINOR_REV = "1";
                        BranchPlantEntity.IS_RELEASED = "0";
                        BranchPlantEntity.NOT_LOCKABLE = "0";
                        BranchPlantEntity.GENERATION = "1";
                        BranchPlantEntity.NEW_VERSION = "1";
                        BranchPlantEntity.CONFIG_ID = BranchPlantEntity.id;
                        BranchPlantEntity.PERMISSION_ID = "9122CD065CF04141B8EFE263FC80BEA4";

                        BranchPlantMap[BranchPlantEntity.Name] = BranchPlantEntity.id;
                        Ots_BranchPlantItemWriter.WriteRow(BranchPlantEntity.DataRow);

                    }
                    successCount++;
                }
                foreach (var BranchPlantLocationEntity in OtisBranchPlantLocationEntities)
                {
                    if (Otis_BranchPlantLocationRelWriter.HeaderRow == null)
                    {
                        Otis_BranchPlantLocationRelWriter.HeaderRow = "ConnectionID\tConfig_id\tSource_id\tBranch_Plant\tDate_Updated\tLocation\tPermission_id" +
                       "\tCreated_by_id\tCreated_on\tModified_by_id\tModified_on\tis_current\tmajor_rev\tminor_rev\tis_released\tnot_lockable\tgeneration\tnew_version\tbehavior\n";
                    }
                
                    if (BranchPlantMap.ContainsKey(BranchPlantLocationEntity.Branch_Plant))
                    {
                         BranchPlantLocationEntity.ConnectionID = TransformerUtils.GetNewArasGuid();
                         BranchPlantLocationEntity.Config_id = BranchPlantLocationEntity.ConnectionID;
                         BranchPlantLocationEntity.Source_id = BranchPlantMap[BranchPlantLocationEntity.Branch_Plant];
                         BranchPlantLocationEntity.Permission_id = "9122CD065CF04141B8EFE263FC80BEA4";
                         BranchPlantLocationEntity.Created_by_id = "Data Migration";
                         BranchPlantLocationEntity.Created_on = DateTime.Now.ToString();
                         BranchPlantLocationEntity.Modified_by_id = "Data Migration";
                         BranchPlantLocationEntity.Modified_on = DateTime.Now.ToString();
                         BranchPlantLocationEntity.is_current = "1";
                         BranchPlantLocationEntity.major_rev = "A";
                         BranchPlantLocationEntity.minor_rev = "1";
                         BranchPlantLocationEntity.is_released = "0";
                         BranchPlantLocationEntity.not_lockable = "0";
                         BranchPlantLocationEntity.generation = "1";
                         BranchPlantLocationEntity.new_version = "1";
                         BranchPlantLocationEntity.behavior = "float";

                         Otis_BranchPlantLocationRelWriter.WriteRow(BranchPlantLocationEntity.DataRow);

                    }
                }
            }
            _migrationDiagnostics.LogTransformTypeStatus(transformName, Ots_BranchPlant, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, Ots_BranchPlant);
        }
    }
}
