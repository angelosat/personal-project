using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Blocks
{
    //public abstract class BlockEntity : Component, ICloneable
    public abstract class BlockEntity : ICloneable, IDisposable, IEntityCompContainer<BlockEntityComp>//, IHasChildren
    {
        //public List<EntityComp> Comps = new List<EntityComp>();
        //public Vector3 Global;
        public HashSet<IntVec3> CellsOccupied = new();
        public IMap Map;
        public bool Exists => this.Map is not null;
        public IntVec3 OriginGlobal;
        EntityCompCollection<BlockEntityComp> _Comps = new();
        //public EntityCompCollection Comps { get { return this._Comps; } private set { this._Comps = value; } }
        public ICollection<BlockEntityComp> Comps { get {
                return this._Comps;
                //as ICollection<BlockEntityComp>;
            } private set { this._Comps = value as EntityCompCollection<BlockEntityComp>; } }
        
        public virtual void Tick(IObjectProvider net, Vector3 global)
        {
            foreach (var comp in this.Comps)
                comp.Tick(net, this, global);
        }
        public virtual void GetTooltip(UI.Control tooltip) { }
        public abstract object Clone();//{throw new Exception();}
        //internal void Spawn(IMap map, params IntVec3[] cellsOccupied)
        //{
        //    foreach (var g in cellsOccupied)
        //    {
        //        map.AddBlockEntity(g, this);
        //        this.CellsOccupied.Add(g);
        //    }
        //    this.Map = map;
        //}
        //internal void Despawn()
        //{
        //    foreach(var global in this.CellsOccupied)
        //        Map.RemoveBlockEntity(global);
        //    this.CellsOccupied.Clear();
        //    this.Map = null;
        //}
        /// <summary>
        /// Dipose any children GameObjects here.
        /// </summary>
        public virtual void Dispose() { } // maybe make this abstract so i don't forget it?
        public virtual void OnRemove(IMap map, Vector3 global) {
            foreach (var c in this.Comps)
                c.Remove(map, global, this);
        }
        public virtual void Break(IMap map, Vector3 global) { }
        public virtual void Place(IMap map, Vector3 global) 
        {
            //this.Global = global;
            foreach (var comp in this.Comps)
                comp.OnEntitySpawn(this, map, global);
        }

        //public Vector3 Global { get; }
        public virtual GameObjectSlot GetChild(string containerName, int slotID) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert to void return and accept the list as an argument so derived objects can add their children and then call the base method so the base class can add its own?
        /// </summary>
        /// <returns></returns>
        public virtual List<GameObjectSlot> GetChildren() { return new List<GameObjectSlot>(); }
       
        public bool HasComp<T>() where T : class, IEntityComp
        {
            return this.GetComp<T>() != null;
        }
        public BlockEntity AddComp(BlockEntityComp comp)
        {
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

        internal virtual void Deconstruct(GameObject actor, Vector3 global)
        {
            foreach (var c in this.Comps)
                c.Deconstruct(actor, global);
        }

        protected virtual void AddSaveData(SaveTag tag)
        {
            //tag.Add(this._Comps.Save("Components"));
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.CellsOccupied.Save(tag, "CellsOccupied");
            this.OriginGlobal.Save(tag, "OriginGlobal");
            tag.Add(this._Comps.Save("Components"));
            this.AddSaveData(tag);
            return tag;
        }

        public void Load(SaveTag tag)
        {
            //this.Comps.Load(tag["Components"]);
            tag.TryGetTag("Components", this._Comps.Load);
            tag.TryGetTagValue<Vector3>("OriginGlobal", v => this.OriginGlobal = v);
            this.CellsOccupied.Load(tag, "CellsOccupied");
            this.LoadExtra(tag);
        }
        protected virtual void LoadExtra(SaveTag tag) { }
        public T GetComp<T>() where T : class, IEntityComp
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
        internal virtual void HandleRemoteCall(IMap map, Vector3 vector3, ObjectEventArgs e) { }

        public virtual void Instantiate(Vector3 global, Action<GameObject> instantiator)
        {
            foreach (var entity in this.GetChildren())
            {
                entity.Instantiate(instantiator);
                //if (entity.Object != null)
                //    entity.Object.ParentTarget = new TargetArgs(global);
            }
        }

        public virtual void Draw(Camera camera, IMap map, Vector3 global) 
        {
            foreach (var comp in this.Comps)
                comp.Draw(camera, map, global);
        }
        public virtual void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera cam, Vector3 global) { }

        internal virtual void GetQuickButtons(UISelectedInfo uISelectedInfo, IMap map, Vector3 vector3)
        {
            foreach (var c in this.Comps)
                c.GetQuickButtons(uISelectedInfo, map, vector3);
        }

        internal virtual void GetSelectionInfo(IUISelection info, IMap map, Vector3 vector3)
        {
            foreach (var c in this.Comps)
                c.GetSelectionInfo(info, map, vector3);
        }

        internal void OnBlockBelowChanged(IMap map, Vector3 global)
        {
            foreach (var c in this.Comps)
                c.OnBlockBelowChanged(map, global);
        }

        internal void MapLoaded(IMap map, Vector3 global)
        {
            foreach (var c in this.Comps)
                c.MapLoaded(map, global);
            this.OnMapLoaded(map, global);
        }

        protected virtual void OnMapLoaded(IMap map, Vector3 global)
        {
        }
        
    }
}
