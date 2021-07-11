﻿using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public abstract class BlockEntityComp : IEntityComp, ISerializable
    {
        public virtual void OnEntitySpawn(BlockEntity entity, MapBase map, Vector3 global) { }
        public virtual void Tick(IObjectProvider net, BlockEntity entity, Vector3 global) { }
        public virtual void Draw(Camera camera, MapBase map, Vector3 global) { }
        public virtual void Load(SaveTag tag)
        {
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.AddSaveData(tag);
            return tag;
        }
        public virtual void AddSaveData(SaveTag tag)
        {
        }

        public virtual void Tick(IObjectProvider net, IEntityCompContainer entity)
        {
        }

        internal virtual void DrawSelected(MySpriteBatch sb, Camera cam, MapBase map, IntVec3 global)
        {
           
        }

        internal virtual void OnDrop(GameObject actor, GameObject item, TargetArgs target, int quantity) { }
        internal virtual void Remove(MapBase map, IntVec3 global, BlockEntity parent) { }

        internal virtual void GetQuickButtons(UISelectedInfo uISelectedInfo, MapBase map, IntVec3 vector3) { }

        internal virtual void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3) { }

        public virtual void Write(BinaryWriter w)
        {
        }

        public virtual ISerializable Read(BinaryReader r)
        {
            return this;
        }

        internal virtual void OnBlockBelowChanged(MapBase map, IntVec3 global)
        {
        }

        internal virtual void MapLoaded(MapBase map, IntVec3 global)
        {
        }

        internal virtual void IsMadeFrom(ItemDefMaterialAmount[] itemDefMaterialAmounts)
        {
        }

        internal virtual void Deconstruct(GameObject actor, IntVec3 global)
        {
        }
    }
}
