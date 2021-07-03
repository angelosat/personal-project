using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Crafting
{
    class ReactionsInterface : GroupBox
    {
        BlockEntity Bench;
        IsWorkstation.Types WorkstationType;
        public Reaction SelectedReaction;
        ListBox<Reaction, Button> List_Recipes;
        List<GameObjectSlot> Slots_Reagents;

        public Panel Panel_bplist, Panel_blueprint, Panel_Selected;
        public ReagentPanel Panel_Reagents;

        public ReactionsInterface(BlockEntity parent, IsWorkstation.Types type, Vector3 workstationGlobal, List<GameObjectSlot> reagentSources, params Button[] extraButtons)
        {
            this.WorkstationType = type;
            this.Bench = parent;
            GroupBox box_bps = new GroupBox();
            this.Panel_bplist = new Panel() { ClientDimensions = new Vector2(150 - BackgroundStyle.Panel.Border, 150) };
            this.Panel_blueprint = new Panel() { Location = Panel_bplist.TopRight, ClientDimensions = Panel_bplist.ClientDimensions };
            this.Panel_blueprint.Controls.Add(new Label() { Text = "No blueprint selected" });
            ListBox<GameObject, Button> list_bps = new ListBox<GameObject, Button>(new Rectangle(0, 0, 150, 150));
            box_bps.Controls.Add(Panel_blueprint, Panel_bplist);
            this.List_Recipes = new ListBox<Reaction, Button>(new Rectangle(0, 0, 150, 200));
            Panel_Selected = new Panel();
            Panel_Reagents = new ReagentPanel() { ClientSize = List_Recipes.Size, Callback = this.RefreshSelectedPanel };
            this.Slots_Reagents = reagentSources;
            this.RefreshRecipes();

            RadioButton
                rd_All = new RadioButton("All", Vector2.Zero, true)
                {
                    LeftClickAction = () =>
                    {
                        this.RefreshRecipes();
                    }
                },
                rd_MatsReady = new RadioButton("Have Materials", rd_All.TopRight)
                {
                    LeftClickAction = () =>
                    {
                    }
                };
            Panel panel_filters = new Panel() { AutoSize = true };
            panel_filters.Controls.Add(rd_All, rd_MatsReady);

            Panel panel_List = new Panel() { Location = panel_filters.BottomLeft, AutoSize = true };
            panel_List.Controls.Add(List_Recipes);

            Panel_Reagents.Location = panel_List.TopRight;


            Panel_Selected.Location = panel_List.BottomLeft;

            Panel_Selected.Size = new Rectangle(0, 0, panel_List.Size.Width + Panel_Reagents.Size.Width, Panel_Reagents.Size.Height);

            this.Controls.Add(
                panel_filters, panel_List, Panel_Reagents, Panel_Selected);

            if (extraButtons.Length > 0)
            {
                var panelbuttons = new Panel() { Location = Panel_Selected.BottomLeft, AutoSize = true };
                foreach (var btn in extraButtons)
                {
                    btn.Width = Panel_Selected.ClientSize.Width;
                    btn.Location = panelbuttons.Controls.BottomLeft;
                    panelbuttons.Controls.Add(btn);
                    
                }
                this.Controls.Add(panelbuttons);
            }
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
                    this.RefreshSelectedPanel();
                    return;

                default:
                    return;
            }
        }

        void RefreshRecipes()
        {
            var reagentSlots = this.Slots_Reagents;
            this.List_Recipes.Build(Reaction.GetAvailableRecipes(this.WorkstationType), foo => foo.Name, (r, btn) =>
            {
                btn.LeftClickAction = () =>
                {
                    this.SelectedReaction = r;
                    this.Panel_Reagents.Refresh(r, reagentSlots);
                    this.Panel_Selected.Controls.Clear();
                };
            });
        }

        private void RefreshSelectedPanel()
        {
            var product = this.Panel_Selected.Tag as Reaction.Product.ProductMaterialPair;
            this.SelectedProduct = product;
            if (product == null)
                return;
            this.RefreshSelectedPanel(product);
        }
        private void RefreshSelectedPanel(Reaction.Product.ProductMaterialPair product)
        {
            this.Panel_Selected.Controls.Clear();
            this.Panel_Selected.Tag = product;
            this.SelectedProduct = product;
            if (product == null)
                return;
            foreach (var mat in product.Requirements)
            {
                int amount = this.Slots_Reagents.Count(obj => (int)obj.ID == mat.ObjectID);
                mat.AmountCurrent = amount;
            }
            CraftingTooltip tip = new CraftingTooltip(product.Product.ToSlotLink(), product.Requirements);
            this.Panel_Selected.Controls.Add(tip);
        }

        public Reaction.Product.ProductMaterialPair SelectedProduct { get; set; }
    }
}
