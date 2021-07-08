using System.Collections.Generic;

namespace Start_a_Town_
{
    class ReferenceManager
    {
        static Dictionary<int, IReferencable> References = new Dictionary<int, IReferencable>();

        static void Save(IReferencable obj)
        {
            var key = obj.GetUniqueID();
            if (!References.ContainsKey(key))
                References[key] = obj;
        }
    }
}
