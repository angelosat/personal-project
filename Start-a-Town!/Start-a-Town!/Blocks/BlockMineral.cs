using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Particles;

namespace Start_a_Town_
{
    class BlockMineral : Block
    {
        class State : IBlockState
        {
            public Color GetTint(byte d)
            { return Material.Database[d].Color; }
            public string GetName(byte d)
            {
                return Material.Database[d].Name;
            }
            public Material Material;
            public State()
            {

            }
            public void Apply(IMap map, Vector3 global)
            {
                map.SetData(global, (byte)this.Material.ID);
            }
            public State(Material material)
            {
                this.Material = material;
            }
            public State(byte data)
            {
                this.Material = Material.Database[data];
            }
            public State(IMap map, Vector3 global)
            {
                this.Material = Material.Database[global.GetData(map)];
            }
            static public void Get(byte data, out Material material)
            {
                material = Material.Database[data];
            }
            public void Apply(ref byte data)
            {
                data = (byte)this.Material.ID;
            }
            public void Apply(Block.Data data)
            {
                data.Value = (byte)this.Material.ID;
            }
        }
        public override bool IsMinable => true;

        public override ParticleEmitterSphere GetEmitter()
        {
            return base.GetDustEmitter();
        }
        public override IEnumerable<byte> GetCraftingVariations()
        {
            var vars = (from mat in Material.Database.Values
                        where mat.Type == MaterialType.Stone || mat.Type == MaterialType.Metal 
                        select (byte)mat.ID);
            return vars;
        }
        public override void DrawUI(SpriteBatch sb, Vector2 pos)
        {
            var token = this.Variations.First();
            sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, Color.White);
        }
        public override void DrawUI(SpriteBatch sb, Vector2 pos, byte state)
        {
            var token = this.Variations.First();
            sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, this.BlockState.GetTint(state));
        }

        public override IBlockState BlockState
        {
            get
            {
                return new State();
            }
        }

        public BlockMineral()
            : base(Block.Types.Mineral, GameObject.Types.CobblestoneItem, 0, 1, true, true)
        {
            this.AssetNames = "stone5height19";
        }

        public override Material GetMaterial(IMap map, Vector3 global)
        {
            var data = map.GetData(global);
            return Material.Database[data];
        }
        public override Material GetMaterial(byte data)
        {
            return Material.Database[data];
        }


        //public override void Break(IMap map, Vector3 global)
        //{
        //    var net = map.Net;
        //    var state = new State(map, global);
        //    var material = state.Material;
        //    var rawmaterialentity = material.ProcessingChain.First().IDType;
        //    var loottable = new LootTable(new Loot(rawmaterialentity, 0.75f, 4));
        //    net.PopLoot(loottable, global, Vector3.Zero);
        //    net.SetBlock(global, Block.Types.Air);
        //}

        public override byte ParseData(string data)
        {
            var mat = Material.Database.Values.FirstOrDefault(m => string.Equals(m.Name, data, StringComparison.OrdinalIgnoreCase));
            if (mat == null)
                return (byte)MaterialDefOf.Stone.ID;
            if (mat.Type != MaterialDefOf.Stone.Type)
                return (byte)MaterialDefOf.Stone.ID;
            return (byte)mat.ID;
        }

        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            return new List<Interaction>(){
                new InteractionMining()
            };
        }

        public override Color GetColor(byte data)
        {
            var mat = Material.Database[data];
            var c = mat.Color;
            c.A = (byte)(255 * mat.Type.Shininess);
            return c;
        }
        public override Vector4 GetColorVector(byte data)
        {
            var mat = Material.Database[data];
            var c = mat.ColorVector;
            return c;
        }

        public override void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, Color tint, float zoom, float depth, Cell cell)
        {
            base.Draw(sb, screenPos, sunlight, blocklight, Material.Database[cell.BlockData].Color.Multiply(tint), zoom, depth, cell);
        }

        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            base.Draw(sb, screenBounds, sunlight, blocklight, fog, Material.Database[cell.BlockData].Color.Multiply(tint), zoom, depth, cell);
        }
        public override void Draw(MySpriteBatch sb, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            base.Draw(sb, screenBounds, sunlight, blocklight, fog, Material.Database[cell.BlockData].Color.Multiply(tint), zoom, depth, cell);
        }
    }
}
