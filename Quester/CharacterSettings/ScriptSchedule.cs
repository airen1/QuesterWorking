using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WowAI
{
    public class ScriptSchedule : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private TimeSpan _scriptStartTime;
        public TimeSpan ScriptStartTime
        {
            get => _scriptStartTime;
            set
            {
                if (_scriptStartTime == value) return;
                _scriptStartTime = value;
                NotifyPropertyChanged();
            }
        }

        private TimeSpan _scriptStopTime;
        public TimeSpan ScriptStopTime
        {
            get => _scriptStopTime;
            set
            {
                if (_scriptStopTime == value) return;
                _scriptStopTime = value;
                NotifyPropertyChanged();
            }
        }

        private string _scriptName;
        public string ScriptName
        {
            get => _scriptName;
            set
            {
                if (_scriptName == value) return;
                _scriptName = value;
                NotifyPropertyChanged();
            }
        }
        private bool _reverse;
        public bool Reverse
        {
            get => _reverse;
            set
            {
                if (_reverse == value) return;
                _reverse = value;
                NotifyPropertyChanged();
            }
        }
    }
}