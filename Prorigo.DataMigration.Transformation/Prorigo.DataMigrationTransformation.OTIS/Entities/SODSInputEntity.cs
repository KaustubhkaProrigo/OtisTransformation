using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class SODSInputEntity : IWritableEntity, IReadableEntity
    {
        public string ID { get; set; }
        public string ParameterName { get; set; }
        public string ParameterDescription { get; set; }
        public string ParameterType { get; set; }
        public string UOM { get; set; }
        public string ValueList { get; set; }
        public string SODSNO { get; set; }

        public string DataRow
        {
            get
            {
                return $"{ID}\t{ParameterName}\t{ParameterDescription}\t{ParameterType}\t{UOM}\t{ValueList}\t{SODSNO}\n";
            }
        }

        public SODSInputEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {

            var IDIndex = dataRow.IndexOf('\t');
            ID = dataRow.Substring(0, IDIndex);

            var ParameterNameIndex = dataRow.IndexOf('\t', IDIndex + 1);
            ParameterName = dataRow.Substring(IDIndex + 1, ParameterNameIndex - IDIndex - 1);

            var ParameterDescriptionIndex = dataRow.IndexOf('\t', ParameterNameIndex + 1);
            ParameterDescription = dataRow.Substring(ParameterNameIndex + 1, ParameterDescriptionIndex - ParameterNameIndex - 1);

            var ParameterTypeIndex = dataRow.IndexOf('\t', ParameterDescriptionIndex + 1);
            ParameterType = dataRow.Substring(ParameterDescriptionIndex + 1, ParameterTypeIndex - ParameterDescriptionIndex - 1);

            var UOMIndex = dataRow.IndexOf('\t', ParameterTypeIndex + 1);
            UOM = dataRow.Substring(ParameterTypeIndex + 1, UOMIndex - ParameterTypeIndex - 1);

            var ValueListIndex = dataRow.IndexOf('\t', UOMIndex + 1);
            ValueList = dataRow.Substring(UOMIndex + 1, ValueListIndex - UOMIndex - 1);

            SODSNO = dataRow.Substring(ValueListIndex + 1);

        }

    }
}
