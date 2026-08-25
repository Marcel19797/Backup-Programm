using System;
using System.Collections.Generic;
using System.Text;

namespace Backup_Programm.Models
{
    /// <summary>
    /// Statische Klasse, die die Backup-Konfiguration bereitstellt.
    /// Wird derzeit verwendet, um die Backup-Pfade zu definieren, die gesichert werden sollen.
    /// </summary>
    /// <remarks>
    /// Diese Klasse kann erweitert werden, um die Backup-Pfade aus einer Konfigurationsdatei, Datenbank oder anderen Quellen zu laden.
    /// Plamung: In Zukunft soll die Konfiguration in einer JSON-Datei gespeichert werden, um die Backup-Pfade flexibel zu gestalten und zu ändern.
    /// </remarks>
    public static class BackupConfiguration
    {
        /// <summary>
        /// Gibt die Liste der Backup-Pfade zurück.
        /// </summary>
        public static List<BackupPath> GetBackupPaths()
        {
            // Hier können Sie die Backup-Pfade aus einer Konfigurationsdatei, Datenbank oder anderen Quellen laden.
            // Für dieses Beispiel geben wir eine statische Liste zurück.
            return new List<BackupPath>
            {
                /// <summary>
                /// Backup-Pfad für Autocad-Dokumente.
                /// </summary>
                new BackupPath(@"C:\Users\Marce\Documents\Autocad", @"Z:\Users\Marce\Documents\Autocad") { LastBackupDate = DateTime.Now },
                /// <summary>
                /// Backup-Pfad für Rainmeter Backup.
                /// </summary>
                new BackupPath(@"C:\Users\Marce\Documents\Rainmeter" , @"Z:\Users\Marce\Documents\Rainmeter") { LastBackupDate = DateTime.Now },
                new BackupPath(@"C:\Users\Marce\AppData\Roaming\Rainmeter" , @"Z:\Users\Marce\AppData\Roaming\Rainmeter") { LastBackupDate = DateTime.Now },
                /// <summary>
                /// Backup-Pfad für Picture.
                /// </summary>
                new BackupPath(@"C:\Users\Marce\Pictures" , @"Z:\Users\Marce\Pictures") { LastBackupDate = DateTime.Now },
                /// <summary>
                /// Backup-Pfad für Desktop.
                /// </summary>
                new BackupPath(@"C:\Users\Marce\Desktop" , @"Z:\Users\Marce\Desktop") { LastBackupDate = DateTime.Now },
                /// <summary>
                /// Backup-Pfad für Downloads.
                /// </summary>
                new BackupPath(@"C:\Users\Marce\Downloads" , @"Z:\Users\Marce\Downloads") { LastBackupDate = DateTime.Now },

                // Browser alt:
                /// <summary>
                /// Backup-Pfad für Browser-Daten von Google Chrome, Vivaldi, Opera und Brave.
                /// </summary>
               // new BackupPath(@"C:\Users\Marce\AppData\Local\Google\Chrome\User Data" , @"Z:\Users\Marce\AppData\Local\Google\Chrome\User Data") { LastBackupDate = DateTime.Now },
               // new BackupPath(@"C:\Users\Marce\AppData\Local\Vivaldi\User Data" , @"Z:\Users\Marce\AppData\Local\Vivaldi\User Data") { LastBackupDate = DateTime.Now },
               // new BackupPath(@"C:\Users\Marce\AppData\Roaming\Opera Software\Opera GX Stable" , @"Z:\Users\Marce\AppData\Roaming\Opera Software\Opera GX Stable") { LastBackupDate = DateTime.Now },
               // new BackupPath(@"C:\Users\Marce\AppData\Local\BraveSoftware\Brave-Browser\User Data" , @"Z:\Users\Marce\AppData\Local\BraveSoftware\Brave-Browser\User Data") { LastBackupDate = DateTime.Now },

                // Games:
                /// <summary>
                /// Backup-Pfad für Rockstar Games AppData und Dokumente.
                /// </summary>
                new BackupPath(@"C:\Users\Marce\AppData\Local\Rockstar Games" , @"Z:\Users\Marce\AppData\Local\Rockstar Games") { LastBackupDate = DateTime.Now },
                new BackupPath(@"C:\Users\Marce\Documents\Rockstar Games" , @"Z:\Users\Marce\Documents\Rockstar Games") { LastBackupDate = DateTime.Now },
                /// <summary>
                /// Backup-Pfad für American Truck Simulator-Dokumente.
                /// </summary>
                new BackupPath(@"C:\Users\Marce\Documents\American Truck Simulator" , @"Z:\Users\Marce\Documents\American Truck Simulator") { LastBackupDate = DateTime.Now },
                /// <summary>
                /// Backup-Pfad für Euro Truck Simulator 2-Dokumente.
                /// </summary>
                new BackupPath(@"C:\Users\Marce\Documents\Euro Truck Simulator 2" , @"Z:\Users\Marce\Documents\Euro Truck Simulator 2") { LastBackupDate = DateTime.Now },
                /// <summary>
                /// Backup-Pfad für WildLifeC.
                /// </summary>
                new BackupPath(@"C:\Users\Marce\AppData\Local\WildLifeC" , @"Z:\Users\Marce\AppData\Local\WildLifeC") { LastBackupDate = DateTime.Now },
                /// <summary>
                /// Backup-Pfad für .minecraft.
                /// </summary>
                new BackupPath(@"C:\Users\Marce\AppData\Roaming\.minecraft" , @"Z:\Users\Marce\AppData\Roaming\.minecraft") { LastBackupDate = DateTime.Now },
                /// <summary>
                /// Backup-Pfad für My Games.
                /// </summary>
                new BackupPath(@"C:\Users\Marce\Documents\My Games" , @"Z:\Users\Marce\Documents\My Games") { LastBackupDate = DateTime.Now },

                // Software:
                /// <summary>
                /// Backup-Pfad für Arduino IDE.
                /// </summary>
                new BackupPath(@"C:\Users\Marce\Documents\Arduino" , @"Z:\Users\Marce\Documents\Arduino") { LastBackupDate = DateTime.Now },
                /// <summary>
                /// Backup-Pfad für Visual Studio.
                /// </summary>
                new BackupPath(@"C:\Users\Marce\source\repos" , @"Z:\Users\Marce\source\repos") { LastBackupDate = DateTime.Now }
            };
        }


    }
}
