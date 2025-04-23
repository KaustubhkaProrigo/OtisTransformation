using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class ArasDrawingEntity : IWritableEntity
    {

        public string DrawingNumber { get; set; }
        public string Revision { get; set; }
        public string Classification { get; set; }
        public string Description  { get; set; }
        public string  Type { get; set; }
        public string Origin_CN { get; set; }
        public string Origin_Date { get; set; }
        public string OriginalCreator { get; set; }
        public string OriginalReviewer { get; set; }



        public string ID { get; set; }
        public string config_id { get; set; }
        public string KEYED_NAME { get; set; }
        public string CREATED_ON { get; set; }
        public string CREATED_BY_ID { get; set; }
        public string MODIFIED_ON { get; set; }
        public string MODIFIED_BY_ID { get; set; }
        public string IS_CURRENT { get; set; }
        public string MAJOR_REV { get; set; }
        public string Permission_Id { get; set; }
        public string NAME { get; set; }
        public string ITEM_NUMBER { get; set; }
        public string Generation { get; set; }



        public string DataRow
        {
            get
            {
                return $"{DrawingNumber}\t{Revision}\t{Classification}\t{Description}\t{Type}\t{Origin_CN}\t{Origin_Date}\t{OriginalCreator}\t{OriginalReviewer}\t" +
                       $"{ID}\t{config_id}\t{KEYED_NAME}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{IS_CURRENT}\t{MAJOR_REV}\t{Permission_Id}\t{NAME}\t{ITEM_NUMBER}\t{Generation}\n";
            }
        }



        public ArasDrawingEntity()
        {
        }


    }
}