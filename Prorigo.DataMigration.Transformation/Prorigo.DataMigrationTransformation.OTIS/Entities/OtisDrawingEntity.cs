using System;
using System.Collections.Generic;
using System.Text;
using DocumentFormat.OpenXml.Office2010.Excel;
using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisDrawingEntity : IReadableEntity, IWritableEntity
    {
        public string Drawing_Number { get; set; }
        public string Revision { get; set; }
        public string State { get; set; }
        public string Name { get; set; }
        public string Classification { get; set; }
        public string Authoring_Tool { get; set; }
        public string Description { get; set; }
        public string MODIFIED_ON { get; set; }
        public string RELEASE_DATE { get; set; }
        public string Originated_on { get; set; }
        public string CN_Number { get; set; }
        public string Property2 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string xProperty1 { get; set; }
        public string xProperty2 { get; set; }
        public string xProperty3 { get; set; }
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
        public string MINOR_REV { get; set; }
        public string PERMISSION_ID { get; set; }
        public string MAJOR_REV { get; set; }

        public string DataRow
        {
            get
            {

                return $"{ARAS_UNIQUENESS_HELPER}\t{ID}\t{CONFIG_ID}\t{KEYED_NAME}\t{Drawing_Number}\t{Name}\t{Classification}\t{Authoring_Tool}\t{Description}\t{CREATED_BY_ID}\t{CREATED_ON}\t{CURRENT_STATE}\t{GENERATION}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{MINOR_REV}\t{PERMISSION_ID}\t{State}\t{Revision}\n";

            }
        }


        public OtisDrawingEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var Drawing_NumberIndex = dataRow.IndexOf('\t');
            Drawing_Number = dataRow.Substring(0, Drawing_NumberIndex);

            var RevisionIndex = dataRow.IndexOf('\t', Drawing_NumberIndex + 1);
            Revision = dataRow.Substring(Drawing_NumberIndex + 1, RevisionIndex - Drawing_NumberIndex - 1);

            var StateIndex = dataRow.IndexOf('\t', RevisionIndex + 1);
            State = dataRow.Substring(RevisionIndex + 1, StateIndex - RevisionIndex - 1);

            var NameIndex = dataRow.IndexOf('\t', StateIndex + 1);
            Name = dataRow.Substring(StateIndex + 1, NameIndex - StateIndex - 1);

            var ClassificationIndex = dataRow.IndexOf('\t', NameIndex + 1);
            Classification = dataRow.Substring(NameIndex + 1, ClassificationIndex - NameIndex - 1);

            var Authoring_ToolIndex = dataRow.IndexOf('\t', ClassificationIndex + 1);
            Authoring_Tool = dataRow.Substring(ClassificationIndex + 1, Authoring_ToolIndex - ClassificationIndex - 1);

            var DescriptionIndex = dataRow.IndexOf('\t', Authoring_ToolIndex + 1);
            Description = dataRow.Substring(Authoring_ToolIndex + 1, DescriptionIndex - Authoring_ToolIndex - 1);

            var MODIFIED_ONIndex = dataRow.IndexOf('\t', DescriptionIndex + 1);
            MODIFIED_ON = dataRow.Substring(DescriptionIndex + 1, MODIFIED_ONIndex - DescriptionIndex - 1);

            var RELEASE_DATEIndex = dataRow.IndexOf('\t', MODIFIED_ONIndex + 1);
            RELEASE_DATE = dataRow.Substring(MODIFIED_ONIndex + 1, RELEASE_DATEIndex - MODIFIED_ONIndex - 1);

            var Originated_onIndex = dataRow.IndexOf('\t', RELEASE_DATEIndex + 1);
            Originated_on = dataRow.Substring(RELEASE_DATEIndex + 1, Originated_onIndex - RELEASE_DATEIndex - 1);

            var CN_NumberIndex = dataRow.IndexOf('\t', Originated_onIndex + 1);
            CN_Number = dataRow.Substring(Originated_onIndex + 1, CN_NumberIndex - Originated_onIndex - 1);

            var Property2Index = dataRow.IndexOf('\t', CN_NumberIndex + 1);
            Property2 = dataRow.Substring(CN_NumberIndex + 1, Property2Index - CN_NumberIndex - 1);

            var Property3Index = dataRow.IndexOf('\t', Property2Index + 1);
            Property3 = dataRow.Substring(Property2Index + 1, Property3Index - Property2Index - 1);

            var Property4Index = dataRow.IndexOf('\t', Property3Index + 1);
            Property4 = dataRow.Substring(Property3Index + 1, Property4Index - Property3Index - 1);

            var xProperty1Index = dataRow.IndexOf('\t', Property4Index + 1);
            xProperty1 = dataRow.Substring(Property4Index + 1, xProperty1Index - Property4Index - 1);

            var xProperty2Index = dataRow.IndexOf('\t', xProperty1Index + 1);
            xProperty2 = dataRow.Substring(xProperty1Index + 1, xProperty2Index - xProperty1Index - 1);

            xProperty3 = dataRow.Substring(xProperty2Index + 1);

        }
    }
}

