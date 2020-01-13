using System.Collections.Generic;
using Out.Utility;
using WoWBot.Core;

namespace WowAI
{
    internal partial class MyQuestHelpClass
    {
        public List<MyQuest> Quest1213HumanLochModanDarkshore = new List<MyQuest>
        {
            new MyQuest(224, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(267, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(224, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(224, QuestAction.Run, ERace.None, EClass.None, 1),
            new MyQuest(267, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(224, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(267, QuestAction.Complete, ERace.None, EClass.None),
            //new MyQuest(416, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(1339, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(QuestAction.SetHs, 353, new Vector3F(-5378.5, -2973.5, 0)),
            new MyQuest(418, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(353, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(1339, QuestAction.Complete, ERace.None, EClass.None),
            //new MyQuest(307, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(1338, QuestAction.Apply, ERace.None, EClass.None),
            //new MyQuest(307, QuestAction.Run, ERace.None, EClass.None, 0),
            //new MyQuest(416, QuestAction.Run, ERace.None, EClass.None, 0),
            //new MyQuest(307, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(418, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(418, QuestAction.Run, ERace.None, EClass.None, 2),
            new MyQuest(418, QuestAction.Run, ERace.None, EClass.None, 1),
            //new MyQuest(416, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(418, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(QuestAction.Grind, 15, new RoundZone(-5391.1, -2954.9, 250)),
//new MyQuest(246, QuestAction.UseFlyPath, "Ironforge, Dun Morogh", new Vector3F(-5424.7,-2929.9, 0)),
//new MyQuest(246, QuestAction.UseFlyPath, "Thelsamar, Loch Modan", new Vector3F(-4821.2,-1152.5, 0)),
            new MyQuest(QuestAction.GetFp, 3524, new Vector3F(-3793.2, -782.3, 0)),
            new MyQuest(963, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(3524, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(QuestAction.GetFp, 3524, new Vector3F(6343.3, 561.6, 0)),
            new MyQuest(QuestAction.SetHs, 3524, new Vector3F(6407.7, 518.1, 0)),
            new MyQuest(983, QuestAction.Apply, ERace.None, EClass.None),
            //new MyQuest(947, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(4811, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(958, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(954, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(2118, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(984, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(3524, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(3524, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(4681, QuestAction.Apply, ERace.None, EClass.None),
        };
    }
}