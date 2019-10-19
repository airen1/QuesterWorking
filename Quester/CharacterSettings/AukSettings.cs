using System.ComponentModel;
using System.Runtime.CompilerServices;
using Out.Utility;


namespace WowAI
{
    public class AukSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

       
        private int _id;
        public int Id
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
                NotifyPropertyChanged();
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged();
            }
        }

        private int _level;
        public int Level
        {
            get => _level;
            set
            {
                if (_level == value) return;
                _level = value;
                NotifyPropertyChanged();
            }
        }

        private ulong _maxPrice;
        public ulong MaxPrice
        {
            get => _maxPrice;
            set
            {
                if (_maxPrice == value) return;
                _maxPrice = value;
                NotifyPropertyChanged();
            }
        }
        private ulong _disscount;
        public ulong Disscount
        {
            get => _disscount;
            set
            {
                if (_disscount == value) return;
                _disscount = value;
                NotifyPropertyChanged();
            }
        }

        private uint _maxCount;
        public uint MaxCount
        {
            get => _maxCount;
            set
            {
                if (_maxCount == value) return;
                _maxCount = value;
                NotifyPropertyChanged();
            }
        }
    }
}