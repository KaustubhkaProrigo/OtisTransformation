using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Prorigo.Plm.DataMigration.IO
{
    public class TypeDataFileWriter : IRollingDataFileWriter
    {
        private readonly string _dataFilePath;
        private readonly long _allowedRows;
        private StreamWriter _streamWriter;

        private string _typeName;
        private int _currentFileIndex;
        private int _currentRowCount;

        public TypeDataFileWriter(string dataFilePath, long allowedRows)
        {
            _allowedRows = allowedRows;
            _dataFilePath = dataFilePath;

            FileExtension = "dat";
        }

        public string TypeName
        {
            get 
            { 
                return _typeName; 
            }
            set 
            {
                _typeName = value;
                var directoryName = Path.Combine(_dataFilePath, _typeName);
                if (!Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
            }
        }

        public string FileBaseName { get; set; }

        public string HeaderRow { get; set; }

        public string FileExtension { get; set; }

        public void WriteRow(string rowData)
        {
            if (_streamWriter == null)
            {
                _streamWriter = new StreamWriter(Path.Combine(_dataFilePath, _typeName, $"{FileBaseName}_{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}_{_currentFileIndex}.{FileExtension}"));
                _streamWriter.Write(HeaderRow);
            }

            if (_currentRowCount >= _allowedRows)
            {
                _streamWriter.Flush();
                _streamWriter.Close();
                _streamWriter = new StreamWriter(Path.Combine(_dataFilePath, _typeName, $"{FileBaseName}_{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}_{++_currentFileIndex}.{FileExtension}"));
                _currentRowCount = 0;
                _streamWriter.Write(HeaderRow);
            }

            _streamWriter.Write(rowData);

            _currentRowCount++;
        }

        public void WriteAllEntities<T>(List<T> entities) where T : IWritableEntity
        {
            foreach (var entity in entities)
                WriteRow(entity.DataRow);

            Dispose();
        }

        public void Dispose()
        {
            if (_streamWriter != null)
            {
                _streamWriter.Flush();
                _streamWriter.Close();
                _streamWriter = null;
            }
        }
    }
}
