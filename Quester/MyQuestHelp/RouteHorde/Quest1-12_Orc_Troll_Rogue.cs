﻿using System.Collections.Generic;
using Out.Utility;
using WoWBot.Core;

namespace WowAI
{
    internal partial class MyQuestHelpClass
    {
        public List<MyQuest> Quest112OrcTrollRogue = new List<MyQuest>
        {
            new MyQuest(4641, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(4641, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(788, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(788, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(790, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(790, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(790, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(804, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(788, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(788, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(804, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(789, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(3065, QuestAction.Apply, ERace.Troll, EClass.Warrior),
            new MyQuest(2383, QuestAction.Apply, ERace.Orc, EClass.Warrior),
            new MyQuest(3082, QuestAction.Apply, ERace.Troll, EClass.Hunter),
            new MyQuest(3087, QuestAction.Apply, ERace.Orc, EClass.Hunter),
            new MyQuest(3084, QuestAction.Apply, ERace.Troll, EClass.Shaman),
            new MyQuest(3089, QuestAction.Apply, ERace.Orc, EClass.Shaman),
            new MyQuest(3086, QuestAction.Apply, ERace.Troll, EClass.Mage),
            new MyQuest(3083, QuestAction.Apply, ERace.Troll, EClass.Rogue),
            new MyQuest(3088, QuestAction.Apply, ERace.Orc, EClass.Rogue),
            new MyQuest(3090, QuestAction.Apply, ERace.Orc, EClass.Warlock),
            new MyQuest(3085, QuestAction.Apply, ERace.Troll, EClass.Priest),
            new MyQuest(3065, QuestAction.Complete, ERace.Troll, EClass.Warrior),
            new MyQuest(2383, QuestAction.Complete, ERace.Orc, EClass.Warrior),
            new MyQuest(3082, QuestAction.Complete, ERace.Troll, EClass.Hunter),
            new MyQuest(3087, QuestAction.Complete, ERace.Orc, EClass.Hunter),
            new MyQuest(3084, QuestAction.Complete, ERace.Troll, EClass.Shaman),
            new MyQuest(3089, QuestAction.Complete, ERace.Orc, EClass.Shaman),
            new MyQuest(3086, QuestAction.Complete, ERace.Troll, EClass.Mage),
            new MyQuest(3083, QuestAction.Complete, ERace.Troll, EClass.Rogue),
            new MyQuest(3088, QuestAction.Complete, ERace.Orc, EClass.Rogue),
            new MyQuest(3090, QuestAction.Complete, ERace.Orc, EClass.Warlock),
            new MyQuest(3085, QuestAction.Complete, ERace.Troll, EClass.Priest),
            new MyQuest(792, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(4402, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(5441, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(792, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(4402, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(5441, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(789, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(4402, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(789, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(792, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(794, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(5441, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(6394, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(6394, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(794, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(QuestAction.UseHs, 794, new Vector3F(-58.5, -4215.8, 0)),
            new MyQuest(794, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(805, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(6394, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(786, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(805, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(826, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(823, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(808, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(818, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(817, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(2161, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(QuestAction.Grind, 7, new RoundZone(-129.9, -4724.7, 150)),
            new MyQuest(823, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(806, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(784, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(837, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(2161, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(QuestAction.SetHs, 784, new Vector3F(340.6, -4686.4, 0)),
            new MyQuest(815, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(791, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(QuestAction.Grind, 9,
                new PolygonZone(new List<ZonePoint>
                {
                    new ZonePoint(-96.51, -4987.63), new ZonePoint(-144.70, -4776.05), new ZonePoint(-299.72, -4828.62),
                    new ZonePoint(-251.61, -4984.43), new ZonePoint(-281.36, -5059.17), new ZonePoint(14.47, -5089.09),
                    new ZonePoint(1.52, -5011.84)
                })),
            new MyQuest(784, QuestAction.Run, ERace.None, EClass.None, 2),
            new MyQuest(830, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(784, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(784, QuestAction.Run, ERace.None, EClass.None, 1),
            new MyQuest(791, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(784, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(830, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(825, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(831, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(791, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(825, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(818, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(818, QuestAction.Run, ERace.None, EClass.None, 1),
            new MyQuest(837, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(837, QuestAction.Run, ERace.None, EClass.None, 1),
            new MyQuest(QuestAction.Grind, 8, new RoundZone(71.2, -4597.8, 150)),
            new MyQuest(825, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(786, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(786, QuestAction.Run, ERace.None, EClass.None, 1),
            new MyQuest(786, QuestAction.Run, ERace.None, EClass.None, 2),
            new MyQuest(786, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(818, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(826, QuestAction.Run, ERace.None, EClass.None, 2),
            new MyQuest(808, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(817, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(815, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(826, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(826, QuestAction.Run, ERace.None, EClass.None, 1),
            new MyQuest(808, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(826, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(817, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(QuestAction.UseHs, 815, new Vector3F(-797, -4920.7, 0)),
            new MyQuest(815, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(837, QuestAction.Run, ERace.None, EClass.None, 2),
            new MyQuest(837, QuestAction.Run, ERace.None, EClass.None, 3),
            new MyQuest(816, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(834, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(834, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(834, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(835, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(QuestAction.Grind, 10, new RoundZone(275.2, -4708.9, 150)),
            new MyQuest(837, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(1859, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(812, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(5726, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(831, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(1859, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(1963, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(813, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(835, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(835, QuestAction.Run, ERace.None, EClass.None, 1),
            new MyQuest(835, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(816, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(QuestAction.Grind, 13, new RoundZone(1184.83, -4068.77, 150)),
            new MyQuest(806, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(816, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(806, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(828, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(828, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(827, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(5726, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(827, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(827, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(829, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(813, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(5726, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(5727, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(813, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(829, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(809, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(5727, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(5727, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(QuestAction.Grind, 13, new RoundZone(338.5, -3938.1, 150)),
            new MyQuest(840, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(840, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(842, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(809, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(924, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(1859, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(812, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(5726, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(831, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(1859, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(1963, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(813, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(1963, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(6365, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(QuestAction.SetHs, 869, new Vector3F(-407.3, -2644.7, 0)),
            new MyQuest(869, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(871, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(5041, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(867, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(6365, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(6384, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(6384, QuestAction.UseFlyPath, "Orgrimmar, Durotar", new Vector3F(-437.9, -2595.8, 0)),
            new MyQuest(1963, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(1858, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(6384, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(6385, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(1858, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(1858, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(6385, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(6386, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(6386, QuestAction.UseFlyPath, "Crossroads, The Barrens", new Vector3F(1675.9, -4313.3, 0)),
            new MyQuest(6386, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(848, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(1492, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(842, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(844, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(870, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(819, QuestAction.Apply, ERace.None, EClass.None),
            new MyQuest(871, QuestAction.Run, ERace.None, EClass.None, 0),
            new MyQuest(871, QuestAction.Run, ERace.None, EClass.None, 2),
            new MyQuest(871, QuestAction.Run, ERace.None, EClass.None, 1),
            new MyQuest(871, QuestAction.Complete, ERace.None, EClass.None),
            new MyQuest(872, QuestAction.Apply, ERace.None, EClass.None),
        };
    }
}