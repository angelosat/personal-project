﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        }
        public static void Start(GameObject parent, Interaction task, TargetArgs target)
        {
            var comp = parent.GetComponent<WorkComponent>();
            comp.Perform(parent, task, target);
        }

        public static void End(GameObject parent, bool success = false)
        {
            var comp = parent.GetComponent<WorkComponent>();
            comp.Interrupt(parent, success);
        }
        internal void UseTool(GameObject parent, TargetArgs target)
        {
            var tool = parent.GetComponent<HaulComponent>().Holding.Object;//.EquipmentSlots[GearType.Mainhand].Object;

            if (tool == null)
            {
                UseHands(parent, target);
                return;
            }
            ToolAbilityDef skill = null;
            if (!tool.TryGetComponent<ToolAbilityComponent>(c => skill = c.Skill))
                return;
            if (skill == null)
                return;
            var work = skill.GetInteraction(parent, target);
            if (work == null)
                return;
            this.Perform(parent, work, target);
        }

        private void UseHands(GameObject parent, TargetArgs target)
        {
        }

        public override void Tick()
        {
            var parent = this.Parent;
            if (this.Task == null)
                return;

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
            this.Task.FinishAction(parent, this.Target);
            this.Task = null;
            this.Target = null;
        }

        public override void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        {
            if (this.Task == null)
                return;
            this.Task.DrawUI(sb, camera, parent, this.Target);
        }

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
            var inter = Interaction.Load(interactionTag);
            this.Task = inter;
            if (inter.Animation != null)
            {
                inter.Animation.Load(save["Animation"]);
            }
        }
        
        public override void OnObjectLoaded(GameObject parent)
        {
            if (this.Task != null)
            {
                if (this.Task.Animation != null)
                {
                    parent.AddAnimation(this.Task.Animation);
                    this.Task.AfterLoad(parent, this.Target);
                }
            }
        }
        
        internal override void MapLoaded(GameObject parent)
        {
            if (this.Target != null)
                this.Target.Map = parent.Map;
        }
    }
}
