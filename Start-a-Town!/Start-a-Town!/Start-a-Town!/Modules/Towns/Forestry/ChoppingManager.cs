using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.AI;
using Start_a_Town_.GameEvents;

namespace Start_a_Town_.Towns.Forestry
{
    public class ChoppingManager : TownComponent
    {
        public override string Name
        {
            get { return "Forestry"; }
        }
        HashSet<int> QueuedTreesIDs = new HashSet<int>();
        //HashSet<GameObject> QueuedTrees// = new HashSet<GameObject>();
        //{
        //    get
        //    {
        //        HashSet<GameObject> list = new HashSet<GameObject>(this.QueueTreeIDs.Select(id=>this.Town.Map.GetNetwork().GetNetworkObject(id)));
        //        return list;
        //    }
        //}
        int GroveIDSequence = 1;
        Dictionary<int, Grove> Groves = new Dictionary<int, Grove>();
        public List<Grove> GetGroves()
        {
            return this.Groves.Values.ToList();
        }
        public List<GameObject> GetTrees()
        {
            List<GameObject> list = this.QueuedTreesIDs.Select(id => this.Town.Map.GetNetwork().GetNetworkObject(id)).ToList();
            return list;
        }
        Dictionary<GameObject, ChopOrder> PendingOrders = new Dictionary<GameObject, ChopOrder>();

        public List<Vector3> GetPositions()
        {
            List<Vector3> list = new List<Vector3>();
            foreach (var tree in this.GetTrees())// this.QueuedTrees)
                list.Add(tree.Global.Round() - Vector3.UnitZ);
            return list;
        }

        public ChoppingManager(Towns.Town town)
        {
            // TODO: Complete member initialization
            this.Town = town;
        }

        public override UI.GroupBox GetInterface()
        {
            return new ChoppingManagerUI(this);
        }
        public override void Handle(IObjectProvider net, Packet msg)
        {
            switch (msg.PacketType)
            {
                case PacketType.ChoppingDesignation:
                    msg.Payload.Deserialize(r =>
                    {
                        int entityID;
                        Vector3 begin, end;
                        bool value;
                        PacketChoppingDesignation.Read(r, out entityID, out begin, out end, out value);
                        this.Designate(begin, end, value);
                        var server = net as Server;
                        if (server != null)
                            server.Enqueue(PacketType.ChoppingDesignation, msg.Payload, SendType.OrderedReliable, true);
                    });
                    break;

                case PacketType.ZoneGrove:
                    msg.Payload.Deserialize(r =>
                    {
                        int entityID, zoneID;
                        Vector3 begin;
                        int w, h;
                        bool remove;
                        PacketZone.Read(r, out entityID, out zoneID, out begin, out w, out h, out remove);
                        Grove grove;
                        int groveID = zoneID;// 0;
                        if (remove)
                        {
                            //grove = this.GetGroveAt(begin);
                            //if (grove != null)
                            if (zoneID != 0)
                                this.RemoveGrove(entityID, zoneID);
                            else
                                this.RemoveGrove(entityID, begin);
                        }
                        else
                        {
                            //grove = this.GetGroveAt(begin);
                            //if (grove != null)
                            //{
                            //    if (this.Town.Map.Net is Client)
                            //    {
                            //        grove.GetInterface().ToWindow("Grove " + grove.ID.ToString()).Show();
                            //        return;
                            //    }
                            //}
                            //else
                            //{
                                grove = new Grove(this, this.GroveIDSequence++, begin, w, h);
                                groveID = grove.ID;
                                AddGrove(entityID, grove);
                            //}
                        }
                        var server = net as Server;
                        if (server != null)
                            server.Enqueue(PacketType.ZoneGrove, PacketZone.Write(entityID, groveID, begin, w, h, remove), SendType.OrderedReliable, true);
                    });
                    break;

                case PacketType.GroveEdit:
                    msg.Payload.Deserialize(r =>
                        {
                            //var id = r.ReadInt32();
                            //var density = r.ReadSingle();
                            int id;
                            string name;
                            float density;
                            PacketGroveEdit.Read(r, out id, out name, out density);
                            var grove = this.Groves[id];
                            grove.Name = name;
                            grove.TargetDensity = density;
                            this.Town.Map.EventOccured(Message.Types.GroveEdited, grove);
                            this.Town.Map.EventOccured(Message.Types.GrovesUpdated);

                            var server = net as Server;
                            if (server != null)
                                server.Enqueue(PacketType.GroveEdit, msg.Payload, SendType.OrderedReliable, true);
                        });
                    break;

                default:
                    break;
            }
        }

        private void AddGrove(int senderID, Grove grove)
        {
            this.Groves.Add(grove.ID, grove);
            this.Town.Map.EventOccured(Message.Types.GrovesUpdated);
            this.Town.Map.EventOccured(Message.Types.GroveAdded, senderID, grove);
        }
        private void RemoveGrove(int senderID, int groveID)
        {
            var grove = this.Groves[groveID];
            this.Groves.Remove(groveID);
            this.Town.Map.EventOccured(Message.Types.GrovesUpdated);
            this.Town.Map.EventOccured(Message.Types.GroveRemoved, senderID, grove);
        }
        private void RemoveGrove(int senderID, Vector3 global)
        {
            var grove = this.GetGroveAt(global);
            if (grove != null)
                this.RemoveGrove(senderID, grove.ID);
        }
        public Grove GetGroveAt(Vector3 global)
        {
            foreach (var grove in this.Groves.Values.ToList())
                if (grove.Contains(global))
                    return grove;
            return null;
        }

