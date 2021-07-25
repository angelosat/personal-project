using System;

namespace Start_a_Town_.Towns
{
    class ToolDesignateZone : ToolZoningPositionsNew
    {
        readonly Type ZoneType;
        readonly int CurrentZoneID;
        int ClickedZoneID;
        int EditingZone => this.CurrentZoneID != 0 ? this.CurrentZoneID : this.ClickedZoneID;

        readonly Town Town;
        public ToolDesignateZone()
        {

        }
        public ToolDesignateZone(Town town, Type zoneType)
        {
            this.ZoneType = zoneType;
            this.CurrentZoneID = 0;
            this.Add = this.Perform;
            this.Town = town;
        }
        void Perform(IntVec3 arg1, int arg2, int arg3, bool arg4)
        {
            PacketZoneDesignation.Send(this.Town.Net, this.ZoneType, this.EditingZone, arg1, arg2, arg3, arg4);
        }

        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            base.MouseLeftPressed(e);
            var zone = this.Town.GetZoneAt(this.Begin);
            this.ClickedZoneID = zone != null ? zone.ID : 0;
            return Messages.Default;
        }
        string _helpText = "Hold down control to clear zone tiles";
        public override string HelpText => _helpText;
    }
}
