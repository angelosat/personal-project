using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Net;
using Start_a_Town_.Crafting;

namespace Start_a_Town_.Blocks.Smeltery
{
    class SmeltingInterfaceNewNew : GroupBox
    {
        ReactionsInterface Interface;
        SlotGrid SlotsInput, SlotsOutput, SlotsFuel;
        PanelLabeled PanelInput, PanelOutput, PanelFuel;
        Bar BarPower, BarProgress;
        Button Button;
        Vector3 Global;
        BlockSmelteryEntity Entity;
        Panel PanelButtons;
        public Label LabelCurrentOrder;
        CheckBoxNew ChkOrdersEnabled;

        public SmeltingInterfaceNewNew()
        {

        }

        public SmeltingInterfaceNewNew Refresh(Vector3 global, BlockSmelteryEntity entity)
        {
            this.Controls.Clear();

            this.Interface = new ReactionsInterface(entity, IsWorkstation.Types.Smeltery, global, entity.Storage.Slots);

            this.Entity = entity;
            this.Global = global;

            this.SlotsFuel = new SlotGrid(entity.Fuels.Slots, 4);
            this.SlotsInput = new SlotGrid(entity.Storage.Slots, 4);
            this.SlotsOutput = new SlotGrid(entity.Output.Slots, 4);

            this.PanelInput = new PanelLabeled("Input") { Location = this.Interface.TopRight };
            this.SlotsInput = new SlotGrid(entity.Storage.Slots, 4, this.SlotInitializer) { Location = this.PanelInput.Controls.BottomLeft };
            this.PanelInput.Controls.Add(this.SlotsInput);

            this.PanelOutput = new PanelLabeled("Output") { Location = this.PanelInput.BottomLeft };
            this.SlotsOutput = new SlotGrid(entity.Output.Slots, 4, 
                this.SlotInitializer
                ) { Location = this.PanelOutput.Controls.BottomLeft };
            this.PanelOutput.Controls.Add(this.SlotsOutput);

            this.PanelFuel = new PanelLabeled("Fuel") { Location = this.PanelOutput.BottomLeft };
            this.SlotsFuel = new SlotGrid(entity.Fuels.Slots, 4, this.SlotInitializer) { Location = this.PanelFuel.Controls.BottomLeft };
            this.PanelFuel.Controls.Add(this.SlotsFuel);

            var panelbarpower = new Panel() { Location = this.PanelFuel.BottomLeft, AutoSize = true };
            this.BarPower = new Bar() { Object = entity.Power, Name = "Power" };
            panelbarpower.Controls.Add(this.BarPower);
            this.Controls.Add(panelbarpower);

            var panelbarprogress = new Panel() { Location = panelbarpower.BottomLeft, AutoSize = true };
            this.BarProgress = new Bar() { Object = entity.SmeltProgress, Name = "Progress" };
            panelbarprogress.Controls.Add(this.BarProgress);
            this.Controls.Add(panelbarprogress);

            this.Button = new Button("Burn") { Location = panelbarprogress.BottomLeft, LeftClickAction = () => BurnFuel(global), TextFunc = () => this.Entity.State == BlockSmelteryEntity.States.Running ? "Stop" : "Start" };

            this.PanelButtons = new Panel() { AutoSize = true, Location = this.Interface.BottomLeft };
            var btnCraft = new Button("Craft");
            var btnOrder = new Button("Order") { Location = btnCraft.TopRight };
            var btnViewOrders = new Button("View Orders") { Location = btnOrder.TopRight };
            var window = new Window
            {
                Title = "Orders",
                AutoSize = true,
                Movable = true
            };

            btnCraft.LeftClickAction = Craft;
            btnOrder.LeftClickAction = PlaceOrder;
            btnViewOrders.LeftClickAction = ViewOrders;

            this.PanelButtons.Controls.Add(btnOrder, btnCraft, btnViewOrders);

            var panelchkbox = new Panel() { AutoSize = true, Location = this.PanelButtons.TopRight };
            this.ChkOrdersEnabled = new CheckBoxNew("Orders enabled")
            {
                Value = this.Entity.ExecutingOrders,
                LeftClickAction = ToggleOrders
            };
            panelchkbox.AddControls(this.ChkOrdersEnabled);

            this.LabelCurrentOrder = new Label(this.Entity.ExecutingOrders.ToString() + " none") { Location = panelchkbox.TopRight };

            this.Controls.Add(this.Interface, this.PanelInput, this.PanelOutput, this.PanelFuel, panelbarpower, panelbarprogress, this.Button, this.PanelButtons, panelchkbox, this.LabelCurrentOrder);
            return this;
        }
        private void ToggleOrders()
        {
            byte[] data = Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                w.Write(this.Global);
                w.Write(!this.Entity.ExecutingOrders);
            });
            Client.Instance.Send(PacketType.WorkstationToggle, data);
        }
        private void ViewOrders()
        {
           
        }

        private void Craft()
        {
            var product = this.Interface.SelectedProduct;
            var craft = new CraftOperation(this.Interface.SelectedReaction.ID, product.Requirements, this.Global);
            byte[] data = Network.Serialize(w=>
                {
                    w.Write(PlayerOld.Actor.RefID);
                    craft.Write(w);
                });
            Client.Instance.Send(PacketType.WorkstationSetCurrent, data);
            throw new Exception();
        }


        private void PlaceOrder()
        {
            var product = this.Interface.SelectedProduct;
            if (product == null)
                return;
            PlayerOld.Actor.Map.GetTown().CraftingManager.PlaceOrder(this.Interface.SelectedReaction.ID, product.Requirements, this.Global);
        }


        void SlotInitializer(InventorySlot s)
        {
            s.LeftClickAction = () =>
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) && s.Tag.StackSize > 1)
                {
                    SplitStackWindow.Instance.Refresh(new TargetArgs(this.Global, s.Tag)).Show();
                    return;
                }
                if (s.Tag.HasValue)
                {
                    DragDropManager.Create(new DragDropSlot(null, new TargetArgs(this.Global, s.Tag), new TargetArgs(this.Global, s.Tag), DragDropEffects.Move | DragDropEffects.Link));
                }
            };
            s.DragDropAction = (args) =>
            {
                var a = args as DragDropSlot;
                Client.PlayerInventoryOperationNew(a.SourceTarget, new TargetArgs(this.Global, s.Tag), a.DraggedTarget.Slot.Object.StackSize);
                return DragDropEffects.Move;
            };
            s.RightClickAction = () =>
            {
                if (s.Tag.HasValue)
                    Client.PlayerSlotRightClick(new TargetArgs(this.Global), s.Tag.Object);
            };
        }
        
        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Message.Types.BlockEntityStateChanged:
                    var entity = e.Parameters[0] as BlockEntityWorkstation;
                    if (entity != this.Entity)
                        return;
                    this.Button.Invalidate();
                    break;

                case Message.Types.OrdersUpdated:
                case Message.Types.WorkstationOrderSet:
                    var order = this.Entity.GetCurrentOrder();
                    this.LabelCurrentOrder.Text = this.Entity.ExecutingOrders.ToString() + (order != null ? order.ReactionID.ToString() : "none");
                    this.ChkOrdersEnabled.Value = this.Entity.ExecutingOrders;
                    this.ViewOrders();
                    break;

                default:
                    break;
            }
        }

        void BurnFuel(Vector3 entityGlobal)
        {
            Client.PlayerRemoteCall(new TargetArgs(entityGlobal), Message.Types.Start);
        }

        private void Client_GameEvent(object sender, GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.InventoryChanged:
                    break;

                default:
                    break;
            }
        }

        protected virtual List<Reaction> GetAvailableBlueprints()
        {
            return (from reaction in Reaction.Dictionary.Values
                    where reaction.ValidWorkshops.Contains(IsWorkstation.Types.Smeltery)

                    select reaction).ToList();
        }
    }
}
