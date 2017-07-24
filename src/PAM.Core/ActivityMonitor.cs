using PAM.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Xml;
using System.Xml.Serialization;

namespace PAM.Core.Models
{
    public enum ActivityMonitorState
    {
        Failed,
        Ready,
        Running
    }

    public class ActivityMonitor
    {
        readonly string _tmpDataFile;
        List<Activity> _activities;

        /// <summary>
        /// Initializes a new instance of the ActivityMonitor class.
        /// </summary>
        public ActivityMonitor()
        {
            _tmpDataFile = Path.Combine(UserSettings.Settings.DataDirectory, UserSettings.Settings.TmpDataFile);
        }

        public Activity[] Activities
        {
            get => State == ActivityMonitorState.Failed ? new Activity[0] : _activities.ToArray();
        }

        public ActivityMonitorState State { get; private set; }

        public void LoadActivities()
        {
            if (UserSettings.Settings.Activities == null)
            {
                throw new NullSettingException("Activities");
            }

            State = ActivityMonitorState.Ready;

            // Load previous activity data
            if (File.Exists(_tmpDataFile))
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<Activity>));
                FileStream fs = File.OpenRead(_tmpDataFile);

                try
                {
                    // Deserialize activities and store in _activities List
                    _activities = (List<Activity>)ser.Deserialize(fs);
                    return;
                }
                catch (XmlException e)
                {
                    Logger.Log.Write($"XML error while loading {_tmpDataFile}: {e.Message}");
                    State = ActivityMonitorState.Failed;
                }
                finally
                {
                    fs.Close();
                    fs.Dispose();
                }
            }

            _activities = new List<Activity>();

            foreach (string a in UserSettings.Settings.Activities.Keys)
            {
                _activities.Add(new Activity(a));
            }
        }

        /// <summary>
        /// Safely disposes of an ActivityMonitor instance.
        /// </summary>
        public void Dispose()
        {
            if (_activities == null || _activities.Count == 0) { return; }

            if (File.Exists(_tmpDataFile))
            {
                File.Delete(_tmpDataFile);
            }

            ProcessMonitor.DisposeWatchers();

            XmlSerializer ser = new XmlSerializer(typeof(List<Activity>));
            FileStream fs = File.OpenWrite(_tmpDataFile);

            // Serialize Activity objects.
            ser.Serialize(fs, _activities);

            fs.Close();
            fs.Dispose();
            State = ActivityMonitorState.Ready;
        }

        /// <summary>
        /// Stops all running Activities.
        /// </summary>
        public void StopAllActivities()
        {
            if (State == ActivityMonitorState.Failed) { return; }

            foreach (var a in _activities)
            {
                if (a.State == ActivityState.Running)
                {
                    a.Stop();
                }
            }
            State = ActivityMonitorState.Ready;
        }

        public Activity Lookup(string name) => _activities.Find(x => x.Name == name);

        #region Process Monitoring

        /// <summary>
        /// Start all ProcessMonitors
        /// </summary>
        public void Start()
        {
            if (State == ActivityMonitorState.Failed) { return; }

            Logger.Log.Write("Initializing ProcessMonitors...");

            if (UserSettings.Settings.Activities == null)
            {
                State = ActivityMonitorState.Failed;
                throw new NullSettingException("Activities");
            }

            foreach (string activity in UserSettings.Settings.Activities.Keys)
            {
                foreach (string process in UserSettings.Settings.Activities[activity])
                {
                    ProcessMonitor monitor;

                    try
                    {
                        monitor = new ProcessMonitor(process);
                    }
                    catch (Exception e) when (e is ManagementException || e is ArgumentException)
                    {
                        State = ActivityMonitorState.Failed;
                        throw new ProcessMonitorException(e.Message);
                    }

                    // Process has started.
                    monitor.OnStart += (s, e) => HandleStart(process, activity);

                    // Process has ended.
                    monitor.OnStop += (s, e) => HandleStop(process, activity);
                }
            }
            Logger.Log.Write("Successfully initialized ProcessMonitors.");
            State = ActivityMonitorState.Running;
        }

        void HandleStart(string process, string activity)
        {
            Activity current = Lookup(activity);

            if (current == null)
            {
                current = new Activity(activity);
                _activities.Add(current);
            }

            current.Processes.Add(process);
            Logger.Log.Write($"Start {process} from {activity}");

            // First process, start activity.
            if (current.Processes.Count == 1)
            {
                current.Start();
            }
        }

        void HandleStop(string process, string activity)
        {
            Activity current = Lookup(activity);

            if (current == null || !current.Processes.Contains(process))
            {
                // Process was initialized before the watcher.
                Logger.Log.Write($"Process Monitor Warning: {process} time not counted");
                return;
            }

            current.Processes.Remove(process);
            Logger.Log.Write($"Stop {process} from {activity}");

            // This activity does not have any more running processes.
            if (current.Processes.Count == 0)
            {
                current.Stop();
            }
        }

        #endregion Process Monitoring
    }
}
