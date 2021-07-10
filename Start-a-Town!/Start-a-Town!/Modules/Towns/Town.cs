using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.AI;
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
        
        internal void OnTooltipCreated(Tooltip tooltip, TargetArgs targetArgs)
        {
            foreach (var c in this.TownComponents)
                c.OnTooltipCreated(tooltip, targetArgs);
        }

        internal void Init()
        {
            this.RoomManager.Init();
        }

        public HashSet<int> Agents = new();
        public IEnumerable<Actor> GetAgents()
        {
            return this.Agents.Select(id => this.Map.Net.GetNetworkObject(id) as Actor);
        }
        public GameObject GetNpc(Guid guid)
        {
            throw new NotImplementedException();
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
        public ReservationManager ReservationManager;
        public TerrainManager TerrainManager;
        public WorkplaceManager ShopManager;
        public QuestsManager QuestManager;

        public List<TownComponent> TownComponents = new();

        public MapBase Map;
        public IObjectProvider Net { get { return this.Map.Net; } }

        public Dictionary<Utility.Types, HashSet<Vector3>> TownUtilitiesNew = new Dictionary<Utility.Types, HashSet<Vector3>>();

        public Town(MapBase map)
        {
            this.Map = map;
            this.FarmingManager = new(this);
            this.ZoneManager = new(this);
            this.ConstructionsManager = new(this);
            this.ChoppingManager = new(this);
            this.DiggingManager = new(this);
            this.DesignationManager = new(this);
            this.RoomManager = new(this);
            this.CraftingManager = new(this);
            this.JobsManager = new(this);
            this.ReservationManager = new(this);
            this.TerrainManager = new(this);
            this.ShopManager = new(this);
            this.QuestManager = new(this);

            this.TownComponents.AddRange(new TownComponent[]{
                this.FarmingManager,
                this.ZoneManager,
                this.ConstructionsManager,
                this.ChoppingManager,
                this.DiggingManager,
                this.DesignationManager,
                this.RoomManager,
                this.CraftingManager,
                this.JobsManager,
                this.ReservationManager,
                this.TerrainManager,
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
            this.AddCitizen(entity.RefID);
            entity.Net.Log.Write(string.Format("{0} has joined the town!", entity.Name));
            this.Map.EventOccured(Message.Types.NpcsUpdated);
        }
        private void RemoveAgent(GameObject entity)
        {
            if (entity.HasComponent<AIComponent>())
            {
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

        internal void Tick()
        {
            foreach (var c in this.TownComponents)
                c.Tick();
        }

        public void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera cam)
        {
            foreach(var comp in this.TownComponents)
                comp.DrawBeforeWorld(sb, map, cam);
        }

        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);

            var compsTag = new SaveTag(SaveTag.Types.Compound, "Components");
            foreach (var comp in this.TownComponents)
                compsTag.Add(comp.Save());

            tag.Add(compsTag);

            SaveAgents(tag);

            var utilitiesTag = new SaveTag(SaveTag.Types.List, "Utilities", SaveTag.Types.Compound);
            
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
            Dictionary<string, SaveTag> compsTag = new Dictionary<string, SaveTag>();
            if (save.TryGetTagValue("Components", out compsTag))
                foreach (var tag in compsTag)
                {
                    var comp = this.TownComponents.FirstOrDefault(c => c.Name == tag.Key);
                    if (comp != null)
                        comp.Load(tag.Value);
                }
           
            LoadAgents(save);

            if (save.TryGetTagValue("Utilities", out List<SaveTag> utilitiesTag))
            {
                foreach (var tag in utilitiesTag)
                {
                    var utilityType = (Utility.Types)(int)tag["Type"].Value;
                    var positionList = new List<Vector3>().Load(tag["Positions"].Value as List<SaveTag>);
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
                    this.AddCitizen(id);
                }
        }

        public void Write(BinaryWriter w)
        {
            foreach (var comp in this.TownComponents)
                comp.Write(w);

            w.Write(this.Agents.Count);
            foreach (var a in this.Agents)
                w.Write(a);

            foreach (var ut in Utility.All())
                w.Write(this.TownUtilitiesNew[ut].ToList());
        }
        public void Read(BinaryReader r)
        {
            foreach (var comp in this.TownComponents)
                comp.Read(r);

            var acount = r.ReadInt32();
            for (int i = 0; i < acount; i++)
            {
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

        public void GetContextActions(GameObject playerEntity, Vector3 pos, ContextArgs a)
        {
            var zone = this.QueryPosition(pos);
            if (zone.Count == 0)
                return;
            zone.First().GetContextActions(playerEntity, a);
        }

        public List<IContextable> QueryPosition(Vector3 pos)
        {
            var list = new List<IContextable>();
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
                    var item = comp.QuerySelectable(target);
                    if (item != null)
                        yield return item;
                }
                yield return target;
            }
        }
        
        internal void GetManagementInterface(TargetArgs t, UI.WindowTargetManagement inter)
        {
            foreach (var c in this.TownComponents)
                c.GetManagementInterface(t, inter);
        }

        internal ZoneNew GetZoneAt(Vector3 pos)
        {
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
            actions.Add(new Tuple<string, Action>("Spawn objects", () => UI.Editor.ObjectsWindowDefs.Instance.Show()));
            actions.Add(new Tuple<string, Action>("Edit blocks", () => UI.Editor.TerrainWindow.Instance.Show()));

            this.QuickMenu = new UIQuickMenu();
            this.QuickMenu.AddItems(actions);
            this.QuickMenu.Location = UIManager.Mouse;
        }

        public IEnumerable<(string name, Action action)> GetInfoTabs(ISelectable selected)
        {
            foreach (var comp in this.TownComponents)
            {
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

        internal IEnumerable<KeyValuePair<Vector3,Blocks.BlockEntity>> GetRefuelablesNew()
        {
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
    }
}
