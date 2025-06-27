using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Prorigo.Plm.IntegratedLicensing;
using System.Net.NetworkInformation;

namespace Prorigo.Plm.DataMigration.Utilities
{
    public static class LicenseUtils
    {
        public static bool ValidateLicenKey(string licenKey, string modulename, string productcode)
        {
            bool isValid = false;

            if (!string.IsNullOrEmpty(licenKey))
            {            // Decrypt and Validate               
                try
                {
                    //string publickeyxml = "<RSAKeyValue><Modulus>sC4Qb7ehe2gyJ6UU17+AyzGZTJZI8QO9CIoKoPKajLntj2hzXvC57F2CZhvhRLbqzNZxCyFAHhcFlEtEUsSF8nEuLGh7hvN5a7OtQSWtCNvuMxNTKkiFlKxm/Xg+tAB7K+M7b/kPMVdujc1N8+ObckMEZhD766Czl1U9rH4PvUR70EOyn7rDTpi/UqeeciXCuSucFQFB7ZfvKs3zAKo5XeXwc3JdRwIJSQKUp84Yl03DAvsHXmrDTCIr4KbpdfrcR0s8ZoFDKLCn9eK9KQacD1eLFM1Xk78heaphxTyMJfpssC87AReO/n7cR5g6IiPd/upaBxkk2+CkYDZqfL09Jw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

                    isValid = true; // SignatureManager.ValidateSignatureIntegrity(licenKey, publickeyxml);

                    if (isValid)
                    {

                        isValid = LicenseValidator.ValidateLicense(licenKey, productcode, modulename);
                    }
                }
                catch
                {
                    isValid = false;
                }
            }
            return isValid;
        }


    }
}