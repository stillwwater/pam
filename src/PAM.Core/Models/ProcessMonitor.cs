using System;
using System.Collections.Generic;
using System.Management;

namespace PAM.Core.Models
{
    internal class ProcessMonitor
    {
        public OnStartEventHandler OnStart;
        public OnStopEventHandler OnStop;
        static List<ManagementEventWatcher> _watchers = new List<ManagementEventWatcher>();

        /// <summary>
        /// Creates a new instance of the ProcessMonitor class.
        /// </summary>
        /// <param name="log"></param>
        public ProcessMonitor(string process)
        {
            if (UserSettings.Settings.Activities == null)
            {
                throw new NullReferenceException("Please define 'Activities' in the user settings and click reset.");
            }

            if (UserSettings.Settings.WatchEventDelay <= 0)
            {
                throw new ArgumentException("The setting 'WatchEventDelay' must be 1 or greater.");
            }

            try
            {
                Watch(process);
            }
            catch (ManagementException) { throw; }
        }

        public delegate void OnStartEventHandler(object sender, EventArgs e);

        public delegate void OnStopEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Stops and disposes all WMI watchers.
        /// </summary>
        public static void DisposeWatchers()
        {
            foreach (var watcher in _watchers)
            {
                watcher.Stop();
                watcher.Dispose();
            }
        }

        void Watch(string processName)
        {
            int within = UserSettings.Settings.WatchEventDelay;
            string scope = UserSettings.Settings.WatchScope;
            string query = BuildQuery("__InstanceOperationEvent", processName, within);

            var watcher = new ManagementEventWatcher(scope, query);

            watcher.EventArrived += (s, e) =>
            {
                string eventName = e.NewEvent.ClassPath.ClassName;

                if (eventName.CompareTo("__InstanceCreationEvent") == 0)
                {
                    OnStart?.Invoke(this, e);
                }
                else if (eventName.CompareTo("__InstanceDeletionEvent") == 0)
                {
                    OnStop?.Invoke(this, e);
                }
            };

            _watchers.Add(watcher);
            watcher.Start();
        }

        string BuildQuery(string eventName, string processName, int within)
        {
            return "SELECT *"
                + $"  FROM {eventName} "
                + $"WITHIN {within}"
                + $" WHERE TargetInstance ISA 'Win32_Process'"
                + $"   AND TargetInstance.Name = '{processName}'";
        }
    }
}
