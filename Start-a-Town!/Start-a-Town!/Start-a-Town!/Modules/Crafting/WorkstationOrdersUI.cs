using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Towns;
using Start_a_Town_.Towns.Crafting;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Crafting
{
    class WorkstationOrdersUI : GroupBox
    {
        Panel Panel;
        BlockEntityWorkstation Workbench;
        Vector3 WorkbenchGlobal;
        ListBox<CraftOrder, Label> TaskList;
        Town Town;
        public WorkstationOrdersUI(Vector3 global, BlockEntityWorkstation workbench)
        {
            this.Town = Engine.Map.GetTown();
            this.Workbench = workbench;
            this.WorkbenchGlobal = global;
            this.TaskList = new ListBox<CraftOrder, Label>(100, 200);
            this.Panel = new Panel() { AutoSize = true };
            this.Panel.Controls.Add(this.TaskList);

            var panelButtons = new Panel() { Location = this.Panel.BottomLeft, AutoSize = true };
            panelButtons.Controls.Add(new Button("Clear", this.Panel.ClientSize.Width) { LeftClickAction = ClearOrders });
            this.Controls.Add(this.Panel, panelButtons);
        }

        
        public void Refresh(Vector3 global, BlockEntityWorkstation workbench)
        {
            this.Workbench = workbench;
            this.WorkbenchGlobal = global;
            var orders = workbench.GetOrders(this.Town.Net, global);
            //this.TaskList.Build(this.Workbench.QueuedOrders, f => f.GetProduct().Product.Name, (o, b) => { b.LeftClickAction = () => CancelOrder(this.WorkbenchGlobal, o); });
            //this.TaskList.Build(this.Town.CraftingManager.GetOrdersFor(global), f => f.Craft.GetProduct().Product.Name, (o, b) => { b.LeftClickAction = () => CancelOrder(this.WorkbenchGlobal, o); });
            this.TaskList.Build(orders, f => f.Craft.GetProduct().Product.Name, (o, b) =>
            {
                b.LeftClickAction = () => 
                CancelOrder(this.WorkbenchGlobal, o); 
            });

        }
        private void Refresh()
        {
            this.Refresh(this.WorkbenchGlobal, this.Workbench);
        }
        public override bool Toggle()
        {
            this.Refresh(this.WorkbenchGlobal, this.Workbench);
            return base.Toggle();
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.OrdersUpdated:
                    //if (e.Parameters[0] as BlockWorkbench.Entity == this.Workbench)
                        this.Refresh();
                    break;

                default:
                    break;
            }
        }

        //void CancelOrder(Vector3 global, CraftOrder op)
        //{
        //    byte[] data = Net.Network.Serialize(w =>
        //    {
        //        w.Write(this.WorkbenchGlobal);
        //        //var index = this.Workbench.QueuedOrders.ToList().FindIndex(c => c == op);
        //        //w.Write(index);
        //        w.Write(op.ID);
        //    });
        //    Net.Client.Instance.Send(Net.PacketType.CraftingOrderRemove, data);
        //}
        void CancelOrder(Vector3 global, CraftOrder op)
        {
            byte[] data = Net.Network.Serialize(w =>
            {
                w.Write(this.WorkbenchGlobal);
                var index = this.Workbench.GetOrders(this.Town.Net, global).FindIndex(c => c == op);
                w.Write(index);
                //w.Write(op.ID);
            });
            Net.Client.Instance.Send(Net.PacketType.CraftingOrderRemove, data);
        }
        private void ClearOrders()
        {
            byte[] data = Net.Network.Serialize(w =>
            {
                w.Write(this.WorkbenchGlobal);
            });
            Net.Client.Instance.Send(Net.PacketType.CraftingOrdersClear, data);
        }
    }
}
