using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisBranchPlantTSVEntity : IReadableEntity
    {
        public string id { get; set; }
        public string Keyed_name { get; set; }
        public string Name { get; set; }
        public string CustomProperties { get; set; }

        public OtisBranchPlantTSVEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var idIndex = dataRow.IndexOf('\t');
            id = dataRow.Substring(0, idIndex);

            var Keyed_nameIndex = dataRow.IndexOf('\t', idIndex + 1);
            Keyed_name = dataRow.Substring(idIndex + 1, Keyed_nameIndex - idIndex - 1);

            var NameIndex = dataRow.IndexOf('\t', Keyed_nameIndex + 1);
            Name = dataRow.Substring(Keyed_nameIndex + 1, NameIndex - Keyed_nameIndex - 1);

            CustomProperties = dataRow.Substring(NameIndex + 1);
        }

    }
}
