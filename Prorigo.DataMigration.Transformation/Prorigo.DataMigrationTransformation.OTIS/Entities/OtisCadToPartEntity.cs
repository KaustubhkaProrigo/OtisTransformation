using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json.Linq;
using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisCadToPartEntity : IReadableEntity, IWritableEntity
    {
        public string Drawing_Number { get; set; }
        public string Revision { get; set; }
        public string Classification { get; set; }
        public string Part_Number { get; set; }
        public string Part_Revision { get; set; }
        public string Part_Clasification { get; set; }
        public string ConnectionID { get; set; }
        public string Config_id { get; set; }
        public string Created_by_id { get; set; }
        public string created_on { get; set; }
        public string Source_id { get; set; }
        public string Related_id { get; set; }
        public string Permission_id { get; set; }
        public string is_current { get; set; }
        public string major_rev { get; set; }
        public string generation { get; set; }


        public string DataRow
        {
            get
            {

                return $"{ConnectionID}\t{Config_id}\t{Source_id}\t{Related_id}\t{Permission_id}\t{Created_by_id}\t{created_on}\t{generation}\t{is_current}\t{major_rev}\n";

            }
        }


        public OtisCadToPartEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var Drawing_NumberIndex = dataRow.IndexOf('\t');
            Drawing_Number = dataRow.Substring(0, Drawing_NumberIndex);

            var RevisionIndex = dataRow.IndexOf('\t', Drawing_NumberIndex + 1);
            Revision = dataRow.Substring(Drawing_NumberIndex + 1, RevisionIndex - Drawing_NumberIndex - 1);

            var ClassificationIndex = dataRow.IndexOf('\t', RevisionIndex + 1);
            Classification = dataRow.Substring(RevisionIndex + 1, ClassificationIndex - RevisionIndex - 1);

            var Part_NumberIndex = dataRow.IndexOf('\t', ClassificationIndex + 1);
            Part_Number = dataRow.Substring(ClassificationIndex + 1, Part_NumberIndex - ClassificationIndex - 1);

            var Part_RevisionIndex = dataRow.IndexOf('\t', Part_NumberIndex + 1);
            Part_Revision = dataRow.Substring(Part_NumberIndex + 1, Part_RevisionIndex - Part_NumberIndex - 1);

            Part_Clasification = dataRow.Substring(Part_RevisionIndex + 1);
        }
    }
}
