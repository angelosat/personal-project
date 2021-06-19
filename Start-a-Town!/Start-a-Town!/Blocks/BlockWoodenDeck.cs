using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Graphics;
using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    class BlockWoodenDeck : Block
    {
        public class State : BlockState
        {
            public Material Material { get; set; }
            public override byte Data
            {
                get
                {
                    return (byte)this.Material.ID;
                }
            }
            public override Color GetTint(byte d)
            { return Material.Database[d].Color; }
            public override string GetName(byte d)
            {
                return Material.Database[d].Name;
            }
            
            public State()
            {

            }
            public State(Material material)
            {
                this.Material = material;
            }
            static public void Read(byte data, out Material material)
            {
                material = Material.Database[data];
            }
            //public void Apply(Map map, Vector3 global)
            //{
            //    Cell cell = global.GetCell(map);
            //    cell.BlockData = (byte)this.Material.ID;
            //}
            //public void Apply(ref byte data)
            //{
            //    data = (byte)this.Material.ID;
            //}
            //public void Apply(Block.Data data)
            //{
            //    data.Value = (byte)this.Material.ID;
            //}
            public override void FromCraftingReagent(GameObject reagent)
            {
                //this.Material = reagent.GetComponent<MaterialsComponent>().Parts["Body"].Material;
                this.Material = reagent.Body.Sprite.Material;
            }
        }

        public override Particles.ParticleEmitterSphere GetEmitter()
        {
            return base.GetDustEmitter();
        }
        
        

        public override void DrawUI(SpriteBatch sb, Vector2 pos)
        {
            var token = this.Variations.First();
            sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, Color.White);
        }
        public override void DrawUI(SpriteBatch sb, Vector2 pos, byte state)
        {
            var token = this.GrayScale;
            sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, this.BlockState.GetTint(state));
        }

        public static IBlockState GetState() { return new State(); }
        public override IBlockState BlockState
        { get { return new State(); } }

        public override bool IsDeconstructible => true;
        AtlasDepthNormals.Node.Token GrayScale;
        //protected override void OnDeconstruct(GameObject actor, Vector3 global)
        //{
        //    actor.Net.PopLoot(ItemFactory.CreateFrom(RawMaterialDef.Planks, Material.GetMaterial(actor.Map.GetData(global))).SetStackSize(this.Ingredient.Amount / 2), global, Vector3.Zero);
        //}
        public BlockWoodenDeck()
            : base(Block.Types.WoodenDeck, GameObject.Types.WoodenDeck, 0, 1, true, true)
        {
            //this.Reagents.Add(GameObject.Types.WoodenPlank);
            //this.Reagents.Add(new Reaction.Reagent("Base", Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks)));
            this.Reagents.Add(new Reaction.Reagent("Base", Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks), Reaction.Reagent.IsOfMaterial(MaterialDefOf.LightWood)));
            //this.Material = Material.LightWood;
            //this.LootTable = new LootTable(new Loot(GameObject.Types.WoodenPlank, 1, 1));

            //this.GrayScale = Block.Atlas.Load("blocks/woodendeckgrayscale");
            this.GrayScale = Block.Atlas.Load("blocks/woodvertical");
            //this.AssetNames = "woodendeck";
            this.Variations.Add(this.GrayScale);

            //this.Reagent = new ItemDefAmount(RawMaterialDef.Planks, 4);
            this.Ingredient = new Ingredient(RawMaterialDef.Planks, null, null, 1);// 4);
            //this.GrayScale = Block.Atlas.Load("blocks/woodverticalgray");
            //this.AssetNames = "woodvertical";

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        //Reaction.Reagent.IsOfMaterialType(MaterialType.Wood), 
                        Reaction.Reagent.IsOfSubType(ItemSubType.Planks)
                        //,
                        //Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks)
                        )),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building) { WorkAmount = 2 };// 20 };
            Towns.Constructions.ConstructionsManager.Walls.Add(this.Recipe);

            //this.LootTable =
            //    new LootTable(
            //        //new Loot(() => Components.Materials.MaterialType.RawMaterial.Create(this.Material))
            //        //new Loot(() => Components.Materials.MaterialType.Planks.CreateFrom(this.Material))
            //        new Loot(() => Components.Materials.MaterialType.Planks.Templates[this.Material])
            //        );
        }
        public override BlockRecipe GetRecipe()
        {
            return this.Recipe;
        }
        public override LootTable GetLootTable(byte data)
        {
            var table =
                new LootTable(
                    //new Loot(() => Components.Materials.MaterialType.RawMaterial.Create(this.Material))
                    //new Loot(() => Components.Materials.MaterialType.Planks.CreateFrom(this.Material))
                    //new Loot(() => Components.Materials.MaterialType.RawMaterial.Planks.CreateFrom(this.GetMaterial(data)))
                    new Loot(() => ItemFactory.CreateFrom(RawMaterialDef.Planks, this.GetMaterial(data)))

                    );
            return table;
        }
        public override IEnumerable<byte> GetCraftingVariations()
        {
            return (from mat in Material.Database.Values
                        where mat.Type == MaterialType.Wood
                        select (byte)mat.ID);

        }
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Database[blockdata];
        }

        public override void OnMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Chop:
                    this.Break(parent.Map, parent.Global);
                    //e.Network.PopLoot(GameObject.Create(GameObject.Types.WoodenPlank), parent.Global, parent.Velocity);
                    //e.Network.Despawn(parent);
                    //e.Network.DisposeObject(parent);
                    return;

                default:
                    break;
            }
        }
        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            var list = new List<Interaction>();
            list.Add(new InteractionChopping());
            return list;
        }
        
        public override void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, Color tint, float zoom, float depth, Cell cell)
        {
            Rectangle sourceRect = this.GrayScale.Rectangle;
            Material mat;
            State.Read(cell.BlockData, out mat);
            tint = tint.Multiply(mat.Color);
            sb.DrawBlock(Block.Atlas.Texture, screenPos, this.GrayScale.Rectangle, zoom, tint, sunlight, blocklight, depth);
        }

        public override void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, float zoom, float depth, Cell cell)
        {
            this.Draw(sb, screenPos, sunlight, blocklight, Color.White, zoom, depth, cell);
            //Rectangle sourceRect = this.GrayScale.Rectangle;
            //Material mat;
            //State.Read(cell.BlockData, out mat);
            //Color tint = mat.TextColor;
            //sb.DrawBlock(Block.Atlas.Texture, screenPos, this.GrayScale.Rectangle, zoom, tint, sunlight, blocklight, depth);
        }

        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, float zoom, float depth, Cell cell)
        {
            //sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[cell.Variation], zoom, Color.White, sunlight, blocklight, depth);
            Rectangle sourceRect = this.GrayScale.Rectangle;
            Material mat;
            State.Read(cell.BlockData, out mat);
            Color tint = mat.Color;
            //sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[cell.Variation], zoom, tint, sunlight, blocklight, depth);
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.GrayScale, zoom, tint, sunlight, blocklight, depth);
            //base.Draw(sb, screenBounds, sunlight, blocklight, tint, zoom, depth, cell);
        }
        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            Material mat;
            State.Read(cell.BlockData, out mat);
            Color finaltint = mat.Color;
            //sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.GrayScale, zoom, finaltint, sunlight, blocklight, depth);
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.GrayScale, zoom, fog, finaltint.Multiply(tint), sunlight, blocklight, depth);
            //base.Draw(sb, screenBounds, sunlight, blocklight, fog, finaltint.Multiply(tint), zoom, depth, cell);
        }
        public override void Draw(MySpriteBatch sb, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            Material mat;
            State.Read(cell.BlockData, out mat);
            Color finaltint = mat.Color;
            finaltint = finaltint.Multiply(tint) * .66f;
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.GrayScale, zoom, fog, finaltint.Multiply(tint), sunlight, blocklight, depth, this);
        }

        //public override void Draw(Camera cam, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, Cell cell)
        //{
        //    Material mat;
        //    State.Read(cell.BlockData, out mat);
        //    Color finaltint = mat.Color;
        //    finaltint = finaltint.Multiply(tint) * .66f;
        //    //cam.SpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.GrayScale, cam.Zoom, fog, finaltint.Multiply(tint), sunlight, blocklight, depth);
        //    cam.SpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.GrayScale, cam.Zoom, fog, finaltint, sunlight, blocklight, depth);

        //}

        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, byte data)
        {
            Material mat;
            State.Read(data, out mat);
            Color finaltint = mat.Color;
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.GrayScale, zoom, fog, finaltint.Multiply(tint), sunlight, blocklight, depth);
        }

        public override Color GetColor(byte data)
        {
            Material mat;
            State.Read(data, out mat);
            var c = mat.Color * .66f;
            c.A = 1;
            return c;
        }
        public override Vector4 GetColorVector(byte data)
        {
            Material mat;
            State.Read(data, out mat);
            var c = mat.ColorVector;
            //c.W = .8f;
            return c;
        }
        //protected override AtlasDepthNormals.Node.Token GetAtlasNodeToken(byte data)
        //{
        //    return this.GrayScale;
        //}
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraOrientation, byte data)
        {
            return this.GrayScale;
        }

        //public override void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, Color tint, Color fog, float zoom, float depth, byte data)
        //{
        //    Material mat;
        //    State.Read(data, out mat);
        //    Color finaltint = mat.Color;
        //    var superFinalTint = finaltint.Multiply(tint);
        //    sb.DrawBlock(Block.Atlas.Texture, screenPos, this.GrayScale, zoom, fog, superFinalTint, sunlight, blocklight, depth);
        //}

        //public override void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, float zoom, float depth, Cell cell)
        //{
        //    Rectangle sourceRect = this.GrayScale.Rectangle;
        //    Material mat;
        //    State.Read(cell.BlockData, out mat);
        //    Color tint = mat.TextColor;
        //    sb.DrawBlock(Block.Atlas.Texture, screenPos, this.GrayScale.Rectangle, zoom, tint, sunlight, blocklight, depth);
        //}
    }
}
