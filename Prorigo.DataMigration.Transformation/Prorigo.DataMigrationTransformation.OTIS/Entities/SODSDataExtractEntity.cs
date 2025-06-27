using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class SODSDataExtractEntity : IWritableEntity, IReadableEntity
    {

        public string SODSNO { get; set; }
        public string ID { get; set; }
        public string SS_No { get; set; }
        public string Description { get; set; }
        public string Condition { get; set; }
        public string ODS_No { get; set; }
        public string Qty { get; set; }
        public string DataRow
        {
            get
            {
                return $"{SODSNO}\t{ID}\t{SS_No}\t{Description}\t{Condition}\t{ODS_No}\t{Qty}\n";
            }
        }

        public SODSDataExtractEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {

            var SODSNoIndex = dataRow.IndexOf('\t');
            SODSNO = dataRow.Substring(0, SODSNoIndex);

            var IDIndex = dataRow.IndexOf('\t', SODSNoIndex + 1);
            ID = dataRow.Substring(SODSNoIndex + 1, IDIndex - SODSNoIndex - 1);

            var SS_NoIndex = dataRow.IndexOf('\t', IDIndex + 1);
            SS_No = dataRow.Substring(IDIndex + 1, SS_NoIndex - IDIndex - 1);

            var DescriptionIndex = dataRow.IndexOf('\t', SS_NoIndex + 1);
            Description = dataRow.Substring(SS_NoIndex + 1, DescriptionIndex - SS_NoIndex - 1);

            var ConditionIndex = dataRow.IndexOf('\t', DescriptionIndex + 1);
            Condition = dataRow.Substring(DescriptionIndex + 1, ConditionIndex - DescriptionIndex - 1);

            var ODS_NoIndex = dataRow.IndexOf('\t', ConditionIndex + 1);
            ODS_No = dataRow.Substring(ConditionIndex + 1, ODS_NoIndex - ConditionIndex - 1);

            Qty = dataRow.Substring(ODS_NoIndex + 1);

        }

    }
}

