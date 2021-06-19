using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting.Blocks;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Blocks.Chest
{
    partial class BlockChest : Block
    {
        static Texture2D ChestNormalMap = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/furniture/chestnormal");
        public override Material GetMaterial(byte blockdata)
        {
            //return Material.LightWood;
            return Material.Database[blockdata];
        }
        public BlockChest()
            : base(Block.Types.Chest, opaque: false)
        {
            //this.Material = Material.LightWood;
            //this.MaterialType = MaterialType.Wood;
            //this.AssetNames = "chest";

            var tex = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/furniture/chest").ToGrayscale();
            //this.Variations.Add(Block.Atlas.Load("blocks/furniture/chest", Map.BlockDepthMap, ChestNormalMap));
            this.Variations.Add(Block.Atlas.Load("chestgrayscale", tex, Map.BlockDepthMap, ChestNormalMap));

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfSubType(ItemSubType.Planks, ItemSubType.Ingots)
                        //,
                        //Reaction.Reagent.IsOfMaterialType(MaterialType.Wood, MaterialType.Metal),
                        //Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                        )),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            Towns.Constructions.ConstructionsManager.Furniture.Add(this.Recipe);
        }
        public override BlockRecipe GetRecipe()
        {
            return this.Recipe;
        }
        public override IEnumerable<byte> GetCraftingVariations()
        {
            var vars = (from mat in Material.Database.Values
                        where mat.Type == MaterialType.Wood || mat.Type == MaterialType.Metal
                        select (byte)mat.ID);
            return vars;
        }
        public override BlockEntity CreateBlockEntity()
        {
            return new BlockChestEntity(16);
        }
        public override Vector4 GetColorVector(byte data)
        {
            var mat = Material.Database[data];
            var c = mat.ColorVector;
            return c;
        }
        //public override void Remove(IMap map, Vector3 global)
        //{
        //    var entity = map.GetBlockEntity(global) as Entity;
        //    entity.Break(map, global);
        //    base.Remove(map, global);
        //}

        public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, Interaction> list)
        {
            BlockChestEntity.GetPlayerActionsWorld(list);
        }
        public override void ShowUI(Vector3 global)
        {
            var entity = Net.Client.Instance.Map.GetBlockEntity(global) as BlockChestEntity;
            var window = new WindowEntityInterface(entity, "Chest", () => global);
            var ui = new ContainerUI().Refresh(global, entity);
            window.Client.Controls.Add(ui);
            window.Show();
        }
        //private static void ShowUI(Vector3 global)
        //{
        //    var entity = Net.Client.Instance.Map.GetBlockEntity(global) as BlockChestEntity;
        //    var window = new WindowEntityInterface(entity, "Chest", () => global);
        //    var ui = new ContainerUI().Refresh(global, entity);
        //    window.Client.Controls.Add(ui);
        //    window.Show();
        //}

    }
}
