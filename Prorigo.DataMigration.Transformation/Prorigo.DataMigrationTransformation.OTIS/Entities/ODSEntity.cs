using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class ODSEntity : IWritableEntity, IReadableEntity
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
                return $"{ARAS_UNIQUENESS_HELPER}\t{ID}\t{CONFIG_ID}\t{KEYED_NAME}\t{ITEM_NUMBER}\n";
            }
        }

        public ODSEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var ARAS_UNIQUENESS_HELPERIndex = dataRow.IndexOf('\t');
            ARAS_UNIQUENESS_HELPER = dataRow.Substring(0, ARAS_UNIQUENESS_HELPERIndex);

            var IDIndex = dataRow.IndexOf('\t', ARAS_UNIQUENESS_HELPERIndex + 1);
            ID = dataRow.Substring(ARAS_UNIQUENESS_HELPERIndex + 1, IDIndex - ARAS_UNIQUENESS_HELPERIndex - 1);

            var CONFIG_IDIndex = dataRow.IndexOf('\t', IDIndex + 1);
            CONFIG_ID = dataRow.Substring(IDIndex + 1, CONFIG_IDIndex - IDIndex - 1);

            var KEYED_NAMEIndex = dataRow.IndexOf('\t', CONFIG_IDIndex + 1);
            KEYED_NAME = dataRow.Substring(CONFIG_IDIndex + 1, KEYED_NAMEIndex - CONFIG_IDIndex - 1);

            var ITEM_NUMBERIndex = dataRow.IndexOf('\t', KEYED_NAMEIndex + 1);
            ITEM_NUMBER = dataRow.Substring(KEYED_NAMEIndex + 1, ITEM_NUMBERIndex - KEYED_NAMEIndex - 1);

            CustomProperties = dataRow.Substring(ITEM_NUMBERIndex + 1);
        }

    }
}
