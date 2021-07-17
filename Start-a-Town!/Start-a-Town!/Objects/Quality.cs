using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public sealed class Quality : Def
    {
        public readonly string Label;
        static Random Rand = new();

        public readonly Color Color;
        public readonly float Multiplier;
        Quality(string label, Color color, float multiplier, int probabilityWeight, int masterySensitivity = 0) : base($"ItemQuality{label}")
        {
            this.Label = label;
            this.Color = color;
            this.Multiplier = multiplier;
            this.ProbabilityTableWeight = probabilityWeight;
            this.MasterySensitivity = masterySensitivity;
        }
        
        int ProbabilityTableWeight;
        float MasterySensitivity;
        public int GetWeightFromMastery(float masteryRatio)
        {
            var masteryExcess = masteryRatio - 1;
            var mastery = (int)(masteryExcess * MasterySensitivity);
            return ProbabilityTableWeight + mastery;
        }

        static public readonly Quality Trash = new("Trash", Color.Gray, .5f, 5, -5);
        static public readonly Quality Common = new("Common", Color.White, 1f, 50);
        static public readonly Quality Uncommon = new("Uncommon", Color.Lime, 1.2f, 30, 5);
        static public readonly Quality Rare = new("Rare", Color.DodgerBlue, 1.4f, 10, 10);
        static public readonly Quality Epic = new("Epic", Color.BlueViolet, 1.6f, 4, 15);
        static public readonly Quality Legendary = new("Legendary", Color.DarkOrange, 1.8f, 1, 20);
        static public readonly Quality Unique = new("Unique", Color.Yellow, 2f, 0);
        static public readonly Quality Cheating = new("Cheating", Color.LightSkyBlue, 100f, 0);
        readonly static Quality[] All = new[]
        {
            Trash, Common, Uncommon, Rare, Epic, Legendary
        };
        static Quality()
        {
            foreach (var q in All)
                Register(q);
        }
        static public Quality GetRandom(Random rand, float mastery) => All.SelectRandomWeighted(rand, q => q.GetWeightFromMastery(mastery));
        static public Quality GetRandom(Random rand) => All.SelectRandomWeighted(rand, q => q.ProbabilityTableWeight);
        static public Quality GetRandom() => All.SelectRandomWeighted(Rand, q => q.ProbabilityTableWeight);
    }
}
