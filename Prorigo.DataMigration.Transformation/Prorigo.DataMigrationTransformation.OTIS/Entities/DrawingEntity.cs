using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class DrawingEntity :IReadableEntity
    {
        public string DrawingNumber { get; set; }
        public string Revision { get; set; }
        public string Classification { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public string Origin_CN { get; set; }
        public string Origin_Date { get; set; }
        public string OriginalCreator { get; set; }
        public string OriginalReviewer { get; set; }


        public DrawingEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var fields = dataRow.Split('\t');

            if (fields.Length < 8)
            {
                throw new System.ArgumentException("Invalid data row: not enough fields");
            }

            DrawingNumber = fields[0];
            Revision = fields[1];
            Classification = fields[2];
            Description  = fields[3];
            Type = fields[4];
            Origin_CN = fields[5];
            Origin_Date = fields[6];
            OriginalCreator = fields[7];
            OriginalReviewer = fields[8];
        }

        
    }
}
