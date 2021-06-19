using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;

namespace Start_a_Town_
{
    class BlockMineral : Block
    {
        class State : IBlockState
        {
            public Color GetTint(byte d)
            { return Material.Templates[d].Color; }
            public string GetName(byte d)
            {
                return Material.Templates[d].Name;
            }
            public Material Material;
            public State()
            {

            }
            public void Apply(IMap map, Vector3 global)
            {
                //global.SetData(map, (byte)this.Material.ID);
                map.SetData(global, (byte)this.Material.ID);
            }
            public State(Material material)
            {
                this.Material = material;
            }
            public State(byte data)
            {
                this.Material = Material.Templates[data];
            }
            public State(IMap map, Vector3 global)
            {
                this.Material = Material.Templates[global.GetData(map)];
            }
            static public void Get(byte data, out Material material)
            {
                material = Material.Templates[data];
            }
            public void Apply(ref byte data)
            {
                data = (byte)this.Material.ID;
            }
            public void Apply(Block.Data data)
            {
                data.Value = (byte)this.Material.ID;
            }
            public void FromMaterial(GameObject reagent)
            {
                this.Material = reagent.GetComponent<MaterialComponent>().Material;
            }
        }
        public override Components.Particles.ParticleEmitterSphere GetEmitter()
        {
            return base.GetDustEmitter();
        }
        public override List<byte> GetVariations()
        {
            var vars = (from mat in Material.Templates.Values
                        where mat.Type == MaterialType.Mineral || mat.Type == MaterialType.Metal 
                        select (byte)mat.ID).ToList();
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
            //this.Material = Material.Stone;
            //this.MaterialType = MaterialType.Mineral;


            //this.LootTable = new LootTable(new Loot(GameObject.Types.Stone, 0.75f, 4));
            //this.LootTable = new LootTable(new Loot(MaterialType.Ore.ID, 0.75f, 4));
           
            //this.Reagents.Add(Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks));//   GameObject.Types.Stone);
            //this.Reagents.Add(new Reaction.Reagent("Base", Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks), Reaction.Reagent.IsOfType(Material.Stone)));
            this.AssetNames = "stone5height19";// "stone5";// "stone1, stone2, stone3, stone4";//sand1";//stone1, stone2, stone3, stone4";
        }

        public override Material GetMaterial(IMap map, Vector3 global)
        {
            var data = map.GetData(global);
            return Material.Templates[data];
        }
        public override Material GetMaterial(byte data)
        {
            return Material.Templates[data];
        }
        public override void OnMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Mine:
                    this.Break(parent.Map, parent.Global);
                    //e.Network.PopLoot(GameObject.Create(GameObject.Types.WoodenPlank), parent.Global, parent.Velocity);
                    //e.Network.Despawn(parent);
                    //e.Network.DisposeObject(parent);
                    return;

                default:
                    break;
            }
        }

        public override void Break(IMap map, Vector3 global)
        {
            //this.LootTable = new LootTable(new Loot(MaterialType.Ore.ID, 0.75f, 4));
            var net = map.Net;
            var state = new State(map, global);
            var material = state.Material;
            var rawmaterialentity = material.ProcessingChain.First().ID;
            var loottable = new LootTable(new Loot(rawmaterialentity, 0.75f, 4));
            net.PopLoot(loottable, global, Vector3.Zero);
            net.SetBlock(global, Block.Types.Air);
        }

        public override byte ParseData(string data)
        {
            var mat = Material.Templates.Values.FirstOrDefault(m => string.Equals(m.Name, data, StringComparison.OrdinalIgnoreCase));
            if (mat == null)
                return (byte)Material.Stone.ID;
            if (mat.Type != Material.Stone.Type)
                return (byte)Material.Stone.ID;
            return (byte)mat.ID;
        }

        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            return new List<Interaction>(){
                new Mining()
            };
        }
        //public override ContextAction GetRightClickAction(Vector3 global)
        //{
        //    return new ContextAction(() => "Mine", () => { Net.Client.PlayerInteract(new TargetArgs(global)); });
        //}

        //protected override Color GetColor(byte data)
        //{
        //    var c = Components.Materials.Material.Templates[data].Color;// *.66f;
        //    return c;
        //}

        public override Color GetColor(byte data)
        {
            //var c = Components.Materials.Material.Templates[data].Color;// *.66f;
            //return c;
            var mat = Components.Materials.Material.Templates[data];
            var c = mat.Color;// *.66f;
            c.A = (byte)(255 * mat.Type.Shininess);
            return c;
        }
        public override Vector4 GetColorVector(byte data)
        {
            var mat = Components.Materials.Material.Templates[data];
            //var c = mat.Color;// *.66f;
            //c.A = (byte)(255 * mat.Type.Shininess);
            var c = mat.ColorVector;
            //c.W = mat.Type.Shininess;
            return c;
        }

        public override void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, float zoom, float depth, Cell cell)
        {
            //base.Draw(sb, screenPos, sunlight, blocklight, Components.Materials.Material.Templates[cell.BlockData].TextColor, zoom, depth, cell);
            this.Draw(sb, screenPos, sunlight, blocklight, Color.White, zoom, depth, cell);
        }

        public override void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, Color tint, float zoom, float depth, Cell cell)
        {
            base.Draw(sb, screenPos, sunlight, blocklight, Components.Materials.Material.Templates[cell.BlockData].Color.Multiply(tint), zoom, depth, cell);
        }

        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, float zoom, float depth, Cell cell)
        {
            base.Draw(sb, screenBounds, sunlight, blocklight, Components.Materials.Material.Templates[cell.BlockData].Color.Multiply(Color.White), zoom, depth, cell);
        }

        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            base.Draw(sb, screenBounds, sunlight, blocklight, fog, Components.Materials.Material.Templates[cell.BlockData].Color.Multiply(tint), zoom, depth, cell);
        }
        public override void Draw(MySpriteBatch sb, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            base.Draw(sb, screenBounds, sunlight, blocklight, fog, Components.Materials.Material.Templates[cell.BlockData].Color.Multiply(tint), zoom, depth, cell);
        }


        //public override void Draw(Camera cam, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, Cell cell)
        //{
        //    var color = this.GetColor(cell.BlockData);
        //    //base.Draw(cam.SpriteBatch, screenBounds, sunlight, blocklight, fog, Components.Materials.Material.Templates[cell.BlockData].Color.Multiply(tint), cam.Zoom, depth, cell);
        //    base.Draw(cam.SpriteBatch, screenBounds, sunlight, blocklight, fog, color.Multiply(tint), cam.Zoom, depth, cell);

        //}


    }
}
