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
    public class WorkComponent : EntityComponent
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
                case Message.Types.Jumped:
                case Message.Types.Walk:
                    this.Interrupt(parent);
                    break;

                default:
                    break;
            }
            return false;
        }

        public void Interrupt(GameObject parent, bool success = false)
        {
            if (this.Task == null)
                return;
            this.Task.Interrupt(parent, success);
            this.Task.FinishAction(parent, this.Target);
            this.Task = null;
            this.Target = null;
            //if (this.Task.Animation != null)
            //    parent.GetComponent<SpriteComponent>().DefaultBody.FadeOutAnimationAndRemove(this.Task.Animation);                
        }


        internal void OnToolContact(GameObject parent)
        {
            this.Task.OnToolContact(parent, this.Target);
        }

        public void Perform(GameObject parent, Interaction task, TargetArgs target)
        {
            if (task == null)
                throw new ArgumentException();
            this.Interrupt(parent);
            this.Task = task;
            this.Target = target;
            this.Task.InitAction(parent, target);
            if (this.Task.HasFinished)
                this.Task = null;
            //if (task.Animation != null)
            //    parent.Body.AddAnimation(task.Animation);
        }
        public static void Start(GameObject parent, Interaction task, TargetArgs target)
        {
            var comp = parent.GetComponent<WorkComponent>();
            comp.Perform(parent, task, target);
            //if (task == null)
            //    return;
            //comp.Interrupt(parent);
            //comp.Task = task;
            //comp.Target = target;
        }

        //public static void Interrupt(GameObject parent, bool success = false)
        //{
        //    var comp = parent.GetComponent<WorkComponent>();
        //    comp.Interrupt(parent, success);
        //}
        public static void End(GameObject parent, bool success = false)
        {
            var comp = parent.GetComponent<WorkComponent>();
            comp.Interrupt(parent, success);
        }
        internal void UseTool(GameObject parent, TargetArgs target)
        {
            //var tool = parent.GetComponent<GearComponent>().Holding.Object;//.EquipmentSlots[GearType.Mainhand].Object;
            var tool = parent.GetComponent<HaulComponent>().Holding.Object;//.EquipmentSlots[GearType.Mainhand].Object;

            if (tool == null)
            {
                UseHands(parent, target);
                return;
            }
            //if (tool.GetComponent<EquipComponent>().Durability.Value == 0)
            //    return;
            ToolAbilityDef skill = null;
            if (!tool.TryGetComponent<ToolAbilityComponent>(c => skill = c.Skill))
                return;
            if (skill == null)
                return;
            //if (skill.Work.IsNull())
            //    return;
            //this.Task = skill.Work.Clone() as Work;
            var work = skill.GetInteraction(parent, target);
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

        public override void Tick(IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            if (this.Task == null)
                return;

            // testing to move this in the ai behaviorinteract
            this.Task.Update(parent, this.Target);

            if (this.Task.State == Interaction.States.Running)
            {
                if (this.Target.Global != parent.Global)
                {
                    var dir = this.Target.Type == TargetType.Direction ? new Vector3(this.Target.Direction, 0) : (this.Target.Global - parent.Global);
                    dir.Normalize();
                    parent.Direction = dir;
                }
                // WARNING: i had to move this here because if the interaction target was this entity itself, then the direction vector became zero and its normal became NaN
                return;
            }
            //if (this.Task.Animation != null)
            //    parent.GetComponent<SpriteComponent>().DefaultBody.FadeOutAnimationAndRemove(this.Task.Animation);                
                //parent.Body.FadeOutAnimationAndRemove(this.Task.Animation);
            this.Task.FinishAction(parent, this.Target);
            this.Task = null;
            this.Target = null;
        }

        public override void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        {
            if (this.Task == null)
                return;
            //this.Task.DrawUI(sb, camera, parent);
            this.Task.DrawUI(sb, camera, parent, this.Target);
        }

        //internal override SaveTag SaveAs(string name = "")
        //{
        //    var tag = new SaveTag(SaveTag.Types.Compound, name);
        //    var isInteracting = (this.Task != null);
        //    tag.Add(isInteracting.Save("IsInteracting"));
        //    if (!isInteracting)
        //        return tag;
        //    tag.Add(this.Target.Save("Target"));
        //    tag.Add(this.Task.SaveAs("Interaction"));
        //    return tag;
        //}
        public override void Write(System.IO.BinaryWriter w)
        {
            var isInteracting = (this.Task != null);
            w.Write(isInteracting);
            if (!isInteracting)
                return;
            this.Target.Write(w);
            w.Write(this.Task.GetType().FullName);
            this.Task.Write(w);
            if (this.Task.Animation != null)
                this.Task.Animation.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            var isinteracting = r.ReadBoolean();
            if (!isinteracting)
                return;
            this.Target = TargetArgs.Read((IObjectProvider)null, r);
            //this.Task = Interaction
            var interactionType = r.ReadString();
            var interaction = Interaction.Create(interactionType);
            interaction.Read(r);
            this.Task = interaction;
            if (this.Task.Animation != null)
                this.Task.Animation.Read(r);
        }
        internal override void AddSaveData(SaveTag tag)
        {
            var isInteracting = (this.Task != null);
            tag.Add(isInteracting.Save("IsInteracting"));
            if (!isInteracting)
                return;
            tag.Add(this.Target.Save("Target"));
            tag.Add(this.Task.SaveAs("Interaction"));
            if (this.Task.Animation != null)
                tag.Add(this.Task.Animation.Save("Animation"));
        }
        internal override void Load(SaveTag save)
        {
            bool isInteracting;
            if (!save.TryGetTagValue<bool>("IsInteracting", out isInteracting))
                return;
            if (!isInteracting)
                return;
            this.Target = new TargetArgs(null, save["Target"]);
            var interactionTag = save["Interaction"];
            //var interName = interactionTag.Name;
            ////var t = Type.GetType(itnerName);
            //var inter = Interaction.Load(interName, interactionTag);
            var inter = Interaction.Load(interactionTag);
            this.Task = inter;
            if (inter.Animation != null)
            {
                inter.Animation.Load(save["Animation"]);
            }
        }
        //public override void Spawn(IObjectProvider net, GameObject parent)
        //{
        //    if (this.Target != null)
        //        this.Target.Network = net;
        //}
        public override void OnObjectLoaded(GameObject parent)
        {
            if (this.Task != null)
            {
                if (this.Task.Animation != null)
                {
                    //parent.Body.AddAnimation(this.Task.Animation);
                    parent.AddAnimation(this.Task.Animation);
                    this.Task.AfterLoad(parent, this.Target);
                }
            }
        }
        //public override void ObjectSynced(GameObject parent)
        //{
        //    if (this.Target != null)
        //        this.Target.Network = parent.Net;
        //}
        internal override void MapLoaded(GameObject parent)
        {
            if (this.Target != null)
                //this.Target.Network = parent.Net;
                this.Target.Map = parent.Map;

            //if (parent.Net is Net.Client)
            //    if (this.Task != null)
            //        this.Task.CurrentTick += 3;//WTF???????
        }
        //List<ParticleEmitter> Emitters = new List<ParticleEmitter>();
        //static public List<ParticleEmitter> GetEmitters(GameObject parent)
        //{
        //    if (parent.Net is Server)
        //        return null;
        //    WorkComponent comp = parent.GetComponent<WorkComponent>();
        //    //if (comp == null)
        //    //    return new List<ParticleEmitter>();
        //    return comp.Emitters;
        //}
        //public override void Draw(MySpriteBatch sb, GameObject parent, Camera camera)
        //{
        //    foreach (var e in this.Emitters)
        //        e.Draw(camera, parent.Map, e.Source);
        //}
        //private void UpdateParticles(GameObject parent)
        //{
        //    foreach (var e in this.Emitters.ToList())
        //    {
        //        e.Update(parent.Map, e.Source);
        //        if (e.Particles.Count == 0)
        //            this.Emitters.Remove(e);
        //    }
        //}
    }
}
