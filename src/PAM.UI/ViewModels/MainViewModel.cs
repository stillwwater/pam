using MahApps.Metro;
using PAM.Core.Models;
using PAM.UI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace PAM.UI.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        const string PAM_BACKGROUND_EXECUTABLE = "pam_bgprocess.exe";
        const string PAM_SETUP_EXECUTABLE = "pam_setup.exe";
        bool _canExit = false;
        RelayCommand _toggleLogCommand;
        RelayCommand _resetCommand;
        RelayCommand _hideWindowCommand;
        RelayCommand _safeExitCommand;
        DataViewModel _dataVM;
        EditorViewModel _editorVM;
        PlotsViewModel _plotsVM;

        /// <summary>
        /// Creates a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (UserSettings.Settings.Activities == null && File.Exists(PAM_SETUP_EXECUTABLE))
            {
                new Process() { StartInfo = new ProcessStartInfo(PAM_SETUP_EXECUTABLE) }.Start();
                Application.Current.Shutdown();
                return;
            }

            string[] dirs =
            {
                UserSettings.Settings.DataDirectory,
                UserSettings.Settings.PlotsDirectory,
                UserSettings.Settings.ScriptsDirectory
            };

            foreach (string s in dirs) { SetupDirectory(s); }

            LoadTheme();
            LoadChildViewModels();

            if (UserSettings.Settings.StartInBackground)
            {
                string[] args = Environment.GetCommandLineArgs();

                if (args.Length > 1 && args[1] == "ignore(StartInBackground)") { return; }

                SafeExit(stopActivities: false);
                StartBackgroundProcess();
                Application.Current.Shutdown();
            }
        }

        public Logger Log
        {
            get => Logger.Log;
        }

        public DataViewModel DataViewModel
        {
            get => _dataVM;
            set => Set(ref _dataVM, value);
        }

        public EditorViewModel EditorViewModel
        {
            get => _editorVM;
            set => Set(ref _editorVM, value);
        }

        public PlotsViewModel PlotsViewModel
        {
            get => _plotsVM;
            set => Set(ref _plotsVM, value);
        }

        public UserSettings UserSettings
        {
            get => UserSettings.Settings;
        }

        public ICommand ToggleLogCommand
        {
            get
            {
                if (_toggleLogCommand == null)
                {
                    _toggleLogCommand = new RelayCommand(x => ToggleLog());
                }
                return _toggleLogCommand;
            }
        }

        public ICommand ResetCommand
        {
            get
            {
                if (_resetCommand == null)
                {
                    _resetCommand = new RelayCommand(x => Reload());
                }
                return _resetCommand;
            }
        }

        public ICommand HideWindowCommand
        {
            get
            {
                if (_hideWindowCommand == null)
                {
                    _hideWindowCommand = new RelayCommand(x => HideWindow());
                }
                return _hideWindowCommand;
            }
        }

        public ICommand SafeExitCommand
        {
            get
            {
                if (_safeExitCommand == null)
                {
                    _safeExitCommand = new RelayCommand(x => SafeExit());
                }
                return _safeExitCommand;
            }
        }

        #region Window

        public void HideWindow()
        {
            SafeExit(stopActivities: false);

            StartBackgroundProcess();

            Application.Current.Shutdown();
        }

        /// <summary>
        /// Reloads most of the back-end of the application.
        /// </summary>
        public void Reload()
        {
            Logger.Log.Write("Reloading...");

            SafeExit(stopActivities: false);
            _canExit = false;
            UserSettings.Reload();
            LoadTheme();
            PlotsViewModel.FreeResources();
            LoadChildViewModels();
        }

        /// <summary>
        /// Safely disposes resources and saves data.
        /// </summary>
        public void SafeExit(bool stopActivities)
        {
            if (_canExit) { return; }

            DataViewModel.Dispose(stopActivities);

            _canExit = true;
        }

        /// <summary>
        /// Safely disposes resources and saves data.
        /// </summary>
        public void SafeExit()
        {
            SafeExit(true);
        }

        #endregion Window

        public void ToggleLog() => Log.IsOpen = !Log.IsOpen;

        void StartBackgroundProcess()
        {
            if (!File.Exists(PAM_BACKGROUND_EXECUTABLE))
            {
                Dialog.Alert($"Could not find {PAM_BACKGROUND_EXECUTABLE}", "Fatal Error", true);
                return;
            }

            var pInfo = new ProcessStartInfo() { FileName = PAM_BACKGROUND_EXECUTABLE };
            new Process() { StartInfo = pInfo }.Start();
        }

        void SetupDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        void LoadChildViewModels()
        {
            DataViewModel = new DataViewModel();
            EditorViewModel = new EditorViewModel();
            PlotsViewModel = new PlotsViewModel();
        }

        void LoadTheme()
        {
            ThemeManager.ChangeAppStyle(
                Application.Current,
                ThemeManager.GetAccent(UserSettings.Settings.ThemeAccent),
                ThemeManager.GetAppTheme(UserSettings.Settings.Theme));
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
