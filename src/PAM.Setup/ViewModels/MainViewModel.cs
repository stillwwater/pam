using PAM.Core.Models;
using PAM.Setup.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace PAM.Setup.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        readonly IChildViewModel[] _childVMs;
        IChildViewModel _currentVM;
        RelayCommand _nextViewCommand;
        RelayCommand _previousViewCommand;
        string _nextButtonText;
        string _title;
        int _pos;

        public MainViewModel()
        {
            NextButtonText = "Next";

            _childVMs = new IChildViewModel[]
            {
                new HomeViewModel(),
                new ActivitiesViewModel(),
                new QueryingViewModel(),
                new InterpretersViewModel(),
                new OtherSettingsViewModel()
            };

            _pos = -1;
            ChangeView(1);
        }

        public ICommand NextViewCommand
        {
            get
            {
                if (_nextViewCommand == null)
                {
                    _nextViewCommand = new RelayCommand(x => ChangeView(1), x => CanChangeView(1));
                }
                return _nextViewCommand;
            }
        }

        public ICommand PreviousViewCommand
        {
            get
            {
                if (_previousViewCommand == null)
                {
                    _previousViewCommand = new RelayCommand(x => ChangeView(-1), x => CanChangeView(-1));
                }
                return _previousViewCommand;
            }
        }

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        public IChildViewModel CurrentVM
        {
            get => _currentVM;
            set => Set(ref _currentVM, value);
        }

        public string NextButtonText
        {
            get => _nextButtonText;
            set => Set(ref _nextButtonText, value);
        }

        public void SafeExit()
        {
            foreach (var vm in _childVMs)
            {
                vm.SaveSettings();
            }

            UserSettings.Settings.Save();
        }

        void ChangeView(int offset)
        {
            _pos += offset;

            NextButtonText = _pos == _childVMs.Length - 1 ? "Finish" : "Next";

            if (_pos >= _childVMs.Length)
            {
                SafeExit();
                Application.Current.Shutdown();
                return;
            }

            CurrentVM = _childVMs[_pos];
            Title = $"PAM - Setup ({_pos}/{_childVMs.Length - 1})";
        }

        bool CanChangeView(int offset) => _pos + offset >= 0;

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
