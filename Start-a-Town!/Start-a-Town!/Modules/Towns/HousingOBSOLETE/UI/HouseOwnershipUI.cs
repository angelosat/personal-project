using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.AI;

namespace Start_a_Town_.Towns.Housing
{
    class HouseOwnershipUI : GroupBox
    {
        ListBoxNew<GameObject, Label> List;
        Residence Residence;
        public HouseOwnershipUI(Residence r)
        {
            this.Residence = r;

            this.List = new ListBoxNew<GameObject, Label>(150, 300);
            var pnllist = new Panel(){AutoSize  = true};
            pnllist.AddControls(this.List);

            var btnclear = new Button("Clear") { LeftClickAction = () => SetOwnership(null) };
            var pnlbtns = new Panel(){AutoSize = true};
            pnlbtns.AddControls(btnclear);

            this.AddControls(pnllist, pnlbtns);
            this.Controls.AlignVertically();
        }
        //public override bool Show()
        //{
        //    var npclist = this.Residence.Town.AIManager.Agents;
        //    this.List.Build(npclist, c => c.Name);
        //    return base.Show();
        //}
        protected override void OnShow()
        {
            Refresh();
            base.OnShow();
        }

        private new void Refresh()
        {
            //var npclist = this.Residence.Town.Agents;
            var npclist = this.Residence.Town.Agents.Select(id=>Client.Instance.GetNetworkObject(id));
            this.List.Build(npclist, c => c.Name, (g, c) => c.LeftClickAction = () => SetOwnership(g));
        }

        private void SetOwnership(GameObject npc)
        {
            Client.Instance.Send(PacketType.ResidenceSetOwnership, Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                w.Write(this.Residence.ID);
                w.Write(npc != null ? AIComponent.GetGuid(npc).ToByteArray() : Guid.Empty.ToByteArray());
            }));
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.NpcsUpdated:
                    this.Refresh();
                    break;

                default:
                    break;
            }
        }
    }
}
