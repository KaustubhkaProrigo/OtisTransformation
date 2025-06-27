using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Prorigo.Plm.DataMigration.Utilities
{
    public static class ExcelUtils
    {
        public static void GenerateExcel(string fileName, string worksheetName, string[] columnHeaders, List<string[]> dataRows)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = worksheetName };
                sheets.Append(sheet);

                Row headerRow = new Row();
                foreach (var columnHeader in columnHeaders)
                {
                    Cell cell = new Cell();
                    cell.DataType = CellValues.String;
                    cell.CellValue = new CellValue(columnHeader);
                    headerRow.AppendChild(cell);
                }
                sheetData.AppendChild(headerRow);

                foreach (var dataRow in dataRows)
                {
                    Row worksheetRow = new Row();
                    foreach (var dataCell in dataRow)
                    {
                        Cell cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(dataCell);
                        worksheetRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(worksheetRow);
                }

                workbookPart.Workbook.Save();
            }
        }
    }
}
