using PAM.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PAM.UI.ViewModels
{
    internal class PlotsViewModel : INotifyPropertyChanged
    {
        readonly BackgroundWorker _worker;
        string _selectedScript;
        string _warningText;
        Visibility _imageVisibility;
        Visibility _progressBarVisibility;
        Stream _mediaStream;
        BitmapImage _plotImage;

        /// <summary>
        /// Initializes a new instance of the PlotsViewModel class.
        /// </summary>
        public PlotsViewModel()
        {
            ChangeVisibilty(false, "Nothing to load.");

            _worker = new BackgroundWorker();

            _worker.DoWork += _worker_DoWork;

            _worker.RunWorkerCompleted += (s, e) =>
            {
                if ((bool)e.Result) { LoadPlotImage(); }
                ProgressBarVisibility = Visibility.Hidden;
            };

            Scripts = Directory.GetFiles(UserSettings.Settings.ScriptsDirectory,
                                         $"*{UserSettings.Settings.ScriptExtension}")
                                         .Where(x => !Path.GetFileName(x).StartsWith("_"))
                                         .ToArray();

            if (Scripts.Length > 0)
            {
                SelectedScript = Scripts[0];
            }
        }

        public string[] Scripts { get; private set; }

        public string WarningText
        {
            get => _warningText;
            set => Set(ref _warningText, value);
        }

        public Visibility ImageVisibility
        {
            get => _imageVisibility;
            set => Set(ref _imageVisibility, value);
        }

        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => Set(ref _progressBarVisibility, value);
        }

        public string SelectedScript
        {
            get => _selectedScript;
            set
            {
                Set(ref _selectedScript, value);

                if (!_worker.IsBusy && !string.IsNullOrEmpty(_selectedScript))
                {
                    _worker.RunWorkerAsync(_selectedScript);
                }
            }
        }

        public BitmapImage PlotImage
        {
            get => _plotImage;
            set => Set(ref _plotImage, value);
        }

        #region Imaging

        /// <summary>
        /// Disposes of media resources and forces Garbage Collection. Should be called before
        /// loading a new BitmapImage.
        /// </summary>
        public void FreeResources()
        {
            if (_mediaStream != null)
            {
                _mediaStream.Close();
                _mediaStream.Dispose();
                _mediaStream = null;
            }
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
        }

        void LoadPlotImage()
        {
            // Find image file in the Plots directory with the same file name as the script file
            // minus the file extension.
            string path = Directory.GetFiles(UserSettings.Settings.PlotsDirectory)
                        .ToList()
                        .Find(x => IsSimilarFileName(x, Path.GetFileName(_selectedScript)));

            if (File.Exists(path))
            {
                FreeResources();

                var bmp = new BitmapImage();
                _mediaStream = new FileStream(path, FileMode.Open);

                try
                {
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.None;
                    bmp.StreamSource = _mediaStream;
                    bmp.EndInit();

                    bmp.Freeze();
                    PlotImage = bmp;

                    ChangeVisibilty(true);
                }
                catch (NotSupportedException)
                {
                    ChangeVisibilty(false, $"Error: \"{path}\" is not a supported image format");
                }
            }
            else
            {
                ChangeVisibilty(false, $"Error: Could not find the output from \"{_selectedScript}\"");
            }
        }

        void ChangeVisibilty(bool isVisible, string message = "")
        {
            if (isVisible)
            {
                ImageVisibility = Visibility.Visible;
                WarningText = string.Empty;
            }
            else
            {
                ImageVisibility = Visibility.Hidden;
                WarningText = message;
                Logger.Log.Write($"Plots: {message}");
            }
        }

        #endregion Imaging

        #region BackgroundWorker

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ProgressBarVisibility = Visibility.Visible;

            if (UserSettings.Settings.InterpreterPath == null)
            {
                ChangeVisibilty(false, "Please include an interpreter path in the settings file.");
                e.Result = false;
                return;
            }

            if (e.Argument == null) { throw new ArgumentException(); }

            ChangeVisibilty(false, $"Executing \"{e.Argument}\"");
            FreeResources();

            string err = RunInterpreter(e.Argument.ToString());

            // External process returned an error.
            if (err != string.Empty)
            {
                e.Result = false;
                ChangeVisibilty(false, $"Error executing \"{e.Argument}\": {err}");
                return;
            }

            e.Result = true;
        }

        #endregion BackgroundWorker

        string RunInterpreter(string script)
        {
            var p = new Process();

            var pInfo = new ProcessStartInfo()
            {
                FileName = UserSettings.Settings.InterpreterPath,
                Arguments = script,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            p.StartInfo = pInfo;
            p.Start();

            string stderr = p.StandardError.ReadToEnd();
            p.WaitForExit();

            return stderr;
        }

        /// <summary>
        /// Checks if a file has the same name as another file with a different extension.
        /// </summary>
        /// <param name="fileA"></param>
        /// <param name="fileB"></param>
        /// <returns></returns>
        bool IsSimilarFileName(string fileA, string fileB)
            => Path.GetFileNameWithoutExtension(fileA) == Path.GetFileNameWithoutExtension(fileB)
            && fileA != fileB;

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
