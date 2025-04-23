using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Prorigo.Plm.DataMigration.Transformer.Metrics
{
    public class TypeMetrics
    {
        [JsonPropertyName("typeName")]
        public string TypeName { get; set; }

        [JsonPropertyName("transformStatus")]
        public TransformStatus TransformStatus { get; set; }

        [JsonPropertyName("transformedObjectCount")]
        public long TransformedObjectCount { get; set; }

        [JsonPropertyName("failedObjectCount")]
        public long FailedObjectCount { get; set; }

        [JsonPropertyName("transformStartTime")]
        public DateTime? TransformStartTime { get; set; }

        [JsonPropertyName("transformTotalTime")]
        public TimeSpan? TransformTotalTime { get; set; }
    }
}
