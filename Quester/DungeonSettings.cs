using System;
using System.Collections.Generic;
using Out.Utility;


namespace WowAI
{

    [Serializable]
    public class DungeonSetting
    {      
        public List<DungeonCoordSettings> DungeonCoordSettings = new List<DungeonCoordSettings>();
    }

    public class DungeonCoordSettings
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public Vector3F Loc { get; set; }
        public int MapId { get; set; }
        public uint AreaId { get; set; }
        public int MobId { get; set; }
        public uint PropId { get; set; }
        public bool Attack { get; set; }
        public int Pause { get; set; }
        public int ItemId { get; set; }
        public uint SkillId { get; set; }
        public string Com { get; set; }
        public string PluginPath { get; set; }
    }   
}
