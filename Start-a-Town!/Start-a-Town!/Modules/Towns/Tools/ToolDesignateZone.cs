using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns
{
    class ToolDesignateZone : ToolZoningPositionsNew
    {
        Type ZoneType;
        int CurrentZoneID;
        int ClickedZoneID;
        int EditingZone { get { return this.CurrentZoneID != 0 ? this.CurrentZoneID : this.ClickedZoneID; } }
        public ToolDesignateZone()
        {

        }
        public ToolDesignateZone(ZoneNew currentEditingZone)
            //: base(currentEditingZone.Town)
        {
            this.CurrentZoneID = currentEditingZone.ID;
            this.Add = this.Perform;
        }
        public ToolDesignateZone(Type zoneType)
            //: base(Client.Instance.Map.Town)
        {
            this.ZoneType = zoneType;
            this.CurrentZoneID = 0;
            this.Add = this.Perform;
        }
        public ToolDesignateZone(Town town, Type zoneType)
            //: base(town)
        {
            this.ZoneType = zoneType;
            this.CurrentZoneID = 0;
            this.Add = this.Perform;
        }
        void Perform(Vector3 arg1, int arg2, int arg3, bool arg4)
        {
            PacketZoneDesignation.Send(Town.Net, this.ZoneType, this.EditingZone, arg1, arg2, arg3, arg4);
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.ZoneRemoved:
                    // TODO make the town have a common zone ID sequence instaed of each manager having each own?
                    throw new Exception();

                default:
                    break;
            }
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            base.MouseLeftPressed(e);
            var zone = this.Town.GetZoneAt(this.Begin);
            this.ClickedZoneID = zone != null ? zone.ID : 0;
            return Messages.Default;
        }
        public override string HelpText => "Hold down control to clear zone tiles";
    }
}
