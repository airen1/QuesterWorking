using System.Collections.Generic;
using WoWBot.Core;

namespace WowAI
{
    internal partial class MyQuestHelpClass
    {
        public List<MyQuest> Quest1516StonetalonMountains = new List<MyQuest>
        {
            new MyQuest(1062, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(6548, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(6548, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(6548, QuestAction.Run, ERace.None, EClass.None, 1),
            new MyQuest(6548, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(6629, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(6629, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(6629, QuestAction.Run, ERace.None, EClass.None, 1),
            new MyQuest(QuestAction.Grind, 17, new RoundZone(111, -351.5, 150)),
            new MyQuest(6629, QuestAction.Complete, ERace.None, EClass.None),
        };
    }
}