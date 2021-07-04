using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns
{
    public class StockpileManager : TownComponent
    {
        

        public override string Name
        {
            get
            {
                return "Stockpiles";
            }
        }
        StockpileContentTracker Tracker;

        internal Stockpile GetStockpile(int stID)
        {
            return this.Stockpiles[stID];
        }
        int _StockpileSequence = 1;

        public int NextStockpileID { get { return _StockpileSequence++; } }
        const float UpdateFrequency = 1; // per second
        static readonly float UpdateTimerMax = (float)Engine.TicksPerSecond / UpdateFrequency;
        float UpdateTimer;
       

        public StockpileManager(Town town)
        {
            this.Town = town;
            this.Tracker = new StockpileContentTracker(this);
        }

        public override void Update()
        {
          
            if (this.Stockpiles.Count > 0)
                this.Tracker.Update();

            if (this.UpdateTimer > 0)
            {
                this.UpdateTimer--;
                return;
            }

            this.UpdateTimer = UpdateTimerMax;
            foreach (var item in this.Stockpiles)
                item.Value.Update();
            if (this.Town.Map.Net is Client)
                return;

        }

        public override GroupBox GetInterface()
        {
            var ui = new StockpilesManagerUI(this.Town);
            return ui;
        }

        //public override void Handle(IObjectProvider net, Packet msg)
        //{
        //    switch (msg.PacketType)
        //    {
                
        //        case PacketType.StockpileFilters:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                PacketStockpileFiltersNew.Handle(net, r, msg);
        //                var server = net as Server;
        //                if (server != null)
        //                    server.Enqueue(PacketType.StockpileFilters, msg.Payload, SendType.OrderedReliable, true);
        //            });
        //            break;

        //        case PacketType.StockpileFiltersCategories:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                PacketStockpileFiltersToggleCategories.Handle(net, r, msg);
        //                net.Forward(msg);
        //            });
        //            break;

        //        case PacketType.StockpileRename:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                //var stockpile = this.Town.Stockpiles[r.ReadInt32()];
        //                //var newname = r.ReadString();
        //                int spID;
        //                string name;
        //                PacketStockpileRename.Read(r, out spID, out name);
        //                var stockpile = this.Stockpiles[spID];
        //                stockpile.Name = name;
        //                net.Map.EventOccured(Components.Message.Types.StockpileUpdated, stockpile);

        //                var server = net as Server;
        //                if (server != null)
        //                    server.Enqueue(PacketType.StockpileRename, msg.Payload, SendType.OrderedReliable, true);
        //            });
        //            break;

        //        case PacketType.StockpileEdit:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int spID;
        //                Vector3 begin, end;
        //                bool value;
        //                PacketStockpileEdit.Read(r, out spID, out begin, out end, out value);
        //                //Vector3 global;
        //                //int w, h;
        //                //bool value;
        //                //PacketStockpileEdit.Read(r, out spID, out global, out w, out h, out value);
        //                var stockpile = this.Stockpiles[spID];

        //                //stockpile.Edit(global, w, h, value);
        //                stockpile.Edit(begin, end, value);

        //                var server = net as Server;
        //                if (server != null)
        //                    server.Enqueue(PacketType.StockpileEdit, msg.Payload, SendType.OrderedReliable, true);
        //            });
        //            break;

        //        case PacketType.StockpileDelete:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int senderid;
        //                int spID;
        //                PacketStockpileDelete.Read(r, out senderid, out spID);
        //                this.RemoveStockpile(spID);
        //                net.Forward(msg);
        //            });
        //            break;


        //        default:
        //            break;
        //    }
        //}

        internal static void Init()
        {
            Stockpile.Init();
        }

        internal ZoneNew PlayerEdit(int zoneID, Vector3 a, int w, int h, bool remove)
        {
            if (remove)
            {
                foreach (var zone in this.Stockpiles.Values.ToList())
                    zone.Edit(a, a + new Vector3(w - 1, h - 1, 0), remove);
            }
            else
            {
                if (zoneID == 0)
                    return RegisterNewZone(a, w, h);
                else
                    this.Stockpiles[zoneID].Edit(a, a + new Vector3(w - 1, h - 1, 0), remove);
            }
            return null;
        }
        internal ZoneNew RegisterNewZone(IEnumerable<IntVec3> allpositions)
        {
            throw new Exception();
            //if (!allpositions.IsConnectedNew())
            //    return null;
            //var finalPositions = allpositions.Where(
            //    po => this.Town.GetZoneAt(po) == null && 
            //    ZoneNew.IsPositionValid(this.Town.Map, po));
            //var stockpile = new Stockpile(this.Town, finalPositions);
            //this.RegisterZone(stockpile);
            //return stockpile;
        }
        private ZoneNew RegisterNewZone(Vector3 a, int w, int h)
        {
            throw new Exception();
            //var allpositions = new BoundingBox(a, a + new Vector3(w - 1, h - 1, 0)).GetBox();
            //var finalPositions = new List<Vector3>();
            //foreach (var po in allpositions)
            //    if (this.Town.GetZoneAt(po) == null && ZoneNew.IsPositionValid(this.Town.Map, po))
            //        finalPositions.Add(po);
            ////var stockpile = new Stockpile(a, w, h);
            //if (!finalPositions.IsConnected())
            //    return null;
            //var stockpile = new Stockpile(this.Town, finalPositions);
            //this.RegisterZone(stockpile);
            //return stockpile;
        }
        internal override void OnBlocksChanged(IEnumerable<IntVec3> positions)
        {
            foreach (var s in this.Stockpiles)
                foreach(var pos in positions)
                    s.Value.OnBlockChanged(pos);
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                //case Components.Message.Types.BlockChanged:
                //    foreach (var s in this.Stockpiles)
                //        s.Value.OnBlockChanged((Vector3)e.Parameters[1]);

                //    break;

                case Components.Message.Types.BlockEntityAdded:
                    var binEntity = e.Parameters[0] as IStorage;
                    if (binEntity != null)
                        
                        this.Storages.Add((Vector3)e.Parameters[1]);
                    break;

                case Components.Message.Types.BlockEntityRemoved:
                    var entity = e.Parameters[0] as IStorage;
                    var global = (IntVec3)e.Parameters[1];
                    if (entity != null)
                    {
                       
                        if (!(entity is IStorage))
                            throw new Exception();
                        if (!this.Storages.Contains(global))
                        {
                            //throw new Exception();
                            Client.Instance.Log.Write("Tried to remove nonexistant storage");
                            break;
                        }
                        this.Storages.Remove(global);
                    }
                    break;

          
                default:
                    foreach (var s in this.Stockpiles)
                        s.Value.OnGameEvent(e);
                    break;
            }
            base.OnGameEvent(e);
        }

        public Dictionary<int, Stockpile> Stockpiles = new();
       
        public HashSet<Vector3> Storages = new();

        public void RegisterZone(Stockpile stockpile)
        {
            if (stockpile.ID == 0)
                stockpile.ID = this.NextStockpileID;
            this.Stockpiles.Add(stockpile.ID, stockpile);
            this.Town.Map.EventOccured(Components.Message.Types.StockpileCreated, stockpile);
            if (this.Town.Map.Net is Client)
                FloatingText.Manager.Create(() => stockpile.Positions.First(), "Stockpile created", ft => ft.Font = UIManager.FontBold);
        }
        public bool RemoveStockpile(int stockpileID)
        {
            //Stockpile stockpile;
            //if (this.Stockpiles.TryGetValue(stockpileID, out stockpile))
            //    this.Stockpiles.Remove(stockpileID);

            Stockpile s;
            if (this.RemoveStockpile(stockpileID, out s))
            {
                if (this.Town.Map.Net is Client)
                    FloatingText.Manager.Create(() => s.Positions.First(), "Stockpile deleted", ft => ft.Font = UIManager.FontBold);
                return true;
            }
            return false;
            //{
            //    this.Map.EventOccured(Components.Message.Types.StockpileDeleted, s);
            //    return true;
            //}
            //return false;
        }
        public bool RemoveStockpile(int stockpileID, out Stockpile deleted)
        {
            //Stockpile stockpile;
            //if (this.Stockpiles.TryGetValue(stockpileID, out stockpile))
            //    this.Stockpiles.Remove(stockpileID);
            var removed = this.Stockpiles.TryGetValue(stockpileID, out deleted);
            if (removed)
            {
                this.Stockpiles.Remove(stockpileID);
                this.Town.Map.EventOccured(Components.Message.Types.StockpileDeleted, deleted);

                return true;
            }
            return false;
        }

        //public override List<SaveTag> Save()
        protected override void AddSaveData(SaveTag tag)
        {
            //List<SaveTag> save = new List<SaveTag>();
            tag.Add(this._StockpileSequence.Save("IDSequence"));
            var stockpiles = new SaveTag(SaveTag.Types.List, "Stockpiles", SaveTag.Types.Compound);
            foreach (var stockpile in this.Stockpiles)
                stockpiles.Add(new SaveTag(SaveTag.Types.Compound, "", stockpile.Value.Save()));
            tag.Add(stockpiles);

            var storages = new SaveTag(SaveTag.Types.List, "Storages", SaveTag.Types.Compound);
            foreach (var storage in this.Storages)
                //storages.Add(new SaveTag(SaveTag.Types.Vector3, "", storage));s
                storages.Add(storage.SaveOld());
            tag.Add(storages);
        }
        public override void Load(SaveTag tag)
        {
            //tag.TryGetTagValue<int>("IDSequence", v => this._StockpileSequence = v);
            //var list = new List<SaveTag>();
            //if (tag.TryGetTagValue("Stockpiles", out list))
            //    foreach (var stag in list)
            //    {
            //        var s = new Stockpile(this.Town, stag);
            //        this.Stockpiles.Add(s.ID, s);
            //    }

            var storagesTag = new List<SaveTag>();
            if (tag.TryGetTagValue("Storages", out storagesTag))
                foreach (var s in storagesTag)
                    this.Storages.Add(s.LoadVector3());
        }
        public override void Write(BinaryWriter w)
        {
            //w.Write(_StockpileSequence); // do i need to sync this?
            //w.Write(this.Stockpiles.Count);
            //foreach (var item in this.Stockpiles)
            //    item.Value.Write(w);
        }
        public override void Read(BinaryReader r)
        {
            //_StockpileSequence = r.ReadInt32();
            //var count = r.ReadInt32();
            //for (int i = 0; i < count; i++)
            //{
            //    var item = new Stockpile(this.Town, r);
            //    this.Stockpiles.Add(item.ID, item);
            //}
        }

        public override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera cam)
        {
            if (!cam.DrawZones)
                return;
            foreach (var s in this.Stockpiles)
                s.Value.DrawBeforeWorld(sb, map, cam);
            //s.Value.DrawWorld(sb, map, cam);
            //cam.DrawGrid(sb, map, s.Value.Positions);
        }

        public Stockpile GetStockpile(Vector3 pos)
        {
            foreach (var s in this.Stockpiles)
                if (s.Value.Positions.Contains(pos))
                    return s.Value;
            return null;
        }
       

        [Obsolete]
        public bool IsItemAtBestStockpile(GameObject item)
        {
            //var currentStockpile = this.QueryPosition(item.GridCell.Below()) as Stockpile;
            var currentStockpile = this.Stockpiles.FirstOrDefault(s => s.Value.Contains(item)).Value;
            if (currentStockpile == null)
                return false;
            var betterStockpile = this.Stockpiles.Values.Except(new Stockpile[] { currentStockpile })
                .Where(s => s.CanAccept(item) && s.Priority > currentStockpile.Priority)
                .OrderByDescending(s => s.Priority)
                .FirstOrDefault();
            return betterStockpile == null;
        }

        internal void Unreserve(GameObject actor)
        {
            foreach (var s in this.Stockpiles)
                s.Value.Unreserve(actor);
        }

        public override void GetManagementInterface(TargetArgs t, WindowTargetManagement inter)
        {
            var sp = this.GetStockpile(t.Global);
            if (sp == null)
                return;
            var filters = new StockpileFiltersUIAdvanced(sp).ToWindow(sp.Name + " Filters");
            inter.Tag = sp;
            inter.HideAction = () => filters.Hide();
            inter.PanelInfo.AddControls(new Label("HI, I'm " + sp.Name));
            inter.PanelActions.AddControls(
                new Button("Edit")
                {
                    LeftClickAction = () => filters.Toggle()// sp.ToggleWindow()
                });
        }

        [Obsolete]
        internal bool IsValidStorage(GameObject item, TargetArgs destination)
        {
            if (destination.HasObject && (destination.Object == null || !destination.Object.IsSpawned || destination.Object.Full))
                return false;
            var global = destination.Global;
            var targetStockpile = this.GetStockpile(global - Vector3.UnitZ);
            if (targetStockpile == null)
                return false;
            return targetStockpile.IsValidStorage(item as Entity, new TargetArgs(global - Vector3.UnitZ), item.StackSize);

            //return targetStockpile.IsValidStorage(itemID, global - Vector3.UnitZ);
        }
        internal bool IsValidStorage(GameObject hauled, TargetArgs destination, int amount)
        {
            //var targetStockpile = this.GetStockpile(destination.Global - Vector3.UnitZ);
            //if (targetStockpile == null)
            //    return false;
            //return targetStockpile.IsValidStorage(hauled, destination.Global - Vector3.UnitZ, amount);
            return this.IsValidStorage(hauled.ID, destination.Global, amount);
        }
        internal bool IsValidStorage(int hauledID, Vector3 global, int amount)
        {
            var targetStockpile = this.GetStockpile(global - Vector3.UnitZ);
            if (targetStockpile == null)
                return false;
            return targetStockpile.IsValidStorage(hauledID, global - Vector3.UnitZ, amount);
        }

        internal bool IsValidStorage(int hauledID, TargetArgs position, int amount)
        {
            throw new NotImplementedException();
            
        }

        public IEnumerable<(Entity item, int amount)> FindItems(Func<Entity, bool> filter, int amount)
        {
            foreach (var i in this.Tracker.FindItems(filter, amount))
                yield return i;
        }
        //internal void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        //{
        //    this.Tracker.DrawUI(sb, camera);
        //}

        internal static void OnHudCreated(Hud hud)
        {
            var ui = new UIStockpileInventoryIcons(); //{ Location = hud.PartyFrame.BottomLeft };
            //ui.Location = ui.LeftCenterScreen;
            hud.AddControls(ui);
            //hud.AddControls(new UIStockpileInventoryIcons() { Location = hud.PartyFrame.BottomLeft });
        }
        public override IContextable QueryPosition(Vector3 global)
        {
            return this.GetStockpile(global);
        }
        public override ISelectable QuerySelectable(TargetArgs target)
        {
            var global = target.Global;
            return this.GetStockpile(global);
        }
        //internal override IEnumerable<Tuple<string, Action>> OnQuickMenuCreated()
        //{
        //    yield return new Tuple<string, Action>("Stockpile", () => ZoneNew.Edit(typeof(Stockpile)));
        //}

        public IEnumerable<Stockpile> GetStockpilesByPriority()
        {
            return this.Stockpiles.Values.OrderByDescending(s => s.Priority);
            return this.Stockpiles.OrderByDescending(kvpair => kvpair.Value.Priority).Select(kv => kv.Value);
        }
        [Obsolete]
        public IEnumerable<IStorage> GetStoragesByPriority()
        {
            //var list = new List<IStorage>(this.Storages.Keys).Concat(this.Stockpiles.Cast<IStorage>());
            //var list = new List<IStorage>(this.Storages.Values).Concat(this.Stockpiles.Cast<IStorage>());

            var containers = this.Storages.Select(g => this.Town.Map.GetBlockEntity(g) as IStorage);
            var stockpiles = this.Stockpiles.Values.Cast<IStorage>();
            return containers.Concat(stockpiles).OrderByDescending(i => i.Settings.Priority);
            var list = new List<IStorage>(containers.Concat(stockpiles));

            return list.OrderByDescending(i => i.Settings.Priority);

            //return this.Storages.OrderByDescending(kvpair => kvpair.Value.Settings.Priority).Select(kv => kv.Value);
        }

        static public TargetArgs GetBestStoragePlace(Actor actor, Entity item, out int maxamount)
        {
            var storages = item.Map.Town.StockpileManager.GetStoragesByPriority();
            maxamount = 0;
            foreach (var s in storages)
                if (s.Accepts(item))
                    return s.GetBestHaulTarget(actor, item);
            return null;
        }
        static public bool GetBestStoragePlace(Actor actor, Entity item, out TargetArgs target)
        {
            var storages = item.Map.Town.StockpileManager.GetStoragesByPriority();
            foreach (var s in storages)
            {
                if (s.Accepts(item))
                    foreach (var spot in s.GetPotentialHaulTargets(actor, item, out int maxamount))
                        if (item.StackSize <= maxamount)
                        {
                            target = spot.Key;
                            target.Map = actor.Map;
                            return true;
                        }
            }
            target = null;
            return false;
        }
        [Obsolete]
        static public IEnumerable<TargetArgs> GetMoreValidStoragePlaces(Actor actor, Entity item, Vector3 center)
        {
            //var storages = item.Map.Town.StockpileManager.GetStoragesByPriority();
            var storage = item.Map.Town.StockpileManager.GetStockpile(center.Below());
            foreach (var spot in storage.GetPotentialHaulTargets(actor, item))
                yield return spot;
        }
        static public Dictionary<TargetArgs, int> GetAllValidStoragePlaces(Actor actor, Entity item, out int maxamount)
        {
            Dictionary<TargetArgs, int> all = new Dictionary<TargetArgs, int>();
            var storages = item.Map.Town.StockpileManager.GetStoragesByPriority();
            maxamount = 0;
            foreach (var s in storages)
            {
                if (s.Accepts(item))
                    foreach (var spot in s.GetPotentialHaulTargets(actor, item, out maxamount))
                        if(actor.CanReserve(spot.Key.Global))
                            all.Add(spot.Key, spot.Value);
            }
            return all;
        }
        
        public List<GameObject> GetAllStorageContents()
        {
            var list = new List<GameObject>();
            foreach(var position in this.Storages)
            {
                var storage = (this.Map.GetBlockEntity(position) as BlockStorage.BlockStorageEntity).Contents;
                list.AddRange(storage);
            }
            return list;
        }
    }
}
