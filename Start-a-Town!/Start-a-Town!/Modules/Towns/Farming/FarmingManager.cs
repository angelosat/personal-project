using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

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

        public override GroupBox GetInterface()
        {
            return new FarmingManagerUI(this);
        }
        
        public List<Vector3> GetAllTillingLocations()
        {
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

        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.IDSequence.Save("IDSequence"));
            var farms = new SaveTag(SaveTag.Types.List, "Farms", SaveTag.Types.Compound);
            foreach (var farm in this.GrowZones)
                farms.Add(farm.Value.Save());
            tag.Add(farms);
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
    }
}
