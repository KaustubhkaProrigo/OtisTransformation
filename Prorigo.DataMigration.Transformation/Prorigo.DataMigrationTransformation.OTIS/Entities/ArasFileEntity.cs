using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    public class ArasFileEntity : FileEntity
    {
        public string FileId { get; set; }
        public string Checksum { get; set; }
        public string LocatedId { get; set; }
        public string VaultId { get; set; }
        public string FilePath { get; set; }
        public string Comment { get; set; }

        public override string DataRow
        {
            get
            {
                return $"{FileId}\t{ObjectId}\t{FileName}\t{Checksum}\t{FileSize}\t{Format}\t{LocatedId}\t{VaultId}\t{FilePath}\t{Comment}\n";
                //"FileId\tObjectId\tFileName\tChecksum\tFileSize\tFormat\tLocatedId\tVaultId\tFilePath\tComment\n"
            }
        }

        public ArasFileEntity()
        {
        }

        public ArasFileEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public override void SetProperties(string dataRow)
        {
            var fileIdIndex = dataRow.IndexOf('\t');
            FileId = dataRow.Substring(0, fileIdIndex);

            var objectIdIndex = dataRow.IndexOf('\t', fileIdIndex + 1);
            ObjectId = dataRow.Substring(fileIdIndex + 1, objectIdIndex - fileIdIndex - 1);

            var fileNameIndex = dataRow.IndexOf('\t', objectIdIndex + 1);
            FileName = dataRow.Substring(objectIdIndex + 1, fileNameIndex - objectIdIndex - 1);

            var checksumIndex = dataRow.IndexOf('\t', fileNameIndex + 1);
            Checksum = dataRow.Substring(fileNameIndex + 1, checksumIndex - fileNameIndex - 1);

            var fileSizeIndex = dataRow.IndexOf('\t', checksumIndex + 1);
            FileSize = dataRow.Substring(checksumIndex + 1, fileSizeIndex - checksumIndex - 1);

            var formatIndex = dataRow.IndexOf('\t', fileSizeIndex + 1);
            Format = dataRow.Substring(fileSizeIndex + 1, formatIndex - fileSizeIndex - 1);

            var locatedIdIndex = dataRow.IndexOf('\t', formatIndex + 1);
            LocatedId = dataRow.Substring(formatIndex + 1, locatedIdIndex - formatIndex - 1);

            var VaultIdIdIndex = dataRow.IndexOf('\t', locatedIdIndex + 1);
            VaultId = dataRow.Substring(locatedIdIndex + 1, VaultIdIdIndex - locatedIdIndex - 1);

            var FilePathIdIndex = dataRow.IndexOf('\t', VaultIdIdIndex + 1);
            FilePath = dataRow.Substring(VaultIdIdIndex + 1, FilePathIdIndex - VaultIdIdIndex - 1);

            Comment = dataRow.Substring(FilePathIdIndex + 1);
        }
    }
}
