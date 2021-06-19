using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

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
            // REGISTERING COMPONENTS DYNAMICALLY ON THEIR CREATION
            Register(
                new PositionComponent()
                //new GeneralComponent(),
                //new PhysicsComponent(),
                //new InventoryComponent(),
                //new NeedsComponent(),
                //new MemoryComponent(),
                //new ActorSpriteComponent(),
                //new SkillsComponent(),
                //new SpellBookComponent(),
                //new AbilitiesComponent(),
                //new PartyComponent(),
                //new ControlComponent(),
                //new BodyComponent(),
                //new ProductionComponent(),
                //new EquipComponent(),
                //new GuiComponent(),
                //new ItemComponent()
                );
        }

        static public void Register(params EntityComponent[] components)
        {
            foreach (var comp in components)
            {
                if (string.IsNullOrWhiteSpace(comp.ComponentName))
                    throw new ArgumentException();
                //{
                //    //Client.Console.Write(Color.Red, "WARNING! Error registering component " + comp.GetType().ToString());
                //    //Client.Console.Show();
                //    ("WARNING! Error registering component " + comp.GetType().ToString()).ToConsole();
                //    continue;
                //}
                if (!Registry.ContainsKey(comp.ComponentName))
                    Registry.Add(comp.ComponentName, comp);
            }
        }

        static public EntityComponent Create(string componentName)
        {
            EntityComponent comp;
            if (!Registry.TryGetValue(componentName, out comp))
                throw new ArgumentException("Invalid component name");
                //return null;
            return comp.Clone() as EntityComponent;

            //return Registry[componentName].Clone() as Component;
        }
    }
}
