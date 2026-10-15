using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace MIDItoMP3
{
    public partial class ToolsWindow : Window
    {
        private readonly List<ToolRow> _rows;

        public ToolsWindow()
        {
            InitializeComponent();
            _rows = ExternalTools.All.Select(t => new ToolRow(t, ExternalTools.GetManualPath(t))).ToList();
            ToolList.ItemsSource = _rows;
        }

        public static string? BrowseForExe(Window owner, ExternalTool tool)
        {
            var dialog = new OpenFileDialog
            {
                Title = Loc.T("BrowseTitle", tool.ExeName),
                Filter = $"{tool.DisplayName} ({tool.ExeName})|{tool.ExeName}|" + Loc.T("ExeFilter") + "|*.exe"
            };
            return dialog.ShowDialog(owner) == true ? dialog.FileName : null;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var row = (ToolRow)((FrameworkElement)sender).DataContext;
            var path = BrowseForExe(this, row.Tool);
            if (path != null)
                row.ManualPath = path;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var missing = _rows.Where(r => !string.IsNullOrWhiteSpace(r.ManualPath) && !File.Exists(r.ManualPath.Trim())).ToList();
            if (missing.Count > 0)
            {
                MessageBox.Show(this,
                    Loc.T("PathMissingText", string.Join("\n", missing.Select(r => $"{r.Tool.DisplayName}: {r.ManualPath}"))),
                    Loc.T("PathMissingTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var row in _rows)
                ExternalTools.SetManualPath(row.Tool, row.ManualPath);
            ExternalTools.Settings.Save();

            DialogResult = true;
        }

        private class ToolRow : INotifyPropertyChanged
        {
            private string _manualPath;

            public ToolRow(ExternalTool tool, string? manualPath)
            {
                Tool = tool;
                _manualPath = manualPath ?? "";
            }

            public ExternalTool Tool { get; }

            public string ManualPath
            {
                get => _manualPath;
                set
                {
                    _manualPath = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ManualPath)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
                }
            }

            public string Status
            {
                get
                {
                    if (!string.IsNullOrWhiteSpace(ManualPath))
                        return File.Exists(ManualPath.Trim()) ? Loc.T("ToolManual") : Loc.T("ToolManualMissing");

                    var (path, source) = ExternalTools.ResolveAutomatic(Tool);
                    return source switch
                    {
                        ToolSource.SystemPath => Loc.T("ToolInPath", path!),
                        ToolSource.Bundled => Loc.T("ToolBundled", path!),
                        _ => Loc.T("ToolNotFound")
                    };
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
