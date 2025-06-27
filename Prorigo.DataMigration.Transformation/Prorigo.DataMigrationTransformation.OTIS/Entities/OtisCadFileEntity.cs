using DocumentFormat.OpenXml.Spreadsheet;
using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisCadFileEntity : IReadableEntity
    {
        public string DrawingNumber { get; set; }
        public string Revision { get; set; }
        public string Classification { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public OtisCadFileEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var DrawingNumberIndex = dataRow.IndexOf('\t');
            DrawingNumber = dataRow.Substring(0, DrawingNumberIndex);

            var RevisionIndex = dataRow.IndexOf('\t', DrawingNumberIndex + 1);
            Revision = dataRow.Substring(DrawingNumberIndex + 1, RevisionIndex - DrawingNumberIndex - 1);

            var ClassificationIndex = dataRow.IndexOf('\t', RevisionIndex + 1);
            Classification = dataRow.Substring(RevisionIndex + 1, ClassificationIndex - RevisionIndex - 1);

            var FileNameIndex = dataRow.IndexOf('\t', ClassificationIndex + 1);
            FileName = dataRow.Substring(ClassificationIndex + 1, FileNameIndex - ClassificationIndex - 1);

            FilePath = dataRow.Substring(FileNameIndex + 1);
        }
    }
}
