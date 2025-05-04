using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Transformer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Prorigo.Plm.DataMigration.Utilities;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    public class OtisFileTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisFileTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly string _fileStoreLocation;
        private const string FILEMETADATA = "FileMetadata";


        public OtisFileTransformer(IConfiguration configuration, ILogger<OtisFileTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var fileSection = _configuration.GetSection("OtisFile");
            _processAreaDataPath = fileSection.GetSection("ProcessAreaDataPath").Value;
            _objectCountPerFile = fileSection.GetValue<long>("ObjectCountPerFile");
            _fileStoreLocation = fileSection.GetSection("FileStoreLocation").Value;
        }

        public void Transform(string LicenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            //License key
            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");
            if (isLicenValid)
            {

                    var FileMetaDataWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "FilesMetadata"), _objectCountPerFile)
                    {
                        FileBaseName = $"OtisFileMetaData",
                        TypeName = FILEMETADATA,
                        HeaderRow = "FileId\tObjectId\tFileName\tChecksum\tFileSize\tFormat\tLocatedId\tVaultId\tFilePath\tComment\n",
                        FileExtension = "tsv"
                    };


                    using (FileMetaDataWriter)
                    {
                        TrasnformSubDirectories(_fileStoreLocation, FileMetaDataWriter);
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

        private void TrasnformSubDirectories(string directoryName, TypeDataFileWriter FileMetaDataWriter)
        {
            var subDirectories = Directory.GetDirectories(directoryName);
            foreach (var subDirectoryName in subDirectories)
            {
                FileInfo[] subDirectoryFiles = new DirectoryInfo(subDirectoryName).GetFiles();
                foreach (FileInfo subDirectoryFile in subDirectoryFiles)
                {
                    var FileId = TransformerUtils.GetNewArasGuid();
                    var objectId = TransformerUtils.GetNewArasGuid();
                    var fileName = subDirectoryFile.Name;
                    var filePath = subDirectoryFile.FullName;
                    var checksum = File.Exists(filePath) ? TransformerUtils.CalculateMD5(filePath) : string.Empty;
                    var locatedId = TransformerUtils.GetNewArasGuid();
                    var fileSize = subDirectoryFile.Length.ToString();
                    var fileFormat = subDirectoryFile.Extension.ToString();
                    var vaultId = "67BBB9204FE84A8981ED8313049BA06C";
                    var comment = "";

                    FileMetaDataWriter.WriteRow($"{FileId}\t{objectId}\t{fileName}\t{checksum}\t{fileSize}\t{fileFormat}\t{locatedId}\t{vaultId}\t{filePath}\t{comment}\n");
                   
                }

                TrasnformSubDirectories(subDirectoryName, FileMetaDataWriter);
            }
        }
    }
}
