using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class EntityCompCollection : List<IEntityComp>
    {
        public virtual SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            if (!this.Any())
                return tag;
            foreach (var c in this)
                tag.Add(c.Save(c.GetType().FullName));
            return tag;
            //return new SaveTag(SaveTag.Types.Compound, name);
        }
        public virtual void Load(SaveTag tag)
        {
            tag.TryGetTag("Components", t => {
                foreach (var c in this)
                    //t.TryGetTagValue<SaveTag>(c.GetType().FullName, ct => c.Load(ct));
                    t.TryGetTagValue<SaveTag>(c.GetType().FullName, ct => c.Load(ct));
            });
        }
    }

    public class EntityCompCollection<T> : List<T> where T : IEntityComp
    {
        public virtual SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            if (!this.Any())
                return tag;
            foreach (var c in this)
                tag.Add(c.Save(c.GetType().FullName));
            return tag;
            //return new SaveTag(SaveTag.Types.Compound, name);
        }
        public virtual void Load(SaveTag tag)
        {
            //tag.TryGetTag("Components", t => {
            //    foreach (var c in this)
            //        t.TryGetTagValue<SaveTag>(c.GetType().FullName, ct => c.Load(ct));
            //});

            foreach (var c in this)
                tag.TryGetTag(c.GetType().FullName, ct => c.Load(ct));
            //tag.TryGetTagValue<SaveTag>(c.GetType().FullName, ct => c.Load(ct));
        }
    }
}
