using System;
using System.Collections.Generic;
using System.Text;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisUDCEntity : IWritableEntity, IReadableEntity
    {
        public string Classification { get; set; }
        public string ots_code { get; set; }
        public string ots_gl_class { get; set; }
        public string ots_line_type { get; set; }
        public string ots_description_1 { get; set; }
        public string ots_description_2 { get; set; }
        public string ots_epc_value { get; set; }
        public string ots_special_handling_code { get; set; }
        public string id { get; set; }
        public string CONFIG_ID { get; set; }
        public string Keyed_name { get; set; }
        public string CREATED_ON { get; set; }
        public string CREATED_BY_ID { get; set; }
        public string MODIFIED_ON { get; set; }
        public string MODIFIED_BY_ID { get; set; }
        public string CURRENT_STATE { get; set; }
        public string PERMISSION_ID { get; set; }
        public string STATE { get; set; }
        public string IS_CURRENT { get; set; }
        public string MAJOR_REV { get; set; }
        public string MINOR_REV { get; set; }
        public string IS_RELEASED { get; set; }
        public string NOT_LOCKABLE { get; set; }
        public string GENERATION { get; set; }
        public string NEW_VERSION { get; set; }


        public string DataRow
        {
            get
            {
                return $"{id}\t{Keyed_name}\t{Classification}\t{ots_code}\t{ots_gl_class}\t{ots_line_type}\t{ots_description_1}\t{ots_description_2}\t{ots_epc_value}\t{ots_special_handling_code}\t{CONFIG_ID}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{CURRENT_STATE}\t{PERMISSION_ID}\t{STATE}\t{IS_CURRENT}\t{MAJOR_REV}\t{MINOR_REV}\t{IS_RELEASED}\t{NOT_LOCKABLE}\t{GENERATION}\t{NEW_VERSION}\n";
            }
        }

        public OtisUDCEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var ClassificationIndex = dataRow.IndexOf('\t');
            Classification = dataRow.Substring(0, ClassificationIndex);

            var ots_codeIndex = dataRow.IndexOf('\t', ClassificationIndex + 1);
            ots_code = dataRow.Substring(ClassificationIndex + 1, ots_codeIndex - ClassificationIndex - 1);

            var ots_gl_classIndex = dataRow.IndexOf('\t', ots_codeIndex + 1);
            ots_gl_class = dataRow.Substring(ots_codeIndex + 1, ots_gl_classIndex - ots_codeIndex - 1);

            var ots_line_typeIndex = dataRow.IndexOf('\t', ots_gl_classIndex + 1);
            ots_line_type = dataRow.Substring(ots_gl_classIndex + 1, ots_line_typeIndex - ots_gl_classIndex - 1);

            var ots_description_1Index = dataRow.IndexOf('\t', ots_line_typeIndex + 1);
            ots_description_1 = dataRow.Substring(ots_line_typeIndex + 1, ots_description_1Index - ots_line_typeIndex - 1);

            var ots_description_2Index = dataRow.IndexOf('\t', ots_description_1Index + 1);
            ots_description_2 = dataRow.Substring(ots_description_1Index + 1, ots_description_2Index - ots_description_1Index - 1);

            var ots_epc_valueIndex = dataRow.IndexOf('\t', ots_description_2Index + 1);
            ots_epc_value = dataRow.Substring(ots_description_2Index + 1, ots_epc_valueIndex - ots_description_2Index - 1);

            ots_special_handling_code = dataRow.Substring(ots_epc_valueIndex + 1);
        }
    }
}
