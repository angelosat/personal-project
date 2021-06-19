using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.AI;
using Start_a_Town_.GameModes;
using Start_a_Town_.Modules.Towns.Housing;
using Start_a_Town_.Towns.Farming;
using Start_a_Town_.Towns.Forestry;
using Start_a_Town_.Towns.Digging;
using Start_a_Town_.Towns.Crafting;
using Start_a_Town_.Towns.Stockpiles;

namespace Start_a_Town_.Towns
{
    public class TownJobCollection : Dictionary<int, TownJob>// List<TownJob>
    {
        static int _jobID = 1;
        public static int JobID
        {
            get { return _jobID;}//++; }
            set { _jobID = value; }
        }
        public new void Add(TownJob job)
        {
            TryInstantiate(job);
           // base.Add(job);
            //base.Add(job.ID, job);
            this[job.ID] = job;
        }

        private static bool TryInstantiate(TownJob job)
        {
            if (!job.Instantiated)
            { 
                job.ID = JobID;
                return true;
            }
            JobID = job.ID + 1;
            return false;
        }
        //public TownJobCollection()
        //    : base()
        //{

        //}
        //public TownJobCollection(int capacity)
        //    : base(capacity)
        //{

        //}
    }
    public partial class Town
    {
        int HouseSequence = 1;
        //int _StockpileSequence = 1;
        //int StockpileSequence { get { return _StockpileSequence++; } }
        public void AddStockpile(Stockpile stockpile)
        {
            if (stockpile.ID == 0)
                stockpile.ID = this.StockpileManager.StockpileSequence;
            stockpile.Town = this;
            this.Stockpiles.Add(stockpile.ID, stockpile);
        }
        public bool DeleteStockpile(int stockpileID)
        {
            //Stockpile stockpile;
            //if (this.Stockpiles.TryGetValue(stockpileID, out stockpile))
            //    this.Stockpiles.Remove(stockpileID);

            Stockpile s;
            return this.DeleteStockpile(stockpileID, out s);
            //{
            //    this.Map.EventOccured(Components.Message.Types.StockpileDeleted, s);
            //    return true;
            //}
            //return false;
        }
        public bool DeleteStockpile(int stockpileID, out Stockpile deleted)
        {
            //Stockpile stockpile;
            //if (this.Stockpiles.TryGetValue(stockpileID, out stockpile))
            //    this.Stockpiles.Remove(stockpileID);
            var removed = this.Stockpiles.TryGetValue(stockpileID, out deleted);
            if (removed)
            {
                this.Stockpiles.Remove(stockpileID);
                this.Map.EventOccured(Components.Message.Types.StockpileDeleted, deleted);
                return true;
            }
            return false;
        }

        public void AddHouse(House house)
        {
            house.ID = this.HouseSequence++;
            house.Name = string.IsNullOrWhiteSpace(house.Name) ? GetDefaultHouseName(house) : house.Name;
            //house.Name = string.IsNullOrWhiteSpace(house.Name) ? "house" + house.ID.ToString("00#") : house.Name;
            this.Houses.Add(house.ID, house);
            this.Map.EventOccured(Message.Types.HousesUpdated);
        }

        public string GetDefaultHouseName(House house)
        {
            return "House_" + house.ID.ToString("00#");
        }
        public string GetDefaultHouseName()
        {
            return "House_" + this.HouseSequence.ToString("00#");
        }

