using System;
using System.IO;
using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class Skill : ISaveable, ISerializable, INamed, IListable
    {
        public NpcSkillsComponent Container;
        public SkillDef Def;
        public int Level;
        Progress LvlProgress = new();
        const int XpToLevelBase = 5;

        public Skill(SkillDef def)
        {
            this.Def = def;
        }

        public int XpToLevel => (int)this.LvlProgress.Max;
        public float CurrentXP => this.LvlProgress.Value;
        public string Name => this.Def.Label;
        public string Label => this.Name;

        [Obsolete]
        static int GetNextLvlXpOld(int currentLvl) => (int)Math.Pow(XpToLevelBase, currentLvl + 1);
        static int GetNextLvlXp(int currentLvl) => currentLvl > 0 ? (int)Math.Pow(XpToLevelBase, currentLvl) * (XpToLevelBase - 1) : XpToLevelBase;
        static public void Init(Hud hud)
        {
            hud.RegisterEventHandler(Components.Message.Types.SkillIncrease, OnSkillIncrease);
        }
        static void OnSkillIncrease(GameEvent a)
        {
            var actor = a.Parameters[0] as GameObject;
            var skill = (string)a.Parameters[1];
            FloatingText.Manager.Create(actor, $"{skill} increased!", ft => ft.Font = UIManager.FontBold);
        }
        internal void Award(float v)
        {
            v *= 10;
            if (this.LvlProgress.Value + v < this.LvlProgress.Max)
            {
                this.LvlProgress.Value += v;
                return;
            }
            var remaining = this.LvlProgress.Value + v;
            int levelsGained = 0;
            int nextLvlXp = (int)this.LvlProgress.Max;
            do
            {
                remaining -= nextLvlXp;
                nextLvlXp = GetNextLvlXp(this.Level + levelsGained++);
            } while (remaining >= nextLvlXp);
            this.Level += levelsGained;
            this.LvlProgress.Max = GetNextLvlXp(this.Level);
            this.LvlProgress.Value = remaining;
            var actor = this.Container.Parent;
            Log.Write($"[{actor}]'s [{this.Label}] has been increased to {this.Level}!");
            actor.Net.EventOccured(Message.Types.SkillIncrease, actor, this.Def.Name);
        }
        [Obsolete]
        internal void AwardOld(float v)
        {
            v *= 500;
            if (this.LvlProgress.Value + v < this.LvlProgress.Max)
            {
                this.LvlProgress.Value += v;
                return;
            }
            this.LvlProgress.Value = this.LvlProgress.Value + v - this.LvlProgress.Max; // carry over superfluous xp to next lvl
            this.LvlProgress.Max = GetNextLvlXp(++this.Level);
            var actor = this.Container.Parent;
            Log.Write($"[{actor}]'s [{this.Label}] has been increased to {this.Level}!");
            actor.Net.EventOccured(Message.Types.SkillIncrease, actor, this.Def.Name);
            return;
        }
        public Control GetListControlGui()
        {
            var label = new Bar(this.LvlProgress)// Label()
            {
                Width = 200,
                TextFunc = () => $"{this.Def.Label}: {this.Level}",
                TooltipFunc = (t) =>
                {
                    t.AddControlsBottomLeft(
                        new Label(this.Def.Description),
                        new Label() { TextFunc = () => $"Current Level: {this.Level}" },
                        new Label() { TextFunc = () => $"Experience: {this.CurrentXP} / {this.XpToLevel}" });
                }
            };
            return label;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, this.Def.Name);
            tag.Add(this.Level.Save("Level"));
            tag.Add(this.LvlProgress.Save("Progress"));
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTagValue("Level", out this.Level);
            this.LvlProgress.Max = GetNextLvlXp(this.Level);
            if (!tag.TryGetTag("Progress", t => this.LvlProgress.Load(t)))
                tag.TryGetTagValue<float>("CurrentXP", v => this.LvlProgress.Value = v);
            
            return this;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Level);
            //w.Write(this.CurrentXP);
            this.LvlProgress.Write(w);
        }

        public ISerializable Read(BinaryReader r)
        {
            this.Level = r.ReadInt32();
            this.LvlProgress.Read(r);
            return this;
        }
        public Skill Clone()
        {
            return new Skill(this.Def) { LvlProgress = new Progress(this.LvlProgress), Level = this.Level };
        }
        public override string ToString()
        {
            return $"{this.Def.Label}: {this.Level} ({this.CurrentXP} / {this.XpToLevel})";
        }
    }
}
