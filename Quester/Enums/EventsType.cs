using System.ComponentModel;

namespace WowAI
{
    public enum EventsType
    {
        NotSellected = -1,
        [Description("Нет активности")]
        Inactivity,
        [Description("Смерть")]
        Death,
        DeathPlayer,
        ChatMessage,
        Gm,
        AttackPlayer,
        PartyInvite,
        ClanInvite,
        GmServer,
        PlayerInZone
    }
}