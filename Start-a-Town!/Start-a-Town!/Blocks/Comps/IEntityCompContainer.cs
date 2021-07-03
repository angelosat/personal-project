using System.Collections.Generic;

namespace Start_a_Town_
{
    public interface IEntityCompContainer
    {
        ICollection<IEntityComp> Comps { get; }
        T GetComp<T>() where T : class, IEntityComp;
        bool HasComp<T>() where T : class, IEntityComp;
    }
    public interface IEntityCompContainer<U> where U : class, IEntityComp
    {
        ICollection<U> Comps { get; }
        T GetComp<T>() where T : class, IEntityComp;
        bool HasComp<T>() where T : class, IEntityComp;
    }
}
