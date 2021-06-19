using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Materials;

namespace Start_a_Town_.UI
{
    class BuildingsWindow : Window
    {
        static BuildingsWindow _Instance;
        public static BuildingsWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new BuildingsWindow();
                return _Instance;
            }
        }

        ListBox<Reaction.Product.ProductMaterialPair, Button> List_Variations = new ListBox<Reaction.Product.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
        //ListBox<GameObject, Button> List_Variations = new ListBox<GameObject, Button>(new Rectangle(0, 0, 150, 200));

        BuildingsWindow()
        {
            //GroupBox box_bps = new GroupBox();
            //Panel panel_bplist = new Panel() { ClientDimensions = new Vector2(150 - BackgroundStyle.Panel.Border, 150) };// AutoSize = true };
            //Panel panel_blueprint = new Panel() { Location = panel_bplist.TopRight, ClientDimensions = panel_bplist.ClientDimensions };
            //panel_blueprint.Controls.Add(new Label() { Text = "No blueprint selected" });
            //ListBox<GameObject, Button> list_bps = new ListBox<GameObject, Button>(new Rectangle(0, 0, 150, 150));
            //List<GameObject> bplist;

            //box_bps.Controls.Add(panel_blueprint, panel_bplist);
            //Label lbl_materials = "Materials".ToLabel();
            //SlotGrid<SlotDefault> slots_mats = new SlotGrid<SlotDefault>(Slots, parent, 4) { Location = lbl_materials.BottomLeft };//
            //Label lbl_bps = "Stored Blueprints".ToLabel(slots_mats.BottomLeft);
            //SlotGrid<SlotDefault> slots_bps = new SlotGrid<SlotDefault>(this.BlueprintSlots, parent, 4) { Location = lbl_bps.BottomLeft };//
            this.Title = "Constructions";
            this.AutoSize = true;

            var list = new ListBox<Reaction, Button>(new Rectangle(0, 0, 150, 200));
            this.List_Variations = new ListBox<Reaction.Product.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
            //this.List_Variations = new ListBox<GameObject, Button>(new Rectangle(0, 0, 150, 200));
            Panel panel_Selected = new Panel();

            Panel panel_Reagents = new Panel() { };//AutoSize = true };
            panel_Reagents.ClientSize = list.Size;


            List<GameObjectSlot> matSlots = new List<GameObjectSlot>();
            Reaction selected = null;
            Action refreshBpList = () =>
            {
                list.Build(GetAvailableBlueprints(), foo => foo.Name, (r, btn) =>
                {
                    btn.LeftClickAction = () =>
                    {
                        selected = r;
                        RefreshProductVariants(panel_Reagents, panel_Selected, r, matSlots);
                    };
                });
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
                        //list.Build(GetAvailableBlueprints(parent).FindAll(foo => BlueprintComponent.MaterialsAvailable(foo, this.Slots)), foo => foo.Name, RecipeListControlInitializer(panel_Selected));
                    }
                };

            Panel panel_filters = new Panel() { Location = this.Client.Controls.BottomLeft, AutoSize = true };
            panel_filters.Controls.Add(rd_All, rd_MatsReady);

            Panel panel_List = new Panel() { Location = panel_filters.BottomLeft, AutoSize = true };
            panel_List.Controls.Add(list);
            panel_Reagents.Controls.Add(this.List_Variations);
            panel_Reagents.Location = panel_List.TopRight;


            panel_Selected.Location = panel_List.BottomLeft;

            panel_Selected.Size = new Rectangle(0, 0, panel_List.Size.Width + panel_Reagents.Size.Width, panel_Reagents.Size.Height);

            //Button btn_craft = new Button(panel_Selected.BottomLeft, panel_List.Width + panel_Reagents.Width, "Craft")
            //{
            //    LeftClickAction = () =>
            //    {
            //        if (list.SelectedItem.IsNull())
            //            return;

            //        Net.Client.PostPlayerInput(Message.Types.StartScript, w =>
            //        {
            //            Ability.Write(w, Script.Types.Reaction, new TargetArgs(parent), ww => (panel_Selected.Tag as GameObject).Write(ww));
            //        });
            //    }
            //};

            this.Client.Controls.Add(
                panel_filters, panel_List, panel_Reagents, panel_Selected);//, btn_craft);


            //handlers.Add(new EventHandler<Net.GameEvent>((sender, e) =>
            //{
            //    if ((e.Parameters[0] as GameObject) != parent)
            //        return;
            //    switch (e.Type)
            //    {
            //        case Message.Types.InventoryChanged:
            //            refreshBpList();
            //            RefreshSelectedPanel(panel_Selected, selected, matSlots, this.Slots);
            //            return;

            //        default:
            //            return;
            //    }
            //}));
        }



        private List<Reaction> GetAvailableBlueprints()
        {
            return (from reaction in Reaction.Dictionary.Values
                    ////where reaction.Building == parent.ID
                    //where reaction.ValidWorkshops.Contains(0)
                    where reaction.ValidWorkshops.Count == 0
                    select reaction).ToList();
        }
        private Action<Reaction, Button> RecipeListControlInitializer(Panel panel_Selected)
        {
            return (foo, btn) =>
            {
                btn.LeftClickAction = () =>
                {
                    Reaction obj = foo;
                    return;
                };
            };
        }
        //private void RefreshMaterialChoices(Panel panelVariants, Panel panelSelected, Reaction reaction, List<GameObjectSlot> slots)
        //{
        //    this.List_Variations.Build(reaction.Products.First().GetMaterialChoices(reaction), p => p.Name, (p, btn) =>
        //    {
        //        btn.LeftClickAction = () => RefreshSelectedPanel(panelSelected, p);// RefreshSelectedPanel(panelSelected, obj);
        //    });
        //    return;
        //}

        private void RefreshProductVariants(Panel panelVariants, Panel panelSelected, Reaction reaction, List<GameObjectSlot> slots)
        {
            //panelVariants.Controls.Clear();

            this.List_Variations.Build(reaction.Products.First().Create(reaction), p => p.Req.ObjectID.ToString(), (p, btn) =>
            {
                btn.LeftClickAction = () =>
                {
                    RefreshSelectedPanel(panelSelected, p);
                    ScreenManager.CurrentScreen.ToolManager.ActiveTool = new Start_a_Town_.PlayerControl.BuildingPlacer(p);
                };// RefreshSelectedPanel(panelSelected, obj);
            });
            return;

            // List<GameObjectSlot> slots = new List<GameObjectSlot>();
            panelVariants.Controls.Add(new Label(reaction.Name) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            panelVariants.Controls.Add(new Label(panelVariants.Controls.BottomLeft, "Materials") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            slots.Clear();
            foreach (var reagent in reaction.Reagents)
            {
                GameObjectSlot matSlot = GameObjectSlot.Empty;
                slots.Add(matSlot);
                Slot slot = new Slot(panelVariants.Controls.BottomLeft) { Tag = matSlot, CustomTooltip = true };
                slot.HoverFunc = () => { string t = ""; foreach (var filter in reagent.Conditions) { t += filter.ToString(); } return t.TrimEnd('\n'); };// reagent.Condition.ToString();
                Label matName = new Label(slot.TopRight, reagent.Name);
                slot.DragDropAction = a =>
                {
                    slot.Tag.Object = (a as DragDropSlot).Slot.Object;
                    return DragDropEffects.Link;
                };
                slot.RightClickAction = () =>
                {
                    slot.Tag.Clear();
                };
                matSlot.Filter = reagent.Filter;// reagent.Condition.Condition;
                matSlot.ObjectChanged = o =>
                {
                    if ((from sl in slots where !sl.HasValue select sl).FirstOrDefault() != null)
                    {
                        panelSelected.Controls.Clear();
                        return;
                    }

                    //RefreshSelectedPanel(panelSelected, reaction, slots, this.Slots);
                };
                panelVariants.Controls.Add(slot, matName);
            }
        }

        private void RefreshSelectedPanel(Panel panel_Selected, Reaction.Product.ProductMaterialPair product)// Reaction reaction)
        {
            panel_Selected.Controls.Clear();
            panel_Selected.Tag = product.Product;
            if (product.IsNull())
                return;

            CraftingTooltip tip = new CraftingTooltip(product.Product.ToSlot(), product.Req);
            panel_Selected.Controls.Add(tip);
            return;
        }
        private void RefreshSelectedPanel(Panel panel_Selected, Reaction reaction, List<GameObjectSlot> materials, List<GameObjectSlot> container)// Reaction reaction)
        {
            if (reaction.IsNull())
                return;
            var matList = (from s in materials select s.Object).ToList();
            List<ItemRequirement> reqs = new List<ItemRequirement>();
            foreach (var mat in matList)
            {
                int amount = container.GetAmount(obj => obj.ID == mat.ID);
                reqs.Add(new ItemRequirement(mat.ID, 1, amount));
            }
            //RefreshSelectedPanel(panel_Selected, reaction.Products.First().Create(reaction, matList), reqs);// (from s in materials select new ItemRequirement(s.Object.ID, 1)).ToList());
            RefreshSelectedPanel(panel_Selected, reaction.Products.First().Create(reaction, materials), reqs);// (from s in materials select new ItemRequirement(s.Object.ID, 1)).ToList());
        }

        private void RefreshSelectedPanel(Panel panel_Selected, GameObject product, List<ItemRequirement> materials)// Reaction reaction)
        {
            panel_Selected.Controls.Clear();
            panel_Selected.Tag = product;
            if (product.IsNull())
                return;

            CraftingTooltip tip = new CraftingTooltip(product.ToSlot(), materials);
            panel_Selected.Controls.Add(tip);
            return;
        }
    }
}
