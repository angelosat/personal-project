using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class NpcSkillsComponent : EntityComponent
    {
        public override string ComponentName => "Npc Skills";
        static public Panel UI = new Panel(new Rectangle(0,0,500, 400));
        public override object Clone()
        {
            return new NpcSkillsComponent(this.SkillsNew.ToArray());//.Values.ToArray());

            return new NpcSkillsComponent(this.SkillsNew.Select(s => s.Def).ToArray());//.Values.ToArray());
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
        //List<NpcSkill> Skills;
        Dictionary<Type, NpcSkill> Skills;
        NpcSkill[] SkillsNew;
        public NpcSkillsComponent(params SkillDef[] defs)
        {
            this.SkillsNew = new NpcSkill[defs.Length];
            for (int i = 0; i < defs.Length; i++)
            {
                this.SkillsNew[i] = new NpcSkill(defs[i]);
            }
        }
        //public ComponentNpcSkills(params NpcSkill[] skills)
        //{
        //    //this.Skills = new List<NpcSkill>(skills);
        //    this.Skills = skills.ToDictionary(s => s.GetType(), s => Activator.CreateInstance(s.GetType()) as NpcSkill);
        //}
        internal override void GetManagementInterface(GameObject gameObject, Control box)
        {
            foreach(var skill in this.Skills.Values)
            {
                box.AddControlsBottomLeft(new Label(string.Format("{0}: {1}", skill.Def.Name, skill.Level)));
            }
        }

        static Control UIContainer;
        public static void GetUI(GameObject actor, Control container)
        {
            var skills = actor.GetComponent<NpcSkillsComponent>().SkillsNew;
            foreach (var skill in skills)
                container.AddControlsBottomLeft(skill.GetControl());
        }
        static readonly TableScrollableCompactNewNew<NpcSkill> GUI = new TableScrollableCompactNewNew<NpcSkill>(SkillDef.All.Length)
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
        //public static Control GetUI()
        //{
        //    if (UIContainer == null)
        //    {
        //        UIContainer = new GroupBox();
        //        //var window = new Window(UIContainer);
        //        //window.SnapToMouse();
        //    }
        //    return UIContainer;
        //}
        public Control GetUI()
        {
            var table = new TableScrollableCompactNew<NpcSkill>(this.SkillsNew.Length, scrollbarMode: ScrollableBoxNew.ScrollModes.None)//ActorDefOf.NpcProps.Skills.Length)
                .AddColumn(null, "name", 80, s => new Label(s.Def.Name), 0)
                .AddColumn(null, "value", 16, s => new Label() { TextFunc = () => s.Level.ToString() }, 0);

            table.AddItems(this.SkillsNew);
            return table;

            var container = new GroupBox();
            foreach (var skill in this.SkillsNew)
                container.AddControlsBottomLeft(skill.GetControl());// new Label() { TextFunc = () => string.Format("{0}: {1}", skill.Name, skill.Level) });
            return container;
        }
        public static Control GetUI(GameObject actor)
        {
            var a = actor as Actor;
            var win = GUI.GetWindow();
            if(win is null)
                win = GUI.ToWindow("Skills");
            GUI.ClearItems();
            GUI.AddItems(a.Skills.SkillsNew);
            win.Validate(true);
            return win;

            Window window;
            if (UIContainer == null)
            {
                UIContainer = new GroupBox();
                window = new Window(UIContainer);
                window.SnapToMouse();
            }
            else 
                window = UIContainer.GetWindow();
            //if (UIContainer.Tag == actor)
            //{
            //    UIContainer.Tag = null;
            //    window.Hide();
            //    return window;
            //}
            UIContainer.Tag = actor;
            window.Title = string.Format("{0} skills", actor.Name);
            var skills = actor.GetComponent<NpcSkillsComponent>().SkillsNew;
            UIContainer.ClearControls();
            foreach (var skill in skills)
                UIContainer.AddControlsBottomLeft(skill.GetControl());// new Label() { TextFunc = () => string.Format("{0}: {1}", skill.Name, skill.Level) });
            //window.Show();
            window.Validate(true);
            return window;
        }
        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            base.GetSelectionInfo(info, parent);
            //if(UIContainer?.TopLevelControl.IsOpen ?? false)
                GetUI(parent as Actor);
        }

        internal void AwardSkillXP(GameObject actor, SkillDef skill, float v)
        {
            //var currentSkill = this.Skills[skill.GetType()];
            var currentSkill = this.SkillsNew.First(s => s.Def == skill);
            currentSkill.CurrentXP += (int)v;
            if (currentSkill.CurrentXP >= currentSkill.XpToLevel)
            {
                currentSkill.Level++;
                actor.NetNew.EventOccured(Message.Types.SkillIncrease, actor, currentSkill.Def.Name);
                //FloatingText.Manager.Create(actor, string.Format("{0} increased!", currentSkill.Name), ft => ft.Font = UIManager.FontBold);
            }
        }

        internal NpcSkill GetSkill(SkillDef skill)
        {
            return this.SkillsNew.First(s => s.Def == skill);
            //return this.Skills[skill.GetType()];
        }

        public NpcSkillsComponent Randomize()
        {
            var range = 10;
            var average = range / 2;
            //var budget = average * this.SkillsNew.Length;
            //int sum = 0;
            //for (int i = 0; i < this.SkillsNew.Length - 1; i++)
            //{
            //    var skill = this.SkillsNew[i];
            //    //var val = Math.Abs(RandomHelper.NextNormal());
            //    //val.ToConsole();
            //    var val = RandomHelper.NextNormal();
            //    val.ToConsole();
            //    skill.Level = average + (int)(val * average);
            //    //skill.Level.ToConsole();
            //    sum += skill.Level;
            //}
            //this.SkillsNew.Last().Level = budget - sum;

            //var values = RandomHelper.NextNormals(this.SkillsNew.Length, 0, 10, 5);
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
