using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class NeedLetDef : Def
    {
        //public NeedDef NeedDef;

        public NeedLetDef(string name
            //, NeedDef needDef
            ):base(name)
        {
            //this.NeedDef = needDef;
        }

        //public NeedLet Create(float value)
        //{
        //    return new NeedLet(this, value);
        //}

        static public void Init()
        {
            Register(NeedLetDefOf.Sleeping);
        }
    }
}
