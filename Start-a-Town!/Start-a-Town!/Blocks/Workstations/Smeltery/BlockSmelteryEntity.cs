using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.Crafting;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Blocks.Smeltery
{
    public class BlockSmelteryEntityNew : BlockEntity
    {
        public BlockSmelteryEntityNew()
        {
            this.Comps.Add(new BlockEntityCompWorkstation(IsWorkstation.Types.Smeltery));
            this.Comps.Add(new BlockEntityCompDeconstructible());
            this.Comps.Add(new EntityCompRefuelable());
        }
        public override object Clone()
        {
            return new BlockSmelteryEntityNew();
        }
    }

    [Obsolete]
    public class BlockSmelteryEntity : BlockEntityWorkstation
    {
        public override IsWorkstation.Types Type { get { return IsWorkstation.Types.Smeltery; } }

        public BlockSmelteryEntity()
        {
            this.Comps.Add(new EntityCompRefuelable());
        }
        public enum States { Stopped, Running }
        public Progress Power { get { return this.GetComp<EntityCompRefuelable>().Fuel; } }
        public Progress SmeltProgress;
        States _State;
        public States State
        {
            get { return this._State; }
            set
            {
                this._State = value;
            }
        }

        public CraftOperation SelectedProduct;

        public override Container Input { get { return this.Storage; } }
        public Container Storage, Output, Fuels;

        public BlockSmelteryEntity(int inCapacity, int outCapacity, int fuelCapacity) : this()
        {
            this.State = States.Stopped;
            this.SmeltProgress = new Progress() { Max = 1 }; // TODO: make max relative to ore material melting point

            this.Fuels = new Container(fuelCapacity) { Name = "Fuel", Filter = item => item.Body.Material.Fuel.Value > 0 };
            this.Storage = new Container(inCapacity) { Name = "Input" };
            this.Output = new Container(outCapacity) { Name = "Output" };
        }


        public override void Break(IMap map, Vector3 global)
        {
            foreach (var slot in this.Storage.GetNonEmpty())
                map.Net.PopLoot(slot.Object, global, Vector3.Zero);
            foreach (var slot in this.Output.GetNonEmpty())
                map.Net.PopLoot(slot.Object, global, Vector3.Zero);
            foreach (var slot in this.Fuels.GetNonEmpty())
                map.Net.PopLoot(slot.Object, global, Vector3.Zero);
        }

        public override GameObjectSlot GetChild(string containerName, int slotID)
        {
            var dic = new Dictionary<string, Container>() {
                    {this.Storage.Name, this.Storage},
                    {this.Output.Name, this.Output},
                    {this.Fuels.Name, this.Fuels}
                };

            var slot = dic[containerName].GetSlot(slotID);
            return slot;
        }

        public bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Insert:
                    GameObjectSlot toInsert = e.Parameters[0] as GameObjectSlot;
                    if (!toInsert.HasValue)
                        throw new Exception("Null item");

                    Container target;
                    if (this.Storage.Filter(toInsert.Object))
                        target = this.Storage;
                    else if (this.Fuels.Filter(toInsert.Object))
                        target = this.Fuels;
                    else
                        return true;

                    target.Slots.Insert(toInsert);
                    return true;


                case Message.Types.SlotInteraction:
                    var actor = e.Parameters[0] as GameObject;
                    var slot = e.Parameters[1] as GameObjectSlot;
                    e.Network.PostLocalEvent(actor, Message.Types.Insert, slot);
                    return true;

                case Message.Types.Activate:
                    throw new NotImplementedException();

                case Message.Types.ArrangeInventory:
                    GameObjectSlot
                        sourceSlot = e.Parameters[0] as GameObjectSlot,
                        targetSlot = e.Parameters[1] as GameObjectSlot;
                    sourceSlot.Swap(targetSlot);
                    return true;


                default:
                    return false;
            }

        }
        internal override void HandleRemoteCall(IMap map, Vector3 global, ObjectEventArgs e)
        {
            var net = map.Net;
            switch (e.Type)
            {
                case Message.Types.Start:
                    if (this.State == States.Stopped)
                        this.Start(map, global);
                    else
                        this.Stop(map, global);
                    break;


                case Message.Types.SetProduct:
                    e.Data.Translate(net, r =>
                    {
                        var reactionID = r.ReadInt32();
                        var matCount = r.ReadInt32();
                        var mats = new List<ItemRequirement>();
                        for (int i = 0; i < matCount; i++)
                            mats.Add(new ItemRequirement(r));
                        var reaction = Reaction.Dictionary[reactionID];
                        var product = reaction.Products.First().GetProduct(reaction, null, mats);
                        var craft = new CraftOperation(reactionID, mats, null, null, this.Storage);
                        this.SelectedProduct = craft;

                    });
                    break;

                case Message.Types.AddProduct:
                    e.Data.Translate(net, r =>
                    {
                        var output = net.GetNetworkObject(r.ReadInt32());
                        this.Out(output);
                    });
                    break;

                case Message.Types.PlayerSlotRightClick:
                    var actor = e.Sender;
                    var child = e.Parameters[0] as GameObject;
                    var found = this.Fuels.Slots.Concat(this.Output.Slots).Concat(this.Storage.Slots).FirstOrDefault(s => s.Object == child);
                    actor.Net.PostLocalEvent(actor, Message.Types.Insert, found);
                    break;

                default:
                    break;
            }
        }

        private void Stop(IMap map, Vector3 global)
        {
            this.State = States.Stopped;
            map.SetBlockLuminance(global, 0);
        }

        private void Start(IMap map, Vector3 global)
        {
            if (this.SelectedProduct == null)
                return;
            if (!this.MaterialsAvailable())
                return;
            this.State = States.Running;
            map.SetBlockLuminance(global, 3);
        }

        private bool MaterialsAvailable()
        {
            foreach (var mat in this.SelectedProduct.Materials)
                if (this.Storage.Slots.GetAmount(obj => obj.GetID() == mat.ObjectID) < mat.AmountRequired)
                    return false;
            return true;
        }

        public override object Clone()
        {
            return new BlockSmelteryEntity(this.Storage.Capacity, this.Output.Capacity, this.Fuels.Capacity);
        }

        void Out(GameObject item)
        {
            var empty = this.Output.GetEmpty().FirstOrDefault();
            empty.Object = item;
        }

        public override void DrawUI(SpriteBatch sb, Camera cam, Vector3 global)
        {
            if (this.Power.Value == 0)
                RefuelIcon.DrawAboveEntity(sb, cam, global);
        }

        static public readonly Icon RefuelIcon = new Icon(ItemContent.LogsGrayscale);
    }
}
