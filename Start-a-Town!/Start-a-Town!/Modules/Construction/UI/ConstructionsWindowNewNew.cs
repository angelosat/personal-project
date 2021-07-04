using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;
using Start_a_Town_.Towns.Constructions;

namespace Start_a_Town_.Modules.Construction.UI
{
    class ConstructionsWindowNewNew : Window
    {
        static ConstructionsWindowNewNew CurrentlyOpen;
        public override bool Show()
        {
            var result = base.Show();
            if (result)
            {
                if (CurrentlyOpen != null)
                {
                    this.Location = CurrentlyOpen.Location;
                    CurrentlyOpen.Hide();
                }
                CurrentlyOpen = this;
            }
            else
                CurrentlyOpen = null;
            return result;
        }
        List<BlockRecipe> PopulateList()
        {
            return
                (from block in Block.Registry
                 let recipe = block.Value.GetRecipe()
                 where recipe != null
                 select recipe).ToList();
        }

        ListBox<BlockRecipe, Button> List_Constructions = new ListBox<BlockRecipe, Button>(new Rectangle(0, 0, 150, 200));

        Panel Panel_Reagents;
        Panel Panel_Selected;
        public ConstructionsWindowNewNew(ConstructionCategory cat)
        {
            this.Title = "Constructions";
            this.AutoSize = true;
            this.Movable = true;

            Panel_Selected = new Panel();

            Panel_Reagents = new Panel() { };
            Panel_Reagents.ClientSize = this.List_Constructions.Size;

            this.List_Constructions.Build(
                    cat.List,
                    foo => foo.Name, (tag, btn) =>
                    {
                        ListBoxNew<BlockRecipe.ProductMaterialPair, Label> reagentsList = new ListBoxNew<BlockRecipe.ProductMaterialPair, Label>();
                        reagentsList.Build(tag.GetVariants(), item => item.Req.ObjectID.GetObject().Name, (item, ctrl) => { });
                        reagentsList.SelectItem(0);

                        btn.Tag = reagentsList;
                        btn.LeftClickAction = () =>
                        {
                            VariationInitializer(tag, btn);

                            var windowtools = cat.GetPanelTools(() => reagentsList.SelectedItem);
                            windowtools.Location = this.BottomLeft;
                            windowtools.Show();
                        };
                    });

            Panel panel_List = new Panel() { AutoSize = true };

            panel_List.Controls.Add(this.List_Constructions);
            Panel_Reagents.Location = panel_List.BottomLeft;// panel_List.TopRight;


            Panel_Selected.Location = panel_List.BottomLeft;

            Panel_Selected.Size = new Rectangle(0, 0, panel_List.Size.Width + Panel_Reagents.Size.Width, Panel_Reagents.Size.Height);

            this.Client.Controls.Add(
                panel_List, Panel_Reagents
                );
        }
        void VariationInitializer(BlockRecipe constr, Button btn)
        {
            var list = btn.Tag as ListBoxNew<BlockRecipe.ProductMaterialPair, Label>;
            Panel_Reagents.ClearControls();
            Panel_Reagents.AddControls(list);
        }
        private void RefreshSelectedPanel(Panel panel_Selected, GameObject product, List<ItemRequirement> materials)// Reaction reaction)
        {
            panel_Selected.Controls.Clear();
            panel_Selected.Tag = product;
            if (product.IsNull())
                return;

            CraftingTooltip tip = new CraftingTooltip(product.ToSlotLink(), materials);
            panel_Selected.Controls.Add(tip);
            return;
        }

        public ConstructionsWindowNewNew Refresh(ConstructionCategory cat)
        {
            if (this.IsOpen && this.List_Constructions.List == cat.List)
            {
                // if window already open with the same list, close it
                this.Hide();
                return this;
            }

            this.List_Constructions.Build(
                    cat.List,
                    foo => foo.Name, (tag, btn) =>
                    {
                        btn.LeftClickAction = () => VariationInitializer(tag, btn);
                    });
            if (!this.IsOpen)
                this.ToggleSmart();
            return this;
        }
        public ConstructionsWindowNewNew Refresh(IEnumerable<BlockRecipe> constrs)
        {
            if (this.IsOpen && this.List_Constructions.List == constrs)
            {
                // if window already open with the same list, close it
                this.Hide();
                return this;
            }

            this.List_Constructions.Build(
                    constrs,
                    foo => foo.Name, (tag, btn) =>
                    {
                        btn.LeftClickAction = () => VariationInitializer(tag, btn);
                    });
            if (!this.IsOpen)
                this.ToggleSmart();
            return this;
        }
        public override bool Hide()
        {
            if (ConstructionCategory.PanelTools!=null)
            ConstructionCategory.PanelTools.Hide();
            return base.Hide();
        }
    }
}
