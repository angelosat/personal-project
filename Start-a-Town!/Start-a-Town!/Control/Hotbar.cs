using System;

namespace Start_a_Town_.PlayerControl
{
    public class HotbarEventArgs : EventArgs
    {
        public int SlotID;
        public HotbarEventArgs(int slotID)
        {
            SlotID = slotID;
        }
    }

    public class Hotbar
    {
        public GameObjectSlot[] Slots;

        public GameObjectSlot this[int i]
        {
            get { return Slots[i]; }
            set
            {
                Slots[i] = value;
                OnHotbarChanged(new HotbarEventArgs(i));
            }
        }

        public Hotbar()
        {
            Slots = new GameObjectSlot[10];
        }

        public event EventHandler<HotbarEventArgs> HotbarChanged;
        void OnHotbarChanged(HotbarEventArgs e)
        {
            if (HotbarChanged != null)
                HotbarChanged(this, e);
        }
    }
}
