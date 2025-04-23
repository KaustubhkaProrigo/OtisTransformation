using System;
using System.Collections.Generic;
using System.Text;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OTSProductEntity : IReadableEntity, IWritableEntity
    {
        public string Platform_Group { get; set; }
        public string Platform_No { get; set; }
        public string MR_MRL { get; set; }
        public string Belted_Roped { get; set; }
        public string Controller { get; set; }
        public string Underslung_Overslung { get; set; }
        public string Roping { get; set; }
        public string Product_No { get; set; }
        public string Description { get; set; }
        public string Product_Name { get; set; }
        public string COMPY_Region { get; set; }
        public string CODE { get; set; }
        public string DL_Duty_Load { get; set; }
        public string V_Speed { get; set; }
        public string R_Rise_Max { get; set; }
        public string CREATED_BY_ID { get; set; }
        public string ID { get; set; }
        public string CONFIG_ID { get; set; }
        public string KEYED_NAME { get; set; }
        public string ITEM_NUMBER { get; set; }
        public string NAME { get; set; }
        public string CREATED_ON { get; set; }
        public string CURRENT_STATE { get; set; }
        public string GENERATION { get; set; }
        public string IS_CURRENT { get; set; }
        public string IS_RELEASED { get; set; }
        public string MAJOR_REV { get; set; }
        public string MINOR_REV { get; set; }
        public string PERMISSION_ID { get; set; }
        public string STATE { get; set; }


        public string DataRow
        {
            get
            {

                return $"{ID}\t{CONFIG_ID}\t{KEYED_NAME}\t{ITEM_NUMBER}\t{NAME}\t{Platform_Group}\t{Platform_No}\t{MR_MRL}\t{Belted_Roped}\t{Controller}\t{Underslung_Overslung}\t{Roping}\t{Product_No}\t{Description}\t{Product_Name}\t{COMPY_Region}\t{CODE}\t{DL_Duty_Load}\t{V_Speed}\t{R_Rise_Max}\t{CREATED_BY_ID}\t{CREATED_ON}\t{CURRENT_STATE}\t{GENERATION}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{MINOR_REV}\t{PERMISSION_ID}\t{STATE}\n";

            }
        }


        public OTSProductEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var Platform_GroupIndex = dataRow.IndexOf('\t');
            Platform_Group = dataRow.Substring(0, Platform_GroupIndex);

            var Platform_NoIndex = dataRow.IndexOf('\t', Platform_GroupIndex + 1);
            Platform_No = dataRow.Substring(Platform_GroupIndex + 1, Platform_NoIndex - Platform_GroupIndex - 1);

            var MR_MRLIndex = dataRow.IndexOf('\t', Platform_NoIndex + 1);
            MR_MRL = dataRow.Substring(Platform_NoIndex + 1, MR_MRLIndex - Platform_NoIndex - 1);

            var Belted_RopedIndex = dataRow.IndexOf('\t', MR_MRLIndex + 1);
            Belted_Roped = dataRow.Substring(MR_MRLIndex + 1, Belted_RopedIndex - MR_MRLIndex - 1);

            var ControllerIndex = dataRow.IndexOf('\t', Belted_RopedIndex + 1);
            Controller = dataRow.Substring(Belted_RopedIndex + 1, ControllerIndex - Belted_RopedIndex - 1);

            var Underslung_OverslungIndex = dataRow.IndexOf('\t', ControllerIndex + 1);
            Underslung_Overslung = dataRow.Substring(ControllerIndex + 1, Underslung_OverslungIndex - ControllerIndex - 1);

            var RopingIndex = dataRow.IndexOf('\t', Underslung_OverslungIndex + 1);
            Roping = dataRow.Substring(Underslung_OverslungIndex + 1, RopingIndex - Underslung_OverslungIndex - 1);

            var Product_NoIndex = dataRow.IndexOf('\t', RopingIndex + 1);
            Product_No = dataRow.Substring(RopingIndex + 1, Product_NoIndex - RopingIndex - 1);

            var DescriptionIndex = dataRow.IndexOf('\t', Product_NoIndex + 1);
            Description = dataRow.Substring(Product_NoIndex + 1, DescriptionIndex - Product_NoIndex - 1);

            var Product_NameIndex = dataRow.IndexOf('\t', DescriptionIndex + 1);
            Product_Name = dataRow.Substring(DescriptionIndex + 1, Product_NameIndex - DescriptionIndex - 1);

            var COMPY_RegionIndex = dataRow.IndexOf('\t', Product_NameIndex + 1);
            COMPY_Region = dataRow.Substring(Product_NameIndex + 1, COMPY_RegionIndex - Product_NameIndex - 1);

            var CODEIndex = dataRow.IndexOf('\t', COMPY_RegionIndex + 1);
            CODE = dataRow.Substring(COMPY_RegionIndex + 1, CODEIndex - COMPY_RegionIndex - 1);

            var DL_Duty_LoadIndex = dataRow.IndexOf('\t', CODEIndex + 1);
            DL_Duty_Load = dataRow.Substring(CODEIndex + 1, DL_Duty_LoadIndex - CODEIndex - 1);

            var V_SpeedIndex = dataRow.IndexOf('\t', DL_Duty_LoadIndex + 1);
            V_Speed = dataRow.Substring(DL_Duty_LoadIndex + 1, V_SpeedIndex - DL_Duty_LoadIndex - 1);

            R_Rise_Max = dataRow.Substring(V_SpeedIndex + 1);

        }
    }
}
