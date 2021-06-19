﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Stats
{
    class StatBuilding : Stat
    {
        public StatBuilding()
            : base(Types.Building, "Building", BonusType.Percentile, defaultValue: 1)
        {

        }
        public override Stat Clone()
        {
            return new StatBuilding();
        }
    }
}
