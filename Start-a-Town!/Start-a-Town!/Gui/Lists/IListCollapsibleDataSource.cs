using System.Collections.Generic;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public interface IListCollapsibleDataSource : IListable
    {
        IEnumerable<IListCollapsibleDataSource> ListBranches { get; }
        IEnumerable<IListable> ListLeafs { get; }
        Control GetGui();
    }
}
