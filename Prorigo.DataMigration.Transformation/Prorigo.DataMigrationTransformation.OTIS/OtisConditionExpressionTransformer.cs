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
using System.Linq.Expressions;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Vml.Office;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    internal class OtisConditionExpressionTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtisConditionExpressionTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;
        private string[] _processType;

        string ConditionExpresion = "ConditionExpression";
        public OtisConditionExpressionTransformer(IConfiguration configuration, ILogger<OtisConditionExpressionTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var ConditionExpressionSection = _configuration.GetSection("OtisConditionExpression");
            _processAreaDataPath = ConditionExpressionSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = ConditionExpressionSection.GetValue<long>("ObjectCountPerFile");
            _processType = ConditionExpressionSection.GetSection("ProcessType").Get<string[]>();
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
            var ConditionExpressionWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "FormulaLinkupParameters"), _objectCountPerFile)
            {
                FileBaseName = $"TR{processtype}_ConditionExpression",
                TypeName = $"{processtype}ConditionExpression",
                FileExtension = "tsv",
                HeaderRow = "DrawingNo\tParameterName\tConditionExpression\tConditionTable\tisExpression\n"
            };

            var FormulaLinkupReader = new TypeDataFileReader(Path.Combine(_processAreaDataPath, "FormulaLinkupParameters"));
            var FormulaLinkupEntities = FormulaLinkupReader.ReadAllEntities<FormulaLinkupEntity>(processtype, "*.tsv");


            long successCount = 0;

            using (ConditionExpressionWriter)
            {
                Dictionary<int, string> headerIndexDict = new Dictionary<int, string>();
                Dictionary<string, string> headerValues = new Dictionary<string, string>();

                var FormulaEntities = new List<FormulaLinkupEntity>();
                GroupEntity(FormulaLinkupEntities, FormulaEntities);

                void GroupEntity(IEnumerable<FormulaLinkupEntity> FormulaLinkupEntities, List<FormulaLinkupEntity> FormulaEntities)
                {
                    var EntityGroups = FormulaLinkupEntities.GroupBy(p => new { p.ID, p.DrawingNo });

                    foreach (var EntityGroup in EntityGroups)
                    {
                        var DrawingNo = string.Empty;
                        var PropertyName = string.Empty;
                        var ConditionExpression = string.Empty;
                        var ConditionTable = string.Empty;

                        if (EntityGroup.Count() == 1)
                        {
                            var Id = EntityGroup.Key;
                            var entities = EntityGroup.OrderBy(e => e.PropertyName);
                            foreach (var entity in entities)
                            {
                                FormulaEntities.Add(entity);
                                DrawingNo = entity.DrawingNo;
                                PropertyName = entity.PropertyName;
                                var isExpression = "1";
                                ConditionExpressionWriter.WriteRow($"{DrawingNo}\t{PropertyName}\t{entity.Formula}\t{ConditionTable}\t{isExpression}\n");
                            }
                        }
                        else
                        {
                            var Id = EntityGroup.Key;
                            var entities = EntityGroup.OrderBy(e => e.PropertyName);

                            var values = new List<string>();

                            var conditionParts = new List<string>();
                            var conditionGroup = new List<string>();
                            string finalOutput = "";

                            foreach (var entity in entities)
                            {
                                if (headerIndexDict.Count() == 0)
                                {
                                    PropertyName = entity.PropertyName;

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
                                    DrawingNo = entity.DrawingNo;
                                    PropertyName = entity.PropertyName;

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
                            ConditionExpressionWriter.WriteRow($"{DrawingNo}\t{PropertyName}\t{ConditionExpression}\t[{finalOutput.TrimEnd(',')}]\t{"0"}\n");
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