        public bool RemoveHouse(House house)
        {
            var h = this.Houses.Remove(house.ID);
            this.Map.EventOccured(Message.Types.HousesUpdated);
            return h;
        }
        public bool RemoveHouse(int houseID)
        {
            var h = this.Houses.Remove(houseID);
            this.Map.EventOccured(Message.Types.HousesUpdated);
            return h;            
        }
        public bool RemoveHouse(int houseID, out House house)
        {
            house = this.Houses[houseID];
            var h = this.Houses.Remove(houseID);
            this.Map.EventOccured(Message.Types.HousesUpdated);
            return h;
        }
        public House GetHouseAt(Vector3 global)
        {
            //foreach (var house in this.Houses.Values)
            //    if (house.Enterior.Contains(global))
            //        return house;
            //return null;
            foreach (var house in this.Houses.Values)
            {
                //var contain = house.Box.Contains(global.Round());
                var contains = house.Contains(global);
                if (contains)// != ContainmentType.Disjoint)
                    return house;
                //if (house.Box.Contains(global))
                //    return house;
            }
            return null;
        }
        public void ClearHouses()
        {
            this.Houses.Clear();
            this.Map.EventOccured(Message.Types.HousesUpdated);
        }
        public List<House> GetHouses()
        {
            return this.Houses.Values.ToList();
        }
        public House GetHouse(int houseID)
        {
            return this.Houses[houseID];
        }
        public bool RenameHouse(int houseID, string name)
        {
            var h = GetHouse(houseID);
            if (h == null)
                return false;
            h.Name = name;
            this.Map.EventOccured(Message.Types.HousesUpdated);
            return true;
        }
        Dictionary<int, House> Houses = new Dictionary<int, House>();// new List<Stockpile>();
        public Dictionary<int, Stockpile> Stockpiles = new Dictionary<int,Stockpile>();// new List<Stockpile>();

        int AgentSequence = 1;
        public List<GameObject> Agents = new List<GameObject>();

        StockpileManager StockpileManager;
        public ConstructionsManager ConstructionsManager;
        public FarmingManager FarmingManager;
        public ChoppingManager ChoppingManager;
        public DiggingManager DiggingManager;
        public CraftingManager CraftingManager;
        public LaborsManager AIManager;

        public List<TownComponent> TownComponents = new List<TownComponent>();

        //public UITownWindow UITownWindow;

        public IMap Map;
        public Net.IObjectProvider Net { get { return this.Map.Net; } }

        HashSet<Vector2> OwnedChunks = new HashSet<Vector2>() { Vector2.Zero };
        public List<Chunk> GetOwnedChunks()
        {
            //return OwnedChunks.ToList();
            //return (from vector in this.OwnedChunks where this.Map.ActiveChunks.ContainsKey(vector) select this.Map.ActiveChunks[vector]).ToList();
            return (from vector in this.OwnedChunks where this.Map.ChunkExists(vector) select this.Map.GetChunkAt(vector)).ToList();

        }

        public List<AIJob> AIJobs = new List<AIJob>();

        public TownJobCollection Jobs { get; set; }
        public HashSet<TownJob> JobHistory { get; set; }
        int JobHistorySize = 256;
        public Town(IMap map)//IObjectProvider net)
        {
            this.Map = map;
            this.Jobs = new TownJobCollection();
            this.JobHistory = new HashSet<TownJob>();

            //UITownWindow = new UITownWindow(this);

            // net.GameEvent += net_GameEvent;
            this.StockpileManager = new StockpileManager(this);
            this.ConstructionsManager = new ConstructionsManager(this);
            this.FarmingManager = new FarmingManager(this);
            this.ChoppingManager = new ChoppingManager(this);
            this.DiggingManager = new DiggingManager(this);
            this.CraftingManager = new CraftingManager(this);
            this.AIManager = new LaborsManager(this);

            this.TownComponents.AddRange(new TownComponent[]{
                this.StockpileManager,
                this.ConstructionsManager,
                this.FarmingManager,
                this.ChoppingManager,
                this.DiggingManager,
                this.CraftingManager,
                this.AIManager
            });
        }

      

        public void Update()
        {
            //UpdateStockpiles();
            //this.StockpileManager.Update();
            //this.ConstructionsManager.Update();
            //this.FarmingManager.Update();
            foreach (var comp in this.TownComponents)
                comp.Update();
        }

        public void HandleGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.EntitySpawned:
                    var entity = e.Parameters[0] as GameObject;
                    AddAgent(entity);
                    break;

                case Message.Types.EntityDespawned:
                    entity = e.Parameters[0] as GameObject;
                    RemoveAgent(entity);
                    break;

