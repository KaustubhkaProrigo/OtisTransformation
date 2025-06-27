using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    internal class PMSOutputEntity : IReadableEntity, IWritableEntity
    {
        public string PMSNO { get; set; }
        public string ID { get; set; }
        public string ParameterName { get; set; }
        public string ParameterDescription { get; set; }
        public string ParameterType { get; set; }
        public string UOM { get; set; }
        public string ValueListorValueRange { get; set; }
        public string Formula { get; set; }
        public string ProductRangeValueVerification { get; set; }
        public string Condition { get; set; }

        public string DataRow
        {
            get
            {
                return $"{PMSNO}\t{ID}\t{ParameterName}\t{ParameterDescription}\t{ParameterType}\t{UOM}\t{ValueListorValueRange}\t{Formula}\t{ProductRangeValueVerification}\t{Condition}\n";
            }
        }

        public PMSOutputEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public void SetProperties(string dataRow)
        {
            var PMSNOIndex = dataRow.IndexOf('\t');
            PMSNO = dataRow.Substring(0, PMSNOIndex);

            var IDIndex = dataRow.IndexOf('\t', PMSNOIndex + 1);
            ID = dataRow.Substring(PMSNOIndex + 1, IDIndex - PMSNOIndex - 1);

            var ParameterNameIndex = dataRow.IndexOf('\t', IDIndex + 1);
            ParameterName = dataRow.Substring(IDIndex + 1, ParameterNameIndex - IDIndex - 1);

            var ParameterDescriptionIndex = dataRow.IndexOf('\t', ParameterNameIndex + 1);
            ParameterDescription = dataRow.Substring(ParameterNameIndex + 1, ParameterDescriptionIndex - ParameterNameIndex - 1);

            var ParameterTypeIndex = dataRow.IndexOf('\t', ParameterDescriptionIndex + 1);
            ParameterType = dataRow.Substring(ParameterDescriptionIndex + 1, ParameterTypeIndex - ParameterDescriptionIndex - 1);

            var UOMIndex = dataRow.IndexOf('\t', ParameterTypeIndex + 1);
            UOM = dataRow.Substring(ParameterTypeIndex + 1, UOMIndex - ParameterTypeIndex - 1);

            var ValueListorValueRangeIndex = dataRow.IndexOf('\t', UOMIndex + 1);
            ValueListorValueRange = dataRow.Substring(UOMIndex + 1, ValueListorValueRangeIndex - UOMIndex - 1);

            var FormulaIndex = dataRow.IndexOf('\t', ValueListorValueRangeIndex + 1);
            Formula = dataRow.Substring(ValueListorValueRangeIndex + 1, FormulaIndex - ValueListorValueRangeIndex - 1);

            var ProductRangeValueVerificationIndex = dataRow.IndexOf('\t', FormulaIndex + 1);
            ProductRangeValueVerification = dataRow.Substring(FormulaIndex + 1, ProductRangeValueVerificationIndex - FormulaIndex - 1);

            Condition = dataRow.Substring(ProductRangeValueVerificationIndex + 1);
        }
    }
}