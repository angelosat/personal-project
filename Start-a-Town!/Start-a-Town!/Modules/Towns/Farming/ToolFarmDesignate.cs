using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Farming
{
    class ToolFarmDesignate : ToolZoningPositionsNew
    {
        int CurrentZoneID;
        int ClickedZoneID;
        int EditingZone { get { return this.CurrentZoneID != 0 ? this.CurrentZoneID : this.ClickedZoneID; } }
        public ToolFarmDesignate(GrowingZone currentEditingZone)
            : base(currentEditingZone.Town)
            //: base(Perform, existingGetter)
        {
            this.CurrentZoneID = currentEditingZone.ID;
            this.Add = this.Perform;
            //this.GetZones = () => currentEditingZone.Tasks.Keys.ToList();
            this.GetZones = () => currentEditingZone.Town.FarmingManager.GrowZones.Values.Select(t => t as ZoneNew).ToList();
        }
        public ToolFarmDesignate(Town town)
            : base(town)
        //: base(Perform, existingGetter)
        {
            this.CurrentZoneID = 0;
            this.Add = this.Perform;
            //this.GetZones = () => currentEditingZone.Tasks.Keys.ToList();
            this.GetZones = () => Client.Instance.Map.Town.FarmingManager.GrowZones.Values.Select(t => t as ZoneNew).ToList();
        }
        void Perform(Vector3 arg1, int arg2, int arg3, bool arg4)
        {
            Client.Instance.Send(PacketType.FarmCreate, new PacketCreateFarmland(PlayerOld.Actor.RefID, this.EditingZone, arg1, arg2, arg3, arg4).Write());
            //return;
            //if (this.CurrentZoneID == 0)
            //    CreateFarm(arg1, arg2, arg3, arg4);
            //else
            //    this.Edit(arg1, arg2, arg3, arg4);
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.FarmRemoved:
                    var zone = e.Parameters[0] as GrowingZone;
                    if (zone.ID == this.CurrentZoneID)
                    {
                        this.CurrentZoneID = 0;
                        //this.GetZones = e.Net.Map.Town.FarmingManager.GetAllPositions;
                        this.GetZones = () => e.Net.Map.Town.FarmingManager.GrowZones.Values.Select(t => t as ZoneNew).ToList();
                    }
                    break;

                default:
                    break;
            }
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            base.MouseLeftPressed(e);
            var zone = this.Town.FarmingManager.GetZoneAt(this.Begin);
            this.ClickedZoneID = zone != null ? zone.ID : 0;
            return Messages.Default;
        }
        private void Edit(Vector3 global, int w, int h, bool arg4)
        {
            var begin = global;
            var end = global + new Vector3(w - 1, h - 1, 0);
            Client.Instance.Send(PacketType.FarmlandDesignate, PacketDesignate.Write(PlayerOld.Actor.RefID, this.CurrentZoneID, begin, end, arg4));
        }
        private static void CreateFarm(Vector3 global, int w, int h, bool remove)
        {
            Client.Instance.Send(PacketType.FarmCreate, new PacketCreateFarmland(PlayerOld.Actor.RefID, 0, global, w, h, remove).Write());
        }
    }
}
