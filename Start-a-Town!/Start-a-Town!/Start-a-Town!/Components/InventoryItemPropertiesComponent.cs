using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class InventoryItemPropertiesComponent : Component
    {
        //public GameObject Instance;
        public ushort ID;
        public float Weight;
        public byte StackMax;
        public bool Mutable;
    }
}
