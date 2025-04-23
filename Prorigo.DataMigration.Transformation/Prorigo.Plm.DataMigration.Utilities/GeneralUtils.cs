using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prorigo.Plm.DataMigration.Utilities
{
    public static class GeneralUtils
    {
        public static string GetDateRangeLabel(DateTime? fromDate, DateTime? toDate)
        {
            var dateRange = $"Full Range";
            if(fromDate != null && toDate != null)
                dateRange = $"{fromDate} - {toDate}";
            else if(fromDate != null && toDate == null)
                dateRange = $"{fromDate} - End";
            else if (fromDate == null && toDate != null)
                dateRange = $"Start - {toDate}";

            return dateRange;
        }

        public static List<(DateTime, DateTime)> GetDateRanges(DateTime fromDate, DateTime toDate, int extractionIntervalInMonths)
        {
            var dateRanges = new List<(DateTime, DateTime)>();

            var rangeStartDate = fromDate;
            var rangeEndDate = rangeStartDate.AddMonths(extractionIntervalInMonths);
            while (rangeEndDate < toDate)
            {
                dateRanges.Add((rangeStartDate, rangeEndDate));

                rangeStartDate = rangeEndDate;
                rangeEndDate = rangeStartDate.AddMonths(extractionIntervalInMonths);
            }

            if (rangeEndDate >= toDate)
                dateRanges.Add((rangeStartDate, toDate));

            return dateRanges;
        }

        public static IEnumerable<List<T>> SplitElementsToBatches<T>(List<T> elements, int batchSize)
        {
            for (int i = 0; i < elements.Count; i += batchSize)
            {
                yield return elements.GetRange(i, Math.Min(batchSize, elements.Count - i));
            }
        }
    }
}
