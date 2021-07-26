using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes.StaticMaps;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class PopulationManager : ISaveable, ISerializable
    {
        static internal class Packets
        {
            static int PacketVisitorArrived, PacketAdventurerCreated;
            public static void Init()
            {
                PacketVisitorArrived = Network.RegisterPacketHandler(ReceiveNotifyVisit);
                PacketAdventurerCreated = Network.RegisterPacketHandler(ReceiveNotifyAdventurerCreated);
            }
            
            public static void SendNotifyVisit(Actor actor)
            {
                var w = Server.Instance.GetOutgoingStream();
                w.Write(PacketVisitorArrived, actor.RefID);
            }
            public static void SendNotifyAdventurerCreated(Actor actor)
            {
                var w = Server.Instance.GetOutgoingStream();
                w.Write(PacketAdventurerCreated, actor.RefID);
            }
            private static void ReceiveNotifyAdventurerCreated(INetwork net, BinaryReader r)
            {
                var client = net as Client;
                var actorID = r.ReadInt32();
                var actor = client.GetNetworkObject(actorID) as Actor;
                var world = client.Map.World as StaticWorld;
                world.Population.RegisterActor(client, actor);
            }
            private static void ReceiveNotifyVisit(INetwork net, BinaryReader r)
            {
                if (net is Server)
                    throw new Exception();
                var actorID = r.ReadInt32();
                var actor = net.GetNetworkObject(actorID) as Actor;
                ReportVisit(net, actor);
            }

            private static void ReportVisit(INetwork net, Actor actor)
            {
                var props = actor.GetVisitorProperties();
                net.Report($"{actor.Name} is {(!actor.Exists ? ("visiting" + (props.Discovered ? "" : " for the first time!")) : "departing")}");
                props.Discovered = true;
            }
        }
        static internal void Init()
        {
            Packets.Init();
            OffsiteAreaDefOf.Init();
            VisitorNeedsDefOf.Init();
        }

        bool Populated;
        readonly List<VisitorProperties> ActorsAdventuring = new(ActorsCap);
        readonly public StaticWorld World;
        const int ActorsCap = 8;
        const float TickRate = 1 / 3f, InitialChance = .05f,  VisitChanceBaseRate = .001f;// 2 seconds per tick //1 tick per second 
        const int InitialApproval = 50;

        int TickCount = (int)(Engine.TicksPerSecond / TickRate);
        public PopulationManager(StaticWorld world)
        {
            this.World = world;
        }
        public void Update(INetwork net)
        {
            if (net is Server)
                this.HandleErrors();
            foreach (var v in this.ActorsAdventuring)
                v.Tick();
            this.TickCount--;
            if (this.TickCount > 0)
                return;
            this.TickCount = (int)(Engine.TicksPerSecond / TickRate);
            this.Tick(net);
        }

        private void HandleErrors()
        {
            var map = this.World.Map;
            var net = map.Net;
            var allActors = net.GetNetworkObjects().OfType<Actor>();
            var citizens = map.Town.GetAgents();
            foreach (var actor in allActors)
            {
                if (citizens.Contains(actor))
                    continue;
                if (!this.ActorsAdventuring.Any(v => v.Actor == actor))
                {
                    this.Populated = true;
                    Packets.SendNotifyAdventurerCreated(actor);
                    RegisterActor(actor.Net as Server, actor);
                    Log.WriteToFile($"{actor.Name} is not a town member and was missing from the world population list.");
                }
            }
        }

        void Tick(INetwork net)
        {
            this.PopulateNew(net);
        }

        private void PopulateNew(INetwork net)
        {
            if (net is Client)
                return;
            if (this.ActorsAdventuring.Count < ActorsCap)
            {
                Actor actor = GenerateVisitor();
                actor.SyncInstantiate(Server.Instance);
                Packets.SendNotifyAdventurerCreated(actor);
                RegisterActor(Server.Instance, actor);
            }
        }

        private static Actor GenerateVisitor()
        {
            var visitor = ActorDefOf.Npc.Create() as Actor;
            visitor.Inventory.Insert(ItemDefOf.Coins.Create().SetStackSize(500));
            return visitor;
        }

        private void RegisterActor(INetwork net, Actor actor)
        {
            var props = new VisitorProperties(net.Map.World as StaticWorld, actor, InitialChance, InitialApproval) { OffsiteArea = OffsiteAreaDefOf.Forest };
            this.ActorsAdventuring.Add(props);
            MakeVisitor(actor);
            net.Write(string.Format("{0} created", actor.Name));
            net.EventOccured(Components.Message.Types.NewAdventurerCreated, actor);
        }

        private static void MakeVisitor(Actor actor)
        {
            actor.AddNeed(VisitorNeedsDefOf.All.ToArray());
            actor.ModifyNeed(VisitorNeedsDefOf.Guidance, n => 10);
        }

        public IEnumerable<VisitorProperties> Find(Func<VisitorProperties, bool> pred)
        {
            foreach (var v in this.ActorsAdventuring.Where(pred))
                yield return v;
        }
     
        internal IEnumerable<VisitorProperties> GetVisitorProperties()
        {
            foreach (var v in this.ActorsAdventuring)
                yield return v;
        }
        internal VisitorProperties GetVisitorProperties(Actor actor)
        {
            return this.ActorsAdventuring.FirstOrDefault(v => v.Actor == actor);
        }
        internal void OnTargetSelected(IUISelection info, ISelectable selected)
        {
        }
        public GroupBox GetUI()
        {
            var box = new GroupBox();
            var list = new ListBoxNew<VisitorProperties, ButtonNew>(200, UIManager.LargeButton.Height * 8, (props, container) => 
            {
                var npc = props.Actor;
                var btn = ButtonNew.CreateBig(()=>UI.SelectionManager.Select(npc), container.Client.Width, npc.RenderIcon(), () => npc.Npc.FullName, () => npc.Exists ? "Visiting" : (props.Discovered ? "" : "Unknown"));
                return btn; 
            });

            var filters = new GroupBox().AddControlsLineWrap(new[]{
                new Button("All", ()=>list.Filter(i=>true)),
                new Button("Visiting", ()=>list.Filter(i=>i.Actor.Exists)),
                new Button("Away", ()=>list.Filter(i=>!i.Actor.Exists && i.Discovered)),
                new Button("Unknown", ()=>list.Filter(i=>!i.Discovered)),
            });

            list.AddItems(this.ActorsAdventuring.ToArray());

            list.OnGameEventAction = e =>
              {
                  switch (e.Type)
                  {
                      case Components.Message.Types.NewAdventurerCreated:
                          var actor = e.Parameters[0] as Actor;
                          list.AddItems(this.ActorsAdventuring.Find(v=>v.Actor == actor));
                          break;

                      default:
                          break;
                  }
              };
            box.AddControlsVertically(filters, list);
            list.OnShowAction = () =>
            {
                list.Clear();
                list.AddItems(this.ActorsAdventuring.ToArray());
            };
            return box;

        }
        public void ResolveReferences()
        {
            foreach (var actor in this.ActorsAdventuring.Select(p => p.Actor)) // i added this to add visitor needs to existing visitors because I wasn't saving them in the needscomponent class
            {
                // TODO move this somewhere else
                if(this.World.Map.Net is Server) 
                    if (!actor.GetNeeds(VisitorNeedsDefOf.NeedCategoryVisitor).Any())
                            MakeVisitor(actor);
                if (!actor.Exists) // hacky. in progress of finding best way to save unspawned actors
                    this.World.Map.Net.Instantiate(actor);
            }
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Populated.Save(tag, "Populated");
            this.TickCount.Save(tag, "Tick");
            this.ActorsAdventuring.SaveNewBEST(tag, "Population");
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            this.Populated.TryLoad(tag, "Populated");
            this.TickCount.TryLoad(tag, "Tick");
            this.ActorsAdventuring.TryLoad(tag, "Population", this);
            return this;
        }
        public void Write(BinaryWriter w)
        {
            this.ActorsAdventuring.Write(w);
        }

        public ISerializable Read(BinaryReader r)
        {
            this.ActorsAdventuring.Initialize(r, this);
            return this;
        }
    }
}
