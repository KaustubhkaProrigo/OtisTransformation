using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisInputOutputEntity : IReadableEntity, IWritableEntity
    {
        public string ID { get; set; }
        public string Parameter_Name { get; set; }
        public string Parameter_Description { get; set; }
        public string Parameter_Type { get; set; }
        public string UOM { get; set; }
        public string ValueList_or_ValueRange { get; set; }
        public string DrawingNo { get; set; }
        public string DataRow
        {
            get
            {
                return $"{ID}\t{Parameter_Name}\t{Parameter_Description}\t{Parameter_Type}\t{UOM}\t{ValueList_or_ValueRange}\t{DrawingNo}\n";
            }
        }
        public OtisInputOutputEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public void SetProperties(string dataRow)
        {
            var IDIndex = dataRow.IndexOf('\t');
            ID = dataRow.Substring(0, IDIndex);

            var Parameter_NamerIndex = dataRow.IndexOf('\t', IDIndex + 1);
            Parameter_Name = dataRow.Substring(IDIndex + 1, Parameter_NamerIndex - IDIndex - 1);

            var Parameter_DescriptionIndex = dataRow.IndexOf('\t', Parameter_NamerIndex + 1);
            Parameter_Description = dataRow.Substring(Parameter_NamerIndex + 1, Parameter_DescriptionIndex - Parameter_NamerIndex - 1);

            var Parameter_TypeIndex = dataRow.IndexOf('\t', Parameter_DescriptionIndex + 1);
            Parameter_Type = dataRow.Substring(Parameter_DescriptionIndex + 1, Parameter_TypeIndex - Parameter_DescriptionIndex - 1);

            var UOMIndex = dataRow.IndexOf('\t', Parameter_TypeIndex + 1);
            UOM = dataRow.Substring(Parameter_TypeIndex + 1, UOMIndex - Parameter_TypeIndex - 1);

            var ValueList_or_ValueRangeIndex = dataRow.IndexOf('\t', UOMIndex + 1);
            ValueList_or_ValueRange = dataRow.Substring(UOMIndex + 1, ValueList_or_ValueRangeIndex - UOMIndex - 1);

            DrawingNo = dataRow.Substring(ValueList_or_ValueRangeIndex + 1);
        }
    }
}
