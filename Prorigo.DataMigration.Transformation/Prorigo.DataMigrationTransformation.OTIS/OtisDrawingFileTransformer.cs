using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using Prorigo.Plm.DataMigration.Utilities;
using Prorigo.DataMigrationTransformation.OTIS.Entities;


namespace Prorigo.DataMigrationTransformation.OTIS
{
    public class OtisDrawingFileTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisDrawingFileTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly string _fileStoreLocation;
        private readonly string _fileVaultName;
        private readonly string _vaultId;
        private const string FILEMETADATA = "FileMetadata";
        private const string OtisDrawingToFile = "Conv_ExcelToTsv_DrawingTemplate_FileData";


        public OtisDrawingFileTransformer(IConfiguration configuration, ILogger<OtisDrawingFileTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var fileSection = _configuration.GetSection("OtisDrawingFile");
            _processAreaDataPath = fileSection.GetSection("ProcessAreaDataPath").Value;
            _objectCountPerFile = fileSection.GetValue<long>("ObjectCountPerFile");
            _fileStoreLocation = fileSection.GetSection("FileStoreLocation").Value;
            _fileVaultName = fileSection.GetSection("VaultName").Value;
            _vaultId = fileSection.GetSection("VaultId").Value;
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
                
                TrasnformDrawingFile(transformName);
            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }

        private void TrasnformDrawingFile(string transformName)
        {
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, FILEMETADATA);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, FILEMETADATA, TransformStatus.InProgress);

            var FileMetaDataWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "Drawing"), _objectCountPerFile)
            {
                FileBaseName = $"OtisFileMetaData",
                TypeName = FILEMETADATA,
                HeaderRow = "FileId\tObjectId\tFileName\tChecksum\tFileSize\tFormat\tLocatedId\tVaultId\tFilePath\tComment\n",
                FileExtension = "tsv"
            };

            var OtisCadFileRelWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "Drawing"), _objectCountPerFile)
            {
                FileBaseName = $"Otis_DrawingToFile",
                TypeName = "DrawingToFile",
                FileExtension = "tsv",
            };

            var OtisCADEntityReader = new TypeDataFileReader(_processAreaDataPath);
            var OtisCADEntities = OtisCADEntityReader.ReadAllEntities<ArasCadEntity>("Drawing", "*.tsv");

            //Dictionary<string, string> DrawingNameRevisionToIdMap = new Dictionary<string, string>();
            var DrawingNameRevisionToIdMap = OtisCADEntities.ToDictionary(e => e.ItemNumber+"|"+e.OtsRevision, e => e.ID);

            var OtisCADFileEntityReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "Drawing"));
            var OtisCADFileEntities = OtisCADFileEntityReader.ReadAllEntities<OtisCadFileEntity>(OtisDrawingToFile, "*.tsv");

            long successCount = 0;
            using (FileMetaDataWriter)
            {
                using (OtisCadFileRelWriter)
                {
                    foreach (var OtisCADFileEntity in OtisCADFileEntities)
                    {                        
                        var drawingObject = OtisCADFileEntity.DrawingNumber + "|" + OtisCADFileEntity.Revision;
                        if (DrawingNameRevisionToIdMap.ContainsKey(drawingObject))
                        {
                            var drawingName = OtisCADFileEntity.DrawingNumber;
                            var drawingRevision = OtisCADFileEntity.Revision;
                            var objectID = DrawingNameRevisionToIdMap[drawingObject];
                            var fileNameo = OtisCADFileEntity.FileName;
                            var filepath = OtisCADFileEntity.FilePath;
                            var comment = string.Empty;
                            var fileExten = OtisCADFileEntity.FileName;
                            //FileName.Substring(0, rel.FileName.IndexOf(".")).ToUpper();

                            var absoluteFilePath = Path.Combine(_fileStoreLocation,filepath);
                            if (File.Exists(absoluteFilePath))
                            {
                                try
                                {
                                    var arasFileId = TransformerUtils.GetNewArasGuid();
                                    var checksum = File.Exists(absoluteFilePath) ? TransformerUtils.CalculateMD5(absoluteFilePath) : string.Empty;
                                    var fileSize = (new FileInfo(absoluteFilePath)).Length.ToString();

                                    var arasFileEntity = new ArasFileEntity { FileId = arasFileId, ObjectId = objectID, FileName = fileNameo, Checksum = checksum, FileSize = fileSize, Format = "", LocatedId = TransformerUtils.GetNewArasGuid(), VaultId = _vaultId, FilePath = absoluteFilePath, Comment = comment };
                                    FileMetaDataWriter.WriteRow(arasFileEntity.DataRow);

                                    if (OtisCadFileRelWriter.HeaderRow == null)
                                    {
                                        OtisCadFileRelWriter.HeaderRow = "connection_id\tconfig_id\tsource_id\trelated_id\tpermission_id\tcreated_by_id\tcreated_on\tgeneration\tis_current\tmajor_rev\n";
                                    }
                                    var ConnectionID = TransformerUtils.GetNewArasGuid();
                                    var Config_id = ConnectionID;
                                    var Source_id = objectID;
                                    var Related_id = arasFileId;
                                    var Permission_id = "EA3ED7E7391542D7A17AF2F42B5274ED";
                                    var Created_by_id = "Data Migration";
                                    var created_on = DateTime.Now.ToString();
                                    var generation = "1";
                                    var is_current = "1";
                                    var major_rev = "A";

                                    OtisCadFileRelWriter.WriteRow($"{ConnectionID}\t{Config_id}\t{Source_id}\t{Related_id}\t{Permission_id}\t{Created_by_id}\t{created_on}\t{generation}\t{is_current}\t{major_rev}\n");

                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Transformation failed for File: '{absoluteFilePath}' | ObjectId: {objectID} | Error: '{ex.Message}'");
                                    continue;
                                }
                            }
                            else
                            {
                                _logger.LogError($"File does not exist at '{absoluteFilePath}' | ObjectId: {objectID}");
                            }

                            continue;
                        }
                        else 
                        {
                            _logger.LogError($"CAD Object does not exist: '{drawingObject}'");
                        }
                        
                    }
                }
            }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, FILEMETADATA, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, FILEMETADATA);
        }
    }
}

