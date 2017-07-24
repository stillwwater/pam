using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace PAM.Core.Models

{
    public class UserSettings : INotifyPropertyChanged
    {
        const string DEFAULT_FILE = "settings.json";
        static UserSettings _instance = null;

        Dictionary<string, string[]> _activities;
        string _interpreterPath  = "rscript.exe";
        string _scriptExtension  = ".R";
        string _plotsDirectory   = "Plots";
        string _scriptsDirectory = "Scripts";
        string _dataDirectory    = "Data";
        string _tmpDataFile      = "pam_data.tmp.xml";
        string _pamDataCSV       = "pam_data.csv";
        string _theme            = "BaseLight";
        string _themeAccent      = "Teal";
        string _editorFontFamily = "Consolas";
        double _editorFontSize   = 12.5d;
        string _editorTextWrap   = "NoWrap";
        bool _startInBackground  = false;
        bool _logOpened          = false;
        int _logPanelHeight      = 200;
        int _convertMS           = 60_000;
        int _watchEventDelay     = 30;
        string _watchScope       = @"\\.\root\CIMV2";
        DateTime? _lastRecordDate = null;

        private UserSettings()
        {
        }

        public static string SettingsFile { get; set; }

        public static UserSettings Settings
        {
            get
            {
                if (_instance == null)
                {
                    Load(DEFAULT_FILE);
                }
                return _instance;
            }
        }

        #region Serializable Property Accessors

        public Dictionary<string, string[]> Activities
        {
            get => _activities;
            set => Set(ref _activities, value);
        }

        public string InterpreterPath
        {
            get => _interpreterPath;
            set => Set(ref _interpreterPath, value);
        }

        public string ScriptExtension
        {
            get => _scriptExtension;
            set => Set(ref _scriptExtension, value);
        }

        public string PlotsDirectory
        {
            get => _plotsDirectory;
            set => Set(ref _plotsDirectory, value);
        }

        public string ScriptsDirectory
        {
            get => _scriptsDirectory;
            set => Set(ref _scriptsDirectory, value);
        }

        public string DataDirectory
        {
            get => _dataDirectory;
            set => Set(ref _dataDirectory, value);
        }

        public string TmpDataFile
        {
            get => _tmpDataFile;
            set => Set(ref _tmpDataFile, value);
        }

        public string PamDataCSV
        {
            get => _pamDataCSV;
            set => Set(ref _pamDataCSV, value);
        }

        public string Theme
        {
            get => _theme;
            set => Set(ref _theme, value);
        }

        public string ThemeAccent
        {
            get => _themeAccent;
            set => Set(ref _themeAccent, value);
        }

        public string EditorFontFamily
        {
            get => _editorFontFamily;
            set => Set(ref _editorFontFamily, value);
        }

        public double EditorFontSize
        {
            get => _editorFontSize;
            set => Set(ref _editorFontSize, value);
        }

        public string EditorTextWrap
        {
            get => _editorTextWrap;
            set => Set(ref _editorTextWrap, value);
        }

        public bool StartInBackground
        {
            get => _startInBackground;
            set => Set(ref _startInBackground, value);
        }

        public bool LogOpened
        {
            get => _logOpened;
            set => Set(ref _logOpened, value);
        }

        public int LogPanelHeight
        {
            get => _logPanelHeight;
            set => Set(ref _logPanelHeight, value);
        }

        public int ConvertMS
        {
            get => _convertMS;
            set => Set(ref _convertMS, value);
        }

        public int WatchEventDelay
        {
            get => _watchEventDelay;
            set => Set(ref _watchEventDelay, value);
        }

        public string WatchScope
        {
            get => _watchScope;
            set => Set(ref _watchScope, value);
        }

        public DateTime? LastRecordDate
        {
            get => _lastRecordDate;
            set => Set(ref _lastRecordDate, value);
        }

        #endregion Serializable Property Accessors

        /// <summary>
        /// Reloads settings from file (Changes that aren't saved will be lost).
        /// </summary>
        /// <param name="instance">UserSettings object to be reloaded</param>
        public static void Reload()
        {
            _instance = null;
            Load(SettingsFile);
        }

        /// <summary>
        /// Saves defined properties to a JSON file.
        /// </summary>
        /// <returns>Success</returns>
        public void Save()
        {
            File.WriteAllText(SettingsFile, ToString());
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Loads settings from a JSON file.
        /// </summary>
        /// <param name="settingsFile"></param>
        /// <param name="forceDefaults">Override user configuration and use default settings.</param>
        /// <returns></returns>
        static UserSettings Load(string settingsFile, bool forceDefaults = false)
        {
            SettingsFile = settingsFile;

            if (!File.Exists(settingsFile) || forceDefaults)
            {
                // Create JSON file using defaults
                _instance = new UserSettings();
                _instance.Save();
                return _instance;
            }

            try
            {
                _instance = JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText(settingsFile));
            }
            catch (JsonReaderException e)
            {
                Logger.Log.Write("Failed to load user settings, using defaults.\r\n\r\n"
                                 + $"{e.Message} in {SettingsFile}");

                return Load(settingsFile, forceDefaults: true);
            }

            return _instance;
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
