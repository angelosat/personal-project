using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    public class BlueprintStage : Dictionary<GameObject.Types, int>
    {
        //void Add(GameObject.Types material, int count)
        //{
        //    this[material] = count;
        //}
        //public GameObjectSlot GetSlot()
        //{

        //}

        public Predicate<GameObject> Condition;

        public BlueprintStage(Predicate<GameObject> condition = null)
            : base()
        {
            this.Condition = condition != null ? condition : foo => true;
        }
    }

    public class Blueprint
    {
        //public static readonly GameObject BlueprintBlueprint = Blueprint.Create(GameObject.Types.BlueprintBlueprint, GameObject.Types.FurnitureParts, new BlueprintStage() { { GameObject.Types.WoodenPlank, 2 } }));
        public static readonly Dictionary<GameObject.Types, int> BlueprintMaterials = new Dictionary<GameObject.Types, int>() { { GameObject.Types.Paper, 1 } };

        public List<BlueprintStage> Stages;
        public GameObject.Types ProductID;
        public int Level = 1;

        public Blueprint(GameObject.Types productID, params BlueprintStage[] stages)
        {
            ProductID = productID;
            Stages = new List<BlueprintStage>();
            foreach (BlueprintStage stage in stages)
                Stages.Add(stage);
        }

        static public GameObject Create(GameObject.Types blueprintIndex, GameObject.Types productIndex, params BlueprintStage[] stages)
        {
            GameObject obj = new GameObject();
            obj["Info"] = new DefComponent(blueprintIndex, ObjectType.Blueprint, "Blueprint: " + GameObject.Objects[productIndex].Name, "A blueprint");
            obj["Gui"] = GameObject.Objects[productIndex]["Gui"];
            obj["Physics"] = new PhysicsComponent();
            obj["Sprite"] = new SpriteComponent(Sprite.Default);//Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[24] } }, new Vector2(16, 16));
            //obj["Blueprint"] = new BlueprintComponent().SetValue("Blueprint", new Blueprint(productIndex, stages));
            //obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.CraftingWorkbench);
            return obj;
        }

        public BlueprintStage this[int stage]
        {
            get { return Stages[stage]; }
            set { Stages[stage] = value; }
        }

        public string Name
        { get { return GameObject.Objects[ProductID].Name; } }

        public ObjectFilter GetFilter(int stage)
        {
            ObjectFilter filter = new ObjectFilter(FilterType.Include);
            foreach (KeyValuePair<GameObject.Types, int> mat in this[stage])
            {
                List<GameObject.Types> existing;
                string type = GameObject.Objects[mat.Key].Type;
                if (!filter.TryGetValue(type, out existing))
                {
                    existing = new List<GameObject.Types>();
                    filter[type] = existing;
                }
                existing.Add(mat.Key);
            }
            return filter;
        }
        public GameObject Craft()
        {
            return GameObject.Create(ProductID);
        }
        internal void Craft(ItemContainer container)
        {
            this.Craft(container, container);
        }
        internal void Craft(ItemContainer mats, ItemContainer products)
        {
            GameObject pr;
            this.Craft(mats, products, out pr);
        }
        public bool Craft(GameObjectSlotCollection container, out GameObject product)
        {
            return Craft(container, container, out product);
        }
        public bool Craft(GameObjectSlotCollection materialsContainer, GameObjectSlotCollection targetContainer, out GameObject product)
        {
            // check materials
            if (!CheckMaterials(materialsContainer))
            {
                product = null;
                return false;
            }
            // remove materials
            RemoveMaterials(materialsContainer);
            product = GameObject.Create(ProductID);

            var t1 = targetContainer
                .FindAll(slot => !slot.HasValue);
            var t2 = t1.Concat(targetContainer.FindAll(slot => slot.HasValue).FindAll(slot => slot.Object.IDType == ProductID).FindAll(slot => slot.StackSize < slot.StackSize));

                t2.First().SetObject(product).StackSize++;


            //targetContainer
            //    .FindAll(slot => !slot.HasValue)
            //    .Concat(targetContainer.FindAll(slot => slot.HasValue).FindAll(slot => slot.Object.ID == ProductID).FindAll(slot => slot.StackSize < slot.StackSize))
            //    .First().Set(product).StackSize++;
            return true;
        }

        public bool Craft(ItemContainer materialsContainer, ItemContainer targetContainer, out GameObject product)
        {
            // check materials
            if (!CheckMaterials(materialsContainer))
            {
                product = null;
                return false;
            }
            // remove materials
            RemoveMaterials(materialsContainer);
            product = GameObject.Create(ProductID);

            var t1 = targetContainer
                .FindAll(slot => !slot.HasValue);
            var t2 = t1.Concat(targetContainer.FindAll(slot => slot.HasValue).FindAll(slot => slot.Object.IDType == ProductID).FindAll(slot => slot.StackSize < slot.StackSize));

            t2.First().SetObject(product).StackSize++;


            //targetContainer
            //    .FindAll(slot => !slot.HasValue)
            //    .Concat(targetContainer.FindAll(slot => slot.HasValue).FindAll(slot => slot.Object.ID == ProductID).FindAll(slot => slot.StackSize < slot.StackSize))
            //    .First().Set(product).StackSize++;
            return true;
        }
        static public bool Copy(IObjectProvider net, GameObject.Types bpID, GameObject materialsParent, ItemContainer materialsContainer, GameObject productsParent, ItemContainer targetContainer, out GameObject product)
        {
            var mats = new Dictionary<GameObject.Types, int>() { { GameObject.Types.Paper, 1 } };
            // check materials
            if (!CheckMaterials(materialsContainer, mats))
            {
                product = null;
                return false;
            }
            RemoveMaterials(net, materialsContainer, mats);
            GameObjectSlot firstAvailable = (from slot in targetContainer
                                             where slot.HasValue
                                             where slot.Object.IDType == bpID
                                             where slot.StackSize < slot.StackMax
                                             select slot)
                                             .FirstOrDefault();
            if (firstAvailable == null)
                firstAvailable = (from slot in targetContainer where !slot.HasValue select slot).FirstOrDefault();
            product = GameObject.Create(bpID);
            if (firstAvailable == null) // if no available slot, pop loot it
            {
                net.PopLoot(product, productsParent);
                return true;
            }

            net.Spawn(product, productsParent, firstAvailable.ID);
            return true;
        }
        public bool Craft(IObjectProvider net, GameObject materialsParent, ItemContainer materialsContainer, GameObject productsParent, ItemContainer targetContainer, out GameObject product)
        {
            // check materials
            if (!CheckMaterials(materialsContainer))
            {
                product = null;
                return false;
            }
            RemoveMaterials(net, materialsContainer);
            GameObjectSlot firstAvailable = (from slot in targetContainer
                                             where slot.HasValue
                                             where slot.Object.IDType == ProductID
                                             where slot.StackSize < slot.StackMax
                                             select slot)
                                             .FirstOrDefault();
            if(firstAvailable == null)
                firstAvailable = (from slot in targetContainer where !slot.HasValue select slot).FirstOrDefault();

            
            if(GameObject.Objects[ProductID].HasComponent<PackableComponent>())
            {
                product = GameObject.Create(GameObject.Types.Package);
                product.GetComponent<PackageComponent>().Content.Object = GameObject.Create(ProductID);
            }
            else
                product = GameObject.Create(ProductID);

            if (firstAvailable == null) // if no available slot, pop loot it
            {
                net.PopLoot(product, productsParent);
                return true;
            }
            // remove materials

            

            net.Spawn(product, productsParent, firstAvailable.ID);
            //targetContainer.InsertObject(net, product);
            

            //Net.Network.InventoryOperation(net, product, firstAvailable, product.ToSlot(), 1);
            //TargetArgs productsTarget = new TargetArgs(productsParent);
            //net.InventoryOperation(productsParent, new ArrangeChildrenArgs(produ)
            return true;
            //var t1 = targetContainer
            //    .FindAll(slot => !slot.HasValue);
            //var t2 = t1.Concat(targetContainer.FindAll(slot => slot.HasValue).FindAll(slot => slot.Object.ID == ProductID).FindAll(slot => slot.StackSize < slot.StackSize));

            //t2.First().Set(product).StackSize++;
        }
        //public bool Craft(GameObject actor, GameObject table, GameObjectSlotCollection materialsContainer, GameObjectSlotCollection targetContainer, out GameObject product)
        //{
        //    return Craft(materialsContainer, targetContainer, out product);
        //}
        public static bool CheckMaterials(GameObjectSlotCollection container, Dictionary<GameObject.Types, int> mats)
        {
            foreach (var mat in mats)
            {
                int amount = 0;
                container
                    .FindAll(slot => slot.HasValue)
                    .FindAll(slot => slot.Object.IDType == mat.Key)
                    .ForEach(slot => amount += slot.StackSize);
                if (amount < mat.Value)
                {
                    //  product = null;
                    return false;
                }
            }
            return true;
        }
        bool CheckMaterials(GameObjectSlotCollection container)
        {
            foreach (var stage in this.Stages)
                foreach (var mat in stage)
                {
                    int amount = 0;
                    container
                        .FindAll(slot => slot.HasValue)
                        .FindAll(slot => slot.Object.IDType == mat.Key)
                        .ForEach(slot => amount += slot.StackSize);
                    if (amount < mat.Value)
                    {
                      //  product = null;
                        return false;
                    }
                }
            return true;
        }
        void RemoveMaterials(GameObjectSlotCollection container)
        {
            foreach (var stage in this.Stages)
                foreach (var mat in stage)
                {
                    int remaining = mat.Value;
                    foreach (var slot in container
                        .FindAll(slot => slot.HasValue)
                        .FindAll(slot => slot.Object.IDType == mat.Key))
                    {
                        int diff = remaining - slot.StackSize;
                        slot.StackSize -= Math.Min(slot.StackSize, remaining);
                        remaining -= diff;
                        if (remaining < 0)
                            break;
                    }
                }
        }
        void RemoveMaterials(IObjectProvider net, GameObjectSlotCollection container)
        {
            foreach (var stage in this.Stages)
                foreach (var mat in stage)
                {
                    int remaining = mat.Value;
                    foreach (var slot in container
                        .FindAll(slot => slot.HasValue)
                        .FindAll(slot => slot.Object.IDType == mat.Key))
                    {
                        int diff = remaining - slot.StackSize;

                        if (diff == 0) //if the slot will become empty, dispose object from network
                            net.DisposeObject(slot.Object);

                        slot.StackSize -= Math.Min(slot.StackSize, remaining);

                        remaining -= diff;
                        if (remaining < 0)
                            break;
                    }
                }
        }
        static void RemoveMaterials(IObjectProvider net, GameObjectSlotCollection container, Dictionary<GameObject.Types, int> mats)
        {
            foreach (var mat in mats)
            {
                int remaining = mat.Value;
                foreach (var slot in container
                    .FindAll(slot => slot.HasValue)
                    .FindAll(slot => slot.Object.IDType == mat.Key))
                {
                    int diff = remaining - slot.StackSize;

                    if (diff == 0) //if the slot will become empty, dispose object from network
                        net.DisposeObject(slot.Object);

                    slot.StackSize -= Math.Min(slot.StackSize, remaining);

                    remaining -= diff;
                    if (remaining < 0)
                        break;
                }
            }
        }
    }

    class Plan
    {
        public static GameObject Create(GameObject.Types blueprintIndex, GameObject.Types productIndex, params BlueprintStage[] stages)
        {
            GameObject obj = new GameObject();
            obj["Info"] = new DefComponent(blueprintIndex, ObjectType.Plan, "Plan: " + GameObject.Objects[productIndex].Name, "A plan");
            obj["Gui"] = GameObject.Objects[productIndex]["Gui"];
            obj["Physics"] = new PhysicsComponent();
            obj["Sprite"] = new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[24] } }, new Vector2(16, 16));
            //obj["Blueprint"] = new BlueprintComponent().SetValue("Blueprint", new Blueprint(productIndex, stages));
            //obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
            return obj;
        }
    }

    class Schematic
    {
        public static GameObject Create(GameObject.Types blueprintIndex, GameObject.Types productIndex, params BlueprintStage[] stages)
        {
            GameObject obj = new GameObject();
            obj["Info"] = new DefComponent(blueprintIndex, ObjectType.Schematic, "Schematic: " + GameObject.Objects[productIndex].Name, "A schematic");
            obj["Gui"] = GameObject.Objects[productIndex]["Gui"];
            obj["Physics"] = new PhysicsComponent();
            obj["Sprite"] = new SpriteComponent(Sprite.Default);//Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[24] } }, new Vector2(16, 16));
            //obj["Blueprint"] = new BlueprintComponent().SetValue("Blueprint", new Blueprint(productIndex, stages));
            //obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
            return obj;
        }
    }
}
