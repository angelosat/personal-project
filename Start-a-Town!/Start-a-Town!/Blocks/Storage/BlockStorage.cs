using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    partial class BlockStorage : Block
    {
        static Texture2D ChestNormalMap = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/furniture/chestnormal");
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Registry[blockdata];
        }
        public BlockStorage()
            : base(Block.Types.Bin, opaque: false)
        {
            var tex = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/furniture/chest").ToGrayscale();
            this.Variations.Add(Block.Atlas.Load("chestgrayscale", tex, MapBase.BlockDepthMap, ChestNormalMap));
           
            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent()),
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
            var vars = (from mat in Material.Registry.Values
                        where mat.Type == MaterialType.Wood || mat.Type == MaterialType.Metal
                        select (byte)mat.ID);
            return vars;
        }
        public override BlockEntity CreateBlockEntity()
        {
            return new BlockStorageEntity();
        }
        public override Vector4 GetColorVector(byte data)
        {
            var mat = Material.Registry[data];
            var c = mat.ColorVector;
            return c;
        }

        public override void OnDrop(GameObject actor, GameObject dropped, TargetArgs target, int amount = -1)
        {
            var binEntity = actor.Map.GetBlockEntity(target.Global) as BlockStorageEntity;
            binEntity.Insert(dropped);
            actor.ClearCarried();
        }

        StorageContentsUI ContentsUI = new StorageContentsUI();
        internal override void Select(UISelectedInfo uISelectedInfo, MapBase map, Vector3 vector3)
        {
            uISelectedInfo.AddTabAction("Contents", () => ShowContents(map, vector3));
        }

        private void ShowContents(MapBase map, Vector3 vector3)
        {
            var ent = map.GetBlockEntity(vector3) as BlockStorageEntity;
            this.ContentsUI.Refresh(ent);
            var win = this.ContentsUI.GetWindow();
            if (win == null)
                this.ContentsUI.ToWindow("Contents").Show();
            else if (!ContentsUI.IsOpen)
                win.Show();
        }
    }
}
