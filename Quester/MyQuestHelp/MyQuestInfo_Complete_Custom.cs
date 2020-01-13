using System.Collections.Generic;
using Out.Utility;

namespace WowAI
{
    internal partial class MyQuestHelpClass
    {
        public Dictionary<uint, MyQuestInfo> MyQuestInfosCompleteCustom = new Dictionary<uint, MyQuestInfo>
        {
            {3114, new MyQuestInfo(0, new Vector3F(-6058.01, 388.26, 392.76)) },
            {5635, new MyQuestInfo(0, new Vector3F(-8515.6, 859.8, 0)) },
            {1718, new MyQuestInfo(0, new Vector3F(-1706.63, -4331.65, 4.36))},

            {3118, new MyQuestInfo(0, new Vector3F(10518.44, 780.10, 1329.60))},
            {5654, new MyQuestInfo(6018, new Vector3F(1452.2, -4179.5, 0))},
            {5928, new MyQuestInfo(3033, new Vector3F(-1042.59, -279.16, 159.03 ))},
            {861, new MyQuestInfo(3441, new Vector3F(-1411.18, -123.60, 158.94))},
        };
    }
}