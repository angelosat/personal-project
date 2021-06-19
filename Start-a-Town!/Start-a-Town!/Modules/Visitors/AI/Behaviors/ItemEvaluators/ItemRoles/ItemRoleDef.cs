using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    abstract class ItemRoleDef : Def
    {
        protected ItemRoleDef(string name) : base($"ItemRole{name}")
        {
        }
    }
}
