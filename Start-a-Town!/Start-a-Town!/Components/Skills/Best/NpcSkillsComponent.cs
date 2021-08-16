using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class NpcSkillsComponent : EntityComponent
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
            this.SkillsNew = new Skill[defs.Length];
            for (int i = 0; i < defs.Length; i++)
            {
                this.SkillsNew[i] = new Skill(defs[i]) { Container = this };
            }
        }
        public NpcSkillsComponent()
        {

        }
        public NpcSkillsComponent(params Skill[] skills)
        {
            var count = skills.Length;
            this.SkillsNew = new Skill[count];
            for (int i = 0; i < count; i++)
            {
                var newSkill = skills[i].Clone();
                newSkill.Container = this;
                this.SkillsNew[i] = newSkill;
            }
        }
        public readonly Skill[] SkillsNew;
        public NpcSkillsComponent(params SkillDef[] defs)
        {
            this.SkillsNew = new Skill[defs.Length];
            for (int i = 0; i < defs.Length; i++)
            {
                this.SkillsNew[i] = new Skill(defs[i]);
            }
        }
       
        public static void GetUI(GameObject actor, Control container)
        {
            var skills = actor.GetComponent<NpcSkillsComponent>().SkillsNew;
            foreach (var skill in skills)
                container.AddControlsBottomLeft(skill.GetListControlGui());
        }
        static readonly TableScrollableCompactNewNew<Skill> GuiTable = new TableScrollableCompactNewNew<Skill>()
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
        static readonly ListBoxNoScroll GuiList = new();

        static public Control GetGUI(Actor actor)
        {
            var skills = actor.Skills.SkillsNew;
            GuiTable.ClearItems();
            GuiTable.AddItems(skills);
            return GuiTable;
        }
        
        public Control GetUI()
        {
            var table = new TableScrollableCompactNewNew<Skill>()
                .AddColumn(null, "name", 80, s => new Label(s.Def.Label), 0)
                .AddColumn(null, "value", 16, s => new Label() { TextFunc = () => s.Level.ToString() }, 0);

            table.AddItems(this.SkillsNew);
            return table;

            var container = new GroupBox();
            foreach (var skill in this.SkillsNew)
                container.AddControlsBottomLeft(skill.GetListControlGui());
            return container;
        }
        public static Control GetGui(GameObject actor)
        {
            var a = actor as Actor;
            //var win = GuiTable.GetWindow();
            //if(win is null)
            //    win = GuiTable.ToWindow("Skills");
            //GuiTable.ClearItems();
            //GuiTable.AddItems(a.Skills.SkillsNew); 
            var win = GuiList.GetWindow();
            if (win is null)
                win = GuiList.ToWindow("Skills");
            GuiList.Clear();
            GuiList.AddItems(a.Skills.SkillsNew);
            win.Validate(true);
            return win;
        }

      
        internal Skill GetSkill(SkillDef skill)
        {
            return this.SkillsNew.First(s => s.Def == skill);
        }
        [InspectorHidden]
        internal Skill this[SkillDef skill] => this.GetSkill(skill);
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
