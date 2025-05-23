﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    public enum TransformType
    {
        DatToTsv,
        ReorderColumns,
        OtisParameterFeatureOption,
        OtisExceltotsv,
        OtisPart,
        OtisFile,
        OtisProduct,
        EBOMExtractData,
        Otis_VM_BreakdownItem,
        EBOMInOutExtractData,
        Drawing_ParameterRelationship,
        EBOM2Extract,
        ExcelToTsvCellRangeInputOutput,
        ExcelToTsvStartCellValueInputOutput,
        ExcelToTsvPart,
        ExcelToTsvParameter,
        ExcelToTsvDrawing,
        OtisCADDrawing,
        OtisDrawingFile
    }
}
