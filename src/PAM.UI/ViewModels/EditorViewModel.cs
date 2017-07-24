using PAM.Core.Models;
using PAM.UI.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PAM.UI.ViewModels
{
    internal class EditorViewModel : INotifyPropertyChanged
    {
        string _selectedItem;
        string _editorText;

        RelayCommand _saveFileCommand;

        /// <summary>
        /// Creates a new instance of the EditorViewModel Class.
        /// </summary>
        public EditorViewModel()
        {
            var tmp = Directory.GetFiles(".", "*.json").ToList();
            tmp.AddRange(Directory.GetFiles(UserSettings.DataDirectory));

            tmp.AddRange(Directory.GetFiles(UserSettings.ScriptsDirectory));

            tmp.Select(x => Path.GetFileName(x));

            Files = tmp.ToArray();
            SelectedFile = Files[0];
        }

        public UserSettings UserSettings
        {
            get => UserSettings.Settings;
        }

        public string[] Files
        {
            get;
            private set;
        }

        public string SelectedFile
        {
            get => _selectedItem;

            set
            {
                _selectedItem = value;
                Load();
            }
        }

        public string EditorText
        {
            get => _editorText;
            set => Set(ref _editorText, value);
        }

        public ICommand SaveFileCommand
        {
            get
            {
                if (_saveFileCommand == null)
                {
                    _saveFileCommand = new RelayCommand(param => Save(), param => CanSave());
                }
                return _saveFileCommand;
            }
        }

        /// <summary>
        /// Load contents from SelectedFile into the EditorText.
        /// </summary>
        void Load()
        {
            if (SelectedFile != null && File.Exists(SelectedFile))
            {
                EditorText = File.ReadAllText(SelectedFile);
            }
        }

        /// <summary>
        /// Save EditorText to the SelectedFile
        /// </summary>
        void Save()
        {
            File.WriteAllText(SelectedFile, EditorText);
            Logger.Log.Write($"Editor saved \"{SelectedFile}\"");
        }

        bool CanSave() => SelectedFile != null && File.Exists(SelectedFile);

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
