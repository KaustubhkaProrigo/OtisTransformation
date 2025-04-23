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
        public string Long_Description { get; set; }
        public string Classification { get; set; }
        public string Part_Type { get; set; }
        public string Assembly { get; set; }
        public string Assembly_Mode { get; set; }
        public string Unit { get; set; }
        public string Material { get; set; }
        public string Weight { get; set; }
        public string Serviceable { get; set; }
        public string Service_Kit_Number { get; set; }
        public string Source { get; set; }
        public string Otis_Non_Otis { get; set; }
        public string Vendor_Code { get; set; }
        public string Vendor_Name { get; set; }
        public string Vendor_Address { get; set; }
        public string Vendor_Part_Number { get; set; }
        public string Vendor_Part_Description { get; set; }
        public string Effectivity_Date { get; set; }
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
        public string CREATED_BY_ID { get; set; }
        public string ARAS_UNIQUENESS_HELPER { get; set; }
        public string ID { get; set; }
        public string CONFIG_ID { get; set; }
        public string KEYED_NAME { get; set; }
        public string CREATED_ON { get; set; }
        public string CURRENT_STATE { get; set; }
        public string GENERATION { get; set; }
        public string IS_CURRENT { get; set; }
        public string IS_RELEASED { get; set; }
        public string MAJOR_REV { get; set; }
        public string MINOR_REV { get; set; }
        public string MODIFIED_ON { get; set; }
        public string PERMISSION_ID { get; set; }
        public string STATE { get; set; }
        public string Ots_Image {get; set;}
        public string Make_Buy { get; set; }

        public string DataRow
        {
            get
            {

                return $"{ARAS_UNIQUENESS_HELPER}\t{ID}\t{CONFIG_ID}\t{KEYED_NAME}\t{Part_Number}\t{Part_Name}\t{Long_Description}\t{Part_Type}\t{Assembly_Mode}\t{Unit}\t{Material}\t{Weight}\t{Serviceable}\t{Service_Kit_Number}\t{Source}\t{Otis_Non_Otis}\t{Effectivity_Date}\t{U_S_Jurisdiction}\t{U_S_ECCN}\t{U_S_Category}\t{U_S_Rationale}\t{U_S_Source}\t{U_S_Classifiers_Email}\t{U_S_Date_Classified}\t{Chinese_Description}\t{French_Description}\t{Japanese_Description}\t{CREATED_BY_ID}\t{CREATED_ON}\t{CURRENT_STATE}\t{GENERATION}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{MINOR_REV}\t{MODIFIED_ON}\t{PERMISSION_ID}\t{Make_Buy}\t{STATE}\n";

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

            var Long_DescriptionIndex = dataRow.IndexOf('\t', Part_NameIndex + 1);
            Long_Description = dataRow.Substring(Part_NameIndex + 1, Long_DescriptionIndex - Part_NameIndex - 1);

            var ClassificationIndex = dataRow.IndexOf('\t', Long_DescriptionIndex + 1);
            Classification = dataRow.Substring(Long_DescriptionIndex + 1, ClassificationIndex - Long_DescriptionIndex - 1);

            var Part_TypeIndex = dataRow.IndexOf('\t', ClassificationIndex + 1);
            Part_Type = dataRow.Substring(ClassificationIndex + 1, Part_TypeIndex - ClassificationIndex - 1);

            var AssemblyIndex = dataRow.IndexOf('\t', Part_TypeIndex + 1);
            Assembly = dataRow.Substring(Part_TypeIndex + 1, AssemblyIndex - Part_TypeIndex - 1);

            var Assembly_ModeIndex = dataRow.IndexOf('\t', AssemblyIndex + 1);
            Assembly_Mode = dataRow.Substring(AssemblyIndex + 1, Assembly_ModeIndex - AssemblyIndex - 1);

            var UnitIndex = dataRow.IndexOf('\t', Assembly_ModeIndex + 1);
            Unit = dataRow.Substring(Assembly_ModeIndex + 1, UnitIndex - Assembly_ModeIndex - 1);

            var MaterialIndex = dataRow.IndexOf('\t', UnitIndex + 1);
            Material = dataRow.Substring(UnitIndex + 1, MaterialIndex - UnitIndex - 1);

            var WeightIndex = dataRow.IndexOf('\t', MaterialIndex + 1);
            Weight = dataRow.Substring(MaterialIndex + 1, WeightIndex - MaterialIndex - 1);

            var ServiceableIndex = dataRow.IndexOf('\t', WeightIndex + 1);
            Serviceable = dataRow.Substring(WeightIndex + 1, ServiceableIndex - WeightIndex - 1);

            var Service_Kit_NumberIndex = dataRow.IndexOf('\t', ServiceableIndex + 1);
            Service_Kit_Number = dataRow.Substring(ServiceableIndex + 1, Service_Kit_NumberIndex - ServiceableIndex - 1);

            var SourceIndex = dataRow.IndexOf('\t', Service_Kit_NumberIndex + 1);
            Source = dataRow.Substring(Service_Kit_NumberIndex + 1, SourceIndex - Service_Kit_NumberIndex - 1);

            var Otis_Non_OtisIndex = dataRow.IndexOf('\t', SourceIndex + 1);
            Otis_Non_Otis = dataRow.Substring(SourceIndex + 1, Otis_Non_OtisIndex - SourceIndex - 1);

            var Vendor_CodeIndex = dataRow.IndexOf('\t', Otis_Non_OtisIndex + 1);
            Vendor_Code = dataRow.Substring(Otis_Non_OtisIndex + 1, Vendor_CodeIndex - Otis_Non_OtisIndex - 1);

            var Vendor_NameIndex = dataRow.IndexOf('\t', Vendor_CodeIndex + 1);
            Vendor_Name = dataRow.Substring(Vendor_CodeIndex + 1, Vendor_NameIndex - Vendor_CodeIndex - 1);

            var Vendor_AddressIndex = dataRow.IndexOf('\t', Vendor_NameIndex + 1);
            Vendor_Address = dataRow.Substring(Vendor_NameIndex + 1, Vendor_AddressIndex - Vendor_NameIndex - 1);

            var Vendor_Part_NumberIndex = dataRow.IndexOf('\t', Vendor_AddressIndex + 1);
            Vendor_Part_Number = dataRow.Substring(Vendor_AddressIndex + 1, Vendor_Part_NumberIndex - Vendor_AddressIndex - 1);

            var Vendor_Part_DescriptionIndex = dataRow.IndexOf('\t', Vendor_Part_NumberIndex + 1);
            Vendor_Part_Description = dataRow.Substring(Vendor_Part_NumberIndex + 1, Vendor_Part_DescriptionIndex - Vendor_Part_NumberIndex - 1);

            var Effectivity_DateIndex = dataRow.IndexOf('\t', Vendor_Part_DescriptionIndex + 1);
            Effectivity_Date = dataRow.Substring(Vendor_Part_DescriptionIndex + 1, Effectivity_DateIndex - Vendor_Part_DescriptionIndex - 1);

            var U_S_JurisdictionIndex = dataRow.IndexOf('\t', Effectivity_DateIndex + 1);
            U_S_Jurisdiction = dataRow.Substring(Effectivity_DateIndex + 1, U_S_JurisdictionIndex - Effectivity_DateIndex - 1);

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
