
using Prorigo.Plm.DataMigration.IO;
//using static ClosedXML.Excel.XLPredefinedFormat;

namespace Prorigo.DataMigrationTransformation.OTIS.Entities
{
    class OtisParameterEntity :  IReadableEntity
    {
        public string Parameter { get; set; }
        public string Parameter_Description { get; set; }
        public string Value_Number { get; set; }
        public string Value { get; set; }
        public string Value_Description { get; set; }
        public string Local_Parameter_Value { get; set; }
        public string Function_Application { get; set; }
        public string DataType { get; set; }
        public string Loop { get; set; }
        public string Door { get; set; }
        public string UOM { get; set; }
        public string Customer_Level { get; set; }
        public string Level { get; set; }
        public string ERP { get; set; }
        public string Family { get; set; }
        public string Usage_Priority { get; set; }
        public string Subsystem { get; set; }
        public string Module { get; set; }
        public string Product { get; set; }
        public string Platform_Parameter { get; set; }
        public string Source { get; set; }
        public string Source_Document_Number { get; set; }
        public string Module_Variant_Number { get; set; }
        public string Windchill_Version { get; set; }
        public string Iteration { get; set; }
        public string Last_modified_Date { get; set; }
        public string Library_Name { get; set; }
        public string Number { get; set; }
        public string Platform { get; set; }

        public string Platform_Value { get; set; }
        public string ots_Image { get; set; }
        //public string Generation { get; set; }
        //public string is_current { get; set; }
        //public string is_released { get; set; }
        //public string major_rev { get; set; }
        //public string minor_rev { get; set; }
        //public string state { get; set; }
        //public string ot_is_3d_Param { get; set; }
        //public string ID { get; set; }




        //public string DataRow
        //{
        //    get
        //    {

        //        return $"{Parameter}\t{Parameter_Description}\t{Value_Number}\t{Value}\t{Value_Description}\t{Function_Application}\t{DataType}\t{Loop}\t{Door}\t{UOM}\t{Customer_Level}\t{Level}\t{ERP}\t{Family}\t{Usage_Priority}\t{Subsystem}\t{Module}\t{Product}\t{Platform_Parameter}\t{Source}\t{Source_Document_Number}\t{Module_Variant_Number}\t{Windchill_Version}\t{Iteration}\t{Last_modified_Date}\t{Library_Name}\t{Number}\t{Platform}\t{Platform_Value}\t{ots_Image}\n";
        //    }
        //}


