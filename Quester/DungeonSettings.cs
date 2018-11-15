using Out.Internal.Core;
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
        public int PropId { get; set; }
        public bool Attack { get; set; }
        public int Pause { get; set; }
        public int ItemId { get; set; }
        public uint SkillId { get; set; }
        public string Com { get; set; }
        public string PluginPath { get; set; }
    }   
}


/*[13.08.2016 10:45:21] OutSide: [Serializable]
    internal class AppConfig
    {
        [NonSerialized]
        public static string path = "settings.json";
        private bool startClientMinimized = false;
        
        public bool StartClientMinimized
        {
            get
            {
                return startClientMinimized;
            }
            set
            {
                startClientMinimized = value;
                ConfigLoader.SaveConfig(path, this);
            }
        }
    }
//[13.08.2016 10:45:34] OutSide: вот тебе простейший пример конфига (потом можно добавлять другие нужные переменные
//[13.08.2016 10:45:43] OutSide: internal static class ConfigLoader
    {
        public static object LoadConfig(string path, Type targetType, object def)
        {
            if (!File.Exists(path))
                File.WriteAllText(path, JObject.FromObject(def).ToString());
            var obj = JObject.Parse(File.ReadAllText(path)).ToObject(targetType);
            (obj as Config).InitAfterLoad();
            return obj;
        }

        public static void SaveConfig(string path, object def)
        {
            File.WriteAllText(path, JObject.FromObject(def).ToString());
        }
    }

//класс который его сохраняет\загружает
[13.08.2016 10:46:01] OutSide: 
сама загрузка
var aconfig = new AppConfig();
aconfig = (AppConfig)ConfigLoader.LoadConfig(AppConfig.path, typeof(AppConfig), aconfig);
[13.08.2016 10:46:30] OutSide: читай сколько хочешь, хочешь чтото изменить - новое значение присвоил, и он его и сохранит сразу на диске
[13.08.2016 10:47:16] OutSide: в референсах - Newtonsoft.Json
*/
