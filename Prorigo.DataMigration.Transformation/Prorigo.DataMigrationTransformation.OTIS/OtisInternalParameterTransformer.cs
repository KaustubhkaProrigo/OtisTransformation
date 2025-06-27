using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Drawing.Diagrams;

//using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.Pivot2023Calculation;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Engineering;
using Prorigo.DataMigrationTransformation.OTIS.Entities;
using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using Prorigo.Plm.DataMigration.Utilities;


//using static ClosedXML.Excel.XLPredefinedFormat;

namespace Prorigo.Plm.DataMigration.OtisDataTransformer
{
    public class OtisInternalParameterTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisInternalParameterTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private string _classification = "Internal";
        private readonly IConfigurationSection _typesConfigSection;

        private const string OtisParameter = "Conv_ExcelToTSV_InternalParameter";
        private const string FilesMetadata = "FilesMetadata";
        private const string ParameterTofileRelationship = "ParameterTofileRelationship";

        public OtisInternalParameterTransformer(IConfiguration configuration, ILogger<OtisInternalParameterTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var OtisParameterSection = _configuration.GetSection("OtisInternalParameter");
            _processAreaDataPath = OtisParameterSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = OtisParameterSection.GetValue<long>("ObjectCountPerFile");
            //_classification = OtisParameterSection.GetSection("classification").Get<string[]>();
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

                //foreach (var classification in _classification)
                //{
                //    DefaultValueAdder(transformName, classification);
                //}

                DefaultValueAdder(transformName, _classification);
            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }


            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }

        public void DefaultValueAdder(string transformName, string classification)
        {
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, OtisParameter);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, OtisParameter, TransformStatus.InProgress);

            var OtisItemTypeWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"{classification}_Otis_InternalParameter",
                TypeName = "Parameter",
                FileExtension = "tsv",
            };

            var OtisRelationshipWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "Parameter"), _objectCountPerFile)
            {
                FileBaseName = $"{classification}_OtisInternalParameterValue",
                TypeName = "ParameterValue",
                FileExtension = "tsv",
            };

            var ParameterToFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "Parameter"), _objectCountPerFile)
            {
                FileBaseName = $"{classification}_InternalParameterTofileRelationship",
                TypeName = "ParameterTofileRelationship",
                FileExtension = "tsv",
            };

            var OtisEntityReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "Internal_Output_Parameter"));
            var OtisEntities = OtisEntityReader.ReadAllEntities<OtisInternalParameterEntity>(OtisParameter);

            var OtisFileEntityReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "Parameter"));
            var OtisFileEntities = OtisFileEntityReader.ReadAllEntities<OtisFileMetaEntity>("FileMetadata");

            //var OtisParameterToFileRelReader = new TypeDataFileReader(_processAreaDataPath);
            //var OtisParameterToFileRelEntities = OtisParameterToFileRelReader.ReadAllEntities<ParameterToFileRelEntity>("ParameterTofileRelationship");


            var OtisGroups = OtisEntities
                               .GroupBy(entity => new { entity.Parameter })
                               .ToList();

            Dictionary<string, string> FileNameToIDMap = new Dictionary<string, string>();

            foreach (var FileEntity in OtisFileEntities)
            {
                var FileName = FileEntity.FileName.Contains("_") ? FileEntity.FileName.Substring(0, FileEntity.FileName.IndexOf("_")) : FileEntity.FileName;

                if (!FileNameToIDMap.ContainsKey(FileName))
                {
                    FileNameToIDMap[FileName] = FileEntity.FileId;
                }
            }
            HashSet<(string sourceId, string relatedId)> processedRelationship = new HashSet<(string, string)>();
            long successCount = 0;


                        using (OtisRelationshipWriter)
                        {
                            using (ParameterToFileWriter)
                            {
                                using (OtisItemTypeWriter)
                                {
                                    foreach (var OtisGroup in OtisGroups)
                                    {
                                        if (OtisItemTypeWriter.HeaderRow == null)
                                        {
                                            OtisItemTypeWriter.HeaderRow = "id\tconfig_id\tots_name\tkeyed_name\titem_number\tots_description\tots_functional_description\tclassification\tots_parameter_type\tots_uom\tots_family\tcreated_on\tcreated_by_id\tcurrent_state\tpermission_id\tgeneration\tis_current\tis_released\tmajor_rev\tminor_rev\tstate\tots_is3dparameter\tots_image\tots_requested_region\n";
                                        }

                                        if (string.IsNullOrWhiteSpace(OtisGroup.Key.Parameter))
                                        {
                                            continue;  // Skip if Value is blank or null
                                        }

                                        var ID = TransformerUtils.GetNewArasGuid();
                                        var CONFIG_ID = ID;
                                        var Parameter_Name = OtisGroup.Key.Parameter;
                                        var Keyed_Name = Parameter_Name;
                                        var Item_Number = Parameter_Name;
                                        var firstEntity = OtisGroup.First();
                                        var DESCRIPTION = firstEntity.Parameter_Description;
                                        var Functional_Description = firstEntity.Function_Application;
                                        var Classification = classification;
                                        var ots_parameter_type = firstEntity.Data_Type;
                                        var UOM = firstEntity.UOM;
                                        var ots_image = FileNameToIDMap.ContainsKey(Parameter_Name) ? "vault:///?fileId=" + FileNameToIDMap[Parameter_Name] : string.Empty;

                                        if (UOM.Contains("-"))
                                        {
                                            UOM = UOM.Substring(0, UOM.IndexOf('-'));
                                        }

                                        var Family = firstEntity.Family;

                                        if (Family.Contains("-"))
                                        {
                                            Family = Family.Substring(Family.IndexOf('-') + 1);
                                        }

                                        var CREATED_ON = DateTime.Now.ToString();
                                        var CREATED_BY_ID = "Data Migration";
                                        var CURRENT_STATE = "AA14A064E54A4ED99C82CF71CCCDCFFD";
                                        var PERMISSION_ID = "9122CD065CF04141B8EFE263FC80BEA4";
                                        var GENERATION = 1;
                                        var IS_CURRENT = 1;
                                        var IS_RELEASED = 1;
                                        var MAJOR_REV = "A";
                                        var MINOR_REV = 1;
                                        var STATE = "Released";
                                        var ot_is_3d_Param = (ots_parameter_type.Contains("3DText") || ots_parameter_type.Contains("3DNumber") || ots_parameter_type.Contains("3DYES/NO")) ? "Yes" : "No";
                                        var requested_region = "";

                                        if (ots_parameter_type.Contains("Yes/No", StringComparison.OrdinalIgnoreCase))
                                        {
                                            ots_parameter_type = "Boolean";
                                        }
                                        else if (ots_parameter_type.Contains("Number", StringComparison.OrdinalIgnoreCase))
                                        {
                                            ots_parameter_type = "Number";
                                        }
                                        else if
                                        (ots_parameter_type.Contains("Text", StringComparison.OrdinalIgnoreCase))
                                        {
                                            ots_parameter_type = "Text";
                                        }

                                        OtisItemTypeWriter.WriteRow($"{ID}\t{CONFIG_ID}\t{Parameter_Name}\t{Keyed_Name}\t{Item_Number}\t{DESCRIPTION}\t{Functional_Description}\t{Classification}\t{ots_parameter_type}\t{UOM}\t{Family}\t{CREATED_ON}\t{CREATED_BY_ID}\t{CURRENT_STATE}\t{PERMISSION_ID}\t{GENERATION}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{MINOR_REV}\t{STATE}\t{ot_is_3d_Param}\t{ots_image}\t{requested_region}\n");

                                        if (!string.IsNullOrEmpty(ots_image))
                                        {
                                            foreach (var ParamFile in OtisGroup)
                                            {


                                                if (ParameterToFileWriter.HeaderRow == null)
                                                {
                                                    ParameterToFileWriter.HeaderRow = "connectionId\tsourceId\trelatedId\tconfigId\tCreatedOn\tCreated_By_Id\tModified_On\tIs_Current\tMajor_Rev\tIs_Released\tNot_Lockable\tGeneration\tNew_version\tPermission_Id\tBehaviour\n";
                                                }

                                                var connectionId = TransformerUtils.GetNewArasGuid();
                                                var sourceId = ID;
                                                var relatedId = FileNameToIDMap[Parameter_Name];
                                                if (processedRelationship.Contains((sourceId, relatedId)))
                                                {
                                                    continue;
                                                }
                                                processedRelationship.Add((sourceId, relatedId));

                                                var configId = connectionId;
                                                var CreatedOn = DateTime.Now.ToString();
                                                var Created_By_Id = "Data Migration";
                                                var Modified_On = DateTime.Now.ToString();
                                                var Is_Current = "1";
                                                var Major_Rev = "A";
                                                var Is_Released = "0";
                                                var Not_Lockable = "0";
                                                var Generation = "1";
                                                var New_version = "1";
                                                var Permission_Id = "9122CD065CF04141B8EFE263FC80BEA4";
                                                var Behaviour = "float";

                                                ParameterToFileWriter.WriteRow($"{connectionId}\t{sourceId}\t{relatedId}\t{configId}\t{CreatedOn}\t{Created_By_Id}\t{Modified_On}\t{Is_Current}\t{Major_Rev}\t{Is_Released}\t{Not_Lockable}\t{Generation}\t{New_version}\t{Permission_Id}\t{Behaviour}\n");

                                            }

                                        }
                                        foreach (var row in OtisGroup)
                                        {
                                            if (OtisRelationshipWriter.HeaderRow == null)
                                            {
                                                OtisRelationshipWriter.HeaderRow = "connection_id\tsource_id\tots_value\tots_legacy_id\tots_valueDescription\tcreated_on\tcreated_by_id\tconfig_id\tpermission_id\tis_released\tnot_lockable\tis_current\tmajor_rev\tgeneration\tbehavior\tots_released_date\tots_to_effective_date\n";
                                            }

                                            if (string.IsNullOrWhiteSpace(row.Value))
                                            {
                                                continue;  // Skip if Value is blank or null
                                            }

                                            var ConnectionID = TransformerUtils.GetNewArasGuid();
                                            var SourceID = ID;
                                            var Value = row.Value;
                                            var Value_Number = row.Value_Number;
                                            var Description = row.Value_Description;
                                            var Created_On = DateTime.Now.ToString();
                                            var Created_By_Id = "Data Migration";
                                            var Config_Id = ConnectionID;
                                            var Permission_Id = "9122CD065CF04141B8EFE263FC80BEA4";
                                            var is_released = 1;
                                            var not_lockable = 0;
                                            var is_current = 1;
                                            var major_rev = "A";
                                            var generation = 1;
                                            var behavior = "float";
                                            var ots_released_date = DateTime.Now.ToString();
                                            var ots_to_effective_date = "12/31/2099";


                                            OtisRelationshipWriter.WriteRow($"{ConnectionID}\t{SourceID}\t{Value}\t{Value_Number}\t{Description}\t{Created_On}\t{Created_By_Id}\t{Config_Id}\t{Permission_Id}\t{is_released}\t{not_lockable}\t{is_current}\t{major_rev}\t{generation}\t{behavior}\t{ots_released_date}\t{ots_to_effective_date}\n");
                                        }

                                        successCount++;
                                    }
                                }
                            }
                        }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, OtisParameter, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, OtisParameter);
        }
    }
}