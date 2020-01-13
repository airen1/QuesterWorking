using System.Collections.Generic;
using Out.Utility;
using WoWBot.Core;

namespace WowAI
{
    internal partial class MyQuestHelpClass
    {
        public List<MyQuest> Quest18HumanIronforgeRedridge = new List<MyQuest>
        {
            new MyQuest(2041, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(2041, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(1338, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(246, QuestAction.UseFlyPath, "Lakeshire, Redridge", new Vector3F(-8836.2, 490.1, 0)),
            new MyQuest(125, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(3741, QuestAction.Apply, ERace.None, EClass.None),
           // new MyQuest(127, QuestAction.Apply, ERace.None, EClass.None),
        };
    }
}