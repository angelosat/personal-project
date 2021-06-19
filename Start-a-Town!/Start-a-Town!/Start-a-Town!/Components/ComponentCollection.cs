using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    public class ComponentCollection : SortedDictionary<string, Component>
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

        public void Update(Net.IObjectProvider net, GameObject entity, Chunk chunk = null)
        {
            //foreach (KeyValuePair<string, Component> component in Collection)
            //    component.Value.Update(entity);
            foreach (Component component in Values)
                component.Update(net, entity, chunk);
        }
        public void Update(GameObject entity)
        {
            //foreach (KeyValuePair<string, Component> component in Collection)
            //    component.Value.Update(entity);
            foreach (Component component in Values)
                component.Update(entity);
        }
        //public void RandomBlockUpdate(Net.IObjectProvider net, GameObject parent)
        //{
        //    foreach (Component component in Values)
        //        component.RandomBlockUpdate(net, parent);
        //}


        public T GetComponent<T>(string name) where T : Component
        {
            //return (T)Collection[name];
            return (T)this[name];
        }

    }

}
