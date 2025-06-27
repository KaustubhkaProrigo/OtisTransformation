using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisBranchPlantLocationTransformedEntity : IReadableEntity, IWritableEntity
    {
        public string ConnectionID { get; set; }
        public string Config_id { get; set; }
        public string Source_id { get; set; }
        public string Branch_Plant { get; set; }
        public string Date_Updated { get; set; }
        public string Location { get; set; }
        public string Custom { get; set; }
        public string DataRow
        {
            get
            {
                return $"{ConnectionID}\t{Config_id}\t{Source_id}\t{Branch_Plant}\t{Date_Updated}\t{Location}\t{Custom}\n";
            }
        }
        public OtisBranchPlantLocationTransformedEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public void SetProperties(string dataRow)
        {
            var ConnectionIDIndex = dataRow.IndexOf('\t');
            ConnectionID = dataRow.Substring(0, ConnectionIDIndex);

            var Config_idIndex = dataRow.IndexOf('\t', ConnectionIDIndex + 1);
            Config_id = dataRow.Substring(ConnectionIDIndex + 1, Config_idIndex - ConnectionIDIndex - 1);

            var Source_idIndex = dataRow.IndexOf('\t', Config_idIndex + 1);
            Source_id = dataRow.Substring(Config_idIndex + 1, Source_idIndex - Config_idIndex - 1);

            var Branch_PlantIndex = dataRow.IndexOf('\t', Source_idIndex + 1);
            Branch_Plant = dataRow.Substring(Source_idIndex + 1, Branch_PlantIndex - Source_idIndex - 1);

            var Date_UpdatedIndex = dataRow.IndexOf('\t', Branch_PlantIndex + 1);
            Date_Updated = dataRow.Substring(Branch_PlantIndex + 1, Date_UpdatedIndex - Branch_PlantIndex - 1);

            var LocationIndex = dataRow.IndexOf('\t', Date_UpdatedIndex + 1);
            Location = dataRow.Substring(Date_UpdatedIndex + 1, LocationIndex - Date_UpdatedIndex - 1);

            Custom = dataRow.Substring(LocationIndex + 1);
        }
    }
}
