using Backup_Programm.Interfaces;
using System.Diagnostics;
using System.IO;

namespace Backup_Programm.Services
{
    public class SevenZipArchiveService : IArchiveService
    {
        public Task ArchiveDirectoryAsync(string sourcePath, string destinationArchiveFile, IProgress<string> progress)
        {
            return Task.Run(() =>
            {
                if (!Directory.Exists(sourcePath))
                {
                    progress?.Report($"Übersprungen (Ordner nicht gefunden): {sourcePath}");
                    return;
                }

                // Ermittelt den Pfad zur 7zr.exe im gleichen Verzeichnis wie deine WPF-Anwendung
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "7zr.exe");

                if (!File.Exists(exePath))
                {
                    progress?.Report($"Fehler: {exePath} wurde nicht gefunden. Bitte Eigenschaften in VS 2026 prüfen.");
                    return;
                }

                // Sicherstellen, dass der übergeordnete Zielordner für das Archiv existiert
                string? destDir = Path.GetDirectoryName(destinationArchiveFile);
                if (!string.IsNullOrEmpty(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                progress?.Report($"Packe Archiv: {destinationArchiveFile}");

                try
                {
                    // Parameter: 'a' = add, '-t7z' = Format, '-mx=5' = Normale Kompression (9 wäre Ultra, dauert aber länger)
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        // Der Stern (*) packt den Inhalt des Ordners, nicht den Ordner selbst als oberste Ebene
                        Arguments = $"a -t7z \"{destinationArchiveFile}\" \"{sourcePath}\\*\" -mx=5",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        if (process == null)
                        {
                            progress?.Report($"Fehler: Prozess konnte nicht gestartet werden.\n");
                            return;
                        }

                        process.WaitForExit();

                        if (process.ExitCode == 0)
                        {
                            progress?.Report($"Fertig (Gepackt): {sourcePath}\n");
                        }
                        else
                        {
                            string error = process.StandardError.ReadToEnd();
                            progress?.Report($"Fehler beim Packen ({process.ExitCode}): {error}\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    progress?.Report($"Ausnahmefehler bei 7-Zip: {ex.Message}\n");
                }
            });
        }

        public Task ArchiveIncrementalAsync(string sourcePath, string baseArchiveFile, string incrementalDestArchive, IProgress<string> progress)
        {
            return Task.Run(() =>
            {
                if (!Directory.Exists(sourcePath)) { return; }

                // Prüfen, ob das Basis-Archiv existiert (zwingend nötig für den Vergleich)
                if (!File.Exists(baseArchiveFile))
                {
                    progress?.Report($"Übersprungen: Kein Basis-Archiv gefunden ({baseArchiveFile}). Bitte erst ein Voll-Archiv erstellen.");
                    return;
                }

                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "7zr.exe");
                if (!File.Exists(exePath))
                {
                    progress?.Report($"Fehler: {exePath} nicht gefunden.");
                    return;
                }

                string? destDir = Path.GetDirectoryName(incrementalDestArchive);
                if (!string.IsNullOrEmpty(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                progress?.Report($"Prüfe auf neue Dateien und packe Inkrement: {incrementalDestArchive}");

                try
                {
                    // u = Update Modus
                    // -u- = Originaldatei nicht verändern
                    // -up0q3r2x2y2z0w2! = Nur Änderungen in die neue Datei schreiben
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = $"u \"{baseArchiveFile}\" -u- -up0q3r2x2y2z0w2!\"{incrementalDestArchive}\" \"{sourcePath}\\*\" -mx=5",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        if (process == null)
                        {
                            progress?.Report($"Fehler: Prozess konnte nicht gestartet werden.\n");
                            return;
                        }

                        process.WaitForExit();

                        if (process.ExitCode == 0)
                        {
                            // 7-Zip ist so schlau, gar nicht erst eine Datei anzulegen, wenn es keine Änderungen gab!
                            if (File.Exists(incrementalDestArchive))
                            {
                                progress?.Report($"Fertig (Inkrement gesichert): {incrementalDestArchive}\n");
                            }
                            else
                            {
                                progress?.Report($"Keine neuen oder geänderten Dateien für: {sourcePath} gefunden.\n");
                            }
                        }
                        else
                        {
                            string error = process.StandardError.ReadToEnd();
                            progress?.Report($"Fehler beim Inkrement ({process.ExitCode}): {error}\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    progress?.Report($"Ausnahmefehler bei 7-Zip: {ex.Message}\n");
                }
            });
        }
    }
}