using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.AI;
using Start_a_Town_.GameEvents;
using Start_a_Town_.GameModes;
using Start_a_Town_.Net.Packets;
using Start_a_Town_.UI;
using Start_a_Town_.Towns;
using Start_a_Town_.Towns.Forestry;

namespace Start_a_Town_
{
    public class ChoppingManager : TownComponent
    {
        public enum Types { Chopping, Foraging }
        static public readonly Icon ChopIcon = new(ItemContent.AxeFull);// GameObject.Objects[ItemTemplate.Axe.GetID()].GetIcon();
        static public readonly Icon ForageIcon = new(ItemContent.BerriesFull);// GameObject.Objects[GameObject.Types.Berries].GetIcon();


        public override string Name
        {
            get { return "Forestry"; }
        }
        readonly HashSet<int> QueuedForaging = new();
        
        int GroveIDSequence = 1;
        readonly Dictionary<int, Grove> Groves = new();
        public List<Grove> GetGroves()
        {
            return this.Groves.Values.ToList();
        }
        public List<GameObject> GetTrees()
        {
            var list = this.ChoppingTasks.Select(id => this.Town.Map.Net.GetNetworkObject(id)).ToList();
            return list;
        }
        public List<GameObject> GetPlants()
        {
            var list = this.QueuedForaging.Select(id => this.Town.Map.Net.GetNetworkObject(id)).ToList();
            return list;
        }
        readonly Dictionary<GameObject, ChopOrder> PendingForageOrders = new();
        public HashSet<int> ChoppingTasks = new();

