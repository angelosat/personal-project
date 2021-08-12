using System;
using System.Collections.Generic;
using System.IO;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class NpcSkill : ISaveable, ISerializable, INamed
    {
        public SkillDef Def;
        public string Name { get { return this.Def.Name; } }
        public int Level;
        public float CurrentXP;
        const int XpToLevelBase = 5;
        public int XpToLevel => (int)Math.Pow(XpToLevelBase, this.Level + 1); 

        public NpcSkill(SkillDef def)
        {
            this.Def = def;
        }
        static public Dictionary<int, NpcSkill> Registry = new Dictionary<int, NpcSkill>();

        static public void Init(Hud hud)
        {
            hud.RegisterEventHandler(Components.Message.Types.SkillIncrease, OnSkillIncrease);
        }

        public Control GetControl()
        {
            var label = new Label()
            {
                TextFunc = () => $"{this.Def.Name}: {this.Level}",
                TooltipFunc = (t) =>
                {
                    t.AddControlsBottomLeft(
                        new Label(this.Def.Description),
                        new Label() { TextFunc = () => $"Current Level: {this.Level}"},
                        new Label() { TextFunc = () => $"Experience: {this.CurrentXP} / {this.XpToLevel}" });
                }
            };
            return label;
        }

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
            tag.TryGetTagValue("Level", out this.Level);
            //tag.TryGetTagValue<int>("CurrentXP", out this.CurrentXP);
            tag.TryGetTagValue<int>("CurrentXP", v=> this.CurrentXP = (float)v);
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
            this.CurrentXP = r.ReadSingle();// r.ReadInt32();
            return this;
        }
        public NpcSkill Clone()
        {
            return new NpcSkill(this.Def) { CurrentXP = this.CurrentXP, Level = this.Level };
        }
        public override string ToString()
        {
            return $"{this.Def.Label}: {this.Level} ({this.CurrentXP} / {this.XpToLevel})";
        }
    }
}
