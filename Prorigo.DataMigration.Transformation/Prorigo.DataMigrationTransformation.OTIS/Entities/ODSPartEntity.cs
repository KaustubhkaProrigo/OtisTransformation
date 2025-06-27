using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class ODSPartEntity : IWritableEntity, IReadableEntity
    {
        public string ODSNo { get; set; }
        public string ID { get; set; }
        public string Remark { get; set; }
        public string Condition { get; set; }
        public string TableType { get; set; }
        public string Type { get; set; }
        public string QT { get; set; }
        public string ExpressionID { get; set; }

        public string DataRow
        {
            get
            {
                return $"{ODSNo}\t{ID}\t{Remark}\t{Condition}\t{TableType}\t{Type}\t{QT}\t{ExpressionID}\n";
            }
        }

        public ODSPartEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {

            var ODSNoIndex = dataRow.IndexOf('\t');
            ODSNo = dataRow.Substring(0, ODSNoIndex);

            var IDIndex = dataRow.IndexOf('\t', ODSNoIndex + 1);
            ID = dataRow.Substring(ODSNoIndex + 1, IDIndex - ODSNoIndex - 1);

            var RemarkIndex = dataRow.IndexOf('\t', IDIndex + 1);
            Remark = dataRow.Substring(IDIndex + 1, RemarkIndex - IDIndex - 1);

            var ConditionIndex = dataRow.IndexOf('\t', RemarkIndex + 1);
            Condition = dataRow.Substring(RemarkIndex + 1, ConditionIndex - RemarkIndex - 1);

            var TableTypeIndex = dataRow.IndexOf('\t', ConditionIndex + 1);
            TableType = dataRow.Substring(ConditionIndex + 1, TableTypeIndex - ConditionIndex - 1);

            var TypeIndex = dataRow.IndexOf('\t', TableTypeIndex + 1);
            Type = dataRow.Substring(TableTypeIndex + 1, TypeIndex - TableTypeIndex - 1);

            var QTIndex = dataRow.IndexOf('\t', TypeIndex + 1);
            QT = dataRow.Substring(TypeIndex + 1, QTIndex - TypeIndex - 1);

            ExpressionID = dataRow.Substring(QTIndex + 1);

        }

    }
}

