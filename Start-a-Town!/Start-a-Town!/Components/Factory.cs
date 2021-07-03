using System;
using System.Collections.Generic;

namespace Start_a_Town_.Components
{
    class Factory
    {
        static Dictionary<string, EntityComponent> _Registry;
        static Dictionary<string, EntityComponent> Registry
        {
            get
            {
                if (_Registry is null)
                
                    Initialize();
                return _Registry;
            }
        }
        static void Initialize()
        {
            _Registry = new Dictionary<string, EntityComponent>();
            Register(
                new PositionComponent()
                );
        }

        static public void Register(params EntityComponent[] components)
        {
            foreach (var comp in components)
            {
                if (string.IsNullOrWhiteSpace(comp.ComponentName))
                    throw new ArgumentException();
                
                if (!Registry.ContainsKey(comp.ComponentName))
                    Registry.Add(comp.ComponentName, comp);
            }
        }

        static public EntityComponent Create(string componentName)
        {
            EntityComponent comp;
            if (!Registry.TryGetValue(componentName, out comp))
                throw new ArgumentException("Invalid component name");
            return comp.Clone() as EntityComponent;
        }
    }
}
