﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Net;
using Start_a_Town_.Crafting;

namespace Start_a_Town_.Blocks
{
    class WorkbenchInterface : GroupBox
    {
        //BlockWorkbench.Entity BenchEntity;
        //WorkbenchReactionComponent Bench;
        BlockWorkbenchEntity Bench;
        Vector3 WorkstationGlobal;

        //InterfaceWorkbenchOrders QueuedOrdersUI;
        //WorkstationOrdersUI QueuedOrdersUI;

        Reaction SelectedReaction;
        ListBox<Reaction, Button> List_Recipes;
        List<GameObjectSlot> Slots_Reagents;

        Panel Panel_bplist, Panel_blueprint, Panel_Selected;
        ReagentPanel Panel_Reagents;

        public WorkbenchInterface(BlockWorkbenchEntity parent, Vector3 workstationGlobal)
        {
            //this.BenchEntity = parent;
            //this.Bench = parent.GetComponent<WorkbenchReactionComponent>();
            this.Bench = parent;
            this.WorkstationGlobal = workstationGlobal;

            GroupBox box_bps = new GroupBox();
            this.Panel_bplist = new Panel() { ClientDimensions = new Vector2(150 - BackgroundStyle.Panel.Border, 150) };// AutoSize = true };
            this.Panel_blueprint = new Panel() { Location = Panel_bplist.TopRight, ClientDimensions = Panel_bplist.ClientDimensions };
            this.Panel_blueprint.Controls.Add(new Label() { Text = "No blueprint selected" });
            ListBox<GameObject, Button> list_bps = new ListBox<GameObject, Button>(new Rectangle(0, 0, 150, 150));
            //List<GameObject> bplist;

            box_bps.Controls.Add(Panel_blueprint, Panel_bplist);
            Label lbl_materials = "Materials".ToLabel();
            //SlotGrid<SlotDefault> slots_mats = new SlotGrid<SlotDefault>(this.Bench.Slots, 4) { Location = lbl_materials.BottomLeft };//
            SlotGrid<SlotDefault> slots_mats = new SlotGrid<SlotDefault>(this.Bench.Storage.Slots, 4) { Location = lbl_materials.BottomLeft };//

            Label lbl_bps = "Stored Blueprints".ToLabel(slots_mats.BottomLeft);
            //SlotGrid<SlotDefault> slots_bps = new SlotGrid<SlotDefault>(this.Bench.BlueprintSlots, 4) { Location = lbl_bps.BottomLeft };//
            SlotGrid<SlotDefault> slots_bps = new SlotGrid<SlotDefault>(this.Bench.Blueprints.Slots, 4) { Location = lbl_bps.BottomLeft };//

            this.List_Recipes = new ListBox<Reaction, Button>(new Rectangle(0, 0, 150, 200));

            Panel_Selected = new Panel();

            Panel_Reagents = new ReagentPanel() { ClientSize = List_Recipes.Size, Callback = this.RefreshSelectedPanel };
            //Panel_Reagents.ClientSize = List_Recipes.Size;

            this.Slots_Reagents = new List<GameObjectSlot>();

            this.RefreshRecipes();

            RadioButton
                rd_All = new RadioButton("All", Vector2.Zero, true)
                {
                    LeftClickAction = () =>
                    {
                        //refreshBpList();
                        this.RefreshRecipes();
                    }
                },
                rd_MatsReady = new RadioButton("Have Materials", rd_All.TopRight)
                {
                    LeftClickAction = () =>
                    {
                        //list.Build(GetAvailableBlueprints(parent).FindAll(foo => BlueprintComponent.MaterialsAvailable(foo, this.Slots)), foo => foo.Name, RecipeListControlInitializer(panel_Selected));
                    }
                };
            Panel panel_filters = new Panel() { Location = slots_bps.BottomLeft, AutoSize = true };
            panel_filters.Controls.Add(rd_All, rd_MatsReady);

            Panel panel_List = new Panel() { Location = panel_filters.BottomLeft, AutoSize = true };
            panel_List.Controls.Add(List_Recipes);

            Panel_Reagents.Location = panel_List.TopRight;


            Panel_Selected.Location = panel_List.BottomLeft;

            Panel_Selected.Size = new Rectangle(0, 0, panel_List.Size.Width + Panel_Reagents.Size.Width, Panel_Reagents.Size.Height);

            Button btn_craft = new Button(Panel_Selected.BottomLeft, panel_List.Width + Panel_Reagents.Width, "Craft")
            {
                LeftClickAction = () =>
                {
                    Craft();
                }
            };

            Button btn_order = new Button("Order") { Location = btn_craft.BottomLeft, LeftClickAction = PlaceOrder };
            Button btn_vieworders = new Button("View Orders") { Location = btn_order.TopRight, LeftClickAction = ViewOrders };

            //this.QueuedOrdersUI = new WorkstationOrdersUI(this.WorkstationGlobal, parent);// new InterfaceWorkbenchOrders(this.WorkstationGlobal, parent);
            var window = new Window();
            window.Title = "Orders";
            window.AutoSize = true;
            window.Movable = true;
            //window.Client.AddControls(this.QueuedOrdersUI);

            this.Controls.Add(
                lbl_materials,
                slots_mats,
                lbl_bps, slots_bps,
                panel_filters, panel_List, Panel_Reagents, Panel_Selected, btn_craft, btn_order, btn_vieworders);

            //Client.Instance.GameEvent += InventoryChanged;

        }

