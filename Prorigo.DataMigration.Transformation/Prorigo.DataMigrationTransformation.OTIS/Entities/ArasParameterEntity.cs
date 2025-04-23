using Prorigo.Plm.DataMigration.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class ArasParameterEntity: IWritableEntity
    {
        public string OLDID { get; set; }
        public string Parameter_Name { get; set; }
        public string Parameter_Description { get; set; }
        public string Parameter_Type { get; set; }
        public string UOM { get; set; }
        public string ValueListOrRange { get; set; }
        public string DrawingNo { get; set; }

        public string ID { get; set; }

        public string CLASSIFICATION { get; set; }
        public string CREATED_ON { get; set; }
        public string CREATED_BY_ID { get; set; }
        public string CONFIG_ID { get; set; }
        public string PERMISSION_ID { get; set; }
        public string OTS_DESCRIPTION { get; set; }
        public string OTS_FAMILY { get; set; }
        public string OTS_FUNCTIONAL_DESCRIPTION { get; set; }
        public string OTS_IS3DPARAMETER { get; set; }
        public string OTS_ISPREFERRED { get; set; }
        public string OTS_PARAMETER_TYPE { get; set; }
        public string OTS_UOM { get; set; }
        public string OTS_NAME { get; set; }

        public string DataRow
        {
            get
            {

                return $"{OLDID}\t{Parameter_Name}\t{Parameter_Description}\t{Parameter_Type}\t{UOM}\t{ValueListOrRange}\t{DrawingNo}\t{ID}\t{CLASSIFICATION}\t{CREATED_ON}\t{CREATED_BY_ID}\t{CONFIG_ID}\t{PERMISSION_ID}\t{OTS_DESCRIPTION}\t{OTS_FAMILY}\t{OTS_FUNCTIONAL_DESCRIPTION}\t{OTS_IS3DPARAMETER}\t{OTS_ISPREFERRED}\t{OTS_PARAMETER_TYPE}\t{OTS_UOM}\t{OTS_NAME}\n";
            }
        }
    }
}