                default:
                    break;
            }
            foreach (var comp in this.TownComponents)
                comp.OnGameEvent(e);
        }

        private void AddAgent(GameObject entity)
        {
            if (entity.HasComponent<AIComponent>())
            {
                //entity.Name += " " + this.AgentSequence.ToString();
                this.AgentSequence++;
                this.Agents.Add(entity);
                this.Map.EventOccured(Message.Types.AgentsUpdated);
            }
        }

        private void RemoveAgent(GameObject entity)
        {
            if (entity.HasComponent<AIComponent>())
            {
                this.Agents.Remove(entity);
                this.Map.EventOccured(Message.Types.AgentsUpdated);
            }
        }

        //public void HandleGameEvent(GameEvent e)
        //{
        //    switch (e.Type)
        //    {
        //        case Message.Types.ScriptFinished:
        //            GameObject actor = e.Parameters[0] as GameObject;
        //            TargetArgs target = e.Parameters[1] as TargetArgs;
        //            Script.Types script = (Script.Types)e.Parameters[2];
        //            foreach (var job in this.Jobs.Values.ToList())
        //            {
        //                var found = job.Steps.FirstOrDefault(step => step.Target.Object == target.Object && step.Script == script);
        //                if (found.IsNull())
        //                    return;
        //                job.Steps.Remove(found);
        //                e.Net.EventOccured(Components.Message.Types.JobStepFinished, actor, target.Object, script);
        //                job.FinishedSteps.Add(found);
        //                if (job.Steps.Count == 0)
        //                {
        //                    //job.State = TownJob.States.Finished;
        //                    job.Complete();
        //                    foreach (var worker in job.Workers)
        //                        worker.GetComponent<WorkerComponent>().Jobs.Remove(job);
        //                    this.Jobs.Remove(job.ID);
        //                    this.JobHistory.Add(job);
        //                    TownJobsWindow.Instance.Refresh(this);
        //                  //  e.Net.GameEvent -= net_GameEvent;
        //                }
        //            }
        //            break;

        //        //case Message.Types.BlockChanged:
        //        //    this.ConstructionsManager.HandleBlock(e.Parameters[0] as IMap, (Vector3)e.Parameters[1]);
        //        //    break;

        //        default:

        //            break;
        //    }
        //    return;
        //}

        public void DrawWorld(MySpriteBatch sb, Map map, Camera cam)
        {
            foreach (var item in this.Stockpiles)
                item.Value.DrawWorld(sb, map, cam);
        }

        //private void UpdateStockpiles()
        //{
        //    //foreach (var item in this.Stockpiles)
        //    //    item.Value.Update();
        //}


        internal void InvalidateBlock(Vector3 global)
        {
            foreach (var house in this.Houses.Values.ToList())
            {
                if (this.Map.GetBlock(global) == Block.Air)
                {
                    var c = house.Box.Contains(global);
                    if (c == ContainmentType.Contains || c == ContainmentType.Intersects)
                    {
                        var outerwalls = house.NorthWalls.Union(house.SouthWalls).Union(house.EastWalls).Union(house.WestWalls).ToList();
                        if (outerwalls.Contains(global))
                            this.RemoveHouse(house);
                    }
                }
                if (house.Enterior.Contains(global))
                    house.InvalidateBlock(global);
                if (house.Walls.Contains(global))
                    house.InvalidateBlock(global);

            }


            //if (this.Map.GetBlock(global) == Block.Air)
            //    foreach (var house in this.Houses.Values.ToList())
            //    {
            //        var c = house.Box.Contains(global);
            //        if (c == ContainmentType.Contains || c == ContainmentType.Intersects)
            //        {
            //            var outerwalls = house.NorthWalls.Union(house.SouthWalls).Union(house.EastWalls).Union(house.WestWalls).ToList();
            //            if (outerwalls.Contains(global))
            //                this.RemoveHouse(house);
            //        }
            //    }

            //foreach (var house in this.Houses.Values)
            //    house.InvalidateBlock(global);
        }

        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            var housestag = new SaveTag(SaveTag.Types.List, "Houses", SaveTag.Types.Compound);
            foreach (var house in this.Houses)
                housestag.Add(house.Value.Save("House"));
            tag.Add(housestag);

            var compsTag = new SaveTag(SaveTag.Types.Compound, "Components");
            foreach (var comp in this.TownComponents)
                compsTag.Add(new SaveTag(SaveTag.Types.Compound, comp.Name, comp.Save()));
            tag.Add(compsTag);

            return tag;
        }
        public void Load(SaveTag save)
        {
            List<SaveTag> housesTag;
            if (save.TryGetTagValue("Houses", out housesTag))
                foreach (var tag in housesTag)
                    this.AddHouse(new House(this, tag));
            Dictionary<string, SaveTag> compsTag = new Dictionary<string, SaveTag>();
            if (save.TryGetTagValue("Components", out compsTag))
                foreach (var tag in compsTag)
                {
                    var comp = this.TownComponents.FirstOrDefault(c => c.Name == tag.Key);
                    if (comp != null)
                        comp.Load(tag.Value);
                }
            //List<SaveTag> compsTag = new List<SaveTag>();
            //if (save.TryGetTagValue("Components", out compsTag))
            //    foreach (var tag in compsTag)
            //        this.TownComponents.FirstOrDefault(c => c.Name == tag.Name).Load(tag);
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Houses.Count);
            foreach (var h in this.Houses.Values)
                h.Write(w);

            foreach (var comp in this.TownComponents)
                comp.Write(w);
        }

        public void Read(BinaryReader r)
        {
            var hcount = r.ReadInt32();
            for (int i = 0; i < hcount; i++)
            {
                this.AddHouse(new House(this, r));
            }

            foreach (var comp in this.TownComponents)
                comp.Read(r);
        }


        internal void HandlePacket(Server server, Packet msg)
        {
            foreach (var comp in this.TownComponents)
                comp.HandlePacket(server, msg);
        }

        internal void HandlePacket(Client client, Packet msg)
        {
            foreach (var comp in this.TownComponents)
                comp.HandlePacket(client, msg);
        }

        public List<Zone> GetZones()
        {
            return new List<Zone>();
            //var zones = this.FarmingManager.Farmlands.Values.Cast<Zone>().Concat(this.Stockpiles.Values).ToList();
            var zones = this.Stockpiles.Values.Cast<Zone>().ToList();
            return zones;
        }

        internal void OnGameEvent(GameEvent e)
        {
            foreach (var comp in this.TownComponents)
                comp.OnGameEvent(e);
            //switch(e.Type)
            //{
            //    case Message.Types.EntityDespawned:
            //        break;

            //    default:
            //        break;
            //}
        }

        internal bool JobComplete(AIJob job, GameObject parent)
        {
            if (!job.Instructions.Last().Completed)
                throw new Exception();
            if (job.Source == null)
                throw new Exception();
            if (this.AIJobs.Remove(job.Source))
            {
                //if (parent.Net is Client)
                //    throw new Exception();
                var workAward = 50;
                var server = parent.Net as Server;
                server.Enqueue(PacketType.NeedModifyValue, Network.Serialize(w =>
                {
                    w.Write(parent.InstanceID);
                    w.Write((int)Components.Needs.Need.Types.Work);
                    w.Write(workAward);
                }));

                //Start_a_Town_.AI.AIManager.JobComplete(parent, job.Source.Description);
                return true;
                //Start_a_Town_.AI.AIManager.JobComplete(parent, job.Source.Description);
            }
            return false;
        }

        internal void AddJob(AIJob job)
        {
            if (this.Net is Client)
                return;

            if (job.Instructions.Count == 0)
                throw new Exception();
            if (this.AIJobs.Contains(job))
                throw new Exception();
            job.Description.Insert(0, "Complete Job: ");
            this.AIJobs.Add(job);
            this.Map.EventOccured(Message.Types.JobsUpdated, job);
        }

        internal void RemoveJob(AIJob job)
        {
            if (this.Net is Client)
                return;
            this.AIJobs.Remove(job);
            job.Cancel();
        }
    }
}
