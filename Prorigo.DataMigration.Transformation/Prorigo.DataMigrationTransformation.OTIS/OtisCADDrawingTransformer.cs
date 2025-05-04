using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    public class OtisCADDrawingTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisCADDrawingTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private string _classification;
        private readonly IConfigurationSection _typesConfigSection;

        private const string OtisDrawing = "Conv_ExcelToTsv_DrawingTemplate_DrawingData";
        private const string OtisDrawingToPart = "Conv_ExcelToTsv_DrawingTemplate_CADPartRelationship";
        private const string OtisDrawingToFile = "Conv_ExcelToTsv_DrawingTemplate_FileData";
        //private const string FilesMetadata = "FilesMetadata";
        private const string OtisPart = "Part";

        public OtisCADDrawingTransformer(IConfiguration configuration, ILogger<OtisCADDrawingTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var CADValidationSection = _configuration.GetSection("OtisCADDrawing");
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
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, OtisDrawing);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, OtisDrawing, TransformStatus.InProgress);

            var OtisDrawingWriter = new TypeDataFileWriter(_processAreaDataPath, _objectCountPerFile)
            {
                FileBaseName = $"Otis_Drawing",
                TypeName = "Drawing",
                FileExtension = "tsv",
            }; 
            
            var OtisCadToPartRelWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath,"Drawing"), _objectCountPerFile)
            {
                FileBaseName = $"Otis_DrawingToPart",
                TypeName = "DrawingToPart",
                FileExtension = "tsv",
            };

            //var OtisCadFileRelWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "Drawing"), _objectCountPerFile)
            //{
            //    FileBaseName = $"Otis_DrawingToFile",
            //    TypeName = "DrawingToFile",
            //    FileExtension = "tsv",
            //};

            var OtisDrawingEntityReader = new TypeDataFileReader(_processAreaDataPath);
            var OtisDrawingEntities = OtisDrawingEntityReader.ReadAllEntities<OtisDrawingEntity>(OtisDrawing);

            var OtisDrawingPartEntityReader = new TypeDataFileReader(_processAreaDataPath);
            var OtisDrawingToPartEntities = OtisDrawingPartEntityReader.ReadAllEntities<OtisCadToPartEntity>(OtisDrawingToPart);

            var OtisPartEntityReader = new TypeDataFileReader(_processAreaDataPath);
            var OtisPartEntities = OtisPartEntityReader.ReadAllEntities<OtisPartTSVEntity>(OtisPart);

            //var OtisCADFileEntityReader = new TypeDataFileReader(_processAreaDataPath);
            //var OtisCADFileEntities = OtisCADFileEntityReader.ReadAllEntities<OtisCadFileEntity>(OtisDrawingToFile);

            //var OtisFileEntityReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, FilesMetadata));
            //var OtisFileEntities = OtisFileEntityReader.ReadAllEntities<OtisFileMetaEntity>("FileMetadata");

            var partDict = OtisPartEntities.ToDictionary(e => e.item_number, e => e.id);
            //var fileDict = OtisFileEntities.ToDictionary(e => e.FileName.Split('_')[0].ToUpper(), e => e.FileId);

            var OtisGroups = OtisDrawingEntities
                           .GroupBy(entity => new { entity.Drawing_Number })
                           .ToList();

            Dictionary<string, string> DrawingMap = new Dictionary<string, string>();

            long successCount = 0;

            //using (OtisCadFileRelWriter)
            //{
                using (OtisCadToPartRelWriter)
                {
                    using (OtisDrawingWriter)
                    {
                        foreach (var OtisGroup in OtisGroups)
                        {
                            if (OtisDrawingWriter.HeaderRow == null)
                            {
                                OtisDrawingWriter.HeaderRow = "ARAS_UNIQUENESS_HELPER\tid\tconfig_id\tkeyed_name\titem_number\tname\tclassification\tAuthoringTool\tDescription\tcreated_by_id\tcreated_on\tcurrent_state\tgeneration\tis_current\tis_released\tmajor_rev\tminor_rev\tpermission_id\tstate\tots_revision\n";
                            }

                            var firstEntity = OtisGroup.First();
                            firstEntity.ID = TransformerUtils.GetNewArasGuid();
                            firstEntity.CONFIG_ID = firstEntity.ID;
                            firstEntity.ARAS_UNIQUENESS_HELPER = "";
                            firstEntity.KEYED_NAME = firstEntity.Drawing_Number;
                            //firstEntity.Effectivity_Date = "12/31/2099";
                            firstEntity.CREATED_BY_ID = "Data Migration";
                            firstEntity.CREATED_ON = DateTime.Now.ToString();
                            firstEntity.CURRENT_STATE = "9EB3760DC37E4950BDB908906433FFE8";
                            firstEntity.GENERATION = "1";
                            firstEntity.IS_CURRENT = "1";
                            firstEntity.IS_RELEASED = "1";
                            firstEntity.MAJOR_REV = "A";
                            firstEntity.MINOR_REV = "1";
                            firstEntity.MODIFIED_ON = DateTime.Now.ToString();
                            firstEntity.PERMISSION_ID = "EA3ED7E7391542D7A17AF2F42B5274ED";
                            firstEntity.State = "Released";

                            DrawingMap[firstEntity.Drawing_Number] = firstEntity.ID;
                            OtisDrawingWriter.WriteRow(firstEntity.DataRow);

                        }
                        successCount++;
                    }

                    foreach (var rel in OtisDrawingToPartEntities)
                    {
                        if (OtisCadToPartRelWriter.HeaderRow == null)
                        {
                            OtisCadToPartRelWriter.HeaderRow = "connection_id\tconfig_id\tsource_id\trelated_id\tpermission_id\tcreated_by_id\tcreated_on\tgeneration\tis_current\tmajor_rev\n";
                        }

                        if (DrawingMap.ContainsKey(rel.Drawing_Number) && partDict.ContainsKey(rel.Part_Number))
                        {
                            rel.ConnectionID = TransformerUtils.GetNewArasGuid();
                            rel.Config_id = rel.ConnectionID;
                            rel.Source_id = DrawingMap[rel.Drawing_Number];
                            rel.Related_id = partDict[rel.Part_Number];
                            rel.Permission_id = "EA3ED7E7391542D7A17AF2F42B5274ED";
                            rel.Created_by_id = "Data Migration";
                            rel.created_on = DateTime.Now.ToString();
                            rel.generation = "1";
                            rel.is_current = "1";
                            rel.major_rev = "A";

                            OtisCadToPartRelWriter.WriteRow(rel.DataRow);

                        }
                    }

                    //foreach (var rel in OtisCADFileEntities)
                    //{
                    //    if (OtisCadFileRelWriter.HeaderRow == null)
                    //    {
                    //        OtisCadFileRelWriter.HeaderRow = "connection_id\tconfig_id\tsource_id\trelated_id\tpermission_id\tcreated_by_id\tcreated_on\tgeneration\tis_current\tmajor_rev\n";
                    //    }

                    //    var fileName = rel.FileName.Substring(0, rel.FileName.IndexOf(".")).ToUpper();
                    //    if (DrawingMap.ContainsKey(fileName))
                    //    {
                    //        var ConnectionID = TransformerUtils.GetNewArasGuid();
                    //        var Config_id = ConnectionID;
                    //        var Source_id = DrawingMap[fileName];
                    //        var Related_id = fileDict[fileName];
                    //        var Permission_id = "EA3ED7E7391542D7A17AF2F42B5274ED";
                    //        var Created_by_id = "Data Migration";
                    //        var created_on = DateTime.Now.ToString();
                    //        var generation = "1";
                    //        var is_current = "1";
                    //        var major_rev = "A";

                    //        OtisCadFileRelWriter.WriteRow($"{ConnectionID}\t{Config_id}\t{Source_id}\t{Related_id}\t{Permission_id}\t{Created_by_id}\t{created_on}\t{generation}\t{is_current}\t{major_rev}\n");

                    //    }
                    //}
                }
            //}

            _migrationDiagnostics.LogTransformTypeStatus(transformName, OtisDrawing, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, OtisDrawing);
        }
    }
}
