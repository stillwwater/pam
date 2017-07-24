using PAM.Core.Models;
using PAM.Setup.Helpers;
using PAM.Setup.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace PAM.Setup.ViewModels
{
    internal class ActivitiesViewModel : IChildViewModel, INotifyPropertyChanged
    {
        RelayCommand _newRowCommand;

        public ActivitiesViewModel()
        {
            ActivityRows = new ObservableCollection<TextBox>();
            ActivityRows.CollectionChanged += (s, e) => Notify("ActivityRows");

            ProcessesRows = new ObservableCollection<TextBox>();
            ProcessesRows.CollectionChanged += (s, e) => Notify("ProcessesRows");

            LoadSettings();
        }

        public string Name
        {
            get => "Activities";
        }

        public Strings Strings
        {
            get => Strings.GetInstance();
        }

        public ObservableCollection<TextBox> ActivityRows { get; private set; }

        public ObservableCollection<TextBox> ProcessesRows { get; private set; }

        public ICommand NewRowCommand
        {
            get
            {
                if (_newRowCommand == null)
                {
                    _newRowCommand = new RelayCommand(x => NewRow(), x => CanAddRow());
                }
                return _newRowCommand;
            }
        }

        /// <summary>
        /// Parse rows and save them to UserSettings
        /// </summary>
        public void SaveSettings()
        {
            var acts = UserSettings.Settings.Activities;

            acts.Clear();

            string key;
            string value;

            for (int i = 0; i < ActivityRows.Count; i++)
            {
                key = ActivityRows[i].Text.Trim();
                value = ProcessesRows[i].Text.Trim();

                if (acts.Keys.Contains(key) || key == string.Empty || value == string.Empty)
                {
                    continue;
                }
                acts.Add(key, value.Split(',').Select(x => x.Trim()).ToArray());
            }
            UserSettings.Settings.Activities = acts;
        }

        /// <summary>
        /// Populate ActivtyRows and ProcessesRows from UserSettings
        /// </summary>
        void LoadSettings()
        {
            var acts = UserSettings.Settings.Activities;

            if (acts == null || acts.Count == 0)
            {
                UserSettings.Settings.Activities = new Dictionary<string, string[]>();
                NewRow();
                return;
            }

            foreach (var kv in acts)
            {
                NewRow(kv.Key, string.Join(", ", kv.Value));
            }
        }

        void NewRow(string activityText = "", string processesText = "")
        {
            ActivityRows.Add(new TextBox() { Text = activityText });
            ProcessesRows.Add(new TextBox() { Text = processesText });
        }

        bool CanAddRow()
            => ActivityRows.Last().Text != string.Empty && ProcessesRows.Last().Text != string.Empty;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void Notify(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion INotifyPropertyChanged Members
    }
}
