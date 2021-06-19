using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    public class ComponentCollection : Dictionary<string, EntityComponent>//SortedDictionary<string, Component>
    {
        //Hashtable Collection;
        //public Component this[Component.Types type]
        //{ get { return Collection[type]; } }

        //public Component this[string name]
        //{ get { return Collection[name]; } }

        //SortedDictionary<string, Component> Collection;
        //public CompomentCollection()
        //{
        //    //Collection = new Hashtable();
        //    Collection = new SortedDictionary<string, Component>(); 
        //    }
        //public void Add(Component component)
        //{ Collection[component.Type] = component; }
        //public void Remove(Component component)
        //{ Collection.Remove(component.Type); }
        //public void Remove(string name)
        //{ Collection.Remove(name); }


        public void Update(GameObject entity)
        {
            foreach (var component in this.Values)
                component.Tick(entity);
        }
        //public void RandomBlockUpdate(IObjectProvider net, GameObject parent)
        //{
        //    foreach (Component component in Values)
        //        component.RandomBlockUpdate(net, parent);
        //}


        public T GetComponent<T>(string name) where T : EntityComponent
        {
            //return (T)Collection[name];
            return (T)this[name];
        }

    }

}
