using System;
using System.Collections.Generic;
using System.Text;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisODSEntity : IWritableEntity, IReadableEntity
    {
        public string ODS_Number { get; set; }
        public string Revision { get; set; }
        public string State { get; set; }
        public string ODS_Name { get; set; }
        public string Description { get; set; }
        public string Classification { get; set; }
        public string Created_By { get; set; }
        public string Creation_Date { get; set; }
        public string Modified_By { get; set; }
        public string Modified_Date { get; set; }
        public string Module { get; set; }
        public string Product { get; set; }
        public string Subsytem { get; set; }
        public string Platform { get; set; }
        public string Effectivity_Date { get; set; }
        public string Status { get; set; }
        public string id { get; set; }
        public string ARAS_UNIQUENESS_HELPER { get; set; }
        public string KEYED_NAME { get; set; }
        public string CURRENT_STATE { get; set; }
        public string IS_CURRENT { get; set; }
        public string MINOR_REV { get; set; }
        public string IS_RELEASED { get; set; }
        public string NOT_LOCKABLE { get; set; }
        public string GENERATION { get; set; }
        public string NEW_VERSION { get; set; }
        public string CONFIG_ID { get; set; }
        public string PERMISSION_ID { get; set; }
        public string MAJOR_REV { get; set; }
        public string Ots_Calculation_sheet { get; set; }
        public string Owned_By_id { get; set; }


        public string DataRow
        {
            get
            {
                return $"{ARAS_UNIQUENESS_HELPER}\t{id}\t{CONFIG_ID}\t{KEYED_NAME}\t{ODS_Number}\t{ODS_Name}\t{Description}\t{Classification}\t{Created_By}\t{Creation_Date}\t{Modified_By}\t{Modified_Date}\t{Owned_By_id}\t{Revision}\t{State}\t{CURRENT_STATE}\t{IS_CURRENT}\t{MINOR_REV}\t{MAJOR_REV}\t{IS_RELEASED}\t{NOT_LOCKABLE}\t{GENERATION}\t{NEW_VERSION}\t{PERMISSION_ID}\t{Ots_Calculation_sheet}\n";
            }
        }

        public OtisODSEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var ODS_NumberIndex = dataRow.IndexOf('\t');
            ODS_Number = dataRow.Substring(0, ODS_NumberIndex);

            var RevisionIndex = dataRow.IndexOf('\t', ODS_NumberIndex + 1);
            Revision = dataRow.Substring(ODS_NumberIndex + 1, RevisionIndex - ODS_NumberIndex - 1);

            var StateIndex = dataRow.IndexOf('\t', RevisionIndex + 1);
            State = dataRow.Substring(RevisionIndex + 1, StateIndex - RevisionIndex - 1);

            var ODS_NameIndex = dataRow.IndexOf('\t', StateIndex + 1);
            ODS_Name = dataRow.Substring(StateIndex + 1, ODS_NameIndex - StateIndex - 1);

            var DescriptionIndex = dataRow.IndexOf('\t', ODS_NameIndex + 1);
            Description = dataRow.Substring(ODS_NameIndex + 1, DescriptionIndex - ODS_NameIndex - 1);

            var ClassificationIndex = dataRow.IndexOf('\t', DescriptionIndex + 1);
            Classification = dataRow.Substring(DescriptionIndex + 1, ClassificationIndex - DescriptionIndex - 1);

            var Created_ByIndex = dataRow.IndexOf('\t', ClassificationIndex + 1);
            Created_By = dataRow.Substring(ClassificationIndex + 1, Created_ByIndex - ClassificationIndex - 1);

            var Creation_DateIndex = dataRow.IndexOf('\t', Created_ByIndex + 1);
            Creation_Date = dataRow.Substring(Created_ByIndex + 1, Creation_DateIndex - Created_ByIndex - 1);

            var Modified_ByIndex = dataRow.IndexOf('\t', Creation_DateIndex + 1);
            Modified_By = dataRow.Substring(Creation_DateIndex + 1, Modified_ByIndex - Creation_DateIndex - 1);

            var Modified_DateIndex = dataRow.IndexOf('\t', Modified_ByIndex + 1);
            Modified_Date = dataRow.Substring(Modified_ByIndex + 1, Modified_DateIndex - Modified_ByIndex - 1);

            var ModuleIndex = dataRow.IndexOf('\t', Modified_DateIndex + 1);
            Module = dataRow.Substring(Modified_DateIndex + 1, ModuleIndex - Modified_DateIndex - 1);

            var ProductIndex = dataRow.IndexOf('\t', ModuleIndex + 1);
            Product = dataRow.Substring(ModuleIndex + 1, ProductIndex - ModuleIndex - 1);

            var SubsytemIndex = dataRow.IndexOf('\t', ProductIndex + 1);
            Subsytem = dataRow.Substring(ProductIndex + 1, SubsytemIndex - ProductIndex - 1);

            var PlatformIndex = dataRow.IndexOf('\t', SubsytemIndex + 1);
            Platform = dataRow.Substring(SubsytemIndex + 1, PlatformIndex - SubsytemIndex - 1);

            var Effectivity_DateIndex = dataRow.IndexOf('\t', PlatformIndex + 1);
            Effectivity_Date = dataRow.Substring(PlatformIndex + 1, Effectivity_DateIndex - PlatformIndex - 1);

            Status = dataRow.Substring(Effectivity_DateIndex + 1);
        }
    }
}
