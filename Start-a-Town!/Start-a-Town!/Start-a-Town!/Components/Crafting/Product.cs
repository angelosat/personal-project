using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Components.Crafting
{
    partial class Reaction
    {
        public class Product
        {
            public class Modifier
            {
                public string LocalMaterialName { get; set; }
                Action<GameObject, GameObject> OnPerform { get; set; }
                public Modifier()
                {

                }
                public Modifier(string matName)
                {
                    this.LocalMaterialName = matName;
                }
                public Modifier(string matName, Action<GameObject, GameObject> modifier)
                    : this(matName)
                {
                    this.OnPerform = modifier;
                }
                public virtual void Apply(GameObject material, GameObject product) { OnPerform(material, product); }
                public virtual void Apply(List<GameObjectSlot> materials, GameObject product) { }
                public virtual void Apply(List<GameObjectSlot> materials, GameObject building, GameObject product) { }
                public virtual void Apply(List<GameObjectSlot> materials, GameObject building, GameObject product, GameObject tool) { }
            }

            public enum Types { Tools, Workbenches, Furniture, Blocks }
            public Func<List<GameObjectSlot>, GameObject> ObjectCreator { get; set; }
            //public int Type;
            List<Modifier> Modifiers { get; set; }
            //public int Quantity = 1;
            public Product(Func<List<GameObjectSlot>, GameObject> creator, params Modifier[] modifiers)
            {
                this.ObjectCreator = creator;
                this.Modifiers = new List<Modifier>(modifiers);
            }
            public Product(IItemFactory factory, params Modifier[] modifiers)
            {
                this.ObjectCreator = factory.Create;
                this.Modifiers = new List<Modifier>(modifiers);
            }

            //public Product(GameObject.Types type, params Modifier[] modifiers)
            //{
            //    this.Type = (int)type;
            //    this.Modifiers = new List<Modifier>(modifiers);
            //}
            //public Product(int type, params Modifier[] modifiers)
            //{
            //    this.Type = type;
            //    this.Modifiers = new List<Modifier>(modifiers);
            //}

            public List<GameObject> GetMaterialChoices(Reaction parent)
            {
                List<GameObject> mats = new List<GameObject>();
                foreach (var r in parent.Reagents)
                {
                    foreach (var mat in from mat in MaterialComponent.Registry
                                        let obj = GameObject.Objects[mat]
                                        //where r.Condition.Condition(obj)
                                        where r.Filter(obj)
                                        select obj)
                    {
                        mats.Add(mat);
                    }
                }
                return mats;
            }

            public List<ProductMaterialPair> Create(Reaction parent)
            {
                List<ProductMaterialPair> products = new List<ProductMaterialPair>();
                foreach (var r in parent.Reagents)
                {
                    foreach (var mat in from mat in ReagentComponent.Registry //MaterialComponent.Registry
                                        let obj = GameObject.Objects[mat]
                                        //where r.Condition.Condition(obj)
                                        where r.Filter(obj)
                                        select obj)
                    {
                        //string matName = mat.GetComponent<MaterialComponent>().Material.Name;
                        string matName = mat.GetComponent<MaterialsComponent>().Parts["Body"].Material.Name;
                        GameObject product = this.ObjectCreator(new List<GameObjectSlot>());// GameObject.Create(this.Type);
                        foreach (var mod in this.Modifiers)
                            if (r.Name == mod.LocalMaterialName)
                                mod.Apply(mat, product);
                        products.Add(new ProductMaterialPair(product, new ItemRequirement(mat.ID, 1)));
                    }
                }
                return products;
            }

            //public GameObject Create(Reaction parent, List<GameObject> materials)
            //{
            //    if (materials.Count != parent.Reagents.Count)
            //        throw new ArgumentException("Material count mismatch");
            //    GameObject product = GameObject.Create(this.Type);
            //    for (int i = 0; i < materials.Count; i++)
            //    {
            //        Reagent r = parent.Reagents[i];
            //        GameObject mat = materials[i];
            //        //if (!r.Condition.Condition(mat))
            //        if (!r.Pass(mat))
            //            throw new ArgumentException("Wrong materials");
            //        foreach (var mod in this.Modifiers)
            //            if (r.Name == mod.LocalMaterialName)
            //                mod.Apply(mat, product);
            //    }
            //    return product;
            //}

            public GameObject Create(Reaction parent, List<GameObjectSlot> reagents)
            {
                if (reagents.Count != parent.Reagents.Count)
                    throw new ArgumentException("Material count mismatch");
                GameObject product = this.ObjectCreator(reagents);
                //product.StackSize = this.Quantity;
                return product;
            }

            public ProductMaterialPair GetProduct(Reaction parent, List<GameObjectSlot> reagents)
            {
                if (reagents.Count != parent.Reagents.Count)
                    throw new ArgumentException("Material count mismatch");
                GameObject product = this.ObjectCreator(reagents);
                //product.StackSize = this.Quantity;

                foreach (var mod in this.Modifiers)
                    mod.Apply(reagents, product);
                //return new ProductMaterialPair(product, new ItemRequirement(reagents.First().Object.ID, 1)); // TODO: add rest of reagents
                return new ProductMaterialPair(product,
                    from reagent in reagents select new ItemRequirement(reagent.Object.ID, 1) { Name = reagent.Name }); // TODO: add rest of reagents
            }
            public ProductMaterialPair GetProduct(Reaction parent, GameObject building, List<GameObjectSlot> reagents)
            {
                if (reagents.Count != parent.Reagents.Count)
                    throw new ArgumentException("Material count mismatch");
                GameObject product = this.ObjectCreator(reagents);
                foreach (var mod in this.Modifiers)
                    mod.Apply(reagents, building, product);
                //return new ProductMaterialPair(product, new ItemRequirement(reagents.First().Object.ID, 1)); // TODO: add rest of reagents
                return new ProductMaterialPair(product,
                    from reagent in reagents select new ItemRequirement(reagent.Object.ID, 1) { Name = reagent.Name }); // TODO: add rest of reagents
            }
            public ProductMaterialPair GetProduct(Reaction parent, GameObject building, List<GameObjectSlot> reagents, GameObject tool)
            {
                if (reagents.Count != parent.Reagents.Count)
                    throw new ArgumentException("Material count mismatch");
                var reagentsWithTool = new List<GameObjectSlot>(reagents);
                //reagentsWithTool.Add(tool);
                GameObject product = this.ObjectCreator(reagentsWithTool);
                foreach (var mod in this.Modifiers)
                    mod.Apply(reagents, building, product, tool);
                return new ProductMaterialPair(product,
                    from reagent in reagents select new ItemRequirement(reagent.Object.ID, 1) { Name = reagent.Name }) { Tool = tool }; // TODO: add rest of reagents
            }
            public ProductMaterialPair GetProduct(Reaction parent, GameObject building, List<ItemRequirement> reagents, GameObject tool)
            {
                if (reagents.Count != parent.Reagents.Count)
                    throw new ArgumentException("Material count mismatch");
                var reagentsWithTool = (from req in reagents select new GameObjectSlot(GameObject.Objects[req.ObjectID]) { Name = req.Name }).ToList();
                //reagentsWithTool.Add(tool);
                GameObject product = this.ObjectCreator(reagentsWithTool);
                foreach (var mod in this.Modifiers)
                    mod.Apply(reagentsWithTool, building, product, tool);
                return new ProductMaterialPair(product, reagents) { Tool = tool }; // TODO: add rest of reagents
            }
            public ProductMaterialPair GetProduct(Reaction parent, GameObject building, List<ItemRequirement> reagents)
            {
                if (reagents.Count != parent.Reagents.Count)
                    throw new ArgumentException("Material count mismatch");
                //var reagentsWithTool = (from req in reagents select GameObject.Objects[req.ObjectID].ToSlot()).ToList();
                var reagentsWithTool = (from req in reagents select new GameObjectSlot(GameObject.Objects[req.ObjectID]) { Name = req.Name }).ToList();
                //reagentsWithTool.Add(tool);
                GameObject product = this.ObjectCreator(reagentsWithTool);
                foreach (var mod in this.Modifiers)
                    mod.Apply(reagentsWithTool, building, product);
                return new ProductMaterialPair(product, reagents); // TODO: add rest of reagents
            }
            //public List<ProductMaterialPair> Create(Reaction parent)
            //{
            //    List<ProductMaterialPair> products = new List<ProductMaterialPair>();// Dictionary<GameObject, ItemRequirement>();
            //    if(string.IsNullOrEmpty(this.FromMaterial))
            //    {

            //        foreach(var r in parent.Reagents)
            //        {
            //            foreach (var mat in from mat in MaterialComponent.Registry
            //                                let obj = GameObject.Objects[mat]
            //                                where r.Condition(obj)
            //                                select obj)
            //            {
            //                string matName = mat.GetComponent<MaterialComponent>().Name;
            //                GameObject product = GameObject.Create(this.Type);
            //                products.Add(new ProductMaterialPair(product, new ItemRequirement(mat.ID, 1)));
            //            }
            //        }
            //        return products;
            //    }
            //    Reagent reagent = parent.Reagents.FirstOrDefault(r => r.Name == this.FromMaterial);
            //    foreach (var mat in from mat in MaterialComponent.Registry
            //                        let obj = GameObject.Objects[mat]
            //                        where reagent.Condition(obj)
            //                        select obj)
            //    {
            //        string matName = mat.GetComponent<MaterialComponent>().Name;
            //        GameObject product = GameObject.Create(this.Type);
            //        product.Name = product.Name.Insert(0, matName + " ");
            //        products.Add(new ProductMaterialPair(product, new ItemRequirement(mat.ID, 1)));
            //    }
            //    return products;
            //}

            static public List<Product> Create(params Product[] product)
            {
                return new List<Product>(product);
            }
            public class ProductMaterialPair : IConstructionProduct
            {
                public GameObject Product;
                public ItemRequirement Req { get { return this.Requirements.FirstOrDefault(); } }
                public List<ItemRequirement> Requirements;
                public GameObject Tool;
                public Skill Skill;

                public Skill GetSkill()
                {
                    return this.Skill;
                }
                public List<ItemRequirement> GetReq()
                {
                    return this.Requirements;
                }
                public void SpawnProduct(IMap map, Vector3 global)
                {
                    var net = map.GetNetwork();
                    var clone = this.Product.Clone();
                    clone.Global = global;
                    net.Spawn(clone);
                }

                public ProductMaterialPair()
                {

                }
                public ProductMaterialPair(GameObject product, ItemRequirement req)
                    : this(product, new List<ItemRequirement>() { req })
                {
                    //this.Product = product;
                    //this.Req = req;
                }
                public ProductMaterialPair(GameObject product, IEnumerable<ItemRequirement> req)
                {
                    this.Product = product;
                    this.Requirements = req.ToList();
                }
                public ProductMaterialPair(System.IO.BinaryReader r)
                {
                    this.Product = GameObject.CreatePrefab(r);
                    //this.Req = new ItemRequirement(r);
                    int reqCount = r.ReadInt32();
                    this.Requirements = new List<ItemRequirement>();
                    for (int i = 0; i < reqCount; i++)
                    {
                        this.Requirements.Add(new ItemRequirement(r));
                    }
                }
                public ProductMaterialPair(SaveTag tag)
                {
                    this.Product = GameObject.Load(tag["Product"] as SaveTag);
                    //this.Req = new ItemRequirement(tag);
                    //foreach(var tag in tag.TryGetTag())
                    this.Requirements = new List<ItemRequirement>();
                    tag.TryGetTag("Materials", t =>
                    {
                        foreach (var item in t.Value as List<SaveTag>)
                        {
                            this.Requirements.Add(new ItemRequirement(item));
                        }
                        //this.Req.Add(new ItemRequirement(t.TryGetTag))
                    });
                }
                public void Write(System.IO.BinaryWriter w)
                {
                    this.Product.Write(w);
                    //w.Write((int)this.Req.ObjectID);
                    //w.Write(this.Req.Max);
                    w.Write(this.Requirements.Count);
                    foreach (var item in this.Requirements)
                    {
                        //w.Write((int)item.ObjectID);
                        //w.Write(item.Max);
                        item.Write(w);
                    }
                }
                //public void Read(System.IO.BinaryReader r)
                //{
                //    this.Product = GameObject.CreatePrefab(r);
                //    this.Req.ObjectID = r.ReadInt32();
                //    this.Req.Max = r.ReadInt32();
                //}
                public List<SaveTag> Save()
                {
                    List<SaveTag> save = new List<SaveTag>();
                    save.Add(new SaveTag(SaveTag.Types.Compound, "Product", this.Product.Save()));
                    //save.Add(new SaveTag(SaveTag.Types.Int, "Material", (int)this.Req.ObjectID));
                    //save.Add(new SaveTag(SaveTag.Types.Int, "Amount", this.Req.Max));
                    SaveTag reqs = new SaveTag(SaveTag.Types.List, "Materials", SaveTag.Types.Compound);
                    foreach (var item in this.Requirements)
                    {
                        //SaveTag req = new SaveTag(SaveTag.Types.Compound, "");
                        //req.Add(new SaveTag(SaveTag.Types.Compound, "Material", (int)item.ObjectID));
                        //req.Add(new SaveTag(SaveTag.Types.Int, "Amount", item.Max));
                        //reqs.Add(req);
                        reqs.Add(new SaveTag(SaveTag.Types.Compound, "", item.Save()));
                    }
                    save.Add(reqs);
                    return save;
                }
                
                public bool MaterialsAvailable(IEnumerable<GameObjectSlot> container)
                {
                    foreach(var item in this.Requirements)
                    {
                        int amountFound = 0;
                        foreach (var found in from slot in container where slot.HasValue where (int)slot.Object.ID == item.ObjectID select slot.Object)
                            amountFound += found.StackSize;
                        if (amountFound < item.Max)
                            return false;
                    }
                    return true;
                }
                public bool MaterialsAvailable(GameObject actor)
                {
                    //var container = actor.GetComponent<PersonalInventoryComponent>().Children;
                    var container = Player.Actor.GetComponent<PersonalInventoryComponent>().Slots.Slots;// Children;

                    //var nearbyMaterials = actor.GetNearbyObjects(rng => rng <= 2).ConvertAll(o => o.ToSlot());
                    //var availableMats = nearbyMaterials.Concat(container);
                    //return this.MaterialsAvailable(availableMats);
                    return this.MaterialsAvailable(container);
                }
                public bool MaterialsAvailable(GameObject actor, GameObject workstation)
                {
                    var container = workstation.GetChildren();
                    //var container = actor.GetComponent<PersonalInventoryComponent>().Children;
                    var inventory = Player.Actor.GetComponent<PersonalInventoryComponent>().Slots.Slots;// Children;
                    var availableMats = inventory.Concat(container);
                    //var nearbyMaterials = actor.GetNearbyObjects(rng => rng <= 2).ConvertAll(o => o.ToSlot());
                    //var availableMats = nearbyMaterials.Concat(container);
                    //return this.MaterialsAvailable(availableMats);
                    return this.MaterialsAvailable(availableMats);
                }

                public List<GameObjectSlot> MaterialsFound(GameObject actor)
                {
                    //var container = workstation.GetChildren();
                    var inventory = Player.Actor.GetComponent<PersonalInventoryComponent>().Slots.Slots;// Children;
                    //var availableMats = inventory.Concat(container);
                    return inventory.ToList();
                }

                public bool ConsumeMaterials(Net.IObjectProvider net, IEnumerable<GameObjectSlot> container)
                {
                    foreach (var item in this.Requirements)
                    {
                        int amountRemaining = item.Max;
                        foreach (var found in from slot in container where slot.HasValue where (int)slot.Object.ID == item.ObjectID select slot)
                        {
                            int amountToTake = Math.Min(found.Object.StackSize, amountRemaining);
                            amountRemaining -= amountToTake;
                            //found.Object.StackSize -= amountToTake;
                            
                            //if(found.Object.StackSize == 0)
                            if (amountToTake == found.Object.StackSize)
                            {
                                net.Despawn(found.Object);
                                net.DisposeObject(found.Object);
                                found.Clear();
                                //net.SyncDisposeObject(found);
                            }
                            else
                                found.Object.StackSize -= amountToTake;
                            if (amountRemaining == 0)
                                break;
                        }
                    }
                    return true;
                }

                //public UI.CraftingTooltip GetUI()
                //{
                //    foreach (var mat in this.Requirements)
                //    {
                //        //int amount = this.Bench.Slots.GetAmount((GameObject obj) => (int)obj.ID == mat.ObjectID);
                //        int amount = this.Slots_Reagents.Count(obj => (int)obj.ID == mat.ObjectID);
                //        mat.Amount = amount;
                //    }
                //    UI.CraftingTooltip tip = new UI.CraftingTooltip(this.Product.ToSlot(), this.Requirements);
                //    return tip;
                //}
            }
        }
    }
}
