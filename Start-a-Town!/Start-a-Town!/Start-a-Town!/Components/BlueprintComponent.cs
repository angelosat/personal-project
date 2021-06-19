using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class BlueprintEventArgs : EventArgs
    {
        public Blueprint Blueprint;
        public int Orientation, Variation;
        public BlueprintEventArgs(Blueprint bp, int variation = 0, int orientation = 0)
        {
            this.Blueprint = bp;
            this.Orientation = orientation;
            this.Variation = variation;
        }
    }
    class BlueprintComponent : Component
    {
        public override string ComponentName
        {
            get { return "Blueprint"; }
        }
        static BlueprintCollection _Blueprints;
        //static public Dictionary<GameObject.Types, Blueprint> Blueprints
        static public BlueprintCollection Blueprints
        {
            get
            {
                if (_Blueprints == null)
                    _Blueprints = GenerateBlueprints();
                return _Blueprints;
            }
        }

        public Blueprint Blueprint { get { return (Blueprint)this["Blueprint"]; } set { this["Blueprint"] = value; } }
        public Action<GameObject, GameObject> OnCraft { get { return (Action<GameObject, GameObject>)this["OnCraft"]; } set { this["OnCraft"] = value; } }

        public BlueprintComponent()
        {
            this.OnCraft = (actor, product) =>
            {
                //actor.PostMessage(Message.Types.SkillAward, product, Skill.Types.Crafting, 1);
                throw new NotImplementedException();
            };
            Blueprint = null;// new Blueprint();
            this[Stat.Stage.Name] = 0;
        }

        static private BlueprintCollection GenerateBlueprints()
        {
            BlueprintCollection bps = new BlueprintCollection();
            //bps.Add(new Blueprint(GameObject.Types.Workbench, new Dictionary<GameObject.Types, int>() { { GameObject.Types.WoodenPlank, 3 } }));
            //bps.Add(new Blueprint(GameObject.Types.Soil, new Dictionary<GameObject.Types, int>() { { GameObject.Types.Soilbag, 1 } }));
            //bps.Add(new Blueprint(GameObject.Types.WoodenDeck, new Dictionary<GameObject.Types, int>() { { GameObject.Types.WoodenPlank, 2 } }));
            //bps.Add(new Blueprint(GameObject.Types.Wall, new Dictionary<GameObject.Types, int>() { { GameObject.Types.Log, 1 } }));

            //bps.Add(new Blueprint(GameObject.Types.Workbench, new BlueprintStage() { { GameObject.Types.WoodenPlank, 2 } }));
            //bps.Add(new Blueprint(GameObject.Types.Soil, new BlueprintStage() { { GameObject.Types.Soilbag, 1 } }));
            bps.Add(new Blueprint(GameObject.Types.Cobblestone, new BlueprintStage() { { GameObject.Types.Stone, 4 } }));
            //bps.Add(new Blueprint(GameObject.Types.WoodenDeck, new BlueprintStage() { { GameObject.Types.WoodenPlank, 1 } }));
            //bps.Add(new Blueprint(GameObject.Types.Scaffolding, new BlueprintStage() { { GameObject.Types.WoodenPlank, 1 } }));

            //bps.Add(new Blueprint(GameObject.Types.Campfire, new BlueprintStage() { { GameObject.Types.Log, 1 } }, new BlueprintStage() { { GameObject.Types.Coal, 1 } }));
            //bps.Add(new Blueprint(GameObject.Types.Bed, new BlueprintStage() {  { GameObject.Types.FurnitureParts, 1 } }));//{ GameObject.Types.Log, 1 } }, new BlueprintStage() { { GameObject.Types.WoodenPlank, 1 } }));
            //bps.Add(new Blueprint(GameObject.Types.Crate, new BlueprintStage() { { GameObject.Types.WoodenPlank, 3 } }));
            //bps.Add(new Blueprint(GameObject.Types.Door, new BlueprintStage() { { GameObject.Types.WoodenPlank, 2 } }));

            return bps;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            switch (e.Type)
            {
                case Message.Types.Crafted:
                    var materialsContainer = e.Parameters[0] as List<GameObjectSlot>;
                    //var targetContainer = e.Parameters[1] as List<GameObjectSlot>;
                    GameObjectSlot productSlot;
                    if (Craft(e.Sender, parent, materialsContainer, materialsContainer, out productSlot))
                        OnCraft(e.Sender, productSlot.Object);
                    return true;

                case Message.Types.Craft:
                    //GameObject product;
                    //this.Blueprint.Craft(new GameObjectSlotCollection(InventoryComponent.GetSlots(e.Sender)), out product);
                    //e.Sender.PostMessage(Message.Types.Refresh);

                    GameObject sender = e.Parameters[0] as GameObject;
                    GameObject product;
                    this.Blueprint.Craft(new GameObjectSlotCollection(InventoryComponent.GetSlots(sender)), out product);
                    //e.Sender.PostMessage(Message.Types.Refresh);

                    return true;

                //case Message.Types.CraftOnBench:
                //    GameObject craftsman = e.Parameters[0] as GameObject;
                //    GameObject bench = e.Sender;
                //   // this.Blueprint.Craft(craftsman, bench, out product);
                //    blueprintcp
                //    //e.Sender.PostMessage(Message.Types.Refresh);
                //    e.Sender.PostMessage(Message.Types.Receive, parent, product.ToSlot(), product);
                //    return true;

                default:
                    return true;
            }
        }

        public override object Clone()
        {
            return new BlueprintComponent()
                .SetValue("Blueprint", Blueprint)
                .SetValue("OnCraft", this.OnCraft);
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            if (Blueprint == null)
                return;

            tooltip.Controls.Add(new CraftingTooltip(GameObject.Objects[Blueprint.ProductID].ToSlot(), (from stage in this.Blueprint.Stages from mat in stage select new ItemRequirement(mat.Key, mat.Value, 0))) { Location = tooltip.Controls.BottomLeft });
            return;

            Panel materials = new Panel(tooltip.Controls.Last().BottomLeft);
            materials.AutoSize = true;
            foreach (BlueprintStage stage in Blueprint.Stages)
                foreach (KeyValuePair<GameObject.Types, int> material in stage)
                {
                    SlotWithText slot = new SlotWithText(materials.Controls.Count > 0 ? materials.Controls.Last().BottomLeft : Vector2.Zero).SetSlotText(material.Value.ToString());
                    slot.Tag = GameObject.Objects[material.Key].ToSlot();
                    materials.Controls.Add(slot);//.ToSlot(material.Value)));
                }
            //GroupBox box = new GroupBox(tooltip.Controls.Last().BottomLeft);
            Panel box = new Panel(materials.BottomLeft);//tooltip.Controls.Last().BottomLeft);
            box.AutoSize = true;
            GameObject.Objects[Blueprint.ProductID].GetTooltip(box);
            tooltip.Controls.Add(materials, box);


        }
        public override void GetActorTooltip(GameObject parent, GameObject actor, Tooltip tooltip)
        {
            base.GetActorTooltip(parent, actor, tooltip);
        }
        static public bool RequiresWorkbench(GameObject obj)
        {
            BlueprintComponent bp = obj["Blueprint"] as BlueprintComponent;
            foreach (var stage in bp.Blueprint.Stages)
                foreach (var mat in stage)
                    if ((int)GameObject.Objects[mat.Key]["Physics"]["Size"] > 0)
                        return true;
            return false;
        }

        static public bool MaterialsAvailable(GameObject obj, List<GameObjectSlot> materials)
        {
            BlueprintComponent bp;
            return MaterialsAvailable(obj, materials, out bp);
        }
        static public bool MaterialsAvailable(GameObject obj, List<GameObjectSlot> materials, out BlueprintComponent bp)
        {
         //   BlueprintComponent 
                bp = obj["Blueprint"] as BlueprintComponent;
            foreach (var stage in bp.Blueprint.Stages)
                foreach (var reqMat in stage)
                {
                    int amount = 0;
                    materials
                        .FindAll(foo=>foo.HasValue)
                        .ForEach(mat => amount += (mat.Object.ID == reqMat.Key ? mat.StackSize : 0));
                    if (amount < reqMat.Value)
                        return false;          
                }
            return true;
        }
        static public bool Craft(GameObject creator, GameObject blueprintObject, List<GameObjectSlot> container, out GameObjectSlot productSlot)
        {
            return Craft(creator, blueprintObject, container, container, out productSlot);
        }
        static public bool Craft(GameObject creator, GameObject blueprintObject, List<GameObjectSlot> materialsContainer, List<GameObjectSlot> productContainer, out GameObjectSlot productSlot)
        {
            BlueprintComponent bpComp;// = blueprintObject["Blueprint"] as BlueprintComponent;
            productSlot = GameObjectSlot.Empty;
            if (!MaterialsAvailable(blueprintObject, materialsContainer, out bpComp))
                return false;
            Blueprint bp = bpComp["Blueprint"] as Blueprint;
            foreach (var material in bp.Stages[0])
                materialsContainer.Remove(material.Key, material.Value);
            throw new NotImplementedException();
            //productSlot = GameObject.Create(bp.ProductID).PostMessage(Message.Types.Crafted, creator).ToSlot();
            productContainer.Insert(productSlot);
            
            return true;
        }
    }
}
