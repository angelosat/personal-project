using System.IO;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class ProgressLeveledExp : Progress
    {
        public int Level { get; private set; }
        
        public void SetLevel(int level)
        {
            if (level == this.Level)
                return;
            this.Value = 0;
            this.Level = level;
        }
        readonly int BaseAmountToLevel;

        public ProgressLeveledExp(int amountToLevelBase, int level)
        {
            this.BaseAmountToLevel = amountToLevelBase;
            this.Level = level;
        }

        public override float Max { get => GetAmountRequired(this.Level + 1) ; }

        float GetAmountRequired(int level)
        {
            if (level == 0)
                return BaseAmountToLevel;
            return level * BaseAmountToLevel + GetAmountRequired(level - 1);
        }

        public override float Value
        {
            set
            {
                if (value >= this.Max)
                {
                    value -= this.Max;
                    this.Level++;
                }
                base.Value = value;
            }
        }

        public Control GetControl()
        {
            var box = new GroupBox();
            box.AddControlsBottomLeft(
                new Label() { TextFunc = () => string.Format("Current Level: {0}", this.Level) },
                new Label() { TextFunc = () => string.Format("Next Level: {0} / {1}", (int)this.Value, (int)this.Max) });
            return box;
        }

        protected override void SaveExtra(SaveTag tag)
        {
            this.Level.Save(tag, "Level");
        }
        protected override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTagValue<int>("Level", v => this.Level = v);
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            w.Write(this.Level);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.Level = r.ReadInt32();
        }
    }
}
