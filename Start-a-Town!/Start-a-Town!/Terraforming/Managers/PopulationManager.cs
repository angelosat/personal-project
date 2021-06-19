using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes.StaticMaps;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class PopulationManager : ISaveable, ISerializable
    {
        static public class Packets
        {
            static int PacketVisitorArrived, PacketAdventurerCreated, PacketVisitorPropertiesUpdated;
            public static void Init()
            {
                PacketVisitorArrived = Network.RegisterPacketHandler(ReceiveNotifyVisit);
                PacketAdventurerCreated = Network.RegisterPacketHandler(ReceiveNotifyAdventurerCreated);
                PacketVisitorPropertiesUpdated = Network.RegisterPacketHandler(VisitorPropertiesUpdated);
            }
            public static void SyncVisitorProperties(IObjectProvider net, List<VisitorProperties> actorsAdventuring)
            {
                throw new Exception();
                var server = net as Server;
                var w = server.OutgoingStream;
                w.Write(PacketVisitorPropertiesUpdated);
                actorsAdventuring.Sync(w);
            }
            private static void VisitorPropertiesUpdated(IObjectProvider net, BinaryReader r)
            {
                var world = net.Map.World as StaticWorld;
                world.Population.ActorsAdventuring.Sync(r);
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
            private static void ReceiveNotifyAdventurerCreated(IObjectProvider net, BinaryReader r)
            {
                var client = net as Client;
                var actorID = r.ReadInt32();
                var actor = client.GetNetworkObject(actorID) as Actor;
                var world = client.Map.World as StaticWorld;
                world.Population.RegisterActor(client, actor);
            }
            private static void ReceiveNotifyVisit(IObjectProvider net, BinaryReader r)
            {
                if (net is Server)
                    throw new Exception();
                var actorID = r.ReadInt32();
                var actor = net.GetNetworkObject(actorID) as Actor;
                ReportVisit(net, actor);
            }

            private static void ReportVisit(IObjectProvider net, Actor actor)
            {
                var props = actor.GetVisitorProperties();
                //net.Report(string.Format("{0} is {1}", actor.Name, !actor.IsSpawned ? ("visiting" + (props.Discovered ? "" : " for the first time!")) : "departing"));
                net.Report($"{actor.Name} is {(!actor.IsSpawned ? ("visiting" + (props.Discovered ? "" : " for the first time!")) : "departing")}");

                props.Discovered = true;
            }

            
        }
        static public void Init()
        {
            Packets.Init();
            OffsiteAreaDefOf.Init();
            VisitorNeedsDefOf.Init();
        }

        bool Populated;
        readonly List<VisitorProperties> ActorsAdventuring = new(ActorsCap);
        readonly public StaticWorld World;
        const int ActorsInitial = 1;//4;
        const int ActorsCap = 8;
        const float TickRate = 1 / 3f, InitialChance = .05f,  VisitChanceBaseRate = .001f;// 2 seconds per tick //1 tick per second 
        const int InitialApproval = 50;
        //const float InitialApproval = .5f;

        int TickCount = (int)(Engine.TicksPerSecond / TickRate);
        public PopulationManager(StaticWorld world)
        {
            this.World = world;
            //this.Actors = new List<Actor>(ActorsCap);
        }
        public void Update(IObjectProvider net)
        {
            if (net is Server)
                this.HandleErrors();
            foreach (var v in this.ActorsAdventuring)
                v.Tick();
            this.TickCount--;
            if (this.TickCount > 0)
                return;
            this.TickCount = (int)(Engine.TicksPerSecond / TickRate); // TickRate * Engine.TicksPerSecond;
            this.Tick(net);
        }

        private void HandleErrors()
        {
            var map = this.World.Map;
            var net = map.Net;
            var allActors = net.GetNetworkObjects().OfType<Actor>();
            var citizens = map.Town.GetAgents();
            foreach(var actor in allActors)
            {
                if (citizens.Contains(actor))
                    continue;
                if(!this.ActorsAdventuring.Any(v=>v.Actor == actor))
                {
                    this.Populated = true;
                    //var props = new VisitorProperties(this.World, actor, InitialChance, InitialApproval) { OffsiteArea = OffsiteAreaDefOf.Forest };
                    //this.ActorsAdventuring.Add(props);
                    Packets.SendNotifyAdventurerCreated(actor);
                    RegisterActor(actor.Net as Server, actor);
                    Log.WriteToFile($"{actor.Name} is not a town member and was missing from the world population list.");
                }
            }
        }

        void Tick(IObjectProvider net)
        {
            this.PopulateNew(net);
            //foreach (var v in this.ActorsAdventuring)
            //    v.Tick();
            //Packets.SyncVisitorProperties(Server.Instance, this.ActorsAdventuring);
            return;
            this.TryVisitDepart(net);
            this.TickHeroes(net);
        }

        private void TickHeroes(IObjectProvider net)
        {
            foreach (var vis in this.ActorsAdventuring)
            {

                var actor = vis.Actor;
                if (actor.IsSpawned)
                    continue;
                if (net is Server)
                    vis.OffsiteArea?.Tick(vis);

                TickNeeds(actor);
            }
        }

        private static void TickNeeds(Actor actor)
        {
            foreach (var n in actor.GetNeeds())
                n.Tick(actor);
        }
        void TryVisitDepart(IObjectProvider net)
        {
            if (net is Client)
                return;
            for (int i = 0; i < this.ActorsAdventuring.Count; i++)
            {
                var visitorProps = this.ActorsAdventuring[i];
                //var visitChance = visitorProps.GetVisitDepartChance();
                //if (!this.World.Random.Chance(visitChance))
                //{
                //    continue;
                //}
                var actor = visitorProps.Actor;
                var isVisiting = actor.IsSpawned;
                var map = Net.Server.Instance.Map as StaticMap;
                //var world = map.World as StaticWorld;
                var world = this.World;
                if (isVisiting)
                {
                    // moved departing to a taskgiver
                    //if (world.Random.Chance(visitorProps.GetDepartChance()))
                    //{

                    //}
                }
                else
                {
                    if (world.Random.Chance(visitorProps.GetVisitChance()))
                    {
                        Packets.SendNotifyVisit(actor);
                        Vector3 coords = map.GetRandomEdgeCell().Above();
                        map.SyncSpawn(actor, coords, Vector3.Zero);
                        visitorProps.Arrive();
                        visitorProps.ResetTimer(world.Clock);//.Clock);
                    }
                }
            }
            Packets.SyncVisitorProperties(Server.Instance, this.ActorsAdventuring);
        }
        private void PopulateNew(IObjectProvider net)
        {
            if (net is Client)
                return;
            if (this.ActorsAdventuring.Count < ActorsCap)
            {
                Actor actor = GenerateVisitor();
                Server.Instance.SyncInstantiate(actor);
                //var actor = GameObject.Create()
                Packets.SendNotifyAdventurerCreated(actor);
                RegisterActor(Server.Instance, actor);
            }
        }
        //private void Populate()
        //{
        //    this.Populated = true;
        //    for (int i = 0; i < ActorsInitial; i++)
        //    {
        //        Actor actor = GenerateVisitor();
        //        Server.Instance.SyncInstantiate(actor);
        //        //var actor = GameObject.Create()
        //        Packets.SendNotifyAdventurerCreated(actor);
        //        RegisterActor(Server.Instance, actor);
        //    }
        //}

        private static Actor GenerateVisitor()
        {
            var visitor = ActorDefOf.Npc.Create() as Actor;
            //visitor.ModifyNeed(VisitorNeedsDefOf.Guidance, n => 10);
            visitor.Inventory.Insert(ItemDefOf.Coins.Create().SetStackSize(500));
            return visitor;
        }

        private void RegisterActor(IObjectProvider net, Actor actor)
        {
            //this.Actors.Add(actor);
            //this.ActorsAdventuring[actor] = new VisitorProperties(actor, InitialChance, .5f);
            //var props = new VisitorProperties(net, actor, InitialChance, InitialApproval) { OffsiteArea = OffsiteAreaDefOf.Forest };
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
            //if (selected is TargetArgs targetArgs)
            //    if (targetArgs.Object is Actor actor)
            //if (selected is Actor actor)
            //{
            //    if (this.GetVisitorProperties(actor) is VisitorProperties props)
            //    {
            //        info.AddTabAction("Visitor", props.ShowGUI);
            //    }
            //}
        }
        public GroupBox GetUI()
        {
            var box = new GroupBox();
            var list = new ListBoxNew<VisitorProperties, ButtonNew>(200, UIManager.LargeButton.Height * 8, (props, container) => 
            {
                var npc = props.Actor;
                //var btn = ButtonNew.CreateBig(props.ShowGUI, container.Client.Width, npc.RenderIcon(), () => npc.Npc.FullName, () => npc.IsSpawned ? "Visiting" : (npc.Npc.Discovered ? "" : "Unknown"));
                //var btn = ButtonNew.CreateBig(props.ShowQuestsGUI, container.Client.Width, npc.RenderIcon(), () => npc.Npc.FullName, () => npc.IsSpawned ? "Visiting" : (npc.Npc.Discovered ? "" : "Unknown"));
                //var btn = ButtonNew.CreateBig(props.ShowGUI, container.Client.Width, npc.RenderIcon(), () => npc.Npc.FullName, () => npc.IsSpawned ? "Visiting" : (npc.Npc.Discovered ? "" : "Unknown"));
                var btn = ButtonNew.CreateBig(()=>UI.UISelectedInfo.Refresh(npc), container.Client.Width, npc.RenderIcon(), () => npc.Npc.FullName, () => npc.IsSpawned ? "Visiting" : (props.Discovered ? "" : "Unknown"));
                return btn; 
            });

            var filters = new GroupBox().AddControlsLineWrap(new[]{
                new Button("All", ()=>list.Filter(i=>true)),
                new Button("Visiting", ()=>list.Filter(i=>i.Actor.IsSpawned)),
                new Button("Away", ()=>list.Filter(i=>!i.Actor.IsSpawned && i.Discovered)),
                new Button("Unknown", ()=>list.Filter(i=>!i.Discovered)),
            });

            //list.AddItems(this.ActorsAdventuring.Values.ToArray());
            list.AddItems(this.ActorsAdventuring.ToArray());

            list.OnGameEventAction = e =>
              {
                  switch (e.Type)
                  {
                      case Components.Message.Types.NewAdventurerCreated:
                          var actor = e.Parameters[0] as Actor;
                          //list.AddItems(this.ActorsAdventuring[actor]);
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
                //l.AddItems(this.ActorsAdventuring.Values.ToArray());
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
                if (!actor.IsSpawned) // hacky. in progress of finding best way to save unspawned actors
                    this.World.Map.Net.Instantiate(actor);
            }
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Populated.Save(tag, "Populated");
            this.TickCount.Save(tag, "Tick");
            this.ActorsAdventuring.SaveNewBEST(tag, "Population");
            //this.SaveUnspawnedActors(tag, "UnspawnedActors");
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            this.Populated.TryLoad(tag, "Populated");
            this.TickCount.TryLoad(tag, "Tick");
            this.ActorsAdventuring.TryLoad(tag, "Population", this);
            //this.LoadUnspawnedActors(tag, "UnspawnedActors");
            return this;
        }
        void SaveUnspawnedActors(SaveTag tag, string name)
        {
            //this.ActorsAdventuring.Where(a => !a.Actor.IsSpawned).SaveNewBEST(name);
            var unspawned = this.ActorsAdventuring.Where(a => !a.Actor.IsSpawned).Select(v=>v.Actor);
            var t = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Compound);
            foreach (var i in unspawned)
                t.Add(i.Save());
            tag.Add(t);
        }
        void LoadUnspawnedActors(SaveTag tag, string name)
        {
            tag.TryGetTag(name, tlist =>
            {
                var list = tlist.Value as List<SaveTag>;
                foreach (var t in list)
                {
                    var actor = GameObject.Load(t) as Actor;
                    this.GetVisitorProperties(actor).Actor = actor;
                }
            });
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
