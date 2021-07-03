using System.Collections.Generic;

namespace Start_a_Town_.Components
{
    public class ComponentCollection : Dictionary<string, EntityComponent>
    {
        public void Update(GameObject entity)
        {
            foreach (var component in this.Values)
                component.Tick(entity);
        }
        
        public T GetComponent<T>(string name) where T : EntityComponent
        {
            return (T)this[name];
        }
    }
}
