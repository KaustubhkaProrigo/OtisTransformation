using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
    public class OtisProductTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisProductTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private string _classification;
        private readonly IConfigurationSection _typesConfigSection;

        private const string OtisProduct = "OtisProduct";

        public OtisProductTransformer(IConfiguration configuration, ILogger<OtisProductTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var CADValidationSection = _configuration.GetSection("OtisProduct");
            _processAreaDataPath = CADValidationSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = CADValidationSection.GetValue<long>("ObjectCountPerFile");
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
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, OtisProduct);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, OtisProduct, TransformStatus.InProgress);

            var OtisProductWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"OtisProduct",
                TypeName = "Product",
                FileExtension = "tsv",
            };

            var OtisEntityReader = new TypeDataFileReader(_processAreaDataPath);
            var OtisEntities = OtisEntityReader.ReadAllEntities<OTSProductEntity>(OtisProduct);

            //var OtisGroups = OtisEntities
            //               .GroupBy(entity => new { entity.Part_Number })
            //               .ToList();


            long successCount = 0;

            using (OtisProductWriter)
            {
                foreach (var OtisGroup in OtisEntities)
                {
                    if (OtisProductWriter.HeaderRow == null)
                    {
                        OtisProductWriter.HeaderRow = "ID\tCONFIG_ID\tKEYED_NAME\tITEM_NUMBER\tNAME\tPlatform_Group\tPlatform_No\tMR_MRL\tBelted_Roped\tController\tUnderslung_Overslung\tRoping\tProduct_No\tDescription\tProduct_Name\tCOMPY_Region\tCODE\tDL_Duty_Load\tV_Speed\tR_Rise_Max\tCREATED_BY_ID\tCREATED_ON\tCURRENT_STATE\tGENERATION\tIS_CURRENT\tIS_RELEASED\tMAJOR_REV\tMINOR_REV\tPERMISSION_ID\tSTATE\n";
                    }

                    if(OtisGroup.MR_MRL.Contains("MRL"))
                    {
                        OtisGroup.MR_MRL = "ML";
                    }
                  
                    OtisGroup.ID = TransformerUtils.GetNewArasGuid();
                    OtisGroup.CONFIG_ID = OtisGroup.ID;
                    OtisGroup.KEYED_NAME = OtisGroup.Product_No;
                    OtisGroup.ITEM_NUMBER = OtisGroup.Product_No;
                    OtisGroup.NAME = OtisGroup.Product_No;
                    OtisGroup.CREATED_BY_ID = "DM";
                    OtisGroup.CREATED_ON = DateTime.Now.ToString();
                    OtisGroup.CURRENT_STATE = "B20F5D65D72948668BC04B30F4E58B11";
                    OtisGroup.GENERATION = "1";
                    OtisGroup.IS_CURRENT = "1";
                    OtisGroup.IS_RELEASED = "1";
                    OtisGroup.MAJOR_REV = "A";
                    OtisGroup.MINOR_REV = "1";
                    OtisGroup.PERMISSION_ID = "2382DEC5CAAC42BBA9D27DFD0B0D742C";
                    OtisGroup.STATE = "Released";
                    OtisGroup.Controller = Regex.Replace(OtisGroup.Controller, @"\s*,\s*", ",");
                    


                    OtisProductWriter.WriteRow(OtisGroup.DataRow);

                }
                successCount++;
            }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, OtisProduct, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, OtisProduct);
        }
    }
}
