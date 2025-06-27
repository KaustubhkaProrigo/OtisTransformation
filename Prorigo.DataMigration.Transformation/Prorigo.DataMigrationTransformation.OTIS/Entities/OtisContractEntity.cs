using System;
using System.Collections.Generic;
using System.Text;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Prorigo.Plm.DataMigration.IO;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisContractEntity : IWritableEntity, IReadableEntity
    {
        public string Sl_no { get; set; }
        public string Program_Num { get; set; }
        public string Contract_Number { get; set; }
        public string Project_Number { get; set; }
        public string Type { get; set; }
        public string Sub_Type { get; set; }
        public string Equipment_Code { get; set; }
        public string PDD { get; set; }
        public string Is_Rescheduled { get; set; }
        public string Reason_for_Reschedule { get; set; }
        public string Project_Manager { get; set; }
        public string Field_Supervisor { get; set; }
        public string Field_Supervisor_Contact_Number { get; set; }
        public string Project_Name { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Region { get; set; }
        public string Sub_Region { get; set; }
        public string Unit_Status { get; set; }
        public string Duty { get; set; }
        public string Stop { get; set; }
        public string Inspection_Clause { get; set; }
        public string is_deviated { get; set; }
        public string Deviation_Code { get; set; }
        public string Finish { get; set; }
        public string STD_LT { get; set; }
        public string Additional_LT { get; set; }
        public string Branch_Plant { get; set; }
        public string Order_Type { get; set; }
        public string Sold_To { get; set; }
        public string Ship_To { get; set; }
        public string Stop_Code { get; set; }
        public string Adjustment_Schedule { get; set; }
        public string FH_Code { get; set; }
        public string ARO_ARD { get; set; }
        public string NEW_CONTROLLER { get; set; }
        public string NEW_COP { get; set; }
        public string NEW_LED { get; set; }
        public string HOP { get; set; }
        public string Aesthetics { get; set; }
        public string DoorType { get; set; }
        public string DOOR { get; set; }
        public string Fire_Rating { get; set; }
        public string VisionPanel { get; set; }
        public string ASTHETICS { get; set; }
        public string ID { get; set; }
        public string ARAS_UNIQUENESS_HELPER { get; set; }
        public string KEYED_NAME { get; set; }
        public string CURRENT_STATE { get; set; }
        public string CREATED_BY_ID { get; set; }
        public string CREATED_ON { get; set; }
        public string MODIFIED_BY_ID { get; set; }
        public string MAJOR_REV { get; set; }
        public string IS_CURRENT { get; set; }
        public string MINOR_REV { get; set; }
        public string IS_RELEASED { get; set; }
        public string NOT_LOCKABLE { get; set; }
        public string GENERATION { get; set; }
        public string NEW_VERSION { get; set; }
        public string CONFIG_ID { get; set; }
        public string PERMISSION_ID { get; set; }
        public string OTS_Revision { get; set; }
        public string MODIFIED_ON { get; set; }
        public string STATE { get; set; }
        public string OTS_PRODUCT { get; set; }


        public string DataRow
        {
            get
            {
                return $"{ARAS_UNIQUENESS_HELPER}\t{ID}\t{CONFIG_ID}\t{KEYED_NAME}\t{CREATED_BY_ID}\t{CREATED_ON}\t{MODIFIED_BY_ID}\t{MODIFIED_ON}\t{Branch_Plant}\t{Equipment_Code}" +
                    $"\t{City}\t{Contract_Number}\t{PDD}\t{OTS_PRODUCT}\t{Program_Num}\t{Project_Name}\t{Type}\t{PERMISSION_ID}\t{CURRENT_STATE}\t{STATE}\t{IS_CURRENT}\t{MAJOR_REV}\t{MINOR_REV}" +
                    $"\t{IS_RELEASED}\t{NOT_LOCKABLE}\t{GENERATION}\t{NEW_VERSION}\t{OTS_Revision}\t{Project_Number}\t{Region}\n";

            }
        }

        public OtisContractEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var Sl_noIndex = dataRow.IndexOf('\t');
            Sl_no = dataRow.Substring(0, Sl_noIndex);

            var Program_NumIndex = dataRow.IndexOf('\t', Sl_noIndex + 1);
            Program_Num = dataRow.Substring(Sl_noIndex + 1, Program_NumIndex - Sl_noIndex - 1);

            var Contract_NumberIndex = dataRow.IndexOf('\t', Program_NumIndex + 1);
            Contract_Number = dataRow.Substring(Program_NumIndex + 1, Contract_NumberIndex - Program_NumIndex - 1);

            var Project_NumberIndex = dataRow.IndexOf('\t', Contract_NumberIndex + 1);
            Project_Number = dataRow.Substring(Contract_NumberIndex + 1, Project_NumberIndex - Contract_NumberIndex - 1);

            var TypeIndex = dataRow.IndexOf('\t', Project_NumberIndex + 1);
            Type = dataRow.Substring(Project_NumberIndex + 1, TypeIndex - Project_NumberIndex - 1);

            var Sub_TypeIndex = dataRow.IndexOf('\t', TypeIndex + 1);
            Sub_Type = dataRow.Substring(TypeIndex + 1, Sub_TypeIndex - TypeIndex - 1);

            var Equipment_CodeIndex = dataRow.IndexOf('\t', Sub_TypeIndex + 1);
            Equipment_Code = dataRow.Substring(Sub_TypeIndex + 1, Equipment_CodeIndex - Sub_TypeIndex - 1);

            var PDDIndex = dataRow.IndexOf('\t', Equipment_CodeIndex + 1);
            PDD = dataRow.Substring(Equipment_CodeIndex + 1, PDDIndex - Equipment_CodeIndex - 1);

            var Is_RescheduledIndex = dataRow.IndexOf('\t', PDDIndex + 1);
            Is_Rescheduled = dataRow.Substring(PDDIndex + 1, Is_RescheduledIndex - PDDIndex - 1);

            var Reason_for_RescheduleIndex = dataRow.IndexOf('\t', Is_RescheduledIndex + 1);
            Reason_for_Reschedule = dataRow.Substring(Is_RescheduledIndex + 1, Reason_for_RescheduleIndex - Is_RescheduledIndex - 1);

            var Project_ManagerIndex = dataRow.IndexOf('\t', Reason_for_RescheduleIndex + 1);
            Project_Manager = dataRow.Substring(Reason_for_RescheduleIndex + 1, Project_ManagerIndex - Reason_for_RescheduleIndex - 1);

            var Field_SupervisorIndex = dataRow.IndexOf('\t', Project_ManagerIndex + 1);
            Field_Supervisor = dataRow.Substring(Project_ManagerIndex + 1, Field_SupervisorIndex - Project_ManagerIndex - 1);

            var Field_Supervisor_Contact_NumberIndex = dataRow.IndexOf('\t', Field_SupervisorIndex + 1);
            Field_Supervisor_Contact_Number = dataRow.Substring(Field_SupervisorIndex + 1, Field_Supervisor_Contact_NumberIndex - Field_SupervisorIndex - 1);

            var Project_NameIndex = dataRow.IndexOf('\t', Field_Supervisor_Contact_NumberIndex + 1);
            Project_Name = dataRow.Substring(Field_Supervisor_Contact_NumberIndex + 1, Project_NameIndex - Field_Supervisor_Contact_NumberIndex - 1);

            var CityIndex = dataRow.IndexOf('\t', Project_NameIndex + 1);
            City = dataRow.Substring(Project_NameIndex + 1, CityIndex - Project_NameIndex - 1);

            var StateIndex = dataRow.IndexOf('\t', CityIndex + 1);
            State = dataRow.Substring(CityIndex + 1, StateIndex - CityIndex - 1);

            var RegionIndex = dataRow.IndexOf('\t', StateIndex + 1);
            Region = dataRow.Substring(StateIndex + 1, RegionIndex - StateIndex - 1);

            var Sub_RegionIndex = dataRow.IndexOf('\t', RegionIndex + 1);
            Sub_Region = dataRow.Substring(RegionIndex + 1, Sub_RegionIndex - RegionIndex - 1);

            var Unit_StatusIndex = dataRow.IndexOf('\t', Sub_RegionIndex + 1);
            Unit_Status = dataRow.Substring(Sub_RegionIndex + 1, Unit_StatusIndex - Sub_RegionIndex - 1);

            var DutyIndex = dataRow.IndexOf('\t', Unit_StatusIndex + 1);
            Duty = dataRow.Substring(Unit_StatusIndex + 1, DutyIndex - Unit_StatusIndex - 1);

            var StopIndex = dataRow.IndexOf('\t', DutyIndex + 1);
            Stop = dataRow.Substring(DutyIndex + 1, StopIndex - DutyIndex - 1);

            var Inspection_ClauseIndex = dataRow.IndexOf('\t', StopIndex + 1);
            Inspection_Clause = dataRow.Substring(StopIndex + 1, Inspection_ClauseIndex - StopIndex - 1);

            var is_deviatedIndex = dataRow.IndexOf('\t', Inspection_ClauseIndex + 1);
            is_deviated = dataRow.Substring(Inspection_ClauseIndex + 1, is_deviatedIndex - Inspection_ClauseIndex - 1);

            var Deviation_CodeIndex = dataRow.IndexOf('\t', is_deviatedIndex + 1);
            Deviation_Code = dataRow.Substring(is_deviatedIndex + 1, Deviation_CodeIndex - is_deviatedIndex - 1);

            var FinishIndex = dataRow.IndexOf('\t', Deviation_CodeIndex + 1);
            Finish = dataRow.Substring(Deviation_CodeIndex + 1, FinishIndex - Deviation_CodeIndex - 1);

            var STD_LTIndex = dataRow.IndexOf('\t', FinishIndex + 1);
            STD_LT = dataRow.Substring(FinishIndex + 1, STD_LTIndex - FinishIndex - 1);

            var Additional_LTIndex = dataRow.IndexOf('\t', STD_LTIndex + 1);
            Additional_LT = dataRow.Substring(STD_LTIndex + 1, Additional_LTIndex - STD_LTIndex - 1);

            var Branch_PlantIndex = dataRow.IndexOf('\t', Additional_LTIndex + 1);
            Branch_Plant = dataRow.Substring(Additional_LTIndex + 1, Branch_PlantIndex - Additional_LTIndex - 1);

            var Order_TypeIndex = dataRow.IndexOf('\t', Branch_PlantIndex + 1);
            Order_Type = dataRow.Substring(Branch_PlantIndex + 1, Order_TypeIndex - Branch_PlantIndex - 1);

            var Sold_ToIndex = dataRow.IndexOf('\t', Order_TypeIndex + 1);
            Sold_To = dataRow.Substring(Order_TypeIndex + 1, Sold_ToIndex - Order_TypeIndex - 1);

            var Ship_ToIndex = dataRow.IndexOf('\t', Sold_ToIndex + 1);
            Ship_To = dataRow.Substring(Sold_ToIndex + 1, Ship_ToIndex - Sold_ToIndex - 1);

            var Stop_CodeIndex = dataRow.IndexOf('\t', Ship_ToIndex + 1);
            Stop_Code = dataRow.Substring(Ship_ToIndex + 1, Stop_CodeIndex - Ship_ToIndex - 1);

            var Adjustment_ScheduleIndex = dataRow.IndexOf('\t', Stop_CodeIndex + 1);
            Adjustment_Schedule = dataRow.Substring(Stop_CodeIndex + 1, Adjustment_ScheduleIndex - Stop_CodeIndex - 1);

            var FH_CodeIndex = dataRow.IndexOf('\t', Adjustment_ScheduleIndex + 1);
            FH_Code = dataRow.Substring(Adjustment_ScheduleIndex + 1, FH_CodeIndex - Adjustment_ScheduleIndex - 1);

            var ARO_ARDIndex = dataRow.IndexOf('\t', FH_CodeIndex + 1);
            ARO_ARD = dataRow.Substring(FH_CodeIndex + 1, ARO_ARDIndex - FH_CodeIndex - 1);

            var NEW_CONTROLLERIndex = dataRow.IndexOf('\t', ARO_ARDIndex + 1);
            NEW_CONTROLLER = dataRow.Substring(ARO_ARDIndex + 1, NEW_CONTROLLERIndex - ARO_ARDIndex - 1);

            var NEW_COPIndex = dataRow.IndexOf('\t', NEW_CONTROLLERIndex + 1);
            NEW_COP = dataRow.Substring(NEW_CONTROLLERIndex + 1, NEW_COPIndex - NEW_CONTROLLERIndex - 1);

            var NEW_LEDIndex = dataRow.IndexOf('\t', NEW_COPIndex + 1);
            NEW_LED = dataRow.Substring(NEW_COPIndex + 1, NEW_LEDIndex - NEW_COPIndex - 1);

            var HOPIndex = dataRow.IndexOf('\t', NEW_LEDIndex + 1);
            HOP = dataRow.Substring(NEW_LEDIndex + 1, HOPIndex - NEW_LEDIndex - 1);

            var AestheticsIndex = dataRow.IndexOf('\t', HOPIndex + 1);
            Aesthetics = dataRow.Substring(HOPIndex + 1, AestheticsIndex - HOPIndex - 1);

            var DoorTypeIndex = dataRow.IndexOf('\t', AestheticsIndex + 1);
            DoorType = dataRow.Substring(AestheticsIndex + 1, DoorTypeIndex - AestheticsIndex - 1);

            var DOORIndex = dataRow.IndexOf('\t', DoorTypeIndex + 1);
            DOOR = dataRow.Substring(DoorTypeIndex + 1, DOORIndex - DoorTypeIndex - 1);

            var Fire_RatingIndex = dataRow.IndexOf('\t', DOORIndex + 1);
            Fire_Rating = dataRow.Substring(DOORIndex + 1, Fire_RatingIndex - DOORIndex - 1);

            var VisionPanelIndex = dataRow.IndexOf('\t', Fire_RatingIndex + 1);
            VisionPanel = dataRow.Substring(Fire_RatingIndex + 1, VisionPanelIndex - Fire_RatingIndex - 1);

            ASTHETICS = dataRow.Substring(VisionPanelIndex + 1);
        }
    }
}
