using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_.Crafting
{
    class WorkstationControlsUI : GroupBox
    {
        readonly BlockEntityWorkstation Entity;
        //Vector3 Global;
        readonly CheckBoxNew ChkOrdersEnabled;
        //UIWorkstationOrders QueuedOrdersUI;

        public WorkstationControlsUI(BlockEntityWorkstation entity, Vector3 global, Action Craft, Action PlaceOrder, Action ViewOrders, Action ToggleOrders)
        {
            this.Entity = entity;
            //this.Global = global;

            //this.QueuedOrdersUI = new UIWorkstationOrders(this.Global, this.Entity);
            //var window = new Window();
            //window.Title = "Orders";
            //window.AutoSize = true;
            //window.Movable = true;
            //window.Client.AddControls(this.QueuedOrdersUI);

            this.AutoSize = true;
            var panelButtons = new Panel() { AutoSize = true };
            var btnCraft = new Button("Craft");
            //var btnOrder = new Button("Order") { Location = btnCraft.TopRight };
            var btnViewOrders = new Button("View Orders") { Location = btnCraft.TopRight };
            btnCraft.LeftClickAction = Craft;
            //btnOrder.LeftClickAction = PlaceOrder;
            btnViewOrders.LeftClickAction = ViewOrders;
            panelButtons.Controls.Add(btnCraft,
                //btnOrder, 
                btnViewOrders);

            var panelchkbox = new Panel() { AutoSize = true, Location = panelButtons.TopRight };
            this.ChkOrdersEnabled = new CheckBoxNew("Orders enabled");
            this.ChkOrdersEnabled.Value = this.Entity.ExecutingOrders;
            this.ChkOrdersEnabled.LeftClickAction = ToggleOrders;
            panelchkbox.AddControls(this.ChkOrdersEnabled);

            this.AddControls(panelButtons, panelchkbox);
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.OrdersUpdated:
                case Message.Types.WorkstationOrderSet:
                    var order = this.Entity.GetCurrentOrder();// entity.GetCurrentOrder();
                    this.ChkOrdersEnabled.Value = this.Entity.ExecutingOrders;
                    break;

                default:
                    break;
            }
        }


        //private void ToggleOrders()
        //{
        //    byte[] data = Network.Serialize(w =>
        //    {
        //        w.Write(Player.Actor.InstanceID);
        //        w.Write(this.Global);
        //        w.Write(!this.Entity.ExecutingOrders);
        //    });
        //    Client.Instance.Send(PacketType.WorkstationToggle, data);
        //}

        //private void ViewOrders()
        //{
        //    this.QueuedOrdersUI.Refresh(this.Global, this.Entity);
        //    var win = this.QueuedOrdersUI.GetWindow();
        //    //win.Location = this.GetWindow().TopRight;
        //    win.SmartPosition();
        //    win.Show();// Toggle();
        //}

        //private void Craft()
        //{
        //    var product = this.Interface.SelectedProduct;
        //    var craft = new CraftOperation(this.Interface.SelectedReaction.ID, product.Requirements, this.Global);
        //    byte[] data = Network.Serialize(w =>
        //    {
        //        w.Write(Player.Actor.InstanceID);
        //        craft.Write(w);
        //        //w.Write(this.Global);
        //        //product.Write(w);
        //    });
        //    Client.Instance.Send(PacketType.WorkstationSetCurrent, data);
        //    Client.PlayerUseInteraction(new TargetArgs(this.Global), new InteractionCraft().Name);
        //}


        //private void PlaceOrder()
        //{
        //    var product = this.Interface.SelectedProduct;
        //    if (product == null)
        //        return;
        //    Player.Actor.Map.GetTown().CraftingManager.PlaceOrder(this.Interface.SelectedReaction.ID, product.Requirements, this.Global);

        //    //Client.Instance.Send(PacketType.CraftingOrderPlace, )
        //}
    }
}
