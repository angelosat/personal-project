using System.Collections.Generic;

namespace Start_a_Town_
{
    public interface IStorageNew
    {
        int StorageID { get; }
        StorageSettings Settings { get; }
        bool Accepts(ItemDef item, MaterialDef material, Def variation);
    }
    public interface IStorage
    {
        MapBase Map { get; }
        int ID { get; }
        StorageSettings Settings { get; }
        bool Accepts(Entity item);
        Dictionary<TargetArgs, int> GetPotentialHaulTargets(Actor actor, GameObject item, out int maxamount);
        IEnumerable<TargetArgs> GetPotentialHaulTargets(Actor actor, GameObject item);
        bool IsValidStorage(Entity item, TargetArgs target, int quantity);
    }
}
