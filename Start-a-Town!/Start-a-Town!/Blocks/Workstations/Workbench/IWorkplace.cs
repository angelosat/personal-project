using System.Collections.Generic;

namespace Start_a_Town_
{
    public interface IWorkplace
    {
        List<CraftOrderNew> Orders { get; }
        IsWorkstation.Types WorkstationType { get; }
        JobDef Labor { get; }
    }
}
