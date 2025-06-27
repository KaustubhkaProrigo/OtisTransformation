using System;
using System.Collections.Generic;
using System.Text;
using DocumentFormat.OpenXml.EMMA;
using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisBranchPlantEntity : IWritableEntity, IReadableEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Company_Code { get; set; }
        public string Company_Code_Description { get; set; }
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
                return $"{id}\t{Keyed_name}\t{Name}\t{Description}\t{Company_Code}\t{Company_Code_Description}\t{CONFIG_ID}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{CURRENT_STATE}\t{PERMISSION_ID}\t{STATE}\t{IS_CURRENT}\t{MAJOR_REV}\t{MINOR_REV}\t{IS_RELEASED}\t{NOT_LOCKABLE}\t{GENERATION}\t{NEW_VERSION}\n";
            }
        }

        public OtisBranchPlantEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var NameIndex = dataRow.IndexOf('\t');
            Name = dataRow.Substring(0, NameIndex);

            var DescriptionIndex = dataRow.IndexOf('\t', NameIndex + 1);
            Description = dataRow.Substring(NameIndex + 1, DescriptionIndex - NameIndex - 1);

            var Company_CodeIndex = dataRow.IndexOf('\t', DescriptionIndex + 1);
            Company_Code = dataRow.Substring(DescriptionIndex + 1, Company_CodeIndex - DescriptionIndex - 1);

            Company_Code_Description = dataRow.Substring(Company_CodeIndex + 1);
        }
    }
}
