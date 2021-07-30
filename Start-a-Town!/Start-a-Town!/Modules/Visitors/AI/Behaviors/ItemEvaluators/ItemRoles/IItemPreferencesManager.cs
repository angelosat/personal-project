using System.Collections.Generic;

namespace Start_a_Town_
{
    public interface IItemPreferenceContext
    { }
    public interface IItemPreferencesManager : ISaveable, ISerializable
    {
        Entity GetPreference(IItemPreferenceContext tag);
        IEnumerable<Entity> GetJunk();
        void RemoveJunk(Entity entity);
        bool AddPreference(Entity item);
        void RemovePreference(IItemPreferenceContext tag);
        bool IsPreference(Entity item);
    }
}
