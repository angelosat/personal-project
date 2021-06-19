using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Blocks;
using Start_a_Town_.Tokens;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_.Crafting
{
    class WorkstationUI : GroupBox
    {
        //WorkstationInterface Interface;
        ReactionsInterface Interface;
        //ReagentPanel Panel_Reagents;
        //Button Btn_Build;
        //SlotGrid SlotsInput, SlotsOutput, SlotsFuel;
        //PanelLabeled PanelInput, PanelOutput, PanelFuel;
        //Bar BarPower, BarProgress;
        //Button Button;
        Vector3 Global;
        BlockEntityWorkstation Entity;
        Panel PanelButtons, PanelCurrentProject;
        WorkstationOrdersUI QueuedOrdersUI;
        //public Label LabelCurrentOrder;
        //CheckBoxNew ChkOrdersEnabled;
        WorkstationControlsUI ButtonsUI;

        Window WindowSelectReaction;


        public WorkstationUI(Vector3 global, BlockEntityWorkstation entity)
        {
            this.Controls.Clear();

            this.Entity = entity;
            this.Global = global;

            var panelSlots = new Panel() { AutoSize = true };
            var input = entity.Input;
            SlotGrid<SlotDefault> slots_mats = new SlotGrid<SlotDefault>(input.Slots, 4);//
            panelSlots.AddControls(slots_mats);

            //this.Interface = new ReactionsInterface(entity, IsWorkstation.Types.Smeltery, global, entity.Input.Slots,
            this.Interface = new ReactionsInterface(entity, this.Entity.Type, global, entity.Input.Slots,
                new Button("Set Project") { LeftClickAction = SetCurrentProject },
                new Button("Place Order") { LeftClickAction = PlaceOrder }
                );// { Location = panelSlots.BottomLeft };
            this.WindowSelectReaction = this.Interface.ToWindow("Select product");
            this.WindowSelectReaction.Movable = true;

            this.PanelCurrentProject = new Panel() { Location = panelSlots.BottomLeft, Size = this.Interface.Panel_Selected.Size };
            RefreshCurrentProject();


            var panelBtnReactions = new Panel() { AutoSize = true, Location = this.PanelCurrentProject.BottomLeft };
            var btnReactions = new Button("Select product", this.PanelCurrentProject.ClientSize.Width)
            {
                LeftClickAction = () =>
                    {
                        this.WindowSelectReaction.Toggle();
                        this.WindowSelectReaction.Location = this.GetWindow().TopRight;
                        this.WindowSelectReaction.ConformToScreen();
                    }
            };
            panelBtnReactions.AddControls(btnReactions);

            var btnCraft = new Button("Craft");
            var btnOrder = new Button("Order") { Location = btnCraft.TopRight };
            var btnViewOrders = new Button("View Orders") { Location = btnOrder.TopRight };

            this.QueuedOrdersUI = new WorkstationOrdersUI(this.Global, this.Entity);
            var window = new Window();
            window.Title = "Orders";
            window.AutoSize = true;
            window.Movable = true;
            window.Client.AddControls(this.QueuedOrdersUI);

            this.ButtonsUI = new WorkstationControlsUI(this.Entity, this.Global, Craft, PlaceOrder, ViewOrders, ToggleOrders)// { Location = new Vector2(0, panelBtnReactions.Bottom) };
            { Location = panelBtnReactions.BottomLeft };
            //{ Location = panelSlots.BottomLeft };
            //{ Location = this.Interface.BottomLeft };

            //this.LabelCurrentOrder = new Label(this.Entity.ExecutingOrders.ToString() + " none") { Location = this.ButtonsUI.TopRight };

            this.Controls.Add(
                panelSlots,
                //slots_mats,
                //this.Interface, 
                panelBtnReactions,
                this.PanelCurrentProject,
                ButtonsUI
                //,
                //this.LabelCurrentOrder
                );
        }

        private void RefreshCurrentProject()
        {
            this.PanelCurrentProject.Controls.Clear();
            if (this.Entity.CurrentOrder != null)
            {
                var item = new CraftingTooltip(this.Entity.CurrentOrder.GetProduct().Product.ToSlot(), this.Entity.CurrentOrder.Materials);
                this.PanelCurrentProject.AddControls(item);
            }
        }
        private void ToggleOrders()
        {
            byte[] data = Network.Serialize(w =>
            {
                w.Write(Player.Actor.InstanceID);
                w.Write(this.Global);
                w.Write(!this.Entity.ExecutingOrders);
            });
            Client.Instance.Send(PacketType.WorkstationToggle, data);
        }


        private void ViewOrders()
        {
            this.QueuedOrdersUI.Refresh(this.Global, this.Entity);
            var win = this.QueuedOrdersUI.GetWindow();
            //win.Location = this.GetWindow().TopRight;
            win.SmartPosition();
            win.Show();// Toggle();
        }

        private void Craft()
        {
            SetCurrentProject();
            Client.PlayerUseInteraction(new TargetArgs(this.Global), new InteractionCraft().Name);
        }

        private void SetCurrentProject()
        {
            var product = this.Interface.SelectedProduct;
            var craft = new CraftOperation(this.Interface.SelectedReaction.ID, product.Requirements, this.Global);
            byte[] data = Network.Serialize(w =>
            {
                w.Write(Player.Actor.InstanceID);
                craft.Write(w);
                //w.Write(this.Global);
                //product.Write(w);
            });
            Client.Instance.Send(PacketType.WorkstationSetCurrent, data);
        }


        private void PlaceOrder()
        {
            var product = this.Interface.SelectedProduct;
            if (product == null)
                return;
            Player.Actor.Map.GetTown().CraftingManager.PlaceOrder(this.Interface.SelectedReaction.ID, product.Requirements, this.Global);
            this.ViewOrders();

            //Client.Instance.Send(PacketType.CraftingOrderPlace, )
        }

        private void Craft(CraftOperation obj)
        {
            Client.PlayerRemoteCall(new TargetArgs(this.Global), Message.Types.Craft, w =>
            {
                w.Write(Player.Actor.InstanceID);
                w.Write(this.Global);
                obj.WriteOld(w);
            });
        }

        void SlotInitializer(InventorySlot s)
        {
            s.LeftClickAction = () =>
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) && s.Tag.StackSize > 1)
                {
                    //SplitStackWindow.Instance.Show(invSlot, parent);
                    SplitStackWindow.Instance.Refresh(new TargetArgs(this.Global, s.Tag)).Show();

                    return;
                }
                if (s.Tag.HasValue)
                {
                    //DragDropManager.Create(new DragDropSlot(parent, this.Tag, this.Tag, DragDropEffects.Move | DragDropEffects.Link));
                    DragDropManager.Create(new DragDropSlot(null, new TargetArgs(this.Global, s.Tag), new TargetArgs(this.Global, s.Tag), DragDropEffects.Move | DragDropEffects.Link));
                }
            };
            s.DragDropAction = (args) =>
            {
                var a = args as DragDropSlot;
                //Net.Client.PlayerInventoryOperationNew(a.Source, s.Tag, a.Slot.Object.StackSize);
                Net.Client.PlayerInventoryOperationNew(a.SourceTarget, new TargetArgs(this.Global, s.Tag), a.DraggedTarget.Slot.Object.StackSize);
                return DragDropEffects.Move;
            };
            s.RightClickAction = () =>
            {
                if (s.Tag.HasValue)
                    Net.Client.PlayerSlotRightClick(new TargetArgs(this.Global), s.Tag.Object);
                //Net.Client.PlayerSlotInteraction(s.Tag);
            };
        }
        void OutputSlotInitializer(InventorySlot s)
        {
            s.RightClickAction = () =>
            {
                if (s.Tag.HasValue)
                    Net.Client.PlayerSlotRightClick(new TargetArgs(this.Global), s.Tag.Object);
                //Net.Client.PlayerSlotInteraction(s.Tag);
            };
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                //case Message.Types.ObjectStateChanged:
                case Message.Types.BlockEntityStateChanged:
                    //var entity = e.Parameters[0] as GameObject;
                    //if (entity != this.Entity)
                    //    return;
                    var entity = e.Parameters[0] as BlockEntityWorkstation;
                    if (entity != this.Entity)
                        return;
                    //this.Button.Invalidate();
                    break;

                case Message.Types.OrdersUpdated:
                case Message.Types.WorkstationOrderSet:
                    //entity = e.Parameters[0] as BlockEntityWorkstation;
                    //if (entity != this.Entity)
                    //    break;
                    //var order = this.Entity.GetCurrentOrder();// entity.GetCurrentOrder();
                    //this.LabelCurrentOrder.Text = this.Entity.ExecutingOrders.ToString() + (order != null ? order.ReactionID.ToString() : "none");
                    //this.ChkOrdersEnabled.Value = this.Entity.ExecutingOrders;
                    //this.ViewOrders();
                    this.RefreshCurrentProject();
                    break;

                //case Message.Types.OrdersUpdated:
                //    var global = (Vector3)e.Parameters[0];
                //    if (global != this.Global)
                //        break;
                //    var order = entity.GetCurrentOrder();
                //    this.LabelCurrentOrder.Text = this.Entity.ExecutingOrders.ToString() + (order != null ? order.ReactionID.ToString() : "none");
                //    break;

                default:
                    break;
            }
            base.OnGameEvent(e);
        }

        void BurnFuel(Vector3 entityGlobal)
        {
            //Net.Client.PlayerRemoteCall(parent, Message.Types.Start);
            Net.Client.PlayerRemoteCall(new TargetArgs(entityGlobal), Message.Types.Start);
        }

        private void Client_GameEvent(object sender, GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.InventoryChanged:
                    //this.RefreshSelectedPanel();
                    break;


                default:
                    break;
            }
        }


        //private void RefreshMaterialPicking(Reaction reaction)
        //{
        //    this.Panel_Reagents.Controls.Clear();
        //    this.Panel_Reagents.Controls.Add(new Label(reaction.Name) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
        //    this.Panel_Reagents.Controls.Add(new Label(this.Panel_Reagents.Controls.BottomLeft, "Materials") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
        //    List<GameObjectSlot> mats = new List<GameObjectSlot>();
        //    foreach (var reagent in reaction.Reagents)
        //    {
        //        GameObjectSlot matSlot = GameObjectSlot.Empty;
        //        matSlot.Name = reagent.Name;
        //        mats.Add(matSlot);
        //        Slot slot = new Slot(this.Panel_Reagents.Controls.BottomLeft) { Tag = matSlot, CustomTooltip = true };
        //        slot.HoverFunc = () => { string t = ""; foreach (var filter in reagent.Conditions) { t += filter.ToString() + "\n"; } return t.TrimEnd('\n'); };//
        //        Label matName = new Label(slot.TopRight, reagent.Name);
        //        slot.LeftClickAction = () =>
        //        {
        //            MaterialPicker.Instance.Label.Text = "Material for: " + reagent.Name;
        //            MaterialPicker.Instance.Show(UIManager.Mouse, reagent, o => { slot.Tag.Object = o; });
        //        };
        //        slot.RightClickAction = () =>
        //        {
        //            slot.Tag.Clear();
        //        };
        //        matSlot.Filter = reagent.Filter;
        //        matSlot.ObjectChanged = o =>
        //        {
        //            if ((from sl in mats where !sl.HasValue select sl).FirstOrDefault() != null)
        //            {
        //                this.Panel_Selected.Controls.Clear();
        //                return;
        //            }

        //            var product = reaction.Products.First().GetProduct(reaction, mats);
        //            RefreshSelectedPanel(product);

        //            // set selected product in smeltery
        //            List<ItemRequirement> reqs = product.Requirements;// (from mat in mats select new ItemRequirement(mat.Object.ID, 1)).ToList();
        //            var crafting = new CraftOperation(reaction.ID, reqs, null, null, this.Entity.Input);
        //            Net.Client.PlayerRemoteCall(new TargetArgs(this.Global), Message.Types.SetProduct, w =>
        //            {
        //                //crafting.Write(w);
        //                w.Write(reaction.ID);
        //                w.Write(reqs.Count);
        //                foreach (var req in reqs)
        //                    req.Write(w);
        //            });
        //        };
        //        this.Panel_Reagents.Controls.Add(slot, matName);
        //    }
        //}

        protected virtual List<Reaction> GetAvailableBlueprints()
        {
            return (from reaction in Reaction.Dictionary.Values
                    //where reaction.Building == parent.ID
                    //where reaction.ValidWorkshops.Contains(ItemTemplate.Smeltery.ID)
                    where reaction.ValidWorkshops.Contains(Tokens.IsWorkstation.Types.Smeltery)

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



        //private void RefreshSelectedPanel()
        //{
        //    var product = this.Panel_Selected.Tag as Reaction.Product.ProductMaterialPair;
        //    if (product.IsNull())
        //        return;
        //    this.RefreshSelectedPanel(product);
        //}
        //private void RefreshSelectedPanel(Reaction.Product.ProductMaterialPair product)
        //{
        //    var container = Player.Actor.GetComponent<PersonalInventoryComponent>().Slots.Slots;
        //    foreach (var item in product.Requirements)
        //    {
        //        int amountFound = 0;
        //        foreach (var found in from found in container where found.HasValue where (int)found.Object.ID == item.ObjectID select found.Object)
        //            amountFound += found.StackSize;
        //        item.Amount = amountFound;
        //    }

        //    this.Panel_Selected.Controls.Clear();
        //    this.Panel_Selected.Tag = product;//.Product;
        //    if (product.IsNull())
        //        return;

        //    //CraftingTooltip tip = new CraftingTooltip(product.Product.ToSlot(), product.Req);
        //    CraftingTooltip tip = new CraftingTooltip(product.Product.ToSlot(), product.Requirements);
        //    this.Panel_Selected.Controls.Add(tip);
        //    return;
        //}

        //private void CreateItemRequirements(Reaction reaction, List<GameObjectSlot> materials, List<GameObjectSlot> container)// Reaction reaction)
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
        //    //RefreshSelectedPanel(panel_Selected, reaction.Products.First().Create(reaction, matList), reqs);// (from s in materials select new ItemRequirement(s.Object.ID, 1)).ToList());
        //    //RefreshSelectedPanel(reaction.Products.First().Create(reaction, materials), reqs);// (from s in materials select new ItemRequirement(s.Object.ID, 1)).ToList());
        //    RefreshSelectedPanel(reaction.Products.First().GetProduct(reaction, materials));
        //}



    }

}
