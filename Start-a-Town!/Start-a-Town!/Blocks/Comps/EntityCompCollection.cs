using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class EntityCompCollection<T> : List<T> where T : IBlockEntityComp
    {
        public virtual SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            if (!this.Any())
                return tag;
            foreach (var c in this)
                tag.Add(c.Save(c.GetType().FullName));
            return tag;
        }
        public virtual void Load(SaveTag tag)
        {
            foreach (var c in this)
                tag.TryGetTag(c.GetType().FullName, ct => c.Load(ct));
        }
    }
}
