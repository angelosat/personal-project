using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class SoilComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "Soil"; }
        }
        public override object Clone()
        {
            return new SoilComponent();
        }

        
    }
}
