using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Transformer.Metrics;

namespace Prorigo.Plm.DataMigration.Transformer
{
    public class MigrationDiagnostics : IMigrationDiagnostics
    {
        private MigrationMetrics _migrationMetrics;

        private readonly string _diagnosticsFileName;
        private readonly Process _currentProcess;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly object lockObject = new object();
        private readonly object transformLockObject = new object();
        private readonly object transformTypeLockObject = new object();
        private readonly object activityLockObject = new object();

        public MigrationDiagnostics(string processAreaDataPath)
        {
            _migrationMetrics = new MigrationMetrics();
            _currentProcess = Process.GetCurrentProcess();

            string _diagnosticFileName = "TransformerDiagnostics_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".json";
            _diagnosticsFileName = Path.Combine(processAreaDataPath, _diagnosticFileName);

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            _jsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
            _jsonSerializerOptions.Converters.Add(new TimeSpanJsonConverter());
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public void LogActivityStartTime(string activityName)
        {
            var activityMetrics = GetActivityMetrics(activityName);
            activityMetrics.ActivityStartTime = DateTime.Now;

            WriteMetrics();
        }

        public void LogActivityEndTime(string activityName)
        {
            var activityMetrics = GetActivityMetrics(activityName);
            activityMetrics.ActivityTotalTime = DateTime.Now - activityMetrics.ActivityStartTime;

            WriteMetrics();
        }

        public void LogTransformStartTime(string transformName)
        {
            var transformMetrics = GetTransformMetrics(transformName);
            transformMetrics.TransformStartTime = DateTime.Now;

            WriteMetrics();
        }

        public void LogTransformEndTime(string transformName)
        {
            var transformMetrics = GetTransformMetrics(transformName);
            transformMetrics.TransformTotalTime = DateTime.Now - transformMetrics.TransformStartTime;

            WriteMetrics();
        }
        public void LogTransformStatus(string transformName, TransformStatus transformStatus, string errorMessage = null)
        {
            var transformMetrics = GetTransformMetrics(transformName);
            transformMetrics.TransformStatus = transformStatus;

            if (!string.IsNullOrWhiteSpace(errorMessage))
                transformMetrics.ErrorMessage = errorMessage;

            WriteMetrics();
        }

        public void LogTransformTypeStartTime(string transformName, string typeName)
        {
            var typeMetrics = GetTransformTypeMetrics(transformName, typeName);
            typeMetrics.TransformStartTime = DateTime.Now;

            WriteMetrics();
        }

        public void LogTransformTypeEndTime(string transformName, string typeName)
        {
            var typeMetrics = GetTransformTypeMetrics(transformName, typeName);
            typeMetrics.TransformTotalTime = DateTime.Now - typeMetrics.TransformStartTime;

            WriteMetrics();
        }

        public void LogTransformTypeStatus(string transformName, string typeName, TransformStatus transformStatus, long transformedObjectCount, long failedObjectCount)
        {
            var typeMetrics = GetTransformTypeMetrics(transformName, typeName);
            typeMetrics.TransformStatus = transformStatus;
            typeMetrics.TransformedObjectCount = transformedObjectCount;
            typeMetrics.FailedObjectCount = failedObjectCount;

            WriteMetrics();
        }

        private ActivityMetrics GetActivityMetrics(string activityName)
        {
            var activityMetrics = new ActivityMetrics();

            if (_migrationMetrics.ActivityMetrics == null)
                _migrationMetrics.ActivityMetrics = new List<ActivityMetrics>();

            lock (activityLockObject)
            {
                activityMetrics = _migrationMetrics.ActivityMetrics.Where(m => m.ActivityName.Equals(activityName)).FirstOrDefault();
                if (activityMetrics == null)
                {
                    activityMetrics = new ActivityMetrics();
                    activityMetrics.ActivityName = activityName;
                    _migrationMetrics.ActivityMetrics.Add(activityMetrics);
                }
            }

            return activityMetrics;
        }

        private TypeMetrics GetTransformTypeMetrics(string transformName, string typeName)
        {
            TypeMetrics typeMetrics = null;

            var transformMetrics = GetTransformMetrics(transformName);
            if (transformMetrics.TypeMetrics == null)
                transformMetrics.TypeMetrics = new List<TypeMetrics>();

            lock (transformTypeLockObject)
            {
                typeMetrics = transformMetrics.TypeMetrics.Where(m => m.TypeName.Equals(typeName)).FirstOrDefault();
                if (typeMetrics == null)
                {
                    typeMetrics = new TypeMetrics() { TypeName = typeName };
                    transformMetrics.TypeMetrics.Add(typeMetrics);
                }
            }

            return typeMetrics;
        }

        private void WriteMetrics()
        {
            lock (lockObject)
            {
                _migrationMetrics.CurrentMemory = $"{_currentProcess.WorkingSet64 / (1000 * 1000)} MB";
                _migrationMetrics.PeakMemory = $"{_currentProcess.PeakWorkingSet64 / (1000 * 1000)} MB";
                using (StreamWriter streamWriter = File.CreateText(_diagnosticsFileName))
                {
                    var jsonString = JsonSerializer.Serialize(_migrationMetrics, _jsonSerializerOptions);
                    streamWriter.WriteLine(jsonString);
                }
             }
        }

        private TransformMetrics GetTransformMetrics(string transformName)
        {
            var transformMetrics = new TransformMetrics();
            if (_migrationMetrics.TransformMetrics == null)
                _migrationMetrics.TransformMetrics = new List<TransformMetrics>();

            lock (transformLockObject)
            {
                transformMetrics = _migrationMetrics.TransformMetrics.Where(m => m.TransformName.Equals(transformName)).FirstOrDefault();
                if (transformMetrics == null)
                {
                    transformMetrics = new TransformMetrics();
                    transformMetrics.TransformName = transformName;
                    _migrationMetrics.TransformMetrics.Add(transformMetrics);
                }
            }

            return transformMetrics;
        }

        class DateTimeJsonConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert,
                JsonSerializerOptions options) =>
                    DateTime.ParseExact(reader.GetString(), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            public override void Write(Utf8JsonWriter writer, DateTime dateTimeValue,
                JsonSerializerOptions options) =>
                    writer.WriteStringValue(dateTimeValue.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
        }

        class TimeSpanJsonConverter : JsonConverter<TimeSpan>
        {
            public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert,
                JsonSerializerOptions options) =>
                    TimeSpan.ParseExact(reader.GetString(), @"dd\.hh\:mm\:ss", CultureInfo.InvariantCulture);

            public override void Write(Utf8JsonWriter writer, TimeSpan timeSpanValue,
                JsonSerializerOptions options) =>
                        writer.WriteStringValue(timeSpanValue.ToString(@"dd\.hh\:mm\:ss", CultureInfo.InvariantCulture));
        }
    }
}
