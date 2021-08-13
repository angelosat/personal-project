using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Blocks;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public abstract class BlockEntityComp : Inspectable, IBlockEntityComp, ISerializable
    {
        public BlockEntity Parent;
        public MapBase Map => this.Parent.Map;
        public IntVec3 Global => this.Parent.OriginGlobal;
        public virtual void OnSpawned(BlockEntity entity, MapBase map, IntVec3 global) { }
        public virtual void Draw(Camera camera, MapBase map, IntVec3 global) { }
        public virtual void DrawUI(SpriteBatch sb, Camera camera) { }
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

        public virtual void Tick() { }
        //public virtual void Tick(MapBase map, IBlockEntityCompContainer entity) { }

        internal virtual void DrawSelected(MySpriteBatch sb, Camera cam, MapBase map, IntVec3 global)
        {
           
        }

        internal virtual void OnDrop(GameObject actor, GameObject item, TargetArgs target, int quantity) { }
        internal virtual void Remove(MapBase map, IntVec3 global, BlockEntity parent) { }

        internal virtual void GetQuickButtons(SelectionManager uISelectedInfo, MapBase map, IntVec3 vector3) { }

        internal virtual void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
            var list = new ListBoxObservable<string, GroupBox>(this.Errors, e => new GroupBox().AddControlsLineWrap(UI.Label.ParseNewNew(e)));// new UI.Label(e));
            info.AddInfo(list);
        }

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

        internal virtual void IsMadeFrom(ItemMaterialAmount[] itemDefMaterialAmounts)
        {
        }

        internal virtual void Deconstruct(GameObject actor, IntVec3 global)
        {
        }

        internal virtual void NeighborChanged()
        {
        }
        protected ObservableCollection<string> Errors = new();
        //internal virtual IEnumerable<string> GetErrorMessages()
        //{
        //    yield break;
        //}
    }
}
