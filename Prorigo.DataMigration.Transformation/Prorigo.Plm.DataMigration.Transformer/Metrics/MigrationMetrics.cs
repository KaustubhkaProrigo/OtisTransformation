using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Prorigo.Plm.DataMigration.Transformer.Metrics
{
    public class MigrationMetrics
    {
        [JsonPropertyName("currentMemory")]
        public string CurrentMemory { get; set; }

        [JsonPropertyName("peakMemory")]
        public string PeakMemory { get; set; }

        [JsonPropertyName("transformCount")]
        public int TransformCount
        {
            get
            {
                if (TransformMetrics != null)
                    return TransformMetrics.Count;
                else
                    return 0;
            }
        }

        [JsonPropertyName("transforms")]
        public List<TransformMetrics> TransformMetrics { get; set; }

        [JsonPropertyName("activities")]
        public List<ActivityMetrics> ActivityMetrics { get; set; }

    }
}
