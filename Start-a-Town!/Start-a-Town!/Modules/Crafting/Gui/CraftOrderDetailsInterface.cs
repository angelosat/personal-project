using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;
using System.Linq;

namespace Start_a_Town_
{
    class CraftOrderDetailsInterface : GroupBox
    {
        public Action<Reaction.Product.ProductMaterialPair> Callback = a => { };
        readonly Panel PanelCollapsible;
        readonly CheckBoxNew ChkHaulOnFinish;
        readonly Control ReagentsContainer;
        CraftOrder Order => this.Tag as CraftOrder;
        public CraftOrderDetailsInterface(CraftOrder order)
            :this()
        {
            this.Tag = order;
        }
        CraftOrderDetailsInterface()
        {
            this.AutoSize = true;

            this.ReagentsContainer = new ScrollableBoxNewNew(200, 200, ScrollModes.Vertical);

            this.PanelCollapsible = new Panel() { AutoSize = false }.SetClientDimensions(200, 200);
            this.PanelCollapsible.AddControls(this.ReagentsContainer);
            this.AddControls(this.PanelCollapsible);

            this.ChkHaulOnFinish = new CheckBoxNew("Haul on finish", () => PacketCraftOrderToggleHaul.Send(this.Order, !this.Order.HaulOnFinish), () => this.Order.HaulOnFinish)
            {
                Location = this.PanelCollapsible.BottomLeft
            };
            this.AddControls(this.ChkHaulOnFinish);
        }
        protected override void OnTagChanged()
        {
            var list = this.CreateList(this.Order);
            list.Build();
            this.ReagentsContainer.ClearControls();
            this.ReagentsContainer.AddControls(list);
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
