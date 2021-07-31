using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Modules.Crafting
{
    class WorkstationGuiNew : GroupBox
    {
        IntVec3 Global;
        readonly MapBase Map;
        readonly Panel PanelReactions;
        readonly ScrollableBoxNewNew ListOrders;

        public WorkstationGuiNew(MapBase map, IntVec3 global, BlockEntityCompWorkstation entity)
        {
            this.Global = global;
            this.Map = map;
            var panelOrders = new PanelTitled("Orders", 300, 500);
            var btnAddOrder = new Button("Add Order") { LeftClickAction = this.AddOrder };

            this.PanelReactions = new Panel() { AutoSize = true };
            var validreactions = Reaction.Dictionary.Values.Where(r => r.ValidWorkshops.Any(t => entity.IsWorkstationType(t))).ToList();

            var reactions = new ButtonList<Reaction>(validreactions, 200, 400, r => r.Name, (r, b) => b.LeftClickAction = () => this.PlaceOrder(r));
            this.PanelReactions.AddControls(reactions);

            var w = panelOrders.Client.ClientSize.Width;
            var h = panelOrders.Client.ClientSize.Height;
            var list = new ListBoxObservable<CraftOrderNew>(c => c.GetGui());
            this.ListOrders = new ScrollableBoxNewNew(w, h, ScrollModes.Vertical);
            list.Bind(entity.Orders);
            this.ListOrders.AddControls(list);

            panelOrders.AddControls(this.ListOrders);

            this.AddControls(panelOrders, btnAddOrder);
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
            this.PanelReactions.Show();
        }
        void PlaceOrder(Reaction r)
        {
            PacketOrderAdd.Send(this.Map.Net, this.Global, r.ID);
            this.PanelReactions.Hide();
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.BlocksChanged:
                    if ((e.Parameters[1] as IEnumerable<IntVec3>).Contains(this.Global))
                        this.GetWindow().Hide();
                    break;
                default:
                    base.OnGameEvent(e);
                    break;
            }
        }
    }
    //class WorkstationGuiNew : GroupBox
    //{
    //    IntVec3 Global;
    //    readonly MapBase Map;
    //    readonly BlockEntityCompWorkstation Entity;
    //    readonly Button BtnAddOrder;
    //    readonly ButtonList<Reaction> Reactions;
    //    readonly Panel PanelReactions;
    //    readonly PanelTitled PanelOrders;
    //    readonly ScrollableBoxNewNew ListOrders;

    //    public WorkstationGuiNew(MapBase map, IntVec3 global, BlockEntityCompWorkstation entity)
    //    {
    //        this.Entity = entity;
    //        this.Global = global;
    //        this.Map = map;
    //        this.PanelOrders = new PanelTitled("Orders", 300, 500);
    //        this.BtnAddOrder = new Button("Add Order") { LeftClickAction = this.AddOrder };

    //        this.PanelReactions = new Panel() { AutoSize = true };
    //        var validreactions = Reaction.Dictionary.Values.Where(r => r.ValidWorkshops.Any(t => entity.IsWorkstationType(t))).ToList();

    //        this.Reactions = new ButtonList<Reaction>(validreactions, 200, 400, r => r.Name, (r, b) => b.LeftClickAction = () => this.PlaceOrder(r));

    //        this.PanelReactions.AddControls(this.Reactions);

    //        var w = this.PanelOrders.Client.ClientSize.Width;
    //        var h = this.PanelOrders.Client.ClientSize.Height;
    //        var list = new ListBoxNoScroll<CraftOrderNew>(c => c.GetGui());
    //        this.ListOrders = new ScrollableBoxNewNew(w, h, ScrollModes.Vertical);
    //        var orders = entity.Orders;
    //        if (orders is not null)
    //            this.RefreshOrders(orders);

    //        this.PanelOrders.AddControls(this.ListOrders);

    //        this.AddControls(this.PanelOrders, this.BtnAddOrder);
    //        this.AlignTopToBottom();
    //    }
    //    public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
    //    {
    //        if (!this.PanelReactions.HitTest() && this.PanelReactions.IsOpen)
    //            this.PanelReactions.Hide();
    //        base.HandleLButtonDown(e);
    //    }
    //    public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
    //    {
    //        this.PanelReactions.Hide();
    //        base.HandleRButtonDown(e);
    //    }
    //    private void AddOrder()
    //    {
    //        this.PanelReactions.Location = UIManager.Mouse;
    //        this.PanelReactions.Show();
    //    }
    //    void PlaceOrder(Reaction r)
    //    {
    //        PacketOrderAdd.Send(this.Map.Net, this.Global, r.ID);
    //        this.PanelReactions.Hide();
    //    }

    //    void RefreshOrders(List<CraftOrderNew> orders)
    //    {
    //        this.ListOrders.Client.ClearControls();
    //        if (orders == null)
    //            return;
    //        foreach (var o in orders)
    //        {
    //            var ui = o.GetGui();
    //            var panel = new Panel() { AutoSize = true };
    //            panel.AddControls(ui);
    //            this.ListOrders.Client.AddControls(panel);
    //        }
    //        this.ListOrders.AlignTopToBottom();
    //    }

    //    internal override void OnGameEvent(GameEvent e)
    //    {
    //        switch (e.Type)
    //        {
    //            case Components.Message.Types.OrdersUpdatedNew:
    //                if (e.Parameters[0] == this.Entity)
    //                    this.RefreshOrders(this.Entity.Orders);
    //                break;

    //            case Components.Message.Types.BlocksChanged:
    //                if ((e.Parameters[1] as IEnumerable<IntVec3>).Contains(this.Global))
    //                    this.GetWindow().Hide();
    //                break;
    //            default:
    //                base.OnGameEvent(e);
    //                break;
    //        }
    //    }
    //}
}
