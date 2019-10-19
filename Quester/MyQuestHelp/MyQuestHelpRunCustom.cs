using System;
using System.Collections.Generic;
using WoWBot.Core;

namespace WowAI
{
    internal partial class MyQuestHelpClass
    {
        public readonly Dictionary<Tuple<uint, int>, MyQuestInfoRun> MyQuestInfosRunCustom = new Dictionary<Tuple<uint, int>, MyQuestInfoRun>
            {
                {new Tuple<uint, int>(790, 0), new MyQuestInfoRun(new RoundZone(-529.16, -4108.92, 40))},
                {new Tuple<uint, int>(6394, 0), new MyQuestInfoRun(new RoundZone(-88, -4275.5, 20))},
                {new Tuple<uint, int>(794, 0), new MyQuestInfoRun(new RoundZone(-56.8, -4219.9, 20))},
                {new Tuple<uint, int>(784, 2), new MyQuestInfoRun(new RoundZone(-246, -5120.9, 15))},
                {new Tuple<uint, int>(786, 0), new MyQuestInfoRun(new RoundZone(-1056.5, -4595.8, 40))},
                {new Tuple<uint, int>(786, 1), new MyQuestInfoRun(new RoundZone(-917.5, -4482.3, 40))},
                {new Tuple<uint, int>(786, 2), new MyQuestInfoRun(new RoundZone(-974.2, -4406.4, 40))},
                {new Tuple<uint, int>(815, 0), new MyQuestInfoRun(new RoundZone(-1278.1, -5535.9, 500))},
                {new Tuple<uint, int>(817, 0), new MyQuestInfoRun(new RoundZone(-1278.1, -5535.9, 500))},
                {
                    new Tuple<uint, int>(816, 0),
                    new MyQuestInfoRun(new RoundZone(913.87, -3824.96, 150), new List<uint> {3110})
                },
                {
                    new Tuple<uint, int>(5726, 0),
                    new MyQuestInfoRun(new RoundZone(1500.2, -4843.3, 120), new List<uint> {3198, 3197})
                }, //     902.73, -4164.07,
                {
                    new Tuple<uint, int>(6062, 0),
                    new MyQuestInfoRun(new RoundZone(139.34, -4781.44, 150), new List<uint> {3099})
                },
                {
                    new Tuple<uint, int>(6083, 0),
                    new MyQuestInfoRun(new RoundZone(805.89, -5072.35, 250), new List<uint> {3107})
                },
                {
                    new Tuple<uint, int>(6082, 0),
                    new MyQuestInfoRun(new RoundZone(464.82, -4554.64, 150), new List<uint> {3126})
                },
                {new Tuple<uint, int>(806, 0), new MyQuestInfoRun(new RoundZone(868.7, -4189.7, 40))},
                {
                    new Tuple<uint, int>(813, 0),
                    new MyQuestInfoRun(new RoundZone(1318.8, -4743.2, 150), new List<uint> {3127})
                },
                {new Tuple<uint, int>(826, 2), new MyQuestInfoRun(new RoundZone(-1276.9, -5532.2, 40))},
                {new Tuple<uint, int>(872, 2), new MyQuestInfoRun(new RoundZone(-210.06, -3324.29, 40))},
                {new Tuple<uint, int>(850, 0), new MyQuestInfoRun(new RoundZone(25.2, -1715.1, 40))},
                {new Tuple<uint, int>(6629, 0), new MyQuestInfoRun(new RoundZone(111, -351.5, 40))},
                {new Tuple<uint, int>(4921, 0), new MyQuestInfoRun(new RoundZone(-1789.50, -2376.27, 40))},
                {new Tuple<uint, int>(821, 2), new MyQuestInfoRun(new RoundZone(-2390, -1891, 40))},
                {new Tuple<uint, int>(888, 1), new MyQuestInfoRun(new RoundZone(-1715.1, -3820.2, 40))},
                {new Tuple<uint, int>(218, 0), new MyQuestInfoRun(new RoundZone(-6508.9, 300.7, 20))},
                {new Tuple<uint, int>(315, 0), new MyQuestInfoRun(new RoundZone(-5324.91, -234.30, 40))},
                {new Tuple<uint, int>(825, 0), new MyQuestInfoRun(new RoundZone(-185.49, -5277.38, 100))},
                {
                    new Tuple<uint, int>(5481, 0),
                    new MyQuestInfoRun(new RoundZone(2137.6, 631.7, 150), new List<uint> {175566})
                },
                {
                    new Tuple<uint, int>(5482, 0),
                    new MyQuestInfoRun(new RoundZone(2667.7, 475.2, 150), new List<uint> {176753})
                },
                {new Tuple<uint, int>(408, 2), new MyQuestInfoRun(new RoundZone(3044.6, 658.7, 15))},
            };
    }
}