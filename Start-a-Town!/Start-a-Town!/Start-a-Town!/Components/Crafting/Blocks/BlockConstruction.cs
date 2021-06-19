using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Skills;

namespace Start_a_Town_.Components.Crafting
{
    public partial class BlockConstruction
    {
        static int _IDSequence = 0;
        public static int IDSequence { get { return _IDSequence++; } }
        const int BlockConstructionIDRange = 10000;

        static Dictionary<int, BlockConstruction> _Dictionary;
        public static Dictionary<int, BlockConstruction> Dictionary
        {
            get
            {
                if (_Dictionary.IsNull())
                    _Dictionary = new Dictionary<int, BlockConstruction>();
                return _Dictionary;
            }
        }

        public int ID { get; set; }
        public string Name { get; set; }
        //public GameObject.Types Building { get; set; }
        public List<Reaction.Reagent> Reagents { get; set; }
        public BlockConstruction.Product BlockProduct { get; set; }
        public Skill Skill { get; set; }

        public BlockConstruction(List<Reaction.Reagent> reagents, BlockConstruction.Product product, Skill skill = null)
        {
            this.ID = IDSequence;
            this.Name = product.Block.ToString();// Block.Registry[product.Type].Nam;
            this.Skill = skill;
            //this.Building = building;
            this.Reagents = reagents;
            this.BlockProduct = product;
            //GameObject.Objects.Add(this.ToObject());

            // don't register automatically? have blocks override a GetConstruction method to provide their construction recipe? (or return null if they don't have one)
            //Dictionary[ID] = this;
        }

        public List<BlockConstruction.ProductMaterialPair> GetVariants()
        {
            List<BlockConstruction.ProductMaterialPair> list = new List<ProductMaterialPair>();
            foreach (var reagent in this.Reagents)
            {
                //var validMats = (from mat in MaterialComponent.Registry let obj = GameObject.Objects[mat] where reagent.Pass(obj) select obj).ToList();
                var validMats = (from mat in ReagentComponent.Registry let obj = GameObject.Objects[mat] where reagent.Filter(obj) select obj).ToList();
                foreach (var mat in validMats)
                {
                    byte data = 0;
                    //BlockConstruction.Product.Modifier mod = this.BlockProduct.Modifiers.First();
                    //if (reagent.Name == mod.LocalMaterialName)
                    //    mod.Apply(mat, ref data);
                    IBlockState state = this.BlockProduct.Block.BlockState;
                    state.FromMaterial(mat);
                    state.Apply(ref data);
                    list.Add(new ProductMaterialPair(this.BlockProduct.Block, data, new ItemRequirement(mat.ID, 1)) { Skill = this.Skill });
                }
            }

            //var mats = (from reagent in this.Reagents from mat in MaterialComponent.Registry let obj = GameObject.Objects[mat] where reagent.Pass(obj) select obj).ToList();
            //foreach(var item in mats)
            //    list.Add(new ProductMaterialPair(this.BlockProduct.Block, 0, new ItemRequirement(item.ID, 1)));
            return list;
        }

        //static public readonly BlockConstruction WoodenDeck = new BlockConstruction(
        //    Reaction.Reagent.Create(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterialType(MaterialType.Wood), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
        //    new BlockConstruction.Product(Block.WoodenDeck));

                //, 
                //new Blocks.GetMaterialFromReagent("Base", BlockWoodenDeck.State.Tsanslate)


                //new ApplyState<BlockWoodenDeck.State>(
                //    BlockWoodenDeck.GetState(),
                //    (reagent, state) => state.Material = new Blocks.GetMaterialFromReagent("Base").Apply(reagent))

                //new ApplyState<BlockWoodenDeck.State>(
                //    (reagent, state)=> new Blocks.GetMaterialFromReagent("Base").Apply(reagent, state.Init)
                    //);
    }
}
