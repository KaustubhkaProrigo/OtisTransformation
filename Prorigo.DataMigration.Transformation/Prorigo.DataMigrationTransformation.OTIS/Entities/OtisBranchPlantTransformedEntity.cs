using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisBranchPlantTransformedEntity : IReadableEntity, IWritableEntity
    {
        public string id { get; set; }
        public string Keyed_name { get; set; }
        public string Custom { get; set; }
        public string DataRow
        {
            get
            {
                return $"{id}\t{Keyed_name}\t{Custom}\n";
            }
        }
        public OtisBranchPlantTransformedEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public void SetProperties(string dataRow)
        {
            var idIndex = dataRow.IndexOf('\t');
            id = dataRow.Substring(0, idIndex);

            var Keyed_nameIndex = dataRow.IndexOf('\t', idIndex + 1);
            Keyed_name = dataRow.Substring(idIndex + 1, Keyed_nameIndex - idIndex - 1);

            Custom = dataRow.Substring(Keyed_nameIndex + 1);
        }
    }
}
