using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Prorigo.Plm.DataMigration.IO;
using Prorigo.Plm.DataMigration.Transformer;
using Prorigo.Plm.DataMigration.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Prorigo.DataMigrationTransformation.OTIS
{
    public class ExcelEBOMCalculationSheetTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExcelEBOMCalculationSheetTransformer> _logger;
        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;
        private string[] _processType;

        public ExcelEBOMCalculationSheetTransformer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<ExcelEBOMCalculationSheetTransformer> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;

            var Configuration = _configuration.GetSection("ExcelEBOMCalculationSheet");
            _processAreaDataPath = Configuration.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = Configuration.GetValue<long>("ObjectCountPerFile");
            _processType = Configuration.GetSection("ProcessType").Get<string[]>();

        }
        public void Transform(string LicenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");
            if (isLicenValid)
            {
                TransformExcelFiles(_processAreaDataPath);
            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }
        private void TransformExcelFiles(string directoryName)
        {
            foreach (var processType in _processType)
            {
                string fileTypePath = Path.Combine(directoryName, "BOM Files", processType);
                if (Directory.Exists(fileTypePath))
                {
                    var excelFiles = Directory.GetFiles(fileTypePath, "*.xlsx");
                    foreach (var excelFile in excelFiles)
                    {
                        TransformFile(excelFile, processType);
                    }
                }
            }
        }

        private void TransformFile(string fileNameWithPath, string processType)
        {
            var fileExtension = Path.GetExtension(fileNameWithPath);
            if (!fileExtension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(fileNameWithPath)))
            {
                List<string> DRColumnList = new List<string>();

                var worksheets = package.Workbook.Worksheets;
                foreach (var worksheet in worksheets)
                {
                    if (worksheet.Name.Contains("EBOM"))
                    {
                        var worksheetName = package.Workbook.Worksheets[worksheet.Name];
                        if (worksheetName != null)
                        {
                            string DRcellVal = worksheetName.Cells[1, 3].Value.ToString();
                            DRcellVal = DRcellVal.Replace("\t", "|T|").Replace("\n", "|N|").Replace("\r", "|R|").TrimEnd().TrimStart();

                            if (!DRColumnList.Contains(DRcellVal))
                            {
                                DRColumnList.Add(DRcellVal);
                            }
                        }
                        GenerateTsv(processType, fileNameWithPath, worksheet.Name, DRColumnList);
                    }
                }
            }
        }

        private void GenerateTsv(string processType, string fileNameWithPath, string sheetName, List<string> DRColumnList)
        {
            string excelFileName = Path.GetFileNameWithoutExtension(fileNameWithPath);
            string sheetTsvFileName = $"{excelFileName}_{processType}.tsv";
            //string processFolderPath = Path.Combine(_processAreaDataPath, "CalculationSheet");

            var EBOMCalculationSheetWriter = new TypeDataFileWriter(_processAreaDataPath,_objectCountPerFile)
            {
                FileBaseName = sheetTsvFileName,
                TypeName = $"{processType}_CalculationSheet",
                FileExtension = "tsv"
            };

            var objectCountPerFile = 0;
            using (EBOMCalculationSheetWriter)
            {
                if (EBOMCalculationSheetWriter.HeaderRow == null)
                    EBOMCalculationSheetWriter.HeaderRow = "Id\tARAS_UNIQUENESS_HELPER\tCONFIG_ID\tITEM_NUMBER\tKEYED_NAME\tDESCRIPTION\tCREATED_ON\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tPERMISSION_ID\tIS_CURRENT\tIS_RELEASED\tMAJOR_REV\tMINOR_REV\tCLASSIFICATION\tNOT_LOCKABLE\tGENERATION\tSTATE\tCURRENT_STATE\tOTS_REVISION\tNEW_VERSION\n";

                foreach (var DRColumn in DRColumnList)
                {
                    var Id = TransformerUtils.GetNewArasGuid();
                    var ARAS_UNIQUENESS_HELPER = Id;
                    var CONFIG_ID = Id;
                    var ITEM_NUMBER = DRColumn;
                    var KEYED_NAME = ITEM_NUMBER;
                    var DESCRIPTION = KEYED_NAME;
                    var CREATED_ON = DateTime.Now.ToString();
                    var CREATED_BY_ID = "Data Migration";
                    var MODIFIED_ON = DateTime.Now.ToString();
                    var MODIFIED_BY_ID = "Data Migration";
                    var PERMISSION_ID = "9122CD065CF04141B8EFE263FC80BEA4";
                    var IS_CURRENT = "1";
                    var IS_RELEASED = "1";
                    var MAJOR_REV = "A";
                    var MINOR_REV = "1";
                    var CLASSIFICATION = "Drawing";
                    var NOT_LOCKABLE = "0";
                    var GENERATION = "1";
                    var STATE = "Released";
                    var CURRENT_STATE = "B9909619FB294E1AB0B8B4FDF58BD282";
                    var OTS_REVISION = "A.1";
                    var NEW_VERSION = "1";

                    EBOMCalculationSheetWriter.WriteRow($"{Id}\t{ARAS_UNIQUENESS_HELPER}\t{CONFIG_ID}\t{ITEM_NUMBER}\t{KEYED_NAME}\t{DESCRIPTION}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{PERMISSION_ID}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{MINOR_REV}\t{CLASSIFICATION}\t{NOT_LOCKABLE}\t{GENERATION}\t{STATE}\t{CURRENT_STATE}\t{OTS_REVISION}\t{NEW_VERSION}\n");
                    objectCountPerFile++;
                }
            }
        }
    }
}