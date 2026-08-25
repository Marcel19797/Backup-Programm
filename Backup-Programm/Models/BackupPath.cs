using System;
using System.Collections.Generic;
using System.Text;

namespace Backup_Programm.Models
{
    public class BackupPath
    {
        /// <summary>
        /// Setzt den Quellpfad für das Backup.
        /// </summary>
        /// <remarks>Der Quellpfad sollte ein gültiger Verzeichnispfad sein.</remarks>
        public string SourcePath { get; set; }
        /// <summary>
        /// Setzt den Zielpfad für das Backup.
        /// </summary>
        /// <remarks>Der Zielpfad sollte ein gültiger Verzeichnispfad sein.</remarks>
        public string DestinationPath { get; set; }
        /// <summary>
        /// Setzt das Datum des letzten Backups.
        /// </summary>
        /// <remarks>Dieses Datum wird verwendet, um zu bestimmen, ob ein Backup durchgeführt werden muss.</remarks>
        public DateTime LastBackupDate { get; set; }
        /// <summary>
        /// Initialisiert eine neue Instanz der BackupPath-Klasse.
        /// </summary>
        /// <param name="sourcePath">Der Quellpfad für das Backup.</param>
        /// <param name="destinationPath">Der Zielpfad für das Backup.</param>
        /// <remarks>Das Datum des letzten Backups wird auf DateTime.MinValue gesetzt.</remarks>
        public BackupPath(string sourcePath, string destinationPath)
        {
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
            LastBackupDate = DateTime.MinValue;
        }
    }
}
