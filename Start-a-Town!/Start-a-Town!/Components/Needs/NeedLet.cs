using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class NeedLet : ISerializable, ISaveable
    {
        public NeedLetDef Def;
        public float RateMod;
        public float ValueMod;
        public NeedLet()
        {

        }
        public NeedLet(NeedLetDef needLetDef, float value, float rate)
        {
            this.Def = needLetDef;
            this.RateMod = rate;
            this.ValueMod = value;
        }

        public override string ToString()
        {
            return string.Format("{0}: ValueMod: {1:+#;-#;0} RateMod: {2:+#;-#;0}", this.Def.Name, this.ValueMod, this.RateMod);
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.Def.Name.Save("Def"));
            tag.Add(this.RateMod.Save("RateMod"));
            tag.Add(this.ValueMod.Save("ValueMod"));

            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            //tag.TryGetTagValue<string>("Def", t => this.Def = Start_a_Town_.Def.GetDef<NeedLetDef>(t));
            tag.LoadDef<NeedLetDef>("Def");
            tag.TryGetTagValue<float>("RateMod", out this.RateMod);
            tag.TryGetTagValue<float>("ValueMod", out this.ValueMod);

            return this;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Def.Name);
            w.Write(this.RateMod);
            w.Write(this.ValueMod);
        }

        public ISerializable Read(BinaryReader r)
        {
            //this.Def = Start_a_Town_.Def.GetDef<NeedLetDef>(r.ReadString());
            this.Def = r.ReadDef<NeedLetDef>();
            this.RateMod = r.ReadSingle();
            this.ValueMod = r.ReadSingle();
            return this;
        }
    }
}
