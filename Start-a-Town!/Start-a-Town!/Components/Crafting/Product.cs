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
        public int Complexity { get; private set; }

        public class Product
        {
            public class Modifier
            {
                public string LocalMaterialName { get; set; }
                Action<Entity, Entity> OnPerform { get; set; }
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
                public virtual void Apply(Entity material, Entity product) { OnPerform(material, product); }
                public virtual void Apply(Dictionary<string, Entity> materials, Entity product) { }
                public virtual void Apply(Dictionary<string, Entity> materials, GameObject building, Entity product) { }
                public virtual void Apply(Dictionary<string, Entity> materials, GameObject building, Entity product, Entity tool) { }
            }

            public enum Types { Tools, Workbenches, Furniture, Blocks }
            public Func<Dictionary<string, Entity>, Entity> ObjectCreator;
            //public Func<Dictionary<string, Entity>, Entity> ObjectCreator2;

            //public int Type;
            readonly List<Modifier> Modifiers = new();
            readonly List<Action<Dictionary<string, Entity>, Entity>> ModifierActions = new();
            public int Quantity = 1;
            public Product(string ingredientCopy)
            {
                this.ObjectCreator = dic => dic[ingredientCopy];
            }
            public Product(Func<Dictionary<string, Entity>, Entity> creator, int quantity = 1)
            {
                this.ObjectCreator = creator;
                this.Modifiers = new List<Modifier>();
                this.Quantity = quantity;
            }
            public Product(Func<Dictionary<string, Entity>, Entity> creator, params Modifier[] modifiers)
            {
                this.ObjectCreator = creator;
                this.Modifiers = new List<Modifier>(modifiers);
            }
            public Product(IItemFactory factory, params Modifier[] modifiers)
            {
                this.ObjectCreator = factory.Create;
                this.Modifiers = new List<Modifier>(modifiers);
            }
            public Product(ItemDef def, int quantity = 1)
            {
                this.ObjectCreator = mats=> def.Factory(def);
                this.Quantity = quantity;
            }
            public Product GetMaterialFromIngredient(string reagentName)
            {
                this.ModifierActions.Add((mats, e) => e.SetMaterial(mats[reagentName].PrimaryMaterial));
                return this;
            }
            //public Product GetMaterialFromReagentMaterial(string reagentName)
            //{
            //    this.ModifierActions.Add((mats, e) => e.GetNameFromReagentMaterial(mats[reagentName]));
            //    return this;
            //}
            //public Product GetMaterialFromReagent(string reagentName)
            //{
            //    //this.ModifierActions.Add((mats, e) => e.GetNameFromReagent(mats[reagentName]));
            //    this.ModifierActions.Add((mats, e) => e.SetMaterial(mats[reagentName].DominantMaterial));
            //    return this;
            //}
            public Product AddModifier(Modifier mod)
            {
                this.Modifiers.Add(mod);
                return this;
            }
            public List<GameObject> GetMaterialChoices(Reaction parent)
            {
                List<GameObject> mats = new List<GameObject>();
                foreach (var r in parent.Reagents)
                {
                    foreach (var mat in from mat in ItemCraftingComponent.Registry
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
                                        let obj = GameObject.Objects[mat] as Entity
                                        //where r.Condition.Condition(obj)
                                        where r.Filter(obj)
                                        select obj)
                    {
                        //string matName = mat.GetComponent<MaterialComponent>().Material.Name;
                        string matName = mat.GetComponent<MaterialsComponent>().Parts["Body"].Material.Name;
                        var product = this.ObjectCreator(new Dictionary<string, Entity>());// GameObject.Create(this.Type);
                        foreach (var mod in this.Modifiers)
                            if (r.Name == mod.LocalMaterialName)
                                mod.Apply(mat, product);
                        products.Add(new ProductMaterialPair(product, new ItemRequirement(mat.IDType, 1)));
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
            internal GameObject Create(Reaction reaction, List<GameObjectSlot> materials)
            {
                return this.Create(reaction, materials.ToDictionary(v => v.Name, v => v.Object as Entity));
            }
            public GameObject Create(Reaction parent, Dictionary<string, Entity> reagents)
            {
                if (reagents.Count != parent.Reagents.Count)
                    throw new ArgumentException("Material count mismatch");
                var product = this.ObjectCreator(reagents);
                //product.StackSize = this.Quantity;
                return product;
            }
            internal ProductMaterialPair GetProduct(Reaction reaction, List<GameObjectSlot> mats)
            {
                return GetProduct(reaction, mats.ToDictionary(v => v.Name, v => v.Object as Entity));
            }
            public ProductMaterialPair GetProduct(Reaction reaction, Dictionary<string, int> reagents)
            {
                if (reagents.Count != reaction.Reagents.Count)
                    throw new ArgumentException("Material count mismatch");
                //var reagentSlots = reagents.Select(k => new GameObjectSlot() { Name = k.Key, Object = GameObject.Objects[(GameObject.Types)k.Value] }).ToList();
                var reagentSlots = reagents.ToDictionary(v => v.Key, v => GameObject.Objects[v.Value] as Entity);
                var product = this.ObjectCreator(reagentSlots);
                product.StackSize = this.Quantity;

                foreach (var mod in this.Modifiers)
                    mod.Apply(reagentSlots, product);

                //var reqs = from reagent in reagentSlots select new ItemRequirement(reagent.Object.IDType, reaction.Reagents.First(r => r.Name == reagent.Name).Quantity) { Name = reagent.Name };
                var reqs = from reagent in reagentSlots select new ItemRequirement(reagent.Value.ID, reaction.Reagents.First(r => r.Name == reagent.Key).Quantity) { Name = reagent.Key };
                return new ProductMaterialPair(product, reqs);
            }
            public ProductMaterialPair Make(Actor actor, Reaction reaction, List<ObjectAmount> materials)//, out Dictionary<string, ObjectAmount> ingrs)
            {
                Dictionary<string, ObjectAmount> ingrs = CalculateIngredients(reaction, materials);
                return this.Make(actor, reaction, ingrs);
                //var dic = ingrs.ToDictionary(o => o.Key, o => o.Value.Object as Entity);
                //var product = this.ObjectCreator(dic);
                //foreach (var a in this.ModifierActions)
                //    a(dic, product);
                //var prodpair = new ProductMaterialPair(product, ingrs);
                //return prodpair;
            }
            public ProductMaterialPair Make(Actor actor, Reaction reaction, Dictionary<string, ObjectAmount> ingrs)//, out Dictionary<string, ObjectAmount> ingrs)
            {
                if (actor.Net is Net.Client)
                    return new ProductMaterialPair(reaction, null, ingrs);
                var dic = ingrs.ToDictionary(o => o.Key, o => o.Value.Object as Entity);
                var product = this.ObjectCreator(dic);
                foreach (var a in this.ModifierActions)
                    a(dic, product);
                product.SetQuality(DetermineQuality(actor, reaction));
                if (ingrs.Values.All(i => i.Object.Def.StackDimension == 1)) // TODO refactor the way product quantity is determined from ingredient dimensions
                    product.SetStackSize(this.Quantity);
                var prodpair = new ProductMaterialPair(reaction, product, ingrs);
                return prodpair;
                //return product;
            }

            private static Quality DetermineQuality(Actor actor, Reaction reaction)
            {
                var craftingLvl = actor.GetSkill(reaction.CraftSkill).Level;
                int complexity = reaction.Complexity;
                var masteryRatio = craftingLvl / (float)complexity;
                var quality = Quality.GetRandom(actor.Map.World.Random, masteryRatio);
                return quality;
            }

            private static Dictionary<string, ObjectAmount> CalculateIngredients(Reaction parent, List<ObjectAmount> materials)
            {
                var ingrs = new Dictionary<string, ObjectAmount>();
                //HashSet<GameObject> exhausted = new HashSet<GameObject>();
                foreach (var reagent in parent.Reagents)
                {
                    foreach (var mat in materials)
                    {
                        var amountRequired = reagent.Quantity * mat.Object.Def.StackDimension;
                        if (mat.Amount < amountRequired)
                            continue;
                        if (!reagent.Filter(mat.Object))
                            continue;
                        var name = reagent.Name;
                        if (ingrs.TryGetValue(name, out var existing))
                            existing.Amount += amountRequired;
                        else
                            ingrs[name] = new ObjectAmount(mat.Object, amountRequired);
                        mat.Amount -= amountRequired;
                        if (mat.Amount < 0)
                            throw new Exception();
                        var amountFound = ingrs[name].Amount;
                        if (amountFound > amountRequired)
                            throw new Exception();
                        else if (amountFound == amountRequired)
                        {
                            //exhausted.Add(mat.Object);
                            break;
                        }
                    }
                }


                //check if all mats found
                //foreach (var reagent in parent.Reagents)
                //{
                //}

                return ingrs;
            }

            public ProductMaterialPair GetProduct(Reaction parent, Dictionary<string, Entity> reagents)
            {
                if (reagents.Count != parent.Reagents.Count)
                    throw new ArgumentException("Material count mismatch");
                var product = this.ObjectCreator(reagents);

                foreach (var mod in this.Modifiers)
                    mod.Apply(reagents, product);
                return new ProductMaterialPair(product,
                    reagents.Select(v=>new ItemRequirement(v.Value.ID, 1){Name = v.Key}));
            }
            public ProductMaterialPair GetProduct(Reaction parent, GameObject building, Dictionary<string, Entity> reagents)
            {
                if (reagents.Count != parent.Reagents.Count)
                    throw new ArgumentException("Material count mismatch");
                var product = this.ObjectCreator(reagents);
                foreach (var mod in this.Modifiers)
                    mod.Apply(reagents, building, product);
                //return new ProductMaterialPair(product, new ItemRequirement(reagents.First().Object.ID, 1)); // TODO: add rest of reagents
                return new ProductMaterialPair(product,
                    //from reagent in reagents select new ItemRequirement(reagent.Object.IDType, 1) { Name = reagent.Name }); // TODO: add rest of reagents
                    reagents.Select(v => new ItemRequirement(v.Value.ID, 1) { Name = v.Key }));
            }
            public ProductMaterialPair GetProduct(Reaction parent, GameObject building, Dictionary<string, Entity> reagents, Entity tool)
            {
                if (reagents.Count != parent.Reagents.Count)
                    throw new ArgumentException("Material count mismatch");
                //var reagentsWithTool = new List<GameObjectSlot>(reagents);
                //reagentsWithTool.Add(tool);
                var product = this.ObjectCreator(reagents);
                foreach (var mod in this.Modifiers)
                    mod.Apply(reagents, building, product, tool);
                return new ProductMaterialPair(product,
                    //from reagent in reagents select new ItemRequirement(reagent.Object.IDType, 1) { Name = reagent.Name }) { Tool = tool }; // TODO: add rest of reagents
                    reagents.Select(v => new ItemRequirement(v.Value.ID, 1) { Name = v.Key })) { Tool = tool };

            }
            public ProductMaterialPair GetProduct(Reaction parent, GameObject building, List<ItemRequirement> reagents, Entity tool)
            {
                if (reagents.Count != parent.Reagents.Count)
                    throw new ArgumentException("Material count mismatch");
                //var reagentsWithTool = (from req in reagents select new GameObjectSlot(GameObject.Objects[req.ObjectID]) { Name = req.Name }).ToList();
                var reagentsWithTool = reagents.ToDictionary(v => v.Name, v => v.GetObject() as Entity);

                //reagentsWithTool.Add(tool);
                var product = this.ObjectCreator(reagentsWithTool);
                foreach (var mod in this.Modifiers)
                    mod.Apply(reagentsWithTool, building, product, tool);
                return new ProductMaterialPair(product, reagents) { Tool = tool }; // TODO: add rest of reagents
            }
            public ProductMaterialPair GetProduct(Reaction parent, GameObject building, List<ItemRequirement> reagents)
            {
                if (reagents.Count != parent.Reagents.Count)
                    throw new ArgumentException("Material count mismatch");
                //var reagentsWithTool = (from req in reagents select new GameObjectSlot(GameObject.Objects[req.ObjectID]) { Name = req.Name }).ToList();
                var reagentsWithTool = reagents.ToDictionary(v => v.Name, v => v.GetObject() as Entity);

                //reagentsWithTool.Add(tool);
                var product = this.ObjectCreator(reagentsWithTool);
                foreach (var mod in this.Modifiers)
                    mod.Apply(reagentsWithTool, building, product);
                return new ProductMaterialPair(product, reagents); // TODO: add rest of reagents
            }
            

            static public List<Product> Create(params Product[] product)
            {
                return new List<Product>(product);
            }
            public class ProductMaterialPair : IConstructionProduct
            {
                public GameObject Product;
                public readonly Reaction Reaction;
                public ItemRequirement Req { get { return this.Requirements.FirstOrDefault(); } }
                public List<ItemRequirement> Requirements;
                public GameObject Tool;
                public ToolAbilityDef Skill;
                public Dictionary<string, ObjectAmount> RequirementsNew;
                public ToolAbilityDef GetSkill()
                {
                    return this.Skill;
                }
                public List<ItemRequirement> GetReq()
                {
                    return this.Requirements;
                }
                public void SpawnProduct(IMap map, Vector3 global)
                {
                    var net = map.Net;
                    var clone = this.Product.Clone();
                    clone.Global = global;
                    net.Spawn(clone);
                }

                public ProductMaterialPair()
                {

                }
                public ProductMaterialPair(Reaction reaction, GameObject product, Dictionary<string, ObjectAmount> ingredients)
                {
                    this.Reaction = reaction;
                    this.Product = product;
                    this.RequirementsNew = ingredients;
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
                    save.Add(new SaveTag(SaveTag.Types.Compound, "Product", this.Product.SaveInternal()));
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
                public bool MaterialsAvailable(IEnumerable<GameObject> items)
                {
                    foreach (var item in this.Requirements)
                    {
                        int amountFound = 0;
                        foreach (var found in items.Where(i => i.ID == item.ObjectID))
                            amountFound += found.StackSize;
                        if (amountFound < item.AmountRequired)
                            return false;
                    }
                    return true;
                }
                public void ConsumeMaterials()
                {
                    foreach (var req in this.RequirementsNew)
                    {
                        //int amountFound = 0;
                        //foreach (var found in items.Where(i => i.ID == item.ObjectID))
                        //    amountFound += found.StackSize;
                        //if (amountFound < item.AmountRequired)
                        //    return false;
                        var reagentName = req.Key;
                        if (this.Reaction.GetIngredient(reagentName).IsPreserved)
                            continue;
                        var item = req.Value;
                        if (item.Amount > item.Object.StackSize)
                            throw new Exception("required amount is larger than actual amount");
                        item.Object.StackSize -= item.Amount;
                    }
                    if (this.RequirementsNew.Values.Any(o => o.Object.StackSize > 0))
                    {
                        // UNDONE reactions can preserve ingredients (like repairing)
                        //throw new Exception("leftover materials in reaction"); 
                    }
                }
                public bool MaterialsAvailable(IEnumerable<GameObjectSlot> container)
                {
                    foreach(var item in this.Requirements)
                    {
                        int amountFound = 0;
                        foreach (var found in from slot in container where slot.HasValue where (int)slot.Object.IDType == item.ObjectID select slot.Object)
                            amountFound += found.StackSize;
                        if (amountFound < item.AmountRequired)
                            return false;
                    }
                    return true;
                }
                public bool MaterialsAvailable(GameObject actor)
                {
                    //var container = actor.GetComponent<PersonalInventoryComponent>().Children;
                    var container = PlayerOld.Actor.GetComponent<PersonalInventoryComponent>().Slots.Slots;// Children;

                    //var nearbyMaterials = actor.GetNearbyObjects(rng => rng <= 2).ConvertAll(o => o.ToSlot());
                    //var availableMats = nearbyMaterials.Concat(container);
                    //return this.MaterialsAvailable(availableMats);
                    return this.MaterialsAvailable(container);
                }
                public bool MaterialsAvailable(GameObject actor, GameObject workstation)
                {
                    var container = workstation.GetChildren();
                    //var container = actor.GetComponent<PersonalInventoryComponent>().Children;
                    var inventory = PlayerOld.Actor.GetComponent<PersonalInventoryComponent>().Slots.Slots;// Children;
                    var availableMats = inventory.Concat(container);
                    //var nearbyMaterials = actor.GetNearbyObjects(rng => rng <= 2).ConvertAll(o => o.ToSlot());
                    //var availableMats = nearbyMaterials.Concat(container);
                    //return this.MaterialsAvailable(availableMats);
                    return this.MaterialsAvailable(availableMats);
                }

                public List<GameObjectSlot> MaterialsFound(GameObject actor)
                {
                    //var container = workstation.GetChildren();
                    var inventory = PlayerOld.Actor.GetComponent<PersonalInventoryComponent>().Slots.Slots;// Children;
                    //var availableMats = inventory.Concat(container);
                    return inventory.ToList();
                }

                public bool ConsumeMaterials(IObjectProvider net, IEnumerable<GameObjectSlot> container)
                {
                    foreach (var item in this.Requirements)
                    {
                        int amountRemaining = item.AmountRequired;
                        foreach (var found in from slot in container where slot.HasValue where (int)slot.Object.IDType == item.ObjectID select slot)
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
                public bool ConsumeMaterials(IObjectProvider net, IEnumerable<Entity> items)
                {
                    foreach (var item in this.Requirements)
                    {
                        int amountRemaining = item.AmountRequired;
                        foreach (var found in items.Where(i => i?.ID == item.ObjectID))
                        {
                            int amountToTake = Math.Min(found.StackSize, amountRemaining);
                            amountRemaining -= amountToTake;
                            found.StackSize -= amountToTake;
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

            public Product RestoreDurability()
            {
                this.ModifierActions.Add((mats, prod) => prod.GetResource(ResourceDef.Durability).Percentage = 1);
                return this;
            }
        }
    }
}
