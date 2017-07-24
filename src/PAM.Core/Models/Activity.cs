using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PAM.Core.Models
{
    public enum ActivityState
    {
        Stopped,
        Running
    }

    [Serializable()]
    public class Activity : ISerializable, INotifyPropertyChanged
    {
        ObservableCollection<string> _processes;
        ActivityState _state;
        uint _elapsedTime;

        public Activity()
        {
            if (_processes == null)
            {
                _processes = new ObservableCollection<string>();
            }
            _processes.CollectionChanged += (s, e) => Notify("Processes");
        }

        /// <summary>
        /// Creates a new instance of the Activity Class.
        /// </summary>
        /// <param name="name"></param>
        public Activity(string name) : this()
        {
            Name = name;
            ElapsedTime = 0u;
        }

        /// <summary>
        /// Deserializes Activity class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public Activity(SerializationInfo info, StreamingContext context) : this()
        {
            Name = (string)info.GetValue("Name", typeof(string));
            StartTime = (long)info.GetValue("StartTime", typeof(long));
            ElapsedTime = (uint)info.GetValue("ElapsedTime", typeof(uint));
            State = (ActivityState)info.GetValue("State", typeof(ActivityState));
            ProcessesArray = (string[])info.GetValue("Processes", typeof(string[]));
        }

        [XmlIgnore()]
        public ObservableCollection<string> Processes
        {
            get => _processes;
        }

        public string[] ProcessesArray
        {
            get => _processes.ToArray();
            set => _processes = new ObservableCollection<string>(value);
        }

        public ActivityState State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        public long StartTime { get; set; }

        public string Name { get; set; }

        public uint ElapsedTime
        {
            get => _elapsedTime;
            set => Set(ref _elapsedTime, value);
        }

        /// <summary>
        /// Start Activity
        /// </summary>
        public void Start()
        {
            StartTime = DateTime.Now.Ticks;
            State = ActivityState.Running;
        }

        /// <summary>
        /// Add Elapsed time
        /// </summary>
        public void Stop()
        {
            uint diff = (uint)((DateTime.Now.Ticks - StartTime)
                      / TimeSpan.TicksPerMillisecond
                      / UserSettings.Settings.ConvertMS);

            ElapsedTime += diff;
            StartTime = 0L;
            _processes.Clear();
            State = ActivityState.Stopped;

            Logger.Log.Write($"{Name} elapsed time: {ElapsedTime}m (+{diff}m)");
        }

        /// <summary>
        /// Serialize Activity
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("ElapsedTime", ElapsedTime);
            info.AddValue("StartTime", StartTime);
            info.AddValue("ProcessesArray", ProcessesArray);
            info.AddValue("State", State);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void Set<T>(ref T field, T value, [CallerMemberName] string sender = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) { return; }
            field = value;
            Notify(sender);
        }

        void Notify(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion INotifyPropertyChanged Members
    }
}
