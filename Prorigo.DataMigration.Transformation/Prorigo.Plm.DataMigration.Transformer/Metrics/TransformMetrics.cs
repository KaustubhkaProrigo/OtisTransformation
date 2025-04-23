using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Prorigo.Plm.DataMigration.Transformer.Metrics
{
    public class TransformMetrics
    {
        [JsonPropertyName("transformName")]
        public string TransformName { get; set; }

        [JsonPropertyName("transformStatus")]
        public TransformStatus TransformStatus { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonPropertyName("trasnformStartTime")]
        public DateTime? TransformStartTime { get; set; }

        [JsonPropertyName("transformTotalTime")]
        public TimeSpan? TransformTotalTime { get; set; }

        [JsonPropertyName("typeCount")]
        public int TypeCount
        {
            get
            {
                if (TypeMetrics != null)
                    return TypeMetrics.Count;
                else
                    return 0;
            }
        }

        [JsonPropertyName("types")]
        public List<TypeMetrics> TypeMetrics { get; set; }
    }
}
