using System.Collections.Generic;

namespace Start_a_Town_
{
    public interface IItemPreferencesManager : ISaveable, ISerializable
    {
        //Entity GetPreference(GearType gt);
        //Entity GetPreference(ToolAbilityDef toolUse);
        Entity GetPreference(object tag);
        IEnumerable<Entity> GetJunk();
        void RemoveJunk(Entity entity);
        bool AddPreference(Entity item);
        void RemovePreference(ToolAbilityDef toolUse);
        bool IsPreference(Entity item);
    }
}
