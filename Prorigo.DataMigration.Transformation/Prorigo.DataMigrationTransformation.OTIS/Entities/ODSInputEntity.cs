using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class ODSInputEntity : IWritableEntity, IReadableEntity
    {
        public string Rev { get; set; }
        public string Item { get; set; }
        public string Parameter_Name { get; set; }
        public string Parameter_Description { get; set; }
        public string Parameter_Type { get; set; }
        public string UOM { get; set; }
        public string Value { get; set; }
        public string ODSNo { get; set; }

        public string DataRow
        {
            get
            {
                return $"{Rev}\t{Item}\t{Parameter_Name}\t{Parameter_Description}\t{Parameter_Type}\t{UOM}\t{Value}\t{ODSNo}\n";
            }
        }

        public ODSInputEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {

            var RevIndex = dataRow.IndexOf('\t');
            Rev = dataRow.Substring(0, RevIndex);

            var ItemIndex = dataRow.IndexOf('\t', RevIndex + 1);
            Item = dataRow.Substring(RevIndex + 1, ItemIndex - RevIndex - 1);

            var Parameter_NameIndex = dataRow.IndexOf('\t', ItemIndex + 1);
            Parameter_Name = dataRow.Substring(ItemIndex + 1, Parameter_NameIndex - ItemIndex - 1);

            var Parameter_DescriptionIndex = dataRow.IndexOf('\t', Parameter_NameIndex + 1);
            Parameter_Description = dataRow.Substring(Parameter_NameIndex + 1, Parameter_DescriptionIndex - Parameter_NameIndex - 1);

            var ParameterTypeIndex = dataRow.IndexOf('\t', Parameter_DescriptionIndex + 1);
            Parameter_Type = dataRow.Substring(Parameter_DescriptionIndex + 1, ParameterTypeIndex - Parameter_DescriptionIndex - 1);

            var UOMIndex = dataRow.IndexOf('\t', ParameterTypeIndex + 1);
            UOM = dataRow.Substring(ParameterTypeIndex + 1, UOMIndex - ParameterTypeIndex - 1);

            var ValueIndex = dataRow.IndexOf('\t', UOMIndex + 1);
            Value = dataRow.Substring(UOMIndex + 1, ValueIndex - UOMIndex - 1);

            ODSNo = dataRow.Substring(ValueIndex + 1);

        }

    }
}
