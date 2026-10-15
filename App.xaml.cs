using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace MIDItoMP3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Логи пишемо поруч із settings.json: папка з exe може бути недоступна для запису (Program Files)
        private static readonly string LogDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MIDItoMP3");

        public App()
        {
            this.DispatcherUnhandledException += (s, e) =>
            {
                MessageBox.Show(e.Exception.ToString(), Loc.T("UnhandledErrorTitle"));
                WriteLog("unhandled_error.log", e.Exception);
                e.Handled = true;
            };
        }

        private static void WriteLog(string fileName, Exception ex)
        {
            try
            {
                Directory.CreateDirectory(LogDir);
                File.WriteAllText(Path.Combine(LogDir, fileName), ex.ToString());
            }
            catch
            {
                // не вдалося записати лог — повідомлення користувач уже побачив
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), Loc.T("StartupErrorTitle"));
                WriteLog("startup_error.log", ex);
            }
        }
    }

}
