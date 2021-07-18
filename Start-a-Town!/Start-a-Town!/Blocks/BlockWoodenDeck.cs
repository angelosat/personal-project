using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Blocks;
using Start_a_Town_.Graphics;
using System.Collections.Generic;
using System.Linq;

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
            { return Material.Registry[d].Color; }
            public override string GetName(byte d)
            {
                return Material.Registry[d].Name;
            }

            public State()
            {

            }
            public State(Material material)
            {
                this.Material = material;
            }
            public static void Read(byte data, out Material material)
            {
                material = Material.Registry[data];
            }

            public override void FromCraftingReagent(GameObject reagent)
            {
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

        readonly AtlasDepthNormals.Node.Token GrayScale;

        public BlockWoodenDeck()
            : base(Block.Types.WoodenDeck, 0, 1, true, true)
        {
            this.GrayScale = Block.Atlas.Load("blocks/woodvertical");
            this.Variations.Add(this.GrayScale);
            this.Ingredient = new Ingredient(RawMaterialDef.Planks, null, null, 1);// 4);
            this.BuildProperties.WorkAmount = 2;
            this.ToggleConstructionCategory(ConstructionsManager.Walls, true);
        }

        public override LootTable GetLootTable(byte data)
        {
            var table =
                new LootTable(
                    new Loot(() => ItemFactory.CreateFrom(RawMaterialDef.Planks, this.GetMaterial(data)))
                    );
            return table;
        }
        public override IEnumerable<byte> GetEditorVariations()
        {
            return (from mat in Material.Registry.Values
                    where mat.Type == MaterialType.Wood
                    select (byte)mat.ID);

        }
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Registry[blockdata];
        }

        public override List<Interaction> GetAvailableTasks(MapBase map, IntVec3 global)
        {
            var list = new List<Interaction>();
            list.Add(new InteractionChopping());
            return list;
        }

        public override void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, Color tint, float zoom, float depth, Cell cell)
        {
            Rectangle sourceRect = this.GrayScale.Rectangle;
            State.Read(cell.BlockData, out var mat);
            tint = tint.Multiply(mat.Color);
            sb.DrawBlock(Block.Atlas.Texture, screenPos, this.GrayScale.Rectangle, zoom, tint, sunlight, blocklight, depth);
        }

        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            State.Read(cell.BlockData, out var mat);
            Color finaltint = mat.Color;
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.GrayScale, zoom, fog, finaltint.Multiply(tint), sunlight, blocklight, depth);
        }
        public override void Draw(MySpriteBatch sb, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            State.Read(cell.BlockData, out var mat);
            Color finaltint = mat.Color;
            finaltint = finaltint.Multiply(tint) * .66f;
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.GrayScale, zoom, fog, finaltint.Multiply(tint), sunlight, blocklight, depth, this);
        }

        public override Color GetColor(byte data)
        {
            State.Read(data, out var mat);
            var c = mat.Color * .66f;
            c.A = 1;
            return c;
        }
        public override Vector4 GetColorVector(byte data)
        {
            State.Read(data, out var mat);
            var c = mat.ColorVector;
            return c;
        }

        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraOrientation, byte data)
        {
            return this.GrayScale;
        }
    }
}
