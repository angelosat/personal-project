using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class ReagentFilterCustom : Reaction.Reagent.ReagentFilter
    {
        string _Name;
        Func<GameObject, bool> _Cond;
        public ReagentFilterCustom(string name, Func<GameObject,bool> cond)
        {
            this._Name = name;
            this._Cond = cond;
        }
        public override string Name => _Name;
        public override bool Condition(Entity obj)
        {
            return this._Cond(obj);
        }
    }
}
