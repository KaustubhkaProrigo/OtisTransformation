using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json.Linq;
using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisProductPlatformRelEntity : IReadableEntity
    {
        public string Platform_No { get; set; }
        public string Platform_Revision { get; set; }
        public string Platform_Clasification { get; set; }
        public string Product_Number { get; set; }
        public string Revision { get; set; }
        public string Classification { get; set; }


        public OtisProductPlatformRelEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var Platform_NoIndex = dataRow.IndexOf('\t');
            Platform_No = dataRow.Substring(0, Platform_NoIndex);

            var Platform_RevisionIndex = dataRow.IndexOf('\t', Platform_NoIndex + 1);
            Platform_Revision = dataRow.Substring(Platform_NoIndex + 1, Platform_RevisionIndex - Platform_NoIndex - 1);

            var Platform_ClasificationIndex = dataRow.IndexOf('\t', Platform_RevisionIndex + 1);
            Platform_Clasification = dataRow.Substring(Platform_RevisionIndex + 1, Platform_ClasificationIndex - Platform_RevisionIndex - 1);

            var Product_NumberIndex = dataRow.IndexOf('\t', Platform_ClasificationIndex + 1);
            Product_Number = dataRow.Substring(Platform_ClasificationIndex + 1, Product_NumberIndex - Platform_ClasificationIndex - 1);

            var RevisionIndex = dataRow.IndexOf('\t', Product_NumberIndex + 1);
            Revision = dataRow.Substring(Product_NumberIndex + 1, RevisionIndex - Product_NumberIndex - 1);

            Classification = dataRow.Substring(RevisionIndex + 1);
        }
    }
}
