using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Tokens;

namespace Start_a_Town_
{
    public interface IWorkplace
    {
        List<CraftOrderNew> Orders { get; }
        IsWorkstation.Types WorkstationType { get; }
        JobDef Labor { get; }
        //public override AILabor Labor { get { return AILabor.Carpenter; } }
    }
}
