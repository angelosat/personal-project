using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;
using Start_a_Town_.Towns.Crafting;
using Start_a_Town_.Components;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Crafting
{
    public abstract class BlockEntityWorkstation : BlockEntity
    {
        //protected List<CraftOrder> PendingOrders = new List<CraftOrder>();

        /// <summary>
        /// make this a property that returns first item from this.getorders(this.global) ?
        /// </summary>
        public CraftOperation CurrentOrder;
        public bool ExecutingOrders;
        //public List<CraftOrder> AssignedOrders = new List<CraftOrder>();
        public abstract Container Input { get; }
        //public abstract Container Output { get; }
        public abstract Tokens.IsWorkstation.Types Type { get; }

        public override void Dispose()
        {
            this.Input.Dispose();
        }
        
        public bool MaterialsPresent(CraftOperation craft)
        {
            //var input = this.GetMaterialsContainer();
            var input = this.Input;

            var mats = craft.Materials;
            foreach (var req in mats)
                if (input.GetAmount(o => (int)o.ID == req.ObjectID) < req.Max)
                    return false;
            return true;
        }

        public CraftOperation GetCurrentOrder()
        {
            return this.CurrentOrder;

            //var order = this.PendingOrders.FirstOrDefault();
            //if (order == null)
            //    return null;
            //return order.Craft;// this.PendingOrders.FirstOrDefault();

            //var town = map.GetTown();
            //var order = town.CraftingManager.GetOrdersFor(global).FirstOrDefault();
            //if (order == null)
            //    return null;
            //return order.Craft;
        }
        //public abstract Container GetMaterialsContainer();

        internal void SetCurrentProject(IObjectProvider net, CraftOperation craft)
        {
            //var craft = new CraftOperation(product.)
            this.ExecutingOrders = false;
            this.CurrentOrder = craft;
            net.Map.EventOccured(Message.Types.WorkstationOrderSet, this);
        }

        //void DisableOrders(IObjectProvider net, Vector3 global)
        //{
        //    this.ExecutingOrders = false;
        //    net.Map.Town.CraftingManager.DisableWorkstation(global);
        //    //foreach(var order in this.GetQueuedOrders())
        //    //{
        //    //    //net.Map.Town.RemoveJob(order.Job);
        //    //    net.Map.Town.CraftingManager.RemoveOrder(order.ID);
        //    //}
        //}
        //void EnableOrders(IObjectProvider net, Vector3 global)
        //{
        //    if (this.ExecutingOrders)
        //        return;

        //    this.ExecutingOrders = true;
        //    net.Map.Town.CraftingManager.EnableWorkstation(global);

        //    //foreach(var order in this.)
        //}
        public bool Insert(GameObject material)
        {
            return this.Input.InsertObject(material);
        }
        internal List<CraftOrder> GetOrders(IObjectProvider net, Vector3 workstationGlobal)
        {
            //return this.AssignedOrders;
            return net.Map.Town.CraftingManager.GetOrders(workstationGlobal) ?? new List<CraftOrder>();
        }

    }
}
