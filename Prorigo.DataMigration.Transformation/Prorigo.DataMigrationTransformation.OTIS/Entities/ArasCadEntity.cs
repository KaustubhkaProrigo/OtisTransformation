using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class ArasCadEntity : IReadableEntity
    {
        public string ID { get; set; }
        public string ItemNumber { get; set; }
        public string OtsRevision { get; set; }


        public ArasCadEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var ARASUNIQUENESSHELPER = dataRow.IndexOf('\t');

            var IdIndex = dataRow.IndexOf('\t', ARASUNIQUENESSHELPER + 1);
            ID = dataRow.Substring(ARASUNIQUENESSHELPER + 1, IdIndex - ARASUNIQUENESSHELPER - 1);

            var configIdIndex = dataRow.IndexOf('\t', IdIndex + 1);
            var keyednameIndex = dataRow.IndexOf('\t', configIdIndex + 1);

            var ItemnumberIndex = dataRow.IndexOf('\t', keyednameIndex + 1);
            ItemNumber = dataRow.Substring(keyednameIndex + 1, ItemnumberIndex - keyednameIndex - 1);

            var nameIndex = dataRow.IndexOf('\t', ItemnumberIndex + 1);
            var classificationIndex = dataRow.IndexOf('\t', nameIndex + 1);
            var AuthoringToolIndex = dataRow.IndexOf('\t', classificationIndex + 1);
            var DescriptionIndex = dataRow.IndexOf('\t', AuthoringToolIndex + 1);
            var createdbyidIndex = dataRow.IndexOf('\t', DescriptionIndex + 1);
            var createdonIndex = dataRow.IndexOf('\t', createdbyidIndex + 1);
            var currentstateIndex = dataRow.IndexOf('\t', createdonIndex + 1);
            var generationIndex = dataRow.IndexOf('\t', currentstateIndex + 1);
            var iscurrentIndex = dataRow.IndexOf('\t', generationIndex + 1);
            var isreleasedIndex = dataRow.IndexOf('\t', iscurrentIndex + 1);
            var majorrevIndex = dataRow.IndexOf('\t', isreleasedIndex + 1);
            var minorrevIndex = dataRow.IndexOf('\t', majorrevIndex + 1);
            var permissionidIndex = dataRow.IndexOf('\t', minorrevIndex + 1);
            var stateIndex = dataRow.IndexOf('\t', permissionidIndex + 1);
            
            OtsRevision = dataRow.Substring(stateIndex + 1);
        }
    }
}
