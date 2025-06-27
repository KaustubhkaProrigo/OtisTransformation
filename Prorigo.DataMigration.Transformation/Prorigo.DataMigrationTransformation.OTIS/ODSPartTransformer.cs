using System;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using Prorigo.DataMigrationTransformation.OTIS.Entities;

using Prorigo.Plm.DataMigration.Utilities;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    class ODSPartTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ODSPartTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;

        private const string Calculation = "Calculation";
        private const string Otis_Breakdown = "BreakdownItem_ODS";
        private const string Otis_Part = "Part";
        private const string ODS = "ODS";

        public ODSPartTransformer(IConfiguration configuration, ILogger<ODSPartTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var ODSPartSection = _configuration.GetSection("ODSPart");
            _processAreaDataPath = ODSPartSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = ODSPartSection.GetValue<long>("ObjectCountPerFile"); //change
        }

        public void Transform(string LicenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            //License key
            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");
            if (isLicenValid)
            {
                var className = this.GetType().Name;
                var transformName = className.Substring(0, className.IndexOf("Transformer"));
                TransformFiles(transformName, Calculation);
            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }

        private void TransformFiles(string typeName, string transformName)
        {
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, typeName);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.InProgress);

            //var path = Path.Combine(_processAreaDataPath, ODS);

            var ODSPartDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "BOM Files"));
            var ODSPartEntities = ODSPartDataReader.ReadAllEntities<ODSPartEntity>(ODS, "*.tsv");

            var BreakDownDataReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, ODS));
            var BreakDownEntities = BreakDownDataReader.ReadAllEntities<ODSEntity>(Otis_Breakdown, "*.tsv");

            var PartReader = new TypeDataFileReader(_processAreaDataPath);
            var PartEntities = PartReader.ReadAllEntities<OtisPartTSVEntity>(Otis_Part, "*.tsv");

            var PartDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, ODS), _objectCountPerFile)
            {
                FileBaseName = $"ODSPart_MetaData",
                TypeName = $"ODSPart_MetaData",
                FileExtension = "tsv"
            };
            var failedFilesDataFileWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, ODS), _objectCountPerFile)
            {
                FileBaseName = $"TR_Failed_ODSPart_MetaData",
                TypeName = "Failed_ODSPart_MetaData",
                HeaderRow = "Part_Name\tODSNumber\tErrorDescription\n",
                FileExtension = "tsv"
            };
            var BreakDown = BreakDownEntities.ToDictionary(e => e.ITEM_NUMBER, e => e.ID);
            var Part = PartEntities.ToDictionary(e => e.item_number, e => e.id);

            int objectCount = 0, failedObjectCount = 0;
            using (failedFilesDataFileWriter)
            {
                using (PartDataFileWriter)
                {
                    if (PartDataFileWriter.HeaderRow == null)
                        PartDataFileWriter.HeaderRow = "ConnectionId\tCONFIG_ID\tKEYED_NAME\tCREATED_ON\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tIS_CURRENT\tMAJOR_REV\tSTATE\tIS_RELEASED\tNOT_LOCKABLE\tGENERATION\tPERMISSION_ID\tSOURCE_ID\tQUANTITY\tRELATED_ITEMTYPE\tRELATED_ID\tEXPRESSIONID\n";

                    foreach (var ODSPartEntity in ODSPartEntities)
                    {

                        var ODS_Number = ODSPartEntity.ODSNo;
                        var ConnectionId = TransformerUtils.GetNewArasGuid();
                        var ConfigId = ConnectionId;
                        var KEYED_NAME = ConnectionId;
                        var PERMISSION_ID = "95475AE006E7415794BDC93808DC04D2";
                        var CREATED_ON = DateTime.Now;
                        var CREATED_BY_ID = "Data Migration";
                        var MODIFIED_ON = DateTime.Now.ToString();
                        var MODIFIED_BY_ID = "Data Migration";
                        var IS_RELEASED = "1";
                        var STATE = "Released";
                        var IS_CURRENT = "1";
                        var MAJOR_REV = "A";
                        var NOT_LOCKABLE = "0";
                        var GENERATION = "1";
                        var RELATED_ITEMTYPE = "4F1AC04A2B484F3ABA4E20DB63808A88";
                        var condition = ODSPartEntity.Condition;
                        var ExpressionId = ODSPartEntity.ExpressionID;

                        var PN = "";
                        var RelatedID = "";
                        var SourceID = "";

                        if (!condition.Contains("|"))
                            PN = condition;

                        if (BreakDown.ContainsKey(ODS_Number))
                        {
                            SourceID = BreakDown[ODS_Number];
                        }
                        else
                        {
                            failedFilesDataFileWriter.WriteRow($"{PN}\t{ODS_Number}\tMissing ODS\n");
                            continue;
                        }

                        var QTY = ODSPartEntity.QT;

                        if (QTY == "QT")
                        {
                            var parts = condition.Split('|');
                            PN = parts.Length >= 2 ? parts[parts.Length - 2] : "";
                            QTY = condition.Split('|').Last();
                        }
                        else
                        {
                            PN = condition.Split('|').Last();
                        }

                        PN = PN?.Trim();
                        if (!string.IsNullOrWhiteSpace(PN) && !PN.Equals("PN", StringComparison.OrdinalIgnoreCase) && Part.ContainsKey(PN) && BreakDown.ContainsKey(ODS_Number))
                        {
                            RelatedID = Part[PN];
                            SourceID = BreakDown[ODS_Number];
                        }
                        else
                        {
                            failedFilesDataFileWriter.WriteRow($"{PN}\t{ODS_Number}\tMissing Part\n");
                            continue;
                        }

                        PartDataFileWriter.WriteRow($"{ConnectionId}\t{ConfigId}\t{KEYED_NAME}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{IS_CURRENT}\t{MAJOR_REV}\t{STATE}\t{IS_RELEASED}\t{NOT_LOCKABLE}\t{GENERATION}\t{PERMISSION_ID}\t{SourceID}\t{QTY}\t{RELATED_ITEMTYPE}\t{RelatedID}\t{ExpressionId}\n");
                        objectCount++;
                    }

                }
            }

            _migrationDiagnostics.LogTransformTypeStatus(transformName, typeName, TransformStatus.Completed, objectCount, failedObjectCount);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, typeName);
        }
    }
}
