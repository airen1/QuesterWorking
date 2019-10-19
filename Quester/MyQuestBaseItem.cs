using System.Collections.Generic;

namespace WowAI
{
    internal partial class Host
    {
        public class MyQuestBase
        {
            public List<MyQuestBaseItem> MyQuestBases = new List<MyQuestBaseItem>();
        }

        public class MyQuestBaseItem
        {
            public uint Id = 0;
            public uint Level = 0;
            public uint RequiresLevel = 0;
            public uint Side = 0;
            public List<uint> Race = new List<uint>();
            public List<uint> Class = new List<uint>();
            public MyQuestStart QuestStart = new MyQuestStart();
            public MyQuestEnd QuestEnd = new MyQuestEnd();
            public uint PreviousQuest = 0;
        }
        public class MyQuestStart
        {
            public uint QuestStartId = 0;
            public EMyUnitType QuestStarType = EMyUnitType.Unknown;
        }

        public class MyQuestEnd
        {
            public uint QuestEndId = 0;
            public EMyUnitType QuestEndType = EMyUnitType.Unknown;
        }
    }
}