using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Workbench;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Items;
using Start_a_Town_.GameModes;
using Start_a_Town_.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.AI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Crafting;
using Start_a_Town_.Tokens;

namespace Start_a_Town_.Blocks
{
    class BlockWorkbench : BlockWithEntity//Workstation// Block, IBlockWorkstation
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

            public override void FromCraftingReagent(GameObject reagent)
            {
                //this.Material = reagent.GetComponent<MaterialsComponent>().Parts["Body"].Material;
                this.Material = reagent.Body.Material;
            }
        }
        //public override bool IsDeconstructable => true;
        //public override AILabor Labor { get { return AILabor.Craftsman; } }
        public override IBlockState BlockState
        { get { return new State(); } }
        public override void DrawUI(SpriteBatch sb, Vector2 pos, byte state)
        {
            var token = this.Orientations.First();
            sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, this.BlockState.GetTint(state));
        }
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Database[blockdata];
        }
        
        //AtlasDepthNormals.Node.Token[] Orientations = new AtlasDepthNormals.Node.Token[4];
        AtlasDepthNormals.Node.Token[] Orientations = Block.TexturesCounter;
        public BlockWorkbench() :
            base(Block.Types.Workbench, opaque: false, solid: true)
        {
            //this.Variations.Add(Block.Atlas.Load("blocks/furniture/stool", Map.BlockDepthMap, Block.NormalMap));
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();

            //this.Orientations[0] = Block.Atlas.Load("blocks/counters/counter1");
            //this.Orientations[1] = Block.Atlas.Load("blocks/counters/counter4");
            //this.Orientations[2] = Block.Atlas.Load("blocks/counters/counter3");
            //this.Orientations[3] = Block.Atlas.Load("blocks/counters/counter2");

            //this.Orientations[0] = Block.Atlas.Load(Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/counters/counter1").ToGrayscale(), "counter1grayscale");
            //this.Orientations[1] = Block.Atlas.Load(Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/counters/counter4").ToGrayscale(), "counter4grayscale");
            //this.Orientations[2] = Block.Atlas.Load(Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/counters/counter3").ToGrayscale(), "counter3grayscale");
            //this.Orientations[3] = Block.Atlas.Load(Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/counters/counter2").ToGrayscale(), "counter2grayscale");

            this.Variations.Add(this.Orientations.First());
            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfMaterialType(MaterialType.Wood),
                        //Reaction.Reagent.IsOfSubType(ItemSubType.Planks),
                        Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            Towns.Constructions.ConstructionsManager.Production.Add(this.Recipe);

        }

        public override BlockRecipe GetRecipe()
        {
            return this.Recipe;
        }
        public override IEnumerable<byte> GetCraftingVariations()
        {
            var vars = (from mat in Material.Database.Values
                        where mat.Type == MaterialType.Wood
                        select (byte)mat.ID);
            return vars;
        }
        public override Vector4 GetColorVector(byte data)
        {
            var mat = Material.Database[data];
            var c = mat.ColorVector;
            return c;
        }

        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            return new List<Interaction>(){
                //new InteractionCraft(),
                new InteractionCraftNew(),
                new InteractionAddMaterial()//map.GetBlockEntity(global) as Entity)
            };
        }

        public override BlockEntity CreateBlockEntity()
        {
            return new BlockWorkbenchEntity();
            //return new BlockEntity().AddComp(new BlockEntityCompWorkstation(IsWorkstation.Types.Workbench));
        }



        //public class InteractionInsert : Interaction
        //{
        //    public InteractionInsert()
        //    {
        //        this.Name = "Insert";
        //    }
        //    public override void Perform(GameObject a, TargetArgs t)
        //    {
        //        var entity = a.Map.GetBlockEntity(t.Global) as BlockWorkbenchEntity;
        //        if (entity == null)
        //            throw new ArgumentException();
        //        var hauled = a.GetComponent<HaulComponent>().GetSlot();//.Slot;
        //        if (hauled == null)
        //            return;
        //        entity.Insert(hauled);
        //    }

        //    public override object Clone()
        //    {
        //        return new InteractionInsert();
        //    }
        //}
    }
    
}
