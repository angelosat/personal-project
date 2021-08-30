﻿using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public enum ReactionType { Friendly, Hostile }
    public class PersonalityComponent : EntityComponent
    {
        static readonly Random Randomizer = new();

        public override object Clone()
        {
            return new PersonalityComponent(this.Traits.Select(d => d.Def).ToArray());
        }

        public ReactionType Reaction;
        public List<string> Hatelist;
        HashSet<MaterialDef> Favorites = new();
        public Trait[] Traits;

        public override string Name { get; } = "Personality";

        public PersonalityComponent(ItemDef def)
        {
            var traits = def.ActorProperties.Traits;
            var count = traits.Length;
            this.Traits = new Trait[count];
            for (int i = 0; i < count; i++)
            {
                this.Traits[i] = new Trait(traits[i]);
            }
            this.Randomize();
        }
        public PersonalityComponent()
        {

        }
        public PersonalityComponent(ReactionType reaction = ReactionType.Friendly, params string[] hatedTypes)
        {
            this.Hatelist = new List<string>(hatedTypes);
            this.Reaction = reaction;

        }
        public PersonalityComponent(params TraitDef[] traits)
        {
            var count = traits.Length;
            this.Traits = new Trait[count];
            for (int i = 0; i < count; i++)
            {
                this.Traits[i] = new Trait(traits[i]);
            }
            this.Randomize();
        }
        public Control GetCreationGui()
        {
            var box = new GroupBox();
            foreach (var t in this.Traits)
                box.AddControlsBottomLeft(t.GetListControlGui());
            return box;
        }
        public Trait GetTrait(TraitDef def)
        {
            return this.Traits.First(t => t.Def == def);
        }
        public IEnumerable<MaterialDef> GetFavorites()
        {
            foreach (var i in this.Favorites)
                yield return i;
        }
        /// <summary>
        /// https://softwareengineering.stackexchange.com/questions/254301/algorithm-to-generate-n-random-numbers-between-a-and-b-which-sum-up-to-x
        /// </summary>
        public PersonalityComponent Randomize()
        {
            RandomizeTraits();
            RandomizeFavorites();
            return this;
        }

        private void RandomizeFavorites()
        {
            this.Favorites = GenerateMaterialPreferences();
        }

        private void RandomizeTraits()
        {
            int budget = 0; //placeholder
            var random = Randomizer;
            var count = this.Traits.Length;
            double sum = 0;
            double[] values = new double[count];
            double min = -1, max = 1;
            for (int i = 0; i < count - 1; i++)
            {
                var rest = count - (i + 1);
                double restmin = min * rest;
                double restmax = max * rest;
                min = Math.Max(min, sum - restmax);
                max = Math.Min(max, sum - restmin);

                var v = getV(min, max);
                if (Math.Abs(v) > Trait.ValueRange)
                    throw new Exception();
                sum -= v;
                values[i] = v;
            }
            values[count - 1] = budget + sum;

            var totalSum = values.Sum();
            if (totalSum != budget)
                throw new Exception();


            for (int i = 0; i < count; i++)
            {
                var value = values[i];
                this.Traits[i].Value = (int)(value * Trait.ValueRange);
                if (Math.Abs(value) > Trait.ValueRange)
                    throw new Exception();
            }

            static double getV(double minimum, double maximum)
            {
                return RandomHelper.NextNormal(minimum, maximum);
            }
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            this.Traits.Write(w);
            this.Favorites.WriteDefs(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Traits.Read(r);
            this.Favorites.Clear();
            this.Favorites.ReadDefs(r);
        }
        internal override void SaveExtra(SaveTag tag)
        {
            this.Traits.SaveImmutable(tag, "Traits");
            this.Favorites.SaveDefs(tag, "Favorites");
        }
        internal override void LoadExtra(SaveTag tag)
        {
            this.Traits.TryLoadImmutable(tag, "Traits");
            this.Favorites.Clear();
            if (!this.Favorites.TryLoadDefs(tag, "Favorites"))
                this.Favorites = GenerateMaterialPreferences();
        }
        static Control GUI;
        internal static Window GetGui(Actor actor)
        {
            Window win;
            if (GUI == null)
            {
                GUI = new GroupBox();
                win = new Window(GUI) { Movable = true, Closable = true };
                win.OnSelectedTargetChangedAction = t =>
                {
                    if (t.Object is not Actor actor)
                        win.Hide();
                    else
                        GetGui(actor);
                };
            }
            else
                win = GUI.GetWindow();
            win.Title = $"{actor.Name} personality";
            GUI.ClearControls();
            var p = actor.Personality;
            var boxtraits = new GroupBox();
            foreach (var t in p.Traits)
                boxtraits.AddControlsBottomLeft(t.GetListControlGui());
            GUI.AddControlsVertically(boxtraits.ToPanelLabeled("Traits"), getFavoritesUI(boxtraits.Width));
            return win;

            Control getFavoritesUI(int width)
            {
                var box = UIHelper.Wrap(p.Favorites.Select(m => new Button(m.Label) { TextColorFunc = () => m.Color }), width);
                return box.ToPanelLabeled("Favorite Materials");
            }
        }

        public class Props : ComponentProps
        {
            public override Type CompClass => typeof(NpcSkillsComponent);
            public TraitDef[] Items;
            public Props(params TraitDef[] defs)
            {
                this.Items = defs;
            }
        }

        static public HashSet<MaterialDef> GenerateMaterialPreferences()
        {
            var list = new HashSet<MaterialDef>();
            foreach (var type in Def.GetDefs<MaterialTypeDef>())
            {
                if (type.SubTypes.Any())
                    list.Add(type.SubTypes.SelectRandom(Randomizer));
            }
            return list;
        }
    }
}
