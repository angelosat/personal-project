using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public sealed class Moodlet : ISaveable, ISerializable, INamed
    {
        public enum Modes { Finite, Indefinite }


        public MoodletDef Def { get; private set; }
        int TicksRemaining;

        public string Name => this.Def.Name;

        public bool Tick(Actor parent)
        {
            if (!this.Def.Condition(parent))
                return false;
            if (this.Def.Mode == Modes.Indefinite)
                return true;
            
            this.TicksRemaining--;
            return this.TicksRemaining > 0;
        }
        public Moodlet()
        {
            
        }
        public Moodlet(int ticks = 0)
        {
            this.TicksRemaining = ticks;
        }

        public Moodlet(MoodletDef moodletDef)
        {
            this.Def = moodletDef;
            this.TicksRemaining = this.Def.Duration;
        }

        //public void TryAssign(Actor actor)
        //{
        //    var hasMoodlet = actor.HasMoodlet(this);
        //    var condition = this.Def.Condition(actor);
        //    if (!condition && !hasMoodlet)
        //        actor.AddMoodlet(this);
        //    else if (condition && hasMoodlet)
        //        actor.RemoveMoodlet(this);
        //}
        public Control GetUI()
        {
            return new Label(string.Format("{0} {1}", this.Def.Description, this.Def.Value.ToString("+#;-#;0"))) {
                TextColorFunc = () => this.Def.Value < 0 ? Color.Red : Color.Lime,
                HoverFunc = () => this.Def.Mode == Modes.Finite ? string.Format("{0} remaining", (this.TicksRemaining / Engine.TicksPerSecond).ToString(" #0.##s")) : ""
            };
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);// this.Def.Name);
            tag.Add(this.Def.Name.Save("Def"));
            tag.Add(this.TicksRemaining.Save("TicksRemaining"));
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            //tag.TryGetTag("Def", t => this.Def = MoodletDef.Dictionary[t]);
            tag.TryGetTagValue<string>("Def", t => this.Def = Start_a_Town_.Def.GetDef<MoodletDef>(t));
            tag.TryGetTagValue<int>("TicksRemaining", out this.TicksRemaining);
            return this;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Def.Name);
            w.Write(this.TicksRemaining);
        }

        public ISerializable Read(BinaryReader r)
        {
            this.Def = Start_a_Town_.Def.GetDef<MoodletDef>(r.ReadString());
            this.TicksRemaining = r.ReadInt32();
            return this;
        }
    }
}
