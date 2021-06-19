using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.UI;

namespace Start_a_Town_.Modules.Construction.UI
{
    class StructureWindow : Window
    {
        static StructureWindow _Instance;
        public static StructureWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new StructureWindow();
                return _Instance;
            }
        }

        ListBox<Reaction.Product.ProductMaterialPair, Button> List_Variations = new ListBox<Reaction.Product.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
        Panel Panel_Selected;
        Panel Panel_Reagents;
        Button Btn_Build;

        protected StructureWindow()
        {
            this.Title = "Constructions";
            this.AutoSize = true;
            this.Movable = true;

            var list = new ListBox<Reaction, Button>(new Rectangle(0, 0, 150, 200));
            this.List_Variations = new ListBox<Reaction.Product.ProductMaterialPair, Button>(new Rectangle(0, 0, 150, 200));
            Panel_Selected = new Panel();

            Panel_Reagents = new Panel() { };
            Panel_Reagents.ClientSize = list.Size;

            List<GameObjectSlot> matSlots = new List<GameObjectSlot>();
            Reaction selected = null;
            Action refreshBpList = () =>
            {
                list.Build(GetAvailableBlueprints(), foo => foo.Name, (r, btn) =>
                {
                    btn.LeftClickAction = () =>
                    {
                        selected = r;
                        RefreshMaterialPicking(r);
                        //RefreshProductVariants(Panel_Reagents, Panel_Selected, r, matSlots);
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
            Panel_Reagents.Controls.Add(this.List_Variations);
            Panel_Reagents.Location = panel_List.TopRight;


            Panel_Selected.Location = panel_List.BottomLeft;

            Panel_Selected.Size = new Rectangle(0, 0, panel_List.Size.Width + Panel_Reagents.Size.Width, Panel_Reagents.Size.Height);
            this.Btn_Build = new Button("Build", this.Panel_Selected.Width)
            {
                Location = this.Panel_Selected.BottomLeft,
                LeftClickAction = () =>
                {
                    Build();
                }
            };
            this.Client.Controls.Add(
                panel_filters, panel_List, Panel_Reagents, Panel_Selected, Btn_Build);//, btn_craft);
        }

        private void Build()
        {
            if (this.Panel_Selected.Tag == null)
                return;
            Reaction.Product.ProductMaterialPair product = this.Panel_Selected.Tag as Reaction.Product.ProductMaterialPair;
            if(product.IsNull())
                return;
            ScreenManager.CurrentScreen.ToolManager.ActiveTool = new StructureTool(product);
        }

        private void RefreshMaterialPicking(Reaction reaction)
        {
            this.Panel_Reagents.Controls.Clear();
            this.Panel_Reagents.Controls.Add(new Label(reaction.Name) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            this.Panel_Reagents.Controls.Add(new Label(this.Panel_Reagents.Controls.BottomLeft, "Materials") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            List<GameObjectSlot> mats = new List<GameObjectSlot>();
            foreach (var reagent in reaction.Reagents)
            {
                GameObjectSlot matSlot = GameObjectSlot.Empty;
                matSlot.Name = reagent.Name;
                mats.Add(matSlot);
                Slot slot = new Slot(this.Panel_Reagents.Controls.BottomLeft) { Tag = matSlot, CustomTooltip = true };
                slot.HoverFunc = () => { string t = ""; foreach (var filter in reagent.Conditions) { t += filter.ToString(); } return t.TrimEnd('\n'); };//
                Label matName = new Label(slot.TopRight, reagent.Name);
                slot.LeftClickAction = () =>
                {
                    //MaterialPicker picker = new MaterialPicker(reagent, o => { slot.Tag.Object = o; });
                    MaterialPicker.Instance.Show(UIManager.Mouse, reagent, o => { slot.Tag.Object = o; });

                    //MaterialPicker.Instance.Refresh(reagent, o => { slot.Tag.Object = o; });
                    //MaterialPicker.Instance.Location = UIManager.Mouse;
                    //MaterialPicker.Instance.Show();
                };
                slot.RightClickAction = () =>
                {
                    slot.Tag.Clear();
                };
                matSlot.Filter = reagent.Filter;
                matSlot.ObjectChanged = o =>
                {
                    if ((from sl in mats where !sl.HasValue select sl).FirstOrDefault() != null)
                    {
                        this.Panel_Selected.Controls.Clear();
                        return;
                    }
                    //RefreshSelectedPanel(reaction.Products.First().Create(reaction, mats), mats);
                    RefreshSelectedPanel(reaction.Products.First().GetProduct(reaction, mats));
                };
                this.Panel_Reagents.Controls.Add(slot, matName);
            }
        }

        private void RefreshSelectedPanel(GameObject product, List<GameObjectSlot> mats)
        {
            this.Panel_Selected.Controls.Clear();
            this.Panel_Selected.Tag = product;
            if (product.IsNull())
                return;

            List<ItemRequirement> reqs = new List<ItemRequirement>();
            foreach (var mat in mats)
                reqs.Add(new ItemRequirement(mat.Object.ID, 1));
            CraftingTooltip tip = new CraftingTooltip(product.ToSlot(), reqs);
            this.Panel_Selected.Controls.Add(tip);
        }

        protected virtual List<Reaction> GetAvailableBlueprints()
        {
            return (from reaction in Reaction.Dictionary.Values
                    //where reaction.Building == parent.ID
                    //where reaction.ValidWorkshops.Contains(Reaction.Site.World)
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

        private void RefreshProductVariants(Panel panelVariants, Panel panelSelected, Reaction reaction, List<GameObjectSlot> slots)
        {
            this.List_Variations.Build(reaction.Products.First().Create(reaction), p => p.Req.ObjectID.ToString(), (p, btn) =>
            {
                btn.LeftClickAction = () =>
                {
                    RefreshSelectedPanel(panelSelected, p);
                    ScreenManager.CurrentScreen.ToolManager.ActiveTool = new Start_a_Town_.PlayerControl.BuildingPlacer(p);
                };
            });
            return;
        }
        private void RefreshSelectedPanel(Reaction.Product.ProductMaterialPair product)
        {
            this.Panel_Selected.Controls.Clear();
            this.Panel_Selected.Tag = product;//.Product;
            if (product.IsNull())
                return;

            CraftingTooltip tip = new CraftingTooltip(product.Product.ToSlot(), product.Req);
            this.Panel_Selected.Controls.Add(tip);
            return;
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
