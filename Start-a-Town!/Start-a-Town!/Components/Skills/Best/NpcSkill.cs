using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class NpcSkill : ISaveable, ISerializable, INamed
    {
        //public virtual string Name { get; }
        //public virtual string Description { get; }
        //public virtual Icon Icon { get; }

        //public int ID;
        public SkillDef Def;
        public string Name { get { return this.Def.Name; } }
        public int Level;
        public int CurrentXP;
        const int XpToLevelBase = 5;
        public int XpToLevel
        {
            get { return (int)Math.Pow(XpToLevelBase, this.Level + 1); }
        }

        public NpcSkill(SkillDef def)
        {
            this.Def = def;
        }
        static public Dictionary<int, NpcSkill> Registry = new Dictionary<int, NpcSkill>();

        static public void Init(Hud hud)
        {
            hud.RegisterEventHandler(Components.Message.Types.SkillIncrease, OnSkillIncrease);
        }

        //public void GetTooltipInfo(Tooltip tooltip)
        //{
        //    tooltip.AddControlsBottomLeft(new Label(this.Description));
        //}
        public Control GetControl()
        {
            var label = new Label()
            {
                TextFunc = () => string.Format("{0}: {1}", this.Def.Name, this.Level),
                //HoverText = this.Description,
                TooltipFunc = (t) =>
                {
                    t.AddControlsBottomLeft(
                        new Label(this.Def.Description),
                        new Label() { TextFunc = () => string.Format("Current Level: {0}", this.Level) },
                        new Label() { TextFunc = () => string.Format("Experience: {0} / {1}", this.CurrentXP, this.XpToLevel) });
                }
            };
            return label;
        }

        //internal static readonly NpcSkill Digging = new NpcSkillDigging();
        //internal static readonly NpcSkill Constructing = new NpcSkillDigging();

        static void OnSkillIncrease(GameEvent a)
        {
            var actor = a.Parameters[0] as GameObject;
            var skill = (string)a.Parameters[1];
            FloatingText.Manager.Create(actor, string.Format("{0} increased!", skill), ft => ft.Font = UIManager.FontBold);
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, this.Def.Name);
            tag.Add(this.Level.Save("Level"));
            tag.Add(this.CurrentXP.Save("CurrentXP"));
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTagValue<int>("Level", out this.Level);
            tag.TryGetTagValue<int>("CurrentXP", out this.CurrentXP);
            return this;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Level);
            w.Write(this.CurrentXP);
        }

        public ISerializable Read(BinaryReader r)
        {
            this.Level = r.ReadInt32();
            this.CurrentXP = r.ReadInt32();
            return this;
        }
        public NpcSkill Clone()
        {
            return new NpcSkill(this.Def) { CurrentXP = this.CurrentXP, Level = this.Level };
        }
    }
}