        //public override void HandlePacket(Net.Server server, Net.Packet msg)
        //{
        //    switch (msg.PacketType)
        //    {
        //        case PacketType.ChoppingDesignation:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                int entityID;
        //                Vector3 begin, end;
        //                bool value;
        //                PacketChoppingDesignation.Read(r, out entityID, out begin, out end, out value);
        //                this.Designate(begin, end, value);
        //                server.Enqueue(PacketType.ChoppingDesignation, msg.Payload, SendType.OrderedReliable, true);
        //            });
        //            break;

        //        case PacketType.ChoppingZoneCreate:
        //            msg.Payload.Deserialize(r =>
        //            {
        //                var p = new PacketCreateChoppingZone(r);
        //                var newpacket = new PacketCreateChoppingZone(p.EntityID, p.Global, p.Width, p.Height);
        //                GenerateWork(p.Global, p.Width, p.Height);
        //                server.Enqueue(PacketType.ChoppingZoneCreate, newpacket.Write(), SendType.OrderedReliable);
        //            });
        //            break;

        //        default:
        //            break;
        //    }
        //}

        private void Designate(Vector3 begin, Vector3 end, bool value)
        {
            //foreach(var pos in begin.GetBox(end))
            //{
            //    var above = pos + Vector3.UnitZ;
            //}
            foreach(var tree in this.GetTrees())//.QueuedTrees.ToList())
            {
                if (!tree.Exists)
                {
                    //this.QueuedTrees.Remove(tree);
                    this.QueuedTreesIDs.Remove(tree.InstanceID);
                    this.CancelOrder(tree);
                }
            }

            var bbox = new BoundingBox(begin + Vector3.UnitZ, end + Vector3.UnitZ);
            var trees = from entity in this.Town.Map.GetObjects(bbox)
                        where entity.HasComponent<TreeComponent>()
                        select entity;
            foreach (var tree in trees)
            {
                if (value)
                    //this.QueuedTrees.Add(tree);
                    this.QueuedTreesIDs.Add(tree.InstanceID);
                else
                {
                    //if (this.QueuedTrees.Remove(tree))
                    this.QueuedTreesIDs.Remove(tree.InstanceID);
                    {
                        CancelOrder(tree);
                    }
                }
            }
        }

        private void CancelOrder(GameObject tree)
        {
            ChopOrder order;
            if (this.PendingOrders.TryGetValue(tree, out order))
            {
                order.Job.Cancel();
                this.PendingOrders.Remove(tree);
            }
        }

        public override void OnUpdate()
        {
            if (this.Town.Map.Net is Net.Client)
                return;
            foreach(var tree in this.GetTrees())//.QueuedTrees)
            {
                ChopOrder order;
                if(this.PendingOrders.TryGetValue(tree, out order))
                {
                    continue;
                }
                this.CreateJob(tree);
            }
            foreach (var grove in this.Groves.Values)
                grove.GenerateWork();
        }
        private void GenerateWork(Vector3 global, int w, int h)
        {
            var end = global + new Vector3(w-1, h-1,0);
            var bbox = new BoundingBox(global,end);
            var trees = from entity in this.Town.Map.GetObjects(bbox)
                        where entity.HasComponent<TreeComponent>()
                        select entity;
            foreach(var tree in trees)
            {
                //if (this.QueuedTrees.Contains(tree))
                if (this.QueuedTreesIDs.Contains(tree.InstanceID))
                    continue;

                CreateJob(tree);
            }
        }

        private void CreateJob(GameObject tree)
        {
            //this.QueuedTrees.Add(tree);
            this.QueuedTreesIDs.Add(tree.InstanceID);
            AIJob job = new AIJob();
            var i = new TreeComponent.InteractionChopping();
            job.AddStep(new AIInstruction(new TargetArgs(tree), i));
            job.Labor = AILabor.Lumberjack;

            this.Town.AddJob(job);

            this.PendingOrders.Add(tree, new ChopOrder(tree, job));
        }

        class ChopOrder
        {
            public GameObject Tree;
            public AIJob Job;
            public ChopOrder(GameObject tree, AIJob job)
            {
                this.Tree = tree;
                this.Job = job;
            }
        }

        internal override void OnGameEvent(GameEvent e)
        {
            foreach (var grove in this.Groves.Values)
                grove.OnGameEvent(e);

            switch(e.Type)
            {
                case Message.Types.EntityDespawned:
                    GameObject entity;
                    EventEntityDespawned.Read(e.Parameters, out entity);
                    this.QueuedTreesIDs.Remove(entity.InstanceID);
                    break;

                default:
                    break;
            }
        }

        public override List<SaveTag> Save()
        {
            var tag = new List<SaveTag>();
            //tag.Add(this.QueuedTrees.Select(t => t.InstanceID).Save("Trees"));
            tag.Add(this.QueuedTreesIDs.Save("Trees"));

            return tag;
        }
        public override void Load(SaveTag tag)
        {
            //List<int> treeIDs = new List<int>();
            //if (tag.TryGetTagValue<List<int>>("Trees", out treeIDs))
            //    foreach (var id in treeIDs)
            //        //this.QueuedTrees.Add(this.Town.Map.GetNetwork().GetNetworkObject(id));
            //        this.QueuedTreesIDs.Add(id);

            tag.TryGetTagValue<List<SaveTag>>("Trees", v => this.QueuedTreesIDs = new HashSet<int>(new List<int>().Load(v)));
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            //w.Write(this.QueuedTrees.Select(t => t.InstanceID).ToList());
            w.Write(this.QueuedTreesIDs.ToList());
        }
        public override void Read(System.IO.BinaryReader r)
        {
            foreach(var id in r.ReadListInt())
                this.QueuedTreesIDs.Add(id);
        }
    }
}
