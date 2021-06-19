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
        public BlockRecipe.ProductMaterialPair Product;// { get; set; }
        public List<ItemDefMaterialAmount> Container = new List<ItemDefMaterialAmount>();
        public Progress BuildProgress { get; set; }// = new Progress();
        public Vector3 Origin { get; set; }
        public List<Vector3> Children { get; set; } = new List<Vector3>();
        public BlockConstructionEntity()
        {

        }
        public BlockConstructionEntity(BlockRecipe.ProductMaterialPair product, Vector3 origin, GameObject initialMaterial, int amount)
        {
            this.Product = product;
            //foreach (var mat in product.Materials)
            //{
            //    this.Container.Add(new ObjectIDAmount(mat.ObjectID, initialMaterial.ID == mat.ObjectID ? initialMaterial.StackSize : 0));
            //}
            if (amount > initialMaterial.StackSize)
                throw new Exception();
            //this.Container.Add(new ItemDefAmount(product.Block.Ingredient.Item, amount));
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
            //var product = this.Product;
            //tooltip.AddControlsBottomLeft(new Label() { TextFunc = () => string.Format("{0} {1} {2} / {3}", product.Requirement.Material.Label, product.Requirement.Def.Label, this.Container.FirstOrDefault()?.Amount??0, product.Requirement.Amount) });

            var product = this.Product;
            var req = product.Requirement;
            var block = product.Block;
            var ing = block.Ingredient;
            tooltip.AddControlsBottomLeft(new Label()
            {
                TextFunc = GetIngredientText// () => $"{ing.Material.Name} {ing.ItemDef.Label} {this.Container.FirstOrDefault()?.Amount ?? 0} / {ing.Amount}"
            });
        }
        internal override void GetSelectionInfo(IUISelection info, IMap map, Vector3 vector3)
        {
            var product = this.Product;
            info.AddInfo(new Label() { TextFunc =
                //() => string.Format("{0} {1} {2} / {3}", product.Requirement.Material.Name, product.Requirement.Def.Label, this.Container.First().Amount, product.Requirement.Amount) 
                GetIngredientText }
            );
        
        }
        string GetIngredientText()
        {
            var product = this.Product;
            var req = product.Requirement;
            var block = product.Block;
            var ing = block.Ingredient;
            //return $"{ing.Material.Name} {ing.ItemDef.Label} {this.Container.FirstOrDefault()?.Amount ?? 0} / {ing.Amount}";
            return $"{product.Requirement.Material.Name} {product.Requirement.Def.Label} {this.Container.First().Amount} / {product.Requirement.Amount}";
        }
        Control GetMaterialsControl()
        {
            var box = new GroupBox();
            box.AddControls(new Label() { TextFunc = () => "placeholder" });
            //foreach (var req in this.Product.Materials)
            //{
            //    var containedAmount = this.Container.FirstOrDefault(o => o.ObjectID == req.ObjectID)?.Amount ?? 0;
            //    var label = GameObject.Objects[req.ObjectID].Name;
            //    box.AddControlsBottomLeft(new Label()
            //    {
            //        TextFunc = () =>
            //        {
            //            return string.Format("{0} {1} / {2}", label, GetContainedMaterialAmount(req.ObjectID), req.Amount);
            //        }
            //    });
            //}
            return box;
        }

        internal void HandleDepositedItem(GameObject dropped, int amount)
        {
            //if (dropped.Def != this.Product.Block.Ingredient.Item)
            //    throw new Exception();
            if (dropped.Def != this.Product.Requirement.Def)
                throw new Exception();
            if (dropped.PrimaryMaterial != this.Product.Requirement.Material)
                throw new Exception();
            var req = this.Container.FirstOrDefault(r => r.Def == dropped.Def);
            req.Amount += amount;
            dropped.StackSize -= amount;
            //if (req.Amount > this.Product.Block.Ingredient.Amount)
            //    throw new Exception();
            if (req.Amount > this.Product.Requirement.Amount)
                throw new Exception();
        }

        public bool IsReadyToBuild(out ItemDef def, out Material mat, out int amount)
        {
            //var req = this.Container.First();
            //if(req.Amount == this.Product.Block.Reagent.Amount)
            //{
            //    def = null;
            //    mat = null;
            //    amount = 0;
            //    return true;
            //}
            //def = this.Product.Block.Reagent.Def;
            //mat = this.Product.MainMaterial;
            //amount = this.Product.Block.Reagent.Amount;
            //return false;
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
            //foreach (var req in this.Product.Materials)
            //{
            //    int missing = GetMissingAmount(req.ObjectID);
            //    if (missing > 0)
            //    {
            //        reqAmount = new ObjectIDAmount(req.ObjectID, missing);
            //        return false;
            //    }
            //}
            //reqAmount = null;
            //return true;
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
            //return this.Container.FirstOrDefault(d => d.Def == objid);
        }
        public bool IsValidHaulDestination(ItemDef objectID)
        {
            return this.Product.Requirement.Def == objectID;
            //return this.Product.Block.Ingredient.Item == objectID;
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

            //this.Container.TryLoad(tag, "Container");
            //this.Container = tag.LoadList<ItemDefAmount>("Container").ToList();
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
            //this.Container.Write(w);
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
