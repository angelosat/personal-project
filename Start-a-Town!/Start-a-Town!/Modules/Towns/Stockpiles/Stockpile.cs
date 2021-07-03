using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Vegetation;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Start_a_Town_.Towns;

namespace Start_a_Town_
{
    public class Stockpile : ZoneNew, IStorage, IContextable// : Zone
    {
        class Packets
        {
            static int PacketStockpileSync;
            static internal void Init()
            {
                PacketStockpileSync = Network.RegisterPacketHandler(Receive);
            }
            internal static void SyncPriority(Stockpile stockpile, StoragePriority p)
            {
                var net = stockpile.Map.Net;
                if (net is Server)
                    stockpile.Settings.Priority = p;
                var w = net.GetOutgoingStream();
                w.Write(PacketStockpileSync);
                w.Write(stockpile.ID);
                w.Write(p.Value);
            }
            private static void Receive(IObjectProvider net, BinaryReader r)
            {
                var stockpileID = r.ReadInt32();
                var p = r.ReadInt32();
                //var stockpile = net.Map.Town.StockpileManager.Stockpiles[stockpileID];
                var stockpile = net.Map.Town.ZoneManager.GetZone<Stockpile>(stockpileID);

                var newPriority = StoragePriority.GetFromValue(p);
                if (net is Server)
                    SyncPriority(stockpile, newPriority);
                else
                    stockpile.Settings.Priority = newPriority;
            }
        }
        public class SpotReservation : Dictionary<int, int>
        {
            public GameObject Actor;
            public int ObjectID;
            public int Amount;
            public SpotReservation(GameObject actor, int objectID, int amount)
            {
                this.Actor = actor;
                this.ObjectID = objectID;
                this.Amount = amount;
            }
        }
        public class StockpileContent
        {
            public Vector3 Global;
            public GameObject Object;
            public int ObjectType
            {
                get
                {
                    if (this.Object != null)
                        return (int)this.Object.IDType;
                    else if (this.Reservations.Any())
                        return this.Reservations.First().Value.ObjectID;
                    else return -1;
                    //return this.Object != null ? (int)this.Object.ID : this._ObjectType;
                }
                //set
                //{
                //    this._ObjectType = value;
                //}
            }
            public Dictionary<GameObject, SpotReservation> Reservations = new Dictionary<GameObject, SpotReservation>();
            public StockpileContent(Vector3 global)
            {
                this.Global = global;
            }
            public int GetRemainingCapacityFor(GameObject obj)
            {
                var type = this.ObjectType;
                if (type == -1)
                    return obj.StackMax;
                else if (type == (int)obj.IDType)
                {
                    var reserved = this.Reservations.Sum(p => p.Value.Amount);
                    var remaining = obj.StackMax - reserved - (this.Object != null ? this.Object.StackSize : 0);
                    if (remaining < 0)
                        throw new Exception();
                    return remaining;
                }
                else return 0;
            }

            internal void Reserve(GameObject actor, GameObject obj, int amount)
            {
                if (this.Reservations.Any())
                    if (this.Reservations.First().Value.ObjectID != (int)obj.IDType)
                        throw new Exception();
                if (this.Reservations.ContainsKey(actor))
                    throw new Exception();
                this.Reservations.Add(actor, new SpotReservation(actor, (int)obj.IDType, amount));
            }

            internal void Unreserve(GameObject actor)
            {
                this.Reservations.Remove(actor);
            }

            internal void PlaceItem(GameObject actor, GameObject obj)
            {
                this.Object = obj;
                SpotReservation res = this.Reservations[actor];
                if (res.Amount != obj.StackSize)
                    throw new Exception();
                this.Reservations.Remove(actor);
            }
        }

        public class StockpileReservation
        {
            public int ObjectID = -1;
            public int Amount = 0;
            public int Capacity = 0;
            
