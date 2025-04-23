using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    public class ParameterEntity: IReadableEntity
    {
        public string OLDID { get; set; }
        public string Parameter_Name { get; set; }
        public string Parameter_Description { get; set; }
        public string Parameter_Type { get; set; }
        public string UOM { get; set; }
        public string ValueListOrRange { get; set; }
        public string DrawingNo { get; set; }

        public ParameterEntity(string dataRow)
        {
            SetProperties(dataRow);
        }

        public void SetProperties(string dataRow)
        {
            var fields = dataRow.Split('\t');

            if (fields.Length < 7)
            {
                throw new ArgumentException("Invalid data row: not enough fields");
            }

            OLDID = fields[0];
            Parameter_Name = fields[1];
            Parameter_Description = fields[2];
            Parameter_Type = fields[3];
            UOM = fields[4];
            ValueListOrRange = fields[5];
            DrawingNo = fields[6];
        }
    }
}

