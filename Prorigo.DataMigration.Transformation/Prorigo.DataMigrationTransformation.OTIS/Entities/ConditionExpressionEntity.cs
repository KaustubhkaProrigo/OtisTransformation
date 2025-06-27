using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    internal class ConditionExpressionEntity : IReadableEntity, IWritableEntity
    {
        public string DrawingNo { get; set; }
        public string ParameterName { get; set; }
        public string ConditionExpression { get; set; }
        public string ConditionTable { get; set; }
        public string isExpression { get; set; }

        public string DataRow
        {
            get
            {
                return $"{DrawingNo}\t{ParameterName}\t{ConditionExpression}\t{ConditionTable}\t{isExpression}\n";
            }
        }

        public ConditionExpressionEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public void SetProperties(string dataRow)
        {
            var DrawingNoIndex = dataRow.IndexOf('\t');
            DrawingNo = dataRow.Substring(0, DrawingNoIndex);

            var ParameterNameIndex = dataRow.IndexOf('\t', DrawingNoIndex + 1);
            ParameterName = dataRow.Substring(DrawingNoIndex + 1, ParameterNameIndex - DrawingNoIndex - 1);

            var ConditionExpressionIndex = dataRow.IndexOf('\t', ParameterNameIndex + 1);
            ConditionExpression = dataRow.Substring(ParameterNameIndex + 1, ConditionExpressionIndex - ParameterNameIndex - 1);

            var ConditionTableIndex = dataRow.IndexOf('\t', ConditionExpressionIndex + 1);
            ConditionTable = dataRow.Substring(ConditionExpressionIndex + 1, ConditionTableIndex - ConditionExpressionIndex - 1);

            isExpression = dataRow.Substring(ConditionTableIndex + 1);
        }
    }
}
