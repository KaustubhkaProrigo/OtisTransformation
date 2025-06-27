using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisParameterTransformedEntity : IReadableEntity, IWritableEntity
    {
        public string id { get; set; }
        public string config_id { get; set; }
        public string ots_name { get; set; }
        public string keyed_name { get; set; }
        public string item_number { get; set; }
        public string ots_description { get; set; }
        public string ots_functional_description { get; set; }
        public string classification { get; set; }
        public string Custom { get; set; }
        public string DataRow
        {
            get
            {
                return $"{id}\t{config_id}\t{ots_name}\t{keyed_name}\t{item_number}\t{ots_description}\t{ots_functional_description}\t{classification}\t{Custom}\n";
            }
        }
        public OtisParameterTransformedEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public void SetProperties(string dataRow)
        {
            var idIndex = dataRow.IndexOf('\t');
            id = dataRow.Substring(0, idIndex);

            var config_idIndex = dataRow.IndexOf('\t', idIndex + 1);
            config_id = dataRow.Substring(idIndex + 1, config_idIndex - idIndex - 1);

            var ots_nameIndex = dataRow.IndexOf('\t', config_idIndex + 1);
            ots_name = dataRow.Substring(config_idIndex + 1, ots_nameIndex - config_idIndex - 1);

            var keyed_nameIndex = dataRow.IndexOf('\t', ots_nameIndex + 1);
            keyed_name = dataRow.Substring(ots_nameIndex + 1, keyed_nameIndex - ots_nameIndex - 1);

            var item_numberIndex = dataRow.IndexOf('\t', keyed_nameIndex + 1);
            item_number = dataRow.Substring(keyed_nameIndex + 1, item_numberIndex - keyed_nameIndex - 1);

            var ots_descriptionIndex = dataRow.IndexOf('\t', item_numberIndex + 1);
            ots_description = dataRow.Substring(item_numberIndex + 1, ots_descriptionIndex - item_numberIndex - 1);

            var ots_functional_descriptionIndex = dataRow.IndexOf('\t', ots_descriptionIndex + 1);
            ots_functional_description = dataRow.Substring(ots_descriptionIndex + 1, ots_functional_descriptionIndex - ots_descriptionIndex - 1);

            var classificationIndex = dataRow.IndexOf('\t', ots_functional_descriptionIndex + 1);
            classification = dataRow.Substring(ots_functional_descriptionIndex + 1, classificationIndex - ots_functional_descriptionIndex - 1);

            Custom = dataRow.Substring(classificationIndex + 1);
        }
    }
}
