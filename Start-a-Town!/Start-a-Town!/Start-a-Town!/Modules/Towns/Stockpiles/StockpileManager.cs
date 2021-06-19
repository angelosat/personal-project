using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.AI;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Stockpiles
{
    class StockpileManager : TownComponent
    {
        public override string Name
        {
            get
            {
                return "Stockpiles";
            }
        }

        int _StockpileSequence = 1;
        public int StockpileSequence { get { return _StockpileSequence++; } }
        //Town Town;
        //HashSet<GameObject> QueuedObjects = new HashSet<GameObject>();
        const float UpdateFrequency = 1; // per second
        float UpdateTimerMax = (float)Engine.TargetFps / UpdateFrequency;
        float UpdateTimer;

        //delegate void PacketHandler(IObjectProvider net, System.IO.BinaryReader r, Packet p);
        //static Dictionary<PacketType, PacketHandler> Handlers = new Dictionary<PacketType, PacketHandler>(){
        //    {PacketType.StockpileFilters, PacketStockpileFilters.Handle}
        //};

        public StockpileManager(Town town)
        {
            this.Town = town;
        }

        void GenerateWork()
        {
            
            List<GameObject> entitiesToInsert = new List<GameObject>();
        
            List<GameObject> allOwnedEntities = new List<GameObject>();
            foreach (var stockpile in this.Town.Stockpiles)
                allOwnedEntities.AddRange(stockpile.Value.GetContents());

            // get all entities in area around stockpile who aren't already owned by a stockpile
            // or get entities in town owned chunks?
            //var chunks = this.Town.GetOwnedChunks();
            //foreach (var chunk in chunks)
            //{
            var objs = this.Town.Map.GetObjects();// chunk.GetObjects();
                entitiesToInsert.AddRange(from entity in objs
                                          where entity.GetPhysics().Size != Components.ObjectSize.Immovable // == Components.ObjectSize.Haulable
                                          where !allOwnedEntities.Contains(entity)
                                          //where !this.QueuedObjects.Contains(entity)
                                          //where entity.HasComponent<Components.ReagentComponent>()
                                          select entity);
            //}

            foreach(var entity in entitiesToInsert)
            {
                // find closest valid stockpile
                //var sortedStockpiles = this.Town.Stockpiles.OrderBy(foo => Vector3.DistanceSquared(foo.Value.Begin, entity.Global));
                var sortedStockpiles = this.Town.Stockpiles.OrderBy(foo => Vector3.DistanceSquared(foo.Value.Positions.First(), entity.Global)); // TODO: this is shit

                foreach (var stockpile in sortedStockpiles)
                {
                    //if(this.QueuedObjects.Contains(entity))
                    //{
                    //    if (stockpile.Value.Positions.Contains(entity.Global))
                    //        this.QueuedObjects.Remove(entity);
                    //    continue;

                    //    //var i = new BoundingBox(stockpile.Value.Begin, stockpile.Value.End).Contains(entity.Global);
                    //    //if (i != ContainmentType.Disjoint)
                    //    //    this.QueuedObjects.Remove(entity); // if entity already in a stockpile, remove it from queue and ignore it
                    //    //continue;
                    //}

                    var existingOrder = stockpile.Value.PendingOrders.FirstOrDefault(i => i.Entity == entity);
                    if (existingOrder != null)
                    {
                        if (stockpile.Value.Positions.Contains(existingOrder.Entity.Global))
                        {
                            stockpile.Value.ReservedSpots.Remove(existingOrder.ReservedSpot);
                            stockpile.Value.PendingOrders.Remove(existingOrder);
                        }
                        continue;
                    }

                    TargetArgs target;
                    if (stockpile.Value.TryDeposit(entity, out target))
                    {
                        var targetPos = new TargetArgs(target);// - Vector3.UnitZ); // target the block underneath the empty space, because it provides the "drop" interaction for the client
                        var targetEntity = new TargetArgs(entity);
                        AIJob job = new AIJob();
                        //job.Instructions.Enqueue(new AIInstruction(targetEntity, new PickUp()));
                        //job.Instructions.Enqueue(new AIInstruction(targetPos, new DropCarriedSnap()));
                        job.AddStep(new AIInstruction(targetEntity, new PickUp()));
                        job.AddStep(new AIInstruction(targetPos, new DropCarriedSnap()));
                        job.Labor = AILabor.Hauler;
                        job.Description = "Haul " + entity.Name + " to " + stockpile.Value.Name;
                        this.Town.AddJob(job);

                        //this.QueuedObjects.Add(entity);
                        stockpile.Value.PendingOrders.Add(new Stockpile.HaulOrder(entity, targetPos.Global, job));
                        if(target.Type == TargetType.Position)
                            stockpile.Value.ReserveSpot(target.Global);
                        // TODO: here, mark the free spot as reserved as to not put further items in the same one
                    }

                }
            }
            // generate work and advertise to AI
        }

        public override void Update()
        {
            //if (this.Town.Map.GetNetwork() is Net.Client)
            //    return;

            // no need to do this since pickup interaction has the existing condition as a cancel condition
            //foreach (var item in this.QueuedObjects.ToList())
            //    if (!item.Exists)
            //        this.QueuedObjects.Remove(item);

            if(this.UpdateTimer > 0)
            {
                this.UpdateTimer--;
                return;
            }
            this.UpdateTimer = UpdateTimerMax;
            foreach (var item in this.Town.Stockpiles)
                item.Value.Update();
            //this.Town.Map.EventOccured(Components.Message.Types.StockpileUpdated);
            if (this.Town.Map.GetNetwork() is Net.Client)
                return;
            this.GenerateWork();
        }

        public override GroupBox GetInterface()
        {
            //return new TownUIOld(this.Town);
            //return new UITownWindowOld(this.Town);
            var ui = new StockpilesManagerUI(this.Town);
            //ui.Refresh();
            return ui;
        }

        public override void Handle(IObjectProvider net, Packet msg)
        {
            //BinaryReader r = new BinaryReader(new MemoryStream(msg.Decompressed));
            //PacketHandler handler;
            //if (Handlers.TryGetValue(msg.PacketType, out handler))
            //    handler(net, r, msg);
            //return;

            switch (msg.PacketType)
            {
                //case PacketType.StockpileFilters:
                //    msg.Payload.Deserialize(r =>
                //    {
                //        PacketStockpileFilters p = new PacketStockpileFilters(net, r);
                //        var stockpile = this.Town.Stockpiles[p.StockpileID];
                //        foreach (var f in p.Changes)
                //            stockpile.FilterToggle(f.Value, f.Key);
                //        net.Map.EventOccured(Components.Message.Types.StockpileUpdated, stockpile);
                //        var server = net as Server;
                //        if (server != null)
                //            server.Enqueue(PacketType.StockpileFilters, msg.Payload, SendType.OrderedReliable, true);
                //    });
                //    break;
                case PacketType.StockpileFilters:
                    msg.Payload.Deserialize(r =>
                    {
                        PacketStockpileFiltersNew.Handle(net, r, msg);
                        var server = net as Server;
                        if (server != null)
                            server.Enqueue(PacketType.StockpileFilters, msg.Payload, SendType.OrderedReliable, true);
                    });
                    break;

                case PacketType.StockpileFiltersCategories:
                    msg.Payload.Deserialize(r =>
                    {
                        PacketStockpileFiltersToggleCategories.Handle(net, r, msg);
                        net.Forward(msg);
                    });
                    break;

                case PacketType.StockpileRename:
                    msg.Payload.Deserialize(r =>
                    {
                        //var stockpile = this.Town.Stockpiles[r.ReadInt32()];
                        //var newname = r.ReadString();
                        int spID;
                        string name;
                        PacketStockpileRename.Read(r, out spID, out name);
                        var stockpile = this.Town.Stockpiles[spID];
                        stockpile.Name = name;
                        net.Map.EventOccured(Components.Message.Types.StockpileUpdated, stockpile);
                        
                        var server = net as Server;
                        if (server != null)
                            server.Enqueue(PacketType.StockpileRename, msg.Payload, SendType.OrderedReliable, true);
                    });
                    break;

                case PacketType.StockpileEdit:
                    msg.Payload.Deserialize(r =>
                    {
                        int spID;
                        Vector3 begin, end;
                        bool value;
                        PacketStockpileEdit.Read(r, out spID, out begin, out end, out value);
                        //Vector3 global;
                        //int w, h;
                        //bool value;
                        //PacketStockpileEdit.Read(r, out spID, out global, out w, out h, out value);
                        var stockpile = this.Town.Stockpiles[spID];

                        //stockpile.Edit(global, w, h, value);
                        stockpile.Edit(begin, end, value);

                        var server = net as Server;
                        if (server != null)
                            server.Enqueue(PacketType.StockpileEdit, msg.Payload, SendType.OrderedReliable, true);
                    });
                    break;

                case PacketType.StockpileDelete:
                    msg.Payload.Deserialize(r =>
                    {
                        int senderid;
                        int spID;
                        PacketStockpileDelete.Read(r, out senderid, out spID);
                        this.Town.DeleteStockpile(spID);
                        net.Forward(msg);
                    });
                    break;

                default:
                    break;
            }
        }

        //public void GenerateJobs()
        //{
        //    var all = this.Town.Map.GetObjects();
        //    var reagents =
        //        from obj in all
        //        where obj.HasComponent<Components.ReagentComponent>()
        //        select obj;

        //}

        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.BlockChanged:
                    foreach (var s in this.Town.Stockpiles)
                        s.Value.OnBlockChanged((Vector3)e.Parameters[1]);
                    break;

                default:
                    break;
            }
            base.OnGameEvent(e);
        }

        public Dictionary<int, Stockpile> Stockpiles { get { return this.Town.Stockpiles; } }

        public override List<SaveTag> Save()
        {
            List<SaveTag> save = new List<SaveTag>();
            save.Add(this._StockpileSequence.Save("IDSequence"));
            var stockpiles = new SaveTag(SaveTag.Types.List, "Stockpiles", SaveTag.Types.Compound);
            foreach (var stockpile in this.Stockpiles)
                stockpiles.Add(new SaveTag(SaveTag.Types.Compound, "", stockpile.Value.Save()));
            save.Add(stockpiles);
            return save;
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<int>("IDSequence", v => this._StockpileSequence = v);
            var list = new List<SaveTag>();
            if (tag.TryGetTagValue("Stockpiles", out list))
                foreach (var stag in list)
                {
                    var s = new Stockpile(this.Town, stag);
                    this.Stockpiles.Add(s.ID, s);
                }
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(_StockpileSequence); // do i need to sync this?
            w.Write(this.Stockpiles.Count);
            foreach (var item in this.Stockpiles)
                item.Value.Write(w);
        }
        public override void Read(BinaryReader r)
        {
            _StockpileSequence = r.ReadInt32();
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var item = new Stockpile(this.Town, r);
                this.Stockpiles.Add(item.ID, item);
            }
        }
    }
}
