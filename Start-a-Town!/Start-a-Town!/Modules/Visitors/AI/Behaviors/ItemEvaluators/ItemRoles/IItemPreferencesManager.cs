using Start_a_Town_.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public interface IItemPreferencesManager : ISaveable, ISerializable
    {
        //public IEnumerable<(ItemRole role, Entity item)> GetPreferences(Actor actor);
        //public Entity GetPreference(ItemRole role);
        Entity GetPreference(GearType gt);
        Entity GetPreference(ToolAbilityDef toolUse);
        IEnumerable<Entity> GetJunk();
        void RemoveJunk(Entity entity);
        void AddPreferenceTool(Entity tool);
        void RemovePreference(ToolAbilityDef toolUse);
        bool IsPreference(Entity item);
    }
}
