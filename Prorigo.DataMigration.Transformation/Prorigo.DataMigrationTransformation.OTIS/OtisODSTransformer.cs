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
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;


namespace Prorigo.DataMigrationTransformation.OTIS
{
    public class OtisODSTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisODSTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;
        private const string OtisODS = "Conv_ExcelToTsv_ODSTemplate_ODSData";
        private const string ODS = "ODS";



        public OtisODSTransformer(IConfiguration configuration, ILogger<OtisODSTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var ValidationSection = _configuration.GetSection("OtisODS");
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
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, OtisODS);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, OtisODS, TransformStatus.InProgress);

            var ODSItemWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, ODS), _objectCountPerFile)
            {
                FileBaseName = $"VM_BreakdownItem_ODS",
                TypeName = "BreakdownItem_ODS",
                FileExtension = "tsv",
            };

            var CalSheetItemWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, ODS), _objectCountPerFile)
            {
                FileBaseName = $"CalSheet_ODS",
                TypeName = "CalSheet_ODS",
                FileExtension = "tsv",
            };

            var ODS_CalSheetItemRelWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, ODS), _objectCountPerFile)
            {
                FileBaseName = $"ODS_To_Cal_relationship",
                TypeName = "ODS_To_Cal_relationship",
                FileExtension = "tsv",
            };
            var ODSItemReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath,ODS));
            var ODSItemEntities = ODSItemReader.ReadAllEntities<OtisODSEntity>(OtisODS, "*.tsv");

            var uniqueODSItemEntities = ODSItemEntities
            .GroupBy(item => item.ODS_Number)     
            .Select(group => group.First())        
            .ToList();


            long successCount = 0;

            using (ODS_CalSheetItemRelWriter)
            {
                using (CalSheetItemWriter)
                {
                    using (ODSItemWriter)
                    {
                        foreach (var ODSItemEntity in uniqueODSItemEntities)
                        {
                            if (ODSItemWriter.HeaderRow == null)
                            {
                                ODSItemWriter.HeaderRow = "ARAS_UNIQUENESS_HELPER\tid\tCONFIG_ID\tKEYED_NAME\tITEM_NUMBER\tODS_NAME\tDESCRIPTION\tCLASSIFICATION\tCREATED_BY\tCREATION_DATE\tMODIFIED_BY" +
                                                                    "\tMODIFIED_DATE\tOWNED_BY_ID\tOTS_REVISION\tSTATE\tCURRENT_STATE\tIS_CURRENT\tMINOR_REV\tMAJOR_REV\tIS_RELEASED\tNOT_LOCKABLE\tGENERATION\tNEW_VERSION\tPERMISSION_ID\tOTS_CALCULATION_SHEET\n";
                            }

                            if (CalSheetItemWriter.HeaderRow == null)
                            {
                                CalSheetItemWriter.HeaderRow = "ARAS_UNIQUENESS_HELPER\tid\tCONFIG_ID\tKEYED_NAME\tNUMBER\tNAME\tDESCRIPTION\tCLASSIFICATION\tCREATED_BY\tCREATION_DATE\tMODIFIED_BY" +
                                                                    "\tMODIFIED_DATE\tOTS_REVISION\tSTATE\tCURRENT_STATE\tIS_CURRENT\tMINOR_REV\tMAJOR_REV\tIS_RELEASED\tNOT_LOCKABLE\tGENERATION\tNEW_VERSION\tPERMISSION_ID\tOTS_PARENT_ITEM\n";
                            }

                            if (ODS_CalSheetItemRelWriter.HeaderRow == null)
                            {
                                ODS_CalSheetItemRelWriter.HeaderRow = "CONNECTION_ID\tCONFIG_ID\tKEYED_NAME\tSOURCE_ID\tRELATED_ID\tCREATED_BY\tCREATION_DATE\tMODIFIED_BY\tMODIFIED_DATE\tSTATE\tCURRENT_STATE\tIS_CURRENT\tMINOR_REV\tMAJOR_REV\tIS_RELEASED\tNOT_LOCKABLE\tGENERATION\tNEW_VERSION\tPERMISSION_ID\n";
                            }

                            ODSItemEntity.id = TransformerUtils.GetNewArasGuid();
                            var ODSCalItemId = TransformerUtils.GetNewArasGuid();

                            ODSItemEntity.ARAS_UNIQUENESS_HELPER = "";
                            ODSItemEntity.CONFIG_ID = ODSItemEntity.id;
                            ODSItemEntity.KEYED_NAME = ODSItemEntity.ODS_Number;
                            ODSItemEntity.Description = ODSItemEntity.Description;
                            ODSItemEntity.Creation_Date = DateTime.Now.ToString();
                            ODSItemEntity.Created_By = "Data Migration";
                            ODSItemEntity.Modified_Date = DateTime.Now.ToString();
                            ODSItemEntity.Modified_By = "Data Migration";
                            ODSItemEntity.Owned_By_id = "DBA5D86402BF43D5976854B8B48FCDD1";
                            ODSItemEntity.IS_CURRENT = "1";
                            ODSItemEntity.MAJOR_REV = ODSItemEntity.Revision;
                            ODSItemEntity.Revision = ODSItemEntity.Revision + ".1";
                            ODSItemEntity.MINOR_REV = "1";
                            ODSItemEntity.IS_RELEASED = "0";
                            ODSItemEntity.NOT_LOCKABLE = "0";
                            ODSItemEntity.GENERATION = "1";
                            ODSItemEntity.NEW_VERSION = "0";
                            ODSItemEntity.Ots_Calculation_sheet = ODSCalItemId;


                            if (ODSItemEntity.State == "Released")
                            {
                                ODSItemEntity.PERMISSION_ID = "6B1D34D6B1D246DBAE349E157B6B56CC"; //released state permission
                                ODSItemEntity.CURRENT_STATE = "B9CD21B6F6314ACBB9727CFECE9EFEFB"; //released state
                                ODSItemEntity.IS_RELEASED = "1";
                            }
                            else if (ODSItemEntity.State == "In Work")
                            {
                                ODSItemEntity.PERMISSION_ID = "ED7931BF745C4F8799F4EA9000223369"; //in work state permission
                                ODSItemEntity.CURRENT_STATE = "C0EE6F35D28240CCA50E84867D71CFF3"; //in work
                            }
                            else if (ODSItemEntity.State == "Under Review")
                            {
                                ODSItemEntity.PERMISSION_ID = "C02D558D8B1140689A12681F01762F7B"; //Under Review state permission
                                ODSItemEntity.CURRENT_STATE = "D60E4E49FF1A4AE1B4CA7F3E00F682EB"; //Under Review
                            }
                            else
                            {
                                ODSItemEntity.PERMISSION_ID = "ED7931BF745C4F8799F4EA9000223369"; //in work state permission
                                ODSItemEntity.CURRENT_STATE = "C0EE6F35D28240CCA50E84867D71CFF3"; //in work
                            }

                            ODSItemWriter.WriteRow(ODSItemEntity.DataRow);


                            var ARAS_UNIQUENESS_HELPER = "";
                            var configID = ODSCalItemId;
                            var classification = ODSItemEntity.Classification;
                            var keyedName = ODSItemEntity.ODS_Number;
                            var otsName = ODSItemEntity.ODS_Name;
                            var otsNumber = ODSItemEntity.ODS_Number;
                            var createdOn = ODSItemEntity.Creation_Date;
                            var createdByID = ODSItemEntity.Created_By;
                            var modifiedOn = ODSItemEntity.Modified_Date;
                            var modifiedByID = ODSItemEntity.Modified_By;
                            var otsDescription = ODSItemEntity.Description;
                            var isCurrent = "1";
                            var isReleased = "0";
                            var generation = "1";
                            var newVersion = "1";
                            var notLockable = "0";
                            var majorRev = ODSItemEntity.MAJOR_REV;
                            var otsRevision = ODSItemEntity.Revision;
                            var minorRev = "1";
                            var permissionID = "9122CD065CF04141B8EFE263FC80BEA4";
                            var otsParentitem = ODSItemEntity.id;
                            var currentState = String.Empty;
                            var state = ODSItemEntity.State;

                            if (state == "Released")
                            {
                                //permissionID = "9122CD065CF04141B8EFE263FC80BEA4"; //released state permission
                                currentState = "B9909619FB294E1AB0B8B4FDF58BD282"; //released state
                                ODSItemEntity.IS_RELEASED = "1";
                            }
                            else if (state == "In Work")
                            {
                                //permissionID = "9122CD065CF04141B8EFE263FC80BEA4"; //in work state permission
                                currentState = "E46E93907F144DB894BA8620D7C14149"; //in work
                            }
                            else if (state == "Under Review")
                            {
                                //permissionID = "9122CD065CF04141B8EFE263FC80BEA4"; //Under Review state permission
                                currentState = "827227F537B241CD8C9736859F86C845"; //Under Review
                            }
                            else
                            {
                                //permissionID = "9122CD065CF04141B8EFE263FC80BEA4"; //in work state permission
                                currentState = "B9909619FB294E1AB0B8B4FDF58BD282"; //in work
                            }


                            CalSheetItemWriter.WriteRow($"{ARAS_UNIQUENESS_HELPER}\t{ODSCalItemId}\t{configID}\t{keyedName}\t{otsNumber}\t{otsName}\t{otsDescription}\t{classification}\t{createdByID}\t{createdOn}\t{modifiedByID}\t{modifiedOn}\t{otsRevision}\t{state}\t{currentState}\t{isCurrent}\t{minorRev}\t{majorRev}\t{isReleased}\t{notLockable}\t{generation}\t{newVersion}\t{permissionID}\t{otsParentitem}\n");

                            var connectionId = TransformerUtils.GetNewArasGuid();
                            var config_ID = connectionId;
                            var sourceId = ODSItemEntity.id;
                            var relatedId = ODSCalItemId;
                            var keyed_Name = connectionId;
                            var created_On = ODSItemEntity.Creation_Date;
                            var createdBy_ID = ODSItemEntity.Created_By;
                            var modified_On = ODSItemEntity.Modified_Date;
                            var modified_By_ID = ODSItemEntity.Modified_By;
                            var is_Current = "1";
                            var is_Released = "0";
                            var rel_generation = "1";
                            var new_Version = "1";
                            var not_Lockable = "0";
                            var major_Rev = "A";
                            var minor_Rev = "1";
                            var permission_ID = "ED7931BF745C4F8799F4EA9000223369";
                            var current_State = "";
                            var State = "Released";

                            ODS_CalSheetItemRelWriter.WriteRow($"{connectionId}\t{config_ID}\t{keyed_Name}\t{sourceId}\t{relatedId}\t{createdBy_ID}\t{created_On}\t{modified_By_ID}\t{modified_On}\t{State}\t{current_State}\t{is_Current}\t{minor_Rev}\t{major_Rev}\t{is_Released}\t{not_Lockable}\t{rel_generation}\t{new_Version}\t{permission_ID}\n");
                            successCount++;
                        }
                    }
                }
            }
           

            
            _migrationDiagnostics.LogTransformTypeStatus(transformName, OtisODS, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, OtisODS);
        }
    }
}
