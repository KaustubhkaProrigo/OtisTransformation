using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prorigo.Plm.DataMigration.Utilities
{
    public static class EntityUtils
    {
        public static Dictionary<string, Dictionary<string, string>> GetEntitiesByColumnNames(Dictionary<string, string> entityIdDataRowMap, string headerRow, List<string> columnNames)
        {
            var columnHeaders = headerRow.TrimEnd('\n').Split('\t').ToList();

            var columnNameColumnIndexMap = new Dictionary<string, int>();
            foreach(var columnName in columnNames)
                columnNameColumnIndexMap[columnName] = columnHeaders.IndexOf(columnName);

            var entityIdDataMap = new Dictionary<string, Dictionary<string, string>>();
            foreach (var partQualityPlanIdDataRowMapEntry in entityIdDataRowMap)
            {
                var rowCellValues = partQualityPlanIdDataRowMapEntry.Value.TrimEnd('\n').Split('\t').ToList();

                var columnValues = new Dictionary<string, string>();
                foreach(var columnNameColumnIndexMapEntry in columnNameColumnIndexMap)
                {
                    columnValues[columnNameColumnIndexMapEntry.Key] = rowCellValues[columnNameColumnIndexMapEntry.Value];
                }

                entityIdDataMap[partQualityPlanIdDataRowMapEntry.Key] = columnValues;
            }

            return entityIdDataMap;
        }
    }
}
