using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using System.IO;

namespace Start_a_Town_
{
    class EntityCompRefuelable : BlockEntityComp, IPowerSource
    {
        static int p;
        static EntityCompRefuelable()
        {
            p = Network.RegisterPacketHandler(ChangeFilters);
        }
        private static void ChangeFiltersSend(IObjectProvider net, Vector3 entity, int[] nodeIndices = null, int[] leafIndices = null)
        {
            var s = net.GetOutgoingStream();
            s.Write(p);
            s.Write(entity);
            s.Write(nodeIndices ?? new int[] { });
            s.Write(leafIndices ?? new int[] { });
        }
        private static void ChangeFilters(IObjectProvider net, BinaryReader r)
        {
            var entity = r.ReadVector3();
            var nodes = r.ReadIntArray();
            var items = r.ReadIntArray();
            net.Map.GetBlockEntity(entity).GetComp<EntityCompRefuelable>().ToggleItemFiltersCategories(nodes);
            net.Map.GetBlockEntity(entity).GetComp<EntityCompRefuelable>().ToggleItemFilters(items);
            if (net is Server)
                ChangeFiltersSend(net, entity, nodes, items);
        }

        public Progress Fuel = new();
        readonly List<ItemDefMaterialAmount> StoredFuelItems = new();
        
        public EntityCompRefuelable(int storedFuelCapacity = 100)
        {
            this.Fuel.Max = storedFuelCapacity;
        }
       
        public bool Accepts(Entity fuel)
        {
            return this.DefaultFilters.Filter(fuel);
        }
        internal float GetTotalStoredFuel()
        {
            return this.StoredFuelItems.Sum(o => o.Material.Fuel.Value * o.Amount);
        }
        private void StoreFuel(ItemDef iD, Material mat, int actualAmountToAdd)
        {
            var existing = this.StoredFuelItems.FirstOrDefault(o => o.Def == iD && o.Material == mat);
            if (existing == null)
                this.StoredFuelItems.Add(new ItemDefMaterialAmount(iD, mat, actualAmountToAdd));
            else
                existing.Amount += actualAmountToAdd;
        }
        public void ConsumePower(MapBase map, float amount)
        {
            this.Fuel.Value -= amount;

            if (this.Fuel.Value < this.GetTotalStoredFuel())
            {
                var firstFuel = this.StoredFuelItems[0];
                firstFuel.Amount -= 1;
                if (firstFuel.Amount == 0)
                {
                    this.StoredFuelItems.RemoveAt(0);
                    map.EventOccured(Components.Message.Types.FuelConsumed, this);
                }
            }
        }
        internal override void Remove(MapBase map, Vector3 global, BlockEntity parent)
        {
            if (map.Net is Net.Client)
                return;
            foreach (var oa in this.StoredFuelItems.ToList())
            {
                do
                {
                    var matEntity = oa.Create();
                    var amountToSpawn = Math.Min(oa.Amount, matEntity.StackMax);
                    if (amountToSpawn == 0)
                        throw new Exception();
                    matEntity.StackSize = amountToSpawn;
                    oa.Amount -= amountToSpawn;
                    map.Net.PopLoot(matEntity, global, Vector3.Zero);
                } while (oa.Amount > 0);
                this.StoredFuelItems.Remove(oa); // just in case
            }
        }
        internal override void OnDrop(GameObject actor, GameObject item, TargetArgs target, int quantity)
        {
            this.AddFuelNew(item, quantity);
        }
        public override void OnEntitySpawn(BlockEntity entity, MapBase map, Vector3 global)
        {
            var material = map.GetBlockMaterial(global);
            this.Fuel.Value += material.Fuel.Value;
        }
        private void AddFuelNew(GameObject item, int quantity)
        {
            var amount = quantity == -1 ? item.StackMax : quantity;
            if (amount == 0)
                throw new Exception();
            var fuel = item.Fuel;
            var fuelMissing = this.Fuel.Max - this.Fuel.Value;
            var desiredAmount = (int)(fuelMissing / fuel);
            var actualAmountToAdd = Math.Min(amount, desiredAmount);
            var actualFuelToAdd = actualAmountToAdd * fuel;
            item.StackSize -= actualAmountToAdd;

            // add fuel immediately or store item and consume it when power is requested?
            this.Fuel.Value += actualFuelToAdd;
            this.StoreFuel(item.Def, item.PrimaryMaterial, actualAmountToAdd);

            item.Map.EventOccured(Components.Message.Types.FuelConsumed, this);
        }

        internal override void GetSelectionInfo(IUISelection info, MapBase map, Vector3 vector3)
        {
            var bar = new Bar()
            {
                ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, this.Fuel.Percentage),
                Object = this.Fuel,
                Name = "Fuel: ",
            };
            bar.TextFunc = () => bar.Percentage.ToString("##0%");
            info.AddInfo(bar);

            var boxcontents = new ListBox<ItemDefMaterialAmount, Label>(150, Label.DefaultHeight * 3);
            void refreshContents()
            {
                boxcontents.Build(this.StoredFuelItems);
            }
            refreshContents();
            boxcontents.OnGameEventAction = e =>
            {
                if (e.Type == Components.Message.Types.FuelConsumed)
                    if (e.Parameters[0] == this)
                        refreshContents();
            };
            info.AddInfo(boxcontents);

            info.AddTabAction("Fuel", () =>
            {
                DefaultFilters.GetControl((n, l) => ChangeFiltersSend(map.Net, vector3, n, l)).ToWindow("Select permitted fuel").Toggle();
            });
        }
        
        private void ToggleItemFiltersCategories(int[] categoryIndices)
        {
            var indices = categoryIndices;
            foreach (var i in indices)
            {
                var c = DefaultFilters.GetNodeByIndex(i);
                var all = c.GetAllDescendantLeaves();
                var minor = all.GroupBy(a => a.Enabled).OrderBy(a => a.Count()).First();
                foreach (var f in minor)
                    f.Enabled = !minor.Key;
            }
        }
        private void ToggleItemFilters(int[] gameObjects)
        {
            var indices = gameObjects;
            foreach (var i in indices)
            {
                var f = DefaultFilters.GetLeafByIndex(i);
                f.Enabled = !f.Enabled;
            }
        }

        public bool HasAvailablePower(float amount)
        {
            return this.Fuel.Value >= amount;
        }

        public float GetRemaniningPower()
        {
            return this.Fuel.Value;
        }

        public override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.Fuel.Save("Fuel"));
            tag.Add(this.StoredFuelItems.SaveNewBEST("StoredFuelItems"));
        }

        public override void Load(SaveTag tag)
        {
            this.Fuel.Load(tag["Fuel"]);
            this.StoredFuelItems.TryLoadMutable(tag, "StoredFuelItems");
        }
        public override void Write(BinaryWriter w)
        {
            this.Fuel.Write(w);
            w.Write(this.StoredFuelItems);
        }
        public override ISerializable Read(BinaryReader r)
        {
            this.Fuel.Read(r);
            this.StoredFuelItems.ReadMutable(r);
            return this;
        }

        readonly StorageFilterCategoryNew DefaultFilters =
            new StorageFilterCategoryNew("Wood")
                .AddChildren(Def.Database.Values.OfType<ItemDef>()
                    .Where(d => d.DefaultMaterialType == MaterialType.Wood)
                    .Select(d => new StorageFilterCategoryNew(d.Label).AddLeafs(d.DefaultMaterialType.SubTypes.Select(m => new StorageFilterNew(d, m)))));
    }
}
