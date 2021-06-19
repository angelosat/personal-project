using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Towns.Crafting;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Modules.Crafting;
using Start_a_Town_.Crafting;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.Crafting
{
    class CraftOrderDetailsInterface : GroupBox
    {
        public Action<Reaction.Product.ProductMaterialPair> Callback = a => { };
        TableScrollableCompact<GameObject> TableReagents;
        Panel PanelParts, PanelItemTypes, PanelMaterials, PanelCollapsible;
        Reaction.Reagent SelectedReagent;
        object AllowColumnToken;
        Button BtnAllowAll, BtnClearAll, BtnInvert;
        Dictionary<Reaction.Reagent, ListBoxNew<ItemDef, CheckBoxNew>> CachedReagentLists = new Dictionary<Reaction.Reagent, ListBoxNew<ItemDef, CheckBoxNew>>();
        Dictionary<Reaction.Reagent, ListBoxNew<Material, CheckBoxNew>> CachedReagentMaterialLists = new Dictionary<Reaction.Reagent, ListBoxNew<Material, CheckBoxNew>>();
        CheckBoxNew ChkHaulOnFinish;

        CraftOrderNew Order;
        public CraftOrderDetailsInterface(CraftOrderNew order)
        {
            this.Order = order;
            this.AutoSize = true;

            this.BtnAllowAll = new Button("Allow All") { LeftClickAction = AllowAll };
            this.BtnClearAll = new Button("Clear All") { Location = this.BtnAllowAll.TopRight, LeftClickAction = ClearAll };
            this.BtnInvert = new Button("Invert") { Location = this.BtnClearAll.TopRight, LeftClickAction = Invert };
            this.AddControls(this.BtnAllowAll, this.BtnClearAll, this.BtnInvert);

            this.PanelParts = new Panel() { AutoSize = true, Location = this.BtnAllowAll.BottomLeft };
            var reagents = order.Reaction.Reagents;
            
            var listParts = new ListBoxNew<Reaction.Reagent, Label>(70, 150);
            listParts.Build(reagents, f => f.Name, (c, btn) => btn.LeftClickAction = () => this.SelectReagent(c));
            this.PanelParts.AddControls(listParts);
            //this.AddControls(this.PanelParts);


            this.AllowColumnToken = new object();

            var listcollapsible = CreateList(order);
            listcollapsible.Build();

            this.PanelItemTypes = new Panel() { Location = this.PanelParts.TopRight };//, AutoSize = true };

            this.PanelMaterials = new PanelLabeledNew("Materials") { Location = this.PanelItemTypes.BottomLeft }.SetClientDimensions(200, 150);

            this.PanelCollapsible = new Panel() { AutoSize = false }.SetClientDimensions(200, 200);
            this.PanelCollapsible.AddControls(listcollapsible);
            this.AddControls(this.PanelCollapsible); //listcollapsible);// 

            this.ChkHaulOnFinish = new CheckBoxNew("Haul on finish", this.Order.HaulOnFinish)
            {
                Location = this.PanelCollapsible.BottomLeft,
                //IsToggledFunc = () =>
                TickedFunc = () =>
                {
                    return this.Order.HaulOnFinish;
                },
                LeftClickAction = () =>
                {
                    //this.Order.HaulOnFinish = !this.Order.HaulOnFinish;
                    PacketCraftOrderToggleHaul.Send(this.Order, !this.Order.HaulOnFinish);
                }
            };
            this.AddControls(this.ChkHaulOnFinish);
        }

        ListBoxCollapsible CreateList(CraftOrderNew order)
        {
            var list = new ListBoxCollapsible(200, 200);
            foreach (var r in order.Reaction.Reagents)
            {
                var items = r.Ingredient.GetAllValidItemDefs();

                var itemTypesNode = new ListBoxCollapsibleNode(r.Name);
                   
                list.AddNode(itemTypesNode);
                foreach (var i in items)
                {
                    //var mats = i.GetValidMaterials().ToList();
                    //var mats = r.Ingredient.AllowedMaterials;
                    //var mats = i.GetValidMaterials();
                    //if (r.Ingredient.AllowedMaterials.Any())
                    //    mats = mats.Intersect(r.Ingredient.AllowedMaterials);
                    var mats = r.Ingredient.AllowedMaterials.Intersect(i.GetValidMaterials());

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
                                LeftClickAction = () => CraftingManager.SetOrderRestrictions(order, r.Name, null, new Material[] { mat }, null)
                            });
                        }
                    }
                }
            }
            return list;
        }

        private void Invert()
        {
            if (this.SelectedReagent == null)
                return;
            foreach (var item in this.GetReagentItems(this.SelectedReagent))
            {
                var isAllowed = this.Order.IsReagentAllowed(this.SelectedReagent.Name, (int)item.IDType);
                CraftingManager.WriteOrderToggleReagent(Client.Instance.OutgoingStream, this.Order, this.SelectedReagent.Name, (int)item.IDType, isAllowed);
            }
        }

        private void ClearAll()
        {
            if (this.SelectedReagent == null)
                return;
            foreach (var item in this.GetReagentItems(this.SelectedReagent))
                CraftingManager.WriteOrderToggleReagent(Client.Instance.OutgoingStream, this.Order, this.SelectedReagent.Name, (int)item.IDType, true);
        }

        private void AllowAll()
        {
            if (this.SelectedReagent == null)
                return;
            foreach (var item in this.GetReagentItems(this.SelectedReagent))
                CraftingManager.WriteOrderToggleReagent(Client.Instance.OutgoingStream, this.Order, this.SelectedReagent.Name, (int)item.IDType, false);
        }

        private void SelectReagent(Reaction.Reagent r)
        {
            this.SelectedReagent = r;

            this.PanelItemTypes.ClearControls();
            this.PanelItemTypes.AddControls(this.CachedReagentLists[r]);

            this.PanelMaterials.ClearControls();
            this.PanelMaterials.AddControls(this.CachedReagentMaterialLists[r]);
        }

        //private void Refresh()
        //{
        //    var items = GetReagentItems(this.SelectedReagent);
        //    this.TableReagents.Build(items, false);
        //    this.RefreshReagents();
        //    this.PanelItemTypes.ConformToControls();
        //}

        private IEnumerable<Entity> GetReagentItems(Reaction.Reagent r)
        {
            var items = (from obj in GameObject.Objects.Values where r.Filter(obj as Entity) select obj as Entity);
            return items;
        }
        private IEnumerable<ItemDef> GetReagentDefs(Reaction.Reagent r)
        {
            var items = Def.Database.Values.OfType<ItemDef>().Where(r.Filter);
            return items;
        }
        private IEnumerable<Material> GetReagentMaterials(Reaction.Reagent r)
        {
            var items = Material.Database.Values.Where(r.Filter);
            return items;
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.OrderParametersChanged:
                    var order = e.Parameters[0] as CraftOrderNew;
                    if (order == this.Order)
                        this.RefreshReagents();
                    break;

                default:
                    break;
            }
        }

        private void RefreshReagents()
        {
            //var reagentControls = this.CachedReagentLists[this.SelectedReagent];
            //foreach (var control in reagentControls.Items)
            //    control.Value = this.Order.IsReagentAllowed(this.SelectedReagent.Name, (int)control.Tag);

        }
    }
}
