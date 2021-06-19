using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Workbench;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Items;
using Start_a_Town_.GameModes;
using Start_a_Town_.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.AI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Crafting;

namespace Start_a_Town_.Blocks
{
    partial class BlockWorkbench : BlockWorkstation// Block, IBlockWorkstation
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
            { return Material.Templates[d].Color; }
            public override string GetName(byte d)
            {
                return Material.Templates[d].Name;
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
                material = Material.Templates[data];
            }
            
            public override void FromMaterial(GameObject reagent)
            {
                //this.Material = reagent.GetComponent<MaterialsComponent>().Parts["Body"].Material;
                this.Material = reagent.Body.Material;
            }
        }

        public override AILabor Labor { get { return AILabor.Craftsman; } }

        public override IBlockState BlockState
        { get { return new State(); } }
        public override void DrawUI(SpriteBatch sb, Vector2 pos, byte state)
        {
            var token = this.Orientations.First();
            sb.Draw(token.Atlas.Texture, pos - new Vector2(token.Rectangle.Width, token.Rectangle.Height) * 0.5f, token.Rectangle, this.BlockState.GetTint(state));
        }
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Templates[blockdata];
        }
        AtlasDepthNormals.Node.Token[] Orientations = new AtlasDepthNormals.Node.Token[4];

        public BlockWorkbench():base(Block.Types.Workbench)
        {
            //this.Variations.Add(Block.Atlas.Load("blocks/furniture/stool", Map.BlockDepthMap, Block.NormalMap));

            //this.Orientations[0] = Block.Atlas.Load("blocks/counters/counter1");
            //this.Orientations[1] = Block.Atlas.Load("blocks/counters/counter4");
            //this.Orientations[2] = Block.Atlas.Load("blocks/counters/counter3");
            //this.Orientations[3] = Block.Atlas.Load("blocks/counters/counter2");

            this.Orientations[0] = Block.Atlas.Load(Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/counters/counter1").ToGrayscale(), "counter1grayscale");
            this.Orientations[1] = Block.Atlas.Load(Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/counters/counter4").ToGrayscale(), "counter4grayscale");
            this.Orientations[2] = Block.Atlas.Load(Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/counters/counter3").ToGrayscale(), "counter3grayscale");
            this.Orientations[3] = Block.Atlas.Load(Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/counters/counter2").ToGrayscale(), "counter2grayscale");

            this.Variations.Add(this.Orientations.First());
            this.Recipe = new BlockConstruction(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfMaterialType(MaterialType.Wood), 
                        //Reaction.Reagent.IsOfSubType(ItemSubType.Planks),
                        Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
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
                        where mat.Type == MaterialType.Wood
                        select (byte)mat.ID).ToList();
            return vars;
        }
        public override Vector4 GetColorVector(byte data)
        {
            var mat = Components.Materials.Material.Templates[data];
            var c = mat.ColorVector;
            return c;
        }
        
        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            return new List<Interaction>(){
                new InteractionCraft(),
                new InteractionAddMaterial()//map.GetBlockEntity(global) as Entity)
            };
        }
        
        void Activate(GameObject a, TargetArgs t)
        {
            var entity = a.Map.GetBlockEntity(t.Global) as Entity;
            if (entity == null)
                throw new ArgumentException();
            var hauled = a.GetComponent<HaulComponent>().GetSlot();//.Slot;
            //if (hauled == null)
            //    return;
            entity.Insert(hauled);
            if (a.Net is Net.Client)
            {
                ShowUI(t.Global);
            }
        }
        private static void ShowUI(Vector3 global)
        {
            var entity = Net.Client.Instance.Map.GetBlockEntity(global) as Entity;
            var window = new WindowEntityInterface(entity, "Workbench", () => global);
            //var ui = new WorkbenchInterface(entity, global);//.Refresh(global, entity);
            var ui = new WorkstationUI(global, entity);//.Refresh(global, entity);

            window.Client.Controls.Add(ui);

            window.Show();
        }

        //public override BlockEntity GetBlockEntity()
        public override BlockEntityWorkstation GetWorkstationBlockEntity()
        {
            return new Entity();
        }

        public class InteractionActivate : Interaction
        {
            public InteractionActivate()
            {
                this.Name = "Use";
            }
            public override void Perform(GameObject a, TargetArgs t)
            {
                if (a.Net is Net.Client)
                {
                    ShowUI(t.Global);
                }
            }

            public override object Clone()
            {
                return new InteractionActivate();
            }
        }
        public class InteractionInsert : Interaction
        {
            public InteractionInsert()
            {
                this.Name = "Insert";
            }
            public override void Perform(GameObject a, TargetArgs t)
            {
                var entity = a.Map.GetBlockEntity(t.Global) as Entity;
                if (entity == null)
                    throw new ArgumentException();
                var hauled = a.GetComponent<HaulComponent>().GetSlot();//.Slot;
                if (hauled == null)
                    return;
                entity.Insert(hauled);
            }

            public override object Clone()
            {
                return new InteractionInsert();
            }
        }
    }
}
