﻿using System.IO;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public sealed class Trait : Inspectable, ISaveable, ISerializable, IProgressBar, INamed, IListable
    {
        public float Percentage
        {
            get => this.Value / MaxDefault;
            set => this.Value = (MaxDefault - MinDefault) * value;
        }

        public TraitDef Def;
        public string Name => this.Def.Name;
        public string Label => this.Value >= 0 ? this.Def.NamePositive : this.Def.NameNegative;
        public const float MinDefault = -100;
        public const float MaxDefault = 100;
        public const float ValueRange = 100;
        public float Value;
        public float Normalized => this.Value / ValueRange;  //unsigned. do i want this?
        public float Min => MinDefault;
        public float Max => MaxDefault;

        public Trait(TraitDef def)
        {
            this.Def = def;
        }
        public override string ToString()
        {
            return $"{this.Def.Name}: {this.Value}";
        }

        public Control GetListControlGui()
        {
            var box = new Panel() { AutoSize = true, BackgroundStyle = BackgroundStyle.TickBox };
            var bar = new BarSigned() { Object = this, TextFunc = () => this.Label, HoverFunc = () => $"{this.Def.Name}: {this.Value} ({this.Label})\n{this.Def.Description.Wrap(TooltipManager.Width)}" };
            box.AddControls(bar);
            return box;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, this.Def.Name);
            tag.Add(this.Value.Save("Value"));
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTagValue<float>("Value", out this.Value);
            return this;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Value);
        }

        public ISerializable Read(BinaryReader r)
        {
            this.Value = r.ReadSingle();
            return this;
        }
    }
}
