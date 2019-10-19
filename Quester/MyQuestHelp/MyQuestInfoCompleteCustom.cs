using System.Collections.Generic;
using Out.Utility;

namespace WowAI
{
    internal partial class MyQuestHelpClass
    {
        public Dictionary<uint, MyQuestInfo> MyQuestInfosCompleteCustom = new Dictionary<uint, MyQuestInfo>
        {
            {3118, new MyQuestInfo(0, new Vector3F(10518.44, 780.10, 1329.60))},
            {5654, new MyQuestInfo(6018, new Vector3F(1452.2, -4179.5, 0))},
        };
    }
}