using IWshRuntimeLibrary;
using MahApps.Metro;
using PAM.Core.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace PAM.Setup.ViewModels
{
    internal class OtherSettingsViewModel : IChildViewModel
    {
        const string PAM_EXE = "pam.exe";
        string _selectedThemeAccent;
        bool _enableDarkTheme;

        public OtherSettingsViewModel()
        {
            ThemeAccents = ThemeManager.Accents.Select(x => x.Name).ToArray();
            SelectedThemeAccent = UserSettings.Settings.ThemeAccent;
            EnableDarkTheme = UserSettings.Settings.Theme == "BaseDark";
            EditorFontFamily = UserSettings.Settings.EditorFontFamily;
            LaunchPAM = true;
        }

        public string Name
        {
            get => "Other Settings";
        }

        public string[] ThemeAccents { get; private set; }

        public string SelectedThemeAccent
        {
            get => _selectedThemeAccent;
            set
            {
                _selectedThemeAccent = value;
                UpdateTheme();
            }
        }

        public bool EnableDarkTheme
        {
            get => _enableDarkTheme;
            set
            {
                _enableDarkTheme = value;
                UpdateTheme();
            }
        }

        public string EditorFontFamily { get; set; }

        public bool CreateDesktopShortcut { get; set; }

        public bool LaunchPAM { get; set; }

        public void SaveSettings()
        {
            UserSettings.Settings.EditorFontFamily = EditorFontFamily;
            UserSettings.Settings.Theme = _enableDarkTheme ? "BaseDark" : "BaseLight";
            UserSettings.Settings.ThemeAccent = _selectedThemeAccent;

            if (!System.IO.File.Exists(PAM_EXE)) { return; }

            if (CreateDesktopShortcut)
            {
                CreatePAMShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            }

            if (LaunchPAM)
            {
                new Process() { StartInfo = new ProcessStartInfo(PAM_EXE) }.Start();
            }
        }

        void UpdateTheme()
        {
            ThemeManager.ChangeAppStyle(
                Application.Current,
                ThemeManager.GetAccent(_selectedThemeAccent),
                ThemeManager.GetAppTheme(_enableDarkTheme ? "BaseDark" : "BaseLight"));
        }

        void CreatePAMShortcut(string shortcutPath)
        {
            var sh = new WshShell();
            var sc = (WshShortcut)sh.CreateShortcut(Path.Combine(shortcutPath, "PAM.lnk"));

            sc.TargetPath = Path.GetFullPath(PAM_EXE);
            sc.WorkingDirectory = Environment.CurrentDirectory;
            sc.Description = "Process Activity Monitor";
            sc.Save();
        }
    }
}
