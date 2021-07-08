using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.GameEvents;
using Start_a_Town_.Net.Packets;
using Start_a_Town_.UI;
using Start_a_Town_.Towns;

namespace Start_a_Town_
{
    public class ChoppingManager : TownComponent
    {
        public enum Types { Chopping, Foraging }
        static public readonly Icon ChopIcon = new(ItemContent.AxeFull);
        static public readonly Icon ForageIcon = new(ItemContent.BerriesFull);

        public override string Name => "Forestry"; 
        readonly HashSet<int> QueuedForaging = new();
        
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
        public HashSet<int> ChoppingTasks = new();

        public List<Vector3> GetPositions()
        {
            var list = new List<Vector3>();
            foreach (var tree in this.GetTrees())
                if (tree is null) //THE NETWORK RETURNS NULL FOR THE PARTICULAR INSTANCEID if disposed
                    list.Add(tree.Global.Round() - Vector3.UnitZ);
            return list;
        }
        public List<Vector3> GetForagingPositions()
        {
            var list = new List<Vector3>();
            foreach (var plant in this.Town.Map.Net.GetNetworkObjects(this.QueuedForaging.ToArray()))
                list.Add(plant.Global.Round());
            return list;
        }

        public ChoppingManager(Towns.Town town)
        {
            this.Town = town;
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
                }
            }
        }
        
        internal override void OnGameEvent(GameEvent e)
        {
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
                    break;

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

        public override void DrawUI(SpriteBatch sb, IMap map, Camera cam)
        {
            this.DrawIcons(sb, map, cam, ChopIcon, GetTrees());
            this.DrawIcons(sb, map, cam, ForageIcon, GetPlants());
        }
        
        private void DrawIcons(SpriteBatch sb, IMap map, Camera camera, Icon icon, IEnumerable<GameObject> objects)
        {
            foreach (var parent in objects)
                icon.DrawAboveEntity(sb, camera, parent);
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
            UpdateQuickButtonsChopping(entities);
            UpdateQuickButtonsForaging(entities);
        }
        private void UpdateQuickButtonsChopping(IEnumerable<GameObject> entities)
        {
            var areTask = entities.Where(e => this.ChoppingTasks.Contains(e.RefID));
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
        }
    }
}
