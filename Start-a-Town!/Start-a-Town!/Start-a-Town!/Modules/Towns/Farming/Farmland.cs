using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Towns.Farming
{
    public class Farmland// : Zone
    {
        //public Vector3 Begin, End;
        //public int Width, Height;
        public int ID;
        public Town Town;
        public HashSet<Vector3> Positions = new HashSet<Vector3>();
        HashSet<GameObject> QueuedPlants = new HashSet<GameObject>();
        bool _Harvesting = true;
        public bool Harvesting
        {
            get { return _Harvesting; }
            private set { _Harvesting = value; }
        }
        public bool Planting = true;
        GameObject _SeedType;
        public GameObject SeedType
        {
            get { return _SeedType; }
            set
            {
                if (value != null)
                    if (!value.HasComponent<SeedComponent>())
                        throw new Exception();

                var old = _SeedType;
                _SeedType = value;

                if (old != value)
                    this.Town.Map.EventOccured(Message.Types.FarmSeedChanged, this, value);
            }
        }
        //public GameObjectSlot SeedType = new GameObjectSlot();

        Dictionary<Vector3, AIJob> QueuedTilling = new Dictionary<Vector3, AIJob>();//HashSet<Vector3>();
        Dictionary<GameObject, AIJob> QueuedHarvesting = new Dictionary<GameObject, AIJob>();
        Dictionary<Vector3, AIJob> PendingPlantJobs = new Dictionary<Vector3, AIJob>();//HashSet<Vector3>();

        public Farmland(int id, Vector3 g, int w, int h)
        {
            this.ID = id;
            //this.Begin = g;
            //this.Width = w;
            //this.Height = h;
            //this.End = new Vector3(g.X + w - 1, g.Y + h - 1, g.Z);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    var pos = new Vector3(i, j, 0) + g;
                    this.Positions.Add(pos);
                }
            }
        }

        internal void SetSeed(GameObject.Types type)
        {
            var obj = GameObject.Objects[type];
            var seedComp = obj.GetComponent<SeedComponent>();
            if (seedComp == null)
                return;
            this.SeedType = GameObject.Objects[type];// type;
            //this.Town.Map.EventOccured(Message.Types.FarmSeedChanged, this, obj);
        }
        internal void SetSeed(int id)
        {
            if(id==-1)
            {
                this.SeedType = null;
            }
            var type = (GameObject.Types)id;
            var obj = GameObject.Objects[type];
            var seedComp = obj.GetComponent<SeedComponent>();
            if (seedComp == null)
                throw new Exception();
            this.SeedType = GameObject.Objects[type];// type;
            //this.Town.Map.EventOccured(Message.Types.FarmSeedChanged, this, obj);
        }
        internal void GenerateWork()
        {
            if (this.Town.Map.Net is Net.Client)
                return;
            GenerateHarvestJobs();
            //for (int i = 0; i < this.Width; i++)
            //{
            //    for (int j = 0; j < this.Height; j++)
            //    {
            foreach (var pos in this.Positions)
            {
                //var current = this.Begin + new Vector3(i, j, -1);
                var current = pos - Vector3.UnitZ;

                var block = this.Town.Map.GetBlock(current);
                switch (block.Type)
                {
                    case Block.Types.Farmland:
                        this.QueuedTilling.Remove(current);

                        this.GeneratePlantJob(block, current);

                        break;

                    default:
                        //if (block.GetMaterial(this.Town.Map, current) == Material.Soil)
                        if(IsPositionValid(this, current))
                        {
                            if (this.QueuedTilling.ContainsKey(current))
                                break;
                            var job = new AIJob();
                            //job.Instructions.Enqueue(new AIInstruction(new TargetArgs(current), new Tilling()));
                            job.AddStep(new AIInstruction(new TargetArgs(current), new Tilling()));
                            this.Town.AddJob(job);
                            job.Labor = AILabor.Farmer;
                            this.QueuedTilling.Add(current, job);
                            break;
                        }
                        else
                            if (this.QueuedTilling.ContainsKey(current))
                                this.QueuedTilling.Remove(current);
                        break;
                }
            }
            //    }
            //}
        }
        static public bool IsPositionValid(Farmland farmland, Vector3 arg)
        {
            return
                Block.GetBlockMaterial(farmland.Town.Map, arg) == Components.Materials.Material.Soil
                && farmland.Town.Map.GetBlock(arg + Vector3.UnitZ) == Block.Air;
        }
        private void GenerateHarvestJobs()
        {
            //var plants = from entity in this.Town.Map.GetObjects(new BoundingBox(this.Begin, this.End))
            //             //where !this.QueuedHarvesting.Contains(entity)
            //             let plantComp = entity.GetComponent<PlantComponent>()
            //             where plantComp != null
            //             select entity;
            if (!this.Harvesting)
                return;
            foreach(var plant in this.QueuedPlants.ToList())// plants)
            {
                var plantComp = plant.GetComponent<PlantComponent>();
                //if (plantComp.CurrentGrowthState != PlantComponent.GrowthStates.Ready)
                if (plantComp.CurrentState != plantComp.Grown)
                {
                    this.QueuedHarvesting.Remove(plant);
                    continue;
                }
                if(this.QueuedHarvesting.ContainsKey(plant))
                    continue;
                var job = new AIJob();
                //job.Instructions.Enqueue(new AIInstruction(new TargetArgs(plant), new PlantComponent.InteractionHarvest(plant, plantComp)));
                job.AddStep(new AIInstruction(new TargetArgs(plant), new PlantComponent.InteractionHarvest(plant, plantComp)));
                this.Town.AddJob(job);
                //this.QueuedPlants.Remove(plant);
                job.Labor = AILabor.Harvester;

                this.QueuedHarvesting.Add(plant, job);
            }
        }

        private void GeneratePlantJob(Block block, Vector3 current)
        {
            //if (this.PendingPlantJobs.ContainsKey(current))
            //    return;

            if (this.SeedType == null)
                return;
            if (block.Type != Block.Types.Farmland)
                return;
            var entity = this.Town.Map.GetBlockEntity(current) as BlockFarmland.Entity;
            if (entity != null) // null block entity means that the farmland is unplanted, so, generate job if an existing one isn't pending
            {
                if (this.PendingPlantJobs.ContainsKey(current))
                    return;
                AIJob job = new AIJob();
                //job.Instructions.Enqueue(new AIInstruction(new TargetArgs(current), new InteractionPlantSeed()));//this.SeedType.ID)));
                job.AddStep(new AIInstruction(new TargetArgs(current), new InteractionPlantSeed()));//this.SeedType.ID)));
                job.Labor = AILabor.Farmer;

                this.Town.AddJob(job);
                this.PendingPlantJobs.Add(current, job);
            }
            else // non null means that the farm is planted, so remove it from pending plantings
            {
                this.PendingPlantJobs.Remove(current);
            }
        }

        internal GroupBox GetInterface()
        {
            //var box = new GroupBox();

            //var seedObjects = //GameObject.Objects.Values.Where(foo => foo.GetComponent<SeedComponent>() != null);
            //    from obj in GameObject.Objects.Values
            //    where obj.GetComponent<SeedComponent>() != null
            //    select new GameObjectSlot(obj);
            //var slotGrid = new SlotGrid(seedObjects.ToList(), 4, s =>
            //{
            //    s.LeftClickAction = () => PacketFarmSetSeed.Send(Player.Actor.InstanceID, this.ID, (int)s.Tag.Object.ID);
            //});
            //box.Controls.Add(slotGrid);
            //return box;

            return new FarmlandUI(this);
        }
        internal Window GetWindow()
        {
            var win = FarmlandUI.GetWindow(this);// this.GetInterface().ToWindow(this.Name);

            return win;
        }
        internal void AddPlant(GameObject plant)
        {
            this.QueuedPlants.Add(plant);
        }

        internal void RemovePositions(Vector3 global, int w, int h)
        {
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    var pos = new Vector3(i, j, 0) + global;
                    this.Positions.Remove(pos);
                }
            }
        }
        internal void Edit(List<Vector3> positions, bool remove)
        {
            if (!remove)
            {
                foreach (var pos in positions)
                    if (IsPositionValid(this, pos - Vector3.UnitZ))
                        this.Positions.Add(pos);
            }
            else
            {
                // check if result positions are connected
                var checkPositions = this.Positions.ToList();
                foreach (var pos in positions)
                    checkPositions.Remove(pos);
                if (checkPositions.Count == 0)
                {
                    this.Positions.Clear(); // i did this because the tool was still showing this farm's tiles
                    this.Town.FarmingManager.RemoveFarm(this);
                    return;
                }
                var isConnected = checkPositions.IsConnected();
                if (isConnected)
                {
                    foreach (var pos in positions)
                        RemovePosition(pos);
                    if (this.Positions.Count == 0)
                        this.Town.FarmingManager.RemoveFarm(this);
                }
                else
                    if (this.Town.Map.GetNetwork() is Net.Client)
                        Net.Client.Console.Write("Resulting zone must be connected");
            }
        }

        private void RemovePosition(Vector3 pos)
        {
            AIJob job;
            if (this.QueuedTilling.TryGetValue(pos, out job))
                job.Cancel();
            this.Positions.Remove(pos);
        }

        public void SetHarvesting(bool value)
        {
            this.Harvesting = value;
            if (value)
                return;

            foreach(var harvest in this.QueuedHarvesting.ToDictionary(f=>f.Key, f=>f.Value))
            {
                this.Town.RemoveJob(harvest.Value);
                this.QueuedHarvesting.Remove(harvest.Key);
            }
        }

        string CurrentName = "";
        public string Name
        {
            get { return string.IsNullOrEmpty(this.CurrentName) ? "Farm " + this.ID.ToString() : this.CurrentName; }
            set { this.CurrentName = value; }
        }
        public int GetSeedID()
        {
            return this.SeedType != null ? (int)this.SeedType.ID : -1;
        }
        public Farmland(Town town, SaveTag tag)
        {
            this.Town = town;
            this.Load(tag);
        }
        public Farmland(Town town, BinaryReader r)
        {
            this.Town = town;
            this.Read(r);
        }

        public List<SaveTag> Save()
        {
            //var tag = new SaveTag(SaveTag.Types.Compound, "Farm");
            List<SaveTag> tag = new List<SaveTag>();
            tag.Add(new SaveTag(SaveTag.Types.Int, "ID", this.ID));
            tag.Add(this.CurrentName.Save("Name"));
            //tag.Add(new SaveTag(SaveTag.Types.Int, "Size", this.Positions.Count));
            var list = new SaveTag(SaveTag.Types.List, "Positions", SaveTag.Types.Vector3);
            foreach (var pos in this.Positions)
                list.Add(new SaveTag(SaveTag.Types.Vector3, "", pos));
            tag.Add(list);
            tag.Add(new SaveTag(SaveTag.Types.Int, "Seed", GetSeedID()));
            tag.Add(this.Harvesting.Save("Harvesting"));
            tag.Add(this.Planting.Save("Planting"));
            return tag;
        }

        
        public void Load(SaveTag tag)
        {
            tag.TryGetTagValue<int>("ID", out this.ID);// v => this.ID = value);
            tag.TryGetTagValue<string>("Name", out this.CurrentName);
            List<SaveTag> positions;
            if(tag.TryGetTagValue("Positions", out positions))
                foreach (var pos in positions)
                    this.Positions.Add((Vector3)pos.Value);
            tag.TryGetTagValue<int>("Seed", v => this.SetSeed(v));// this.SeedType = v == -1 ? null : GameObject.Objects[v]);
            tag.TryGetTagValue<bool>("Harvesting", v => this.Harvesting = v);
            tag.TryGetTagValue<bool>("Planting", out this.Planting);
        }
       
        public void Write(BinaryWriter w)
        {
            w.Write(this.ID);
            w.Write(this.CurrentName);
            w.Write(this.Positions.ToList());
            w.Write(this.GetSeedID());
            w.Write(this.Harvesting);
            w.Write(this.Planting);
        }
        public void Read(BinaryReader r)
        {
            this.ID = r.ReadInt32();
            this.CurrentName = r.ReadString();
            this.Positions = new HashSet<Vector3>(r.ReadListVector3());
            this.SetSeed(r.ReadInt32());
            this.Harvesting = r.ReadBoolean();
            this.Planting = r.ReadBoolean();
        }
    }
}
