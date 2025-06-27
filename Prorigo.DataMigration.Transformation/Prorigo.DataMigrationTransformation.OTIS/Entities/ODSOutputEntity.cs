using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    internal class ODSOutputEntity : IReadableEntity, IWritableEntity
    {
        public string ODSNo { get; set; }
        public string Rev { get; set; }
        public string ID { get; set; }
        public string Parameter { get; set; }
        public string Description { get; set; }
        public string ParameterType { get; set; }
        public string UOM { get; set; }
        public string Output { get; set; }
        public string Input { get; set; }

        public string DataRow
        {
            get
            {
                return $"{ODSNo}\t{Rev}\t{ID}\t{Parameter}\t{Description}\t{ParameterType}\t{UOM}\t{Output}\t{Input}\n";
            }
        }

        public ODSOutputEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public void SetProperties(string dataRow)
        {
            var ODSNoIndex = dataRow.IndexOf('\t');
            ODSNo = dataRow.Substring(0, ODSNoIndex);

            var RevIndex = dataRow.IndexOf('\t', ODSNoIndex + 1);
            Rev = dataRow.Substring(ODSNoIndex + 1, RevIndex - ODSNoIndex - 1);

            var IDIndex = dataRow.IndexOf('\t', RevIndex + 1);
            ID = dataRow.Substring(RevIndex + 1, IDIndex - RevIndex - 1);

            var ParameterIndex = dataRow.IndexOf('\t', IDIndex + 1);
            Parameter = dataRow.Substring(IDIndex + 1, ParameterIndex - IDIndex - 1);

            var DescriptionIndex = dataRow.IndexOf('\t', ParameterIndex + 1);
            Description = dataRow.Substring(ParameterIndex + 1, DescriptionIndex - ParameterIndex - 1);

            var ParameterTypeIndex = dataRow.IndexOf('\t', DescriptionIndex + 1);
            ParameterType = dataRow.Substring(DescriptionIndex + 1, ParameterTypeIndex - DescriptionIndex - 1);

            var UOMIndex = dataRow.IndexOf('\t', ParameterTypeIndex + 1);
            UOM = dataRow.Substring(ParameterTypeIndex + 1, UOMIndex - ParameterTypeIndex - 1);

            var FormulaIndex = dataRow.IndexOf('\t', UOMIndex + 1);
            Output = dataRow.Substring(UOMIndex + 1, FormulaIndex - UOMIndex - 1);

            Input = dataRow.Substring(FormulaIndex + 1);
        }
    }
}