        private void ViewOrders()
        {
            //this.QueuedOrdersUI.Refresh(this.WorkstationGlobal, this.Bench);
            //var win = this.QueuedOrdersUI.GetWindow();
            //win.Location = this.GetWindow().TopRight;
            //win.Toggle();
        }

        private void Craft()
        {
            //if (list.SelectedItem.IsNull())
            if (this.SelectedReaction == null)
                return;

            Reaction.Product.ProductMaterialPair product = this.Panel_Selected.Tag as Reaction.Product.ProductMaterialPair;
            if (product == null)
                return;

            //throw new NotImplementedException();
            // TODO: make crafting not depend on a gameobject as a container parent
            //Client.PlayerCraftRequest(new Components.Crafting.CraftOperation(this.SelectedReaction.ID, product.Requirements, null, product.Tool, this.Bench.Storage));
            Client.PlayerCraftRequest(new Components.Crafting.CraftOperation(this.SelectedReaction.ID, product.Requirements, this.WorkstationGlobal));

            return;
        }
        public void PlaceOrder()
        {
            Reaction.Product.ProductMaterialPair product = this.Panel_Selected.Tag as Reaction.Product.ProductMaterialPair;
            if (product == null)
                return;
            Player.Actor.Map.GetTown().CraftingManager.PlaceOrder(this.SelectedReaction.ID, product.Requirements, this.WorkstationGlobal);
        }

        internal override void OnGameEvent(GameEvent e)
        {
            this.InventoryChanged(null, e);
        }
        void InventoryChanged(object sender, GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.InventoryChanged:
                    if ((e.Parameters[0] as BlockEntity) != this.Bench)
                        return;
                    this.RefreshRecipes();
                    //RefreshSelectedPanel(this.SelectedReaction, this.Slots_Reagents, this.Bench.Slots);
                    this.RefreshSelectedPanel();
                    return;

                default:
                    return;
            }
        }

        void RefreshRecipes()
        {
            var reagentSlots = this.Bench.GetReagentSlots(Player.Actor);//
            //this.List_Recipes.Build(GetAvailableBlueprints(), foo => foo.Name, (r, btn) =>
            this.List_Recipes.Build(Reaction.GetAvailableRecipes(Tokens.IsWorkstation.Types.Workbench), foo => foo.Name, (r, btn) =>
            {
                btn.LeftClickAction = () =>
                {
                    this.SelectedReaction = r;
                    this.Panel_Reagents.Refresh(r, reagentSlots);
                    //RefreshReagents(r, this.Slots_Reagents);
                    this.Panel_Selected.Controls.Clear();
                };
            });
        }

        //private List<Reaction> GetAvailableBlueprints()
        //{
        //    return (from reaction in Reaction.Dictionary.Values
        //            //where reaction.Sites.Contains(this.BenchEntity.GetInfo().ID)
        //            where reaction.ValidWorkshops.Contains(Block.Workbench)
        //            select reaction).ToList();
        //}



