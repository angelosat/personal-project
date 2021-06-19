using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI
{
    class TraitPatience : Trait
    {
        public override Trait.Types Type
        {
            get { return Trait.Types.Patience; }
        }
        public override string Name
        {
            get { return "Patience"; }
        }
        //public TraitPatience()
        //{
        //    this.Value = this.Max;
        //}
    }
}
