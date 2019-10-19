using System;
using WoWBot.Core;


namespace WowAI
{
    internal partial class Host
    {
        public class MyExp
        {
            private long _startExp;
            private int _startLevel;
            private long _allGain;
            public DateTime TimeWork;

            public long CalcExpGain(Player me)
            {
                if (me.Level != _startLevel)
                {
                    _startLevel = me.Level;
                    _allGain = 0;
                    _startExp = me.Exp;
                    TimeWork = DateTime.Now;
                }
                _allGain = me.Exp - _startExp;
                var result = me.Exp - _startExp;
                if (result == 0)
                    result = 1;
                return result;
            }

            public double CalcAverageExp()
            {
                var allTime = DateTime.Now - TimeWork;
                var result = Math.Round(_allGain / allTime.TotalMinutes, 2);
                return result;
            }

            public double CalcMinToLevelUp(Player me)
            {
                var leftExp = me.NextLevelExp - me.Exp;
                return Math.Round(leftExp / CalcAverageExp(), 2);
            }
        }
    }
}