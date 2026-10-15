using System.ComponentModel;
using System.IO;

namespace MIDItoMP3
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string? _filePath;
        private Func<string> _status = () => "";
        private bool _isBusy;

        public MainViewModel()
        {
            // Статус зберігається як функція, щоб після перемикання мови текст перекладався сам
            Loc.Instance.PropertyChanged += (_, _) => OnPropertyChanged(nameof(Status));
        }

        public string? FilePath => _filePath;

        /// <summary>Текст під кнопкою вибору: ім'я файлу, мотиваційна фраза або стан конвертації.</summary>
        public string Status => _status();

        public void SetStatus(Func<string> status)
        {
            _status = status;
            OnPropertyChanged(nameof(Status));
        }

        /// <summary>Статус із перекладом за ключем <see cref="Loc"/>.</summary>
        public void SetStatus(string locKey) => SetStatus(() => Loc.T(locKey));

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                OnPropertyChanged(nameof(IsIdle));
            }
        }

        public bool IsIdle => !_isBusy;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void SetFile(string path)
        {
            _filePath = path;
            string name = Path.GetFileName(path);
            SetStatus(() => name);
        }

        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
