using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes;
using Start_a_Town_.Net.Packets;
using Start_a_Town_.AI;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Towns.Farming
{
    public class FarmingManager : TownComponent
    {
        public override string Name
        {
            get
            {
                return "Farming";
            }
        }

        int IDSequence = 1;

        public Dictionary<int, GrowingZone> GrowZones = new Dictionary<int, GrowingZone>();

        const float UpdateFrequency = 1; // per second
        float UpdateTimerMax = (float)Engine.TicksPerSecond / UpdateFrequency;
        float UpdateTimer;

        List<Vector3> CachedTillingPositions, CachedSowingPositions;
        
        public FarmingManager(Town town)
        {
            this.Town = town;
        }

        public override void Update()
        {
            if (this.Town.Map.Net is Client)
                return;
            CachedTillingPositions = null;
            if (this.UpdateTimer > 0)
            {
                this.UpdateTimer--;
                return;
            }
            this.UpdateTimer = UpdateTimerMax;
        }

        public GrowingZone GetZoneAt(Vector3 global)
        {
            foreach (var farm in this.GrowZones.Values)
            {
                if(farm.GetPositions().Contains(global))
                    return farm;
            }
            return null;
        }
        //[Obsolete]
        //public override void Handle(IObjectProvider net, Packet msg)
        //{
        //    throw new Exception();
        //    switch (msg.PacketType)
        //    {
        //        case PacketType.FarmlandDesignate:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int entityid, farmid;
        //                Vector3 begin, end;
        //                bool value;
        //                PacketDesignate.Read(r, out entityid, out farmid, out begin, out end, out value);
        //                //this.Farmlands[farmid].Edit(begin.GetBox(end), value);
        //                this.GrowZones[farmid].Edit(begin.GetBox(end), value);
        //                var server = net as Server;
        //                if (server != null)
        //                    server.Enqueue(msg.PacketType, msg.Payload);
        //            });
        //            break;

        //        default:
        //            break;
        //    }
        //}

        public override void HandlePacket(Server server, Net.Packet msg)
        {
            switch (msg.PacketType)
            {
                case PacketType.FarmSetSeed:
                    msg.Payload.Deserialize(r =>
                        {
                            PacketFarmSetSeed.Handle(server, msg, this);
                        });
                    break;

                default:
                    this.Handle(server, msg);
                    break;
            }
        }
        public override void HandlePacket(Client client, Net.Packet msg)
        {
            switch (msg.PacketType)
            {
                case PacketType.FarmSetSeed:
                    msg.Payload.Deserialize(r =>
                    {
                        PacketFarmSetSeed.Handle(client, msg, this);
                    });
                    break;

                default:
                    this.Handle(client, msg);
                    break;

            }
        }

        //void RegisterZone(GrowingZone farm)
        //{
        //    if (farm.ID == 0)
        //        farm.ID = this.IDSequence++;
        //    throw new Exception();
        //    //farm.Town = this.Town;
        //    this.GrowZones.Add(farm.ID, farm);
        //    this.Town.Map.EventOccured(Components.Message.Types.FarmCreated, farm);
        //    if (this.Town.Map.Net is Client)
        //        FloatingText.Manager.Create(() => farm.Positions.First(), "Farm created", ft => ft.Font = UIManager.FontBold);
        //}
        private ZoneNew RegisterNewZone(int id, Vector3 a, int w, int h)
        {
            throw new Exception();
            //var allpositions = new BoundingBox(a, a + new Vector3(w - 1, h - 1, 0)).GetBox();
            //var finalPositions = new List<Vector3>();
            //foreach (var po in allpositions)
            //    if (this.Town.GetZoneAt(po) == null && ZoneNew.IsPositionValid(this.Town.Map, po))
            //        finalPositions.Add(po);
            //if (!finalPositions.IsConnected())
            //    return null;
            //var newzone = new GrowingZone(this.Town, finalPositions);
            //this.RegisterZone(newzone);
            //return newzone;
        }
        internal ZoneNew RegisterNewZone(IEnumerable<IntVec3> allpositions)
        {
            throw new Exception();
            //if (!allpositions.IsConnectedNew())
            //    return null;
            //var finalPositions = allpositions.Where(
            //    po => this.Town.GetZoneAt(po) == null &&
            //    ZoneNew.IsPositionValid(this.Town.Map, po));
            //var newzone = new GrowingZone(this.Town, finalPositions);
            //this.RegisterZone(newzone);
            //return newzone;
        }
        public override GroupBox GetInterface()
        {
            return new FarmingManagerUI(this);
        }
        
        //public List<Vector3> GetAllPositions()
        //{
        //    //foreach (var farm in this.GrowZones)
        //    //    foreach (var g in farm.Value.Tasks.Keys)
        //    //        yield return g;

        //    var list = new List<Vector3>();
        //    foreach (var farm in this.GrowZones)
        //        list.AddRange(farm.Value.Tasks.Keys);
        //    return list;
        //}
      
        public List<Vector3> GetAllTillingLocations()
        {
            //if (this.CachedTillingPositions != null)
            //    foreach (var p in this.CachedSowingPositions)
            //        yield return p;
            //foreach (var farm in this.GrowZones)
            //    foreach(var p in farm.Value.GetTillingPositions())
            //        yield return p;
            //this.CachedTillingPositions = list;
            //return list;

            if (this.CachedTillingPositions != null)
                return this.CachedTillingPositions;
            var list = new List<Vector3>();
            foreach (var farm in this.GrowZones)
                list.AddRange(farm.Value.GetTillingPositions());
            this.CachedTillingPositions = list;
            return list;
        }
        
        public List<Vector3> GetAllSowingLocations()
        {
            if (this.CachedSowingPositions != null)
                return this.CachedSowingPositions;
            var list = new List<Vector3>();
            foreach (var farm in this.GrowZones)
                list.AddRange(farm.Value.GetSowingPositions());
            this.CachedSowingPositions = list;
            return list;
        }
       public IEnumerable<GameObject> GetChoppableTrees()
        {
            var list = new List<GameObject>();
            foreach (var farm in this.GrowZones)
                list.AddRange(farm.Value.GetChoppableTrees());
            return list;
        }
       
        public bool RemoveFarm(int growzoneID)
        {
            var zone = this.GrowZones[growzoneID];
            if (zone == null)
                throw new Exception();
            this.GrowZones.Remove(growzoneID);
            this.Town.Map.EventOccured(Components.Message.Types.FarmRemoved, zone);
            if (this.Town.Map.Net is Client)
                FloatingText.Manager.Create(() => zone.Positions.First(), "Farm deleted", ft => ft.Font = UIManager.FontBold);

            return true;
        }

        //internal override void OnGameEvent(GameEvent e)
        //{
        //    switch (e.Type)
        //    {
        //        default:
        //            foreach (var f in this.GrowZones)
        //                f.Value.OnGameEvent(e);
        //            break;
        //    }
        //}

        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.IDSequence.Save("IDSequence"));
            var farms = new SaveTag(SaveTag.Types.List, "Farms", SaveTag.Types.Compound);
            foreach (var farm in this.GrowZones)
                farms.Add(farm.Value.Save());
            tag.Add(farms);
        }

        public override void Load(SaveTag tag)
        {
            //tag.TryGetTagValue<int>("IDSequence", v => this.IDSequence = v);
            //var list = new List<SaveTag>();
            //if (tag.TryGetTagValue("Farms", out list))
            //    foreach (var farmtag in list)
            //    {
            //        var zone = new GrowingZone(this.Town, farmtag);
            //        this.GrowZones.Add(zone.ID, zone);
            //    }
        }

        public override void Write(BinaryWriter w)
        {
            //w.Write(this.IDSequence);
            //w.Write(this.GrowZones.Count);
            //foreach (var farm in this.GrowZones)
            //    farm.Value.Write(w);
        }
        public override void Read(BinaryReader r)
        {
            //this.IDSequence = r.ReadInt32();
            //var count = r.ReadInt32();
            //for (int i = 0; i < count; i++)
            //{
            //    var farm = new GrowingZone(r);
            //    farm.Town = this.Town;
            //    this.GrowZones.Add(farm.ID, farm);
            //}
        }

        public override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera cam)
        {
            if (!cam.DrawZones)
                return;
            foreach (var s in this.GrowZones)
                s.Value.DrawBeforeWorld(sb, map, cam);
        }

        
        public bool IsSowable(Vector3 arg)
        {
            var farm = this.GetZoneAt(arg);
            if (farm == null)
                return false;
            if (!farm.Planting)
                return false;
            var positions = farm.GetSowingPositions();
            var contains = positions.Contains(arg);
            return contains;
        }
        //public bool IsSowable(Vector3 arg, GameObject item)
        //{
        //    var farm = this.GetZoneAt(arg);
        //    if (farm == null)
        //        return false;
        //    if (!farm.Planting)
        //        return false;
        //    if(!item.IsSeedFor(farm.SeedType))
        //        return false;
        //    var positions = farm.GetSowingPositions();
        //    var contains = positions.Contains(arg);
        //    return contains;
        //}
        public bool IsTillable(Vector3 arg)
        {
            var farm = this.GetZoneAt(arg);
            if (farm == null)
                return false;
            var positions = farm.GetTillingPositions();
            var contains = positions.Contains(arg);
            return contains;
        }
        public bool IsValidPlant(GameObject obj)
        {
            var rounded = obj.Global;
            rounded = rounded.Round() - Vector3.UnitZ;
            var farm = this.GetZoneAt(rounded);
            return farm != null;
        }
        
        //public Dictionary<Vector3, GrowingZone.FarmingJob> GetUnreservedTasks(GrowingZone.FarmingJob.Types type)
        //{
        //    Dictionary<Vector3, GrowingZone.FarmingJob> tasks = new Dictionary<Vector3, GrowingZone.FarmingJob>();
        //    foreach (var zone in this.GrowZones.Values)
        //        foreach (var task in zone.Tasks)
        //            if (task.Value.CanReserve && task.Value.Type == type)
        //                tasks[task.Key] = task.Value;
        //    return tasks;
        //}

        
        public override void GetManagementInterface(TargetArgs t, WindowTargetManagement inter)
        {
            var zone = this.GetZoneAt(t.Global);
            if (zone == null)
                return;
            var ui = new GrowingZoneUI(zone);
            inter.AddControls(ui);
            inter.Tag = zone;
            inter.TitleFunc = ()=>zone.Name;
        }
        public override IContextable QueryPosition(Vector3 global)
        {
            return this.GetZoneAt(global);
        }
        public override ISelectable QuerySelectable(TargetArgs target)
        {
            return this.GetZoneAt(target.Global);
        }

        internal static void Initialize()
        {
            
        }

        internal bool IsChoppableTree(GameObject tree)
        {
            var grown = tree.GetComponent<Components.TreeComponent>().Growth.IsFinished;
            if (!grown)
                return false;
            var containedInZone = false;
            foreach (var zone in this.GrowZones.Values)
                containedInZone |= zone.Contains(tree);
            return containedInZone;
        }

        internal ZoneNew PlayerEdit(int zoneID, Vector3 a, int w, int h, bool remove)
        {
            if (remove)
            {
                foreach (var zone in this.GrowZones.Values.ToList())
                    zone.Edit(a, a + new Vector3(w - 1, h - 1, 0), remove);
            }
            else
            {
                if (zoneID == 0)
                    return this.RegisterNewZone(0, a, w, h);
                else
                    this.GrowZones[zoneID].Edit(a, a + new Vector3(w - 1, h - 1, 0), remove);
            }
            return null;
        }
        //internal override IEnumerable<Tuple<string, Action>> OnQuickMenuCreated()
        //{
        //    yield return new Tuple<string, Action>("Farm", () => ZoneNew.Edit(typeof(GrowingZone)));
        //}
    }
}
