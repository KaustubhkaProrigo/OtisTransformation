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
    public class Otis_SupplierTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Otis_SupplierTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;
        private const string Ots_Supplier = "Conv_ExcelToTSV_SupplierTemplate";



        public Otis_SupplierTransformer(IConfiguration configuration, ILogger<Otis_SupplierTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var CADValidationSection = _configuration.GetSection("Otis_Supplier");
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
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, Ots_Supplier);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, Ots_Supplier, TransformStatus.InProgress);

            var Ots_SupplierItemWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"TR_Ots_Supplier",
                TypeName = "Supplier",
                FileExtension = "tsv",
            };


           
            var OtsSupplierItemReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "Supplier"));
            var OtsSupplierItemEntities = OtsSupplierItemReader.ReadAllEntities<OtisSupplierEntity>(Ots_Supplier,"*.tsv");



            long successCount = 0;


            using (Ots_SupplierItemWriter)
            {
                foreach (var SupplierEntity in OtsSupplierItemEntities)
                {
                    if (Ots_SupplierItemWriter.HeaderRow == null)
                    {
                        Ots_SupplierItemWriter.HeaderRow = "id\tKeyed_name\tAddress_Book_No\tName\tSearch_Type\tSupplier_Designation\tLine_Of_Business\tCONFIG_ID\tCREATED_ON" +
                         "\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tCURRENT_STATE\tPERMISSION_ID\tSTATE\tIS_CURRENT\tMAJOR_REV\tMINOR_REV\tIS_RELEASED\tNOT_LOCKABLE\tGENERATION\tNEW_VERSION\n";
                    }

                    SupplierEntity.id = TransformerUtils.GetNewArasGuid();
                    SupplierEntity.Keyed_name = SupplierEntity.Alpha_Name;
                    SupplierEntity.CREATED_ON = DateTime.Now.ToString();
                    SupplierEntity.CREATED_BY_ID = "Data Migration";
                    SupplierEntity.MODIFIED_ON = DateTime.Now.ToString();
                    SupplierEntity.MODIFIED_BY_ID = "Data Migration";
                    SupplierEntity.CURRENT_STATE = "";
                    SupplierEntity.STATE = "";
                    SupplierEntity.IS_CURRENT = "1";
                    SupplierEntity.MAJOR_REV = "A";
                    SupplierEntity.MINOR_REV = "1";
                    SupplierEntity.IS_RELEASED = "0";
                    SupplierEntity.NOT_LOCKABLE = "0";
                    SupplierEntity.GENERATION = "1";
                    SupplierEntity.NEW_VERSION = "1";
                    SupplierEntity.CONFIG_ID = SupplierEntity.id;
                    SupplierEntity.PERMISSION_ID = "2A97478483E247CBA7D8138B198FA850";


                    Ots_SupplierItemWriter.WriteRow(SupplierEntity.DataRow);
                    

                    successCount++;
                }
            }
            _migrationDiagnostics.LogTransformTypeStatus(transformName, Ots_Supplier, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, Ots_Supplier);
        }
    }
}
