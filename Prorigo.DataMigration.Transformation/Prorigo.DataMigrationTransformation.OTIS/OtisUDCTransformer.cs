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
    public class OtisUDCTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisUDCTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;
        private const string Ots_UDC = "Conv_ExcelToTSV_UDCTemplate";



        public OtisUDCTransformer(IConfiguration configuration, ILogger<OtisUDCTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var CADValidationSection = _configuration.GetSection("OtisUDC");
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
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, Ots_UDC);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, Ots_UDC, TransformStatus.InProgress);

            var Ots_UDCItemWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"TR_Ots_UDC",
                TypeName = "UDC",
                FileExtension = "tsv",
            };



            var OtsUDCItemReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "UDC"));
            var OtsUDcItemEntities = OtsUDCItemReader.ReadAllEntities<OtisUDCEntity>(Ots_UDC, "*.tsv");



            long successCount = 0;


            using (Ots_UDCItemWriter)
            {
                foreach (var UDCEntity in OtsUDcItemEntities)
                {
                    if (Ots_UDCItemWriter.HeaderRow == null)
                    {
                        Ots_UDCItemWriter.HeaderRow = "id\tKeyed_name\tClassification\tots_code\tots_gl_class\tots_line_type\tots_description_1\tots_description_2\tots_epc_value\tots_special_handling_code\tCONFIG_ID\tCREATED_ON" +
                         "\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tCURRENT_STATE\tPERMISSION_ID\tSTATE\tIS_CURRENT\tMAJOR_REV\tMINOR_REV\tIS_RELEASED\tNOT_LOCKABLE\tGENERATION\tNEW_VERSION\n";
                    }

                    UDCEntity.id = TransformerUtils.GetNewArasGuid();
                    UDCEntity.Keyed_name = UDCEntity.ots_code;
                    UDCEntity.CREATED_ON = DateTime.Now.ToString();
                    UDCEntity.CREATED_BY_ID = "Data Migration";
                    UDCEntity.MODIFIED_ON = DateTime.Now.ToString();
                    UDCEntity.MODIFIED_BY_ID = "Data Migration";
                    UDCEntity.CURRENT_STATE = "";
                    UDCEntity.STATE = "";
                    UDCEntity.IS_CURRENT = "1";
                    UDCEntity.MAJOR_REV = "A";
                    UDCEntity.MINOR_REV = "1";
                    UDCEntity.IS_RELEASED = "0";
                    UDCEntity.NOT_LOCKABLE = "0";
                    UDCEntity.GENERATION = "1";
                    UDCEntity.NEW_VERSION = "1";
                    UDCEntity.CONFIG_ID = UDCEntity.id;
                    UDCEntity.PERMISSION_ID = "6B02D485B6B64867874A1758D1272801";


                    Ots_UDCItemWriter.WriteRow(UDCEntity.DataRow);


                    successCount++;
                }
            }
            _migrationDiagnostics.LogTransformTypeStatus(transformName, Ots_UDC, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, Ots_UDC);
        }
    }
}
