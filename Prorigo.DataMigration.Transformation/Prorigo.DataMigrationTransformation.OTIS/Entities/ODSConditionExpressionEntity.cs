using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    internal class ODSConditionExpressionEntity : IReadableEntity, IWritableEntity
    {
        public string ODSNo { get; set; }
        public string Parameter { get; set; }
        public string ConditionExpression { get; set; }
        public string ConditionTable { get; set; }
        public string isExpression { get; set; }

        public string DataRow
        {
            get
            {
                return $"{ODSNo}\t{Parameter}\t{ConditionExpression}\t{ConditionTable}\t{isExpression}\n";
            }
        }

        public ODSConditionExpressionEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public void SetProperties(string dataRow)
        {
            var ODSNoIndex = dataRow.IndexOf('\t');
            ODSNo = dataRow.Substring(0, ODSNoIndex);

            var ParameterIndex = dataRow.IndexOf('\t', ODSNoIndex + 1);
            Parameter = dataRow.Substring(ODSNoIndex + 1, ParameterIndex - ODSNoIndex - 1);

            var ConditionExpressionIndex = dataRow.IndexOf('\t', ParameterIndex + 1);
            ConditionExpression = dataRow.Substring(ParameterIndex + 1, ConditionExpressionIndex - ParameterIndex - 1);

            var ConditionTableIndex = dataRow.IndexOf('\t', ConditionExpressionIndex + 1);
            ConditionTable = dataRow.Substring(ConditionExpressionIndex + 1, ConditionTableIndex - ConditionExpressionIndex - 1);

            isExpression = dataRow.Substring(ConditionTableIndex + 1);
        }
    }
}
