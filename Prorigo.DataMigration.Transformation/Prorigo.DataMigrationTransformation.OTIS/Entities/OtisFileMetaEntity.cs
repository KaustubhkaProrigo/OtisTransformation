using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    internal class OtisFileMetaEntity : IReadableEntity
    {
        public string FileId { get; set; }
        public string ObjectId { get; set; }
        public string FileName { get; set; }

        public string Custom { get; set; }

        public OtisFileMetaEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var FileIdIndex = dataRow.IndexOf('\t');
            FileId = dataRow.Substring(0, FileIdIndex);

            var ObjectIdIndex = dataRow.IndexOf('\t', FileIdIndex + 1);
            ObjectId = dataRow.Substring(FileIdIndex + 1, ObjectIdIndex - FileIdIndex - 1);

            var FileNameIndex = dataRow.IndexOf('\t', ObjectIdIndex + 1);
            FileName = dataRow.Substring(ObjectIdIndex + 1, FileNameIndex - ObjectIdIndex - 1);

            Custom = dataRow.Substring(FileNameIndex + 1);

        }
    }
}
