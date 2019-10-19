using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WowAI
{
    public class RegenItems : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _checked;
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                NotifyPropertyChanged();
            }
        }
        private uint _itemId;
        public uint ItemId
        {
            get => _itemId;
            set
            {
                if (_itemId == value) return;
                _itemId = value;
                NotifyPropertyChanged();
            }
        }
        private uint _spellId;
        public uint SpellId
        {
            get => _spellId;
            set
            {
                if (_spellId == value) return;
                _spellId = value;
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

        public int Priority { get; set; }
        public int MeMaxHp { get; set; } = 100;
        public int MeMinHp { get; set; }
        public int MeMaxMp { get; set; } = 100;
        public int MeMinMp { get; set; }
        public bool InFight { get; set; }
        public uint NotMeEffect { get; set; }
        public uint IsMeEffect { get; set; }
        //   public bool WaitEndBuff { get; set; }
        //    public bool UseSkillAfterSkill { get; set; }
        //    public uint UseSkillAfterSkillId { get; set; }
        //    public bool UseKeyAfterItem { get; set; }
        //  public int UseKeyAfterItemKey { get; set; }
        //    public int Level { get; set; }
    }
}