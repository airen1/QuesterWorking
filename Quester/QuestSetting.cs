using Out.Internal.Core;
using System;
using System.Collections.Generic;
using Out.Utility;


namespace WowAI
{

    [Serializable]
    public class QuestSetting
    {

        public List<QuestCoordSettings> QuestCoordSettings = new List<QuestCoordSettings>();
    }

    public class QuestCoordSettings
    {
        public bool Run { get; set; } = true;
        public uint QuestId { get; set; }
        public string QuestName { get; set; } = "";
        public uint NpcId { get; set; }
        public Vector3F Loc { get; set; }
        public string State { get; set; }
        public int Level { get; set; }
        public int MinLevel { get; set; }
        public int RealLevel { get; set; }
    }
}