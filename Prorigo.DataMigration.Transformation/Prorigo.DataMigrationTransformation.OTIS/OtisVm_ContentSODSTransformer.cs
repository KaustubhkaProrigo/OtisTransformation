using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using Prorigo.DataMigrationTransformation.OTIS.Entities;

using Prorigo.Plm.DataMigration.Utilities;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    class OtisVm_ContentSODSTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisVm_ContentSODSTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;

        private const string Vm_ContentSODS = "Vm_ContentSODS";


        public OtisVm_ContentSODSTransformer(IConfiguration configuration, ILogger<OtisVm_ContentSODSTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var Vm_ContentSODSSection = _configuration.GetSection("OtisVm_ContentSODS");
            _processAreaDataPath = Vm_ContentSODSSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = Vm_ContentSODSSection.GetValue<long>("ObjectCountPerFile");
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
                TransformFiles(transformName, Vm_ContentSODS);
            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }

        private void TransformFiles(string typeName, string transformName)
        {
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, typeName);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.InProgress);

            var ProductDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "Product"));
            var ProductEntities = ProductDataReader.ReadAllEntities<OtisBreakdownProductEntity>("BreakdownItem_Product", "*.tsv");

            var ProductODSDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath,"BOM Files"));
            var ProductODSEntities = ProductODSDataReader.ReadAllEntities<Product_ODSEntity>("SODS", "*.TSV");

            var ODSDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "ODS"));
            var ODSEntities = ODSDataReader.ReadAllEntities<ODSEntity>("BreakdownItem_ODS","*.tsv");

            var SODS_ODSDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "SODS"), _objectCountPerFile)
            {
                FileBaseName = $"TR_Vm_ContentSODS",
                TypeName = Vm_ContentSODS,
                FileExtension = "tsv"
            };
            var failedFilesDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "SODS" , Vm_ContentSODS), _objectCountPerFile)
            {
                FileBaseName = $"TR_Failed_Vm_ContentSODS",
                TypeName = $"TR_Failed_Vm_ContentSODS_MetaData",
                HeaderRow = "ProductNumber\tODSNumber\tErrorDescription\n",
                FileExtension = "tsv"
            };

            var Product = ProductEntities.ToDictionary(e => e.ITEM_NUMBER, e => e.ID);
            var ODS = ODSEntities.ToDictionary(e => e.ITEM_NUMBER, e => e.ID);

            int objectCount = 0;
            using (failedFilesDataFileWriter)
            {
                using (SODS_ODSDataFileWriter)
                {
                    if (SODS_ODSDataFileWriter.HeaderRow == null)
                        SODS_ODSDataFileWriter.HeaderRow = "ConnectionId\tCONFIG_ID\tKEYED_NAME\tCREATED_ON\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tIS_CURRENT\tMAJOR_REV\tSTATE\tIS_RELEASED\tNOT_LOCKABLE\tGENERATION\tPERMISSION_ID\tSOURCE_ID\tQUANTITY\tRELATED_ITEMTYPE\tRELATED_ID\tExpressionID\n";

                    var groupedByID = ProductODSEntities
                        .Where(e => string.IsNullOrEmpty(e.ODS_No))
                        .GroupBy(e => e.ID?.Trim() ?? string.Empty)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    var firstRowObjectsToSkip = new HashSet<Product_ODSEntity>(
                        groupedByID.Values.Select(g => g[0])
                    );

                    foreach (var ProductODSEntity in ProductODSEntities)
                    {
                        var RelatedID = "";
                        var SourceID = "";

                        var SODSNO = ProductODSEntity.SODSNO;
                        var ODSNO = ProductODSEntity.ODS_No;//RelatedID
                        string ProductNO = SODSNO.Split('-')[0];//SourceID

                        if (string.IsNullOrEmpty(ProductODSEntity.ODS_No))
                        {
                            var parts = ProductODSEntity.Condition.Split('|');
                            var tempID = parts.Length > 0 ? parts[parts.Length - 1].Trim() : string.Empty;

                            if (firstRowObjectsToSkip.Any(e => e.Condition == ProductODSEntity.Condition))
                            {
                                continue;
                            }

                            if (ODS.ContainsKey(tempID) && Product.ContainsKey(ProductNO))
                            {
                                RelatedID = ODS[tempID];
                                SourceID = Product[ProductNO];
                            }
                            else
                            {
                                if (!ODS.ContainsKey(tempID))
                                {
                                    if (!Product.ContainsKey(ProductNO))
                                    {
                                        failedFilesDataFileWriter.WriteRow($"{ProductNO}\t{tempID}\tMissing Product And ODS\n");
                                        continue;
                                    }
                                    failedFilesDataFileWriter.WriteRow($"{ProductNO}\t{tempID}\tMissing ODS\n");
                                    continue;
                                }
                                else
                                {
                                    failedFilesDataFileWriter.WriteRow($"{ProductNO}\t{tempID}\tMissing Product\n");
                                    continue;
                                }

                            }
                           
                        }
                        else if (ODS.ContainsKey(ODSNO) && Product.ContainsKey(ProductNO) && !string.IsNullOrEmpty(ProductODSEntity.ODS_No))
                        {
                            RelatedID = ODS[ODSNO];
                            SourceID = Product[ProductNO];
                        }
                        else
                        {
                            if (!ODS.ContainsKey(ODSNO))
                            {
                                if (!Product.ContainsKey(ProductNO))
                                {
                                    failedFilesDataFileWriter.WriteRow($"{ProductNO}\t{ODSNO}\tMissing Product And ODS\n");
                                    continue;
                                }
                                failedFilesDataFileWriter.WriteRow($"{ProductNO}\t{ODSNO}\tMissing ODS\n");
                                continue;
                            }
                            else
                            {
                                failedFilesDataFileWriter.WriteRow($"{ProductNO}\t{ODSNO}\tMissing Product\n");
                                continue;
                            }
                        }

                        var ConnectionId = TransformerUtils.GetNewArasGuid();
                        var ConfigId = ConnectionId;
                        var KEYED_NAME = ConnectionId;
                        var PERMISSION_ID = "95475AE006E7415794BDC93808DC04D2";
                        var CREATED_ON = DateTime.Now;
                        var CREATED_BY_ID = "Data Migration";
                        var MODIFIED_ON = DateTime.Now.ToString();
                        var MODIFIED_BY_ID = "Data Migration";
                        var IS_RELEASED = "1";
                        var STATE = "Released";
                        var IS_CURRENT = "1";
                        var MAJOR_REV = "A";
                        var NOT_LOCKABLE = "0";
                        var GENERATION = "1";
                        var RELATED_ITEMTYPE = "A52B00E494304E8DAC9EFB544EAB61A2";
                        var condition = ProductODSEntity.Condition;
                        var QTY = ProductODSEntity.QT.Trim();
                        var ExpressionID = ProductODSEntity.ExpressionID;

                        SODS_ODSDataFileWriter.WriteRow($"{ConnectionId}\t{ConfigId}\t{KEYED_NAME}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{IS_CURRENT}\t{MAJOR_REV}\t{STATE}\t{IS_RELEASED}\t{NOT_LOCKABLE}\t{GENERATION}\t{PERMISSION_ID}\t{SourceID}\t{QTY}\t{RELATED_ITEMTYPE}\t{RelatedID}\t{ExpressionID}\n");
                        objectCount++;
                    }

                    
                }
            }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.Completed, objectCount);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, typeName);
        }
    }
}

