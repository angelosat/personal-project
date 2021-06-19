using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI
{
    class TraitAttention : Trait
    {
        public override Trait.Types Type
        {
            get { return Trait.Types.Attention; }
        }
        public override string Name
        {
            get { return "Attention"; }
        }
        //public TraitAttention()
        //{
        //    this.Value = -40;
        //}
    }
}
