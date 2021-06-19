using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class ApparelDef
    {
        public GearType GearType;
        public int ArmorValue;
        public ApparelDef(GearType gearType, int armorValue)
        {
            this.GearType = gearType;
            this.ArmorValue = armorValue;
        }
    }
}
