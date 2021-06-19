using Start_a_Town_.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class ItemManager : IItemPreferencesManager
    {
        class ItemPreference
        {
            Entity Item;
            bool Keep;
            JobDef Job;
            GearType Gear;
        }

        public IEnumerable<Entity> GetJunk()
        {
            throw new NotImplementedException();
        }

        public Entity GetPreference(GearType gt)
        {
            throw new NotImplementedException();
        }

        public void RemoveJunk(Entity entity)
        {
            throw new NotImplementedException();
        }

        public Entity GetPreference(ToolAbilityDef toolUse)
        {
            throw new NotImplementedException();
        }

        public void AddPreferenceTool(Entity tool)
        {
            throw new NotImplementedException();
        }

        public void RemovePreference(ToolAbilityDef toolUse)
        {
            throw new NotImplementedException();
        }

        public bool IsPreference(Entity item)
        {
            throw new NotImplementedException();
        }
    }
}
