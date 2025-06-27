using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    internal class SODSConditionExpressionEntity : IReadableEntity, IWritableEntity
    {
        public string SODSNo { get; set; }
        public string ParameterName { get; set; }
        public string ConditionExpression { get; set; }
        public string ConditionTable { get; set; }
        public string isExpression { get; set; }

        public string DataRow
        {
            get
            {
                return $"{SODSNo}\t{ParameterName}\t{ConditionExpression}\t{ConditionTable}\t{isExpression}\n";
            }
        }

        public SODSConditionExpressionEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public void SetProperties(string dataRow)
        {
            var SODSNoIndex = dataRow.IndexOf('\t');
            SODSNo = dataRow.Substring(0, SODSNoIndex);

            var ParameterNameIndex = dataRow.IndexOf('\t', SODSNoIndex + 1);
            ParameterName = dataRow.Substring(SODSNoIndex + 1, ParameterNameIndex - SODSNoIndex - 1);

            var ConditionExpressionIndex = dataRow.IndexOf('\t', ParameterNameIndex + 1);
            ConditionExpression = dataRow.Substring(ParameterNameIndex + 1, ConditionExpressionIndex - ParameterNameIndex - 1);

            var ConditionTableIndex = dataRow.IndexOf('\t', ConditionExpressionIndex + 1);
            ConditionTable = dataRow.Substring(ConditionExpressionIndex + 1, ConditionTableIndex - ConditionExpressionIndex - 1);

            isExpression = dataRow.Substring(ConditionTableIndex + 1);
        }
    }
}

