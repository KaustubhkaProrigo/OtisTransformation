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


namespace Prorigo.DataMigrationTransformation.OTIS
{
    public class Otis_BuyerPlannerTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Otis_BuyerPlannerTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;
        private const string Ots_BuyerPlanner = "Conv_ExcelToTSV_BuyerPlannerTemplate";



        public Otis_BuyerPlannerTransformer(IConfiguration configuration, ILogger<Otis_BuyerPlannerTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var CADValidationSection = _configuration.GetSection("Otis_BuyerPlanner");
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
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, Ots_BuyerPlanner);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, Ots_BuyerPlanner, TransformStatus.InProgress);

            var Ots_BuyerPlannerItemWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"TR_Ots_BuyerPlanner",
                TypeName = "BuyerPlanner",
                FileExtension = "tsv",
            };



            var OtisBuyerPlannerItemReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "BuyerPlanner"));
            var OtisBuyerPlannerItemEntities = OtisBuyerPlannerItemReader.ReadAllEntities<OtisBuyerPlannerEntity>(Ots_BuyerPlanner,"*.tsv");



            long successCount = 0;


            using (Ots_BuyerPlannerItemWriter)
            {
                foreach (var BuyerPlannerEntity in OtisBuyerPlannerItemEntities)
                {
                    if (Ots_BuyerPlannerItemWriter.HeaderRow == null)
                    {
                        Ots_BuyerPlannerItemWriter.HeaderRow = "id\tKeyed_name\tAddress_Book_No\tName\tLong_Address_Number\tSearch_Type\tCONFIG_ID\tCREATED_ON" +
                         "\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tCURRENT_STATE\tPERMISSION_ID\tSTATE\tIS_CURRENT\tMAJOR_REV\tMINOR_REV\tIS_RELEASED\tNOT_LOCKABLE\tGENERATION\tNEW_VERSION\n";
                    }

                    BuyerPlannerEntity.id = TransformerUtils.GetNewArasGuid();
                    BuyerPlannerEntity.Keyed_name = BuyerPlannerEntity.Alpha_Name;
                    BuyerPlannerEntity.CREATED_ON = DateTime.Now.ToString();
                    BuyerPlannerEntity.CREATED_BY_ID = "Data Migration";
                    BuyerPlannerEntity.MODIFIED_ON = DateTime.Now.ToString();
                    BuyerPlannerEntity.MODIFIED_BY_ID = "Data Migration";
                    BuyerPlannerEntity.CURRENT_STATE = "";
                    BuyerPlannerEntity.STATE = "";
                    BuyerPlannerEntity.IS_CURRENT = "1";
                    BuyerPlannerEntity.MAJOR_REV = "A";
                    BuyerPlannerEntity.MINOR_REV = "1";
                    BuyerPlannerEntity.IS_RELEASED = "0";
                    BuyerPlannerEntity.NOT_LOCKABLE = "0";
                    BuyerPlannerEntity.GENERATION = "1";
                    BuyerPlannerEntity.NEW_VERSION = "1";
                    BuyerPlannerEntity.CONFIG_ID = BuyerPlannerEntity.id;
                    BuyerPlannerEntity.PERMISSION_ID = "5D952B83080944E291090705FCD3F478";

                    Ots_BuyerPlannerItemWriter.WriteRow(BuyerPlannerEntity.DataRow);


                    successCount++;
                }
            }
            _migrationDiagnostics.LogTransformTypeStatus(transformName, Ots_BuyerPlanner, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, Ots_BuyerPlanner);
        }
    }
}
