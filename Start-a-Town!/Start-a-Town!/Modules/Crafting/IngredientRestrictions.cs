﻿using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Start_a_Town_
{
    //public partial class CraftOrderNew
    //{
    public class IngredientRestrictions : ISaveable, ISerializable
    {
        public HashSet<Material> Material = new();
        public HashSet<MaterialType> MaterialType = new();
        public HashSet<ItemDef> ItemDef = new();
        public IngredientRestrictions()
        {

        }
        public IngredientRestrictions(IngredientRestrictions source)
        {
            this.Material = new(source.Material);
            this.ItemDef = new(source.ItemDef);
            this.MaterialType = new(source.MaterialType);

        }
        public bool IsRestricted(Material item)
        {
            return this.Material.Contains(item);
        }
        public bool IsRestricted(MaterialType item)
        {
            return this.MaterialType.Contains(item);
        }
        public bool IsRestricted(ItemDef item)
        {
            return this.ItemDef.Contains(item);
        }

        public IngredientRestrictions Restrict(Material mat)
        {
            this.Material.Add(mat);
            return this;
        }
        public IngredientRestrictions Restrict(MaterialType mattype)
        {
            //this.MaterialType.Add(item);
            foreach (var m in mattype.SubTypes)
                this.Restrict(m);
            return this;
        }
        public IngredientRestrictions Restrict(ItemDef item)
        {
            this.ItemDef.Add(item);
            return this;
        }

        public void Toggle(Material item)
        {
            if (!this.Material.Contains(item))
                this.Material.Add(item);
            else
                this.Material.Remove(item);
        }
        public void Toggle(MaterialType item)
        {
            if (!this.MaterialType.Contains(item))
                this.MaterialType.Add(item);
            else
                this.MaterialType.Remove(item);
        }
        public void Toggle(ItemDef item)
        {
            if (!this.ItemDef.Contains(item))
                this.ItemDef.Add(item);
            else
                this.ItemDef.Remove(item);
        }

        internal void ToggleRestrictions(ItemDef[] defs, Material[] mats, MaterialType[] matTypes)
        {
            for (int i = 0; i < defs.Length; i++)
            {
                this.Toggle(defs[i]);
            }
            for (int i = 0; i < mats.Length; i++)
            {
                this.Toggle(mats[i]);
            }
            for (int i = 0; i < matTypes.Length; i++)
            {
                this.Toggle(matTypes[i]);
            }
        }

        internal bool IsRestricted(Entity item)
        {
            return
                this.ItemDef.Contains(item.Def) ||
                this.Material.Contains(item.Body.Material) || //will i have cases with multiple bones of different materials? find dominant material? or use the first bone's material?
                this.MaterialType.Contains(item.Body.Material.Type);
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.ItemDef.Select(d => d.Name).Save("ItemDef"));
            tag.Add(this.Material.Select(d => d.ID).Save("Material"));
            tag.Add(this.MaterialType.Select(d => d.ID).Save("MaterialType"));
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            //this.ItemDef = new HashSet<ItemDef>(tag.LoadStringList("ItemDef").Select(s => Def.GetDef<ItemDef>(s)));
            //this.ItemDef.TryLoadDefs<HashSet<ItemDef>, ItemDef>(tag, "Material");
            this.ItemDef.TryLoadDefs<ItemDef>(tag, "Material");

            this.Material = new HashSet<Material>(tag.LoadListInt("ItemDef").Select(s => Start_a_Town_.Material.GetMaterial(s)));
            this.MaterialType = new HashSet<MaterialType>(tag.LoadListInt("MaterialType").Select(s => Start_a_Town_.MaterialType.GetMaterialType(s)));
            return this;
        }


        public void Write(BinaryWriter w)
        {
            w.Write(this.ItemDef.Select(d => d.Name).ToArray());
            w.Write(this.Material.Select(d => d.ID).ToArray());
            w.Write(this.MaterialType.Select(d => d.ID).ToArray());
        }

        public ISerializable Read(BinaryReader r)
        {
            this.ItemDef = new HashSet<ItemDef>(r.ReadStringArray().Select(Def.GetDef<ItemDef>));
            this.Material = new HashSet<Material>(r.ReadIntArray().Select(Start_a_Town_.Material.GetMaterial));
            this.MaterialType = new HashSet<MaterialType>(r.ReadIntArray().Select(Start_a_Town_.MaterialType.GetMaterialType));
            return this;
        }
    }


    //}
}
