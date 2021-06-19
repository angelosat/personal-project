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

namespace Start_a_Town_.Blocks.Workbench
{
    class InterfaceWorkbenchOrders : GroupBox
    {
        Panel Panel;
        BlockWorkbench.Entity Workbench;
        Vector3 WorkbenchGlobal;
        ListBox<CraftOrder, Label> TaskList;
        Town Town;
        public InterfaceWorkbenchOrders(Vector3 global, BlockWorkbench.Entity workbench)
        {
            this.Town = Engine.Map.GetTown();
            this.Workbench = workbench;
            this.WorkbenchGlobal = global;
            this.TaskList = new ListBox<CraftOrder, Label>(100, 200);
            this.Panel = new Panel() { AutoSize = true };
            this.Panel.Controls.Add(this.TaskList);
            this.Controls.Add(this.Panel);
        }
        public void Refresh(Vector3 global, BlockWorkbench.Entity workbench)
        {
            this.Workbench = workbench;
            this.WorkbenchGlobal = global;
            //this.TaskList.Build(this.Workbench.QueuedOrders, f => f.GetProduct().Product.Name, (o, b) => { b.LeftClickAction = () => CancelOrder(this.WorkbenchGlobal, o); });
            //this.TaskList.Build(this.Town.CraftingManager.GetOrdersFor(global), f => f.Craft.GetProduct().Product.Name, (o, b) => { b.LeftClickAction = () => CancelOrder(this.WorkbenchGlobal, o); });
            this.TaskList.Build(workbench.GetQueuedOrders(), f => f.Craft.GetProduct().Product.Name, (o, b) => { b.LeftClickAction = () => CancelOrder(this.WorkbenchGlobal, o); });

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
        internal override void OnGameEvent(Net.GameEvent e)
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

        void CancelOrder(Vector3 global, CraftOrder op)
        {
            byte[] data = Net.Network.Serialize(w =>
            {
                w.Write(this.WorkbenchGlobal);
                //var index = this.Workbench.QueuedOrders.ToList().FindIndex(c => c == op);
                //w.Write(index);
                w.Write(op.ID);
            });
            Net.Client.Instance.Send(Net.PacketType.CraftingOrderRemove, data);
        }
    }
}
