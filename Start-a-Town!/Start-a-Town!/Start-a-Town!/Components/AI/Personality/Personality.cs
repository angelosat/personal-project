using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.AI;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components.AI
{
    public enum ReactionType { Friendly, Hostile }
    public class Personality : Component //: ICloneable
    {
        public override string ComponentName
        {
            get { return "Personality"; }
        }
        public override object Clone()
        {
            Personality p = new Personality(Reaction, Hatelist.ToArray());
            return p;
        }

        public int PatienceOld { get { return (int)this["Patience"]; } set { this["Patience"] = value; } }
        public ReactionType Reaction { get { return (ReactionType)this["Reaction"]; } set { this["Reaction"] = value; } }
        public List<string> Hatelist { get { return (List<string>)this["Hatelist"]; } set { this["Hatelist"] = value; } }

        public Dictionary<Trait.Types, Trait> Traits = Trait.All;// new List<Trait>();
        public Trait Patience;
        public Trait Attention;
        public Trait Composure;

        public Personality(ReactionType reaction = ReactionType.Friendly, params string[] hatedTypes)//, params Need[] needs)
        {
            this.PatienceOld = 100;
            this.Hatelist = new List<string>(hatedTypes);
            this.Reaction = reaction;
            this.Patience = this.Traits[Trait.Types.Patience];
            this.Attention = this.Traits[Trait.Types.Attention];
            this.Composure = this.Traits[Trait.Types.Composure];
        }

        public Control GetUI()
        {
            var box = new GroupBox();
            foreach (var t in this.Traits)
            {
                box.AddControlsBottomLeft(t.Value.GetUI());
            }
            return box;
        }

        internal void Generate(GameObject npc, RandomThreaded random)
        {
            var budget = random.Next((int)Trait.MinDefault, (int)Trait.MaxDefault);
            List<int> values = new List<int>();
            for (int i = 0; i < this.Traits.Count - 1; i++)
            {
                values.Add(random.Next((int)Trait.MinDefault, (int)Trait.MaxDefault));
            }
            values.Add(budget - values.Sum());

            for (int i = 0; i < this.Traits.Count; i++)
            {
                var trait = this.Traits.ElementAt(i);
                var value = values[i];
                trait.Value.Value = value;
            }
            return;
            foreach(var trait in this.Traits.Values)
            {
                trait.Value = random.Next((int)trait.Min, (int)trait.Max);
            }
        }
        //internal void Generate(GameObject npc, int budget, RandomThreaded random)
        //{
        //    foreach (var trait in this.Traits.Values)
        //    {
        //        trait.Value = random.Next((int)trait.Min, (int)trait.Max);
        //    }
        //}
        public override void Write(System.IO.BinaryWriter w)
        {
            foreach (var trait in this.Traits)
                trait.Value.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            foreach (var trait in this.Traits)
                trait.Value.Read(r);
        }
    }
}
