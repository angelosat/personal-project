using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.AI;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Vegetation;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns.Stockpiles
{
    public class Stockpile// : Zone
    {
        readonly static public List<string> Filters = new List<string>() { ReagentComponent.Name, SkillComponent.Name, SeedComponent.Name, SaplingComponent.Name, ConsumableComponent.Name };

        //public HashSet<string> CurrentFilters = new HashSet<string>();//Filters);
        public List<HaulOrder> PendingOrders = new List<HaulOrder>();
        public HashSet<int> CurrentFilters = FiltersAll;// new HashSet<int>();//Filters);

        static public HashSet<int> FiltersAll
        {
            get
            {
                var list = new HashSet<int>();
                foreach (var category in Filters)
                {
                    var items = GameObject.Objects.Values.Where(o => o.Components.ContainsKey(category)).Select(o => (int)o.ID).ToArray();
                    foreach (var i in items)
                        list.Add(i);
                }
                return list;
            }
        }

        public int ID;
        //public Vector3 Begin, End;
        //public int Width, Height;
        public Town Town;
        //public string Name { get { return "Stockpile " + this.ID.ToString(); } }
        string CurrentName = "";
        public string Name
        {
            get { return string.IsNullOrEmpty(this.CurrentName) ? "Stockpile " + this.ID.ToString() : this.CurrentName; }
            set { this.CurrentName = value; }
        }


        bool Valid;

        Dictionary<int, int> Inventory = new Dictionary<int, int>();
        List<GameObject> Contents = new List<GameObject>();
        //List<Vector3> WatchedPositions = new List<Vector3>();
        public List<Vector3> ReservedSpots = new List<Vector3>();
        public HashSet<Vector3> Positions = new HashSet<Vector3>();

        void PopulateOwnedPositions(Vector3 global, int w, int h)
        {
            var list = new List<Vector3>();
            int x = (int)global.X;
            int y = (int)global.Y;
            //int z = (int)global.Z;
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    Vector3 pos = global + new Vector3(i, j, 0);
                    this.Positions.Add(pos);
                }
        }

        public List<GameObject> GetContents()
        {
            return this.Contents.ToList();
        }
        public Dictionary<int, int> GetContentsDictionary()
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();
            var conts = this.Contents;
            foreach(var item in conts)
            {
                //if(!dic.ContainsKey((int)item.ID))
                dic.AddOrUpdate((int)item.ID, item.StackSize, f => f + item.StackSize);
            }
            return dic;
        }
        //public Dictionary<int, int> GetContentsMerged()
        //{
        //    Dictionary<int, int> dic = new Dictionary<int, int>();
        //    var conts = this.Contents;
        //    foreach (var item in conts)
        //    {
        //        //if(!dic.ContainsKey((int)item.ID))
        //        dic.AddOrUpdate((int)item.ID, item.StackSize, f => f + item.StackSize);
        //    }
        //    return dic;
        //}
        static readonly int UpdateTimerMax = Engine.TargetFps;
        int UpdateTimer = UpdateTimerMax;
        private SaveTag farmtag;

        #region Constructors
        public Stockpile(Vector3 begin, int w, int h)
        {
            this.PopulateOwnedPositions(begin, w, h);
            //town.AddStockpile(this);
        }

        //public Stockpile(BinaryReader r)
        //{
        //    //this.Begin = r.ReadVector3();
        //    //this.Width = r.ReadInt32();
        //    //this.Height = r.ReadInt32();
        //    //this.End = Begin + new Vector3(Width, Height, 0);
        //    //this.PopulateOwnedPositions();
        //    this.OwnedPositions = new HashSet<Vector3>();
        //    var count = r.ReadInt32();
        //    for (int i = 0; i < count; i++)
        //    {
        //        this.OwnedPositions.Add(r.ReadVector3());
        //    }
        //}
        #endregion

        //public void Invalidate()
        //{
        //    this.Valid = false;
        //    //this.WatchedCells.Clear();
        //}
        //public void Validate()
        //{
        //    this.WatchedPositions.Clear();
        //}

        public void Update()
        {
            //this.UpdateTimer--;
            //if(this.UpdateTimer<=0)
            //{
            //    this.UpdateTimer = UpdateTimerMax;
            this.UpdateContents();
            //}
        }

        
        
        private void UpdateContents()
        {
            var newList = this.QueryPositions();
            var newObjects = newList.Except(this.Contents);
            // todo: optimize this?
            var removedObjects = this.Contents.Except(newList);

            //bool changed = false;
            //var newInventory = this.GetInventory();
            //    foreach(var item in newInventory)
            //    {
            //        int existing;
            //        if (this.Inventory.TryGetValue(item.Key, out existing))
            //            changed |= (existing != item.Value);
            //        else
            //            changed = true;
            //    }

            // do something with new and removed objects?
            // i also have to update UI when stacksizes change
            //if ((newObjects.Count() == removedObjects.Count()) == 0)
            //    this.Town.UITownWindow.StockpileUI.Refresh();

            this.Contents = newList;
            //OnContentsUpdated();
            this.Town.Map.EventOccured(Message.Types.StockpileContentsChanged, this, newObjects.ToDictionaryGameObjectAmount(), removedObjects.ToDictionaryGameObjectAmount());

        }
        public List<GameObject> QueryPositions()
        {
            //return Town.Map.GetObjects(this.Begin, this.End);
            List<GameObject> list = new List<GameObject>();
            foreach (var pos in this.Positions)
                list.AddRange(from obj in this.Town.Map.GetObjects() where obj.Global == pos where this.Accepts(obj) select obj); // TODO: this is shit
            return list;
        }
        public Dictionary<int, int> GetInventory()
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();
            var contents = this.Contents;
            foreach (var item in contents)
                dic.AddOrUpdate(item.GetInfo().ID, item.StackSize, (key, value) => value += item.StackSize);
            return dic;
        }

        /// <summary>
        /// TODO: optimize: call this only when contents changed
        /// </summary>
        //void OnContentsUpdated()
        //{
        //    // signal UI here
        //    //this.Town.UITownWindow.StockpileUI.Refresh();
        //    this.Town.Map.EventOccured(Message.Types.StockpileContentsChanged, this);
        //}
        //public void QueryPositions()
        //{
        //    this.Contents.Clear();
        //    this.Contents = Town.Map.GetObjects(this.Global, this.End);
        //}

        public void DrawWorld(MySpriteBatch sb, Map map, Camera cam)
        {
            var gridSprite = Sprite.BlockFaceHighlights[Vector3.UnitZ];
            Game1.Instance.GraphicsDevice.Textures[0] = Sprite.Atlas.Texture;// gridSprite.AtlasToken.Atlas.Texture;
            var fx = Game1.Instance.Content.Load<Effect>("blur");
            fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            var col = Color.Yellow;// this.Valid ? Color.Lime : Color.Red;

            foreach (var pos in this.Positions)
                gridSprite.Draw(sb, pos, cam, col);


            //int x = (int)Math.Min(this.Begin.X, this.End.X);
            //int y = (int)Math.Min(this.Begin.Y, this.End.Y);

            //for (int i = x; i < x + this.Width; i++)
            //    for (int j = y; j < y + this.Height; j++)
            //    {
            //        Vector3 global = new Vector3(i, j, this.Begin.Z);
            //        gridSprite.Draw(sb, global, cam, col);
            //    }
        }

        //public void CreateJob(GameObject toInsert)
        //{
        //    TargetArgs freeSpot = new TargetArgs(this.Begin);
        //    TargetArgs toInsertTarget = new TargetArgs(toInsert);
        //    AIJob job = new AIJob();
        //    //job.Instructions.Add(new AIInstruction(toInsertTarget, new PickUp()));

        //    //job.Instructions.Enqueue(new AIInstruction(toInsertTarget, new PickUp()));
        //    job.AddStep(new AIInstruction(toInsertTarget, new PickUp()));

        //}



       
        internal bool Accepts(GameObject entity, out Vector3 freeSpot)
        {
            if (!this.Accepts(entity))
            {
                freeSpot = Vector3.Zero;
                return false;
            }
            var spots = this.GetFreeSpots();
            freeSpot = spots.FirstOrDefault();
            //return freeSpot != null;
            return spots.Count > 0;

            //if (spots.Count == 0)
            //    return false;
            //freeSpot = spots.First();
            //return true;
        }
        internal bool TryDeposit(GameObject entity, out TargetArgs target)
        {
            if (!this.Accepts(entity))
            {
                target = TargetArgs.Empty;
                return false;
            } 
            var existing = this.Contents.Where(o => o.ID == entity.ID).FirstOrDefault(o => o.StackSize + entity.StackSize <= o.StackMax);
            if (existing != null)
            {
                target = new TargetArgs(existing);
                return true;
            }

            var spots = this.GetFreeSpots();
            target = new TargetArgs(spots.FirstOrDefault());
            return spots.Count > 0;
        }
        List<Vector3> GetFreeSpots()
        {
            //var list = new List<Vector3>();
            //int x = (int)Math.Min(this.Global.X, this.End.X);
            //int y = (int)Math.Min(this.Global.Y, this.End.Y);
            //int z = (int)this.Global.Z;
            //for (int i = x; i < x + this.Width; i++)
            //    for (int j = y; j < y + this.Height; j++)
            //    {
            //        Vector3 pos = new Vector3(i, j, z);
            //        if (this.ReservedSpots.Contains(pos))
            //            continue;
            //        if(this.Town.Map.IsEmpty(pos))
            //        {
            //            list.Add(pos);
            //        }
            //    }
            //return list;

            var list = new List<Vector3>();
            foreach (var pos in this.Positions)
            {
                if (this.ReservedSpots.Contains(pos))
                    continue;
                if (this.Town.Map.IsEmpty(pos))
                    list.Add(pos);
            }
            return list;
        }

        public bool ReserveSpot(Vector3 spot)
        {
            if (!this.Positions.Contains(spot))
                return false;
            if (this.ReservedSpots.Contains(spot))
                return false;
            this.ReservedSpots.Add(spot);
            return true;
        }
        public void ClearReserved()
        {
            this.ReservedSpots.Clear();
        }
        public void FilterToggle(bool value, params int[] filters)
        {
            foreach (var n in filters)
                this.SetFilter(n, value);

            //if (enabled)
            //    foreach (var n in names)
            //        this.CurrentFilters.Add(n);
            //else
            //    foreach (var n in names)
            //        this.CurrentFilters.Remove(n);
        }
        internal void FilterCategoryToggle(bool value, string category)
        {
            var items = GameObject.Objects.Values.Where(o => o.Components.ContainsKey(category)).Select(o=>(int)o.ID).ToArray();
            foreach (var i in items)
                this.FilterToggle(value, items);
        }
        //public void FilterToggle(bool value, params string[] names)
        //{
        //    foreach (var n in names)
        //        this.SetFilter(n, value);

        //    //if (enabled)
        //    //    foreach (var n in names)
        //    //        this.CurrentFilters.Add(n);
        //    //else
        //    //    foreach (var n in names)
        //    //        this.CurrentFilters.Remove(n);
        //}
        public void SetFilter(int filter, bool value)
        {
            if (value)
                this.CurrentFilters.Add(filter);
            else
                this.CurrentFilters.Remove(filter);

            foreach (var item in this.PendingOrders.ToList())
            {
                if (!this.Accepts(item.Entity))
                {
                    item.Job.Cancel();
                    this.ReservedSpots.Remove(item.ReservedSpot);
                    this.PendingOrders.Remove(item);
                }
            }
        }
        //public void SetFilter(string filter, bool value)
        //{
        //    if(value)
        //        this.CurrentFilters.Add(filter);
        //    else
        //        this.CurrentFilters.Remove(filter);

        //    foreach(var item in this.PendingOrders.ToList())
        //    {
        //        if(!this.Accepts(item.Entity))
        //        {
        //            item.Job.Cancel();
        //            this.ReservedSpots.Remove(item.ReservedSpot);
        //            this.PendingOrders.Remove(item);
        //        }
        //    }
        //}

        //public void FilterToggle(params string[] names)
        //{
        //    foreach (var n in names)
        //        if (!this.CurrentFilters.Remove(n))
        //            this.CurrentFilters.Add(n);
        //}
        //public void FilterEnable(params string[] names)
        //{
        //    foreach (var n in names)
        //        this.CurrentFilters.Add(n);
        //}
        //public void FilterDisable(params string[] names)
        //{
        //    foreach (var n in names)
        //        this.CurrentFilters.Remove(n);
        //}

        public void GenerateWork()
        {
            var items = (from i in this.Town.Map.GetObjects() where this.Accepts(i) select i).ToList();
        }
        //public bool Accepts(GameObject obj)
        //{
        //    foreach (var f in this.CurrentFilters)
        //        if (obj.Components.ContainsKey(f))
        //            return true;
        //    return false;
        //}
        public bool Accepts(GameObject obj)
        {
            return this.CurrentFilters.Contains((int)obj.ID);
        }
        public class HaulOrder
        {
            public GameObject Entity;
            public Vector3 ReservedSpot;
            public AIJob Job;
            public HaulOrder(GameObject entity, Vector3 reserved, AIJob job)
            {
                this.Entity = entity;
                this.ReservedSpot = reserved;
                this.Job = job;
            }
        }
        internal void Edit(Vector3 begin, Vector3 end, bool value)
        {
            //List<Vector3> positions = new List<Vector3>();
            //var dx = end.X - begin.X;
            //var dy = end.Y - begin.Y;

            //for (int i = 0; i <= dx; i++)// p.Width; i++)
            //    for (int j = 0; j <= dy; j++)//p.Height; j++)
            //    {
            //        Vector3 pos = begin + new Vector3(i, j, 0);
            //        positions.Add(pos);
            //    }
            var positions = begin.GetBox(end);
            if (!value)
                foreach (var pos in positions)
                    this.Positions.Add(pos);
            else
            {
                // check if result positions are connected
                var checkPositions = this.Positions.ToList();
                foreach (var pos in positions)
                    checkPositions.Remove(pos);
                if (checkPositions.Count == 0)
                {
                    this.Town.DeleteStockpile(this.ID);
                    return;
                }
                var isConnected = checkPositions.IsConnected();
                if (isConnected)
                    foreach (var pos in positions)
                        RemovePosition(pos);
                else
                    if (this.Town.Map.GetNetwork() is Net.Client)
                        Net.Client.Console.Write("Resulting stockpile tiles must be connected");
            }
        }

        private void RemovePosition(Vector3 pos)
        {
            foreach(var order in this.PendingOrders)
            {
                if (order.ReservedSpot == pos)
                    order.Job.Cancel();
            }
            this.Positions.Remove(pos);
        }

        //internal void Edit(Vector3 global, int w, int h, bool value)
        //{
        //    List<Vector3> positions = new List<Vector3>();
        //    for (int i = 0; i < w; i++)
        //        for (int j = 0; j < h; j++)
        //        {
        //            Vector3 pos = global + new Vector3(i, j, 0);
        //            positions.Add(pos);
        //        }

        //    if (value)
        //        foreach (var pos in positions)
        //            this.Positions.Add(pos);
        //    else
        //        foreach (var pos in positions)
        //            this.Positions.Remove(pos);
        //}

        //bool IsConnected(Vector3 pos)
        //{
        //    foreach (var n in pos.GetNeighbors())
        //        if (this.Positions.Contains(n))
        //            return true;
        //    return false;

        //    //HashSet<Vector3> Open = new HashSet<Vector3>();
        //    //HashSet<Vector3> Closed = new HashSet<Vector3>();

        //    //foreach(var pos in this.Positions)
        //    //{
        //    //    Node node = new Node();
        //    //    foreach(var n in pos.GetNeighbors())
        //    //    {
        //    //        node.Connections.Add(n - pos);
        //    //    }
        //    //}

        //    //return false;
        //}
        //class Node
        //{
        //    public List<Vector3> Connections = new List<Vector3>();
        //}

        internal void OnBlockChanged(Vector3 global)
        {
            var solid = Block.IsBlockSolid(this.Town.Map, global);
            if (!solid)
            {
                var above = global + Vector3.UnitZ;
                if (this.Positions.Contains(above))
                    this.Positions.Remove(above);
            }
            else
            {
                if (this.Positions.Contains(global))
                    this.Positions.Remove(global);
            }

            //if (solid)
            //{
            //    var below = global - Vector3.UnitZ;
            //    if (this.Positions.Contains(below))
            //        this.Positions.Remove(below);
            //}
            //else
            //    if (this.Positions.Contains(global))
            //        this.Positions.Remove(global);
        }
        internal Window GetWindow()
        {
            var win = StockpileUI.GetWindow(this);
            return win;
        }
        public List<SaveTag> Save()
        {
            var tag = new List<SaveTag>();
            tag.Add(new SaveTag(SaveTag.Types.String, "Name", this.Name));
            tag.Add(new SaveTag(SaveTag.Types.Int, "ID", this.ID));

            //tag.Add(new SaveTag(SaveTag.Types.List, "Positions"), this.Positions.ToList().Save()));
            tag.Add(this.Positions.ToList().Save("Positions"));
            tag.Add(this.CurrentFilters.Save("Fitlers"));
            return tag;
        }
        public void Load(SaveTag tag)
        {
            tag.TryGetTagValue<string>("Name", v => this.Name = v);
            tag.TryGetTagValue<int>("ID", v => this.ID = v);
            //tag.TryGetTagValue<SaveTag>("Positions", v => this.Positions = new HashSet<Vector3>(new List<Vector3>().Load(v)));
            //this.Positions = new HashSet<Vector3>(new List<Vector3>().Load(tag, "Positions"));
            tag.TryGetTagValue<List<SaveTag>>("Positions", v => this.Positions = new HashSet<Vector3>(new List<Vector3>().Load(v)));
            //tag.TryGetTagValue<List<SaveTag>>("Fitlers", v => this.CurrentFilters = new HashSet<string>(new List<string>().Load(v)));
            tag.TryGetTagValue<List<SaveTag>>("Fitlers", v => this.CurrentFilters = new HashSet<int>(new List<int>().Load(v)));
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.ID);
            w.Write(this.Name);
            w.Write(this.Positions.ToList());
            w.Write(this.CurrentFilters.ToList());
        }
        public void Read(BinaryReader r)
        {
            this.ID = r.ReadInt32();
            this.Name = r.ReadString();
            this.Positions = new HashSet<Vector3>(r.ReadListVector3());
            //this.CurrentFilters = new HashSet<string>(r.ReadListString());
            this.CurrentFilters = new HashSet<int>(r.ReadListInt());

        }
        public Stockpile(Town town, BinaryReader r)
        {
            this.Town = town;
            this.Read(r);
        }

        public Stockpile(Towns.Town town, SaveTag tag)
        {
            this.Town = town;
            this.Load(tag);
        }

        
    }
}
