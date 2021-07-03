using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Blocks.Chest
{
    partial class BlockChest
    {
        public class BlockChestEntity : BlockEntity
        {
            public Container Container;


            public BlockChestEntity(int capacity)
            {
                this.Container = new Container(capacity) { Name = "Container" };
            }

            public override GameObjectSlot GetChild(string containerName, int slotID)
            {
                return this.Container.GetSlot(slotID);
            }

            static public void GetPlayerActionsWorld(Dictionary<PlayerInput, Interaction> actions)
            {
                actions.Add(PlayerInput.RButton, new InteractionCustom("Open", Activate));
            }
            static void Activate(GameObject a, TargetArgs t)
            {
                var comp = a.Map.GetBlockEntity(t.Global) as BlockChestEntity;
                var hauled = a.GetComponent<HaulComponent>().GetSlot();
                if (hauled.Object == null)
                    return;
                comp.Container.Slots.Insert(hauled);
                if (a.Net is Client)
                {
                    ShowUI(a, t);
                }
            }
            public override void OnRemove(IMap map, Vector3 global)
            {
                foreach(var slot in this.Container.GetNonEmpty())
                {
                    map.Net.PopLoot(slot.Object, global, Vector3.Zero);
                }
            }
            private static void ShowUI(GameObject a, TargetArgs t)
            {
                var entity = a.Map.GetBlockEntity(t.Global) as BlockChestEntity;
                var window = new WindowEntityInterface(entity, "Chest", () => t.Global);
                var ui = new ContainerUI().Refresh(t.Global, entity);
                window.Client.Controls.Add(ui);
                window.Show();
            }
            public override object Clone()
            {
                return new BlockChestEntity(this.Container.Capacity);
            }

            protected override void AddSaveData(SaveTag tag)
            {
                tag.Add(new SaveTag(SaveTag.Types.Compound, "Container", this.Container.Save()));
            }
            protected override void LoadExtra(SaveTag tag)
            {
                tag.TryGetTag("Container", t => this.Container.Load(t));
            }
            protected override void WriteExtra(System.IO.BinaryWriter io)
            {
                this.Container.Write(io);
            }
            protected override void ReadExtra(System.IO.BinaryReader io)
            {
                this.Container.Read(io);
            }
        }
    }
}
