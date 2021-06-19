using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Particles
{
    interface IVelocityGetter : ICloneable
    {
        Vector3 Get();
    }
}
