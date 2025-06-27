using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class ODSExpressionEntity : IWritableEntity, IReadableEntity
    {
        public string ODSNO { get; set; }
        public string ID { get; set; }
        public string Remark { get; set; }
        public string Type { get; set; }
        public string condition { get; set; }

        public string DataRow
        {
            get
            {
                return $"{ODSNO}\t{ID}\t{Remark}\t{Type}\t{condition}\n";
            }
        }

        public ODSExpressionEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {

            var ODSNOIndex = dataRow.IndexOf('\t');
            ODSNO = dataRow.Substring(0, ODSNOIndex);

            var IDIndex = dataRow.IndexOf('\t', ODSNOIndex + 1);
            ID = dataRow.Substring(ODSNOIndex + 1, IDIndex - ODSNOIndex - 1);

            var RemarkIndex = dataRow.IndexOf('\t', IDIndex + 1);
            Remark = dataRow.Substring(IDIndex + 1, RemarkIndex - IDIndex - 1);

            var TypeIndex = dataRow.IndexOf('\t', RemarkIndex + 1);
            Type = dataRow.Substring(RemarkIndex + 1, TypeIndex - RemarkIndex - 1);

            condition = dataRow.Substring(TypeIndex + 1);

        }

    }
}
