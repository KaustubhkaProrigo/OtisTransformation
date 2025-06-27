using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    internal class SODSOutputEntity : IReadableEntity, IWritableEntity
    {
        public string SODSNO { get; set; }
        public string ID { get; set; }
        public string ParameterName { get; set; }
        public string ParameterDescription { get; set; }
        public string ParameterType { get; set; }
        public string UOM { get; set; }
        public string ValueListorValueRange { get; set; }
        public string ConditionFormulaDetail { get; set; }
        public string DefaultValueFormulaDetails { get; set; }

        public string DataRow
        {
            get
            {
                return $"{SODSNO}\t{ID}\t{ParameterName}\t{ParameterDescription}\t{ParameterType}\t{UOM}\t{ValueListorValueRange}\t{ConditionFormulaDetail}\t{DefaultValueFormulaDetails}\n";
            }
        }

        public SODSOutputEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public void SetProperties(string dataRow)
        {
            var SODSNoIndex = dataRow.IndexOf('\t');
            SODSNO = dataRow.Substring(0, SODSNoIndex);

            var IDIndex = dataRow.IndexOf('\t', SODSNoIndex + 1);
            ID = dataRow.Substring(SODSNoIndex + 1, IDIndex - SODSNoIndex - 1);

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

            var ConditionFormulaDetailIndex = dataRow.IndexOf('\t', ValueListorValueRangeIndex + 1);
            ConditionFormulaDetail = dataRow.Substring(ValueListorValueRangeIndex + 1, ConditionFormulaDetailIndex - ValueListorValueRangeIndex - 1);

            DefaultValueFormulaDetails = dataRow.Substring(ConditionFormulaDetailIndex + 1);
        }
    }
}