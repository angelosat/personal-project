using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Start_a_Town_.Towns;
using System.IO;

namespace Start_a_Town_
{
    abstract public class ZoneNew : ISelectable, ISaveable, ISerializable
    {
        public Town Town => this.Manager.Town;
        public IMap Map { get { return this.Town.Map; } }
        public HashSet<IntVec3> Positions = new();
        public string Name;
        public ZoneManager Manager;
        public int ID { get; set; }
        public Color Color;
        public abstract ZoneDef ZoneDef { get; }
        public abstract string UniqueName { get; }

        abstract public IEnumerable<IntVec3> GetPositions(); // TODO: make it a hashset for faster lookups
        public void Delete()
        {
            this.Manager.Delete(this);
        }
        public bool Exists
        {
            get
            {
                return this.Manager.Zones.ContainsKey(this.ID);
            }
        }
        static readonly Random Random = new();
        protected ZoneNew()
        {
            var array = new byte[3];
            Random.NextBytes(array);
            this.Color = new Color(array[0], array[1], array[2]);
        }
        public ZoneNew(ZoneManager manager):this()
        {
            this.Manager = manager;
        }
        internal virtual void OnBlockChanged(IntVec3 global)
        {
            var map = this.Map;
            var below = global.Below;
            if (this.Positions.Contains(global) && !Block.IsBlockSolid(map, global))
            {
                this.RemovePosition(global);
                return;
            }
            else if (this.Positions.Contains(below) && !map.IsAir(global))
            {
                this.RemovePosition(below);
                return;
            }
        }

        private bool IsValidLocation(IntVec3 global)
        {
            var map = this.Map;
            var above = global.Above();
            if (this.Positions.Contains(global) && !map.IsSolid(global))
                return false;
            if (map.IsSolid(above))
                return false;
            return this.ZoneDef.IsValidLocation(map, global);
        }

        public void AddPosition(IntVec3 pos)
        {
            this.Positions.Add(pos);
            return;
            // maybe dont check here because positions might be added in bulk and the most distant ones might be checked first
            if (pos.GetAdjacentHorLazy().Any(this.Positions.Contains)) 
                this.Positions.Add(pos);
        }

        public void RemovePosition(IntVec3 pos)
        {
            this.RemovePositions(new[] { pos });
            //this.Positions.Remove(pos);
            //var splitgraphs = this.Positions.GetAllConnectedSubGraphs();
            //if (splitgraphs.Count == 1)
            //    return;
            //var largest = splitgraphs.OrderByDescending(g => g.Count).First();
            //this.Positions = largest;
        }
        public void RemovePositions(IEnumerable<IntVec3> positions)
        {
            foreach (var pos in positions)
                this.Positions.Remove(pos);
            if (!this.Positions.Any())
            {
                this.Delete();
                return;
            }
            var splitgraphs = this.Positions.GetAllConnectedSubGraphs();
            if (splitgraphs.Count == 1)
                return;
            var largest = splitgraphs.OrderByDescending(g => g.Count).First();
            this.Positions = largest;
        }
        protected void PopulateOwnedPositions(IEnumerable<Vector3> positions)
        {
            foreach (var pos in positions)
            {
                if (!IsPositionValid(this.Map, pos))
                    continue;
                this.Positions.Add(pos);
            }
        }
        
