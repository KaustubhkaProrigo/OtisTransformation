using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Prorigo.DataMigrationTransformation.OTIS.Extensions
{
    public static class Extensions
    {
        public static int findIndex<T>(this T[] array, T item)
        {
            return Array.IndexOf(array, item);
        }

        /// <summary>
        ///     A FileInfo extension method that renames.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="newName">Name of the new file.</param>
        /// <returns>.</returns>
        public static void Rename(this FileInfo @this, string newName)
        {
            string filePath = Path.Combine(@this.Directory.FullName, newName);
            @this.MoveTo(filePath);
        }
    }
}
