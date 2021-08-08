using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class NpcSkillsComponent : EntityComponent
    {
        public override string Name { get; } = "Npc Skills";
        static public Panel UI = new Panel(new Rectangle(0,0,500, 400));
        public override object Clone()
        {
            return new NpcSkillsComponent(this.SkillsNew.ToArray());
        }
        public NpcSkillsComponent(ItemDef def)
        {
            var defs = def.ActorProperties.Skills;
            this.SkillsNew = new NpcSkill[defs.Length];
            for (int i = 0; i < defs.Length; i++)
            {
                this.SkillsNew[i] = new NpcSkill(defs[i]);
            }
        }
        public NpcSkillsComponent()
        {

        }
        public NpcSkillsComponent(params NpcSkill[] skills)
        {
            var count = skills.Length;
            this.SkillsNew = new NpcSkill[count];
            for (int i = 0; i < count; i++)
            {
                this.SkillsNew[i] = skills[i].Clone();
            }
        }
        NpcSkill[] SkillsNew;
        public NpcSkillsComponent(params SkillDef[] defs)
        {
            this.SkillsNew = new NpcSkill[defs.Length];
            for (int i = 0; i < defs.Length; i++)
            {
                this.SkillsNew[i] = new NpcSkill(defs[i]);
            }
        }
       
        public static void GetUI(GameObject actor, Control container)
        {
            var skills = actor.GetComponent<NpcSkillsComponent>().SkillsNew;
            foreach (var skill in skills)
                container.AddControlsBottomLeft(skill.GetControl());
        }
        static readonly TableScrollableCompactNewNew<NpcSkill> GUI = new TableScrollableCompactNewNew<NpcSkill>()
            .AddColumn("name", "", 96, s => new Label(s.Def.Label) {
                TooltipFunc = (t) =>
                {
                    t.AddControlsBottomLeft(
                        new Label(s.Def.Description),
                        new Label() { TextFunc = () => $"Current Level: {s.Level}" },
                        new Label() { TextFunc = () => $"Experience: {s.CurrentXP} / {s.XpToLevel}" });
                }
            })
            .AddColumn("level", "", 16, s => new Label(()=>$"{s.Level}"));

        static public Control GetGUI(Actor actor)
        {
            var skills = actor.Skills.SkillsNew;
            GUI.ClearItems();
            GUI.AddItems(skills);
            return GUI;
        }
        
        public Control GetUI()
        {
            var table = new TableScrollableCompactNewNew<NpcSkill>()
                .AddColumn(null, "name", 80, s => new Label(s.Def.Label), 0)
                .AddColumn(null, "value", 16, s => new Label() { TextFunc = () => s.Level.ToString() }, 0);

            table.AddItems(this.SkillsNew);
            return table;

            var container = new GroupBox();
            foreach (var skill in this.SkillsNew)
                container.AddControlsBottomLeft(skill.GetControl());
            return container;
        }
        public static Control GetGui(GameObject actor)
        {
            var a = actor as Actor;
            var win = GUI.GetWindow();
            if(win is null)
                win = GUI.ToWindow("Skills");
            GUI.ClearItems();
            GUI.AddItems(a.Skills.SkillsNew);
            win.Validate(true);
            return win;
        }

        internal void AwardSkillXP(GameObject actor, SkillDef skill, float v)
        {
            var currentSkill = this.SkillsNew.First(s => s.Def == skill);
            currentSkill.CurrentXP += (int)v;
            if (currentSkill.CurrentXP >= currentSkill.XpToLevel)
            {
                currentSkill.Level++;
                actor.Net.EventOccured(Message.Types.SkillIncrease, actor, currentSkill.Def.Name);
            }
        }

        internal NpcSkill GetSkill(SkillDef skill)
        {
            return this.SkillsNew.First(s => s.Def == skill);
        }

        public NpcSkillsComponent Randomize()
        {
            var range = 10;
            var average = range / 2;
            
            var values = RandomHelper.NextNormalsBalanced(this.SkillsNew.Length);
            for (int i = 0; i < this.SkillsNew.Length; i++)
            {
                var skill = this.SkillsNew[i];
                skill.Level = (int)(average * (1 + values[i]));
            }
            return this;
        }

        internal override void AddSaveData(SaveTag tag)
        {
            this.SkillsNew.SaveImmutable(tag, "Skills");
        }
        internal override void Load(SaveTag tag)
        {
            this.SkillsNew.TryLoadImmutable(tag, "Skills");
        }
        public override void Write(BinaryWriter w)
        {
            this.SkillsNew.Write(w);
        }
        public override void Read(BinaryReader r)
        {
            this.SkillsNew.Read(r);
        }
        public class Props : ComponentProps
        {
            public override Type CompType => typeof(NpcSkillsComponent);
            public SkillDef[] Items;
            public Props(params SkillDef[] defs)
            {
                this.Items = defs;
            }
        }
    }
}
