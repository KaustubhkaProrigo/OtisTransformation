using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisEBOMEntity : IReadableEntity, IWritableEntity
    {
        public string Source_Part_Number { get; set; }
        public string Related_Part_Number { get; set; }
        public string Description { get; set; }
        public string QT { get; set; }
        public string UOM { get; set; }
        public string Drawing_Number { get; set; }

        public string DataRow
        {
            get
            {
                return $"{Source_Part_Number}\t{Related_Part_Number}\t{Description}\t{QT}\t{UOM}\t{Drawing_Number}\n";
            }
        }

        public OtisEBOMEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public void SetProperties(string dataRow)
        {
            var Source_Part_NumberIndex = dataRow.IndexOf('\t');
            Source_Part_Number = dataRow.Substring(0, Source_Part_NumberIndex);

            var Related_Part_NumberIndex = dataRow.IndexOf('\t', Source_Part_NumberIndex + 1);
            Related_Part_Number = dataRow.Substring(Source_Part_NumberIndex + 1, Related_Part_NumberIndex - Source_Part_NumberIndex - 1);

            var DescriptionIndex = dataRow.IndexOf('\t', Related_Part_NumberIndex + 1);
            Description = dataRow.Substring(Related_Part_NumberIndex + 1, DescriptionIndex - Related_Part_NumberIndex - 1);

            var QTIndex = dataRow.IndexOf('\t', DescriptionIndex + 1);
            QT = dataRow.Substring(DescriptionIndex + 1, QTIndex - DescriptionIndex - 1);

            var UOMIndex = dataRow.IndexOf('\t', QTIndex + 1);
            UOM = dataRow.Substring(QTIndex + 1, UOMIndex - QTIndex - 1);

            Drawing_Number = dataRow.Substring(UOMIndex + 1);
        }
    }
}
