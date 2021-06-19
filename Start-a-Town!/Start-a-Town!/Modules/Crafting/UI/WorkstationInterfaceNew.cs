using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Crafting;
using Start_a_Town_.Blocks;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Modules.Crafting
{
    class WorkstationInterfaceNew : GroupBox
    {
        Vector3 Global;
        IMap Map;
        BlockEntityCompWorkstation Entity;
        Button BtnAddOrder;

        ButtonList<Reaction> Reactions;

        Panel PanelReactions;

        PanelTitled PanelOrders;
        //ListBox<CraftOrderNew, Button> ListOrders;
        ScrollableBox ListOrders;

        public WorkstationInterfaceNew(IMap map, Vector3 global, BlockEntityCompWorkstation entity)
        {
            this.Entity = entity;
            this.Global = global;
            this.Map = map;
            this.PanelOrders = new PanelTitled("Orders", 300, 500);
            this.BtnAddOrder = new Button("Add Order") { LeftClickAction = this.AddOrder };

            this.PanelReactions = new Panel() { AutoSize = true };
            //var validreactions = Reaction.Dictionary.Values.Where(r => r.ValidWorkshops.Contains(entity.WorkstationType)).ToList();
            var validreactions = Reaction.Dictionary.Values.Where(r => r.ValidWorkshops.Any(t => entity.IsWorkstationType(t))).ToList();


            //this.Reactions = new ListBox<Reaction, Button>(200, 400);
            //this.Reactions.Build(validreactions, r => r.Name, (r, b) => b.LeftClickAction = () => PlaceOrder(r));
            this.Reactions = new ButtonList<Reaction>(validreactions, 200, 400, r => r.Name, (r, b) => b.LeftClickAction = () => PlaceOrder(r));

            //this.ListReactions = new ComboBox<Reaction>(this.Reactions, r => r.Name) { Orientation = ComboBox<Reaction>.OpenOrientation.Above };
            this.PanelReactions.AddControls(this.Reactions);

            //this.ListOrders = new ListBox<CraftOrderNew, Button>(this.PanelOrders.Panel.ClientSize);
            this.ListOrders = new ScrollableBox(this.PanelOrders.Client.ClientSize);
            var orders = entity.Orders;// map.Town.CraftingManager.GetOrdersNew(global);
            if (orders != null)
                //this.ListOrders.Build(orders, o => o.Name);
                RefreshOrders(orders);

            this.PanelOrders.AddControls(this.ListOrders);

            this.AddControls(this.PanelOrders, this.BtnAddOrder
                //, this.ListReactions
                );
            this.AlignTopToBottom();
        }
        public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.PanelReactions.HitTest() && this.PanelReactions.IsOpen)
                this.PanelReactions.Hide();
            base.HandleLButtonDown(e);
        }
        public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.PanelReactions.Hide();
            base.HandleRButtonDown(e);
        }
        private void AddOrder()
        {
            this.PanelReactions.Location = UIManager.Mouse;
            //this.PanelReactions.ConformToScreen();
            this.PanelReactions.Show();
        }
        void PlaceOrder(Reaction r)
        {
            PacketOrderAdd.Send(this.Map.Net, this.Global, r.ID);
            this.PanelReactions.Hide();
            return;
      
            //var data = Network.Serialize(w =>
            //    {
            //        w.Write(this.Global);
            //        w.Write(r.ID);
            //    });
            //Client.Instance.Send(PacketType.CraftingOrderPlaceNew, data);
        }

        void RefreshOrders(List<CraftOrderNew> orders)
        {
            this.ListOrders.Client.ClearControls();
            if (orders == null)
                return;
            foreach (var o in orders)
            {
                var ui = o.GetInterface();
                var panel = new Panel() { AutoSize = true };
                panel.AddControls(ui);
                //panel.Width = this.PanelOrders.Panel.ClientSize.Width;
                this.ListOrders.Client.AddControls(panel);
            }
            this.ListOrders.AlignTopToBottom();

            //this.PanelOrders.ClearControls();
            //if (orders == null)
            //    return;
            //foreach(var o in orders)
            //{
            //    var ui = o.GetInterface();
            //    var panel = new Panel(){AutoSize = true};
            //    panel.AddControls(ui);
            //    //panel.Width = this.PanelOrders.Panel.ClientSize.Width;
            //    this.PanelOrders.AddControls(panel);
            //}
            //this.PanelOrders.Panel.AlignTopToBottom();
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.OrdersUpdatedNew:
                    //var global = (Vector3)e.Parameters[0];
                    //var orders = e.Net.Map.Town.CraftingManager.GetOrdersNew(global);
                    if(e.Parameters[0] == this.Entity)
                        RefreshOrders(this.Entity.Orders);
                    break;

                case Components.Message.Types.BlockChanged:
                    if ((Vector3)e.Parameters[1] == this.Global)
                        this.GetWindow().Hide();
                    break;
                case Components.Message.Types.BlocksChanged:
                    if ((e.Parameters[1] as IEnumerable<Vector3>).Contains(this.Global))
                        this.GetWindow().Hide();
                    break;
                default:
                    base.OnGameEvent(e);
                    break;
            }
        }
    }
}
