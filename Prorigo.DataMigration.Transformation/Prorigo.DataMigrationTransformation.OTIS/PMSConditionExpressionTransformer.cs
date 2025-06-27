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
using System.Linq;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    internal class PMSConditionExpressionTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PMSConditionExpressionTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;

        string ConditionExpresion = "ConditionExpression";
        public PMSConditionExpressionTransformer(IConfiguration configuration, ILogger<PMSConditionExpressionTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var ConditionExpressionSection = _configuration.GetSection("PMSConditionExpression");
            _processAreaDataPath = ConditionExpressionSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = ConditionExpressionSection.GetValue<long>("ObjectCountPerFile");
        }

        public void Transform(string LicenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");
            if (isLicenValid)
            {
                var className = this.GetType().Name;
                var transformName = className.Substring(0, className.IndexOf("Transformer"));

                DefaultValueAdder(transformName);

            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }
        public void DefaultValueAdder(string transformName)
        {
            var ConditionExpressionWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "PMS"), _objectCountPerFile)
            {
                FileBaseName = $"TR_ConditionExpression",
                TypeName = $"PMS_OutputConditionExpression",
                FileExtension = "tsv",
                HeaderRow = "PMSNo\tParameterName\tConditionExpression\tConditionTable\tisExpression\n"
            };

            var FormulaLinkupReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "BOM Files", "PMS"));
            var FormulaLinkupEntities = FormulaLinkupReader.ReadAllEntities<PMSOutputEntity>("Output", "*.tsv");


            long successCount = 0;

            using (ConditionExpressionWriter)
            {
                Dictionary<int, string> headerIndexDict = new Dictionary<int, string>();
                Dictionary<string, string> headerValues = new Dictionary<string, string>();

                var FormulaEntities = new List<PMSOutputEntity>();
                GroupEntity(FormulaLinkupEntities, FormulaEntities);

                void GroupEntity(IEnumerable<PMSOutputEntity> FormulaLinkupEntities, List<PMSOutputEntity> FormulaEntities)
                {
                    var EntityGroups = FormulaLinkupEntities.GroupBy(p => new { p.ID, p.ParameterName });

                    foreach (var EntityGroup in EntityGroups)
                    {
                        if (EntityGroup.Any(e => !string.IsNullOrWhiteSpace(e.ProductRangeValueVerification)))
                            continue;

                        var PMSNO = string.Empty;
                        var ParameterName = string.Empty;
                        var ConditionExpression = string.Empty;
                        var ConditionTable = string.Empty;

                        if (EntityGroup.Count() == 1)
                        {
                            var Id = EntityGroup.Key;
                            var entities = EntityGroup.OrderBy(e => e.ParameterName);
                            foreach (var entity in entities)
                            {
                                FormulaEntities.Add(entity);
                                PMSNO = entity.PMSNO;
                                ParameterName = entity.ParameterName;
                                var isExpression = "1";
                                ConditionExpressionWriter.WriteRow($"{PMSNO}\t{ParameterName}\t{entity.Formula}\t{ConditionTable}\t{isExpression}\n");
                            }
                        }
                        else
                        {
                            var Id = EntityGroup.Key;
                            var entities = EntityGroup.OrderBy(e => e.ParameterName);

                            var values = new List<string>();

                            var conditionParts = new List<string>();
                            var conditionGroup = new List<string>();
                            string finalOutput = "";

                            foreach (var entity in entities)
                            {
                                if (headerIndexDict.Count() == 0)
                                {
                                    ParameterName = entity.ParameterName;

                                    var splitheaders = entity.Condition.Split('|');
                                    int index = 0;
                                    foreach (var header in splitheaders)
                                    {
                                        headerIndexDict[index] = header;
                                        index++;
                                    }
                                }
                                else
                                {
                                    var Formula = entity.Formula;
                                    PMSNO = entity.PMSNO;
                                    ParameterName = entity.ParameterName;

                                    var splitConditions = entity.Condition.Split('|');
                                    for (int i = 0; i < splitConditions.Length; i++)
                                    {
                                        var value = splitConditions[i];
                                        var header = headerIndexDict[i];

                                        value = $"\"{value}\"";

                                        if (!string.IsNullOrEmpty(header))
                                        {
                                            headerValues[header] = value;
                                        }

                                        foreach (var kvp in headerValues)
                                        {
                                            conditionParts.Add($"\"{kvp.Key}\" : {kvp.Value}");
                                        }
                                        headerValues.Clear();
                                    }
                                    string group = "{" + string.Join(",", conditionParts) + ",\"VALUE\" : \"" + Formula + "\"},";
                                    conditionGroup.Add(string.Join("\t", group));
                                    conditionParts.Clear();
                                }
                                finalOutput = finalOutput + string.Join("", conditionGroup);
                                conditionGroup.Clear();
                            }
                            ConditionExpressionWriter.WriteRow($"{PMSNO}\t{ParameterName}\t{ConditionExpression}\t[{finalOutput.TrimEnd(',')}]\t{"0"}\n");
                            headerIndexDict.Clear();
                            successCount++;
                        }
                    }
                }
            }
            _migrationDiagnostics.LogTransformTypeStatus(transformName, ConditionExpresion, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, ConditionExpresion);
        }
    }
}