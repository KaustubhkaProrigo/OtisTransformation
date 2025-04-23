using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Prorigo.Plm.DataMigration.Transformer.Metrics
{
    public class ActivityMetrics
    {
        [JsonPropertyName("activityName")]
        public string ActivityName { get; set; }

        [JsonPropertyName("activityStartTime")]
        public DateTime? ActivityStartTime { get; set; }

        [JsonPropertyName("activityTotalTime")]
        public TimeSpan? ActivityTotalTime { get; set; }
    }
}
