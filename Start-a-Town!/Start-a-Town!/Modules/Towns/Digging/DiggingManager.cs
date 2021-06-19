using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.AI;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.GameEvents;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Towns.Digging
{
    public class DiggingManager : TownComponent
    {
        HashSet<Vector3> AllPositions = new HashSet<Vector3>();

        public DiggingManager(Town town)
        {
            this.Town = town;
        }
        public override string Name
        {
            get { return "Digging"; }
        }

        internal HashSet<Vector3> GetPositions()
        {
            return this.AllPositions;
        }

        internal IEnumerable<Vector3> GetDiggableBy(GameObject actor)
        {
            return this.AllPositions.Where(p => this.IsDiggableBy(actor, p));
        }
        public override void HandlePacket(Server server, Packet msg)
        {
            this.Handle(server, msg);
        }
        public override void HandlePacket(Client client, Packet msg)
        {
            this.Handle(client, msg);
        }

        public override void Handle(IObjectProvider net, Packet msg)
        {
            switch (msg.PacketType)
            {
                case PacketType.DiggingDesignate:
                    var p = new PacketDiggingDesignate(msg.Payload);

                    var server = net as Server;
                    var dx = p.End.X - p.Begin.X;
                    var dy = p.End.Y - p.Begin.Y;
                    var dz = p.End.Z - p.Begin.Z;

                    var positions = new BoundingBox(p.Begin, p.End).GetBox();
                    net.EventOccured(Components.Message.Types.MiningDesignation, positions, p.Remove);
                    net.Forward(msg);
                    break;

                    //for (int i = 0; i <= dx; i++)// p.Width; i++)
                    //    for (int j = 0; j <= dy; j++)//p.Height; j++)
                    //        for (int k = 0; k <= dz; k++)
                    //        {
                    //            var offset = new Vector3(i, j, k);//0);
                    //            var global = p.Begin + offset;
                    //            if (!p.Remove)
                    //                this.AddPosition(global);
                    //            else
                    //            {
                    //                this.AllPositions.Remove(global);

                    //            }
                    //        }
                    //net.Forward(msg);
                    //break;

                default:
                    break;
            }
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.BlocksChanged:
                    HandleBlocksChanged(e.Parameters[1] as IEnumerable<Vector3>);
                    break;

                case Components.Message.Types.BlockChanged:
                    IMap map;
                    Vector3 global;
                    EventBlockChanged.Read(e.Parameters, out map, out global);
                    //AITaskMining task;
                    HandleBlocksChanged(new Vector3[] {global});
                    break;

                case Components.Message.Types.MiningDesignation:
                    var positions = e.Parameters[0] as List<Vector3>;
                    var remove = (bool)e.Parameters[1];
                    if (remove)
                        foreach (var p in positions)
                            this.RemovePosition(p);
                    else
                        foreach (var p in positions)
                            this.HandlePosition(p);
                    break;

                //case Components.Message.Types.ZoneDesignation:
                //    this.Add(e.Parameters[0] as Designation, e.Parameters[1] as List<Vector3>, (bool)e.Parameters[2]);
                //    break;

                default:
                    break;
            }
        }

        private void HandleBlocksChanged(IEnumerable<Vector3> globals)
        {
            foreach(var global in globals)
                if(this.AllPositions.Contains(global))
                    if(this.Map.IsAir(global))
                        this.AllPositions.Remove(global);
        }

        private void HandlePosition(Vector3 p)
        {
            if (this.IsMinable(p))
            {
                this.AllPositions.Add(p);
            }
        }
        private void RemovePosition(Vector3 p)
        {
            this.AllPositions.Remove(p);
        }
        //public override List<AIJob> FindJob(GameObject actor)
        //{
        //    throw new Exception();
        //}
        public HashSet<Vector3> GetAllPendingTasks()
        {
            return this.AllPositions;
        }

        private void AddPosition(Vector3 global)
        {
            if(this.IsMinable(global))
                this.AllPositions.Add(global);
        }
        bool IsMinable(Vector3 global)
        {
            var material = Block.GetBlockMaterial(this.Town.Map, global);
            var skill = material.Type.SkillToExtract;
            if (skill == null)
                return false;
            var interaction = skill.GetInteraction();
            if (interaction == null)
                return false;
            return true;
        }
        public override UI.GroupBox GetInterface()
        {
            return new DiggingManagerUI(this);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.AllPositions.ToList().Save("Positions"));
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<List<SaveTag>>("Positions", v => this.AllPositions = new HashSet<Vector3>(new List<Vector3>().Load(v)));
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.AllPositions.ToList());
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.AllPositions = new HashSet<Vector3>(r.ReadListVector3());
        }
        public override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera cam)
        {
            cam.DrawGridBlocks(sb, Block.BlockBlueprint, this.AllPositions, Color.White);
        }
        bool IsDiggingTask(Vector3 global)
        {
            return this.AllPositions.Contains(global);
        }

        internal bool IsDiggableBy(GameObject actor, Vector3 vector3)
        {
            bool isTask = this.IsDiggingTask(vector3);
            bool actorStandingOn = this.Town.Map.GetObjects(vector3 + Vector3.UnitZ).Any(o => o != actor && o.IDType == GameObject.Types.Npc);
            return isTask && !actorStandingOn;
        }
        internal override IEnumerable<Tuple<string, Action>> OnQuickMenuCreated()
        {
            yield return new Tuple<string, Action>("Mine", this.Edit);
            yield return new Tuple<string, Action>("Deconstruct", this.EditDeconstruct);
        }
        public void Edit()
        {
            //ToolManager.SetTool(new ToolDigging((a, b, r) => PacketDiggingDesignate.Send(Client.Instance, a, b, r)));

            ToolManager.SetTool(new ToolDigging((a, b, r) => PacketDesignation.Send(Client.Instance, DesignationDef.Mine, a, b, r)));


            //ToolManager.SetTool(new ToolDesignate3D(
            //        (a,b,r)=>PacketDiggingDesignate.Send(Client.Instance, a, b, r)
            //    , this.Town.DiggingManager.GetAllPendingTasks().ToList// manager.Town.GetZones
            //    ) { ValidityCheck = g => true });
        }
        public void EditDeconstruct()
        {
            ToolManager.SetTool(new ToolDigging((a, b, r) => PacketDesignation.Send(Client.Instance, DesignationDef.Deconstruct, a, b, r)));
        }
    }
}
