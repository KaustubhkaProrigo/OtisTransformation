using System;
using System.Collections.Generic;
using System.Text;
using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisBuyerPlannerEntity : IWritableEntity, IReadableEntity
    {
        public string Address_Number { get; set; }
        public string Alpha_Name { get; set; }
        public string Long_Address { get; set; }
        public string Sch_Typ { get; set; }
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
                return $"{id}\t{Keyed_name}\t{Address_Number}\t{Alpha_Name}\t{Long_Address}\t{Sch_Typ}\t{CONFIG_ID}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{CURRENT_STATE}\t{PERMISSION_ID}\t{STATE}\t{IS_CURRENT}\t{MAJOR_REV}\t{MINOR_REV}\t{IS_RELEASED}\t{NOT_LOCKABLE}\t{GENERATION}\t{NEW_VERSION}\n";
            }
        }

        public OtisBuyerPlannerEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var Address_NumberIndex = dataRow.IndexOf('\t');
            Address_Number = dataRow.Substring(0, Address_NumberIndex);

            var Alpha_NameIndex = dataRow.IndexOf('\t', Address_NumberIndex + 1);
            Alpha_Name = dataRow.Substring(Address_NumberIndex + 1, Alpha_NameIndex - Address_NumberIndex - 1);

            var Long_AddressIndex = dataRow.IndexOf('\t', Alpha_NameIndex + 1);
            Long_Address = dataRow.Substring(Alpha_NameIndex + 1, Long_AddressIndex - Alpha_NameIndex - 1);

            Sch_Typ = dataRow.Substring(Long_AddressIndex + 1);
        }
    }
}
