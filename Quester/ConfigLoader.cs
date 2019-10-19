using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace WowAI
{
    internal partial class Host
    {
        internal static class ConfigLoader
        {
            public static object LoadConfig(string path, Type targetType, object def)
            {
                if (!File.Exists(path))
                    File.WriteAllText(path, JObject.FromObject(def).ToString());
                var obj = JObject.Parse(File.ReadAllText(path)).ToObject(targetType);
                return obj;
            }

            public static void SaveConfig(string path, object def)
            {
                File.WriteAllText(path, JObject.FromObject(def).ToString());
            }
        }
    }
}