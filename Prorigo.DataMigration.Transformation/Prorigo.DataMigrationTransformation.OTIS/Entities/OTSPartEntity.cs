using System;
using System.Collections.Generic;
using System.Text;
using DocumentFormat.OpenXml.Office2010.Excel;
using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OTSPartEntity : IReadableEntity, IWritableEntity
    {
        public string Part_Number { get; set; }
        public string Part_Name { get; set; }
        public string Description { get; set; }
        public string Classification { get; set; }
        public string Part_Type { get; set; }
        public string Sub_Type { get; set; }
        public string Weight { get; set; }
        public string Make_Buy { get; set; }
        public string UOM { get; set; }
        public string CREATED_BY_ID { get; set; }
        public string CREATED_ON { get; set; }
        public string MODIFIED_BY_ID { get; set; }
        public string MODIFIED_ON { get; set; }
        public string STATE { get; set; }
        public string xClassification { get; set; }
        public string Unit { get; set; }
        public string Assembly_Mode { get; set; }
        public string Material { get; set; }
        public string Serviceable { get; set; }
        public string U_S_Jurisdiction { get; set; }
        public string U_S_ECCN { get; set; }
        public string U_S_Category { get; set; }
        public string U_S_Rationale { get; set; }
        public string U_S_Source { get; set; }
        public string U_S_Classifiers_Email { get; set; }
        public string U_S_Date_Classified { get; set; }
        public string Chinese_Description { get; set; }
        public string French_Description { get; set; }
        public string Japanese_Description { get; set; }


        //public string Assembly { get; set; }
        //public string Service_Kit_Number { get; set; }
        //public string Source { get; set; }
        //public string Otis_Non_Otis { get; set; }
        //public string Vendor_Code { get; set; }
        //public string Vendor_Name { get; set; }
        //public string Vendor_Address { get; set; }
        //public string Vendor_Part_Number { get; set; }
        //public string Vendor_Part_Description { get; set; }
        public string Effectivity_Date { get; set; }

        public string ARAS_UNIQUENESS_HELPER { get; set; }
        public string ID { get; set; }
        public string CONFIG_ID { get; set; }
        public string KEYED_NAME { get; set; }
        public string CURRENT_STATE { get; set; }
        public string GENERATION { get; set; }
        public string IS_CURRENT { get; set; }
        public string IS_RELEASED { get; set; }
        public string MAJOR_REV { get; set; }
        public string MINOR_REV { get; set; }
        
        public string PERMISSION_ID { get; set; }
        
        public string Ots_Image {get; set;}
        

        public string DataRow
        {
            get
            {

                return $"{ARAS_UNIQUENESS_HELPER}\t{ID}\t{CONFIG_ID}\t{KEYED_NAME}\t{Part_Number}\t{Part_Name}\t{Description}\t{Classification}\t{Part_Type}\t{Sub_Type}\t{UOM}\t{Effectivity_Date}\t{Assembly_Mode}\t{Unit}\t{Material}\t{Weight}\t{Serviceable}\t{U_S_Jurisdiction}\t{U_S_ECCN}\t{U_S_Category}\t{U_S_Rationale}\t{U_S_Source}\t{U_S_Classifiers_Email}\t{U_S_Date_Classified}\t{Chinese_Description}\t{French_Description}\t{Japanese_Description}\t{CREATED_BY_ID}\t{CREATED_ON}\t{CURRENT_STATE}\t{GENERATION}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{MINOR_REV}\t{MODIFIED_ON}\t{PERMISSION_ID}\t{Make_Buy}\t{STATE}\t{MAJOR_REV}\n";

            }
        }


        public OTSPartEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var Part_NumberIndex = dataRow.IndexOf('\t');
            Part_Number = dataRow.Substring(0, Part_NumberIndex);

            var Part_NameIndex = dataRow.IndexOf('\t', Part_NumberIndex + 1);
            Part_Name = dataRow.Substring(Part_NumberIndex + 1, Part_NameIndex - Part_NumberIndex - 1);

            var DescriptionIndex = dataRow.IndexOf('\t', Part_NameIndex + 1);
            Description = dataRow.Substring(Part_NameIndex + 1, DescriptionIndex - Part_NameIndex - 1);

            var ClassificationIndex = dataRow.IndexOf('\t', DescriptionIndex + 1);
            Classification = dataRow.Substring(DescriptionIndex + 1, ClassificationIndex - DescriptionIndex - 1);

            var Part_TypeIndex = dataRow.IndexOf('\t', ClassificationIndex + 1);
            Part_Type = dataRow.Substring(ClassificationIndex + 1, Part_TypeIndex - ClassificationIndex - 1);

            var Sub_TypeIndex = dataRow.IndexOf('\t', Part_TypeIndex + 1);
            Sub_Type = dataRow.Substring(Part_TypeIndex + 1, Sub_TypeIndex - Part_TypeIndex - 1);

            var WeightIndex = dataRow.IndexOf('\t', Sub_TypeIndex + 1);
            Weight = dataRow.Substring(Sub_TypeIndex + 1, WeightIndex - Sub_TypeIndex - 1);

            var Make_BuyIndex = dataRow.IndexOf('\t', WeightIndex + 1);
            Make_Buy = dataRow.Substring(WeightIndex + 1, Make_BuyIndex - WeightIndex - 1);

            var UOMIndex = dataRow.IndexOf('\t', Make_BuyIndex + 1);
            UOM = dataRow.Substring(Make_BuyIndex + 1, UOMIndex - Make_BuyIndex - 1);

            var CREATED_BY_IDIndex = dataRow.IndexOf('\t', UOMIndex + 1);
            CREATED_BY_ID = dataRow.Substring(UOMIndex + 1, CREATED_BY_IDIndex - UOMIndex - 1);

            var CREATED_ONIndex = dataRow.IndexOf('\t', CREATED_BY_IDIndex + 1);
            CREATED_ON = dataRow.Substring(CREATED_BY_IDIndex + 1, CREATED_ONIndex - CREATED_BY_IDIndex - 1);

            var MODIFIED_BY_IDIndex = dataRow.IndexOf('\t', CREATED_ONIndex + 1);
            MODIFIED_BY_ID = dataRow.Substring(CREATED_ONIndex + 1, MODIFIED_BY_IDIndex - CREATED_ONIndex - 1);

            var MODIFIED_ONIndex = dataRow.IndexOf('\t', MODIFIED_BY_IDIndex + 1);
            MODIFIED_ON = dataRow.Substring(MODIFIED_BY_IDIndex + 1, MODIFIED_ONIndex - MODIFIED_BY_IDIndex - 1);

            var STATEIndex = dataRow.IndexOf('\t', MODIFIED_ONIndex + 1);
            STATE = dataRow.Substring(MODIFIED_ONIndex + 1, STATEIndex - MODIFIED_ONIndex - 1);

            var xClassificationIndex = dataRow.IndexOf('\t', STATEIndex + 1);
            xClassification = dataRow.Substring(STATEIndex + 1, xClassificationIndex - STATEIndex - 1);

            var UnitIndex = dataRow.IndexOf('\t', xClassificationIndex + 1);
            Unit = dataRow.Substring(xClassificationIndex + 1, UnitIndex - xClassificationIndex - 1);

            var Assembly_ModeIndex = dataRow.IndexOf('\t', UnitIndex + 1);
            Assembly_Mode = dataRow.Substring(UnitIndex + 1, Assembly_ModeIndex - UnitIndex - 1);

            var MaterialIndex = dataRow.IndexOf('\t', Assembly_ModeIndex + 1);
            Material = dataRow.Substring(Assembly_ModeIndex + 1, MaterialIndex - Assembly_ModeIndex - 1);

            var ServiceableIndex = dataRow.IndexOf('\t', MaterialIndex + 1);
            Serviceable = dataRow.Substring(MaterialIndex + 1, ServiceableIndex - MaterialIndex - 1);

            var U_S_JurisdictionIndex = dataRow.IndexOf('\t', ServiceableIndex + 1);
            U_S_Jurisdiction = dataRow.Substring(ServiceableIndex + 1, U_S_JurisdictionIndex - ServiceableIndex - 1);

            var U_S_ECCNIndex = dataRow.IndexOf('\t', U_S_JurisdictionIndex + 1);
            U_S_ECCN = dataRow.Substring(U_S_JurisdictionIndex + 1, U_S_ECCNIndex - U_S_JurisdictionIndex - 1);

            var U_S_CategoryIndex = dataRow.IndexOf('\t', U_S_ECCNIndex + 1);
            U_S_Category = dataRow.Substring(U_S_ECCNIndex + 1, U_S_CategoryIndex - U_S_ECCNIndex - 1);

            var U_S_RationaleIndex = dataRow.IndexOf('\t', U_S_CategoryIndex + 1);
            U_S_Rationale = dataRow.Substring(U_S_CategoryIndex + 1, U_S_RationaleIndex - U_S_CategoryIndex - 1);

            var U_S_SourceIndex = dataRow.IndexOf('\t', U_S_RationaleIndex + 1);
            U_S_Source = dataRow.Substring(U_S_RationaleIndex + 1, U_S_SourceIndex - U_S_RationaleIndex - 1);

            var U_S_Classifiers_EmailIndex = dataRow.IndexOf('\t', U_S_SourceIndex + 1);
            U_S_Classifiers_Email = dataRow.Substring(U_S_SourceIndex + 1, U_S_Classifiers_EmailIndex - U_S_SourceIndex - 1);

            var U_S_Date_ClassifiedIndex = dataRow.IndexOf('\t', U_S_Classifiers_EmailIndex + 1);
            U_S_Date_Classified = dataRow.Substring(U_S_Classifiers_EmailIndex + 1, U_S_Date_ClassifiedIndex - U_S_Classifiers_EmailIndex - 1);

            var Chinese_DescriptionIndex = dataRow.IndexOf('\t', U_S_Date_ClassifiedIndex + 1);
            Chinese_Description = dataRow.Substring(U_S_Date_ClassifiedIndex + 1, Chinese_DescriptionIndex - U_S_Date_ClassifiedIndex - 1);

            var French_DescriptionIndex = dataRow.IndexOf('\t', Chinese_DescriptionIndex + 1);
            French_Description = dataRow.Substring(Chinese_DescriptionIndex + 1, French_DescriptionIndex - Chinese_DescriptionIndex - 1);

            Japanese_Description = dataRow.Substring(French_DescriptionIndex + 1);

        }
    }
}
