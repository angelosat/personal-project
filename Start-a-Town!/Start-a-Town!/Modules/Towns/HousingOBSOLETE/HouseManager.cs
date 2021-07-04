using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Housing
{
    public class HouseManager : TownComponent
    {
        public override string Name
        {
            get { return "Housing"; }
        }
        int ResidenceIDSequence = 1;
        public Dictionary<int, Residence> ResidenceList = new Dictionary<int, Residence>();
        public HouseManager(Town town)
            : base(town)
        {

        }
        public override UI.GroupBox GetInterface()
        {
            return new HouseManagerUI(this);
        }
        public override void Handle(IObjectProvider net, Packet msg)
        {
            switch (msg.PacketType)
            {
                case PacketType.ResidenceAdd:
                    Vector3 global;
                    int playerid, residenceid, w, h;
                    bool remove;
                    Network.Deserialize(msg.Payload, r =>
                    {
                        PacketResidenceAdd.Read(r, out playerid, out residenceid, out global, out w, out h, out remove);
                        var res = new Residence(this.Town, residenceid > 0 ? residenceid : ResidenceIDSequence++, global, w, h);
                        AddResidence(res);

                        var server = net as Server;
                        if (server != null)
                            server.Enqueue(PacketType.ResidenceAdd, PacketResidenceAdd.Write(playerid, res.ID, global, w, h, remove));
                    });
                    break;

                case PacketType.ResidenceEdit:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        PacketResidenceAdd.Read(r, out playerid, out residenceid, out global, out w, out h, out remove);
                        var res = this.ResidenceList[residenceid];
                        if (res.Edit(global, w, h, remove))
                            net.Forward(msg);
                    });
                    break;

                case PacketType.ResidenceRename:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        playerid = r.ReadInt32();
                        residenceid = r.ReadInt32();
                        string name = r.ReadString();
                        var res = this.ResidenceList[residenceid];
                        res.Name = name;
                        net.Map.EventOccured(Components.Message.Types.ResidenceUpdated, res);
                        net.Forward(msg);
                    });
                    break;

                case PacketType.ResidenceDelete:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        playerid = r.ReadInt32();
                        residenceid = r.ReadInt32();
                        var res = this.ResidenceList[residenceid];
                        this.ResidenceList.Remove(residenceid);
                        net.Map.EventOccured(Components.Message.Types.ResidenceRemoved, res);
                        net.Forward(msg);
                    });
                    break;

                case PacketType.ResidenceSetOwnership:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        playerid = r.ReadInt32();
                        residenceid = r.ReadInt32();
                        var res = this.ResidenceList[residenceid];
                        var guid = new Guid(r.ReadBytes(16));
                        res.Owner = guid;
                        net.Map.EventOccured(Components.Message.Types.ResidenceUpdated, res);
                        net.Forward(msg);
                    });
                    break;

                default:
                    break;
            }
        }

        private void AddResidence(Residence res)
        {
            this.ResidenceList.Add(res.ID, res);
            this.Town.Map.EventOccured(Components.Message.Types.ResidenceAdded, res);
        }

        public List<Vector3> GetAllResidencePositions()
        {
            var list = new List<Vector3>();
            foreach (var r in this.ResidenceList.Values)
                list.AddRange(r.Positions);
            return list;
        }

        //private void AddResidence(int playerid, int residenceid, int w, int h, bool remove)
        //{
        //    if (residenceid == 0)
        //        residenceid = ResidenceIDSequence++;
        //    this.ResidenceList.Add(new Residence() { ID = residenceid });
        //}
    }
}
