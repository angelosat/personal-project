using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Modules.Crafting;
using Start_a_Town_.Towns.Crafting;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Crafting
{
    class CraftOrderDetailsInterface : ScrollableBoxNewNew
    {
        public Action<Reaction.Product.ProductMaterialPair> Callback = a => { };
        readonly Panel PanelParts, PanelItemTypes, PanelMaterials, PanelCollapsible;
        readonly Dictionary<Reaction.Reagent, ListBoxNoScroll<ItemDef, CheckBoxNew>> CachedReagentLists = new();
        readonly Dictionary<Reaction.Reagent, ListBoxNoScroll<MaterialDef, CheckBoxNew>> CachedReagentMaterialLists = new();
        readonly CheckBoxNew ChkHaulOnFinish;
        readonly CraftOrder Order;
        public CraftOrderDetailsInterface(CraftOrder order)
            : base(70, 150)
        {
            this.Order = order;
            this.AutoSize = true;

            this.PanelParts = new Panel() { AutoSize = true };
            var reagents = order.Reaction.Reagents;

            var listParts = new ListBoxNoScroll<Reaction.Reagent, Label>(c => new Label(c.Name, () => this.SelectReagent(c)))
                .AddItems(reagents);
            this.PanelParts.AddControls(listParts);

            var listbox = new ScrollableBoxNewNew(200, 200, ScrollModes.Vertical);
            var listcollapsible = this.CreateList(order);
            listcollapsible.Build();
            listbox.AddControls(listcollapsible);

            this.PanelItemTypes = new Panel() { Location = this.PanelParts.TopRight };

            this.PanelMaterials = new PanelLabeledNew("Materials") { Location = this.PanelItemTypes.BottomLeft }.SetClientDimensions(200, 150);

            this.PanelCollapsible = new Panel() { AutoSize = false }.SetClientDimensions(200, 200);
            this.PanelCollapsible.AddControls(listbox);
            this.AddControls(this.PanelCollapsible);

            this.ChkHaulOnFinish = new CheckBoxNew("Haul on finish", this.Order.HaulOnFinish)
            {
                Location = this.PanelCollapsible.BottomLeft,
                TickedFunc = () =>  this.Order.HaulOnFinish,
                LeftClickAction = () => PacketCraftOrderToggleHaul.Send(this.Order, !this.Order.HaulOnFinish)
            };
            this.AddControls(this.ChkHaulOnFinish);
        }

        ListCollapsibleNew CreateList(CraftOrder order)
        {
            var list = new ListCollapsibleNew();// 200, 200);
            foreach (var r in order.Reaction.Reagents)
            {
                var items = r.Ingredient.GetAllValidItemDefs();

                var itemTypesNode = new ListBoxCollapsibleNode(r.Name);

                list.AddNode(itemTypesNode);
                foreach (var i in items)
                {
                    var mats = r.Ingredient.GetAllValidMaterials().Intersect(i.GetValidMaterials());

                    if (mats.Count() <= 1) // UNDONE
                    {
                        itemTypesNode.AddLeaf(new CheckBoxNew(i.Name)
                        {
                            TickedFunc = () => !order.IsRestricted(r.Name, i),
                            LeftClickAction = () => CraftingManager.SetOrderRestrictions(order, r.Name, new[] { i }, null, null)
                        });
                    }
                    else
                    {
                        var itemNode = new ListBoxCollapsibleNode(i.Name, () => new CheckBoxNew()
                        {
                            TickedFunc = () => !mats.Any(v => order.IsRestricted(r.Name, v)),
                            LeftClickAction = () => CraftingManager.SetOrderRestrictions(
                                order,
                                r.Name,
                                null,
                                mats.GroupBy(m => order.Restrictions[r.Name].Material.Contains(m)).OrderBy(c => c.Count()).First().ToArray(),
                                null)
                        });
                        itemTypesNode.AddNode(itemNode);
                        foreach (var mat in mats)
                        {
                            itemNode.AddLeaf(new CheckBoxNew(mat.Name)
                            {
                                TickedFunc = () => !order.IsRestricted(r.Name, mat),
                                LeftClickAction = () => CraftingManager.SetOrderRestrictions(order, r.Name, null, new MaterialDef[] { mat }, null)
                            });
                        }
                    }
                }
            }
            return list;
        }

        private void SelectReagent(Reaction.Reagent r)
        {
            this.PanelItemTypes.ClearControls();
            this.PanelItemTypes.AddControls(this.CachedReagentLists[r]);

            this.PanelMaterials.ClearControls();
            this.PanelMaterials.AddControls(this.CachedReagentMaterialLists[r]);
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.OrderParametersChanged:
                    var order = e.Parameters[0] as CraftOrder;
                    if (order == this.Order)
                        this.RefreshReagents();
                    break;

                default:
                    break;
            }
        }

        private void RefreshReagents()
        {

        }
    }
}
