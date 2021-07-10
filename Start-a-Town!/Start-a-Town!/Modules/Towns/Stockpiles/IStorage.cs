using System.Collections.Generic;

namespace Start_a_Town_
{
    public interface IStorage
    {
        MapBase Map { get; }
        int ID { get; }
        StorageSettings Settings { get; }
        bool Accepts(Entity item);
        Dictionary<TargetArgs, int> GetPotentialHaulTargets(GameObject actor, GameObject item, out int maxamount);
        IEnumerable<TargetArgs> GetPotentialHaulTargets(GameObject actor, GameObject item);
        TargetArgs GetBestHaulTarget(GameObject actor, GameObject item);
        bool IsValidStorage(Entity item, TargetArgs target, int quantity);
    }
}
