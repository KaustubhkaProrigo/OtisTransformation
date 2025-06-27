using System;
using System.Collections.Generic;
using System.Text;

using Prorigo.Plm.DataMigration.Transformer.Metrics;

namespace Prorigo.Plm.DataMigration.Transformer
{
    public interface IMigrationDiagnostics
    {
        void LogActivityStartTime(string activityName);

        void LogActivityEndTime(string activityName); 

        void LogTransformStatus(string transformName, TransformStatus transformStatus, string errorMessage = null);

        void LogTransformStartTime(string transformName);

        void LogTransformEndTime(string transformName);

        void LogTransformTypeStatus(string transformName, string typeName, TransformStatus transformStatus, long transformedObjectCount = 0, long failedObjectCount = 0);

        void LogTransformTypeStartTime(string transformName, string typeName);

        void LogTransformTypeEndTime(string transformName, string typeName);
    }
}
