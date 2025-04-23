using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.Plm.DataMigration.Transformer
{
    public interface IDataTransformer
    {
        public void Transform(string LicenseKey);
    }
}
