using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Components.Containers
{
    class StorageComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "ItemContainer"; }
        }

        //ItemContainer Children { get { return (ItemContainer)this["Children"]; } set { this["Children"] = value; } }
        public Container Container;
        public byte Capacity { get { return (byte)this["Capacity"]; } set { this["Capacity"] = value; } }
        GameObject Parent { get { return (GameObject)this["Parent"]; } set { this["Parent"] = value; } }
        public override void MakeChildOf(GameObject parent)
        {
            //this.Parent = parent;
            parent.RegisterContainer(this.Container);
        }
        //public StorageComponent Initialize(ItemContainer container)
        //{
        //    this.Capacity = container.Capacity;
        //    //this.Children = container;
        //    this.Container = new Container() { Name = "Container" };
        //    return this;
        //}
        public StorageComponent Initialize(byte capacity)
        {
            this.Capacity = capacity;
            //this.Children = new ItemContainer(this.Parent, this.Capacity, () => this.Parent.ChildrenSequence);
            this.Container = new Container(capacity) { Name = "Container" };

            return this;
        }
        //public override void ObjectCreated(GameObject parent)
        //{
        //    this.Children = new ItemContainer(this.Parent, this.Capacity, () => parent.ChildrenSequence);
        //    //this.Children.Parent = parent;
        //}
        //public override void ObjectSynced(GameObject parent)
        //{
        //    this.Children = new ItemContainer(this.Capacity, parent.ChildrenSequence);
        //}
        //public override void GetChildren(List<GameObjectSlot> list)
        //{
        //    list.AddRange(this.Children);
        //}
        public override void GetContainers(List<Container> list)
        {
            list.Add(this.Container);
        }
        public StorageComponent()
        {
            //this.Children = new ItemContainer();
            this.Container = new Container() { Name = "Container" };
            this.Parent = null;
        }
        public StorageComponent(int capacity)
        {
            this.Container = new Container(capacity) { Name = "Container" };
            this.Parent = null;
        }
        //public override void Query(GameObject parent, List<InteractionOld> list)
        //{
        //    list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.Open, parent, "Open"));
        //}

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                //case Message.Types.Activate: // TODO: refactor this for all components that have similar functionality (workbenches, furnaces, etc)
                //    GameObject actor = e.Parameters[0] as GameObject;
                //    GameObjectSlot slot = actor.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling];
                //    if (slot.HasValue)
                //        this.Children.InsertObject(e.Network, slot);
                //    return true;

                case Message.Types.SlotInteraction:
                    var actor = e.Parameters[0] as GameObject;
                    var slot = e.Parameters[1] as GameObjectSlot;
                    e.Network.PostLocalEvent(actor, Message.Types.Insert, slot);
                    //actor.GetComponent<WorkComponent>().Perform(actor, new Components.Interactions.PickUp(), new TargetArgs(parent));
                    return true;

                default:
                    return true;
            }
        }

        public override object Clone()
        {
            //return new StorageComponent().Initialize(this.Capacity);
            StorageComponent comp = new StorageComponent(this.Capacity) { Parent = this.Parent };

            using (BinaryWriter w = new BinaryWriter(new MemoryStream()))
            {
                this.Write(w);
                w.BaseStream.Position = 0;
                using (BinaryReader r = new BinaryReader(w.BaseStream))
                    comp.Read(r);
            }

            return comp;
        }

        public override void Instantiate(GameObject parent, Action<GameObject> instantiator)
        {
            //foreach (var slot in
            //    from slot in this.Children// parent.GetChildren()
            //    where slot.HasValue
            //    select slot)
            //{
            //    slot.Object.Instantiate(instantiator);
            //}
        }

        public override void GetClientActions(GameObject parent, List<ContextAction> actions)
        {
            actions.Add(new ContextAction(() => "Interface", () =>
            {
                parent.GetUi().Show();
                //return true;
            }));
        }

        public override void GetRightClickActions(GameObject parent, List<ContextAction> actions)
        {
            actions.Add(new ContextAction(() => "Interface", () =>
            {
                parent.GetUi().Show();
                //return true;
            }));
        }

        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> actions)
        {
            actions.Add(PlayerInput.Activate, new InteractionActivate(parent));
            actions.Add(PlayerInput.ActivateHold, new InteractionInsert(parent));
        }

        public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<GameEvent>> handlers)// List<EventHandler<ObjectEventArgs>> handlers)
        {
            ui.Controls.Add(new ContainerUI().Refresh(parent));
            return;
            ui.Controls.Add(new SlotGrid(this.Container.Slots, parent, 4, s =>
            {
                s.DragDropAction = (args) =>
                {
                    var a = args as DragDropSlot;
                    Net.Client.PlayerInventoryOperationNew(a.Source, s.Tag, a.Slot.Object.StackSize);
                    return DragDropEffects.Move;
                };
                s.RightClickAction = () =>
                {
                    if (s.Tag.HasValue)
                        Net.Client.PlayerSlotInteraction(s.Tag);
                };
            }));
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            this.Container.Write(writer);
            //this.Children.Write(writer);
            //writer.Write(this.Capacity);
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            //this.Children = ItemContainer.Create(this.Parent, reader);
            this.Container.Read(reader);
            //this.Initialize(reader.ReadByte());
        }

        internal override List<SaveTag> Save()
        {
            return new List<SaveTag>()
            {
                new SaveTag(SaveTag.Types.Compound, "Container", this.Container.Save())
                //new SaveTag(SaveTag.Types.Compound, "Slots", this.Children.Save())
            };
        }
        internal override void Load(SaveTag compTag)
        {
            //this.Children = ItemContainer.Create(this.Parent, compTag["Slots"]);
            compTag.TryGetTag("Container", tag => this.Container.Load(tag));
        }

        class InteractionActivate : Interaction
        {
            GameObject Parent;
            public InteractionActivate(GameObject parent)
            {
                this.Parent = parent;
                this.Name = "Interface";
            }
            public override void Perform(GameObject a, TargetArgs t)
            {
                if (a.Net is Net.Client)
                    this.Parent.GetUi().Show();
            }
            public override object Clone()
            {
                return new InteractionActivate(this.Parent);
            }
        }
        class InteractionInsert : Interaction
        {
            GameObject Parent;
            public InteractionInsert(GameObject parent)
            {
                this.Parent = parent;
                this.Name = "Insert";
            }
            public override void Perform(GameObject a, TargetArgs t)
            {
                StorageComponent comp = this.Parent.GetComponent<StorageComponent>();
                //var hauled = GearComponent.GetSlot(a, GearType.Hauling);
                var hauled = a.GetComponent<HaulComponent>().GetSlot();//.Slot;

                if (hauled.Object == null)
                    return;
                comp.Container.InsertObject(hauled);
            }
            public override object Clone()
            {
                return new InteractionInsert(this.Parent);
            }
        }
    }
}
