using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    internal class OtisCalculationSheetEntity : IReadableEntity, IWritableEntity
    {
        public string Id { get; set; }
        public string ARAS_UNIQUENESS_HELPER { get; set; }
        public string CONFIG_ID { get; set; }
        public string ITEM_NUMBER { get; set; }
        public string KEYED_NAME { get; set; }
        public string Custom { get; set; }

        public string DataRow
        {
            get
            {
                return $"{Id}\t{ARAS_UNIQUENESS_HELPER}\t{CONFIG_ID}\t{ITEM_NUMBER}\t{KEYED_NAME}\t{Custom}\n";
            }
        }

        public OtisCalculationSheetEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public void SetProperties(string dataRow)
        {
            var IdIndex = dataRow.IndexOf('\t');
            Id = dataRow.Substring(0, IdIndex);

            var ARAS_UNIQUENESS_HELPERIndex = dataRow.IndexOf('\t', IdIndex + 1);
            ARAS_UNIQUENESS_HELPER = dataRow.Substring(IdIndex + 1, ARAS_UNIQUENESS_HELPERIndex - IdIndex - 1);

            var CONFIG_IDIndex = dataRow.IndexOf('\t', ARAS_UNIQUENESS_HELPERIndex + 1);
            CONFIG_ID = dataRow.Substring(ARAS_UNIQUENESS_HELPERIndex + 1, CONFIG_IDIndex - ARAS_UNIQUENESS_HELPERIndex - 1);

            var ITEM_NUMBERIndex = dataRow.IndexOf('\t', CONFIG_IDIndex + 1);
            ITEM_NUMBER = dataRow.Substring(CONFIG_IDIndex + 1, ITEM_NUMBERIndex - CONFIG_IDIndex - 1);

            var KEYED_NAMEIndex = dataRow.IndexOf('\t', ITEM_NUMBERIndex + 1);
            KEYED_NAME = dataRow.Substring(ITEM_NUMBERIndex + 1, KEYED_NAMEIndex - ITEM_NUMBERIndex - 1);

            Custom = dataRow.Substring(KEYED_NAMEIndex + 1);
        }
    }
}
