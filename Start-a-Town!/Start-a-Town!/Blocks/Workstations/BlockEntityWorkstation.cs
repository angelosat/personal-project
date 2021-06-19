using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;
using Start_a_Town_.Towns.Crafting;
using Start_a_Town_.Components;
using Start_a_Town_.Blocks;
using Start_a_Town_.UI;
using Start_a_Town_.Modules.Crafting;

namespace Start_a_Town_.Crafting
{
    public abstract class BlockEntityWorkstation : BlockEntity
    {
        //protected List<CraftOrder> PendingOrders = new List<CraftOrder>();

        /// <summary>
        /// make this a property that returns first item from this.getorders(this.global) ?
        /// </summary>
        public CraftOperation CurrentOrder;
        public CraftOperationNew CurrentOperation;
        public GameObject CurrentWorker;
        public List<CraftOrderNew> Orders = new List<CraftOrderNew>();
        public bool ExecutingOrders;
        //public List<CraftOrder> AssignedOrders = new List<CraftOrder>();
        public abstract Container Input { get; }
        //public abstract Container Output { get; }
        public abstract IsWorkstation.Types Type { get; }

        internal CraftOrderNew GetOrder(string uniqueID)
        {
            return this.Orders.Find(o => o.GetUniqueLoadID() == uniqueID);
        }
        internal CraftOrderNew GetOrder(int uniqueID)
        {
            return this.Orders.Find(o => o.ID == uniqueID);
        }
        internal bool Reorder(int orderID, bool increasePriority)
        {
            var order = this.GetOrder(orderID);
            if (order == null)
                return false;
            var prevIndex = this.Orders.IndexOf(order);
            this.Orders.Remove(order);
            var newIndex = Math.Max(0, Math.Min(this.Orders.Count, prevIndex + (increasePriority ? -1 : 1)));
            this.Orders.Insert(newIndex, order);
            return true;
        }
        //public List<CraftOrder>

        //public override void Dispose()
        //{
        //    this.Input.Dispose();
        //}

        public override List<GameObjectSlot> GetChildren()
        {
            if (this.Input != null)
                return this.Input.Slots;
            return new List<GameObjectSlot>();
        }

        public bool MaterialsPresent(CraftOperation craft)
        {
            //var input = this.GetMaterialsContainer();
            var input = this.Input;

            var mats = craft.Materials;
            foreach (var req in mats)
                if (input.GetAmount(o => (int)o.IDType == req.ObjectID) < req.AmountRequired)
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
            //craft.Container = this.Input;
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
        //internal bool Insert(GameObject source, int quantity)
        //{
        //    GameObject input = quantity == source.StackSize ? source : source.Clone().SetStack(quantity);
        //    GameObjectSlot slot;
        //    if(!this.Input.InsertObject(input, out slot))
        //    {
        //        return false;
        //    }
        //    if (slot.Object == null)
        //    {
        //        var server = source.Net as Server;
        //        if (server != null)
        //        {
        //            server.InstantiateAndSync(input);
        //            slot.Object = input;
        //            server.SyncSlots(new TargetArgs( slot));
        //            //source.Net.InstantiateInContainer(input, this.);
        //        }
        //    }
        //    else
        //        slot.Object.StackSize += quantity;
        //    source.StackSize -= quantity;
            
        //    return true;
        //}
        public bool Insert(Entity material)
        {
            return this.Input.InsertObject(material);
        }

        //internal List<CraftOrder> GetOrders(IObjectProvider net, Vector3 workstationGlobal)
        //{
        //    //return this.AssignedOrders;
        //    return net.Map.Town.CraftingManager.GetOrders(workstationGlobal) ?? new List<CraftOrder>();
        //}

        public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera cam, Vector3 global)
        {
            //Bar.Draw(sb, cam, global + Vector3.UnitZ, "", this.CraftProgress.Percentage, cam.Zoom * .2f);
            
            var craft = this.GetCurrentOrder();
            if (craft != null)
                Bar.Draw(sb, cam, global, "", craft.CraftProgress.Percentage, cam.Zoom * .2f);
            //Bar.Draw(sb, cam, global + Vector3.UnitZ, "", craft.CraftProgress.Percentage, cam.Zoom * .2f);

            ////Rectangle bounds = camera.GetScreenBounds(global, parent.GetSprite().GetBounds());
            //Vector2 scrLoc = cam.GetScreenPositionFloat(global);// new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
            //Vector2 barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
            //Vector2 textLoc = new Vector2(barLoc.X, scrLoc.Y);
            //InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, this.CraftProgress.Percentage);
            ////UIManager.DrawStringOutlined(sb, this.Name + this.Time.TotalSeconds.ToString(" #0.##s"), textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
            //UIManager.DrawStringOutlined(sb, "test", textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
        }

        //public override SaveTag Save(string name)
        //{
        //    SaveTag tag = new SaveTag(SaveTag.Types.Compound, name);
        //    tag.Add(new SaveTag(SaveTag.Types.Compound, "Input", this.Input.Save()));
        //    if (this.CurrentOrder != null)
        //        tag.Add(this.CurrentOrder.Save("Current"));
        //    //tag.Add(new SaveTag(SaveTag.Types.Compound, "Current", this.CurrentOrder != null ? this.CurrentOrder.Save() : null));
        //    return tag;
        //}

        //public override void Load(SaveTag tag)
        //{
        //    tag.TryGetTag("Input", t => this.Input.Load(t));
        //    tag.TryGetTag("Current", v => this.CurrentOrder = new CraftOperation(v));
        //}

        //public override void Write(System.IO.BinaryWriter w)
        //{
        //    //this.Input.Write(w);
        //    //if (this.CurrentOrder != null)
        //    //{
        //    //    w.Write(true);
        //    //    this.CurrentOrder.Write(w);
        //    //}
        //    //else
        //    //    w.Write(false);
        //}
        //public override void Read(System.IO.BinaryReader r)
        //{
        //    //this.Input.Read(r);
        //    //if (r.ReadBoolean())
        //    //    this.CurrentOrder = new CraftOperation(r);

        //}

        protected override void AddSaveData(SaveTag tag)
        {
            tag.TrySaveRefs(this.Orders, "Orders");
        }
        protected override void LoadExtra(SaveTag tag)
        {
            //this.Orders = tag.LoadRefs<CraftOrderNew>("Orders");
            tag.TryLoadRefs<CraftOrderNew>("Orders", ref this.Orders);
        }
        protected override void WriteExtra(BinaryWriter w)
        {


            this.Orders.Write(w);

        }
        protected override void ReadExtra(BinaryReader r)
        {

            this.Orders.Read(r);//<List<CraftOrderNew>, CraftOrderNew>(r);
        }
    }
}
