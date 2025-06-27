using System;
using System.Collections.Generic;
using System.Text;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OTSProductEntity : IReadableEntity, IWritableEntity
    {
        public string Product_No { get; set; }
        public string Revision { get; set; }
        public string State { get; set; }
        public string Product_Name { get; set; }
        public string Classification { get; set; }
        public string Description { get; set; }
        public string Commercial_Product_Names { get; set; }
        public string Originator { get; set; }
        public string OWNED_BY_USER { get; set; }
        public string MODIFIED_ON { get; set; }
        public string RELEASE_DATE { get; set; }
        public string Originated_on { get; set; }
        public string COMPY_Region { get; set; }
        public string CODE { get; set; }
        public string MR_MRL { get; set; }
        public string Belted_Roped { get; set; }
        public string Controller { get; set; }
        public string Roping { get; set; }
        public string DL_Duty_Load { get; set; }
        public string V_Speed { get; set; }
        public string R_Rise_Max { get; set; }
        public string Underslung_Overslung { get; set; }
        public string Ots_Platform { get; set; }
        public string CREATED_BY_ID { get; set; }
        public string ARAS_UNIQUENESS_HELPER { get; set; }
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
        public string Modified_By_ID { get; set; }
        public string OTS_Product_Struct { get; set; }


        public string DataRow
        {
            get
            {
               return $"{ARAS_UNIQUENESS_HELPER}\t{ID}\t{CONFIG_ID}\t{KEYED_NAME}\t{Product_No}\t{Product_Name}\t{MR_MRL}\t{Belted_Roped}\t{Controller}\t{Underslung_Overslung}\t{Roping}\t{Description}\t{COMPY_Region}\t{CODE}\t{DL_Duty_Load}\t{V_Speed}\t{R_Rise_Max}\t{CREATED_BY_ID}\t{CREATED_ON}\t{CURRENT_STATE}\t{GENERATION}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{MINOR_REV}\t{PERMISSION_ID}\t{STATE}\t{Ots_Platform}\t{Commercial_Product_Names}\t{OTS_Product_Struct}\t";
            }
        }


        public OTSProductEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var Product_NoIndex = dataRow.IndexOf('\t');
            Product_No = dataRow.Substring(0, Product_NoIndex);

            var RevisionIndex = dataRow.IndexOf('\t', Product_NoIndex + 1);
            Revision = dataRow.Substring(Product_NoIndex + 1, RevisionIndex - Product_NoIndex - 1);

            var StateIndex = dataRow.IndexOf('\t', RevisionIndex + 1);
            State = dataRow.Substring(RevisionIndex + 1, StateIndex - RevisionIndex - 1);

            var Product_NameIndex = dataRow.IndexOf('\t', StateIndex + 1);
            Product_Name = dataRow.Substring(StateIndex + 1, Product_NameIndex - StateIndex - 1);

            var ClassificationIndex = dataRow.IndexOf('\t', Product_NameIndex + 1);
            Classification = dataRow.Substring(Product_NameIndex + 1, ClassificationIndex - Product_NameIndex - 1);

            var DescriptionIndex = dataRow.IndexOf('\t', ClassificationIndex + 1);
            Description = dataRow.Substring(ClassificationIndex + 1, DescriptionIndex - ClassificationIndex - 1);

            var Commercial_Product_NamesIndex = dataRow.IndexOf('\t', DescriptionIndex + 1);
            Commercial_Product_Names = dataRow.Substring(DescriptionIndex + 1, Commercial_Product_NamesIndex - DescriptionIndex - 1);

            var OriginatorIndex = dataRow.IndexOf('\t', Commercial_Product_NamesIndex + 1);
            Originator = dataRow.Substring(Commercial_Product_NamesIndex + 1, OriginatorIndex - Commercial_Product_NamesIndex - 1);

            var OWNED_BY_USERIndex = dataRow.IndexOf('\t', OriginatorIndex + 1);
            OWNED_BY_USER = dataRow.Substring(OriginatorIndex + 1, OWNED_BY_USERIndex - OriginatorIndex - 1);

            var MODIFIED_ONIndex = dataRow.IndexOf('\t', OWNED_BY_USERIndex + 1);
            MODIFIED_ON = dataRow.Substring(OWNED_BY_USERIndex + 1, MODIFIED_ONIndex - OWNED_BY_USERIndex - 1);

            var RELEASE_DATEIndex = dataRow.IndexOf('\t', MODIFIED_ONIndex + 1);
            RELEASE_DATE = dataRow.Substring(MODIFIED_ONIndex + 1, RELEASE_DATEIndex - MODIFIED_ONIndex - 1);

            var Originated_onIndex = dataRow.IndexOf('\t', RELEASE_DATEIndex + 1);
            Originated_on = dataRow.Substring(RELEASE_DATEIndex + 1, Originated_onIndex - RELEASE_DATEIndex - 1);

            var COMPY_RegionIndex = dataRow.IndexOf('\t', Originated_onIndex + 1);
            COMPY_Region = dataRow.Substring(Originated_onIndex + 1, COMPY_RegionIndex - Originated_onIndex - 1);

            var CODEIndex = dataRow.IndexOf('\t', COMPY_RegionIndex + 1);
            CODE = dataRow.Substring(COMPY_RegionIndex + 1, CODEIndex - COMPY_RegionIndex - 1);

            var MR_MRLIndex = dataRow.IndexOf('\t', CODEIndex + 1);
            MR_MRL = dataRow.Substring(CODEIndex + 1, MR_MRLIndex - CODEIndex - 1);

            var Belted_RopedIndex = dataRow.IndexOf('\t', MR_MRLIndex + 1);
            Belted_Roped = dataRow.Substring(MR_MRLIndex + 1, Belted_RopedIndex - MR_MRLIndex - 1);

            var ControllerIndex = dataRow.IndexOf('\t', Belted_RopedIndex + 1);
            Controller = dataRow.Substring(Belted_RopedIndex + 1, ControllerIndex - Belted_RopedIndex - 1);

            var RopingIndex = dataRow.IndexOf('\t', ControllerIndex + 1);
            Roping = dataRow.Substring(ControllerIndex + 1, RopingIndex - ControllerIndex - 1);

            var DL_Duty_LoadIndex = dataRow.IndexOf('\t', RopingIndex + 1);
            DL_Duty_Load = dataRow.Substring(RopingIndex + 1, DL_Duty_LoadIndex - RopingIndex - 1);

            var V_SpeedIndex = dataRow.IndexOf('\t', DL_Duty_LoadIndex + 1);
            V_Speed = dataRow.Substring(DL_Duty_LoadIndex + 1, V_SpeedIndex - DL_Duty_LoadIndex - 1);

            var R_Rise_MaxIndex = dataRow.IndexOf('\t', V_SpeedIndex + 1);
            R_Rise_Max = dataRow.Substring(V_SpeedIndex + 1, R_Rise_MaxIndex - V_SpeedIndex - 1);

            Underslung_Overslung = dataRow.Substring(R_Rise_MaxIndex + 1);

        }
    }
}