            public void SetObject(GameObject obj)
            {
                if (obj != null)
                {
                    this.ObjectID = (int)obj.IDType;
                    this.Amount = 0;
                    this.Capacity = obj.StackMax;
                }
                else
                {
                    this.ObjectID = -1;
                    this.Amount = 0;
                    this.Capacity = 0;
                }
            }
            public int RemainingCapacity { get { return this.Capacity - this.Amount; } }// -this.ReservedToAdd; } }
            //public int RemainingAvailable { get { return this.Amount - this.ReservedToRemove; } }
            public int GetRemainingCapacityFor(GameObject obj)
            {
                var type = this.ObjectID;
                if (type == -1)
                    return obj.StackMax;
                else if (type == (int)obj.IDType)
                {
                    var remaining = this.RemainingCapacity;
                    if (remaining < 0)
                        throw new Exception();
                    return remaining;
                }
                else return 0;
            }
            public void Write(BinaryWriter w)
            {
                w.Write(this.ObjectID);
                w.Write(this.Amount);
                w.Write(this.Capacity);
             
            }
            public StockpileReservation Read(BinaryReader r)
            {
                this.ObjectID = r.ReadInt32();
                this.Amount = r.ReadInt32();
                this.Capacity = r.ReadInt32();
   
                return this;
            }
            public SaveTag Save(string name = "")
            {
                var tag = new SaveTag(SaveTag.Types.Compound, name);
                tag.Add(this.ObjectID.Save("ObjectID"));
                tag.Add(this.Amount.Save("Amount"));
                tag.Add(this.Capacity.Save("Capacity"));
             
                return tag;
            }
            public StockpileReservation Load(SaveTag tag)
            {
                this.ObjectID = tag.GetValue<int>("ObjectID");
                this.Amount = tag.GetValue<int>("Amount");
                this.Capacity = tag.GetValue<int>("Capacity");
     
                return this;
            }
        }

        internal static void Init()
        {
            Packets.Init();
        }

        readonly static public List<string> Filters = new List<string>() { ReagentComponent.Name, ToolAbilityComponent.Name, SeedComponent.Name, ConsumableComponent.Name };
        public List<HaulOrder> PendingOrders = new List<HaulOrder>();
        //public List<StorageFilter> CurrentFiltersNew = new List<StorageFilter>(StorageCategory.CreateFilterSet());
        public StorageSettings Settings = new StorageSettings();
        public int Priority
        {
            get
            {
                return this.Settings.Priority.Value;
            }
        }
        string CurrentName = "";
        //public string Name
        //{
        //    get { return string.IsNullOrEmpty(this.CurrentName) ? "Stockpile " + this.ID.ToString() : this.CurrentName; }
        //    set { this.CurrentName = value; }
        //}
        public override string UniqueName => $"Zone_Stockpile_{this.ID}";


        public override string GetName()
        {
            return this.Name;
        }

        //public StockpileManager Manager { get { return this.Town.StockpileManager; } }

        //bool Valid;

        Dictionary<int, int> Inventory = new Dictionary<int, int>();
        List<GameObject> Contents = new List<GameObject>();
        //List<Vector3> WatchedPositions = new List<Vector3>();
        public List<Vector3> ReservedSpots = new List<Vector3>();
        //public Dictionary<Vector3, SpotReservation> ReservedSpotsNew = new Dictionary<Vector3, SpotReservation>();

