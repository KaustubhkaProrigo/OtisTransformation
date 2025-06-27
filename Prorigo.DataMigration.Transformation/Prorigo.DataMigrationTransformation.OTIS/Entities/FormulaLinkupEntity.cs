using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    internal class FormulaLinkupEntity : IReadableEntity, IWritableEntity
    {
        public string DrawingNo { get; set; }
        public string ID { get; set; }
        public string PropertyName { get; set; }
        public string Formula { get; set; }
        public string Condition { get; set; }

        public string DataRow
        {
            get
            {
                return $"{DrawingNo}\t{ID}\t{PropertyName}\t{Formula}\t{Condition}\n";
            }
        }

        public FormulaLinkupEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public void SetProperties(string dataRow)
        {
            var DrawingNoIndex = dataRow.IndexOf('\t');
            DrawingNo = dataRow.Substring(0, DrawingNoIndex);

            var IDIndex = dataRow.IndexOf('\t', DrawingNoIndex + 1);
            ID = dataRow.Substring(DrawingNoIndex + 1, IDIndex - DrawingNoIndex - 1);

            var PropertyNameIndex = dataRow.IndexOf('\t', IDIndex + 1);
            PropertyName = dataRow.Substring(IDIndex + 1, PropertyNameIndex - IDIndex - 1);

            var FormulaIndex = dataRow.IndexOf('\t', PropertyNameIndex + 1);
            Formula = dataRow.Substring(PropertyNameIndex + 1, FormulaIndex - PropertyNameIndex - 1);

            Condition = dataRow.Substring(FormulaIndex + 1);
        }
    }
}