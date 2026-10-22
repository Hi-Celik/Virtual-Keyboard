using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace CyberKeyboard
{
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        }

        private static string LogPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CyberKeyboard", "app_debug.log");

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try { File.AppendAllText(LogPath, $"[{DateTime.Now}] Dispatcher Exception: {e.Exception}\n"); } catch { }
            MessageBox.Show(e.Exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = false;
        }

        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try { File.AppendAllText(LogPath, $"[{DateTime.Now}] Domain Exception: {e.ExceptionObject}\n"); } catch { }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try { File.AppendAllText(LogPath, $"[{DateTime.Now}] Started\n"); } catch { }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            try { File.AppendAllText(LogPath, $"[{DateTime.Now}] App OnExit with code {e.ApplicationExitCode}\n"); } catch { }
        }
    }
}
