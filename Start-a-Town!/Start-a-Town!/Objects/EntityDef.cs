using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    public class EntityDef : Def
    {
        public Type ItemClass = typeof(Entity);
        public string Description;
        public float Height = 1;
        public float Weight = 1;
        public bool Haulable = true;
        public Bone Body;
        public EntityDef(string name) : base(name)
        {

        }
        //public GameObject Create()
        //{
        //    return this.Factory(this);

        //    throw new NotFiniteNumberException();
        //    //return ItemFactory.CreateItem(this);
        //}
        public List<ComponentProps> CompProps = new List<ComponentProps>();

        public EntityDef AddCompProp(ComponentProps props)
        {
            this.CompProps.Add(props);
            return this;
        }
        public T AddCompProp<T>(ComponentProps props) where T : ItemDef
        {
            this.CompProps.Add(props);
            return this as T;
        }
    }
}
