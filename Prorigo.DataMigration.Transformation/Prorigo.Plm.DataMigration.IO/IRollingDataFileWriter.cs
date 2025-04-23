using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Prorigo.Plm.DataMigration.IO
{
    public interface IRollingDataFileWriter : IDisposable
    {
        string TypeName { get; set; }

        string FileBaseName { get; set; }

        string HeaderRow { get; set; }

        void WriteRow(string rowData);

        void WriteAllEntities<T>(List<T> entities) where T : IWritableEntity;
    }
}
