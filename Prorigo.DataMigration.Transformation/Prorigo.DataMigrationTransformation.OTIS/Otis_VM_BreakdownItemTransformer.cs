using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Transformer.Metrics;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Engineering;
using Prorigo.DataMigrationTransformation.OTIS.Entities;
using Prorigo.Plm.DataMigration.OtisDataTransformer;
using Prorigo.Plm.DataMigration.Utilities;


namespace Prorigo.DataMigrationTransformation.OTIS
{
    public class Otis_VM_BreakdownItemTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Otis_VM_BreakdownItemTransformer> _logger;
        private readonly IMigrationDiagnostics _migrationDiagnostics;

        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private readonly IConfigurationSection _typesConfigSection;
        private const string vm_breakdownItem = "VM_Breakdownitem";



        public Otis_VM_BreakdownItemTransformer(IConfiguration configuration, ILogger<Otis_VM_BreakdownItemTransformer> logger, IMigrationDiagnostics migrationDiagnostics)
        {
            _configuration = configuration;
            _logger = logger;
            _migrationDiagnostics = migrationDiagnostics;

            var CADValidationSection = _configuration.GetSection("Otis_VM_BreakdownItem");
            _processAreaDataPath = CADValidationSection.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = CADValidationSection.GetValue<long>("ObjectCountPerFile");
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
            _migrationDiagnostics.LogTransformTypeStartTime(transformName, vm_breakdownItem);
            _migrationDiagnostics.LogTransformTypeStatus(transformName, vm_breakdownItem, TransformStatus.InProgress);

            var VM_BreakdownItemWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, vm_breakdownItem), _objectCountPerFile)
            {
                FileBaseName = $"VM_BreakdownItem",
                TypeName = "BreakdownItem",
                FileExtension = "tsv",
            };



            var VM_BreakdownItemReader = new TypeDataFileReader(_processAreaDataPath);
            var VM_BreakdownItemEntities = VM_BreakdownItemReader.ReadAllEntities<OtisVM_BreakdownItemEntity>(vm_breakdownItem);



            long successCount = 0;


            using (VM_BreakdownItemWriter)
            {
                foreach (var vm_BreakdownItemEntity in VM_BreakdownItemEntities)
                {
                    if (VM_BreakdownItemWriter.HeaderRow == null)
                    {
                        VM_BreakdownItemWriter.HeaderRow = "ARAS_UNIQUENESS_HELPER\tid\tKEYED_NAME\tCREATED_ON\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tCURRENT_STATE\tSTATE" +
                         "\tIS_CURRENT\tMAJOR_REV\tMINOR_REV\tIS_RELEASED\tNOT_LOCKABLE\tGENERATION\tNEW_VERSION\tCONFIG_ID\tPERMISSION_ID\tDESCRIPTION\tClassification\tItem_Number\tName\n";
                    }

                    vm_BreakdownItemEntity.id = TransformerUtils.GetNewArasGuid();
                    vm_BreakdownItemEntity.ARAS_UNIQUENESS_HELPER = vm_BreakdownItemEntity.id;
                    vm_BreakdownItemEntity.KEYED_NAME = vm_BreakdownItemEntity.Name;
                    vm_BreakdownItemEntity.CREATED_ON = DateTime.Now.ToString();
                    vm_BreakdownItemEntity.CREATED_BY_ID = "Data Migration";
                    vm_BreakdownItemEntity.MODIFIED_ON = DateTime.Now.ToString();
                    vm_BreakdownItemEntity.MODIFIED_BY_ID = "Data Migration";
                    vm_BreakdownItemEntity.CURRENT_STATE = "B9CD21B6F6314ACBB9727CFECE9EFEFB";
                    vm_BreakdownItemEntity.STATE = "Released";
                    vm_BreakdownItemEntity.IS_CURRENT = "1";
                    vm_BreakdownItemEntity.MAJOR_REV = vm_BreakdownItemEntity.Windchill_Revision;
                    vm_BreakdownItemEntity.MINOR_REV = vm_BreakdownItemEntity.Windchill_Revision;

                    if (vm_BreakdownItemEntity.MAJOR_REV.Contains("."))
                    {
                        vm_BreakdownItemEntity.MAJOR_REV = vm_BreakdownItemEntity.MAJOR_REV.Substring(0, vm_BreakdownItemEntity.MAJOR_REV.IndexOf('.'));
                    }
                    if (vm_BreakdownItemEntity.MINOR_REV.Contains("."))
                    {
                        vm_BreakdownItemEntity.MINOR_REV = vm_BreakdownItemEntity.MINOR_REV.Substring(vm_BreakdownItemEntity.MINOR_REV.IndexOf('.') + 1);
                    }

                    vm_BreakdownItemEntity.IS_RELEASED = "1";
                    vm_BreakdownItemEntity.NOT_LOCKABLE = "0";
                    vm_BreakdownItemEntity.GENERATION = "1";
                    vm_BreakdownItemEntity.NEW_VERSION = "1";
                    vm_BreakdownItemEntity.CONFIG_ID = vm_BreakdownItemEntity.id;
                    vm_BreakdownItemEntity.PERMISSION_ID = "6B1D34D6B1D246DBAE349E157B6B56CC";
                    vm_BreakdownItemEntity.DESCRIPTION = vm_BreakdownItemEntity.DESCRIPTION;
                    vm_BreakdownItemEntity.Type = vm_BreakdownItemEntity.Type;
                    vm_BreakdownItemEntity.Name = vm_BreakdownItemEntity.Name;
                    vm_BreakdownItemEntity.Data_Number = vm_BreakdownItemEntity.Data_Number;

                    VM_BreakdownItemWriter.WriteRow(vm_BreakdownItemEntity.DataRow);
                    

                    successCount++;
                }
            }
            _migrationDiagnostics.LogTransformTypeStatus(transformName, vm_breakdownItem, TransformStatus.Completed, successCount, 0);
            _migrationDiagnostics.LogTransformTypeEndTime(transformName, vm_breakdownItem);
        }
    }
}
