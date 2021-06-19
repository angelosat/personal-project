using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class ScribeComponent : Component
    {
        public override string ComponentName
        {
            get { return "Scribe"; }
        }

        byte MatCapacity, ProdCapacity, BlueprintCapacity;
        Func<GameObject, bool> MaterialFilter;
        Func<GameObject, bool> BlueprintFilter;
        ItemContainer Materials { get { return (ItemContainer)this["Materials"]; } set { this["Materials"] = value; } }
        ItemContainer Products { get { return (ItemContainer)this["Products"]; } set { this["Products"] = value; } }
        ItemContainer Blueprints { get { return (ItemContainer)this["Blueprints"]; } set { this["Blueprints"] = value; } }

        //public ScribeComponent Initialize(byte materialCapacity, byte productCapacity, byte bpCapacity, Func<GameObject, bool> matFilter, Func<GameObject, bool> bpFilter)
        public ScribeComponent Initialize(byte materialCapacity, byte productCapacity, byte bpCapacity, Func<GameObject, bool> matFilter, Func<GameObject, bool> bpFilter)
        {
            this.MatCapacity = materialCapacity;
            this.ProdCapacity = productCapacity;
            this.BlueprintCapacity = bpCapacity;
            this.MaterialFilter = matFilter;
            this.BlueprintFilter = bpFilter;
            return this;
        }

        public override void ComponentsCreated(GameObject parent) //MakeChildOf(GameObject parent)// 
        {
            this.Materials = new ItemContainer(parent, this.MatCapacity, this.MaterialFilter);
            this.Products = new ItemContainer(parent, this.ProdCapacity);
            this.Blueprints = new ItemContainer(parent, this.BlueprintCapacity, this.BlueprintFilter);
        }

        public override void GetChildren(List<GameObjectSlot> list)
        {
            list.AddRange(this.Materials);
            list.AddRange(this.Products);
            list.AddRange(this.Blueprints);
        }

        public ScribeComponent()
        {

        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Craft://Object:
                    GameObject.Types bpID = (GameObject.Types)e.Parameters[0];
                    GameObject crafter = e.Parameters[1] as GameObject;
                    GameObject bp = GameObject.Objects[bpID];
                    this.Craft(e.Network, parent, crafter, bp);
                    return true;

                default:
                    return true;
            }
        }

        private void Craft(Net.IObjectProvider net, GameObject parent, GameObject crafter, GameObject bp)
        {
            //if (!BlueprintComponent.MaterialsAvailable(bp, this.Materials))
            //    return;
            if (!Blueprint.CheckMaterials(this.Materials, Blueprint.BlueprintMaterials))
                return;
    
            //BlueprintComponent bpComp = bp.GetComponent<BlueprintComponent>();
            GameObject pr;
            //bpComp.Blueprint.Craft(net, parent, this.Materials, parent, this.Materials, out pr);
            Blueprint.Copy(net, bp.ID, parent, this.Materials, parent, this.Materials, out pr);
            // award crafter with exp to crafting skill and knowledge of specific blueprint
            //crafter.GetComponent<KnowledgeComponent>().Blueprints.Gain(crafter, bp, 1);
        }

        public override object Clone()
        {
            return new ScribeComponent().Initialize(this.MatCapacity, this.ProdCapacity, this.BlueprintCapacity, this.MaterialFilter, this.BlueprintFilter);
        }

        public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<GameEvent>> handlers)
        {
            GroupBox box_bps = new GroupBox();
            Panel panel_bplist = new Panel() { ClientDimensions = new Vector2(150 - BackgroundStyle.Panel.Border, 150) };// AutoSize = true };
            Panel panel_blueprint = new Panel() { Location = panel_bplist.TopRight, ClientDimensions = panel_bplist.ClientDimensions };
            panel_blueprint.Controls.Add(new Label() { Text = "No blueprint selected" });
            ListBox<GameObject, Button> list_bps = new ListBox<GameObject, Button>(new Rectangle(0, 0, 150, 150));

            box_bps.Controls.Add(panel_blueprint, panel_bplist);
            //Label lbl_materials = "Materials".ToLabel();
            Panel pnl_materials = new Panel() { AutoSize = true };
            Label lbl_materials = new Label("Materials") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            SlotGrid<SlotDefault> slots_mats = new SlotGrid<SlotDefault>(this.Materials, 4) { Location = lbl_materials.BottomLeft };//
            pnl_materials.Controls.Add(lbl_materials, slots_mats);
            //Label lbl_bps = "Stored Blueprints".ToLabel(slots_mats.BottomLeft);
            Panel pnl_bps = new Panel(pnl_materials.BottomLeft) { AutoSize = true };
            Label lbl_bps = new Label("Stored Blueprints") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            SlotGrid<SlotDefault> slots_bps = new SlotGrid<SlotDefault>(this.Blueprints, 4) { Location = lbl_bps.BottomLeft };//
            pnl_bps.AddControls(lbl_bps, slots_bps);

            var list = new ListBox<GameObject, Button>(new Rectangle(0, 0, 300, 200));
            Panel panel_Selected = new Panel();

            Action refreshBpList = () =>
            {
                list.Build(GetAvailableBlueprints(), foo => foo.Name,
                        RecipeListControlInitializer(panel_Selected));
            };
            refreshBpList();


            RadioButton
                rd_All = new RadioButton("All", Vector2.Zero, true)
                {
                    LeftClickAction = () =>
                    {
                        refreshBpList();
                    }
                },
                rd_MatsReady = new RadioButton("Have Materials", rd_All.TopRight)
                {
                    LeftClickAction = () =>
                    {
                        list.Build(GetAvailableBlueprints().FindAll(foo => BlueprintComponent.MaterialsAvailable(foo, this.Materials)), foo => foo.Name, RecipeListControlInitializer(panel_Selected));
                    }
                };
            Panel panel_filters = new Panel() { Location = pnl_bps.BottomLeft, AutoSize = true };
            panel_filters.Controls.Add(rd_All, rd_MatsReady);

            rd_All.PerformLeftClick();

            Panel panel_List = new Panel() { Location = panel_filters.BottomLeft, AutoSize = true };
            panel_List.Controls.Add(list);

            panel_Selected.Location = panel_List.BottomLeft;
            panel_Selected.Size = panel_List.Size;

            Button btn_craft = new Button(panel_Selected.BottomLeft, panel_List.Width, "Craft")
            {
                LeftClickAction = () =>
                {
                    if (list.SelectedItem.IsNull())
                        return;

                    Net.Client.PostPlayerInput(Message.Types.StartScript, w =>
                    {
                        Ability.Write(w, Script.Types.CraftingBench, new TargetArgs(parent), BitConverter.GetBytes((int)list.SelectedItem.ID));
                        //Ability.Write(w, Script.Types.CraftingWorkbench, new TargetArgs(parent), BitConverter.GetBytes((int)list.SelectedItem.ID));
                    });
                }
            };

            ui.Controls.Add(
                pnl_materials,
                pnl_bps,
                //lbl_materials,
                //slots_mats,
                //lbl_bps, slots_bps,
                panel_filters, panel_List, panel_Selected, btn_craft);

            handlers.Add(new EventHandler<GameEvent>((sender, e) =>
            {
                if ((e.Parameters[0] as GameObject) != parent)
                    return;
                switch (e.Type)
                {
                    case Message.Types.InventoryChanged:
                        refreshBpList();
                        RefreshSelectedPanel(panel_Selected, list.SelectedItem);
                        return;

                    default:
                        return;
                }
            }));
        }

        private List<GameObject> GetAvailableBlueprints()
        {
            List<GameObject> recipes = KnowledgeComponent.GetMemorizedBlueprints(Player.Actor);

            List<GameObjectSlot> storedBps = this.Blueprints.Where(o => o.HasValue).ToList();
            List<GameObject> finalList = recipes;
            var distinctBps = (from bp in storedBps
                               where recipes.FirstOrDefault(r => r.ID == bp.Object.ID).IsNull()
                               select bp.Object);
            finalList.AddRange(distinctBps);
            return finalList;
        }

        private Action<GameObject, Button> RecipeListControlInitializer(Panel panel_Selected)
        {
            return (foo, btn) =>
            {
                btn.LeftClickAction = () =>
                {
                    GameObject obj = foo;
                    RefreshSelectedPanel(panel_Selected, obj);
                    return;
                };
            };
        }

        private void RefreshSelectedPanel(Panel panel_Selected, GameObject obj)
        {
            panel_Selected.Controls.Clear();
            if (obj.IsNull())
                return;
            //Blueprint bp = obj["Blueprint"]["Blueprint"] as Blueprint;
            GameObject product = GameObject.Objects[obj.ID];//bp.ProductID];

            panel_Selected.Controls.Add("Product:".ToLabel(panel_Selected.Controls.BottomLeft));
            //panel_Selected.Controls.Add(new SlotWithText(panel_Selected.Controls.BottomLeft) { Tag = product.ToSlot() });
            Slot slot = new Slot(panel_Selected.Controls.BottomLeft) { Tag = product.ToSlot() };
            var info = new GroupBox(slot.TopRight);
            product.GetInfo().GetTooltip(product, info);
            panel_Selected.Controls.Add(slot, info);
            panel_Selected.Controls.Add("Materials:".ToLabel(panel_Selected.Controls.BottomLeft));

            var mats = new Dictionary<GameObject.Types, int>() { { GameObject.Types.Paper, 1 } };
            foreach (var mat in mats)//bp.Stages[0])
            {
                SlotWithText matSlot = new SlotWithText(panel_Selected.Controls.BottomLeft) { Tag = GameObject.Objects[mat.Key].ToSlot() };
                int amount = 0;
                this.Materials
                    .FindAll(s => s.HasValue)
                    .FindAll(s => s.Object.ID == mat.Key)
                    .ForEach(s => amount += s.StackSize);
                matSlot.Slot.CornerTextFunc = s => (amount.ToString() + "/" + mat.Value.ToString());
                panel_Selected.Controls.Add(matSlot);
            }
        }

    }
}
