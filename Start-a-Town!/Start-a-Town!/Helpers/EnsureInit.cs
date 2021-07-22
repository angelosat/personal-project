using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Start_a_Town_
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EnsureInit : Attribute
    {
    }
    public static class EnsureInitHelper
    { 
        static Type[] CachedTypes;
        public static void Init()
        {
            CachedTypes = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var tt in CachedTypes.Where(t => t.GetCustomAttribute<EnsureInit>() is not null))
                RuntimeHelpers.RunClassConstructor(tt.TypeHandle);
        }
    }
}
