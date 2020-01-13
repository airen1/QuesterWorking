using WoWBot.Core;

namespace WowAI
{
    internal partial class Host
    {
        public uint MyTotalInvSlot()
        {
            uint result = 16;
            foreach (var item in ItemManager.GetItems())
            {
                if (item.Place == EItemPlace.InventoryBag && item.ItemClass != EItemClass.Quiver)
                {
                    result += ((Bag)item).BagSize;
                }
            }

            return result;
        }

        public uint MyFreeInvSlots()
        {
            uint result = 0;
            var quiverWowGuid = WowGuid.Zero;
            foreach (var item in ItemManager.GetItems())
            {
                if (item.Place == EItemPlace.InventoryBag && item.ItemClass == EItemClass.Quiver)
                {
                    quiverWowGuid = item.Guid;
                }
            }

            foreach (var item in ItemManager.GetItems())
            {
                if (item.Place != EItemPlace.Bag1 && item.Place != EItemPlace.Bag2 && item.Place != EItemPlace.Bag3 &&
                    item.Place != EItemPlace.Bag4 && item.Place != EItemPlace.InventoryItem)
                {
                    continue;
                }

                if (item.ContainerGuid == quiverWowGuid)
                {
                    continue;
                }
                //log(item.Name + " " + item.Place + " " + item.ContainerGuid + " " + quiverWowGuid);
                result++;
            }
            return result;
        }

        public uint MyGetFreeSlot()
        {
            return MyTotalInvSlot() - MyFreeInvSlots();
        }
    }
}