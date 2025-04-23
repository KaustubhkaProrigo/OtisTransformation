using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.Pivot2023Calculation;
using DocumentFormat.OpenXml.Office2010.Excel;
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
    public class OtisParameterFeatureOptionTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisParameterFeatureOptionTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private string[] _classification;
        private readonly IConfigurationSection _typesConfigSection;

        private const string OtisParameter = "OtisParameter";
        private const string FilesMetadata = "FilesMetadata";
        private const string VMFeature = "VMFeature";
        private const string VMOption = "VMOption";
        private const string VMFeatureOption = "VMFeatureOption";

        private bool IsNumeric(string str)
        {
            return double.TryParse(str, out _);
        }
        public OtisParameterFeatureOptionTransformer(IConfiguration configuration, ILogger<OtisParameterFeatureOptionTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var OtisParameterSection = _configuration.GetSection("OtisParameter");
            _processAreaDataPath = OtisParameterSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = OtisParameterSection.GetValue<long>("ObjectCountPerFile");
            _classification = OtisParameterSection.GetSection("classification").Get<string[]>();
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

                foreach (var classification in _classification)
                {
                    DefaultValueAdder(transformName, classification);
                }
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
                FileBaseName = $"{classification}_Otis_Parameter",
                TypeName = "Parameter",
                FileExtension = "tsv",
            };

            var OtisRelationshipWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "Parameter"), _objectCountPerFile)
            {
                FileBaseName = $"{classification}_OtisParameterValue",
                TypeName = "ParameterValue",
                FileExtension = "tsv",
            };

            var VMFeatureWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"{classification}_VMFeature",
                TypeName = "VMFeature",
                FileExtension = "tsv",
            };

            var VMOptionWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"{classification}_VMOption",
                TypeName = "VMOption",
                FileExtension = "tsv",
            };

            var VMFeatureOptionWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"{classification}_VMFeatureOption",
                TypeName = "VMFeatureOption",
                FileExtension = "tsv",
            };

            var OtisEntityReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, OtisParameter));
            var OtisEntities = OtisEntityReader.ReadAllEntities<OtisParameterEntity>(classification);

            var OtisFileEntityReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, FilesMetadata));
            var OtisFileEntities = OtisFileEntityReader.ReadAllEntities<OtisFileMetaEntity>("FileMetadata");


            HashSet<string> uniqueVMFeatures = new HashSet<string>();
            HashSet<string> uniqueVMOptions = new HashSet<string>();

            Dictionary<string, string> vmFeatureIdMap = new Dictionary<string, string>(); 
            Dictionary<string, string> vmOptionIdMap = new Dictionary<string, string>();

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
           
            long successCount = 0;

            using (VMFeatureWriter)
            {
                using (VMOptionWriter)
                {
                    using (VMFeatureOptionWriter)
                    {
                        using (OtisRelationshipWriter)
                        {
                            using (OtisItemTypeWriter)
                            {
                                foreach (var OtisGroup in OtisGroups)
                                {
                                    if (OtisItemTypeWriter.HeaderRow == null)
                                    {
                                        OtisItemTypeWriter.HeaderRow = "id\tconfig_id\tots_name\tkeyed_name\titem_number\tots_description\tots_functional_description\tclassification\tots_parameter_type\tots_uom\tots_family\tcreated_on\tcreated_by_id\tcurrent_state\tpermission_id\tgeneration\tis_current\tis_released\tmajor_rev\tminor_rev\tstate\tots_is3dparameter\tots_image\n";
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
                                    var ots_parameter_type = firstEntity.DataType;
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
                                    var CURRENT_STATE = "7490B025D45B44B3930DA13A58585215";
                                    var PERMISSION_ID = "9122CD065CF04141B8EFE263FC80BEA4";
                                    var GENERATION = 1;
                                    var IS_CURRENT = 1;
                                    var IS_RELEASED = 1;
                                    var MAJOR_REV = "A";
                                    var MINOR_REV = 1;
                                    var STATE = "Released";
                                    var ot_is_3d_Param = (ots_parameter_type.Contains("3DText") || ots_parameter_type.Contains("3DNumber") || ots_parameter_type.Contains("3DYES/NO")) ? "Yes" : "No";


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


                                    OtisItemTypeWriter.WriteRow($"{ID}\t{CONFIG_ID}\t{Parameter_Name}\t{Keyed_Name}\t{Item_Number}\t{DESCRIPTION}\t{Functional_Description}\t{Classification}\t{ots_parameter_type}\t{UOM}\t{Family}\t{CREATED_ON}\t{CREATED_BY_ID}\t{CURRENT_STATE}\t{PERMISSION_ID}\t{GENERATION}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{MINOR_REV}\t{STATE}\t{ot_is_3d_Param}\t{ots_image}\n");


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

                                    foreach (var features in OtisGroup)
                                    {
                                        if (VMFeatureWriter.HeaderRow == null)
                                        {
                                            VMFeatureWriter.HeaderRow = "id\tkeyed_name\titem_number\tCreated_On\tCreated_By_Id\tConfig_Id\tPermission_Id\tis_released\tnot_lockable\tis_current\tmajor_rev\tgeneration\n";
                                        }
                                       
                                        if (!uniqueVMFeatures.Contains(features.Parameter))
                                        {
                                            uniqueVMFeatures.Add(features.Parameter);

                                            var id = TransformerUtils.GetNewArasGuid();
                                            var keyed_name = features.Parameter;
                                            var item_number = features.Parameter;
                                            var Created_On = DateTime.Now.ToString();
                                            var Created_By_Id = "Data Migration";
                                            var Config_Id = id;
                                            var Permission_Id = "9122CD065CF04141B8EFE263FC80BEA4";
                                            var is_released = 1;
                                            var not_lockable = 0;
                                            var is_current = 1;
                                            var major_rev = "A";
                                            var generation = 1;
                                            vmFeatureIdMap[keyed_name] = id;
                                         
                                            VMFeatureWriter.WriteRow($"{id}\t{keyed_name}\t{item_number}\t{Created_On}\t{Created_By_Id}\t{Config_Id}\t{Permission_Id}\t{is_released}\t{not_lockable}\t{is_current}\t{major_rev}\t{generation}\n");
                                        }
                                    }


                                    foreach (var options in OtisGroup)
                                    {
                                        if (VMOptionWriter.HeaderRow == null)
                                        {
                                            VMOptionWriter.HeaderRow = "id\tkeyed_name\titem_number\tCreated_On\tCreated_By_Id\tConfig_Id\tPermission_Id\tis_released\tnot_lockable\tis_current\tmajor_rev\tgeneration\n";
                                        }

                                        if (!uniqueVMOptions.Contains(options.Value))
                                        {
                                            uniqueVMOptions.Add(options.Value);

                                            var id = TransformerUtils.GetNewArasGuid();
                                            var keyed_name = options.Value;

                                            if (string.IsNullOrWhiteSpace(keyed_name) || IsNumeric(keyed_name))
                                            {
                                                continue;
                                            }

                                            uniqueVMOptions.Add(keyed_name);

                                            var item_number = options.Value;
                                            var Created_On = DateTime.Now.ToString();
                                            var Created_By_Id = "Data Migration";
                                            var Config_Id = id;
                                            var Permission_Id = "9122CD065CF04141B8EFE263FC80BEA4";
                                            var is_released = 1;
                                            var not_lockable = 0;
                                            var is_current = 1;
                                            var major_rev = "A";
                                            var generation = 1;

                                            vmOptionIdMap[keyed_name] = id;
                                            VMOptionWriter.WriteRow($"{id}\t{keyed_name}\t{item_number}\t{Created_On}\t{Created_By_Id}\t{Config_Id}\t{Permission_Id}\t{is_released}\t{not_lockable}\t{is_current}\t{major_rev}\t{generation}\n");
                                        }
                                    }
                                
                                    foreach (var featureoptions in OtisGroup)
                                    {
                                        if (VMFeatureOptionWriter.HeaderRow == null)
                                        {
                                            VMFeatureOptionWriter.HeaderRow = "connection_id\tsource_id\trelated_id\tsource_name\trelated_name\tCreated_On\tCreated_By_Id\tConfig_Id\tPermission_Id\tbehavior\n";
                                        }

                                        var connection_id = TransformerUtils.GetNewArasGuid();                                       
                                        var source_id = vmFeatureIdMap.ContainsKey(featureoptions.Parameter) ? vmFeatureIdMap[featureoptions.Parameter] : string.Empty;
                                        var related_id = vmOptionIdMap.ContainsKey(featureoptions.Value) ? vmOptionIdMap[featureoptions.Value] : string.Empty;

                                        var source_name = featureoptions.Parameter;
                                        var related_name = featureoptions.Value;


                                        if (string.IsNullOrWhiteSpace(related_name) || IsNumeric(related_name))
                                        {
                                            continue;
                                        }

                                        var Created_On = DateTime.Now.ToString();
                                        var Created_By_Id = "Data Migration";
                                        var Config_Id = connection_id;
                                        var Permission_Id = "9122CD065CF04141B8EFE263FC80BEA4";
                                        var behavior = "float";

                                        VMFeatureOptionWriter.WriteRow($"{connection_id}\t{source_id}\t{related_id}\t{source_name}\t{related_name}\t{Created_On}\t{Created_By_Id}\t{Config_Id}\t{Permission_Id}\t{behavior}\n");
                                    }

                                    successCount++;
                                }
                            }
                        }
                    }
                }
            }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, OtisParameter, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, OtisParameter);
        }
    }
}