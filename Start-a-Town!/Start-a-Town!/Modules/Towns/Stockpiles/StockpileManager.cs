using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns
{
    public class StockpileManager : TownComponent
    {
        public override string Name => "Stockpiles";

        readonly StockpileContentTracker Tracker;

        internal Stockpile GetStockpile(int stID)
        {
            return this.Stockpiles[stID];
        }
        int _stockpileSequence = 1;
        public int NextStockpileID => _stockpileSequence++;
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
           
            if (this.Town.Map.Net is Client)
                return;

        }


        internal static void Init()
        {
            Stockpile.Init();
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
                            Client.Instance.Log.Write("Tried to remove nonexistant storage");
                            break;
                        }
                        this.Storages.Remove(global);
                    }
                    break;
          
                default:
                    break;
            }
            base.OnGameEvent(e);
        }

        public Dictionary<int, Stockpile> Stockpiles = new();
       
        public HashSet<Vector3> Storages = new();

        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this._stockpileSequence.Save("IDSequence"));
            var stockpiles = new SaveTag(SaveTag.Types.List, "Stockpiles", SaveTag.Types.Compound);
            foreach (var stockpile in this.Stockpiles)
                stockpiles.Add(new SaveTag(SaveTag.Types.Compound, "", stockpile.Value.Save()));
            tag.Add(stockpiles);

            var storages = new SaveTag(SaveTag.Types.List, "Storages", SaveTag.Types.Compound);
            foreach (var storage in this.Storages)
                storages.Add(storage.SaveOld());
            tag.Add(storages);
        }
        public override void Load(SaveTag tag)
        {
            this.Storages.LoadVectors(tag["Storages"]);
            //if (tag.TryGetTagValue("Storages", out List<SaveTag> storagesTag))
            //    foreach (var s in storagesTag)
            //        this.Storages.Add(s.LoadVector3());
        }

        public override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera cam)
        {
            if (!cam.DrawZones)
                return;
            foreach (var s in this.Stockpiles)
                s.Value.DrawBeforeWorld(sb, map, cam);
        }

        public Stockpile GetStockpile(Vector3 pos)
        {
            foreach (var s in this.Stockpiles)
                if (s.Value.Contains(pos))
                    return s.Value;
            return null;
        }

        [Obsolete]
        public bool IsItemAtBestStockpile(GameObject item)
        {
            var currentStockpile = this.Stockpiles.FirstOrDefault(s => s.Value.Contains(item)).Value;
            if (currentStockpile == null)
                return false;
            var betterStockpile = this.Stockpiles.Values.Except(new Stockpile[] { currentStockpile })
                .Where(s => s.CanAccept(item) && s.Priority > currentStockpile.Priority)
                .OrderByDescending(s => s.Priority)
                .FirstOrDefault();
            return betterStockpile == null;
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
       
        internal static void OnHudCreated(Hud hud)
        {
            var ui = new UIStockpileInventoryIcons();
            hud.AddControls(ui);
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
        
        public IEnumerable<Stockpile> GetStockpilesByPriority()
        {
            return this.Stockpiles.Values.OrderByDescending(s => s.Priority);
        }
        [Obsolete]
        public IEnumerable<IStorage> GetStoragesByPriority()
        {
            var containers = this.Storages.Select(g => this.Town.Map.GetBlockEntity(g) as IStorage);
            var stockpiles = this.Stockpiles.Values.Cast<IStorage>();
            return containers.Concat(stockpiles).OrderByDescending(i => i.Settings.Priority);
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
            var storage = item.Map.Town.StockpileManager.GetStockpile(center.Below());
            foreach (var spot in storage.GetPotentialHaulTargets(actor, item))
                yield return spot;
        }
    }
}
