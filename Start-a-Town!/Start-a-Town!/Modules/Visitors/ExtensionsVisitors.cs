using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    static class ExtensionsVisitors
    {
        

        internal static VisitorProperties GetVisitorProperties(this Actor actor)
        {
            return actor.Map.World.Population.GetVisitorProperties(actor);
        }
    }
}
