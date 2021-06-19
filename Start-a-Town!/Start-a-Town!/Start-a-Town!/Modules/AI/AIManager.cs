using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.GameEvents;

namespace Start_a_Town_.AI
{
    class AIManager : GameComponent
    {
        public override void Initialize()
        {
            Net.Server.Instance.RegisterPacketHandler(Net.PacketType.AI, new AIPacketHandler());
            Net.Client.Instance.RegisterPacketHandler(Net.PacketType.AI, new AIPacketHandler());

        }

        //void Handle(IObjectProvider net, Packet p)
        //{
        //    switch(p.PacketType)
        //    {
        //        case PacketType.AIJobComplete:
        //            p.Payload.Deserialize(r =>
        //                {
        //                    var entity = net.GetNetworkObject(r.ReadInt32());
        //                    var state = AIState.GetState(entity);
        //                    //state.History.Write(net.Clock, "Completet job: " + )
        //                    var instruction = new AIInstruction(net, r);
        //                    state.History.Write(net.Clock, "Completet job: " + instruction.ToString());
        //                });
        //            break;

        //        default:
        //            break;
        //    }
        //}

        public override void HandlePacket(Client client, Packet msg)
        {
            Handle(client, msg);
        }
        public override void HandlePacket(Server server, Packet p)
        {
            switch (p.PacketType)
            {
                case PacketType.AIGenerateNpc:
                    p.Payload.Deserialize(r =>
                    {
                        //var senderid = r.ReadInt32();
                        //var senderentity = server.GetNetworkObject(senderid);
                        var npc = GenerateNpc(server.GetRandom());
                        npc.Global = p.Player.Character.Global + Vector3.UnitZ;
                        server.Spawn(npc);
                    });
                    break;

                default:
                    HandlePacket(server, p);
                    break;
            }
        }

        public static void HandlePacket(IObjectProvider net, Packet p)
        {
            
        }

        public static void Handle(Client client, Packet p)
        {
            //this.Handle(client, msg);
            switch (p.PacketType)
            {
                case PacketType.AIJobComplete:
                    p.Payload.Deserialize(r =>
                    {
                        var entity = client.GetNetworkObject(r.ReadInt32());
                        //state.History.Write(net.Clock, "Completet job: " + )
                        var jobDescription = r.ReadString();// new AIInstruction(client, r);
                        JobComplete(entity, jobDescription);
                    });
                    break;

                default:
                    break;
            }
        }

        public static void JobComplete(GameObject entity, string jobDescription)
        {
            entity.Net.Map.EventOccured(Components.Message.Types.JobComplete, entity, jobDescription);
            var state = AIState.GetState(entity);
            //state.History.Write(entity.Net.Clock, "Completed job: " + jobDescription);
            //state.History.Write(entity.Net.Clock, jobDescription);
            state.History.WriteEntry(entity, "JOB COMPLETE! " + jobDescription);

            //state.History.Write("Success: " + jobDescription);
            if(entity.Net is Server)
            {
                var server = entity.Net as Server;
                server.Enqueue(PacketType.AIJobComplete, Network.Serialize(w =>
                {
                    w.Write(entity.InstanceID);
                    //instruction.Write(w);
                    w.Write(jobDescription);
                    //var goal = job.Instructions.First();
                    //goal.Target.Write(w);
                    //w.Write(goal.Interaction.Name);
                }));
            }
        }
        //public override void HandlePacket(Net.Server server, Net.Packet msg)
        //{
        //    this.Handle(server, msg);
        //}

        //public void JobComplete(GameObject entity, AIInstruction goal)
        //{

        //}

        static public GameObject GenerateNpc(RandomThreaded random)
        {
            //var npc = GameObject.Objects[GameObject.Types.Npc].Clone();
            var npc = GameObject.Create(GameObject.Types.Npc);
            AIState state = AIState.GetState(npc);
            state.Generate(npc, random);
            npc.Name = NpcComponent.RandomName();
            return npc;
        }

        public override void OnGameEvent(GameEvent e)
        {
            if (e.Net is Client)
                return;
            switch (e.Type)
            {
                //case Message.Types.InteractionFailed:
                //    var entity = e.Parameters[0] as GameObject;
                //    var interaction = e.Parameters[1] as AIInstruction;
                //    if (entity.Map.Town.Agents.Contains(entity))
                //        AIState.GetState(entity).History.WriteEntry(entity, "Failed: " + interaction.ToString());
                //    break;

                //case Message.Types.InteractionSuccessful:
                //    entity = e.Parameters[0] as GameObject;
                //    interaction = e.Parameters[1] as AIInstruction;
                //    if (entity.Map.Town.Agents.Contains(entity))
                //        AIState.GetState(entity).History.WriteEntry(entity, "Success: " + interaction.ToString());
                //    break;

                case Message.Types.EntityDespawned:
                    GameObject entity;
                    EventEntityDespawned.Read(e.Parameters, out entity);
                    //var state = AIState.GetState(entity);
                    //if (state == null)
                    //    break;
                    //state.OnDespawn();
                    AIState state;
                    if (AIState.TryGetState(entity, out state))
                        state.OnDespawn(); // TODO: MAKE THIS STATIC?
                    break;

                default:
                    break;
            }
        }
    }
}
