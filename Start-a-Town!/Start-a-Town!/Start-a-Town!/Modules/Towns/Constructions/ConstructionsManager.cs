using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.GameModes;
using Start_a_Town_.UI;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Towns
{
    public class ConstructionsManager : TownComponent
    {
        public override string Name
        {
            get
            {
                return "Constructions";
            }
        }

        const float UpdateFrequency = 1; // per second
        float UpdateTimerMax = (float)Engine.TargetFps / UpdateFrequency;
        float UpdateTimer;
        public ConstructionsManager(Town town)
        {
            this.Town = town;
        }
        Dictionary<Vector3, AIJob> QueuedConstructions = new Dictionary<Vector3, AIJob>();
        Queue<Vector3> NewConstructions = new Queue<Vector3>();

        public void HandleBlock(IMap map, Vector3 global)
        {
            if (map.GetNetwork() is Net.Client)
                return;

            var block = map.GetBlock(global);
            //if (block.Type != Block.Types.Construction)
            //    this.CurrentConstructions.Remove(global);
            //else
            //    this.CurrentConstructions.Add(global);
            //if (block.Type != Block.Types.Construction)
            if (block.Type == Block.Types.Construction)
            {
                if (!this.NewConstructions.Contains(global))
                    if (!this.QueuedConstructions.ContainsKey(global))
                        this.NewConstructions.Enqueue(global);
            }
            else
            {
                AIJob job;
                if (this.QueuedConstructions.TryGetValue(global, out job))
                {
                    job.Cancel();//.Cancelled = true;
                    this.QueuedConstructions.Remove(global);
                }
            }
        }

        void GenerateWork()
        {
            while(this.NewConstructions.Count>0)
            {
                var global = this.NewConstructions.Dequeue();
                var block = this.Town.Map.GetBlock(global);
                if (block.Type != Block.Types.Construction)
                    continue;


                AIJob job = new AIJob();
                //job.Instructions.Enqueue(new AIInstruction(new TargetArgs(global), new BlockDesignation.InteractionBuild()));
                job.AddStep(new AIInstruction(new TargetArgs(global), new BlockDesignation.InteractionBuild()));
                job.Labor = AILabor.Builder;
                this.Town.AddJob(job);
                this.QueuedConstructions[global] = job;
            }
        }
        public override void Update()
        {
            if (this.Town.Map.GetNetwork() is Net.Client)
                return;
            if (this.UpdateTimer > 0)
            {
                this.UpdateTimer--;
                return;
            }
            this.GenerateWork();
        }
        public override GroupBox GetInterface()
        {
            GroupBox box = new GroupBox();
            box.Controls.Add(new Label("afasf"));
            return box;
        }

        public override List<SaveTag> Save()
        {
            var tag = new List<SaveTag>();
            tag.Add(this.QueuedConstructions.Keys.ToList().Save("Positions"));
            return tag;
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<List<SaveTag>>("Positions", v => this.NewConstructions = new Queue<Vector3>(new List<Vector3>().Load(v)));
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.QueuedConstructions.Keys.ToList());
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.NewConstructions = new Queue<Vector3>(r.ReadListVector3());
        }
    }
}
