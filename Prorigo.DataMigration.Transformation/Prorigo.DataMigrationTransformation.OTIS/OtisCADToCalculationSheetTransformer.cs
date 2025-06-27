using System;
using System.Collections.Generic;
using System.IO;
using Prorigo.Plm.DataMigration.Utilities;

using Prorigo.Plm.DataMigration.Transformer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using Prorigo.DataMigrationTransformation.OTIS.Entities;
using System.Configuration;
using System.Linq;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    class OtisCADToCalculationSheetTransformer : IDataTransformer
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisCADToCalculationSheetTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;
        private string[] _processType;

        string CAD = "Drawing";
        string CalculationSheet = "Conv_CSVToExl_CalculationSheet";
        public OtisCADToCalculationSheetTransformer(IConfiguration configuration, ILogger<OtisCADToCalculationSheetTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var OtisCADToCalculationSheetSection = _configuration.GetSection("OtisCADToCalculationSheet");
            _processAreaDataPath = OtisCADToCalculationSheetSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = OtisCADToCalculationSheetSection.GetValue<long>("ObjectCountPerFile");
            _processType = OtisCADToCalculationSheetSection.GetSection("ProcessType").Get<string[]>();
        }

        public void Transform(string LicenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");
            if (isLicenValid)
            {
                var className = this.GetType().Name;
                var transformName = className.Substring(0, className.IndexOf("Transformer"));
                foreach (var processtype in _processType)
                {
                    DefaultValueAdder(transformName, processtype);
                }

            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }
        public void DefaultValueAdder(string transformName, string processtype)
        {
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, CAD);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, CAD, TransformStatus.InProgress);

            var OtisCADToCalculationSheetWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, CAD), _objectCountPerFile)
            {
                FileBaseName = $"TR_CADToCalculationSheet",
                TypeName = $"{processtype}_CADToCalculationSheet",
                FileExtension = "tsv",
            };

            var MissingDrawingWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, CAD), _objectCountPerFile)
            {
                FileBaseName = $"Missing{processtype}_DrawingToCalSheet",
                TypeName = $"Missing{processtype}_DrawingToCalSheet",
                FileExtension = "tsv",
            };

            //var CADReader = new TypeDataFileReader(_processAreaDataPath);
            //var CADEntities = CADReader.ReadAllEntities<OtisCadTransformedEntity>(CAD, "*.tsv"); //Read CAD Entity

            //var CalculationSheetReader = new TypeDataFileReader(_processAreaDataPath);
            //var CalculationSheetEntities = CalculationSheetReader.ReadAllEntities<OtisCalculationSheetEntity>($"{processtype}_CalculationSheet", "*.tsv"); //Read CalculationSheet Entities

            //Dictionary<string, string> DrawingEntityMap = new Dictionary<string, string>();

            //foreach (var CalculationSheetEntity in CalculationSheetEntities)
            //{
            //    if (!DrawingEntityMap.ContainsKey(CalculationSheetEntity.KEYED_NAME))
            //    {
            //        DrawingEntityMap.Add(CalculationSheetEntity.KEYED_NAME, CalculationSheetEntity.Id);
            //    }
            //}

            var CalculationSheetReader = new TypeDataFileReader(_processAreaDataPath);
            var CalculationSheetEntities = CalculationSheetReader.ReadAllEntities<OtisCalculationSheetEntity>($"{processtype}_CalculationSheet", "*.tsv"); //Read CalculationSheet Entities
            Dictionary<string, string> CalculationSheetEntityMap = new Dictionary<string, string>();

            foreach (var CalculationSheetEntity in CalculationSheetEntities)
            {
                if (!CalculationSheetEntityMap.ContainsKey(CalculationSheetEntity.KEYED_NAME))
                {
                    CalculationSheetEntityMap.Add(CalculationSheetEntity.KEYED_NAME, CalculationSheetEntity.Id);
                }
            }

            var DrawingReader = new TypeDataFileReader(_processAreaDataPath);
            var DrawingEntities = DrawingReader.ReadAllEntities<OtisCadTransformedEntity>(CAD, "*.tsv"); //Read CAD Entity

            Dictionary<string, string> DrawingEntityMap = new Dictionary<string, string>();

            foreach (var DrawingEntity in DrawingEntities)
            {
                if (!DrawingEntityMap.ContainsKey(DrawingEntity.keyed_name))
                {
                    DrawingEntityMap.Add(DrawingEntity.keyed_name, DrawingEntity.id);
                }
            }

            long successCount = 0;

            using (OtisCADToCalculationSheetWriter)
            {

                foreach (var CalculationSheetEntity in CalculationSheetEntities)
                {
                    using (MissingDrawingWriter)
                    {

                        if (OtisCADToCalculationSheetWriter.HeaderRow == null)
                            OtisCADToCalculationSheetWriter.HeaderRow = "ConnectionId\tCONFIG_ID\tFromFilename\tToFilename\tSourceID\tRelatedId\tCREATED_ON\tCREATED_BY_ID\tPERMISSION_ID\tIS_CURRENT\tIS_RELEASED\tMAJOR_REV\tNOT_LOCKABLE\tGENERATION\n";

                        if (MissingDrawingWriter.HeaderRow == null)
                            MissingDrawingWriter.HeaderRow = "FromFilename\tToFilename\n";

                        if (DrawingEntityMap.ContainsKey(CalculationSheetEntity.KEYED_NAME))
                        {
                            var ConnectionId = TransformerUtils.GetNewArasGuid();
                            var CONFIG_ID = ConnectionId;
                            var FromFilename = DrawingEntityMap.Keys.FirstOrDefault(k => k == CalculationSheetEntity.KEYED_NAME);
                            var ToFilename = CalculationSheetEntity.KEYED_NAME;
                            var SourceID = DrawingEntityMap[CalculationSheetEntity.KEYED_NAME];
                            var RelatedId = CalculationSheetEntity.Id;
                            var CREATED_ON = DateTime.Now.ToString();
                            var CREATED_BY_ID = "Data Migration";
                            var PERMISSION_ID = "95475AE006E7415794BDC93808DC04D2";
                            var IS_CURRENT = "1";
                            var IS_RELEASED = "1";
                            var MAJOR_REV = "A";
                            var NOT_LOCKABLE = "0";
                            var GENERATION = "1";

                            OtisCADToCalculationSheetWriter.WriteRow($"{ConnectionId}\t{CONFIG_ID}\t{FromFilename}\t{ToFilename}\t{SourceID}\t{RelatedId}\t{CREATED_ON}\t{CREATED_BY_ID}\t{PERMISSION_ID}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{NOT_LOCKABLE}\t{GENERATION}\n");
                            successCount++;
                        }
                        else if (!DrawingEntityMap.ContainsKey(CalculationSheetEntity.KEYED_NAME))
                        {
                            var FromFilename = "";
                            var ToFilename = CalculationSheetEntity.KEYED_NAME;
                            MissingDrawingWriter.WriteRow($"{FromFilename}\t{ToFilename}\n");
                            successCount++;
                        }
                    }
                }
                }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, CAD, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, CAD);
        }
    }
}
