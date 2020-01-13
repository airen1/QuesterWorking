using System;
using System.Collections.Generic;
using WoWBot.Core;

namespace WowAI
{
    internal partial class MyQuestHelpClass
    {
        public readonly Dictionary<Tuple<uint, int>, MyQuestInfoRun> MyQuestInfosRunCustom = new Dictionary<Tuple<uint, int>, MyQuestInfoRun>
            {
               // {new Tuple<uint, int>(433,0), new MyQuestInfoRun(new RoundZone(-5738.9, -1672.9, 100)) },
                {new Tuple<uint, int>(2499,0), new MyQuestInfoRun(new RoundZone(9292.6, 1102.6, 100)) },
                {new Tuple<uint, int>(483,3), new MyQuestInfoRun(new RoundZone(9772.7, 1554, 40)) },
                {new Tuple<uint, int>(483,1), new MyQuestInfoRun(new RoundZone(9753.9, 1588.7, 40)) },
                {new Tuple<uint, int>(483,2), new MyQuestInfoRun(new RoundZone(9712.1, 1538.6, 40)) },
                {new Tuple<uint, int>(483,0), new MyQuestInfoRun(new RoundZone(9882, 1489.7, 40)) },
                {new Tuple<uint, int>(11,0), new MyQuestInfoRun(new RoundZone(-9999.6, 623, 350)) },
                {new Tuple<uint, int>(766,1), new MyQuestInfoRun(new RoundZone(-1924.5, -713.9, 150), new List<uint> {3035}) },
                {new Tuple<uint, int>(418,0), new MyQuestInfoRun(new RoundZone(-5041.7, -2895.3, 350)) },
                {new Tuple<uint, int>(418,2), new MyQuestInfoRun(new RoundZone(-5041.7, -2895.3, 350)) },
                {new Tuple<uint, int>(418,1), new MyQuestInfoRun(new RoundZone(-5041.7, -2895.3, 350)) },
                {new Tuple<uint, int>(771,1), new MyQuestInfoRun(new RoundZone(-2543.3, -700.2, 350)) },
                {new Tuple<uint, int>(92,0), new MyQuestInfoRun(new RoundZone(-9757.4, -2230.5, 250)) },
                {new Tuple<uint, int>(92,2), new MyQuestInfoRun(new RoundZone(-9757.4, -2230.5, 250)) },
                {new Tuple<uint, int>(92,1), new MyQuestInfoRun(new RoundZone(-9694.60, -2640.87, 150)) },
                {new Tuple<uint, int>(89,0), new MyQuestInfoRun(new RoundZone(-9111.37, -2125.46, 90)) },
                {new Tuple<uint, int>(89,1), new MyQuestInfoRun(new RoundZone(-9111.37, -2125.46, 90)) },
             //  {new Tuple<uint, int>(92,1), new MyQuestInfoRun(new RoundZone(-9650.00, -2434.42, 150)) },
                {new Tuple<uint, int>(3741,0), new MyQuestInfoRun(new RoundZone(-9335.18, -2140.43, 150)) },
                {new Tuple<uint, int>(258,0), new MyQuestInfoRun(new RoundZone(-5199.79, -3857.10, 350)) },
                {new Tuple<uint, int>(2541,0), new MyQuestInfoRun(new RoundZone(9881.3, 1488.9, 150), new List<uint> {2009})  },
                {new Tuple<uint, int>(2459,1), new MyQuestInfoRun(new RoundZone(10018.9, 281.7, 150), new List<uint> {7234})  },
                {new Tuple<uint, int>(1485,0), new MyQuestInfoRun(new RoundZone(-212.6, -4354.5, 150), new List<uint> {3101})  },
                {new Tuple<uint, int>(488,0), new MyQuestInfoRun(new RoundZone(9751.3, 569.9, 350)) },
                {new Tuple<uint, int>(488,2), new MyQuestInfoRun(new RoundZone(9751.3, 569.9, 350)) },
                {new Tuple<uint, int>(488,1), new MyQuestInfoRun(new RoundZone(9751.3, 569.9, 350)) },
                {new Tuple<uint, int>(456,0), new MyQuestInfoRun(new RoundZone(10449.3, 965.7, 250)) },
                {new Tuple<uint, int>(456,1), new MyQuestInfoRun(new RoundZone(10449.3, 965.7, 250)) },
                {new Tuple<uint, int>(766,3), new MyQuestInfoRun(new RoundZone(-1924.5, -713.9, 350)) },
                {new Tuple<uint, int>(745,2), new MyQuestInfoRun(new RoundZone(-2751.7, -729.3, 150)) },
                {new Tuple<uint, int>(745,0), new MyQuestInfoRun(new RoundZone(-2751.7, -729.3, 150)) },
                {new Tuple<uint, int>(745,1), new MyQuestInfoRun(new RoundZone(-2751.7, -729.3, 150)) },
                {new Tuple<uint, int>(748,1), new MyQuestInfoRun(new RoundZone(-1924.5, -713.9, 350)) },
                {new Tuple<uint, int>(1525,1), new MyQuestInfoRun(new RoundZone(866, -4745.3, 150), new List<uint> {3199}) },
                {new Tuple<uint, int>(1525,0), new MyQuestInfoRun(new RoundZone(-231.9, -3021.6, 150), new List<uint> {3269, 3268, 3267, 3271})  },

                {new Tuple<uint, int>(2139,0), new MyQuestInfoRun(new RoundZone(6662.7, -430.5, 40)) },
                {new Tuple<uint, int>(1001,0), new MyQuestInfoRun(new RoundZone(5823.90, 681.36, 350)) },
                {new Tuple<uint, int>(1138,0), new MyQuestInfoRun(new RoundZone(7443.38, -108.16, 350)) },
                {new Tuple<uint, int>(1516,0), new MyQuestInfoRun(new RoundZone(-88, -4275.5, 150), new List<uint> {3102}) },
                {new Tuple<uint, int>(87,0), new MyQuestInfoRun(new RoundZone(-9746.8, 88.5, 40)) },
                {new Tuple<uint, int>(1002,0), new MyQuestInfoRun(new RoundZone(7184.2, -125.5, 250)) },
                {new Tuple<uint, int>(4681,0), new MyQuestInfoRun(new RoundZone(6314.5, 854.9, 150), new List<uint> {176189}) },
                {new Tuple<uint, int>(1787,0), new MyQuestInfoRun(new RoundZone(-9123.9, -1020.3, 150), new List<uint> {474}) },
                {new Tuple<uint, int>(3524,0), new MyQuestInfoRun(new RoundZone(6113.7, 559.7, 50), new List<uint> {175207}) },
               // {new Tuple<uint, int>(418,1), new MyQuestInfoRun(new RoundZone(-5041.7, -2895.3, 250)) },
                {new Tuple<uint, int>(371,0), new MyQuestInfoRun(new RoundZone(2146.9, -529.5, 50)) },
                //{new Tuple<uint, int>(307,0), new MyQuestInfoRun(new RoundZone(-4880.6, -2973.1, 50)) },
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