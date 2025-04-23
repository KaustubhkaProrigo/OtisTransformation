using System;
using System.IO;
using OfficeOpenXml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using Prorigo.Plm.DataMigration.Transformer;
using System.Collections.Generic;
using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using Prorigo.DataMigrationTransformation.OTIS.Entities;
using Prorigo.Plm.DataMigration.Utilities;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    class OtisDrawing_ParameterRelationshipTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisDrawing_ParameterRelationshipTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;
        string Drawing = "Drawing";
        string Parameter = "Parameter";
        string Drawing_Parameter = "Drawing_Parameter";
        string cad = "CAD";
        public OtisDrawing_ParameterRelationshipTransformer(IConfiguration configuration, ILogger<OtisDrawing_ParameterRelationshipTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var PartValidationSection = _configuration.GetSection("Drawing_ParameterRelationship");
            _processAreaDataPath = PartValidationSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = PartValidationSection.GetValue<long>("ObjectCountPerFile");

        }
        public void Transform(string LicenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");
            if (isLicenValid)
            {
                DefaultValueAdder(_processAreaDataPath);
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
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, Drawing_Parameter);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, Drawing_Parameter, TransformStatus.InProgress);


            var DrawingWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, Drawing), _objectCountPerFile)
            {
                FileBaseName = "Drawing",
                TypeName = Drawing,
                FileExtension = "tsv",
            };

            var ParameterWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, Parameter), _objectCountPerFile)
            {
                FileBaseName = "Parameter",
                TypeName = Parameter,
                FileExtension = "tsv",
            };

            var DrawingParameterWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = "Drawing_Parameter",
                TypeName = Drawing_Parameter,
                FileExtension = "tsv",
            };

            var DrawingReader = new TypeDataFileReader(_processAreaDataPath);
            var DrawingEntities = DrawingReader.ReadAllEntities<DrawingEntity>(Drawing, "*.tsv");

            var ParameterReader = new TypeDataFileReader(_processAreaDataPath);
            var ParameterEntities = ParameterReader.ReadAllEntities<ParameterEntity>(Parameter, "*.tsv");

            Dictionary<string, string> DrawingName_DrawingID = new Dictionary<string, string>();
            Dictionary<string, List<string>> DrawingName_ParameterID = new Dictionary<string, List<string>>();

            long successCount = 0;

            using (DrawingWriter)
            {
                foreach (var DrawingEntity in DrawingEntities)
                {
                    if (DrawingWriter.HeaderRow == null)
                        DrawingWriter.HeaderRow = "DrawingNumber\tRevision\tClassification\tDescription\tType\tOrigin_CN\tOrigin_Date\tOriginalCreator\tOriginalReviewer\tid\tconfig_id\tKEYED_NAME\tCREATED_ON\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tIS_CURRENT\tMAJOR_REV\tPermission_Id\tNAME\tITEM_NUMBER\tGeneration\n";

                    var id = TransformerUtils.GetNewArasGuid();

                    var ArasDrawingEntity = new ArasDrawingEntity
                    {  
                        ID = id,
                        config_id= id,
                        IS_CURRENT = "1",
                        Generation = "1",
                        KEYED_NAME= DrawingEntity.DrawingNumber,
                        NAME= DrawingEntity.DrawingNumber,
                        ITEM_NUMBER = DrawingEntity.DrawingNumber,
                        DrawingNumber= DrawingEntity.DrawingNumber,
                        Revision= DrawingEntity.Revision,
                        Classification= DrawingEntity.Classification,
                        Type= DrawingEntity.Type,
                        Description = DrawingEntity.Description,
                        Permission_Id = "03599ED7C23C4BE2962F85FDFDAF006B",
                        MAJOR_REV= DrawingEntity.Revision,
                        CREATED_ON = DrawingEntity.Origin_Date,
                        CREATED_BY_ID = DrawingEntity.OriginalCreator,
                        MODIFIED_ON = DrawingEntity.Origin_Date,
                        MODIFIED_BY_ID= DrawingEntity.OriginalCreator,
                        Origin_Date= DrawingEntity.Origin_Date,
                        OriginalCreator=DrawingEntity.OriginalCreator,
                        OriginalReviewer=DrawingEntity.OriginalReviewer,
                        Origin_CN=DrawingEntity.Origin_CN,
                        
                        
                    };
                    DrawingName_DrawingID[ArasDrawingEntity.DrawingNumber] = ArasDrawingEntity.ID;

                    DrawingWriter.WriteRow(ArasDrawingEntity.DataRow);
                    successCount++;

                }
            }

            using (ParameterWriter)
            {
                foreach (var ParameterEntity in ParameterEntities)
                {
                    if (ParameterWriter.HeaderRow == null)
                        ParameterWriter.HeaderRow = "OLDID\tParameter_Name\tParameter_Description\tParameter_Type\tUOM\tValueListOrRange\tDrawingNo\tID\tCLASSIFICATION\tCREATED_ON\tCREATED_BY_ID\tCONFIG_ID\tPERMISSION_ID\tOTS_DESCRIPTION\tOTS_FAMILY\tOTS_FUNCTIONAL_DESCRIPTION\tOTS_IS3DPARAMETER\tOTS_ISPREFERRED\tOTS_PARAMETER_TYPE\tOTS_UOM\tOTS_NAME\n";

                    var id = TransformerUtils.GetNewArasGuid();

                    var ArasParameterEntity = new ArasParameterEntity
                    {

                        ID = id,
                        CONFIG_ID = id,
                        PERMISSION_ID = "03599ED7C23C4BE2962F85FDFDAF006B",
                        CREATED_ON = DateTime.Now.ToString(),
                        CREATED_BY_ID = "DM Migration",
                        DrawingNo = ParameterEntity.DrawingNo,
                        Parameter_Name = ParameterEntity.Parameter_Name,
                        Parameter_Description = ParameterEntity.Parameter_Description,
                        Parameter_Type = ParameterEntity.Parameter_Type,
                        UOM = ParameterEntity.UOM,
                        ValueListOrRange = ParameterEntity.ValueListOrRange,
                        OLDID = ParameterEntity.OLDID,

                    };
                    if (!DrawingName_ParameterID.ContainsKey(ArasParameterEntity.DrawingNo))
                    {
                        DrawingName_ParameterID[ArasParameterEntity.DrawingNo] = new List<string>();
                    }

                    DrawingName_ParameterID[ArasParameterEntity.DrawingNo].Add(ArasParameterEntity.ID);

                    ParameterWriter.WriteRow(ArasParameterEntity.DataRow);
                    successCount++;
                }
            }
            
            using (DrawingParameterWriter)
            {
                foreach (var kvp in DrawingName_ParameterID)
                {
                    if (DrawingParameterWriter.HeaderRow == null)
                        DrawingParameterWriter.HeaderRow = "connectionID\tFromID\tTOID\tBEHAVIOR\tIS_CURRENT\tIS_RELEASED\tPermission_Id\tNOT_LOCKABLE\tGeneration\n";
                    string drawingNo = kvp.Key;
                    List<string> parameterIds = kvp.Value;

                    if (!DrawingName_DrawingID.TryGetValue(drawingNo, out var drawingId))
                        continue; 

                    foreach (var parameterId in parameterIds)
                    {
                        var ArasDrawing_ParameterEntity = new ArasDrawing_ParameterEntity
                        {
                            connectionID = TransformerUtils.GetNewArasGuid(),
                            FromID = drawingId,
                            TOID = parameterId,
                            BEHAVIOR="float",
                            IS_CURRENT="1",
                            IS_RELEASED="0",
                            Permission_Id= "03599ED7C23C4BE2962F85FDFDAF006B",
                            NOT_LOCKABLE="1",
                            Generation="1",
                        };

                        DrawingParameterWriter.WriteRow(ArasDrawing_ParameterEntity.DataRow);
                        successCount++;
                    }
                }
            }





            _migrationDiagnostics.LogTransformTypeStatus(transformName, Drawing, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, Drawing);
        }
    }
}