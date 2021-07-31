using System.Collections.Generic;

namespace Start_a_Town_
{
    public interface IListCollapsibleDataSource : IListable
    {
        IEnumerable<IListCollapsibleDataSource> ListBranches { get; }
        IEnumerable<IListable> ListLeafs { get; }
    }
}
