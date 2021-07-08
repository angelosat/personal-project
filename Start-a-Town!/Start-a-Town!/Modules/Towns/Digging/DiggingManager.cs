using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.GameEvents;

namespace Start_a_Town_.Towns.Digging
{
    public class DiggingManager : TownComponent
    {
        HashSet<Vector3> AllPositions = new HashSet<Vector3>();

        public DiggingManager(Town town)
        {
            this.Town = town;
        }
        public override string Name => "Digging";

        internal HashSet<Vector3> GetPositions()
        {
            return this.AllPositions;
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
                    var positions = new BoundingBox(p.Begin, p.End).GetBox();
                    net.EventOccured(Components.Message.Types.MiningDesignation, positions, p.Remove);
                    net.Forward(msg);
                    break;

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
        public HashSet<Vector3> GetAllPendingTasks()
        {
            return this.AllPositions;
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

        internal override IEnumerable<Tuple<string, Action>> OnQuickMenuCreated()
        {
            yield return new Tuple<string, Action>("Mine", this.Edit);
            yield return new Tuple<string, Action>("Deconstruct", this.EditDeconstruct);
        }
        public void Edit()
        {
            ToolManager.SetTool(new ToolDigging((a, b, r) => PacketDesignation.Send(Client.Instance, DesignationDef.Mine, a, b, r)));
        }
        public void EditDeconstruct()
        {
            ToolManager.SetTool(new ToolDigging((a, b, r) => PacketDesignation.Send(Client.Instance, DesignationDef.Deconstruct, a, b, r)));
        }
    }
}
