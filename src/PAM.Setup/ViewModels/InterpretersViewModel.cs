using Microsoft.Win32;
using PAM.Core.Models;
using PAM.Setup.Helpers;
using PAM.Setup.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PAM.Setup.ViewModels
{
    internal class InterpretersViewModel : IChildViewModel, INotifyPropertyChanged
    {
        string _interpreterPath;
        RelayCommand _browseFilesCommand;

        public InterpretersViewModel()
        {
            InterpreterPath = UserSettings.Settings.InterpreterPath;
            ScriptExtension = UserSettings.Settings.ScriptExtension;
        }

        public string Name
        {
            get => "Interpreters";
        }

        public Strings Strings
        {
            get => Strings.GetInstance();
        }

        public string ScriptExtension { get; set; }

        public string InterpreterPath
        {
            get => _interpreterPath;
            set => Set(ref _interpreterPath, value);
        }

        public ICommand BrowseFilesCommand
        {
            get
            {
                if (_browseFilesCommand == null)
                {
                    _browseFilesCommand = new RelayCommand(x => BrowseFiles());
                }
                return _browseFilesCommand;
            }
        }

        public void SaveSettings()
        {
            UserSettings.Settings.InterpreterPath = _interpreterPath;
            UserSettings.Settings.ScriptExtension = ScriptExtension;
        }

        void BrowseFiles()
        {
            var od = new OpenFileDialog()
            {
                Filter = ".exe|*.exe"
            };
            od.FileOk += (s, e) => InterpreterPath = od.FileName;
            od.ShowDialog();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void Set<T>(ref T field, T value, [CallerMemberName] string sender = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) { return; }
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(sender));
        }

        #endregion INotifyPropertyChanged Members
    }
}
