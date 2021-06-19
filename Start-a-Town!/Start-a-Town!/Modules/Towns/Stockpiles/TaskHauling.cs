using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.Towns
{
    class TaskHauling : AITask
    {
        public int ObjectType;
        public TaskHauling()
        {
            this.BehaviorType = typeof(TaskBehaviorHaulToStockpileNew);
        }
        internal override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.ObjectType);
        }
        protected override void Read(System.IO.BinaryReader r)
        {
            this.ObjectType = r.ReadInt32();
        }
        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.ObjectType.Save("ObjectType"));
        }
        public override void LoadData(SaveTag tag)
        {
            tag.TryGetTagValue<int>("ObjectType", out this.ObjectType);
        }
    }
}
