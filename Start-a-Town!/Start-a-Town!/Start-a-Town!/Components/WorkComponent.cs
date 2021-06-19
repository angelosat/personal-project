using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Particles;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class WorkComponent : Component
    {
        public override string ComponentName
        {
            get { return "Work"; }
        }
        public override object Clone()
        {
            return new WorkComponent();
        }

        public Interaction Task { get; set; }
        public TargetArgs Target { get; set; }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch(e.Type)
            {
                case Message.Types.Walk:
                    this.Interrupt(parent);
                    break;

                default:
                    break;
            }
            return false;
        }

        public void Interrupt(GameObject parent)
        {
            if (this.Task != null)
                this.Task.Interrupt(parent);
            //    parent.Net.EventOccured(Message.Types.Interrupt, parent);
            //this.Task = null;
            //this.Target = null;
        }

        public void Perform(GameObject parent, Interaction task, TargetArgs target)
        {
            if (task.IsNull())
                return;
            this.Interrupt(parent);
            this.Task = task; 
            this.Target = target;
        }
        public static void Start(GameObject parent, Interaction task, TargetArgs target)
        {
            var comp = parent.GetComponent<WorkComponent>();
            if (task.IsNull())
                return;
            comp.Interrupt(parent);
            comp.Task = task;
            comp.Target = target;
        }
        public static void Stop(GameObject parent)
        {
            var comp = parent.GetComponent<WorkComponent>();
            comp.Interrupt(parent);
        }
        internal void UseTool(GameObject parent, TargetArgs target)
        {
            //var tool = parent.GetComponent<GearComponent>().Holding.Object;//.EquipmentSlots[GearType.Mainhand].Object;
            var tool = parent.GetComponent<HaulComponent>().Holding.Object;//.EquipmentSlots[GearType.Mainhand].Object;

            if (tool.IsNull())
            {
                UseHands(parent, target);
                return;
            }
            //if (tool.GetComponent<EquipComponent>().Durability.Value == 0)
            //    return;
            Skill skill = null;
            if (!tool.TryGetComponent<SkillComponent>(c => skill = c.Skill))
                return;
            if (skill.IsNull())
                return;
            //if (skill.Work.IsNull())
            //    return;
            //this.Task = skill.Work.Clone() as Work;
            var work = skill.GetWork(parent, target);
            if (work == null)
                return;
            this.Perform(parent, work, target);
            //this.Task = work;
            //this.Target = target;
        }

        private void UseHands(GameObject parent, TargetArgs target)
        {
            //throw new NotImplementedException();
        }

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            this.UpdateParticles(parent);
            //this.Task.Update(net, parent);
            if (this.Task.IsNull())
                return;
            //if(this.Task.IsCancelled(parent, this.Target))
            //{
            //    this.Task = null;
            //    this.Target = null;
            //    return;
            //}

            //var dir = this.Target.Type == TargetType.Direction ? new Vector3(this.Target.Direction, 0) : (this.Target.Global - parent.Global);
            //dir.Normalize();
            //parent.Direction = dir;

            this.Task.Update(parent, this.Target);
            //if (this.Task.State != Interaction.States.Finished)
            //    return;

            //switch(this.Task.State)
            //{
            //    case Interaction.States.Failed:

            //        break;

            //    case Interaction.States.Running:
            //        return;

            //    default:
            //        break;
            //}

            if (this.Task.State == Interaction.States.Running)
            {
                var dir = this.Target.Type == TargetType.Direction ? new Vector3(this.Target.Direction, 0) : (this.Target.Global - parent.Global);
                dir.Normalize();
                parent.Direction = dir;
                // WARNING: i had to move this here because if the interaction target was this entity itself, then the direction vector became zero and its normal became NaN
                return;
            }
            this.Task = null;
            this.Target = null;
        }

        public override void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        {
            if (this.Task == null)
                return;
            this.Task.DrawUI(sb, camera, parent);
        }

        List<ParticleEmitter> Emitters = new List<ParticleEmitter>();
        static public List<ParticleEmitter> GetEmitters(GameObject parent)
        {
            if (parent.Net is Net.Server)
                return null;
            WorkComponent comp = parent.GetComponent<WorkComponent>();
            //if (comp == null)
            //    return new List<ParticleEmitter>();
            return comp.Emitters;
        }
        public override void Draw(MySpriteBatch sb, GameObject parent, Camera camera)
        {
            foreach (var e in this.Emitters)
                e.Draw(camera, parent.Map, e.Source);
        }
        private void UpdateParticles(GameObject parent)
        {
            foreach (var e in this.Emitters.ToList())
            {
                e.Update(parent.Map, e.Source);
                if (e.Particles.Count == 0)
                    this.Emitters.Remove(e);
            }
        }
    }
}
