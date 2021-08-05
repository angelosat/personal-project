using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_.Blocks
{
    public abstract class BlockEntity : IDisposable, IEntityCompContainer<BlockEntityComp>//, IHasChildren
    {
        public HashSet<IntVec3> CellsOccupied = new();
        public MapBase Map;
        public bool Exists => this.Map is not null;
        public IntVec3 OriginGlobal;
        readonly BlockEntityCompCollection<BlockEntityComp> Comps = new();

        public BlockEntity(IntVec3 originGlobal)
        {
            this.OriginGlobal = originGlobal;
        }

        public virtual void Tick(MapBase map, IntVec3 global)
        {
            foreach (var comp in this.Comps)
                comp.Tick(this, map, global);
        }
        public virtual void GetTooltip(Control tooltip) { }

        /// <summary>
        /// Dipose any children GameObjects here.
        /// </summary>
        public virtual void Dispose() { } // maybe make this abstract so i don't forget it?
        public virtual void OnRemoved(MapBase map, IntVec3 global)
        {
            foreach (var c in this.Comps)
                c.Remove(map, global, this);
        }
        public virtual void Break(MapBase map, IntVec3 global) { }
        public virtual void Place(MapBase map, IntVec3 global)
        {
            foreach (var comp in this.Comps)
                comp.OnEntitySpawn(this, map, global);
        }

        public virtual GameObjectSlot GetChild(string containerName, int slotID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert to void return and accept the list as an argument so derived objects can add their children and then call the base method so the base class can add its own?
        /// </summary>
        /// <returns></returns>
        public virtual List<GameObjectSlot> GetChildren() { return new List<GameObjectSlot>(); }

        public bool HasComp<T>() where T : class, IBlockEntityComp
        {
            return this.GetComp<T>() != null;
        }
        public BlockEntity AddComp(BlockEntityComp comp)
        {
            comp.Parent = this;
            this.Comps.Add(comp);
            return this;
        }

        internal void OnDrop(GameObject actor, GameObject item, TargetArgs target, int quantity)
        {
            foreach (var comp in this.Comps)
                comp.OnDrop(actor, item, target, quantity);
        }

        internal void IsMadeFrom(ItemDefMaterialAmount[] itemDefMaterialAmounts)
        {
            foreach (var c in this.Comps)
                c.IsMadeFrom(itemDefMaterialAmounts);
        }

        internal virtual void Deconstruct(GameObject actor, IntVec3 global)
        {
            foreach (var c in this.Comps)
                c.Deconstruct(actor, global);
        }

        protected virtual void AddSaveData(SaveTag tag)
        {
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.CellsOccupied.Save(tag, "CellsOccupied");
            this.OriginGlobal.Save(tag, "OriginGlobal");
            tag.Add(this.Comps.Save("Components"));
            this.AddSaveData(tag);
            return tag;
        }

        public void Load(SaveTag tag)
        {
            tag.TryGetTag("Components", this.Comps.Load);
            tag.TryGetTagValue<Vector3>("OriginGlobal", v => this.OriginGlobal = v);
            this.CellsOccupied.Load(tag, "CellsOccupied");
            this.LoadExtra(tag);
        }
        protected virtual void LoadExtra(SaveTag tag) { }
        public T GetComp<T>() where T : class, IBlockEntityComp
        {
            return this.Comps.FirstOrDefault(c => c is T) as T;
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.OriginGlobal);
            this.CellsOccupied.Write(w);

            foreach (var c in this.Comps)
                c.Write(w);
            this.WriteExtra(w);
        }
        public void Read(BinaryReader r)
        {
            this.OriginGlobal = r.ReadIntVec3();
            this.CellsOccupied.Read(r);
            foreach (var c in this.Comps)
                c.Read(r);
            this.ReadExtra(r);
        }
        protected virtual void WriteExtra(BinaryWriter w) { }
        protected virtual void ReadExtra(BinaryReader r) { }
        internal virtual void HandleRemoteCall(MapBase map, Vector3 vector3, ObjectEventArgs e) { }

        public virtual void Instantiate(Vector3 global, Action<GameObject> instantiator)
        {
            foreach (var entity in this.GetChildren())
            {
                entity.Instantiate(instantiator);
            }
        }

        public virtual void Draw(Camera camera, MapBase map, IntVec3 global)
        {
            foreach (var comp in this.Comps)
                comp.Draw(camera, map, global);
        }
        public virtual void DrawUI(SpriteBatch sb, Camera cam, IntVec3 global) { }

        internal virtual void GetQuickButtons(SelectionManager uISelectedInfo, MapBase map, IntVec3 vector3)
        {
            foreach (var c in this.Comps)
                c.GetQuickButtons(uISelectedInfo, map, vector3);
        }

        internal virtual void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
            foreach (var c in this.Comps)
                c.GetSelectionInfo(info, map, vector3);
        }

        internal void OnBlockBelowChanged(MapBase map, IntVec3 global)
        {
            foreach (var c in this.Comps)
                c.OnBlockBelowChanged(map, global);
        }

        internal void MapLoaded(MapBase map, IntVec3 global)
        {
            foreach (var c in this.Comps)
                c.MapLoaded(map, global);
            this.OnMapLoaded(map, global);
        }

        protected virtual void OnMapLoaded(MapBase map, IntVec3 global)
        {
        }

        internal void DrawSelected(MySpriteBatch sb, Camera cam, MapBase map, IntVec3 global)
        {
            foreach (var c in this.Comps)
                c.DrawSelected(sb, cam, map, global);
        }
    }
}
