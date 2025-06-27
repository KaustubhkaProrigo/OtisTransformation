using System;
using System.Collections.Generic;
using System.Text;

using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    public abstract class FileEntity : IWritableEntity, IReadableEntity
    {
        public string ObjectId { get; set; }
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public string Format { get; set; }

        public abstract string DataRow
        {
            get;
        }

        public FileEntity()
        {
        }

        public FileEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public abstract void SetProperties(string dataRow);
    }
}
