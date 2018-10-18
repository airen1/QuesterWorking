using Out.Internal.Core;
using System;
using System.Collections.Generic;
using Out.Utility;


namespace WowAI
{

    [Serializable]

    public class DropBases
    {
        public List<DropBase> Drop = new List<DropBase>();
    }

    public class DropBase
    {
        public uint ItemId;
        public List<uint> MobsId = new List<uint>();
        public string Type;
    }
}