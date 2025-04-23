using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Prorigo.Plm.DataMigration.Transformer
{
    public static class TransformerUtils
    {
        public static string GetNewArasGuid() => Guid.NewGuid().ToString().Replace("-", string.Empty).ToUpper();

        public static string CalculateMD5(string filePath)
        {
            //In case file reading fails because of Network error, retry after 10 seconds
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
            catch(Exception exception)
            {
                Thread.Sleep(10000);
                return CalculateMD5(filePath);
            }
        }

        public static string CopyFileToArasVaultDirectory(string filePath, string fileId, string vaultDirectory)
        {
            var targetFilePath = string.Empty;

            try
            {
                if (File.Exists(filePath))
                {
                    var directoryStructure = $"{vaultDirectory}\\{fileId.Substring(0, 1)}\\{fileId.Substring(1, 2)}\\{fileId.Substring(3)}";
                    Directory.CreateDirectory(directoryStructure);

                    var targetFileName = Path.GetFileName(filePath);
                    targetFilePath = Path.Combine(directoryStructure, targetFileName);
                    if (!File.Exists(targetFilePath))
                    {
                        File.Copy(filePath, targetFilePath);
                    }
                }
            }
            catch(Exception exception)
            {
                Thread.Sleep(10000);
                targetFilePath = CopyFileToArasVaultDirectory(filePath, fileId, vaultDirectory);
            }

            return targetFilePath;
        }

        public static string MoveFileToArasVaultDirectory(string filePath, string fileId, string vaultDirectory)
        {
            var targetFilePath = string.Empty;

            try
            {
                if (File.Exists(filePath))
                {
                    var directoryStructure = $"{vaultDirectory}\\{fileId.Substring(0, 1)}\\{fileId.Substring(1, 2)}\\{fileId.Substring(3)}";
                    Directory.CreateDirectory(directoryStructure);

                    var targetFileName = Path.GetFileName(filePath);
                    targetFilePath = Path.Combine(directoryStructure, targetFileName);
                    if (!File.Exists(targetFilePath))
                    {
                        File.Move(filePath, targetFilePath);
                    }
                }
            }
            catch (Exception exception)
            {
                Thread.Sleep(10000);
                targetFilePath = MoveFileToArasVaultDirectory(filePath, fileId, vaultDirectory);
            }

            return targetFilePath;
        }
    }
}
