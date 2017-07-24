using PAM.Core.Exceptions;
using PAM.Core.Models;
using PAM.UI.Helpers;
using PAM.UI.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PAM.UI.ViewModels
{
    internal class DataViewModel : IDisposable
    {
        readonly string _tmpDataFile;
        readonly string _csvFile;
        readonly ActivityMonitor _actMonitor;

        public DataViewModel()
        {
            string dataDir = UserSettings.Settings.DataDirectory;

            _tmpDataFile = Path.Combine(dataDir, UserSettings.Settings.TmpDataFile);
            _csvFile = Path.Combine(dataDir, UserSettings.Settings.PamDataCSV);

            _actMonitor = new ActivityMonitor();

            try
            {
                _actMonitor.LoadActivities();
            }
            catch (NullSettingException e)
            {
                Dialog.Alert(e.Message, log: true);
            }

            DateTime? lastRecord = UserSettings.Settings.LastRecordDate;

            if (lastRecord == null) { SaveDate(); }

            if (_actMonitor.State == ActivityMonitorState.Ready &&
                (lastRecord != null && lastRecord != DateTime.Now.Date))
            {
                NewDate();
            }

            // Populate the DataGrid using data from the CSV data file
            CSVData = new CSV(_csvFile, "Date").ToDataTable(readFile: true);

            // Initialize ProcessMonitor in the background
            Task.Run(() => StartActivityMonitor());
        }

        public Activity[] Activities
        {
            get => _actMonitor.Activities;
        }

        public DataTable CSVData
        {
            get;
            private set;
        }

        /// <summary>
        /// Safely disposes the DataViewModel class instance.
        /// </summary>
        /// <param name="stopActicities"></param>
        public void Dispose(bool stopActicities)
        {
            if (stopActicities) { _actMonitor.StopAllActivities(); }
            _actMonitor.Dispose();
        }

        /// <summary>
        /// Safely disposes the DataViewModel class instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(stopActicities: true);
        }

        /// <summary>
        /// Converts Activity Data to CSV
        /// </summary>
        CSV ToCSV(bool saveFile = false)
        {
            var header = new List<string>() { "Date" };
            var data = new List<string>();

            header.AddRange(UserSettings.Settings.Activities.Keys.ToArray());
            data.Add(((DateTime)UserSettings.Settings.LastRecordDate).ToString("yyyy-MM-dd"));

            CSV csv = new CSV(_csvFile, header.ToArray());

            foreach (var activity in UserSettings.Settings.Activities.Keys)
            {
                Activity a = _actMonitor.Lookup(activity);

                if (a == null)
                {
                    data.Add("0");
                    continue;
                }
                data.Add(a.ElapsedTime.ToString());
            }
            csv.AppendRow(data.ToArray());

            if (saveFile) { csv.Save(); }
            return csv;
        }

        void StartActivityMonitor()
        {
            try
            {
                _actMonitor.Start();
            }
            catch (Exception e) when (e is NullSettingException || e is ProcessMonitorException)
            {
                Dialog.Alert(e.Message, log: true);
            }
        }

        void NewDate()
        {
            _actMonitor.StopAllActivities();
            ToCSV(saveFile: true);
            File.Delete(_tmpDataFile);

            _actMonitor.LoadActivities();

            SaveDate();
        }

        void SaveDate()
        {
            UserSettings.Settings.LastRecordDate = DateTime.Now.Date;
            UserSettings.Settings.Save();
        }
    }
}
