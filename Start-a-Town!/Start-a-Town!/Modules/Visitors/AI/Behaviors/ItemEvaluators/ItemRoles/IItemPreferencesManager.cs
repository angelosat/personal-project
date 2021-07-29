﻿using System.Collections.Generic;

namespace Start_a_Town_
{
    public interface IItemPreferencesManager : ISaveable, ISerializable
    {
        Entity GetPreference(GearType gt);
        Entity GetPreference(ToolAbilityDef toolUse);
        IEnumerable<Entity> GetJunk();
        void RemoveJunk(Entity entity);
        void AddPreferenceTool(Entity tool);
        bool AddPreference(Entity item);
        void RemovePreference(ToolAbilityDef toolUse);
        bool IsPreference(Entity item);
    }
}
