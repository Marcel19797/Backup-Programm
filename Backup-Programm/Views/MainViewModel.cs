using Backup_Programm.Interfaces;
using Backup_Programm.Models;
using Backup_Programm.Services;
using Backup_Programm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace Backup_Programm.Views
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IBackupService _backupService;
        private readonly IArchiveService _archiveService;
        private bool _isBackupRunning;

        // ObservableCollection aktualisiert die UI automatisch, wenn Elemente hinzugefügt oder entfernt werden
        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();

        public ICommand StartBackupCommand { get; }

        public ICommand StartArchiveCommand { get; }
        public ICommand StartIncrementalCommand { get; }

        public bool IsBackupRunning
        {
            get => _isBackupRunning;
            set
            {
                if (_isBackupRunning != value)
                {
                    _isBackupRunning = value;
                    OnPropertyChanged();
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        // --- Diese Methoden in dein MainViewModel einfügen ---

        public ICommand OpenBookmarksCommand { get; }

        // WICHTIG: Füge das in deinen Konstruktor ( public MainViewModel() ) ein:
        // OpenBookmarksCommand = new RelayCommand(ExecuteOpenBookmarks, CanExecuteBackup);

        private async void ExecuteOpenBookmarks(object? obj)
        {
            // Verhindert, dass man Backup und Lesezeichen gleichzeitig drückt
            IsBackupRunning = true;
            LogMessages.Add("--- Lesezeichen-Aufräumen gestartet ---");
            LogMessages.Add("Hinweis: Schließe den Browser komplett, um den nächsten zu starten!");

            // Nacheinander ausführen und warten
            await OpenAndWaitForBrowserAsync("chrome", "chrome.exe", "chrome://bookmarks/");
            await OpenAndWaitForBrowserAsync("brave", "brave.exe", "brave://bookmarks/");
            await OpenAndWaitForBrowserAsync("opera", "opera.exe", "opera://bookmarks/");
            await OpenAndWaitForBrowserAsync("vivaldi", "vivaldi.exe", "vivaldi://bookmarks/");

            LogMessages.Add("--- Lesezeichen-Aufräumen abgeschlossen ---");
            IsBackupRunning = false;
        }

        private async Task OpenAndWaitForBrowserAsync(string processName, string executableName, string url)
        {
            // 1. Prüfen, ob der Browser bereits läuft
            bool isAlreadyRunning = Process.GetProcessesByName(processName).Length > 0;

            if (isAlreadyRunning)
            {
                LogMessages.Add($"Info: {processName} ist bereits geöffnet.");
                LogMessages.Add($"-> Bitte wechsle zum Browser, bereinige die Daten und schließe ihn komplett.");
            }
            else
            {
                // NEU: URL automatisch in die Zwischenablage kopieren
                try
                {
                    Clipboard.SetText(url);
                }
                catch
                {
                    /* Wird ignoriert, falls die Zwischenablage gerade durch ein anderes Programm blockiert ist */
                }

                // 2. Fenster aufploppen lassen (Text angepasst)
                string message = $"{url}\n\nBitte Lesezeichen speichern und Daten vom Browser löschen.\n(Die URL wurde automatisch in deine Zwischenablage kopiert - einfach Strg+V nutzen!)";
                MessageBox.Show(message, $"{processName} - Aktion erforderlich", MessageBoxButton.OK, MessageBoxImage.Information);

                // 3. Nach dem Klick auf "OK" startet der Browser
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = executableName,
                        Arguments = url,
                        UseShellExecute = true
                    };

                    Process.Start(startInfo);
                    LogMessages.Add($"Gestartet: {executableName}");
                }
                catch (Exception)
                {
                    LogMessages.Add($"Übersprungen: {executableName} nicht gefunden.");
                    return;
                }
            }

            // Kurz warten, damit der Prozess im Taskmanager sicher registriert ist
            await Task.Delay(2000);

            // 4. Warteschleife: Warten, bis der Browser komplett beendet wurde
            if (Process.GetProcessesByName(processName).Length > 0)
            {
                LogMessages.Add($"Warte darauf, dass {processName} komplett geschlossen wird...");

                while (Process.GetProcessesByName(processName).Length > 0)
                {
                    await Task.Delay(1000);
                }

                LogMessages.Add($"{processName} wurde erfolgreich geschlossen.");
                LogMessages.Add("--------------------------------------------------");
            }
        }

        public MainViewModel()
        {
            // Dependency Injection kann hier später eingeführt werden
            _backupService = new BackupService();
            _archiveService = new SevenZipArchiveService();
            OpenBookmarksCommand = new RelayCommand(ExecuteOpenBookmarks);

            StartBackupCommand = new RelayCommand(ExecuteBackup, CanExecuteBackup);
            // Nutzt dieselbe CanExecute-Logik, damit man nicht normales Backup und Archiv gleichzeitig startet
            StartArchiveCommand = new RelayCommand(ExecuteArchiveBackup, CanExecuteBackup);
            StartIncrementalCommand = new RelayCommand(ExecuteIncrementalArchive, CanExecuteBackup);
            OpenBookmarksCommand = new RelayCommand(ExecuteOpenBookmarks, CanExecuteBackup);
        }
        private bool CanExecuteBackup(object? obj)
        {
            return !IsBackupRunning;
        }

        public async void ExecuteBackup(object? obj)
        {
            if (!IsRunningAsAdministrator())
            {
                if (RelaunchAsAdministrator())
                {
                    return;
                }

                LogMessages.Add("Backup konnte nicht mit Administratorrechten gestartet werden.");
                return;
            }

            await RunBackupAsync();
        }

        private async Task RunBackupAsync()
        {
            try
            {
                IsBackupRunning = true;
                LogMessages.Clear();
                LogMessages.Add("Starte den Sicherungsprozess... von C: zu Z:");

                // Progress-Handler, um Updates aus dem Hintergrundthread in die UI zu delegieren
                var progress = new Progress<string>(message =>
                {
                    // Wenn die Nachricht mit "Fortschritt:" beginnt und die letzte Nachricht in der Liste ebenfalls mit "Fortschritt:" beginnt, aktualisiere die letzte Nachricht.
                    // Andernfalls füge die neue Nachricht hinzu.
                    if (message.StartsWith("Fortschritt:") && LogMessages.Count > 0 && LogMessages[LogMessages.Count - 1].StartsWith("Fortschritt:"))
                    {
                        LogMessages[LogMessages.Count - 1] = message;
                    }
                    else
                    {
                        LogMessages.Add(message);
                    }
                });
                var paths = BackupConfiguration.GetBackupPaths();

                await _backupService.ExecuteBackupAsync(paths, progress);

                LogMessages.Add("Sicherungsprozess abgeschlossen.");
            }
            finally
            {
                IsBackupRunning = false;
            }
        }
        /// <summary>
        /// Überprüft, ob die Anwendung mit Administratorrechten ausgeführt wird.
        /// </summary>
        /// <returns>True, wenn die Anwendung mit Administratorrechten ausgeführt wird; andernfalls False.</returns>
        private static bool IsRunningAsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        /// <summary>
        /// Versucht, die Anwendung mit Administratorrechten neu zu starten.
        /// </summary>
        /// <returns>True, wenn der Neustart erfolgreich initiiert wurde; False, wenn der Benutzer die UAC-Aufforderung abgelehnt hat oder ein Fehler aufgetreten ist.</returns>
        /// <remarks>Die Methode versucht, die Anwendung mit Administratorrechten neu zu starten, indem sie die UAC-Aufforderung verwendet.</remarks>
        private static bool RelaunchAsAdministrator()
        {
            var executablePath = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                return false;
            }

            try
            {
                var startInfo = new ProcessStartInfo(executablePath, "--run-backup")
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(startInfo);
                Application.Current.Shutdown();
                return true;
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                return false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Führt den Archivierungs-Sicherungsprozess aus, indem er alle Backup-Pfade durchläuft und für jeden ein Archiv erstellt.
        /// </summary>
        /// <param name="obj">Optionales Parameterobjekt (nicht verwendet)</param>
        /// <remarks> Die Methode verwendet den IArchiveService, um die Verzeichnisse zu archivieren und aktualisiert die LogMessages für die UI.</remarks>
        /// <returns>Task, der den Abschluss der Archivierung darstellt</returns>
        private async void ExecuteArchiveBackup(object? obj)
        {
            IsBackupRunning = true;
            LogMessages.Clear();
            LogMessages.Add("Starte den Archivierungs-Sicherungsprozess... als .7z");

            var progress = new Progress<string>(message =>
            {
                // Gleiche UI-Logik für flüssiges Scrollen und Prozentanzeige (falls 7-Zip das später ausgeben soll)
                if (message.StartsWith("Fortschritt:") && LogMessages.Count > 0 && LogMessages[LogMessages.Count - 1].StartsWith("Fortschritt:"))
                {
                    LogMessages[LogMessages.Count - 1] = message;
                }
                else
                {
                    LogMessages.Add(message);
                }
            });

            var paths = BackupConfiguration.GetBackupPaths();

            /// <summary>
            /// Durchläuft alle Backup-Pfade und erstellt für jeden ein Archiv.
            /// </summary>
            /// <param name="paths">Liste der Backup-Pfade</param>
            /// <param name="progress">Fortschrittsanzeige für die Archivierung</param>
            /// <returns>Task, der den Abschluss der Archivierung darstellt</returns>
            /// <remarks>Die Methode erstellt für jeden Backup-Pfad ein separates Archiv im Zielverzeichnis.</remarks>
            foreach (var path in paths)
            {
                // 1. Die Quelle für 7-Zip ist nun das BACKUP (Z:\Users\Marce\...), nicht mehr C:\
                string sourceForArchive = path.DestinationPath;

                // 2. Das Ziel-Verzeichnis austauschen -> aus "Z:\Users" wird "Z:\Archiv"
                string archiveDest = path.DestinationPath.Replace(@"Z:\Users", @"Z:\Archiv");

                // 3. Dateiendung anhängen -> "Z:\Archiv\Marce\Documents\Autocad.7z"
                string archiveFile = archiveDest + ".7z";

                // WICHTIG: Hier übergeben wir jetzt sourceForArchive statt path.SourcePath!
                await _archiveService.ArchiveDirectoryAsync(sourceForArchive, archiveFile, progress);
            }

            LogMessages.Add("Archivierungsprozess abgeschlossen.");
            IsBackupRunning = false;
        }

        private async void ExecuteIncrementalArchive(object? obj)
        {
            IsBackupRunning = true;
            LogMessages.Clear();
            LogMessages.Add("Starte die inkrementelle Archivierung...");

            var progress = new Progress<string>(message =>
            {
                if (message.StartsWith("Fortschritt:") && LogMessages.Count > 0 && LogMessages[LogMessages.Count - 1].StartsWith("Fortschritt:"))
                {
                    LogMessages[LogMessages.Count - 1] = message;
                }
                else
                {
                    LogMessages.Add(message);
                }
            });

            var paths = BackupConfiguration.GetBackupPaths();

            // Zeitstempel für den Dateinamen (Jahr-Monat-Tag_Stunde-Minute)
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");

            foreach (var path in paths)
            {
                // 1. Die Quelle sind die Dateien aus dem normalen Backup
                string sourceForArchive = path.DestinationPath;

                // 2. Pfad zum Basis-Archiv (wird nur zum Vergleichen gelesen)
                string baseArchiveDest = path.DestinationPath.Replace(@"Z:\Users", @"Z:\Archiv");
                string baseArchiveFile = baseArchiveDest + ".7z";

                // 3. Ziel-Ordner für das Inkrement (aus Z:\Users wird Z:\Archiv-Inkr)
                string inkrDest = path.DestinationPath.Replace(@"Z:\Users", @"Z:\Archiv-Inkr");

                // 4. Inkrementelle Datei mit Zeitstempel am Ende! (z.B. Z:\Archiv-Inkr\Marce\Documents\Autocad_2026-08-04_12-10.7z)
                string inkrFile = inkrDest + "_" + timestamp + ".7z";

                await _archiveService.ArchiveIncrementalAsync(sourceForArchive, baseArchiveFile, inkrFile, progress);
            }

            LogMessages.Add("Inkrementeller Archivierungsprozess abgeschlossen.");
            IsBackupRunning = false;
        }
    }
}
