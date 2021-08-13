﻿using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public sealed class MaterialTypeDef : Def, IInspectable
    {
        public ReactionClass ReactionClass;
        public readonly MaterialCategory Category;
        public HashSet<MaterialDef> SubTypes = new();
        public float Shininess;
        public ToolUseDef SkillToExtract;

        public MaterialTypeDef(string name, MaterialCategory category)
            : base(name)
        {
            this.Category = category;
        }

        public void AddMaterial(MaterialDef mat)
        {
            mat.Type = this;
            this.SubTypes.Add(mat);
        }
    }

    static class MaterialTypeDefOf
    {

        static public readonly MaterialTypeDef Soil = new("Soil", MaterialCategory.Inorganic) { SkillToExtract = ToolUseDefOf.Digging };
        static public readonly MaterialTypeDef Stone = new("Stone", MaterialCategory.Inorganic) { SkillToExtract = ToolUseDefOf.Mining };
        static public readonly MaterialTypeDef Metal = new("Metal", MaterialCategory.Inorganic) { ReactionClass = ReactionClass.Tools, SkillToExtract = ToolUseDefOf.Mining };
        static public readonly MaterialTypeDef Gas = new("Gas", MaterialCategory.Inorganic);
        static public readonly MaterialTypeDef Water = new("Water", MaterialCategory.Inorganic);
        static public readonly MaterialTypeDef Glass = new("Glass", MaterialCategory.Inorganic);

        static public readonly MaterialTypeDef Meat = new("Meat", MaterialCategory.Creature) { ReactionClass = ReactionClass.Protein };
        static public readonly MaterialTypeDef Blood = new("Blood", MaterialCategory.Creature);
        static public readonly MaterialTypeDef Bone = new("Bone", MaterialCategory.Creature);

        static public readonly MaterialTypeDef Fruit = new("Fruit", MaterialCategory.Plant) { ReactionClass = ReactionClass.Protein };
        static public readonly MaterialTypeDef Dye = new("Dye", MaterialCategory.Plant);
        static public readonly MaterialTypeDef Wood = new("Wood", MaterialCategory.Plant) { ReactionClass = ReactionClass.Tools, SkillToExtract = ToolUseDefOf.Chopping, Shininess = .8f };
        static public readonly MaterialTypeDef PlantStem = new("PlantStem", MaterialCategory.Plant);
        static public readonly MaterialTypeDef Seed = new("Seed", MaterialCategory.Plant);

        static MaterialTypeDefOf()
        {
            Def.Register(typeof(MaterialTypeDefOf).GetFields().Select(f => f.GetValue(null) as Def));
        }
    }
}