        //public HashSet<Vector3> Positions = new HashSet<Vector3>();
        public Dictionary<Vector3, StockpileContent> ContentsNew = new Dictionary<Vector3, StockpileContent>();
        public Dictionary<Vector3, StockpileReservation> ContentsNewNew = new Dictionary<Vector3, StockpileReservation>();

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
                    //this.ContentsNew.Add(pos, new StockpileContent(pos));
                    //this.ContentsNewNew[pos] = new StockpileReservation();
                }
        }
        public void CacheContents()
        {
            this.CacheContentsNew();
            this.CachedContents.Clear();
            foreach (var pos in this.Positions)
            {
                var items = this.Map.GetObjects(pos.Above());
                foreach (var item in items.Where(i => i.IsStockpilable()))
                {
                    //this.TotalInventory[item] = item.StackSize;
                    this.CachedContents.AddOrUpdate(item.ID, item.StackSize, (count) => count += item.StackSize);
                }
            }
        }
        public void CacheContentsNew()
        {
            this.Cache.Clear();
            foreach (var pos in this.Positions)
                this.Cache.AddRange(this.Map.GetObjects(pos.Above()).Where(i => i.IsStockpilable()));
        }
        public IEnumerable<GameObject> GetContentsNew()
        {
            foreach (var i in this.Cache)
                yield return i;
        }
        public List<GameObject> GetContents()
        {
            var contents = new List<GameObject>();
            foreach (var pos in this.Positions)
                contents.AddRange(this.Town.Map.GetObjects(pos + IntVec3.UnitZ));
            return contents;
        }
        public Dictionary<int, int> CachedContents = new Dictionary<int, int>();
        List<GameObject> Cache = new();
        public Dictionary<int, int> GetContentsDictionary()
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();
            var conts = this.Contents;
            foreach (var item in conts)
            {
                dic.AddOrUpdate((int)item.IDType, item.StackSize, f => f + item.StackSize);
            }
            return dic;
        }
        
        static readonly int UpdateTimerMax = Engine.TicksPerSecond;


        public void Update() { }
        public void UpdateOld()
        {
            
            this.UpdateContents();
            this.UpdateContentsNew();
        }

        private void UpdateContentsNew()
        {
            var objects = this.Town.Map.GetObjects();
            foreach (var pos in this.ContentsNew)
            {
                var found = objects.Where(o => o.IsAtBlock(pos.Key)).FirstOrDefault();
                if (found == null)
                    continue;
                pos.Value.Object = found;
                // TODO: account for possibility of multiple objects in the same cell
            }
        }

        private void UpdateContents()
        {
            var newList = this.ScanExistingStoredItems();
            var newObjects = newList.Except(this.Contents);
            // todo: optimize this?
            var removedObjects = this.Contents.Except(newList);


            this.Contents = newList;
            //OnContentsUpdated();
            this.Town.Map.EventOccured(Message.Types.StockpileContentsChanged, this, newObjects.ToDictionaryGameObjectAmount(), removedObjects.ToDictionaryGameObjectAmount());

        }
        public List<GameObject> ScanExistingStoredItems()
        {
            //return Town.Map.GetObjects(this.Begin, this.End);
            List<GameObject> list = new List<GameObject>();
            var objects = this.Town.Map.GetObjects();
            foreach (var pos in this.Positions)
                list.AddRange(from obj in objects where this.Accepts(obj) where obj.Global - Vector3.UnitZ == (Vector3)pos select obj); // TODO: this is shit
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

        internal bool Accepts(GameObject obj)
        {
            var item = obj as Entity;
            return item != null ? this.Accepts(item) : false;
        }
        internal bool Accepts(Entity entity, out Vector3 freeSpot)
        {
            if (!this.Accepts(entity))
            {
                freeSpot = Vector3.Zero;
                return false;
            }
            var spots = this.GetFreeSpots();
            freeSpot = spots.FirstOrDefault();
            return spots.Count > 0;

        }
        /// <summary>
        /// Returns true if the stockpile accept this type of item, and there's a free spot available
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        internal bool TryDeposit(GameObject entity, out TargetArgs target)
        {
            if (!this.Accepts(entity))
            {
                target = TargetArgs.Null;
                return false;
            }
            var existing = this.Contents.Where(o => o.IDType == entity.IDType).FirstOrDefault(o => o.StackSize + entity.StackSize <= o.StackMax);
            if (existing != null)
            {
                target = new TargetArgs(existing);
                return true;
            }

            var spots = this.GetFreeSpots();
            target = new TargetArgs(spots.FirstOrDefault());
            return spots.Count > 0;
        }
        public List<TargetArgs> GetPotentialSpotsFor(GameObject obj)
        {
            var spots = new List<TargetArgs>();
            foreach (var content in this.Contents.Where(i => i.IDType == obj.IDType))
                spots.Add(new TargetArgs(content));
            spots.Concat(this.GetFreeSpots().Select(s => new TargetArgs(s)));
            return spots;
        }
        public List<TargetArgs> GetPotentialSpotsFor(GameObject obj, out int maxamount)
        {
            var stackmax = obj.StackMax;
            maxamount = 0;
            var spots = new List<TargetArgs>();
            if (!this.Accepts(obj))
                return spots;
            foreach (var content in this.Contents.Where(i => i.IDType == obj.IDType && !i.Full))
            {
                spots.Add(new TargetArgs(content));
                maxamount += stackmax - content.StackSize;
            }
            var freespots = this.GetFreeSpots();
            maxamount += freespots.Count * stackmax;
            spots = spots.Concat(freespots.Select(s => new TargetArgs(s))).ToList();
            return spots;
        }
        public List<IntVec3> GetPotentialPositionsFor(GameObject obj, out int maxamount)
        {
            var stackmax = obj.StackMax;
            maxamount = 0;
            var spots = new List<IntVec3>();
            if (!this.Accepts(obj))
                return spots;
            foreach (var content in this.Contents.Where(i => i.IDType == obj.IDType && !i.Full))
            {
                spots.Add(content.Global.Round());
                maxamount += stackmax - content.StackSize;
            }
            var freespots = this.GetFreeSpots();
            maxamount += freespots.Count * stackmax;
            spots = spots.Concat(freespots).ToList();
            return spots;
        }
       
        public IEnumerable<IntVec3> GetAvailableCells()
        {
            var emptyCells =
                this.Positions
                .Where(p => this.Town.ReservationManager.CanReserve(p.Above()))
                .Where(p => !this.Town.Map.GetObjects(p.Above()).Any())
                .Select(p => p.Above());
            return emptyCells;
        }
        public bool CanAccept(GameObject item)
        {
            return this.Accepts(item) && this.GetAvailableCells().Any();
        }
        public TargetArgs GetBestHaulTarget(GameObject actor, GameObject item)
        {
            var emptyCells = this.Positions.Where(pos => !this.Town.Map.GetObjects(pos.Above()).Any()).Select(p => p.Above());
            foreach (var pos in emptyCells)
            {
                if (!actor.CanReserve(pos))
                    continue;
                return new TargetArgs(pos);
            }
            return null;
        }
        public IEnumerable<TargetArgs> DistributeToStorageSpotsNewLazy(GameObject actor, GameObject obj)
        {
            var emptyCells = new List<Vector3>();
            foreach (var pos in this.Positions)
            {
                var above = pos.Above();
                var itemsInCell = this.Map.GetObjects(above);
                if (!itemsInCell.Any())
                {
                    emptyCells.Add(above);
                    continue;
                }
                foreach (var item in itemsInCell)
                {
                    if (!this.Accepts(item))
                        continue;
                    if (!item.CanAbsorb(obj))
                        continue;
                    yield return new TargetArgs(item);
                }
            }
            foreach (var cell in emptyCells)
            {
                yield return new TargetArgs(this.Map, cell);
            }
        }
        public Dictionary<TargetArgs, int> DistributeToStorageSpotsNew(GameObject actor, GameObject obj, out int maxamount)
        {
            var valid = new Dictionary<TargetArgs, int>();
            var currentSimilarContents = this.ScanExistingStoredItems().Where(o => o.CanAbsorb(obj) && this.Accepts(o) && o.StackSize < o.StackMax);//.Select(o=>new TargetArgs(o));

            maxamount = 0;
            foreach (var item in currentSimilarContents)
            {
                if (!actor.CanReserve(item))//.Global.SnapToBlock()))
                    continue;
                var validAmount = item.StackMax - item.StackSize;
                valid.Add(new TargetArgs(item), validAmount);
                maxamount += validAmount;
            }
            //var emptyCells = this.Positions.Where(pos => !this.Town.Map.GetObjects(pos.Above()).Any()).Select(p=>p.Above());
            var emptyCells =
                this.Positions
                .Where(pos => !this.Town.Map.GetObjects(pos.Above())
                    .Where(t => t != actor)
                    .Any())
                .Select(p => p.Above()).ToList();

            foreach (var pos in emptyCells)
            {
                if (!actor.CanReserve(pos))
                    continue;
                valid.Add(new TargetArgs(pos), obj.StackMax);
                maxamount += obj.StackMax;
            }
            return valid;
        }
        public Dictionary<Vector3, int> DistributeToStorageSpots(GameObject obj, out int maxamount)
        {
            // sort by remaining capacity so during hauling spots with an existing stack of the item will be preferred

            maxamount = 0;
            var dic = new Dictionary<Vector3, int>();
            var possible = GetValidLocations(obj);


            for (int i = 1; i <= obj.StackMax; i++) // we prioritize positions which contain stacks of the same item type and closest to stackmax
            {
                var best = possible.Where(v => v.Value.GetRemainingCapacityFor(obj) == i);
                foreach (var b in best)
                {
                    maxamount += i;
                    dic.Add(b.Key, i);
                }
            }
            return dic;

            foreach (var pos in this.ContentsNew)
            {
                var type = pos.Value.ObjectType;
                if (type == (int)obj.IDType || type == -1)
                {
                    var remaining = pos.Value.GetRemainingCapacityFor(obj);
                    if (remaining <= 0)
                        continue;
                    maxamount += remaining;
                    dic.Add(pos.Key, remaining);
                }
            }

            return dic;
        }

        private IEnumerable<KeyValuePair<Vector3, StockpileContent>> GetValidLocations(GameObject obj)
        {
            var possible = this.ContentsNew.Where(c => c.Value.ObjectType == (int)obj.IDType || c.Value.ObjectType == -1);
            //var possible = this.Positions.Where(pos => !this.Town.Map.GetObjects(pos + Vector3.UnitZ).Any());
            //var possible = this.ContentsNewNew.Where(c => c.Value.ObjectID == (int)obj.ID || c.Value.ObjectID == -1);
            return possible;
        }
        public IEnumerable<TargetArgs> GetPotentialHaulTargets(GameObject actor, GameObject item)
        {
            foreach (var target in this.DistributeToStorageSpotsNewLazy(actor, item))
                yield return target;
        }
        public Dictionary<TargetArgs, int> GetPotentialHaulTargets(GameObject actor, GameObject item, out int maxAmount)
        {
            return this.DistributeToStorageSpotsNew(actor, item, out maxAmount);
        }
        public List<IntVec3> GetFreeSpots()
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

            var list = new List<IntVec3>();
            foreach (var pos in this.Positions)
            {
                var emptySpace = pos + IntVec3.UnitZ;
                if (this.ReservedSpots.Contains(emptySpace))
                    continue;
                if (this.Town.Map.IsEmpty(emptySpace))
                    list.Add(emptySpace);
            }
            return list;
        }
        
      
       
        public void FilterToggle(bool value, params int[] filters)
        {
            //foreach (var n in filters)
            //    this.SetFilter(n, value);
        }
        public void FilterToggle(bool value, params StorageFilter[] filters)
        {
            foreach (var n in filters)
                this.SetFilter(n, value);
        }
        public void FilterToggle(params StorageFilter[] filters)
        {
            foreach (var n in filters)
                this.ToggleFilter(n);
        }
        internal void FilterCategoryToggle(bool value, string category)
        {
            var items = GameObject.Objects.Values.Where(o => o.Components.ContainsKey(category)).Select(o => (int)o.IDType).ToArray();
            foreach (var i in items)
                this.FilterToggle(value, items);
        }

        public void ToggleFilter(StorageFilter filter)
        {
            this.Settings.Toggle(filter);
        }
        public void SetFilter(StorageFilter filter, bool value)
        {
            this.Settings.Toggle(filter, value);

        }

        public bool Accepts(Entity obj)
        {
            return this.DefaultFilters.Filter(obj);
       
        }
        [Obsolete]
        public bool Accepts(int objID)
        {
            return this.Accepts(GameObject.Objects[objID]);
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
            public override string ToString()
            {
                return this.Entity.ToString() + " to " + this.ReservedSpot.ToString();
            }
        }
        public override IEnumerable<IntVec3> GetPositions()
        {
            foreach (var p in this.Positions)
                yield return p;
        }
       
        

        internal override void OnBlockChanged(IntVec3 global)
        {
            var below = global.Below;

            if (this.Positions.Contains(global))
            {
                if (!Block.IsBlockSolid(this.Town.Map, global))
                {
                    //this.Positions.Remove(global);
                    this.RemovePosition(global);
                    return;
                }
            }
            else if (this.Positions.Contains(below))
            //if (Block.IsBlockSolid(this.Town.Map, global))
            {
                if (!this.Map.IsAir(global))
                {
                    //this.Positions.Remove(below);
                    this.RemovePosition(below);
                    return;
                }
            }
        }
        
        internal void CancelJobs()
        {
            foreach (var item in this.PendingOrders)
            {
                this.Town.RemoveJob(item.Job, "Stockpile removed");
            }
        }
        internal Window GetWindow()
        {
            var win = StockpileUI.GetWindow(this);
            return win;
        }
        protected override void SaveExtra(SaveTag tag)
        {
            //var tag = new SaveTag(SaveTag.Types.Compound, name);
            //tag.Add(new SaveTag(SaveTag.Types.String, "Name", this.Name));
            //tag.Add(new SaveTag(SaveTag.Types.Int, "ID", this.ID));

            //this.Positions.Save(tag, "Positions");
            var spotstag = new SaveTag(SaveTag.Types.List, "Contents", SaveTag.Types.Compound);
            foreach (var spot in this.ContentsNewNew)
            {
                var stag = new SaveTag(SaveTag.Types.Compound);
                stag.Add(spot.Key.SaveOld("Position"));
                stag.Add(spot.Value.Save("Contents"));
                spotstag.Add(stag);
            }
            tag.Add(spotstag);
            //return tag;
        }
        protected override void LoadExtra(SaveTag tag)
        {
            //tag.TryGetTagValue<string>("Name", v => this.Name = v);
            //tag.TryGetTagValue<int>("ID", v => this.ID = v);
            //tag.TryGetTagValue<List<SaveTag>>("Positions", v => this.Positions = new HashSet<IntVec3>(new List<Vector3>().Load(v).Select(i=>(IntVec3)i)));

            tag.TryGetTagValue<List<SaveTag>>("Contents", list =>
            {
                foreach (var t in list)
                {
                    var key = t["Position"].LoadVector3();// (Vector3)t["Position"].Value;
                    var val = new StockpileReservation().Load(t["Contents"]);
                    this.ContentsNewNew[key] = val;
                }
            });
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            //w.Write(this.ID);
            //w.Write(this.Name);
            //this.Positions.Write(w);
            w.Write(this.ContentsNewNew.Count);
            foreach (var pos in this.ContentsNewNew)
            {
                w.Write(pos.Key);
                //w.Write(pos.Value);
                pos.Value.Write(w);
            }
        }
        protected override void ReadExtra(BinaryReader r)
        {
            //this.ID = r.ReadInt32();
            //this.Name = r.ReadString();
            //this.Positions.Read(r);
            //this.CurrentFilters = new HashSet<int>(r.ReadListInt());
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                this.ContentsNewNew[r.ReadVector3()] = new StockpileReservation().Read(r);// r.ReadInt32();
            }
        }
        //public Stockpile(Town town, BinaryReader r)
        //{
        //    this.Town = town;
        //    this.Read(r);
        //}

        //public Stockpile(Towns.Town town, SaveTag tag)
        //{
        //    this.Town = town;
        //    this.Load(tag);
        //}
        public Stockpile(ZoneManager manager):base(manager)
        {
        }
        public Stockpile(ZoneManager manager, IEnumerable<IntVec3> positions) : base(manager)
        {
            this.Positions = new HashSet<IntVec3>(positions);
        }
        public void GetContextActions(ContextArgs a)
        {
            a.Actions.Add(new ContextAction("Edit stockpile", ToggleInterface));
        }

        private bool ToggleInterface()
        {
            return this.GetWindow().Toggle();
        }


        internal void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.EntityPlacedItem:
                    break;
               

                default:
                    break;
            }
        }

        internal void Unreserve(GameObject actor)
        {
            foreach (var item in this.ContentsNew)
                item.Value.Unreserve(actor);

            //item.Value.Reservations.Remove(actor);
        }
        public Window ToggleWindow()
        {
            return StockpileUI.ShowWindow(this);
        }

        [Obsolete]
        internal bool IsValidStorage(int hauledID, Vector3 destination)
        {
            return this.Accepts(hauledID);
            //return false;
        }
        [Obsolete]
        internal bool IsValidStorage(int hauledID, Vector3 destination, int amount)
        {
            if (!this.Accepts(hauledID))
                return false;
            var contents = this.ContentsNewNew[destination];
            if (contents.ObjectID != hauledID)
                return false;
            if (amount > contents.RemainingCapacity)
                return false;
            return true;
        }
        public bool IsValidStorage(Entity item, TargetArgs target, int amount)
        {
            if (!this.Accepts(item))
                return false;
            var itemsOnCell = this.Town.Map.GetObjects(target.Global);

            switch (target.Type)
            {
                case TargetType.Position:
          
                    break;

                case TargetType.Entity:
                    if (!itemsOnCell.Contains(target.Object))
                        throw new Exception();
                    if (target.Object.StackSize + amount > target.Object.StackMax)
                        throw new Exception();
                    break;

                default:
                    break;
            }


            return true;
        }
        [Obsolete]
        public bool IsValidStorage(int hauledID, TargetArgs target, int amount)
        {
            if (!this.Accepts(hauledID))
                return false;
            var itemsOnCell = this.Town.Map.GetObjects(target.Global);

            switch (target.Type)
            {
                case TargetType.Position:
       
                    break;

                case TargetType.Entity:
                    if (!itemsOnCell.Contains(target.Object))
                        throw new Exception();
                    if (target.Object.StackSize + amount > target.Object.StackMax)
                        throw new Exception();
                    break;

                default:
                    break;
            }


            return true;
        }

        public IEnumerable<Vector3> GetPositionsLazy()
        {
            foreach (var pos in this.Positions)
                yield return pos;
        }

        
        public override void GetSelectionInfo(IUISelection panel)
        {
            panel.AddTabAction("Stockpile", this.ToggleFiltersUI);

        }
        static Window WindowFilters;
        private void ToggleFiltersUI()
        {
            // TODO: update controls when selecting another stockpile
            if(WindowFilters is not null && WindowFilters.Tag != this && WindowFilters.IsOpen)
            {
                WindowFilters.Client.ClearControls();
                WindowFilters.Client.AddControls(getGUI());
                WindowFilters.SetTitle($"{this.UniqueName} settings");
                WindowFilters.SetTag(this);
                return;
            }
            if (WindowFilters == null)
            {

                WindowFilters =
                    getGUI()
                    .ToWindow("Stockpile settings");
            }
            WindowFilters.SetTitle($"{this.UniqueName} settings");
            WindowFilters.SetTag(this);
            WindowFilters.Toggle();

            GroupBox getGUI()
            {
                var box = new GroupBox();
                box.AddControlsVertically(
                    new GroupBox().AddControlsHorizontally(
                        new ComboBoxNewNew<StoragePriority>(StoragePriority.All, 128, p => $"Priority: {p}", p => $"{p}", syncPriority, () => this.Settings.Priority)),
                    DefaultFilters.GetControl((n, l) => PacketStorageFiltersNew.Send(this, n, l))
                        .ToPanelLabeled("Fitlers"));
                return box;
                
                void syncPriority(StoragePriority p)
                {
                    Packets.SyncPriority(this, p);
                }
            }
        }

        StorageSettings IStorage.Settings => this.Settings;

        //ZoneDef zoneDef;
        //public override ZoneDef ZoneDef => this.zoneDef ??= Def.GetDef<ZoneStockpileDef>();
        //public override Type DefType => typeof(ZoneStockpileDef);
        public override ZoneDef ZoneDef => ZoneDefOf.Stockpile;

        StorageFilterCategoryNew DefaultFilters = initFilters();
        static StorageFilterCategoryNew initFilters()
        {
            var cats = Def.Database.Values.OfType<ItemDef>().GroupBy(d => d.Category);

            var all = new StorageFilterCategoryNew("All");
            foreach (var cat in cats)
            {
                if (cat.Key == null)
                    continue;
                var c = new StorageFilterCategoryNew(cat.Key.Label);
                foreach (var def in cat)
                {
                    if (def.DefaultMaterialType != null)
                    {
                        c.AddChildren(new StorageFilterCategoryNew(def.Label).AddLeafs(def.DefaultMaterialType.SubTypes.Select(m => new StorageFilterNew(def, m))));
                    }
                    else
                        c.AddLeafs(new StorageFilterNew(def));
                }
                all.AddChildren(c);
            }

            return all;
        }

        public void ToggleItemFiltersCategories(int[] categoryIndices)
        {
            var indices = categoryIndices;
            foreach (var i in indices)
            {
                var c = DefaultFilters.GetNodeByIndex(i);
                var all = c.GetAllDescendantLeaves();
                var minor = all.GroupBy(a => a.Enabled).OrderBy(a => a.Count()).First();
                foreach (var f in minor)
                    f.Enabled = !minor.Key;
            }
        }
        public void ToggleItemFilters(int[] gameObjects)
        {
            var indices = gameObjects;
            foreach (var i in indices)
            {
                var f = DefaultFilters.GetLeafByIndex(i);
                f.Enabled = !f.Enabled;
            }
        }
    }
}
