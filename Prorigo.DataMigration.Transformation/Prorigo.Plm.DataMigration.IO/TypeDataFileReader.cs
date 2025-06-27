using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Prorigo.Plm.DataMigration.IO
{
    public class TypeDataFileReader : IRollingDataFileReader
    {
        private readonly string _typeParentDataPath;

        public TypeDataFileReader(string typeParentDataPath)
        {
            _typeParentDataPath = typeParentDataPath;
        }

        public string HeaderRow { get; private set; }

        public string FileName { get; private set; }

        public int LineNumber { get; private set; }

        public List<R> ReadAllEntities<T,R>(string typeName, bool loadAdditionalColumns = false) where T : R, IReadableEntity
        {
            var typeMetadataEntries = new List<R>();

            var typeMetadataDirectory = Path.Combine(_typeParentDataPath, typeName);
            var typeMetadataFiles = Directory.GetFiles(typeMetadataDirectory);
            foreach (var typeMetadataFile in typeMetadataFiles)
            {
                FileName = typeMetadataFile;
                LineNumber = 0;
                using (var reader = new StreamReader(typeMetadataFile))
                {
                    HeaderRow = ReadLine(reader);
                    var dataRow = ReadLine(reader);
                    while (!string.IsNullOrWhiteSpace(dataRow))
                    {
                        object[] arguments = null;
                        if(loadAdditionalColumns)
                            arguments = new object[] { dataRow, loadAdditionalColumns };
                        else
                            arguments = new object[] { dataRow };

                        R entity;

                        try
                        {
                            entity = (R)Activator.CreateInstance(typeof(T), arguments);

                        }
                        catch (Exception exception)
                        {
                            var errorMessage = $"Failed to load row '{dataRow}' from file '{typeMetadataFile}'";
                            throw new Exception(errorMessage, exception);
                        }

                        typeMetadataEntries.Add(entity);
                        dataRow = ReadLine(reader);
                    }
                }
            }

            return typeMetadataEntries;
        }

        public IEnumerable<T> ReadAllEntities<T>(string typeName, string searchPattern, bool loadAdditionalColumns = false) where T : IReadableEntity
        {
            var typeMetadataDirectory = Path.Combine(_typeParentDataPath, typeName);
            var typeMetadataFiles = Directory.GetFiles(typeMetadataDirectory, searchPattern);
            foreach (var typeMetadataFile in typeMetadataFiles)
            {
                FileName = typeMetadataFile;
                LineNumber = 0;
                using (var reader = new StreamReader(typeMetadataFile))
                {
                    HeaderRow = ReadLine(reader);
                    var dataRow = ReadLine(reader);
                    while (!string.IsNullOrWhiteSpace(dataRow))
                    {
                        object[] arguments = null;
                        if (loadAdditionalColumns)
                            arguments = new object[] { dataRow, loadAdditionalColumns };
                        else
                            arguments = new object[] { dataRow };

                        T entity;

                        try
                        {
                            entity = (T)Activator.CreateInstance(typeof(T), arguments);
                        }
                        catch (Exception exception)
                        {
                            var errorMessage = $"Failed to load row '{dataRow}' from file '{typeMetadataFile}'";
                            throw new Exception(errorMessage, exception);
                        }

                        yield return entity;

                        dataRow = ReadLine(reader);
                    }
                }
            }
        }


        public IEnumerable<T> ReadAllEntities<T>(string typeName, bool loadAdditionalColumns = false) where T : IReadableEntity
        {
            var typeMetadataDirectory = Path.Combine(_typeParentDataPath, typeName);
            var typeMetadataFiles = Directory.GetFiles(typeMetadataDirectory);
            foreach (var typeMetadataFile in typeMetadataFiles)
            {
                FileName = typeMetadataFile;
                LineNumber = 0;
                using (var reader = new StreamReader(typeMetadataFile))
                {
                    HeaderRow = ReadLine(reader);
                    var dataRow = ReadLine(reader);
                    while (!string.IsNullOrWhiteSpace(dataRow))
                    {
                        object[] arguments = null;
                        if (loadAdditionalColumns)
                            arguments = new object[] { dataRow, loadAdditionalColumns };
                        else
                            arguments = new object[] { dataRow };

                        T entity;

                        try
                        {
                            entity = (T)Activator.CreateInstance(typeof(T), arguments);
                        }
                        catch (Exception exception)
                        {
                            var errorMessage = $"Failed to load row '{dataRow}' from file '{typeMetadataFile}'";
                            throw new Exception(errorMessage, exception);
                        }

                        yield return entity;

                        dataRow = ReadLine(reader);
                    }
                }
            }
        }

        public IEnumerable<string> ReadAllLines(string typeName, string searchPattern = null)
        {
            var typeMetadataDirectory = Path.Combine(_typeParentDataPath, typeName);

            string[] typeMetadataFiles = null;
            if(!string.IsNullOrWhiteSpace(searchPattern))
                typeMetadataFiles = Directory.GetFiles(typeMetadataDirectory, searchPattern);
            else
                typeMetadataFiles = Directory.GetFiles(typeMetadataDirectory);

            foreach (var typeMetadataFile in typeMetadataFiles)
            {
                FileName = typeMetadataFile;
                LineNumber = 0;
                using (var reader = new StreamReader(typeMetadataFile))
                {
                    HeaderRow = ReadLine(reader);
                    var dataRow = ReadLine(reader);
                    while (!string.IsNullOrWhiteSpace(dataRow))
                    {
                        yield return dataRow;

                        dataRow = ReadLine(reader);
                    }
                }
            }
        }

        private string ReadLine(StreamReader streamReader)
        {
            var dataRow = streamReader.ReadLine();
            LineNumber++;

            return dataRow;
        }
    }
}

