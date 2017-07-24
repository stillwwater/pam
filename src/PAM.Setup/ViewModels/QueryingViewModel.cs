using PAM.Core.Models;
using PAM.Setup.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PAM.Setup.ViewModels
{
    internal class QueryingViewModel : IChildViewModel, INotifyPropertyChanged
    {
        int _delaySetting;
        string _valueString;

        /// <summary>
        /// Initializes a new instance of the QueryingViewModel class.
        /// </summary>
        public QueryingViewModel()
        {
            DelaySetting = UserSettings.Settings.WatchEventDelay;
        }

        public string Name
        {
            get => "Querying";
        }

        public Strings Strings
        {
            get => Strings.GetInstance();
        }

        public int DelaySetting
        {
            get => _delaySetting;
            set
            {
                Set(ref _delaySetting, value);
                ValueString = FormatSeconds(_delaySetting);
            }
        }

        public string ValueString
        {
            get => _valueString;
            set => Set(ref _valueString, value);
        }

        public void SaveSettings()
        {
            UserSettings.Settings.WatchEventDelay = DelaySetting;
        }

        string FormatSeconds(int seconds)
        {
            string res = string.Empty;
            var ts = TimeSpan.FromSeconds(seconds);

            if (ts.Minutes > 0)
            {
                res += $"{ts.Minutes} minute{(ts.Minutes == 1 ? "" : "s")} ";
            }

            return $"{res}{ts.Seconds} second{(ts.Seconds == 1 ? "" : "s")}";
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
