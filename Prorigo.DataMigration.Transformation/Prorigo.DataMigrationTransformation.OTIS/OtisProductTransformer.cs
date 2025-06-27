using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
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

        private const string OtisProduct = "Conv_ExcelToTsv_ODSTemplate_ProductData";
        private const string OtisPlatformRel = "Conv_ExcelToTsv_ODSTemplate_PlatformProductRel";
        private const string Product = "Product";

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
                TypeName = Product,
                FileExtension = "tsv",
            };

            var VM_BreakdownItemWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, Product), _objectCountPerFile)
            {
                FileBaseName = $"VM_BreakdownItem_Product",
                TypeName = "BreakdownItem_Product",
                FileExtension = "tsv",
            };


            var OtisEntityReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, Product));
            var OtisEntities = OtisEntityReader.ReadAllEntities<OTSProductEntity>(OtisProduct);

            var OtisPlatformRelEntityReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, Product));
            var OtisPlatformRelEntities = OtisPlatformRelEntityReader.ReadAllEntities<OtisProductPlatformRelEntity>(OtisPlatformRel);

            var Prod_PlatformMap = OtisPlatformRelEntities.ToDictionary(e => e.Product_Number, e => e.Platform_No);

            var uniqueProductItemEntities = OtisEntities
            .GroupBy(item => item.Product_No)
            .Select(group => group.First())
            .ToList();

            long successCount = 0;

            using (OtisProductWriter)
            {
                using (VM_BreakdownItemWriter)
                {
                    foreach (var OtisEntity in uniqueProductItemEntities)
                    {
                        if (OtisProductWriter.HeaderRow == null)
                        {
                            OtisProductWriter.HeaderRow = "ARAS_UNIQUENESS_HELPER\tID\tCONFIG_ID\tKEYED_NAME\tITEM_NUMBER\tNAME\tMR_MRL\tBelted_Roped\tController\tUnderslung_Overslung\tRoping\tDescription\tCOMPY_Region\tCODE\tDL_Duty_Load" +
                                "\tV_Speed\tR_Rise_Max\tCREATED_BY_ID\tCREATED_ON\tCURRENT_STATE\tGENERATION\tIS_CURRENT\tIS_RELEASED\tMAJOR_REV\tMINOR_REV\tPERMISSION_ID\tSTATE\tOTS_PLATFORM\tCOMMERTIAL_PRODUCT_NAMES\tOTS_PRODUCT_STRUCTURE\tPlatform_No\n";
                        }

                        if (VM_BreakdownItemWriter.HeaderRow == null)
                        {
                            VM_BreakdownItemWriter.HeaderRow = "ARAS_UNIQUENESS_HELPER\tid\tCONFIG_ID\tKEYED_NAME\tITEM_NUMBER\tODS_NAME\tDESCRIPTION\tCLASSIFICATION\tCREATED_BY\tCREATION_DATE\tMODIFIED_BY" +
                                                                        "\tMODIFIED_DATE\tOWNED_BY_ID\tOTS_REVISION\tSTATE\tCURRENT_STATE\tIS_CURRENT\tMINOR_REV\tMAJOR_REV\tIS_RELEASED\tNOT_LOCKABLE\tGENERATION\tNEW_VERSION\tPERMISSION_ID\n";
                        }

                        if (OtisEntity.MR_MRL.Contains("MRL"))
                        {
                            OtisEntity.MR_MRL = "ML";
                        }

                        OtisEntity.ID = TransformerUtils.GetNewArasGuid();
                        var breakDownItemID = TransformerUtils.GetNewArasGuid();

                        OtisEntity.CONFIG_ID = OtisEntity.ID;
                        OtisEntity.KEYED_NAME = OtisEntity.Product_No;
                        OtisEntity.ITEM_NUMBER = OtisEntity.Product_No;
                        OtisEntity.Product_Name = OtisEntity.Product_Name;
                        OtisEntity.CREATED_BY_ID = "Data Migration";
                        OtisEntity.CREATED_ON = DateTime.Now.ToString();
                        OtisEntity.MODIFIED_ON = DateTime.Now.ToString();
                        OtisEntity.Modified_By_ID = "Data Migration";
                        //  OtisEntity.CURRENT_STATE = "B20F5D65D72948668BC04B30F4E58B11";
                        OtisEntity.GENERATION = "1";
                        OtisEntity.IS_CURRENT = "1";
                        OtisEntity.IS_RELEASED = "0";
                        OtisEntity.MAJOR_REV = "A";
                        OtisEntity.MINOR_REV = "1";
                        OtisEntity.PERMISSION_ID = "2382DEC5CAAC42BBA9D27DFD0B0D742C";
                        OtisEntity.STATE = OtisEntity.State;
                        OtisEntity.Controller = Regex.Replace(OtisEntity.Controller, @"\s*,\s*", ",");
                        OtisEntity.OTS_Product_Struct = breakDownItemID;
                        OtisEntity.Ots_Platform = String.Empty;
                        // OtisEntity.Commercial_Product_Names = String.Empty;

                        if (OtisEntity.State == "Released")
                        {
                            //OtisEntity.PERMISSION_ID = "6B1D34D6B1D246DBAE349E157B6B56CC"; //released state permission
                            OtisEntity.CURRENT_STATE = "B20F5D65D72948668BC04B30F4E58B11"; //released state
                            OtisEntity.IS_RELEASED = "1";
                        }
                        else if (OtisEntity.State == "In Work")
                        {
                            // OtisEntity.PERMISSION_ID = "ED7931BF745C4F8799F4EA9000223369"; //in work state permission
                            OtisEntity.CURRENT_STATE = "A3DB599725EC40E398A02CB385294EA4"; //in work
                        }
                        else if (OtisEntity.State == "Under Review")
                        {
                            //OtisEntity.PERMISSION_ID = "C02D558D8B1140689A12681F01762F7B"; //Under Review state permission
                            OtisEntity.CURRENT_STATE = "1E880678D98C491B98721CE6EF8A1614"; //Under Review
                        }
                        else
                        {
                            // OtisEntity.PERMISSION_ID = "ED7931BF745C4F8799F4EA9000223369"; //in work state permission
                            OtisEntity.CURRENT_STATE = "A3DB599725EC40E398A02CB385294EA4"; //in work
                        }
                        var Platform_No = "";
                        if (Prod_PlatformMap.ContainsKey(OtisEntity.Product_No))
                        {
                            Platform_No = Prod_PlatformMap[OtisEntity.Product_No];
                        }

                        OtisProductWriter.WriteRow(OtisEntity.DataRow + $"{Platform_No}\n");


                        //BreakdownItem
                        var ARAS_UNIQUENESS_HELPER = "";
                        var CONFIG_ID = breakDownItemID;
                        var KEYED_NAME = OtisEntity.KEYED_NAME;
                        var Name = OtisEntity.Product_Name;
                        var Item_Number = OtisEntity.ITEM_NUMBER;
                        var Description = OtisEntity.Description;
                        var Creation_Date = DateTime.Now.ToString();
                        var Created_By = "Data Migration";
                        var Modified_Date = DateTime.Now.ToString();
                        var Modified_By = "Data Migration";
                        var Owned_By_id = "DBA5D86402BF43D5976854B8B48FCDD1";
                        var IS_CURRENT = "1";
                        var MAJOR_REV = "A";
                        var Revision = MAJOR_REV + ".1";
                        var MINOR_REV = "1";
                        var IS_RELEASED = "0";
                        var NOT_LOCKABLE = "0";
                        var GENERATION = "1";
                        var NEW_VERSION = "0";
                        var Classification = "Product";
                        var State = "Released";
                        var CURRENT_STATE = "B9CD21B6F6314ACBB9727CFECE9EFEFB";
                        var PERMISSION_ID = "6B1D34D6B1D246DBAE349E157B6B56CC";

                        VM_BreakdownItemWriter.WriteRow($"{ARAS_UNIQUENESS_HELPER}\t{breakDownItemID}\t{CONFIG_ID}\t{KEYED_NAME}\t{Item_Number}\t{Name}\t{Description}\t{Classification}\t{Created_By}\t{Creation_Date}\t{Modified_By}\t" +
                            $"{Modified_Date}\t{Owned_By_id}\t{Revision}\t{State}\t{CURRENT_STATE}\t{IS_CURRENT}\t{MINOR_REV}\t{MAJOR_REV}\t{IS_RELEASED}\t{NOT_LOCKABLE}\t{GENERATION}\t{NEW_VERSION}\t{PERMISSION_ID}\n");
                    }
                    successCount++;
                }
                    
            }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, OtisProduct, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, OtisProduct);
        }
    }
}
