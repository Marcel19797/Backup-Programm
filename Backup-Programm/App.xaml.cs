using System;
using System.Linq;
using System.Windows;
using Backup_Programm.Views;

namespace Backup_Programm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Überschreibt die OnStartup-Methode, um das Hauptfenster zu erstellen und anzuzeigen. 
        /// Wenn das Programm mit dem Argument "--run-backup" gestartet wird, wird der Backup-Prozess automatisch gestartet, sobald das Hauptfenster geladen ist.
        /// </summary>
        /// <param name="e">Die Startargumente der Anwendung.</param>
        /// <remarks>
        /// Diese Methode wird aufgerufen, wenn die Anwendung gestartet wird.
        /// </remarks>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = new MainWindow();
            MainWindow = window;
            window.Show();

            if (e.Args.Any(arg => string.Equals(arg, "--run-backup", StringComparison.OrdinalIgnoreCase)))
            {
                window.Loaded += (_, __) =>
                {
                    if (window.DataContext is Views.MainViewModel viewModel)
                    {
                        viewModel.StartBackupCommand.Execute(null);
                    }
                };
            }
        }
    }
}
