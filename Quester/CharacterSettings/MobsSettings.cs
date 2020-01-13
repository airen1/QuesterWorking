﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WowAI
{
    public class MobsSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private uint _id;
        public uint Id
        {
            get => _id;
            set
            {
                if (_id == value)
                {
                    return;
                }

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
                if (_name == value)
                {
                    return;
                }

                _name = value;
                NotifyPropertyChanged();
            }
        }

        private Priority _priority;
        public Priority Priority
        {
            get => _priority;
            set
            {
                if (_priority == value)
                {
                    return;
                }

                _priority = value;
                NotifyPropertyChanged();
            }
        }

        private int _level;
        public int Level
        {
            get => _level;
            set
            {
                if (_level == value)
                {
                    return;
                }

                _level = value;
                NotifyPropertyChanged();
            }
        }
    }
}