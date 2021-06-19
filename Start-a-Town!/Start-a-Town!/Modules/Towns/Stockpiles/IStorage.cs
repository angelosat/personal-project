using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public interface IStorage
    {
        IMap Map { get; }
        int ID { get; }
        StorageSettings Settings { get; }
        //bool Accepts(int itemType);
        bool Accepts(Entity item);
        Dictionary<TargetArgs, int> GetPotentialHaulTargets(GameObject actor, GameObject item, out int maxamount);
        IEnumerable<TargetArgs> GetPotentialHaulTargets(GameObject actor, GameObject item);
        TargetArgs GetBestHaulTarget(GameObject actor, GameObject item);
        //bool IsValidStorage(int itemType, TargetArgs target, int quantity);
        bool IsValidStorage(Entity item, TargetArgs target, int quantity);

    }
}
