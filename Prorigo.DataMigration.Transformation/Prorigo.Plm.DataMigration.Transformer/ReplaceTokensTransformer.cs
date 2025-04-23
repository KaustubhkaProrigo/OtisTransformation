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
    public class ReplaceTokensTransformer : IDataTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatToTsvTransformer> _logger;

        private readonly string _processAreaDataPath;
        private readonly string[] _tokens;
        private readonly string[] _replacements;

        public ReplaceTokensTransformer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<DatToTsvTransformer> logger)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;

            var replaceTokensSection = _configuration.GetSection("ReplaceTokens");
            _processAreaDataPath = replaceTokensSection.GetValue<string>("ProcessAreaDataPath");
            _tokens = replaceTokensSection.GetSection("Tokens").Get<string[]>();
            _replacements = replaceTokensSection.GetSection("Replacements").Get<string[]>();
        }

        public void Transform(string LicenseKey)
        {
            Console.WriteLine($"Transformation Started at: {DateTime.Now}");

            //License key
            bool isLicenValid = LicenseUtils.ValidateLicenKey(LicenseKey, "", "DMF");
            if (isLicenValid)
            {
                if (_tokens.Length != _replacements.Length)
                throw new Exception("Tokens length should be same as replacements length");

                var tokenReplacementMap = new Dictionary<string, string>();
                for (int i = 0; i < _tokens.Length; i++)
                    tokenReplacementMap[_tokens[i]] = _replacements[i];

                TrasnformSubDirectories(_processAreaDataPath, tokenReplacementMap);
            }
            else
            {
                Console.Error.WriteLine($"License Key is Missing");
                Console.Error.Flush();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Transformation Completed at: {DateTime.Now}");
        }

        private void TrasnformSubDirectories(string directoryName, Dictionary<string, string> tokenReplacementMap)
        {
            var subDirectories = Directory.GetDirectories(directoryName);
            foreach (var subDirectoryName in subDirectories)
            {
                FileInfo[] subDirectoryFiles = new DirectoryInfo(subDirectoryName).GetFiles("*.tsv");
                foreach (FileInfo subDirectoryFile in subDirectoryFiles)
                {
                    TransformFile(subDirectoryFile.FullName, tokenReplacementMap);
                }

                TrasnformSubDirectories(subDirectoryName, tokenReplacementMap);
            }
        }

        private void TransformFile(string fileNameWithPath, Dictionary<string, string> tokenReplacementMap)
        {
            string fileContent = null;
            using (var datFileReader = new StreamReader(fileNameWithPath))
            {
                fileContent = datFileReader.ReadToEnd();

                foreach (var tokenReplacement in tokenReplacementMap)
                    fileContent = fileContent.Replace(tokenReplacement.Key, tokenReplacement.Value);
            }

            if (!string.IsNullOrWhiteSpace(fileContent))
            {
                File.WriteAllText(fileNameWithPath, fileContent);
            }
        }
    }
}
