using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prorigo.Plm.DataMigration.Utilities;

namespace Prorigo.Plm.DataMigration.Transformer
{
    public class DatToTsvTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatToTsvTransformer> _logger;

        private readonly string _processAreaDataPath;
        private readonly string _datRowDelimiter;
        private readonly string _datColumnDelimiter;
        private readonly string _tsvRowDelimiter;
        private readonly string _tsvColumnDelimiter;

        private static readonly string TAB_REPLACEMENT = "|t|";
        private static readonly string NEWLINE_REPLACEMENT = "|n|";
        private static readonly string RETURN_REPLACEMENT = "|r|";

        public DatToTsvTransformer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<DatToTsvTransformer> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;

            var datToTsvSection = _configuration.GetSection("DatToTsv");
            _processAreaDataPath = datToTsvSection.GetValue<string>("ProcessAreaDataPath");
            _datRowDelimiter = datToTsvSection.GetValue<string>("DatRowDelimiter");
            _datColumnDelimiter = datToTsvSection.GetValue<string>("DatColumnDelimiter");
            _tsvRowDelimiter = datToTsvSection.GetValue<string>("TsvRowDelimiter");
            _tsvColumnDelimiter = datToTsvSection.GetValue<string>("TsvColumnDelimiter");
        }

        public void Transform(string LicenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            //License key
            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");
            if (isLicenValid)
            {

                TrasnformSubDirectories(_processAreaDataPath);
            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }

        private void TrasnformSubDirectories(string directoryName)
        {
            var subDirectories = Directory.GetDirectories(directoryName);
            foreach (var subDirectoryName in subDirectories)
            {
                FileInfo[] subDirectoryFiles = new DirectoryInfo(subDirectoryName).GetFiles();
                foreach (FileInfo subDirectoryFile in subDirectoryFiles)
                {
                    TransformFile(subDirectoryFile.FullName);
                }

                TrasnformSubDirectories(subDirectoryName);
            }
        }

        private void TransformFile(string fileNameWithPath)
        {
            var fileExtension = Path.GetExtension(fileNameWithPath);
            if (!fileExtension.Equals(".dat"))
                return;

            string fileContent = null;
            using (var datFileReader = new StreamReader(fileNameWithPath))
            {
                fileContent = datFileReader.ReadToEnd();

                fileContent = fileContent.Replace("\t", TAB_REPLACEMENT).Replace("\n", NEWLINE_REPLACEMENT).Replace("\r", RETURN_REPLACEMENT);
                fileContent = fileContent.Replace(_datRowDelimiter, _tsvRowDelimiter).Replace(_datColumnDelimiter, _tsvColumnDelimiter);
            }

            if (!string.IsNullOrWhiteSpace(fileContent))
            {
                File.WriteAllText(fileNameWithPath, fileContent);
                var fileNameWithChangedExtension = Path.ChangeExtension(fileNameWithPath, ".tsv");
                new FileInfo(fileNameWithPath).MoveTo(fileNameWithChangedExtension);
            }
        }
    }
}