        //private void RefreshReagents(Reaction reaction, List<GameObjectSlot> slots)
        //{
        //    this.Panel_Reagents.Controls.Clear();
        //    // List<GameObjectSlot> slots = new List<GameObjectSlot>();
        //    this.Panel_Reagents.Controls.Add(new Label(reaction.Name) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
        //    this.Panel_Reagents.Controls.Add(new Label(this.Panel_Reagents.Controls.BottomLeft, "Materials") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
        //    slots.Clear();
        //    foreach (var reagent in reaction.Reagents)
        //    {
        //        GameObjectSlot matSlot = GameObjectSlot.Empty;
        //        matSlot.Name = reagent.Name;
        //        slots.Add(matSlot);
        //        Slot slot = new Slot(this.Panel_Reagents.Controls.BottomLeft) { Tag = matSlot, CustomTooltip = true };
        //        //slot.HoverFunc = () => reagent.Condition.ToString();
        //        slot.HoverFunc = () => { string t = ""; foreach (var filter in reagent.Conditions) { t += filter.ToString(); } return t.TrimEnd('\n'); };//
        //        Label matName = new Label(slot.TopRight, reagent.Name);
        //        slot.LeftClickAction = () =>
        //        {
        //            MaterialPicker.Instance.Label.Text = reagent.Name + " Material";
        //            MaterialPicker.Instance.Show(UIManager.Mouse, reagent, o => { slot.Tag.Object = o; });
        //        };
        //        slot.DragDropAction = a =>
        //        {
        //            slot.Tag.Object = (a as DragDropSlot).Slot.Object;
        //            return DragDropEffects.Move;// Link;
        //        };
        //        slot.RightClickAction = () =>
        //        {
        //            slot.Tag.Clear();
        //        };
        //        matSlot.Filter = reagent.Pass;// reagent.Condition.Condition;
        //        matSlot.ObjectChanged = o =>
        //        {
        //            if ((from sl in slots where !sl.HasValue select sl).FirstOrDefault() != null)
        //            {
        //                this.Panel_Selected.Controls.Clear();
        //                return;
        //            }
        //            //RefreshSelectedPanel(panelSelected, reaction.Products.First().Create(reaction, (from s in slots select s.Object).ToList()), (from s in slots select new ItemRequirement(s.Object.ID, 1)).ToList());
        //            RefreshSelectedPanel(reaction, slots, this.Bench.Slots);
        //        };
        //        this.Panel_Reagents.Controls.Add(slot, matName);
        //    }
        //}

        //private void RefreshSelectedPanel(Reaction reaction, List<GameObjectSlot> materials, List<GameObjectSlot> container)// Reaction reaction)
        //{
        //    if (reaction.IsNull())
        //        return;
        //    var matList = (from s in materials where s.HasValue select s.Object).ToList();
        //    if (matList.Count < materials.Count)
        //    {
        //        this.Panel_Selected.Controls.Clear();
        //        return;
        //    }
        //    List<ItemRequirement> reqs = new List<ItemRequirement>();
        //    foreach (var mat in matList)
        //    {
        //        int amount = container.GetAmount(obj => obj.ID == mat.ID);
        //        reqs.Add(new ItemRequirement(mat.ID, 1, amount));
        //    }

        //    var product = reaction.Products.First().Create(reaction, materials);

        //    this.Panel_Selected.Controls.Clear();
        //    this.Panel_Selected.Tag = product;
        //    if (product.IsNull())
        //        return;

        //    CraftingTooltip tip = new CraftingTooltip(product.ToSlot(), reqs);
        //    this.Panel_Selected.Controls.Add(tip);
        //}

        private void RefreshSelectedPanel()
        {
            var product = this.Panel_Selected.Tag as Reaction.Product.ProductMaterialPair;
            if (product == null)
                return;
            this.RefreshSelectedPanel(product);
        }
        private void RefreshSelectedPanel(Reaction.Product.ProductMaterialPair product)
        {
            this.Panel_Selected.Controls.Clear();
            this.Panel_Selected.Tag = product;
            if (product == null)
                return;
            foreach (var mat in product.Requirements)
            {
                //int amount = this.Bench.Slots.GetAmount((GameObject obj) => (int)obj.ID == mat.ObjectID);
                int amount = this.Bench.Storage.Count(obj => (int)obj.IDType == mat.ObjectID);
                mat.AmountCurrent = amount;
            }
            CraftingTooltip tip = new CraftingTooltip(product.Product.ToSlotLink(), product.Requirements);
            this.Panel_Selected.Controls.Add(tip);
        }

    }
}
