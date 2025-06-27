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
    internal class ExcelToTsvPMSCalSheetTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExcelToTsvPMSCalSheetTransformer> _logger;
        private readonly string _processAreaDataPath;
        private readonly long _objectCountPerFile;

        public ExcelToTsvPMSCalSheetTransformer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<ExcelToTsvPMSCalSheetTransformer> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;

            var Configuration = _configuration.GetSection("ExcelToTsvPMSCalSheet");
            _processAreaDataPath = Configuration.GetValue<string>("ProcessAreaDataPath");
            _objectCountPerFile = Configuration.GetValue<long>("ObjectCountPerFile");

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
            string fileTypePath = Path.Combine(directoryName, "BOM Files", "PMS");
            if (Directory.Exists(fileTypePath))
            {
                var excelFiles = Directory.GetFiles(fileTypePath, "*.xlsx");
                foreach (var excelFile in excelFiles)
                {
                    TransformFile(excelFile);
                }
            }
        }

        private void TransformFile(string fileNameWithPath)
        {
            var fileExtension = Path.GetExtension(fileNameWithPath);
            if (!fileExtension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(fileNameWithPath)))
            {
                // List<string> PMSColumnList = new List<string>();
                Dictionary<string, string> PMSColumnList = new Dictionary<string, string>();
                Dictionary<int, string> splitNoDict = new Dictionary<int, string>();

                var worksheets = package.Workbook.Worksheets;
                foreach (var worksheet in worksheets)
                {
                    var worksheetName = package.Workbook.Worksheets[worksheet.Name];
                    if (worksheetName != null)
                    {
                        string PMSCellVal = worksheetName.Cells[2, 5].Value.ToString();
                        PMSCellVal = PMSCellVal.Replace("\t", "|T|").Replace("\n", "|N|").Replace("\r", "|R|").TrimEnd().TrimStart();
                        PMSCellVal = PMSCellVal.Split('-')[0];

                        string PMSCellDes = worksheetName.Cells[2, 12].Value.ToString();
                        PMSCellDes = PMSCellDes.Replace("\t", "|T|").Replace("\n", "|N|").Replace("\r", "|R|").TrimEnd().TrimStart();
                        PMSCellDes = PMSCellDes.Split('-')[0];

                        if (!PMSColumnList.ContainsKey(PMSCellVal))
                        {
                            PMSColumnList.Add(PMSCellVal, PMSCellDes);
                        }
                        splitNoDict.Clear();
                    }
                    GenerateTsv(fileNameWithPath, worksheet.Name, PMSColumnList);
                }
            }
        }
        private void GenerateTsv(string fileNameWithPath, string sheetName, Dictionary<string, string> PMSColumnList)
        {
            string excelFileName = Path.GetFileNameWithoutExtension(fileNameWithPath);
            string sheetTsvFileName = $"{excelFileName}CalCulationSheet.tsv";

            var PMSCalculationSheetWriter = new TypeDataFileWriter(Path.Combine(_processAreaDataPath, "PMS"), _objectCountPerFile)
            {
                FileBaseName = sheetTsvFileName,
                TypeName = $"CalSheet_PMS",
                FileExtension = "tsv"
            };

            var objectCountPerFile = 0;
            using (PMSCalculationSheetWriter)
            {
                if (PMSCalculationSheetWriter.HeaderRow == null)
                    PMSCalculationSheetWriter.HeaderRow = "Id\tARAS_UNIQUENESS_HELPER\tCONFIG_ID\tITEM_NUMBER\tOTS_Name\tKEYED_NAME\tDESCRIPTION\tCREATED_ON\tCREATED_BY_ID\tMODIFIED_ON\tMODIFIED_BY_ID\tPERMISSION_ID\tIS_CURRENT\tIS_RELEASED\tMAJOR_REV\tCLASSIFICATION\tNOT_LOCKABLE\tGENERATION\tSTATE\tCURRENT_STATE\n";

                foreach (var DRColumn in PMSColumnList)
                {
                    var Id = TransformerUtils.GetNewArasGuid();
                    var ARAS_UNIQUENESS_HELPER = Id;
                    var CONFIG_ID = Id;
                    var ITEM_NUMBER = DRColumn.Key;
                    var KEYED_NAME = ITEM_NUMBER;
                    var DESCRIPTION = DRColumn.Value;
                    var CREATED_ON = DateTime.Now.ToString();
                    var CREATED_BY_ID = "Data Migration";
                    var MODIFIED_ON = DateTime.Now.ToString();
                    var MODIFIED_BY_ID = "Data Migration";
                    var PERMISSION_ID = "95475AE006E7415794BDC93808DC04D2";
                    var IS_CURRENT = "1";
                    var IS_RELEASED = "1";
                    var MAJOR_REV = "A";
                    var CLASSIFICATION = "PMS";
                    var NOT_LOCKABLE = "0";
                    var GENERATION = "1";
                    var STATE = "Released";
                    var CURRENT_STATE = "95475AE006E7415794BDC93808DC04D2";
                    var OTS_Name = ITEM_NUMBER;

                    PMSCalculationSheetWriter.WriteRow($"{Id}\t{ARAS_UNIQUENESS_HELPER}\t{CONFIG_ID}\t{ITEM_NUMBER}\t{OTS_Name}\t{KEYED_NAME}\t{DESCRIPTION}\t{CREATED_ON}\t{CREATED_BY_ID}\t{MODIFIED_ON}\t{MODIFIED_BY_ID}\t{PERMISSION_ID}\t{IS_CURRENT}\t{IS_RELEASED}\t{MAJOR_REV}\t{CLASSIFICATION}\t{NOT_LOCKABLE}\t{GENERATION}\t{STATE}\t{CURRENT_STATE}\n");
                    objectCountPerFile++;
                }
            }
        }
    }
}