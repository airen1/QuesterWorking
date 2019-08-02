using System;
using System.Collections.Generic;

namespace WowAI
{
    [Serializable]
    public class MonsterGroup2
    {
        public List<MonsterGroup> MonsterGroups = new List<MonsterGroup>();
    }

    public class MonsterGroup
    {
        public uint QuestId;
        public List<uint> MonstersId = new List<uint>();
    }
}