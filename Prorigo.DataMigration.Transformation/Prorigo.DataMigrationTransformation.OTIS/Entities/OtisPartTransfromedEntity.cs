using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisPartTransformedEntity : IReadableEntity, IWritableEntity
    {
        public string ARAS_UNIQUENESS_HELPER { get; set; }
        public string id { get; set; }
        public string config_id { get; set; }
        public string keyed_name { get; set; }
        public string item_number { get; set; }
        public string name { get; set; }
        public string Custom { get; set; }

        public string DataRow
        {
            get
            {
                return $"{ARAS_UNIQUENESS_HELPER}\t{id}\t{config_id}\t{keyed_name}\t{item_number}\t{name}\t{Custom}\n";
            }
        }

        public OtisPartTransformedEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public void SetProperties(string dataRow)
        {
            var ARAS_UNIQUENESS_HELPERIndex = dataRow.IndexOf('\t');
            ARAS_UNIQUENESS_HELPER = dataRow.Substring(0, ARAS_UNIQUENESS_HELPERIndex);

            var idIndex = dataRow.IndexOf('\t', ARAS_UNIQUENESS_HELPERIndex + 1);
            id = dataRow.Substring(ARAS_UNIQUENESS_HELPERIndex + 1, idIndex - ARAS_UNIQUENESS_HELPERIndex - 1);

            var config_idIndex = dataRow.IndexOf('\t', idIndex + 1);
            config_id = dataRow.Substring(idIndex + 1, config_idIndex - idIndex - 1);

            var keyed_nameIndex = dataRow.IndexOf('\t', config_idIndex + 1);
            keyed_name = dataRow.Substring(config_idIndex + 1, keyed_nameIndex - config_idIndex - 1);

            var item_numberIndex = dataRow.IndexOf('\t', keyed_nameIndex + 1);
            item_number = dataRow.Substring(keyed_nameIndex + 1, item_numberIndex - keyed_nameIndex - 1);

            var nameIndex = dataRow.IndexOf('\t', item_numberIndex + 1);
            name = dataRow.Substring(item_numberIndex + 1, nameIndex - item_numberIndex - 1);

            Custom = dataRow.Substring(nameIndex + 1);
        }
    }
}