        public List<Vector3> GetPositions()
        {
            var list = new List<Vector3>();
            foreach (var tree in this.GetTrees())// this.QueuedTrees)
                if (tree != null) //no idea why it's null when a tree just gets despawned // BECAUSE THE NETWORK RETURNS NULL FOR THE PARTICULAR INSTANCEID
                    list.Add(tree.Global.Round() - Vector3.UnitZ);
            return list;
        }
        public List<Vector3> GetForagingPositions()
        {
            var list = new List<Vector3>();
            foreach (var plant in this.Town.Map.Net.GetNetworkObjects(this.QueuedForaging.ToArray()))
                list.Add(plant.Global.Round());// - Vector3.UnitZ);
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
                
                case PacketType.ForagingSingle:
                    msg.Payload.Deserialize(r =>
                    {
                        PacketIntInt.Read(r, out int actorID, out int plantID);
                        var tree = net.GetNetworkObject(plantID);
                        if (!tree.HasComponent<PlantComponent>())
                            throw new Exception();
                        var exists = this.QueuedForaging.Contains(plantID);
                        if (!exists)
                            this.QueuedForaging.Add(plantID);
                        else
                            this.QueuedForaging.Remove(plantID);
                        if (net is Server server)
                            server.Enqueue(PacketType.ForagingSingle, msg.Payload, SendType.OrderedReliable, true);
                    });
                    break;
                case PacketType.ChoppingDesignation:
                    msg.Payload.Deserialize(r =>
                    {
                        PacketEntityDesignation.Read(r, out int entityID, out Vector3 begin, out Vector3 end, out bool value);
                        this.Designate((int)Types.Chopping, begin, end, value);
                        if (net is Server server)
                            server.Enqueue(PacketType.ChoppingDesignation, msg.Payload, SendType.OrderedReliable, true);
                    });
                    break;

                case PacketType.ZoneGrove:
                    msg.Payload.Deserialize(r =>
                    {
                        PacketZone.Read(r, out int entityID, out int zoneID, out Vector3 begin, out int w, out int h, out bool remove);
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
                        if (net is Server server)
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

                case PacketType.ForagingDesignate:
                    msg.Payload.Deserialize(r =>
                    {
                        PacketZone.Read(r, out int actorid, out int zoneid, out Vector3 start, out int w, out int h, out bool value);
                        this.DesignateForaging(start, w, h, value);
                    });
                    break;

                default:
                    break;
            }
        }

        private void DesignateForaging(Vector3 start, int w, int h, bool value)
        {
            foreach (var plant in this.Town.Net.GetNetworkObjects(this.QueuedForaging.ToArray()))
                if (!plant.IsSpawned)
                {
                    this.QueuedForaging.Remove(plant.RefID);
                    this.PendingForageOrders.Remove(plant);
                }

            var end = start + new Vector3(w - 1, h - 1, 0);
            var plants = from entity in this.Town.Map.GetObjects(start + Vector3.UnitZ, end + Vector3.UnitZ)
                         where entity.HasComponent<PlantComponent>()
                         select entity;
            foreach (var p in plants)
            {
                if (!value)
                    this.QueuedForaging.Add(p.RefID);
                else
                {
                    this.QueuedForaging.Remove(p.RefID);
                    //CancelOrder(p);
                }
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

        internal void Designate(int type, List<int> ids, bool remove)
        {
            var designationType = (Types)type;
            HashSet<int> collection = null;
            switch (designationType)
            {
                case Types.Chopping:
                    collection = this.ChoppingTasks;
                    break;

                case Types.Foraging:
                    collection = this.QueuedForaging;
                    break;

                default:
                    break;
            }
            foreach (var p in ids)
            {
                if (!remove)
                    collection.Add(p);
                else
                    collection.Remove(p);
            }
            UpdateQuickButtons();
        }

        public void Designate(int type, Vector3 begin, Vector3 end, bool value)
        {
            var bbox = new BoundingBox(begin + Vector3.UnitZ, end + Vector3.UnitZ);
            var designationType = (Types)type;
            switch (designationType)
            {
                case Types.Chopping:
                    this.DesignateChopping(value, bbox);
                    break;

                case Types.Foraging:
                    this.DesignateForaging(value, bbox);
                    break;

                default:
                    break;
            }
            UpdateQuickButtons();

        }

        private void DesignateChopping(bool value, BoundingBox bbox)
        {
            var trees = from entity in this.Town.Map.GetObjects(bbox)
                        where entity.HasComponent<TreeComponent>()
                        select entity;
            foreach (var tree in trees)
            {
                if (!value)
                {
                    this.ChoppingTasks.Add(tree.RefID);
                }
                else
                {
                    this.ChoppingTasks.Remove(tree.RefID);
                }
            }
        }
        private void DesignateForaging(bool value, BoundingBox bbox)
        {
            var plants = from entity in this.Town.Map.GetObjects(bbox)
                         where entity.HasComponent<PlantComponent>()
                         select entity;
            foreach (var p in plants)
            {
                if (!value)
                    this.QueuedForaging.Add(p.RefID);
                else
                {
                    this.QueuedForaging.Remove(p.RefID);
                    //CancelOrder(p);
                }
            }
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

            switch (e.Type)
            {
                case Message.Types.PlantHarvested:
                    var plant = e.Parameters[0] as GameObject;
                    this.QueuedForaging.Remove(plant.RefID);
                    this.UpdateQuickButtons();
                    break;

                case Message.Types.PlantReady:
                    this.UpdateQuickButtons();
                    break;

                case Message.Types.EntityDespawned:
                    GameObject entity;
                    EventEntityDespawned.Read(e.Parameters, out entity);
                    this.ChoppingTasks.Remove(entity.RefID);
                    this.QueuedForaging.Remove(entity.RefID);
                    //AITaskChopping task;
                    //if(this.PendingChoppingTasks.TryGetValue(entity, out task))
                    //{
                    //    task.Cancel();
                    //    this.PendingChoppingTasks.Remove(entity);
                    //}
                    break;

            

                //case Message.Types.TaskComplete:
                //    var task = e.Parameters[0] as AITaskChopping;
                //    var actor = e.Parameters[1];
                //    if (task != null)
                //        this.PendingChoppingTasks.Remove(task.Plant);
                //    break;

                default:
                    break;
            }
        }

      
        
        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.ChoppingTasks.ToList().Save("ChoppingTasks"));
        }
        public override void Load(SaveTag tag)
        {
            //List<int> treeIDs = new List<int>();
            //if (tag.TryGetTagValue<List<int>>("Trees", out treeIDs))
            //    foreach (var id in treeIDs)
            //        //this.QueuedTrees.Add(this.Town.Map.GetNetwork().GetNetworkObject(id));
            //        this.QueuedTreesIDs.Add(id);

            //tag.TryGetTagValue<List<SaveTag>>("Trees", v => this.QueuedTreesIDs = new HashSet<int>(new List<int>().Load(v)));
            tag.TryGetTagValue<List<SaveTag>>("ChoppingTasks", v => this.ChoppingTasks = new HashSet<int>(new List<int>().Load(v)));
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.ChoppingTasks.ToList());
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.ChoppingTasks = new HashSet<int>(r.ReadListInt());
        }

