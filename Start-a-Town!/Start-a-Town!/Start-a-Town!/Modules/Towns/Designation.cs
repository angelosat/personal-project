using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Towns
{
    class Designation
    {
        public int ID;
        public Town Town;
        public HashSet<Vector3> Positions = new HashSet<Vector3>();
    }
}
