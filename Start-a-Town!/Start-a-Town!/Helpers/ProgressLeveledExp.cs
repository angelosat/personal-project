using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class ProgressLeveledExp : Progress
    {
        //private int level;
        public int Level { get; private set; }
        //{ 
        //    get => level; 
        //    set
        //    {
        //        if (level != value) 
        //            this.Value = 0; 
        //        level = value;
        //    }
        //}
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

            //for (int i = 0; i < 10; i++)
            //{
            //    var val = GetAmountRequired(i);
            //    string.Format("{0}: {1}", i, val).ToConsole();
            //}
        }

        //public override float Max { get => (int)Math.Pow(AmountToLevelBase, this.Level + 1); }
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
