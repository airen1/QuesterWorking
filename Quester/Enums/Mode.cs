using System.ComponentModel;

namespace WowAI
{
    public enum Mode
    {
        [Description("Квестинг")]
        Questing,
        [Description("Фарм мобов")]
        FarmMob,
        [Description("Фарм ресурсов")]
        FarmResource,
        [Description("Скрипт")]
        Script,
        [Description("Отбивание")]
        OnlyAttack,
        [Description("Квестинг классик")]
        QuestingClassic,
    }
}
