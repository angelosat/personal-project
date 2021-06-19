using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public interface IEntityCompContainer
    {
        //IMap Map { get; }
        //Vector3 Global { get; }
        //EntityCompCollection Comps { get; }
        ICollection<IEntityComp> Comps { get; }
        T GetComp<T>() where T : class, IEntityComp;
        bool HasComp<T>() where T : class, IEntityComp;
    }
    public interface IEntityCompContainer<U> where U : class, IEntityComp
    {
        //IMap Map { get; }
        //Vector3 Global { get; }
        //EntityCompCollection Comps { get; }
        ICollection<U> Comps { get; }
        T GetComp<T>() where T : class, IEntityComp;
        bool HasComp<T>() where T : class, IEntityComp;
    }
}
