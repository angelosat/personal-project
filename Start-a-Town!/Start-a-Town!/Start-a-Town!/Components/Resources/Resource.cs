using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Start_a_Town_.Components.Resources;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    abstract class Resource : Component, IProgressBar
    {
        public class Recovery : Progress
        {
            public override float Percentage { get { return 1 - this.Value / this.Max; } }
            public Recovery()
            {
                this.Min = 0;
                this.Max = this.Value = Engine.TargetFps;
            }
        }

        public enum Types { Health, Mana, Stamina, Durability }

        static List<Resource> _Registry;
        public static List<Resource> Registry
        {
            get
            {
                if (_Registry == null)
                    Initialize();
                return _Registry;
            }
        }

        static void Initialize()
        {
            _Registry = new List<Resource>()
            {
                new Health(),
                new Durability(),
                new Stamina()
                //new Resource(Types.Health, "Health"),
                //new Resource(Types.Mana, "Mana"),
                //new Resource(Types.Stamina, "Stamina"),
                //new Resource(Types.Durability, "Durability"),
            };
        }

        static Resource GetResource(Types id)
        {
            return Registry.ToDictionary(foo => foo.ID, foo => foo)[id];
        }

        public abstract Types ID { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        float _Value;
        public float Value
        {
            get { return _Value; }
            //set{_Value = value;}
            //private
            set
            {
                this._Value = Math.Max(0, Math.Min(this.Max, value));
            }
        }
        public virtual void Add(float add)
        {
            this.Value += add;
        }

        //public float Min { get; set; }
        //public float Max { get; set; }
        float _Min, _Max;
        public float Min
        {
            get
            {
                return _Min;
            }
            set
            {
                this._Min = value;
                this.Value = this.Value;
            }
        }
        public float Max
        {
            get
            {
                return _Max;
            }
            set
            {
                this._Max = value;
                this.Value = this.Value;
            }
        }
        public float Percentage { get { return this.Value / this.Max; } }
        //Resource(Types id, string name, float value = 1, float max = 1)
        //{
        //    this.ID = id;
        //    this.Name = name;
        //    this.Value = value;
        //    this.Max = max;
        //}

        //public override object Clone()
        //{
        //    //return new Resource(this.ID, this.Name, this.Value, this.Max) { Description = this.Description };
        //    return Create(this.ID, this.Value, this.Min, this.Max);
        //}

        static public Resource Create(Resource toCopy)
        {
            return Create(toCopy.ID, toCopy.Value, toCopy.Min, toCopy.Max);
        }
        static public Resource Create(Types id, float value, float min, float max)
        {
            Resource r = GetResource(id).Clone() as Resource;
            r.Min = min;
            r.Max = max;
            r.Value = value;
            return r;
        }
        static public Resource Create(Types id, float value, float max)
        {
            return Create(id, value, 0, max);
        }
        static public Resource Create(Types id)
        {
            return Create(id, 1, 1);
        }
        static public Resource Create(Types id, float max)
        {
            return Create(id, max, max);
        }
        static public Resource Create(SaveTag tag)
        {
            var id = (Types)tag.GetValue<int>("ID");
            var min = tag.GetValue<float>("Min");
            var max = tag.GetValue<float>("Max");
            var val = tag.GetValue<float>("Value");
            return Create(id, val, min, max);
        }

        public override string ToString()
        {
            //return this.Name + ": " + this.Value.ToString("##0.00") + " / " + this.Max.ToString("##0.00");
            return this.Name + ": " + this.Value.ToString(this.Format) + " / " + this.Max.ToString(this.Format);
        }
        public virtual string Format { get { return ""; } }

        //public override string ToString()
        //{
        //    return this.Name + ": " + (int)this.Value + " / " + (int)this.Max;// +" " + (this.Value / this.Max).ToString("(0%)");
        //}
        internal SaveTag Save(string name)
        {
            SaveTag tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(new SaveTag(SaveTag.Types.Int, "ID", (int)this.ID));
            tag.Add(new SaveTag(SaveTag.Types.Float, "Min", this.Min));
            tag.Add(new SaveTag(SaveTag.Types.Float, "Max", this.Max));
            tag.Add(new SaveTag(SaveTag.Types.Float, "Value", this.Value));
            return tag;
        }

        //protected virtual void OnNameplateCreated(Nameplate plate) { }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(this.Max);
            writer.Write(this.Value);
           
        }
        public override void Read(BinaryReader reader)
        {
            this.Max = reader.ReadSingle();
            this.Value = reader.ReadSingle();
        }

        public virtual Control GetControl() { return null; }
    }
}
