using System;
using System.Collections.Generic;
using System.Text;

namespace Prorigo.Plm.DataMigration.IO
{
    public interface IWritableEntity
    {
        string DataRow
        {
            get;
        }
    }
}
