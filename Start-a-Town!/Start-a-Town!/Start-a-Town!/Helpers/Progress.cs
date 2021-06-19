using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class Progress : IProgressBar
    {
        public virtual float Min { get; set; }
        public virtual float Max { get; set; }
        float _Value;
        public virtual float Value { get { return _Value; } set { this._Value = Math.Max(this.Min, Math.Min(this.Max, value)); } }
        public virtual float Percentage { get { return this.Value / this.Max; } 
            set { this.Value = this.Max * value; } }
        //public Progress()
        //{
        //    this.Min = 0;
        //    this.Max = this.Value = 100;
        //}
        public Progress()
        {
            this.Min = this.Value = 0;
            this.Max = 100;
        }
        public Progress(float min, float max, float value)
        {
            this.Min = min;
            this.Max = max;
            this.Value = value;
        }

        public virtual void Write(BinaryWriter io)
        {
            io.Write(this.Min);
            io.Write(this.Max);
            io.Write(this.Value);
        }
        public virtual void Read(BinaryReader io)
        {
            this.Min = io.ReadSingle();
            this.Max = io.ReadSingle();
            this.Value = io.ReadSingle();
        }
        public Progress(BinaryReader io)
        {
            this.Min = io.ReadSingle();
            this.Max = io.ReadSingle();
            this.Value = io.ReadSingle();
        }
        public SaveTag Save(string name)
        {
            SaveTag tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(new SaveTag(SaveTag.Types.Float, "Min", this.Min));
            tag.Add(new SaveTag(SaveTag.Types.Float, "Max", this.Max));
            tag.Add(new SaveTag(SaveTag.Types.Float, "Value", this.Value)); 
            return tag;
        }
        public void Load(SaveTag tag)
        {
            this.Min = tag.TagValueOrDefault<float>("Min", 0);
            this.Max = tag.TagValueOrDefault<float>("Max", 0);
            this.Value = tag.TagValueOrDefault<float>("Value", 0);
        }
        public Progress(SaveTag tag)
        {
            this.Min = tag.TagValueOrDefault<float>("Min", 0);
            this.Max = tag.TagValueOrDefault<float>("Max", 0);
            this.Value = tag.TagValueOrDefault<float>("Value", 0);
        }
        public override string ToString()
        {
            return this.Value.ToString() + "/" + this.Max.ToString();
        }
        public string ToStringAsSeconds()
        {
            if (this.Max == 0)
                return "";
            var ts = TimeSpan.FromMilliseconds(1000 * this.Value / 60f);
            string fmt = "";
            if (ts.Hours > 0)
                fmt += "%h'h '";
            if (ts.Minutes > 0)
                fmt += "%m'm '";
            if (ts.Seconds > 0)
                fmt += "%s's'";
            return ts.ToString(fmt);
        }
    }
}
