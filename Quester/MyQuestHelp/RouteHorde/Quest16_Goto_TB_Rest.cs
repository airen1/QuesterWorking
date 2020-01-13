using System.Collections.Generic;
using Out.Utility;
using WoWBot.Core;

namespace WowAI
{
    internal partial class MyQuestHelpClass
    {
        public List<MyQuest> Quest16GotoTBRest = new List<MyQuest>
        {
            new MyQuest(QuestAction.GetFp, 853, new Vector3F(-2384.1, -1881.6, 0)),
            new MyQuest(853, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(QuestAction.GetFp, 4921, new Vector3F(-1196.5, 26.1, 0)),
            new MyQuest(QuestAction.UseHs, 4921, new Vector3F(-1196.5, 26.1, 0)),
            new MyQuest(4921, QuestAction.Complete, ERace.None, EClass.None),
        };
    }
}