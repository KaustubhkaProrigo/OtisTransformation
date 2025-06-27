using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisSODSEntity : IWritableEntity, IReadableEntity
    {
        public string ARAS_UNIQUENESS_HELPER { get; set; }
        public string ID { get; set; }
        public string CONFIG_ID { get; set; }
        public string KEYED_NAME { get; set; }
        public string ITEM_NUMBER { get; set; }
        public string CustomProperties { get; set; }
        public string DataRow
        {
            get
            {
                return $"{ID}\t{ARAS_UNIQUENESS_HELPER}\t{CONFIG_ID}\t{ITEM_NUMBER}\t{KEYED_NAME}\n";
            }
        }

        public OtisSODSEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var IDIndex = dataRow.IndexOf('\t');
            ID = dataRow.Substring(0, IDIndex);

            var ARAS_UNIQUENESS_HELPERIndex = dataRow.IndexOf('\t', IDIndex + 1);
             ARAS_UNIQUENESS_HELPER = dataRow.Substring(IDIndex + 1, ARAS_UNIQUENESS_HELPERIndex - IDIndex - 1);

            var CONFIG_IDIndex = dataRow.IndexOf('\t', ARAS_UNIQUENESS_HELPERIndex + 1);
            CONFIG_ID = dataRow.Substring(ARAS_UNIQUENESS_HELPERIndex + 1, CONFIG_IDIndex - ARAS_UNIQUENESS_HELPERIndex - 1);

            var ITEM_NUMBERIndex = dataRow.IndexOf('\t', CONFIG_IDIndex + 1);
            ITEM_NUMBER = dataRow.Substring(CONFIG_IDIndex + 1, ITEM_NUMBERIndex - CONFIG_IDIndex - 1);

            var KEYED_NAMEIndex = dataRow.IndexOf('\t', ITEM_NUMBERIndex + 1);
            KEYED_NAME = dataRow.Substring(ITEM_NUMBERIndex + 1, KEYED_NAMEIndex - ITEM_NUMBERIndex - 1);

            CustomProperties = dataRow.Substring(KEYED_NAMEIndex + 1);
        }

    }
}

