using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WowAI
{
    public class PetSettings : INotifyPropertyChanged
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

        private string _type;
        public string Type
        {
            get => _type;
            set
            {
                if (_type == value) return;
                _type = value;
                NotifyPropertyChanged();
            }
        }

        private EMountType _mountType;

        public EMountType MountType
        {
            get => _mountType;
            set
            {
                if (_mountType == value)
                    return;
                _mountType = value;
                NotifyPropertyChanged();
            }
        }

    }
}