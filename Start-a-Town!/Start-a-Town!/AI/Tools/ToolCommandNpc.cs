using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class ToolCommandNpc : ToolManagement
    {
        private List<GameObject> Actors;
        public ToolCommandNpc()
        {

        }
        public ToolCommandNpc(GameObject npc)
            : this(new List<GameObject>() { npc })
            //: base(ScreenManager.CurrentScreen.Camera)
        {
            //this.Npc = npc;
        }

        public ToolCommandNpc(List<GameObject> actors)
        {
            // TODO: Complete member initialization
            this.Actors = actors.ToList();
        }
        public override Icon GetIcon()
        {
            return Icon.Replace;
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Target != null)
                if (this.Target.Type == TargetType.Position)
                    PacketCommandNpc.Send(Client.Instance, this.Actors.Select(i=>i.RefID).ToList(), this.Target, IsEnqueing);
                    //PacketCommandNpc.Send(Client.Instance, this.Npc.InstanceID, this.Target);
            return base.MouseLeftPressed(e);
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }

        public bool IsEnqueing
        {
            get
            {
                return InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey);
            }
        }
    }
}