        public override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera cam)
        {
            // i started drawing icons instead
            //cam.DrawGridCells(sb, Color.Yellow * .5f, this.GetPositions().Select(s => s + Vector3.UnitZ));
            //cam.DrawGridCells(sb, Color.Yellow * .5f, this.GetForagingPositions());
        }
        public override void DrawUI(SpriteBatch sb, IMap map, Camera cam)
        {
            this.DrawIcons(sb, map, cam, ChopIcon, GetTrees());
            this.DrawIcons(sb, map, cam, ForageIcon, GetPlants());
        }
        
        private void DrawIcons(SpriteBatch sb, IMap map, Camera camera, Icon icon, IEnumerable<GameObject> objects)
        {
            foreach (var parent in objects)
                icon.DrawAboveEntity(sb, camera, parent);
                //parent.DrawIconAbove(sb, camera, icon, .5f);
        }
        internal override void OnContextMenuCreated(IContextable obj, ContextArgs a)
        {
            if (obj is not TargetArgs tree)
                return;
            if (tree.Type != TargetType.Entity)
                return;
            if (!tree.Object.HasComponent<TreeComponent>())
                return;
            var orderExists = this.ChoppingTasks.Contains(tree.Object.RefID);
            a.Actions.Add(new ContextAction(orderExists ? "Cancel: Chop" : "Order: Chop", () =>
            {
                Client.Instance.Send(PacketType.ChoppingSingle, PacketIntInt.Write(PlayerOld.Actor.RefID, tree.Object.RefID));
                return true;
            }));
        }

        
        internal override void OnContextActionBarCreated(ContextActionBar.ContextActionBarArgs a)
        {
            var obj = a.Target;
            if (obj == null)
                return;
            if (obj.Type != TargetType.Entity)
                return;
            var plant = obj.Object as Plant;
            if (obj.Object.HasComponent<PlantComponent>())
            {
                var orderExists = this.ChoppingTasks.Contains(plant.RefID);// this.QueuedTreesIDs.Contains(obj.Object.InstanceID);
                a.Actions.Add(new ContextActionBar.ContextActionBarAction(() => Client.Instance.Send(PacketType.ChoppingSingle, PacketIntInt.Write(PlayerOld.Actor.RefID, plant.RefID)), new Icon(UIManager.Icons32, 12, 32), orderExists ? "Cancel: Cut" : "Order: Cut"));

                if (plant.IsHarvestable)
                {
                    orderExists = this.QueuedForaging.Contains(plant.RefID);
                    a.Actions.Add(new ContextActionBar.ContextActionBarAction(() => Client.Instance.Send(PacketType.ForagingSingle, PacketIntInt.Write(PlayerOld.Actor.RefID, plant.RefID)), new Icon(UIManager.Icons32, 12, 32), orderExists ? "Cancel: Forage" : "Order: Forage"));
                }

            }
            
        }

        internal bool IsChoppingTask(GameObject tree)
        {
            return this.ChoppingTasks.Contains(tree.RefID);
        }
        internal bool IsForagingTask(GameObject obj)
        {
            return this.QueuedForaging.Contains(obj.RefID);
        }
        public void EditChopping()
        {
            ToolManager.SetTool(new ToolDesignatePositions((a, b, c) => this.Add(Types.Chopping, a, b, c), this.GetPositions) { ValidityCheck = IsPositionValid });
        }
        private void EditForaging()
        {
            ToolManager.SetTool(new ToolDesignatePositions((a, b, c) => this.Add(Types.Foraging, a, b, c), this.GetPositions) { ValidityCheck = IsPositionValid });
        }

