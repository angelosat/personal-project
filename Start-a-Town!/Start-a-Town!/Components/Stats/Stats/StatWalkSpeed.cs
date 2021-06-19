using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Stats
{
    class StatWalkSpeed : Stat
    {
        public StatWalkSpeed()
            : base(Types.WalkSpeed, "Movement Speed", BonusType.Percentile, defaultValue: 1)
        {

        }
        public override Stat Clone()
        {
            return new StatWalkSpeed();
        }
    }
}
