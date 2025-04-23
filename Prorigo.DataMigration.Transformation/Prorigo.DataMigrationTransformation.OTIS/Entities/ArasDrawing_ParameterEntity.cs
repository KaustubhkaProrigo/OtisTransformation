using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
class ArasDrawing_ParameterEntity : IWritableEntity
{

        public string connectionID { get; set; }
        public string FromID { get; set; }
       public string TOID { get; set; }
        public string BEHAVIOR { get; set; }
        public string IS_CURRENT { get; set; }
        public string IS_RELEASED { get; set; }
        public string Permission_Id { get; set; }
        public string NOT_LOCKABLE { get; set; }
        public string Generation { get; set; }



        public string DataRow
    {
        get
        {
                return $"{connectionID}\t{FromID}\t{TOID}\t{BEHAVIOR}\t{IS_CURRENT}\t{IS_RELEASED}\t{Permission_Id}\t{NOT_LOCKABLE}\t{Generation}\n";
            }
        }


    public ArasDrawing_ParameterEntity()
    {
    }


}
}

