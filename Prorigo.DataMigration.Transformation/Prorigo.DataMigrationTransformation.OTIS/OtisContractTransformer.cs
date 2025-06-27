using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prorigo.DataMigrationTransformation.OTIS.Entities;
using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using Prorigo.Plm.DataMigration.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    public class OtisContractTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisContractTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;
        private const string OtisContractFolder = "Contract";
        private const string OtisProductFolder = "Product";
        private const string OtisContract = "Conv_ExcelToTsv_Contract_Main";
        private const string OtisBranchPlant = "BranchPlant";

        public OtisContractTransformer(IConfiguration configuration, ILogger<OtisContractTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var ValidationSection = _configuration.GetSection("OtisContract");
            _processAreaDataPath = ValidationSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = ValidationSection.GetValue<long>("ObjectCountPerFile");
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
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, OtisContract);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, OtisContract, TransformStatus.InProgress);

            var ContractItemWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"Contract",
                TypeName = OtisContractFolder,
                FileExtension = "tsv",
            };

            //var Contract_Product_RelWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, OtisContractFolder, "Contract_Product_Rel"), _objectCountPerFile)
            //{
            //    FileBaseName = $"Contract_Product_Items",
            //    TypeName = "Contract_Product_Items",
            //    FileExtension = "tsv",
            //};


            var ContractItemReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, OtisContractFolder));
            var ContractItemEntities = ContractItemReader.ReadAllEntities<OtisContractEntity>(OtisContract);

            var ProductItemReader = new TypeDataFileReader(_processAreaDataPath);
            var ProductItemEntities = ProductItemReader.ReadAllEntities<OtisProductEntity>(OtisProductFolder, "*.tsv");

            var ProductIDMap = ProductItemEntities.ToDictionary(e => e.Item_Number, e => e.ID); //OtisBranchPlantTSVEntity

            var BranchPlantItemReader = new TypeDataFileReader(_processAreaDataPath);
            var BranchPlantItemEntities = BranchPlantItemReader.ReadAllEntities<OtisBranchPlantTSVEntity>(OtisBranchPlant,"*.tsv");

            var BranchPlantIDMap = BranchPlantItemEntities.ToDictionary(e => e.Name, e => e.id); //OtisBranchPlantTSVEntity

            //var Contract_ProdReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, OtisContractFolder));
            //var Contract_ProdEntities = Contract_ProdReader.ReadAllEntities<OtisContractProductRelEntity>(OtisContract_Prod_Rel);

            //var Contract_ProdMap = Contract_ProdEntities.ToDictionary(e => e.Contract_Number, e => e.Product_Number);

            var uniqueContractItemEntities = ContractItemEntities
            .GroupBy(item => item.Contract_Number)
            .Select(group => group.First())
            .ToList();

            //Dictionary<string, string> ContractID = new Dictionary<string, string>();

            long successCount = 0;

            using (ContractItemWriter)
            {
                foreach (var ContractItemEntity in uniqueContractItemEntities)
                {
                    if (ContractItemWriter.HeaderRow == null)
                    {
                        ContractItemWriter.HeaderRow = "ARAS_UNIQUENESS_HELPER\tID\tCONFIG_ID\tKEYED_NAME\tCREATED_BY_ID\tCREATED_ON\tMODIFIED_BY_ID\tMODIFIED_ON\tOTS_BRANCH_PLANT\tOTS_BW_EQUIPMENT_CODE\tOTS_CITY\tOTS_NUMBER\tOTS_PDD\tOTS_PRODUCT\tOTS_PROGRAM_NUM\tOTS_PROJECT_NAME\t" +
                            "OTS_TYPE\tPERMISSION_ID\tCURRENT_STATE\tSTATE\tIS_CURRENT\tMAJOR_REV\tMINOR_REV\tIS_RELEASED\tNOT_LOCKABLE\tGENERATION\tNEW_VERSION\tOTS_REVISION\tOTS_PROJECT_NUMBER\tOTS_REGION\n";
                    }

                    
                    ContractItemEntity.ID = TransformerUtils.GetNewArasGuid();
                    ContractItemEntity.KEYED_NAME = ContractItemEntity.Contract_Number;
                    ContractItemEntity.CONFIG_ID = ContractItemEntity.ID;
                    ContractItemEntity.ARAS_UNIQUENESS_HELPER = "";
                    ContractItemEntity.OTS_PRODUCT = String.Empty;

                    if (ProductIDMap.ContainsKey(ContractItemEntity.Equipment_Code))
                    {
                        ContractItemEntity.OTS_PRODUCT = ProductIDMap[ContractItemEntity.Equipment_Code];
                    }

                    if (BranchPlantIDMap.ContainsKey(ContractItemEntity.Branch_Plant))
                    {
                        ContractItemEntity.Branch_Plant = BranchPlantIDMap[ContractItemEntity.Branch_Plant];
                    }

                    ContractItemEntity.CREATED_BY_ID = "Data Migration";
                    ContractItemEntity.CREATED_ON = DateTime.Now.ToString();
                    ContractItemEntity.MODIFIED_BY_ID = "Data Migration";
                    ContractItemEntity.MODIFIED_ON = DateTime.Now.ToString();
                    ContractItemEntity.OTS_Revision = "A.1";
                    ContractItemEntity.STATE = "Released";
                    ContractItemEntity.CURRENT_STATE = "197D11BE0BEC48FB89EF7CC884853712";
                    ContractItemEntity.PERMISSION_ID = "B2DC93A4B3004EEE8F5884AF4E187B4B";
                    ContractItemEntity.IS_CURRENT = "1";
                    ContractItemEntity.IS_RELEASED = "1";
                    ContractItemEntity.MINOR_REV = "1";
                    ContractItemEntity.NOT_LOCKABLE = "0";
                    ContractItemEntity.NEW_VERSION = "0";
                    ContractItemEntity.GENERATION = "1";
                    ContractItemEntity.MAJOR_REV = "A";

                    //if (!ContractID.ContainsKey(ContractItemEntity.Contract_Number))
                    //{
                    //    ContractID[ContractItemEntity.Contract_Number] = ContractItemEntity.ID;
                    //}
                    
                    ContractItemWriter.WriteRow(ContractItemEntity.DataRow);

                }
                successCount++;
            }

            //using (Contract_Product_RelWriter)
            //{
            //    if (Contract_Product_RelWriter.HeaderRow == null)
            //        Contract_Product_RelWriter.HeaderRow = "connectionID\tSourceID\tRelatedID\tIS_CURRENT\tIS_RELEASED\tPermission_Id\tNOT_LOCKABLE\tGeneration\n";


            //    foreach (var RelItem in Contract_ProdEntities)
            //    {
                    
            //        var contractNumber = RelItem.Contract_Number;
            //        if (ContractID.ContainsKey(contractNumber))
            //        {
            //            var connectionId = TransformerUtils.GetNewArasGuid();
            //            var sourceId = ContractID[RelItem.Contract_Number];
            //            var relatedId = ProductIDMap[RelItem.Product];
            //            var is_Current = "1";
            //            var is_related = "1";
            //            var ParameterId = "";
            //            var Not_Lockable = "1";
            //            var Generation = "1";

            //            Contract_Product_RelWriter.WriteRow($"{connectionId}\t{sourceId}\t{relatedId}\t{is_Current}\t{is_related}\t{ParameterId}\t{Not_Lockable}\t{Generation}\n");

            //        }
                    
            //    }

            //}

        }
    }
}
