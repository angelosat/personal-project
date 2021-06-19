using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Towns;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.Components.AI;

namespace Start_a_Town_.AI
{
    public class LaborsManager : TownComponent
    {
        public List<GameObject> Agents = new List<GameObject>();

        public override string Name
        {
            get { return "Labors"; }
        }
        public LaborsManager(Town town)
        {
            this.Town = town;
        }
        public override GroupBox GetInterface()
        {
            return new AILaborsUITable(this.Town);
        }
        public override void Handle(IObjectProvider net, Net.Packet msg)
        {
            switch(msg.PacketType)
            {
                case PacketType.LaborToggle:
                    msg.Payload.Deserialize(r =>
                        {
                            var entity = net.GetNetworkObject(r.ReadInt32());
                            var laborname = r.ReadString();
                            var labor = AILabor.All.First(l => l.Name == laborname);
                            var state = AIState.GetState(entity);
                            if (state.Labors.Contains(labor))
                                state.Labors.Remove(labor);
                            else
                                state.Labors.Add(labor);
                            net.Map.EventOccured(Components.Message.Types.LaborsUpdated, entity);
                            net.Forward(msg);
                        });
                    break;

                case PacketType.NeedModifyValue:
                    msg.Payload.Deserialize(r =>
                        {
                            var entity = net.GetNetworkObject(r.ReadInt32());
                            //var need = Components.NeedsComponent.GetNeed(entity, (Components.Needs.Need.Types)r.ReadInt32());
                            Components.NeedsComponent.ModifyNeed(entity, (Components.Needs.Need.Types)r.ReadInt32(), r.ReadInt32());
                        });
                    break;

                default:
                    break;
            }
        }
    }
}
