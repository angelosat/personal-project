using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Blocks.Chest
{
    partial class BlockChest
    {
        public class Entity : BlockEntity
        {
            public Container Container;
            public Entity(int capacity)
            {
                this.Container = new Container(capacity) { Name = "Container" };
            }

            public override GameObjectSlot GetChild(string containerName, int slotID)
            {
                return this.Container.GetSlot(slotID);
            }

            static public void GetPlayerActionsWorld(Dictionary<PlayerInput, Interaction> actions)
            {
                //actions.Add(PlayerInput.Activate, new InteractionActivate());
                //actions.Add(PlayerInput.ActivateHold, new InteractionInsert());
                actions.Add(PlayerInput.RButton, new InteractionCustom("Open", Activate));


            }
            static void Activate(GameObject a, TargetArgs t)
            {
                var comp = a.Map.GetBlockEntity(t.Global) as Entity;
                var hauled = a.GetComponent<HaulComponent>().GetSlot();//.Slot;
                comp.Container.Slots.Insert(hauled);
                if (a.Net is Net.Client)
                {
                    ShowUI(a, t);
                }
            }
            public override void Remove(IMap map, Vector3 global)
            {
                foreach(var slot in this.Container.GetNonEmpty())
                {
                    map.GetNetwork().PopLoot(slot.Object, global, Vector3.Zero);
                }
            }
            private static void ShowUI(GameObject a, TargetArgs t)
            {
                var entity = a.Map.GetBlockEntity(t.Global) as Entity;
                var window = new WindowEntityInterface(entity, "Chest", () => t.Global);
                var ui = new ContainerUI().Refresh(t.Global, entity);
                window.Client.Controls.Add(ui);
                window.Show();
            }
            public override object Clone()
            {
                return new Entity(this.Container.Capacity);
            }

            public override SaveTag Save(string name)
            {
                SaveTag save = new SaveTag(SaveTag.Types.Compound, name);//, this.Container.Save());
                save.Add(new SaveTag(SaveTag.Types.Compound, "Container", this.Container.Save()));
                return save;
            }
            public override void Load(SaveTag tag)
            {
                tag.TryGetTag("Container", t => this.Container.Load(t));
            }
            public override void Write(System.IO.BinaryWriter io)
            {
                this.Container.Write(io);
            }
            public override void Read(System.IO.BinaryReader io)
            {
                this.Container.Read(io);
            }

            public class InteractionActivate : Interaction
            {
                public InteractionActivate()
                {
                    this.Name = "Open";
                }
                static readonly TaskConditions conds = new TaskConditions(new AllCheck(new RangeCheck()));
                public override TaskConditions Conditions
                {
                    get
                    {
                        return conds;
                    }
                }
                public override void Perform(GameObject a, TargetArgs t)
                {
                    if (a.Net is Net.Client)
                    {
                        ShowUI(a, t);
                    }
                }

                public override object Clone()
                {
                    return new InteractionActivate();
                }
            }
            class InteractionInsert : Interaction
            {
                public InteractionInsert()
                {
                    this.Name = "Insert";
                }
                static readonly TaskConditions conds = new TaskConditions(new AllCheck(new RangeCheck()));
                public override TaskConditions Conditions
                {
                    get
                    {
                        return conds;
                    }
                }
                public override void Perform(GameObject a, TargetArgs t)
                {
                    var comp = a.Map.GetBlockEntity(t.Global) as Entity;
                    //var hauled = GearComponent.GetSlot(a, GearType.Hauling);
                    var hauled = a.GetComponent<HaulComponent>().GetSlot();//.Slot;

                    if (hauled.Object == null)
                        return;

                    comp.Container.Slots.Insert(hauled);
                }
                public override object Clone()
                {
                    return new InteractionInsert();
                }
            }

            
        }
    }
}
