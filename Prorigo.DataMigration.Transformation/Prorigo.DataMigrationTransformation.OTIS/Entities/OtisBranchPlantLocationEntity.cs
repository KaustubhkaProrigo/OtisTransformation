using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json.Linq;
using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisBranchPlantLocationEntity : IReadableEntity, IWritableEntity
    {
        public string Branch_Plant { get; set; }
        public string Date_Updated { get; set; }
        public string Location { get; set; }
        public string ConnectionID { get; set; }
        public string Config_id { get; set; }
        public string Created_by_id { get; set; }
        public string Created_on { get; set; }
        public string Modified_by_id { get; set; }
        public string Modified_on { get; set; }
        public string Source_id { get; set; }
        public string Permission_id { get; set; }
        public string is_current { get; set; }
        public string major_rev { get; set; }
        public string minor_rev { get; set; }
        public string is_released { get; set; }
        public string not_lockable { get; set; }
        public string generation { get; set; }
        public string new_version { get; set; }
        public string behavior { get; set; }


        public string DataRow
        {
            get
            {

                return $"{ConnectionID}\t{Config_id}\t{Source_id}\t{Branch_Plant}\t{Date_Updated}\t{Location}\t{Permission_id}\t{Created_by_id}\t{Created_on}\t{Modified_by_id}\t{Modified_on}\t{is_current}\t{major_rev}\t{minor_rev}\t{is_released}\t{not_lockable}\t{generation}\t{new_version}\t{behavior}\n";

            }
        }


        public OtisBranchPlantLocationEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var Branch_PlantIndex = dataRow.IndexOf('\t');
            Branch_Plant = dataRow.Substring(0, Branch_PlantIndex);

            var Date_UpdatedIndex = dataRow.IndexOf('\t', Branch_PlantIndex + 1);
            Date_Updated = dataRow.Substring(Branch_PlantIndex + 1, Date_UpdatedIndex - Branch_PlantIndex - 1);

            Location = dataRow.Substring(Date_UpdatedIndex + 1);
        }
    }
}
