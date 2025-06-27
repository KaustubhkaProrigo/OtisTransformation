using System;
using System.Collections.Generic;
using System.Text;
using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisSupplierEntity : IWritableEntity, IReadableEntity
    {
        public string Address_Number{ get; set; }
        public string Alpha_Name { get; set; }
        public string Sch_Typ { get; set; }
        public string Cat_Code_16 { get; set; }
        public string Cat_Code_20 { get; set; }
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
                return $"{id}\t{Keyed_name}\t{Address_Number}\t{Alpha_Name}\t{Sch_Typ}\t{Cat_Code_16}\t{Cat_Code_20}\t{CONFIG_ID}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{CURRENT_STATE}\t{PERMISSION_ID}\t{STATE}\t{IS_CURRENT}\t{MAJOR_REV}\t{MINOR_REV}\t{IS_RELEASED}\t{NOT_LOCKABLE}\t{GENERATION}\t{NEW_VERSION}\n";
            }
        }

        public OtisSupplierEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var Address_NumberIndex = dataRow.IndexOf('\t');
            Address_Number = dataRow.Substring(0, Address_NumberIndex);

            var Alpha_NameIndex = dataRow.IndexOf('\t', Address_NumberIndex + 1);
            Alpha_Name = dataRow.Substring(Address_NumberIndex + 1, Alpha_NameIndex - Address_NumberIndex - 1);

            var Sch_TypIndex = dataRow.IndexOf('\t', Alpha_NameIndex + 1);
            Sch_Typ = dataRow.Substring(Alpha_NameIndex + 1, Sch_TypIndex - Alpha_NameIndex - 1);

            var Cat_Code_16Index = dataRow.IndexOf('\t', Sch_TypIndex + 1);
            Cat_Code_16 = dataRow.Substring(Sch_TypIndex + 1, Cat_Code_16Index - Sch_TypIndex - 1);

            Cat_Code_20 = dataRow.Substring(Cat_Code_16Index + 1);
        }
    }
}
