using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class CraftingComponent : Component
    {
        public int Level;
        public List<int> KnownPlans;

        public CraftingComponent(int[] plans)
        {
            KnownPlans = new List<int>(plans);
        }
        public CraftingComponent(List<int> plans)
        {
            KnownPlans = new List<int>(plans);
        }

        public override object Clone()
        {
            return new CraftingComponent(KnownPlans);
        }
    }
}
