using Start_a_Town_.UI;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public interface IItemPreferenceContext
    { }
    public interface IItemPreferencesManager : ISaveable, ISerializable
    {
        Entity GetPreference(IItemPreferenceContext context);
        Entity GetPreference(IItemPreferenceContext context, out int score);
        IEnumerable<Entity> GetJunk();
        void RemoveJunk(Entity entity);
        bool AddPreference(Entity item);
        void AddPreference(IItemPreferenceContext context, Entity item, int score);
        void RemovePreference(IItemPreferenceContext context);
        bool IsPreference(Entity item);
        Control Gui { get; }
        void ResolveReferences();
        int GetScore(IItemPreferenceContext context, Entity item);
    }
}