        private void Add(Types type, Vector3 start, Vector3 end, bool value)
        {
            PacketEntityDesignation.Send(Client.Instance, (int)type, start, end, value);
            //Client.Instance.Send(PacketType.ChoppingDesignation, PacketChoppingDesignation.Write(Player.Actor.InstanceID, start, end, !value));
        }
        private bool IsPositionValid(Vector3 arg)
        {
            return !Block.IsBlockSolid(this.Map, arg + Vector3.UnitZ);
        }
        internal override IEnumerable<Tuple<string, Action>> OnQuickMenuCreated()
        {
            yield return new Tuple<string, Action>("Chop trees", this.EditChopping);
            yield return new Tuple<string, Action>("Forage", this.EditForaging);
        }

        static readonly IconButton ButtonChopAdd = new(ChopIcon) { HoverText = "Chop down" };
        static readonly IconButton ButtonChopRemove = new(ChopIcon, Icon.Cross) { HoverText = "Cancel chop down" };

        //static IconButton ButtonForageAdd = new IconButton(ForageIcon) { HoverText = "Forage" };
        static readonly QuickButton ButtonForageAdd = new(ForageIcon, null, "Forage");// { HoverText = "Forage" };
        static readonly IconButton ButtonForageRemove = new(ForageIcon, Icon.Cross) { HoverText = "Cancel forage" };


        static void ChopDownAdd(List<TargetArgs> targets)
        {
            PacketEntityDesignation.Send(Client.Instance, (int)Types.Chopping, targets, false);
        }
        static void ChopDownRemove(List<TargetArgs> targets)
        {
            PacketEntityDesignation.Send(Client.Instance, (int)Types.Chopping, targets, true);
        }
        static void ForageAdd(List<TargetArgs> targets)
        {
            PacketEntityDesignation.Send(Client.Instance, (int)Types.Foraging, targets, false);
        }
        static void ForageRemove(List<TargetArgs> targets)
        {
            PacketEntityDesignation.Send(Client.Instance, (int)Types.Foraging, targets, true);
        }

        internal override void UpdateQuickButtons()
        {
            if (this.Town.Net is Server)
                return;
            var entities = UISelectedInfo.GetSelectedEntities();
                //UISelectedInfo.GetSelected()
                //.Where(tar => tar.Type == TargetType.Entity).Select(t => t.Object);
            UpdateQuickButtonsChopping(entities);
            UpdateQuickButtonsForaging(entities);
        }
        private void UpdateQuickButtonsChopping(IEnumerable<GameObject> entities)
        {
            var areTask = entities.Where(e => this.ChoppingTasks.Contains(e.RefID));
            //var areNotTask = entities.Except(areTask).OfType<Tree>();//.Where(IsChoppable);
            var areNotTask = entities.Except(areTask).OfType<Plant>().Where(IsChoppable);

            if (areTask.Any())
                UISelectedInfo.AddButton(ButtonChopRemove, ChopDownRemove, areTask);
            else
                UISelectedInfo.RemoveButton(ButtonChopRemove);

            if (areNotTask.Any())
                UISelectedInfo.AddButton(ButtonChopAdd, ChopDownAdd, areNotTask);
            else
                UISelectedInfo.RemoveButton(ButtonChopAdd);
        }
        private void UpdateQuickButtonsForaging(IEnumerable<GameObject> entities)
        {
            var areTask = entities.Where(e => this.QueuedForaging.Contains(e.RefID));
            var areNotTask = entities.Except(areTask).OfType<Plant>().Where(o=>o.IsHarvestable);
            if (areTask.Any())
                UISelectedInfo.AddButton(ButtonForageRemove, ForageRemove, areTask);
            else
                UISelectedInfo.RemoveButton(ButtonForageRemove);

            if (areNotTask.Any())
                UISelectedInfo.AddButton(ButtonForageAdd, ForageAdd, areNotTask);
            else
                UISelectedInfo.RemoveButton(ButtonForageAdd);
        }

        private static bool IsChoppable(GameObject o)
        {
            return o.HasComponent<PlantComponent>();
            return o.HasComponent<TreeComponent>();
        }
        
    }
}
