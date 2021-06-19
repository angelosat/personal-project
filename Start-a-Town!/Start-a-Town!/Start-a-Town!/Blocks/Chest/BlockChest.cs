using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Materials;
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
            return Material.Templates[blockdata];
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

            this.Recipe = new BlockConstruction(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfSubType(ItemSubType.Planks, ItemSubType.Bars)
                        //,
                        //Reaction.Reagent.IsOfMaterialType(MaterialType.Wood, MaterialType.Metal),
                        //Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                        )),
                    new BlockConstruction.Product(this),
                    Components.Skills.Skill.Building);
        }
        public override BlockConstruction GetRecipe()
        {
            return this.Recipe;
        }
        public override List<byte> GetVariations()
        {
            var vars = (from mat in Material.Templates.Values
                        where mat.Type == MaterialType.Wood || mat.Type == MaterialType.Metal
                        select (byte)mat.ID).ToList();
            return vars;
        }
        public override BlockEntity GetBlockEntity()
        {
            return new Entity(16);
        }
        public override Vector4 GetColorVector(byte data)
        {
            var mat = Components.Materials.Material.Templates[data];
            var c = mat.ColorVector;
            return c;
        }
        //public override void Remove(IMap map, Vector3 global)
        //{
        //    var entity = map.GetBlockEntity(global) as Entity;
        //    entity.Break(map, global);
        //    base.Remove(map, global);
        //}

        public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, Components.Interactions.Interaction> list)
        {
            Entity.GetPlayerActionsWorld(list);
        }
        //public override ContextAction GetRightClickAction(Vector3 global)
        //{
        //    return new ContextAction(() => "Show UI", () => ShowUI(global));
        //}
        private static void ShowUI(Vector3 global)
        {
            var entity = Net.Client.Instance.Map.GetBlockEntity(global) as Entity;
            var window = new WindowEntityInterface(entity, "Chest", () => global);
            var ui = new ContainerUI().Refresh(global, entity);
            window.Client.Controls.Add(ui);
            window.Show();
        }
        //public override void Place(IMap map, Vector3 global, byte data, int variation, int orientation)
        //{
        //    base.Place(map, global, data, variation, orientation);
        //    map.AddBlockEntity(global, this.GetBlockEntity());
        //}
        //public override void Remove(IMap map, Vector3 global)
        //{
        //    base.Remove(map, global);
        //    map.RemoveBlockEntity(global);
        //}
    }
}
