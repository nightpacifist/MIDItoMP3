using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace MIDItoMP3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;
            int phrase = Random.Shared.Next(Loc.Phrases.Length);
            ViewModel.SetStatus(() => Loc.Phrases[phrase]);
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                DropBorder.Background = new SolidColorBrush(Color.FromRgb(220, 240, 255));
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            DropBorder.Background = Brushes.White;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            DropBorder.Background = Brushes.White;
            if (ViewModel.IsBusy) return;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    ViewModel.SetFile(files[0]);
                }
            }
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = Loc.T("FileFilter")
            };

            if (dialog.ShowDialog() == true)
                ViewModel.SetFile(dialog.FileName);
        }

        private async void ConvertMidi_Click(object sender, RoutedEventArgs e)
        {
            string? midiPath = ViewModel.FilePath;
            if (!CheckInputFile(midiPath, "MIDI", ".mid", ".midi")) return;

            string? fluidsynth = EnsureTool(ExternalTools.FluidSynth);
            if (fluidsynth == null) return;
            string? lame = EnsureTool(ExternalTools.Lame);
            if (lame == null) return;

            string sf2Path = Path.Combine(AppContext.BaseDirectory, "FluidR3_GM", "FluidR3_GM.sf2");
            string mp3Path = MakeOutputPath(midiPath!, ".mp3");
            await RunConversion(() => ConvertFile.ConvertMidiToMp3(fluidsynth, lame, midiPath!, sf2Path, mp3Path));
        }

        private async void ConvertMov_Click(object sender, RoutedEventArgs e) => await ConvertMov(Mp4Mode.Compatible);

        private async void RemuxMov_Click(object sender, RoutedEventArgs e) => await ConvertMov(Mp4Mode.Original);

        private async Task ConvertMov(Mp4Mode mode)
        {
            string? movPath = ViewModel.FilePath;
            if (!CheckInputFile(movPath, "MOV", ".mov")) return;

            string? ffmpeg = EnsureTool(ExternalTools.Ffmpeg);
            if (ffmpeg == null) return;
            string? ffprobe = EnsureTool(ExternalTools.Ffprobe);
            if (ffprobe == null) return;

            string mp4Path = MakeOutputPath(movPath!, ".mp4");
            await RunConversion(() => ConvertFile.ConvertMovToMp4(ffmpeg, ffprobe, movPath!, mp4Path, mode));
        }

        private void ToolSettings_Click(object sender, RoutedEventArgs e)
        {
            new ToolsWindow { Owner = this }.ShowDialog();
        }

        private void SwitchLanguage_Click(object sender, RoutedEventArgs e) => Loc.Instance.Toggle();

        /// <summary>Запускає конвертацію у фоновому потоці, щоб вікно не зависало.</summary>
        private async Task RunConversion(Action convert)
        {
            ViewModel.IsBusy = true;
            ViewModel.SetStatus("StatusConverting");
            try
            {
                await Task.Run(convert);
                ViewModel.SetStatus("StatusDone");
            }
            catch (Exception ex)
            {
                ViewModel.SetStatus("StatusFailed");
                MessageBox.Show(this, ex.Message, Loc.T("ConversionErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ViewModel.IsBusy = false;
            }
        }

        /// <summary>
        /// Шлях до вихідного файлу поруч із вхідним: ім'я + мітка часу, щоб не перезаписувати попередні результати.
        /// Напр. song.mid → song_2026-10-15_14-30-05.mp3; якщо такий уже є — додається _2, _3…
        /// </summary>
        private static string MakeOutputPath(string inputPath, string extension)
        {
            string dir = Path.GetDirectoryName(inputPath) ?? "";
            string name = Path.GetFileNameWithoutExtension(inputPath) + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string path = Path.Combine(dir, name + extension);
            for (int i = 2; File.Exists(path); i++)
                path = Path.Combine(dir, $"{name}_{i}{extension}");
            return path;
        }

        private bool CheckInputFile(string? path, string kind, params string[] extensions)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                MessageBox.Show(this, Loc.T("NoFileText"), Loc.T("NoFileTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (!extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, Loc.T("WrongFileText", kind), Loc.T("WrongFileTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Повертає шлях до програми. Якщо її не знайдено, пропонує вказати шлях вручну і зберігає його.
        /// </summary>
        private string? EnsureTool(ExternalTool tool)
        {
            var (path, _) = ExternalTools.Resolve(tool);
            if (path != null) return path;

            var answer = MessageBox.Show(this,
                Loc.T("ToolMissingText", tool.DisplayName, tool.ExeName),
                Loc.T("ToolMissingTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes) return null;

            path = ToolsWindow.BrowseForExe(this, tool);
            if (path == null) return null;

            ExternalTools.SetManualPath(tool, path);
            ExternalTools.Settings.Save();
            return path;
        }
    }
}