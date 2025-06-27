using System;
using System.Collections.Generic;
using System.Text;
using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisVM_BreakdownItemEntity : IWritableEntity, IReadableEntity
    {
        public string Type { get; set; }
        public string Data_Number { get; set; }
        public string Name { get; set; }
        public string Windchill_Revision { get; set; }
        public string id { get; set; }
        public string ARAS_UNIQUENESS_HELPER { get; set; }
        public string KEYED_NAME { get; set; }
        public string CREATED_ON { get; set; }
        public string CREATED_BY_ID { get; set; }
        public string MODIFIED_ON { get; set; }
        public string MODIFIED_BY_ID { get; set; }
        public string CURRENT_STATE { get; set; }
        public string STATE { get; set; }
        public string IS_CURRENT { get; set; }
        public string MAJOR_REV { get; set; }
        public string MINOR_REV { get; set; }
        public string IS_RELEASED { get; set; }
        public string NOT_LOCKABLE { get; set; }
        public string GENERATION { get; set; }
        public string NEW_VERSION { get; set; }
        public string CONFIG_ID { get; set; }
        public string PERMISSION_ID { get; set; }
        public string DESCRIPTION { get; set; }
      

        public string DataRow
        {
            get
            {
                return $"{id}\t{ARAS_UNIQUENESS_HELPER}\t{KEYED_NAME}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{CURRENT_STATE}\t{STATE}\t{IS_CURRENT}\t{MAJOR_REV}\t{MINOR_REV}\t{IS_RELEASED}\t{NOT_LOCKABLE}\t{GENERATION}\t{NEW_VERSION}\t{CONFIG_ID}\t{PERMISSION_ID}\t{DESCRIPTION}\t{Type}\t{Data_Number}\t{Name}\n";
            }
        }

        public OtisVM_BreakdownItemEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var TypeIndex = dataRow.IndexOf('\t');
            Type = dataRow.Substring(0, TypeIndex);

            var Data_NumberIndex = dataRow.IndexOf('\t', TypeIndex + 1);
            Data_Number = dataRow.Substring(TypeIndex + 1, Data_NumberIndex - TypeIndex - 1);

            var NameIndex = dataRow.IndexOf('\t', Data_NumberIndex + 1);
            Name = dataRow.Substring(Data_NumberIndex + 1, NameIndex - Data_NumberIndex - 1);

            Windchill_Revision = dataRow.Substring(NameIndex + 1);
        }
    }
}