        public OtisParameterEntity(string dataRow)
        {
            SetProperties(dataRow);
        }
        public void SetProperties(string dataRow)
        {
            var ParameterIndex = dataRow.IndexOf('\t');
            Parameter = dataRow.Substring(0, ParameterIndex);

            var Parameter_DescriptionIndex = dataRow.IndexOf('\t', ParameterIndex + 1);
            Parameter_Description = dataRow.Substring(ParameterIndex + 1, Parameter_DescriptionIndex - ParameterIndex - 1);

            var Value_NumberIndex = dataRow.IndexOf('\t', Parameter_DescriptionIndex + 1);
            Value_Number = dataRow.Substring(Parameter_DescriptionIndex + 1, Value_NumberIndex - Parameter_DescriptionIndex - 1);

            var ValueIndex = dataRow.IndexOf('\t', Value_NumberIndex + 1);
            Value = dataRow.Substring(Value_NumberIndex + 1, ValueIndex - Value_NumberIndex - 1);

            var Value_DescriptionIndex = dataRow.IndexOf('\t', ValueIndex + 1);
            Value_Description = dataRow.Substring(ValueIndex + 1, Value_DescriptionIndex - ValueIndex - 1);

            var Local_Parameter_ValueIndex = dataRow.IndexOf('\t', Value_DescriptionIndex + 1);
            Local_Parameter_Value = dataRow.Substring(Value_DescriptionIndex + 1, Local_Parameter_ValueIndex - Value_DescriptionIndex - 1);

            var Function_ApplicationIndex = dataRow.IndexOf('\t', Local_Parameter_ValueIndex + 1);
            Function_Application = dataRow.Substring(Local_Parameter_ValueIndex + 1, Function_ApplicationIndex - Local_Parameter_ValueIndex - 1);

            var DataTypeIndex = dataRow.IndexOf('\t', Function_ApplicationIndex + 1);
            DataType = dataRow.Substring(Function_ApplicationIndex + 1, DataTypeIndex - Function_ApplicationIndex - 1);

            var LoopIndex = dataRow.IndexOf('\t', DataTypeIndex + 1);
            Loop = dataRow.Substring(DataTypeIndex + 1, LoopIndex - DataTypeIndex - 1);

            var DoorIndex = dataRow.IndexOf('\t', LoopIndex + 1);
            Door = dataRow.Substring(LoopIndex + 1, DoorIndex - LoopIndex - 1);

            var UOMIndex = dataRow.IndexOf('\t', DoorIndex + 1);
            UOM = dataRow.Substring(DoorIndex + 1, UOMIndex - DoorIndex - 1);

            var Customer_LevelIndex = dataRow.IndexOf('\t', UOMIndex + 1);
            Customer_Level = dataRow.Substring(UOMIndex + 1, Customer_LevelIndex - UOMIndex - 1);

            var LevelIndex = dataRow.IndexOf('\t', Customer_LevelIndex + 1);
            Level = dataRow.Substring(Customer_LevelIndex + 1, LevelIndex - Customer_LevelIndex - 1);

            var ERPIndex = dataRow.IndexOf('\t', LevelIndex + 1);
            ERP = dataRow.Substring(LevelIndex + 1, ERPIndex - LevelIndex - 1);

            var FamilyIndex = dataRow.IndexOf('\t', ERPIndex + 1);
            Family = dataRow.Substring(ERPIndex + 1, FamilyIndex - ERPIndex - 1);

            var Usage_PriorityIndex = dataRow.IndexOf('\t', FamilyIndex + 1);
            Usage_Priority = dataRow.Substring(FamilyIndex + 1, Usage_PriorityIndex - FamilyIndex - 1);

            var SubsystemIndex = dataRow.IndexOf('\t', Usage_PriorityIndex + 1);
            Subsystem = dataRow.Substring(Usage_PriorityIndex + 1, SubsystemIndex - Usage_PriorityIndex - 1);

            var ModuleIndex = dataRow.IndexOf('\t', SubsystemIndex + 1);
            Module = dataRow.Substring(SubsystemIndex + 1, ModuleIndex - SubsystemIndex - 1);

            var ProductIndex = dataRow.IndexOf('\t', ModuleIndex + 1);
            Product = dataRow.Substring(ModuleIndex + 1, ProductIndex - ModuleIndex - 1);

            var Platform_ParameterIndex = dataRow.IndexOf('\t', ProductIndex + 1);
            Platform_Parameter = dataRow.Substring(ProductIndex + 1, Platform_ParameterIndex - ProductIndex - 1);

            var SourceIndex = dataRow.IndexOf('\t', Platform_ParameterIndex + 1);
            Source = dataRow.Substring(Platform_ParameterIndex + 1, SourceIndex - Platform_ParameterIndex - 1);

            var Source_Document_NumberIndex = dataRow.IndexOf('\t', SourceIndex + 1);
            Source_Document_Number = dataRow.Substring(SourceIndex + 1, Source_Document_NumberIndex - SourceIndex - 1);

            var Module_Variant_NumberIndex = dataRow.IndexOf('\t', Source_Document_NumberIndex + 1);
            Module_Variant_Number = dataRow.Substring(Source_Document_NumberIndex + 1, Module_Variant_NumberIndex - Source_Document_NumberIndex - 1);

            var Windchill_VersionIndex = dataRow.IndexOf('\t', Module_Variant_NumberIndex + 1);
            Windchill_Version = dataRow.Substring(Module_Variant_NumberIndex + 1, Windchill_VersionIndex - Module_Variant_NumberIndex - 1);

            var IterationIndex = dataRow.IndexOf('\t', Windchill_VersionIndex + 1);
            Iteration = dataRow.Substring(Windchill_VersionIndex + 1, IterationIndex - Windchill_VersionIndex - 1);

            var Last_modified_DateIndex = dataRow.IndexOf('\t', IterationIndex + 1);
            Last_modified_Date = dataRow.Substring(IterationIndex + 1, Last_modified_DateIndex - IterationIndex - 1);

            var Library_NameIndex = dataRow.IndexOf('\t', Last_modified_DateIndex + 1);
            Library_Name = dataRow.Substring(Last_modified_DateIndex + 1, Library_NameIndex - Last_modified_DateIndex - 1);

            var NumberIndex = dataRow.IndexOf('\t', Library_NameIndex + 1);
            Number = dataRow.Substring(Library_NameIndex + 1, NumberIndex - Library_NameIndex - 1);

            var PlatformIndex = dataRow.IndexOf('\t', NumberIndex + 1);
            Platform = dataRow.Substring(NumberIndex + 1, PlatformIndex - NumberIndex - 1);

            Platform_Value = dataRow.Substring(PlatformIndex + 1);
        }
    }
}
