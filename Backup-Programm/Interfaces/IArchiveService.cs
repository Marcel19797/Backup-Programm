using System;
using System.Collections.Generic;
using System.Text;

namespace Backup_Programm.Interfaces
{

    public interface IArchiveService
    {
        /// <summary>
        /// Archiviert das angegebene Verzeichnis in eine Archivdatei und meldet den Fortschritt über das IProgress-Interface.
        /// </summary>
        /// <param name="sourcePath">Der Quellpfad des Verzeichnisses, das archiviert werden soll.</param>
        /// <param name="destinationArchiveFile">Der Zielpfad der Archivdatei.</param>
        /// <param name="progress">Das IProgress-Interface, über das der Fortschritt gemeldet wird.</param>
        /// <returns>Ein Task, der die asynchrone Operation darstellt.</returns>
        /// <remarks>
        /// Diese Methode führt die Archivierung asynchron aus und meldet den Fortschritt über das angegebene IProgress-Interface.
        /// </remarks>
        Task ArchiveDirectoryAsync(string sourcePath, string destinationArchiveFile, IProgress<string> progress);

        /// <summary>
        /// Archiviert das angegebene Verzeichnis inkrementell basierend auf einer Basisarchivdatei und meldet den Fortschritt über das IProgress-Interface.
        /// </summary>
        /// <param name="sourcePath">Der Quellpfad des Verzeichnisses, das archiviert werden soll.</param>
        /// <param name="baseArchiveFile">Die Basisarchivdatei, auf der die inkrementelle Archivierung basiert.</param>
        /// <param name="incrementalDestArchive">Der Zielpfad der inkrementellen Archivdatei.</param>
        /// <param name="progress">Das IProgress-Interface, über das der Fortschritt gemeldet wird.</param>
        /// <returns>Ein Task, der die asynchrone Operation darstellt.</returns>
        /// <remarks>
        /// Diese Methode führt die inkrementelle Archivierung asynchron aus und meldet den Fortschritt über das angegebene IProgress-Interface.
        /// </remarks>
        Task ArchiveIncrementalAsync(string sourcePath, string baseArchiveFile, string incrementalDestArchive, IProgress<string> progress);
    }
}
