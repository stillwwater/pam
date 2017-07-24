using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PAM.Core.Models
{
    public class Logger : INotifyPropertyChanged
    {
        static Logger _instance;
        string _content;
        bool _isOpen;

        private Logger()
        {
            _isOpen = UserSettings.Settings.LogOpened;
            _content = string.Empty;
        }

        public static Logger Log
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Logger();
                }
                return _instance;
            }
        }

        public static bool Silent { get; set; }

        public string Content
        {
            get => _content;
            private set => Set(ref _content, value);
        }

        public bool IsOpen
        {
            get => _isOpen;
            set => Set(ref _isOpen, value);
        }

        /// <summary>
        /// Add timestamped message to the log.
        /// </summary>
        /// <param name="message"></param>
        public void Write(string message)
        {
            if (Silent || string.IsNullOrEmpty(message)) { return; }
            _instance.Content += $"{DateTime.Now.ToString("h:mm:ss tt")}    {message}\r\n";
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
