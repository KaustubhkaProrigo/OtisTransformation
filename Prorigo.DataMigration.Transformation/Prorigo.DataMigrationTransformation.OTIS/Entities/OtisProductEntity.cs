using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    internal class OtisProductEntity : IReadableEntity
    {
        public string ARAS_UNIQUENESS_HELPER { get; set; }
        public string ID { get; set; }
        public string ConfigId { get; set; }
        public string KeyedName { get; set; }
        public string Item_Number { get; set; }
        public string Name { get; set; }
        public string Custom { get; set; }

        public OtisProductEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var ARAS_UNIQUENESS_HELPERIndex = dataRow.IndexOf('\t');
            ARAS_UNIQUENESS_HELPER = dataRow.Substring(0, ARAS_UNIQUENESS_HELPERIndex);

            var IDIndex = dataRow.IndexOf('\t', ARAS_UNIQUENESS_HELPERIndex + 1);
            ID = dataRow.Substring(ARAS_UNIQUENESS_HELPERIndex + 1, IDIndex - ARAS_UNIQUENESS_HELPERIndex - 1);

            var ConfigIdIndex = dataRow.IndexOf('\t', IDIndex + 1);
            ConfigId = dataRow.Substring(IDIndex + 1, ConfigIdIndex - IDIndex - 1);

            var KeyedNameIndex = dataRow.IndexOf('\t', ConfigIdIndex + 1);
            KeyedName = dataRow.Substring(ConfigIdIndex + 1, KeyedNameIndex - ConfigIdIndex - 1);

            var Item_NumberIndex = dataRow.IndexOf('\t', KeyedNameIndex + 1);
            Item_Number = dataRow.Substring(KeyedNameIndex + 1, Item_NumberIndex - KeyedNameIndex - 1);
            
            var NameIndex = dataRow.IndexOf('\t', Item_NumberIndex + 1);
            Name = dataRow.Substring(Item_NumberIndex + 1, NameIndex - Item_NumberIndex - 1);

            Custom = dataRow.Substring(NameIndex + 1);

        }
    }
}
