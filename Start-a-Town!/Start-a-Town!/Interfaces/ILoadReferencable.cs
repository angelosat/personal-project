using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public interface ILoadReferencable<T> : ISaveable where T : new()
    {
        string GetUniqueLoadID();
    }
    public interface ILoadReferencable : ISaveable
    {
        string GetUniqueLoadID();
    }
}
