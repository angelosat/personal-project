using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns.Farming
{
    public class FarmingManager : TownComponent
    {
        public override string Name => "Farming";
       

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

        public IEnumerable<GameObject> GetChoppableTrees()
        {
            var list = new List<GameObject>();
            foreach (var farm in this.GrowZones)
                list.AddRange(farm.Value.GetChoppableTrees());
            return list;
        }
       
        protected override void AddSaveData(SaveTag tag)
        {
            var farms = new SaveTag(SaveTag.Types.List, "Farms", SaveTag.Types.Compound);
            foreach (var farm in this.GrowZones)
                farms.Add(farm.Value.Save());
            tag.Add(farms);
        }

        public override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera cam)
        {
            if (!cam.DrawZones)
                return;
            foreach (var s in this.GrowZones)
                s.Value.DrawBeforeWorld(sb, map, cam);
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

    }
}
