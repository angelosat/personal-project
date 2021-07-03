using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class BlockConstructionEntity : BlockEntity, IConstructible
    {
        public BlockRecipe.ProductMaterialPair Product;
        public List<ItemDefMaterialAmount> Container = new();
        public Progress BuildProgress { get; set; }
        public Vector3 Origin { get; set; }
        public List<Vector3> Children { get; set; } = new List<Vector3>();
        public BlockConstructionEntity()
        {

        }
        public BlockConstructionEntity(BlockRecipe.ProductMaterialPair product, Vector3 origin, GameObject initialMaterial, int amount)
        {
            this.Product = product;
            if (amount > initialMaterial.StackSize)
                throw new Exception();
            this.Container.Add(new ItemDefMaterialAmount(initialMaterial.Def, initialMaterial.PrimaryMaterial, amount));

            this.BuildProgress = new Progress(0, product.Recipe.WorkAmount, 0);
            this.Origin = origin;
        }

        public override object Clone()
        {
            throw new Exception();
        }
        public override void GetTooltip(UI.Control tooltip)
        {
            var product = this.Product;
            var req = product.Requirement;
            var block = product.Block;
            var ing = block.Ingredient;
            tooltip.AddControlsBottomLeft(new Label()
            {
                TextFunc = GetIngredientText
            });
        }
        internal override void GetSelectionInfo(IUISelection info, IMap map, Vector3 vector3)
        {
            var product = this.Product;
            info.AddInfo(new Label() { TextFunc =
                GetIngredientText }
            );
        
        }
        string GetIngredientText()
        {
            var product = this.Product;
            var req = product.Requirement;
            var block = product.Block;
            var ing = block.Ingredient;
            return $"{product.Requirement.Material.Name} {product.Requirement.Def.Label} {this.Container.First().Amount} / {product.Requirement.Amount}";
        }

        internal void HandleDepositedItem(GameObject dropped, int amount)
        {
            if (dropped.Def != this.Product.Requirement.Def)
                throw new Exception();
            if (dropped.PrimaryMaterial != this.Product.Requirement.Material)
                throw new Exception();
            var req = this.Container.FirstOrDefault(r => r.Def == dropped.Def);
            req.Amount += amount;
            dropped.StackSize -= amount;
            if (req.Amount > this.Product.Requirement.Amount)
                throw new Exception();
        }

        public bool IsReadyToBuild(out ItemDef def, out Material mat, out int amount)
        {
            var req = this.Container.First();
            if (req.Amount == this.Product.Requirement.Amount)
            {
                def = null;
                mat = null;
                amount = 0;
                return true;
            }
            def = this.Product.Requirement.Def;
            mat = this.Product.Requirement.Material;
            amount = this.Product.Requirement.Amount - req.Amount;
            return false;
        }

        public int GetMissingAmount(ItemDef def)
        {
            return this.GetReq(def).Amount - this.GetContainedMaterialAmount(def);
        }
        ItemDefMaterialAmount GetReq(ItemDef objid)
        {
            var req = this.Product.Requirement;
            if (objid != req.Def)
                throw new Exception();
            return req;
        }
        public bool IsValidHaulDestination(ItemDef objectID)
        {
            return this.Product.Requirement.Def == objectID;
        }
        private int GetContainedMaterialAmount(ItemDef def)
        {
            return this.Container.FirstOrDefault(o => o.Def == def).Amount;
        }

        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(new SaveTag(SaveTag.Types.Compound, "Product", this.Product.Save()));
            tag.Add(new SaveTag(SaveTag.Types.Vector3, "Origin", this.Origin));
            tag.Add(this.Children.Save("Children"));
            tag.Add(this.Container.SaveNewBEST("Container"));
            tag.Add(this.BuildProgress.Save("BuildProgress"));
        }
        protected override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTag("Product", t => this.Product = new BlockRecipe.ProductMaterialPair(t));
            tag.TryGetTagValue<Vector3>("Origin", t => this.Origin = t);
            tag.TryGetTagValue<List<SaveTag>>("Children", t => this.Children.Load(t));
            this.Container.TryLoadMutable(tag, "Container");
            tag.TryGetTag("BuildProgress", v => this.BuildProgress = new Progress(v));
        }

        protected override void WriteExtra(BinaryWriter w)
        {
            this.Product.Write(w);
            this.BuildProgress.Write(w);
            w.Write(this.Origin);
            w.Write(this.Children);
            this.Container.Write(w);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.Product = new BlockRecipe.ProductMaterialPair(r);
            this.BuildProgress = new Progress(r);
            this.Origin = r.ReadVector3();
            this.Children = r.ReadListVector3();
            this.Container.ReadMutable(r);
        }
    }
}
