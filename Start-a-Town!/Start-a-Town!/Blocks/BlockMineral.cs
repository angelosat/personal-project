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
            { return MaterialDef.Registry[d].Color; }
            public string GetName(byte d)
            {
                return MaterialDef.Registry[d].Name;
            }
            public MaterialDef Material;
            public State()
            {

            }
            public void Apply(MapBase map, Vector3 global)
            {
                map.SetBlockData(global, (byte)this.Material.ID);
            }
            public State(MaterialDef material)
            {
                this.Material = material;
            }
            public State(byte data)
            {
                this.Material = MaterialDef.Registry[data];
            }
            public State(MapBase map, Vector3 global)
            {
                this.Material = MaterialDef.Registry[global.GetData(map)];
            }
            static public void Get(byte data, out MaterialDef material)
            {
                material = MaterialDef.Registry[data];
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
        public override IEnumerable<byte> GetEditorVariations()
        {
            var vars = (from mat in MaterialDef.Registry.Values
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

        public override IBlockState BlockState => new State();
            

        public BlockMineral()
            : base("Mineral", 0, 1, true, true)
        {
            this.LoadVariations("stone5height19");
        }
        
        //public override MaterialDef GetMaterial(byte data)
        //{
        //    return MaterialDef.Registry[data];
        //}

        public override byte ParseData(string data)
        {
            var mat = MaterialDef.Registry.Values.FirstOrDefault(m => string.Equals(m.Name, data, StringComparison.OrdinalIgnoreCase));
            if (mat == null)
                return (byte)MaterialDefOf.Stone.ID;
            if (mat.Type != MaterialDefOf.Stone.Type)
                return (byte)MaterialDefOf.Stone.ID;
            return (byte)mat.ID;
        }

        public override List<Interaction> GetAvailableTasks(MapBase map, IntVec3 global)
        {
            return new List<Interaction>(){
                new InteractionMining()
            };
        }

        public override Color GetColor(byte data)
        {
            var mat = MaterialDef.Registry[data];
            var c = mat.Color;
            c.A = (byte)(255 * mat.Type.Shininess);
            return c;
        }
        public override Vector4 GetColorVector(byte data)
        {
            var mat = MaterialDef.Registry[data];
            var c = mat.ColorVector;
            return c;
        }

        public override void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, Color tint, float zoom, float depth, Cell cell)
        {
            base.Draw(sb, screenPos, sunlight, blocklight, MaterialDef.Registry[cell.BlockData].Color.Multiply(tint), zoom, depth, cell);
        }

        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            base.Draw(sb, screenBounds, sunlight, blocklight, fog, MaterialDef.Registry[cell.BlockData].Color.Multiply(tint), zoom, depth, cell);
        }
        public override void Draw(MySpriteBatch sb, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            base.Draw(sb, screenBounds, sunlight, blocklight, fog, MaterialDef.Registry[cell.BlockData].Color.Multiply(tint), zoom, depth, cell);
        }
    }
}
