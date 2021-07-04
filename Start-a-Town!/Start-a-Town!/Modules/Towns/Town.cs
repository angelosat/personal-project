using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.AI;
using Start_a_Town_.Towns.Housing;
using Start_a_Town_.Towns.Farming;
using Start_a_Town_.Towns.Digging;
using Start_a_Town_.Towns.Crafting;
using Start_a_Town_.Towns.Constructions;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    public partial class Town
    {
        UIQuickMenu QuickMenu;

        int HouseSequence = 1;
        
        public void AddHouse(House house)
        {
            house.ID = this.HouseSequence++;
            house.Name = string.IsNullOrWhiteSpace(house.Name) ? GetDefaultHouseName(house) : house.Name;
            //house.Name = string.IsNullOrWhiteSpace(house.Name) ? "house" + house.ID.ToString("00#") : house.Name;
            this.Houses.Add(house.ID, house);
            this.Map.EventOccured(Message.Types.HousesUpdated);
        }

        internal void OnTooltipCreated(Tooltip tooltip, TargetArgs targetArgs)
        {
            foreach (var c in this.TownComponents)
                c.OnTooltipCreated(tooltip, targetArgs);
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

        internal void Init()
        {
            this.RoomManager.Init();
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
        //public Dictionary<int, Stockpile> Stockpiles = new Dictionary<int,Stockpile>();// new List<Stockpile>();

        int AgentSequence = 1;
        //public List<GameObject> Agents = new List<GameObject>();
        public HashSet<int> Agents = new HashSet<int>();
        public IEnumerable<Actor> GetAgents()
        {
            return this.Agents.Select(id => this.Map.Net.GetNetworkObject(id) as Actor);//.ToList();
        }
        public GameObject GetNpc(Guid guid)
        {
            throw new NotImplementedException();
            //return this.Agents.Find(g => AIComponent.GetGuid(g) == guid);
        }

        public StockpileManager StockpileManager;
        public FarmingManager FarmingManager;
        public ZoneManager ZoneManager;
        public ConstructionsManager ConstructionsManager;
        public ChoppingManager ChoppingManager;
        public DiggingManager DiggingManager;
        public DesignationManager DesignationManager;
        public RoomManager RoomManager;
        public CraftingManager CraftingManager;
        public JobsManager JobsManager;
        //public HouseManager HousingManager;
        public ReservationManager ReservationManager;
        public TerrainManager TerrainManager;
        public WorkplaceManager ShopManager;
        public QuestsManager QuestManager;

        public List<TownComponent> TownComponents = new();

        

        //public UITownWindow UITownWindow;

        public IMap Map;
        public IObjectProvider Net { get { return this.Map.Net; } }

        readonly HashSet<Vector2> OwnedChunks = new() { Vector2.Zero };
        public List<Chunk> GetOwnedChunks()
        {
            //return OwnedChunks.ToList();
            //return (from vector in this.OwnedChunks where this.Map.ActiveChunks.ContainsKey(vector) select this.Map.ActiveChunks[vector]).ToList();
            return (from vector in this.OwnedChunks where this.Map.ChunkExists(vector) select this.Map.GetChunkAt(vector)).ToList();

        }

        public List<AIJob> AIJobs = new List<AIJob>();

        //public HashSet<TownJob> JobHistory { get; set; }
        int JobHistorySize = 256;

        //public Dictionary<Block.Types, HashSet<Vector3>> TownUtilities = new Dictionary<Block.Types, HashSet<Vector3>>();
        //public Dictionary<Blocks.BlockEntity, HashSet<Vector3>> TownUtilities = new Dictionary<Blocks.BlockEntity, HashSet<Vector3>>();
        public Dictionary<Utility.Types, HashSet<Vector3>> TownUtilitiesNew = new Dictionary<Utility.Types, HashSet<Vector3>>();

        public Town(IMap map)//IObjectProvider net)
        {
            this.Map = map;
            //this.JobHistory = new HashSet<TownJob>();


            //UITownWindow = new UITownWindow(this);

            // net.GameEvent += net_GameEvent;
            //this.StockpileManager = new(this);
            this.FarmingManager = new(this);
            this.ZoneManager = new(this);
            this.ConstructionsManager = new(this);
            this.ChoppingManager = new(this);
            this.DiggingManager = new(this);
            this.DesignationManager = new(this);
            this.RoomManager = new(this);

            this.CraftingManager = new(this);
            this.JobsManager = new(this);
            //this.HousingManager = new(this);
            this.ReservationManager = new(this);
            //this.RegionManager = new RegionManager(this.Map);
            this.TerrainManager = new(this);
            //this.ShopManager = new(this);
            this.ShopManager = new(this);
            this.QuestManager = new(this);

            this.TownComponents.AddRange(new TownComponent[]{
                //this.StockpileManager,
                this.FarmingManager,
                this.ZoneManager,
                this.ConstructionsManager,
                this.ChoppingManager,
                this.DiggingManager,
                this.DesignationManager,
                this.RoomManager,
                this.CraftingManager,
                this.JobsManager,
                //this.HousingManager,
                this.ReservationManager,
                this.TerrainManager,
                //this.ShopManager,
                this.ShopManager,
                this.QuestManager
            });

            

            
            var utilities = (Utility.Types[])Enum.GetValues(typeof(Utility.Types));
            foreach(var u in utilities)
            {
                this.TownUtilitiesNew[u] = new HashSet<Vector3>();
            }
        }

        public void Update()
        {
            foreach (var agent in this.Agents.ToArray())
                if (this.Net.GetNetworkObject(agent) == null)
                    this.Agents.Remove(agent);
            foreach (var comp in this.TownComponents)
                comp.Update();
        }

        public void HandleGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.EntitySpawned:
                    var entity = e.Parameters[0] as GameObject;
                    //AddAgent(entity);
                    break;

                case Message.Types.EntityDespawned:
                    entity = e.Parameters[0] as GameObject;
                    if(this.Agents.Contains(entity.RefID)) //TODO: dont dismiss despawned entities (they might be active outside the map)
                        RemoveAgent(entity);
                    break;

                default:
                    break;
            }
            foreach (var comp in this.TownComponents)
                comp.OnGameEvent(e);
        }
        public void AddUtility(Utility.Types type, Vector3 global)
        {
            this.TownUtilitiesNew[type].Add(global);
        }
        public void RemoveUtility(Utility.Types type, Vector3 global)
        {
            if (!this.TownUtilitiesNew[type].Remove(global))
            {
                //throw new Exception();
            }
            if (this.TownUtilitiesNew.Any(ut => ut.Value.Contains(global)))
                throw new Exception();
        }
        public IEnumerable<Vector3> GetUtilities(Utility.Types type)
        {
            return this.TownUtilitiesNew[type];
        }
        public bool HasUtility(Vector3 global, Utility.Types utility)
        {
            if (this.TownUtilitiesNew.TryGetValue(utility, out var list))
                return list.Contains(global);
            return false;
        }
        private void AddAgent(GameObject entity)
        {
            if (!entity.HasComponent<AIComponent>())
                throw new Exception();

                //entity.Name += " " + this.AgentSequence.ToString();
                this.AgentSequence++;
                //this.Agents.Add(entity.InstanceID);
            this.AddCitizen(entity.RefID);
                entity.Net.Log.Write(string.Format("{0} has joined the town!", entity.Name));
                this.Map.EventOccured(Message.Types.NpcsUpdated);
        }

        private void RemoveAgent(GameObject entity)
        {
            if (entity.HasComponent<AIComponent>())
            {
                //this.Agents.Remove(entity.InstanceID);
                this.RemoveCitizen(entity.RefID);
                entity.Net.Log.Write(string.Format("{0} was dismissed from the town!", entity.Name));
                this.Map.EventOccured(Message.Types.NpcsUpdated);
            }
        }

        public void ToggleAgent(GameObject entity)
        {
            if (!this.Agents.Contains(entity.RefID))
                this.AddAgent(entity);
            else
                this.RemoveAgent(entity);
        }
        internal void Tick()
        {
            foreach (var c in this.TownComponents)
                c.Tick();
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

        public void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera cam)
        {
            //this.StockpileManager.DrawBeforeWorld(sb, map, cam);
            //this.FarmingManager.DrawBeforeWorld(sb, map, cam);
            //this.ChoppingManager.DrawBeforeWorld(sb, map, cam);
            foreach(var comp in this.TownComponents)
                comp.DrawBeforeWorld(sb, map, cam);
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
                if (this.Map.GetBlock(global) == BlockDefOf.Air)
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
                //compsTag.Add(new SaveTag(SaveTag.Types.Compound, comp.Name, comp.Save()));
                compsTag.Add(comp.Save());

            tag.Add(compsTag);

            SaveAgents(tag);

            var utilitiesTag = new SaveTag(SaveTag.Types.List, "Utilities", SaveTag.Types.Compound);
            //foreach(var t in this.TownUtilities)
            //{
            //    var typeTag = new SaveTag(SaveTag.Types.Compound);
            //    typeTag.Add(new SaveTag(SaveTag.Types.Int, "Type", (int)t.Key));
            //    var positionsTag = t.Value.ToList().Save("Positions");
            //    typeTag.Add(positionsTag);
            //    utilitiesTag.Add(typeTag);
            //}
            foreach (var t in this.TownUtilitiesNew)
            {
                var typeTag = new SaveTag(SaveTag.Types.Compound);
                typeTag.Add(new SaveTag(SaveTag.Types.Int, "Type", (int)t.Key));
                var positionsTag = t.Value.ToList().Save("Positions");
                typeTag.Add(positionsTag);
                utilitiesTag.Add(typeTag);
            }
            tag.Add(utilitiesTag);

            return tag;
        }

        private void SaveAgents(SaveTag tag)
        {
            var agentsTag = new SaveTag(SaveTag.Types.List, "Agents", SaveTag.Types.Int);
            foreach (var a in this.Agents)
                agentsTag.Add(new SaveTag(SaveTag.Types.Int, "", a));
            tag.Add(agentsTag);
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


            LoadAgents(save);

            //return;
            List<SaveTag> utilitiesTag;
            if(save.TryGetTagValue("Utilities", out utilitiesTag))
            {
                foreach(var tag in utilitiesTag)
                {
                    //var blocktype = (Block.Types)(int)tag["Type"].Value;
                    //var positionList = new List<Vector3>().Load(tag["Positions"].Value as List<SaveTag>);// tag.LoadVector3();// tag["Positions"].Value as List<Vector3>;
                    //var hash = new HashSet<Vector3>(positionList);
                    //this.TownUtilities[blocktype] = hash;
                    var utilityType = (Utility.Types)(int)tag["Type"].Value;
                    var positionList = new List<Vector3>().Load(tag["Positions"].Value as List<SaveTag>);// tag.LoadVector3();// tag["Positions"].Value as List<Vector3>;
                    var hash = new HashSet<Vector3>(positionList);
                    this.TownUtilitiesNew[utilityType] = hash;
                }
            }

            foreach (var c in this.TownComponents)
                c.ResolveReferences();
        }

        private void LoadAgents(SaveTag save)
        {
            List<SaveTag> agentsTag;
            if (save.TryGetTagValue("Agents", out agentsTag))
                foreach (var bytes in agentsTag)
                {
                    var id = (int)bytes.Value;
                    //var agent = this.Map.Net.GetNetworkObject(id);
                    this.AddCitizen(id);
                }
        }

        private void AddCitizen(int id)
        {
            this.Agents.Add(id);
            foreach (var c in this.TownComponents)
                c.OnCitizenAdded(id);
        }

        private void RemoveCitizen(int id)
        {
            this.Agents.Remove(id);
            foreach (var c in this.TownComponents)
                c.OnCitizenRemoved(id);
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Houses.Count);
            foreach (var h in this.Houses.Values)
                h.Write(w);

            foreach (var comp in this.TownComponents)
                comp.Write(w);

            w.Write(this.Agents.Count);
            foreach (var a in this.Agents)
                w.Write(a);

            //foreach(var ut in this.TownUtilitiesNew)
            //{
            //    w.Write(ut.Value.ToList());
            //}
            foreach (var ut in Utility.All())
                w.Write(this.TownUtilitiesNew[ut].ToList());
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

            var acount = r.ReadInt32();
            for (int i = 0; i < acount; i++)
            {
                //this.Agents.Add(r.ReadInt32());
                this.AddCitizen(r.ReadInt32());
            }

            foreach (var ut in Utility.All())
                this.TownUtilitiesNew[ut] = new HashSet<Vector3>(r.ReadListVector3());
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
        internal void HandlePacket(IObjectProvider net, PacketType type, BinaryReader r)
        {
            foreach (var comp in this.TownComponents)
                comp.HandlePacket(net, type, r);
        }

        internal bool JobComplete(AIJob job, GameObject parent)
        {
            if (!job.Instructions.Last().Completed)
                throw new Exception();
            if (job.Source == null)
                throw new Exception();

            // temporary not working because i'm not storing jobs in the AIJOBS instance, but creating specific jobs for each actor
            if (this.AIJobs.Remove(job.Source))
            {
                //if (parent.Net is Client)
                //    throw new Exception();
                var workAward = 50;
                var server = parent.Net as Server;
                server.Enqueue(PacketType.NeedModifyValue, Network.Serialize(w =>
                {
                    w.Write(parent.RefID);
                    w.Write((int)Need.Types.Work);
                    w.Write(workAward);
                }));

                //Start_a_Town_.AI.AIManager.JobComplete(parent, job.Source.Description);
                return true;
                //Start_a_Town_.AI.AIManager.JobComplete(parent, job.Source.Description);
            }
            return false;
        }
        internal void JobStarted(GameObject agent, AIJob job)
        {
            AILog.SyncWrite(agent, "JOB STARTED: " + job.Description);
            //job.Initialize(agent); // TODO: maybe don't have 3582375 jobstart methods all over the place?
            var server = agent.Net as Server;
            if (server == null)
                return;
            server.OutgoingStream.Write((int)PacketType.AIJobStarted);
            server.OutgoingStream.Write(agent.RefID);
            server.OutgoingStream.Write(job.Description);
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

        public void GetContextActions(Vector3 pos, ContextArgs a)
        {
            var zone = this.QueryPosition(pos);
            if (zone.Count == 0)
                return;
            zone.First().GetContextActions(a);
        }

        public List<IContextable> QueryPosition(Vector3 pos)
        {
            var list = new List<IContextable>();
            //list.Add(this.StockpileManager.GetStockpile(pos));
            //return list.Where(item => item != null).ToList();
            foreach (var comp in this.TownComponents)
                list.Add(comp.QueryPosition(pos));
            return list.Where(t => t != null).ToList();
        }
        public IEnumerable<ISelectable> QuerySelectables(TargetArgs target)
        {
            while (true)
            {
                foreach (var comp in this.TownComponents)
                {
                    //var item = comp.QuerySelectable(target.Global);
                    var item = comp.QuerySelectable(target);
                    if (item != null)
                        yield return item;
                }
                yield return target;
            }
            //var list = new List<UI.ISelectable>();
            //foreach (var comp in this.TownComponents)
            //    list.Add(comp.QuerySelectable(pos));
            //return list.Where(t => t != null).ToList();
        }
        //public List<AIJob> GetJobsFor(GameObject actor)
        //{
        //    //var list = this.ConstructionsManager.FindJob(actor);
        //    //list = list.Concat(this.StockpileManager.FindJob(actor)).ToList();
        //    var list = new List<AIJob>();
        //    foreach (var comp in this.TownComponents)
        //        list = list.Concat(comp.FindJob(actor)).ToList();
        //    return list.Where(i => i != null).Where(i => i.Task.IsAvailable()).ToList();
        //}

        //public List<AI.AITask> GetTasks()
        //{
        //    var list = new List<AI.AITask>();
        //    foreach (var comp in this.TownComponents)
        //        comp.GetTasks(list);
        //    return list;
        //}

        internal void GetManagementInterface(TargetArgs t, UI.WindowTargetManagement inter)
        {
            foreach (var c in this.TownComponents)
                c.GetManagementInterface(t, inter);
        }



        internal ZoneNew GetZoneAt(Vector3 pos)
        {
            //return this.FarmingManager.GetZoneAt(pos) as ZoneNew ?? this.StockpileManager.GetStockpile(pos) as ZoneNew;
            return this.ZoneManager.GetZoneAt(pos);
        }

        internal void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        {
            foreach (var comp in this.TownComponents)
                comp.DrawUI(sb, this.Map, camera);
        }

        internal void SelectPosition(UI.UISelectedInfo uISelectedInfo, Vector3 vector3)
        {
            var queries = this.Map.Town.QueryPosition(vector3);
            if (this.Map.IsHidden(vector3))
            {
                uISelectedInfo.SetName("Unknown block");
            }
            else
            {
                var block = this.Map.GetBlock(vector3);
                uISelectedInfo.SetName(block.Name);
                block.Select(uISelectedInfo, this.Map, vector3);
            }
        }

        internal UIQuickMenu ToggleQuickMenu()
        {
            if(this.QuickMenu == null)
            {
                InitQuickMenu();
            }
            this.QuickMenu.Toggle();
            return this.QuickMenu;
        }

        private void InitQuickMenu()
        {
            var actions = new List<Tuple<string, Action>>();
            foreach (var comp in this.TownComponents)
                actions.AddRange(comp.OnQuickMenuCreated());
            actions.Add(new Tuple<string, Action>("Debug commands", UIDebugCommands.Refresh));
            //actions.Add(new Tuple<string, Action>("Spawn objects", () => UI.Editor.ObjectsWindow.Instance.Show()));
            actions.Add(new Tuple<string, Action>("Spawn objects", () => UI.Editor.ObjectsWindowDefs.Instance.Show()));
            actions.Add(new Tuple<string, Action>("Edit blocks", () => UI.Editor.TerrainWindow.Instance.Show()));

            this.QuickMenu = new UIQuickMenu();
            this.QuickMenu.AddItems(actions);
            this.QuickMenu.Location = UIManager.Mouse;
        }


        //internal void Select(GameObject gameObject, UISelectedInfo info)
        //{
        //    foreach (var comp in this.TownComponents)
        //    {
        //        comp.UpdateQuickButtons();
        //        comp.OnSelect(gameObject, info);
        //    }
        //}
        public IEnumerable<(string name, Action action)> GetInfoTabs(ISelectable selected)
        {
            foreach (var comp in this.TownComponents)
            {
                //comp.OnTargetSelected(info, target);
                foreach (var i in comp.GetInfoTabs(selected))
                    yield return i;
            }
        }
        internal void Select(ISelectable target, UISelectedInfo info)
        {
            foreach (var comp in this.TownComponents)
            {
                comp.UpdateQuickButtons();
                comp.OnTargetSelected(info, target);
            }
        }

        internal IEnumerable<Vector3> GetRefuelables()
        {
            //var stations = this.CraftingManager.GetWorkstations().ToList();
            //return stations.Where(g => this.Map.GetBlock(g).Type == Block.Types.Smeltery);

            return this.Map.GetBlockEntities().Where(g => this.Map.GetBlock(g).Type == Block.Types.Smeltery);
        }
        internal IEnumerable<KeyValuePair<Vector3,Blocks.BlockEntity>> GetRefuelablesNew()
        {
            //return this.Map.GetBlockEntitiesCache().Where(kv => kv.Value.HasComp<EntityCompRefuelable>());
            var entities = this.Map.GetBlockEntitiesCache();
            var count = entities.Count;
            for (int i = 0; i < count; i++)
            {
                var kv = entities.ElementAt(i);
                if (kv.Value.HasComp<EntityCompRefuelable>())
                    yield return kv;
            }
        }

        internal virtual void OnTargetSelected(IUISelection info, ISelectable selection)
        {
            if(selection is TargetArgs targetArgs)
                foreach (var c in this.TownComponents)
                    c.OnTargetSelected(info, targetArgs);
        }

        internal IEnumerable<T> GetBusinesses<T>() where T : Workplace
        {
            return this.ShopManager.GetShops().OfType<T>();
            throw new NotImplementedException();
        }

        internal Workplace GetShop(int shopID)
        {
            return this.ShopManager.GetShop(shopID);
        }
        internal T GetShop<T>(int shopID) where T  : Workplace
        {
            return this.ShopManager.GetShop(shopID) as T;
        }

        internal void OnBlocksChanged(IEnumerable<IntVec3> positions)
        {
            foreach (var c in this.TownComponents)
                c.OnBlocksChanged(positions);
        }

        //internal IEnumerable<KeyValuePair<Vector3, Blocks.BlockEntity>> GetSwitchables()
        //{
        //    //return this.Map.GetBlockEntitiesCache().Where(kv => kv.Value.HasComp<EntityCompRefuelable>());
        //    var entities = this.Map.GetBlockEntitiesCache();
        //    var count = entities.Count;
        //    for (int i = 0; i < count; i++)
        //    {
        //        var kv = entities.ElementAt(i);
        //        if (kv.Value.HasComp<BlockEntityCompSwitchable>())
        //            yield return kv;
        //    }
        //}

        //UndiscoveredAreaManager UndiscoveredAreaManager = new UndiscoveredAreaManager();
        //protected void InitUndiscoveredAreas()
        //{
        //    Stopwatch watch = Stopwatch.StartNew();
        //    this.UndiscoveredAreaManager.Init(this.Map);
        //    watch.Stop();
        //    string.Format("undiscovered areas initialized in {0} ms", watch.ElapsedMilliseconds).ToConsole(); ;
        //}
        //internal bool IsUndiscovered(Vector3 global)
        //{
        //    return this.UndiscoveredAreaManager.IsUndiscovered(global);
        //}
    }
}