        internal void Edit(IntVec3 begin, IntVec3 end, bool remove)
        {
            var inputpositions = begin.GetBoxLazy(end);

            if (!remove)
            {
                var finalPositions = inputpositions.Where(pos => this.Town.GetZoneAt(pos) == null).Union(this.Positions);
                if (!finalPositions.IsConnectedNew())
                {
                    //if (this.Town.Map.Net is Client)
                    //    Client.Instance.Log.Write("Resulting zone must be connected");
                    // TODO is it safe to register new zones independently on client?
                    //if (this.GetType() == typeof(GrowingZone))
                    //    this.Map.Town.FarmingManager.RegisterNewZone(inputpositions);
                    //else if (this.GetType() == typeof(Stockpile))
                    //    this.Map.Town.StockpileManager.RegisterNewZone(inputpositions);
                    this.Manager.RegisterNewZone(this.GetType(), inputpositions);

                    return;
                }
                foreach (var pos in inputpositions.Except(this.Positions))
                {
                    if (this.Town.GetZoneAt(pos) == null)
                    {
                        this.AddPosition(pos);
                    }
                }
            }
            else
            {
                this.RemovePositions(inputpositions);
                //// check if result positions are connected
                //var checkPositions = thisPositions;//.Intersect(inputpositions).ToList();
                //foreach (var pos in inputpositions)
                //    checkPositions.Remove(pos);
                //if (checkPositions.Count == 0)
                //{
                //    this.Delete();
                //    return;
                //}
                //var isConnected = checkPositions.IsConnectedNew();
                //if (isConnected)
                //    foreach (var pos in inputpositions)
                //        this.RemovePosition(pos);
                //else
                //    if (this.Town.Map.Net is Client)
                //    Client.Instance.Log.Write("Resulting zone must be connected");
            }
        }
        public void Invalidate()
        {
            this.Validate();
        }
        protected virtual void Validate() { }
        internal void OnBlockChangedNew(IntVec3 pos)
        {
            if (!this.Positions.Contains(pos))
                return;
            if (!this.ZoneDef.IsValidLocation(this.Map, pos))
                this.RemovePosition(pos);
            this.Validate();
        }
        internal void EditOld(IntVec3 begin, IntVec3 end, bool remove)
        {
            //var thisPositions = this.GetPositions();
            var thisPositions = this.Positions;
            //var inputpositions = begin.GetBox(end);
            var inputpositions = begin.GetBoxLazy(end);

            if (!remove)
            {
                var finalPositions = inputpositions.Where(pos => this.Town.GetZoneAt(pos) == null).Union(thisPositions);
                if(!finalPositions.IsConnectedNew())
                {
                    if (this.Town.Map.Net is Client)
                        Client.Instance.Log.Write("Resulting zone must be connected");
                    return;
                }
                foreach (var pos in inputpositions.Except(thisPositions))
                {
                    if (this.Town.GetZoneAt(pos) == null)
                    {
                        this.AddPosition(pos);
                    }
                }
            }
            else
            {
                // check if result positions are connected
                var checkPositions = thisPositions;//.Intersect(inputpositions).ToList();
                foreach (var pos in inputpositions)
                    checkPositions.Remove(pos);
                if (checkPositions.Count == 0)
                {
                    this.Delete();
                    return;
                }
                var isConnected = checkPositions.IsConnectedNew();
                if (isConnected)
                    foreach (var pos in inputpositions)
                        this.RemovePosition(pos);
                else
                    if (this.Town.Map.Net is Client)
                        Client.Instance.Log.Write("Resulting zone must be connected");
            }
        }
        [Obsolete]
        public bool ContainsOld(GameObject obj)
        {
            foreach (var pos in this.GetPositions().Select(p => p.Above()))
                if (this.Town.Map.GetObjects(pos).Contains(obj))
                    return true;
            return false;
        }
        internal bool Contains(GameObject obj)
        {
            var g = obj.Global.SnapToBlock() - Vector3.UnitZ;
            return this.GetPositions().Contains(g);
        }
        internal bool Contains(IntVec3 pos)
        {
            return this.Positions.Contains(pos);
        }
        internal void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera cam)
        {
            var isselected = UISelectedInfo.IsSelected(this);
            var col = Color.Lerp(this.Color, Color.White, isselected ? .5f : 0) * .5f; //(isselected ? Color.White : this.Color) * .5f;// this.Color * (isselected ? .5f : .3f);
            var positions = this.GetPositions().Select(t => t + IntVec3.UnitZ);
            //var positions = this.GetPositions();                                          
            //if (!(ToolManager.Instance.ActiveTool is ToolZoningPositions))
                cam.DrawGrid(sb, map, positions, col);
                //cam.DrawGridBlocks(sb, Block.BlockBlueprintGrayscale, positions, col);
        }

        internal static bool IsPositionValid(IMap map, Vector3 pos)
        {
            if (!map.IsSolid(pos))
                return false;
            if (map.IsSolid(pos.Above()))
                return false;
            return true;
        }
        public void RequestDelete()
        {
            PacketZoneDelete.Send(Client.Instance, this.GetType(), this.ID);
        }
        public void Edit()
        {
            ToolManager.SetTool(new ToolDesignateZone(this.GetType()));
        }
        static public void Edit(Type zoneType)
        {
            ToolManager.SetTool(new ToolDesignateZone(zoneType));
        }
        //public virtual void Select(UISelectedInfo info)
        //{
        //    //info.AddButtons(new IconButton(Icon.Cross) { LeftClickAction = this.RequestDelete, HoverText = "Delete" });
        //    //info.AddButtons(new IconButton(Icon.Construction) { LeftClickAction = this.Edit, HoverText = "Edit" });
        //}

        public abstract string GetName();

        public virtual void GetSelectionInfo(IUISelection info)
        {
            //throw new NotImplementedException();
        }

        public void GetQuickButtons(UISelectedInfo info)
        {
            //throw new NotImplementedException();
            info.AddButtons(new IconButton(Icon.Cross) { LeftClickAction = this.RequestDelete, HoverText = "Delete" });
            info.AddButtons(new IconButton(Icon.Construction) { LeftClickAction = this.Edit, HoverText = "Edit" });
        }

        public void TabGetter(Action<string, Action> getter)
        {
            throw new NotImplementedException();
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            //this.GetType().FullName.Save(tag, "Type");
            this.ID.Save(tag, "ID");
            this.Name.Save(tag, "Name");
            this.Positions.Save(tag, "Positions");

            this.SaveExtra(tag);
            return tag;
        }
        protected virtual void SaveExtra(SaveTag tag) { }

        public ISaveable Load(SaveTag tag)
        {
            this.ID = tag.GetValue<int>("ID");
            tag.TryGetTagValueNew("Name", ref this.Name);
            tag.TryGetTagValue<List<SaveTag>>("Positions", v => this.Positions = new HashSet<IntVec3>(new List<Vector3>().Load(v).Select(i => (IntVec3)i)));
            this.LoadExtra(tag);
            return this;
        }
        protected virtual void LoadExtra(SaveTag tag) { }

        public void Write(BinaryWriter w)
        {
            w.Write(this.ID);
            w.Write(this.Name);
            this.Positions.Write(w);
            this.WriteExtra(w);
        }
        protected virtual void WriteExtra(BinaryWriter w) { }

        public ISerializable Read(BinaryReader r)
        {
            this.ID = r.ReadInt32();
            this.Name = r.ReadString();
            this.Positions.Read(r);
            this.ReadExtra(r);
            return this;
        }
        protected virtual void ReadExtra(BinaryReader r) { }

        public virtual IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            yield break;
        }
    }
}
