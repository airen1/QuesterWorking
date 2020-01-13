using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WowAI
{
    public class EventSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private EventsAction _actionEvent;
        public EventsAction ActionEvent
        {
            get => _actionEvent;
            set
            {
                if (_actionEvent == value)
                {
                    return;
                }

                _actionEvent = value;
                NotifyPropertyChanged();
            }
        }

        private EventsType _typeEvents;
        public EventsType TypeEvents
        {
            get => _typeEvents;
            set
            {
                if (_typeEvents == value)
                {
                    return;
                }

                _typeEvents = value;
                NotifyPropertyChanged();
            }
        }

        private string _soundFile;
        public string SoundFile
        {
            get => _soundFile;
            set
            {
                if (_soundFile == value)
                {
                    return;
                }

                _soundFile = value;
                NotifyPropertyChanged();
            }
        }

        private int _pause;
        public int Pause
        {
            get => _pause;
            set
            {
                if (_pause == value)
                {
                    return;
                }

                _pause = value;
                NotifyPropertyChanged();
            }
        }
    }
}