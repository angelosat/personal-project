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
        Dictionary<Vector3, AIJob> HandledPositions = new Dictionary<Vector3, AIJob>();

        public DiggingManager(Town town)
        {
            this.Town = town;
        }
        public override string Name
        {
            get { return "Digging"; }
        }

        internal List<Vector3> GetPositions()
        {
            return this.AllPositions.ToList();
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

                    for (int i = 0; i <= dx; i++)// p.Width; i++)
                        for (int j = 0; j <= dy; j++)//p.Height; j++)
                            for (int k = 0; k <= dz; k++)
                        {
                            var offset = new Vector3(i, j, k);//0);
                            var global = p.Begin + offset;
                            if (!p.Remove)
                                this.AddPosition(global);
                            else
                            {
                                this.AllPositions.Remove(global);
                                if (server == null)
                                    continue;
                                AIJob job;
                                if (this.HandledPositions.TryGetValue(global, out job))
                                    job.Cancel();
                                this.HandledPositions.Remove(global);
                            }
                    }
                    if (server != null)
                    {
                        server.Enqueue(PacketType.DiggingDesignate, msg.Payload, SendType.OrderedReliable, true);
                    }
                    break;

                default:
                    break;
            }
        }

        private void AddPosition(Vector3 global)
        {
            var material = Block.GetBlockMaterial(this.Town.Map, global);
            var skill = material.Type.SkillToExtract;
            if (skill == null)
                return;
            var interaction = skill.GetWork();
            if (interaction == null)
                return;
            this.AllPositions.Add(global);
        }

        public override void OnUpdate()
        {
            foreach(var pos in this.AllPositions.ToList())
            {
                if (this.HandledPositions.ContainsKey(pos))
                    continue;
                var blockPos = pos;// -Vector3.UnitZ;
                var material = Block.GetBlockMaterial(this.Town.Map, blockPos);
                var skill = material.Type.SkillToExtract;
                if (skill == null)
                    continue;
                var interaction = skill.GetWork();
                if (interaction == null)
                    continue;
                AIJob job = new AIJob();
                AIInstruction instr = new AIInstruction(new TargetArgs(blockPos), interaction);// new InteractionDigging());
                job.AddStep(instr);
                job.Labor = material.Type.Labor;
                this.Town.AddJob(job);
                this.HandledPositions.Add(pos, job);
            }
        }
        public override UI.GroupBox GetInterface()
        {
            return new DiggingManagerUI(this);
        }

        public override List<SaveTag> Save()
        {
            var tag = new List<SaveTag>();
            tag.Add(this.AllPositions.ToList().Save("Positions"));
            return tag;
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
        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.BlockChanged:
                    IMap map;
                    Vector3 global;
                    EventBlockChanged.Read(e.Parameters, out map, out global);
                    var block = map.GetBlock(global);
                    if (block.Type == Block.Types.Air)
                    {
                        this.HandledPositions.Remove(global);
                        this.AllPositions.Remove(global);
                    }
                    break;
                    
                default:
                    break;
            }
        }
    }
}
