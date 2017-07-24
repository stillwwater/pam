using PAM.BackgroundProcess.Dialogs;
using PAM.Core.Exceptions;
using PAM.Core.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace PAM.BackgroundProcess
{
    internal partial class MainWindow : Form
    {
        const string LOG_FILE = "pam_bgprocess.log";
        const string PAM_GUI_EXECUTABLE = "pam.exe";
        readonly ActivityMonitor _actMonitor;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            FormClosed += (s, e) => SaveLog();

            _actMonitor = new ActivityMonitor();

            try
            {
                _actMonitor.LoadActivities();
                _actMonitor.Start();
            }
            catch (Exception e) when (e is NullSettingException || e is ProcessMonitorException)
            {
                Logger.Log.Write(e.Message);
                Close();
            }

            NotifyIcon.MouseClick += NotifyIcon_MouseClick;
            NotifyIcon.ContextMenu = BuildMenu();

            Logger.Log.Write("Started PAM Background Process.");
        }

        /// <summary>
        /// Hides the form window on startup.
        /// </summary>
        /// <param name="value"></param>
        protected override void SetVisibleCore(bool value)
        {
            if (value && !IsHandleCreated)
            {
                value = false;
                CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SafeExit(openGui: true);
            }
        }

        ContextMenu BuildMenu()
        {
            var items = new MenuItem[]
            {
                new MenuItem("Open GUI", (s, e) => SafeExit(openGui: true)),
                new MenuItem("Status", (s, e) => new DataDialog(_actMonitor.Activities).Show()),
                new MenuItem("Show Log", (s, e) => new LogDialog(Logger.Log.Content).Show()),
                new MenuItem("Exit", (s, e) => SafeExit(openGui: false))
            };

            return new ContextMenu(items);
        }

        /// <summary>
        /// Safely close the application.
        /// </summary>
        /// <param name="openGui"></param>
        void SafeExit(bool openGui)
        {
            Logger.Log.Write("Closing PAM Background Process...");

            if (_actMonitor != null)
            {
                if (!openGui) { _actMonitor.StopAllActivities(); }
                _actMonitor.Dispose();
            }

            if (openGui)
            {
                StartProcess(PAM_GUI_EXECUTABLE);
            }

            Close();
        }

        void StartProcess(string processName)
        {
            if (!File.Exists(processName))
            {
                Logger.Log.Write($"Error: Could not find {PAM_GUI_EXECUTABLE}");
                Close();
                return;
            }

            var pInfo = new ProcessStartInfo()
            {
                FileName = processName,
                Arguments = "ignore(StartInBackground)"
            };
            var p = new Process() { StartInfo = pInfo };

            p.Start();
        }

        void SaveLog() => File.WriteAllText(LOG_FILE, Logger.Log.Content);
    }
}
