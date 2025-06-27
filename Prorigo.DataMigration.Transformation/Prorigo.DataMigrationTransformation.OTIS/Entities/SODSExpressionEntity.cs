using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class SODSExpressionEntity : IWritableEntity, IReadableEntity
    {
        public string SODSNO { get; set; }
        public string ID { get; set; }
        public string SSNo { get; set; }
        public string description { get; set; }
        public string condition { get; set; }

        public string DataRow
        {
            get
            {
                return $"{SODSNO}\t{ID}\t{SSNo}\t{condition}\n";
            }
        }
        public SODSExpressionEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {

            var SODSNOIndex = dataRow.IndexOf('\t');
            SODSNO = dataRow.Substring(0, SODSNOIndex);

            var IDIndex = dataRow.IndexOf('\t', SODSNOIndex + 1);
            ID = dataRow.Substring(SODSNOIndex + 1, IDIndex - SODSNOIndex - 1);

            var SSNoIndex = dataRow.IndexOf('\t', IDIndex + 1);
            SSNo = dataRow.Substring(IDIndex + 1, SSNoIndex - IDIndex - 1);

            var descriptionIndex = dataRow.IndexOf('\t', SSNoIndex + 1);
            description = dataRow.Substring(SSNoIndex + 1, descriptionIndex - SSNoIndex - 1);

            condition = dataRow.Substring(descriptionIndex + 1);

        }

    }
}
