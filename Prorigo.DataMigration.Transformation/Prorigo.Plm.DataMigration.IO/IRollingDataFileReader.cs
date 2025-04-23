using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Prorigo.Plm.DataMigration.IO
{
    public interface IRollingDataFileReader
    {
        string HeaderRow { get; }

        List<R> ReadAllEntities<T, R>(string typeName, bool loadAdditionalColumns = false) where T : R, IReadableEntity;

        IEnumerable<T> ReadAllEntities<T>(string typeName, string searchPattern, bool loadAdditionalColumns = false) where T : IReadableEntity;

        IEnumerable<T> ReadAllEntities<T>(string typeName, bool loadAdditionalColumns = false) where T : IReadableEntity;
    }
}
