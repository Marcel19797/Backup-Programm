using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Backup_Programm.Models;
using Backup_Programm.Interfaces;

namespace Backup_Programm.Services
{
    public class BackupService : IBackupService
    {
        /// <summary>
        /// Führt das Backup für die angegebenen Pfade asynchron aus.[cite: 7]
        /// </summary>
        public Task ExecuteBackupAsync(List<BackupPath> paths, IProgress<string> progress)
        {
            return Task.Run(() =>
            {
                foreach (var path in paths)
                {
                    try
                    {
                        if (!Directory.Exists(path.SourcePath))
                        {
                            progress.Report($"Übersprungen (Ordner nicht gefunden): {path.SourcePath}");
                            continue;
                        }

                        // 1. Ordner prüfen auf Anzahl und Größe
                        long totalSize = 0;
                        int totalFiles = 0;
                        CalculateDirectoryInfo(path.SourcePath, ref totalSize, ref totalFiles);

                        double sizeInMb = totalSize / (1024.0 * 1024.0);
                        progress.Report($"Prüfe Ordner: {path.SourcePath}");
                        progress.Report($" -> {totalFiles} Dateien ({sizeInMb:F2} MB)");

                        if (totalFiles == 0)
                        {
                            progress.Report("Fertig (Ordner ist leer)\n");
                            continue;
                        }

                        // 2. Kopiervorgang (nur Prozentanzeige)
                        int processedFiles = 0;

                        // Ein Callback, der nach jeder abgearbeiteten Datei aufgerufen wird
                        Action fileProcessedCallback = () =>
                        {
                            // Thread-sicheres Hochzählen
                            int current = Interlocked.Increment(ref processedFiles);
                            int percent = (int)((current / (double)totalFiles) * 100);
                            progress.Report($"Fortschritt: {percent}%");
                        };

                        SyncDirectory(path.SourcePath, path.DestinationPath, fileProcessedCallback);

                        // 3. Fertig anzeigen
                        progress.Report($"Fertig: {path.SourcePath}\n");
                    }
                    catch (Exception ex)
                    {
                        progress.Report($"Fehler beim Backup von {path.SourcePath}: {ex.Message}");
                    }
                } // 4. Zum nächsten Ordner springen
            });
        }

        private void CalculateDirectoryInfo(string dir, ref long totalSize, ref int totalFiles)
        {
            try
            {
                var files = Directory.GetFiles(dir);
                totalFiles += files.Length;

                foreach (var file in files)
                {
                    totalSize += new FileInfo(file).Length;
                }

                foreach (var subDir in Directory.GetDirectories(dir))
                {
                    CalculateDirectoryInfo(subDir, ref totalSize, ref totalFiles);
                }
            }
            // Ignoriere Ordner, auf die aufgrund von Rechten oder Pfadlängen nicht zugegriffen werden kann
            catch (UnauthorizedAccessException) { }
            catch (PathTooLongException) { }
            catch (DirectoryNotFoundException) { }
        }

        private void SyncDirectory(string sourceDir, string destDir, Action onFileProcessed)
        {
            if (!Directory.Exists(sourceDir)) return;

            Directory.CreateDirectory(destDir);

            // Multithreading: Parallel.ForEach kann verwendet werden, um Dateien parallel zu kopieren[cite: 7]
            Parallel.ForEach(Directory.GetFiles(sourceDir), (file) =>
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);

                // Überprüfen, ob die Datei aktualisiert werden muss[cite: 7]
                if (!File.Exists(destFile) || File.GetLastWriteTime(file) > File.GetLastWriteTime(destFile))
                {
                    try
                    {
                        File.Copy(file, destFile, true);
                    }
                    catch
                    {
                        /* Fehler bei einzelnen Dateien können hier protokolliert werden, blockieren aber nicht die Schleife */
                    }
                }

                // Melde die Datei als verarbeitet, auch wenn sie nur übersprungen wurde, 
                // da sie für die Berechnung der Gesamt-Prozentzahl wichtig ist.
                onFileProcessed();
            });

            // Rekursiv Unterverzeichnisse kopieren[cite: 7]
            Parallel.ForEach(Directory.GetDirectories(sourceDir), (dir) =>
            {
                string dirName = Path.GetFileName(dir);
                string destSubDir = Path.Combine(destDir, dirName);
                SyncDirectory(dir, destSubDir, onFileProcessed);
            });
        }
    }
}