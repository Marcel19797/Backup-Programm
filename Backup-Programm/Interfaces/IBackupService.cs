using Backup_Programm.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backup_Programm.Interfaces
{

    public interface IBackupService
    {
        /// <summary>
        /// Führt ein Backup der angegebenen Pfade durch und meldet den Fortschritt über das IProgress-Interface.
        /// </summary>
        /// <param name="paths">Die Liste der Backup-Pfade, die gesichert werden sollen.</param>
        /// <param name="progress">Das IProgress-Interface, über das der Fortschritt gemeldet wird.</param>
        /// <returns>Ein Task, der die asynchrone Ausführung des Backups darstellt.</returns>
        /// <remarks> 
        /// Diese Methode führt das Backup asynchron aus und meldet den Fortschritt über das angegebene IProgress-Interface.
        /// </remarks>
        Task ExecuteBackupAsync(List<BackupPath> paths, IProgress<string> progress);

    }
}
