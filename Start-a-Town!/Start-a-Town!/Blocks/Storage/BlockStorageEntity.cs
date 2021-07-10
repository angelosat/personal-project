using System;
using System.Collections.Generic;
using Start_a_Town_.Blocks;
using Microsoft.Xna.Framework;
using System.IO;

namespace Start_a_Town_
{
    partial class BlockStorage : Block
    {
        public class BlockStorageEntity : BlockEntity, IStorage
        {
            Vector3 Global;
            readonly int Capacity = 4;
            StorageSettings Settings = new();
            public List<GameObject> Contents = new(4);
            public MapBase Map { get; }
            public int ID { get; }
            StorageSettings IStorage.Settings => this.Settings;
            public bool IsFull => this.Contents.Count == Capacity;

            public bool Accepts(Entity item)
            {
                throw new NotImplementedException();
            }

            public override object Clone()
            {
                return new BlockStorageEntity();
            }

            public Dictionary<TargetArgs, int> GetPotentialHaulTargets(GameObject actor, GameObject item, out int maxamount)
            {
                var dic = new Dictionary<TargetArgs, int>();
                maxamount = item.StackMax;
                if (this.Contents.Count < 4)
                    dic.Add(new TargetArgs(actor.Map, this.Global), maxamount);
                return dic;
            }

            public bool IsValidStorage(Entity item, TargetArgs target, int quantity)
            {
                if (this.Contents.Count == this.Capacity)
                    return false;
                return this.Accepts(item);
            }
            public void Insert(GameObject obj)
            {
                if (this.IsFull)
                    throw new Exception();
                this.Contents.Add(obj);
                obj.Global = this.Global;
                obj.Net.EventOccured(Components.Message.Types.ContentsChanged, this);
            }
            internal void Remove(GameObject item)
            {
                this.Contents.Remove(item);
                item.Net.EventOccured(Components.Message.Types.ContentsChanged, this);
            }
            public override void Place(MapBase map, Vector3 global)
            {
                base.Place(map, global);
                this.Global = global;
            }
            public override void OnRemove(MapBase map, Vector3 global)
            {
                foreach (var i in this.Contents)
                    map.Net.PopLoot(i, global, Vector3.Zero);
            }
            
            protected override void AddSaveData(SaveTag save)
            {
                save.Add(this.Global.SaveOld("Global"));
                var contents = new SaveTag(SaveTag.Types.List, "Contents", SaveTag.Types.Compound);
                foreach (var obj in this.Contents)
                    contents.Add(new SaveTag(SaveTag.Types.Compound, "", obj.SaveInternal()));
                save.Add(contents);
            }

            protected override void LoadExtra(SaveTag tag)
            {
                this.Global = tag["Global"].LoadVector3();
                var contents = tag["Contents"].Value as List<SaveTag>;
                foreach (var i in contents)
                {
                    var obj = GameObject.Load(i);
                    this.Contents.Add(obj);
                    obj.Global = this.Global;
                }
            }
            public override void Instantiate(Vector3 global, Action<GameObject> instantiator)
            {
                foreach (var i in this.Contents)
                    i.Instantiate(instantiator);
            }
            protected override void WriteExtra(BinaryWriter w)
            {
                w.Write(this.Global);
                w.Write(this.Contents.Count);
                foreach (var i in this.Contents)
                {
                    i.Write(w);
                }
            }
            protected override void ReadExtra(BinaryReader r)
            {
                this.Global = r.ReadVector3();
                var count = r.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                var obj = GameObject.CreatePrefab(r);
                this.Contents.Add(obj);
                }
            }

            public TargetArgs GetBestHaulTarget(GameObject actor, GameObject item)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<TargetArgs> GetPotentialHaulTargets(GameObject actor, GameObject item)
            {
                throw new NotImplementedException();
            }
        }
    }
}
