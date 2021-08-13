﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using System.IO;
using System.Collections.ObjectModel;

namespace Start_a_Town_
{
    class BlockEntityCompRefuelable : BlockEntityComp, IPowerSource, IStorageNew
    {
        [EnsureStaticCtorCall]
        static class Packets
        {
            static readonly int pCategory, pMaterial, pVariation;
            static Packets()
            {
                pCategory = Network.RegisterPacketHandler(ReceiveCategory);
                pMaterial = Network.RegisterPacketHandler(ReceiveMaterial);
                pVariation = Network.RegisterPacketHandler(ReceiveVariation);
            }

            private static void ReceiveMaterial(INetwork net, BinaryReader r)
            {
                var comp = getComp(net, r);
                var item = Def.GetDef<ItemDef>(r);
                var material = Def.GetDef<MaterialDef>(r);
                comp.Settings.Toggle(item, material);
                if (net is Server)
                    Send(comp, item, material);
            }

            private static void ReceiveVariation(INetwork net, BinaryReader r)
            {
                var comp = getComp(net, r);
                var item = Def.GetDef<ItemDef>(r);
                var def = Def.GetDef<Def>(r);
                comp.Settings.Toggle(item, def);
                if (net is Server)
                    Send(comp, item, def);
            }

            private static void ReceiveCategory(INetwork net, BinaryReader r)
            {
                var comp = getComp(net, r);
                var cat = r.ReadString() is string catName && !catName.IsNullEmptyOrWhiteSpace() ? Def.GetDef<ItemCategory>(catName) : null;
                comp.Settings.Toggle(cat);
                if (net is Server)
                    Send(comp, cat);
            }

            internal static void Send(BlockEntityCompRefuelable owner, ItemDef item, Def def)
            {
                var parent = owner.Parent;
                var w = parent.Map.Net.GetOutgoingStream();
                w.Write(def is MaterialDef ? pMaterial : pVariation);
                w.Write(parent.OriginGlobal);
                item.Write(w);
                def.Write(w);
            }
            internal static void Send(BlockEntityCompRefuelable owner, ItemCategory category)
            {
                var parent = owner.Parent;
                var w = parent.Map.Net.GetOutgoingStream();
                w.Write(pCategory);
                w.Write(parent.OriginGlobal);
                //category.Write(w);
                w.Write(category?.Name ?? "");
            }
            private static BlockEntityCompRefuelable getComp(INetwork net, BinaryReader r)
            {
                var global = r.ReadIntVec3();
                return net.Map.GetBlockEntity(global).GetComp<BlockEntityCompRefuelable>();
            }
        }
      
        public Progress Fuel = new();
        readonly ObservableCollection<ItemMaterialAmount> StoredFuelItems = new();
        public StorageSettings Settings { get; } = new();

        public BlockEntityCompRefuelable(int storedFuelCapacity = 100)
        {
            this.Fuel.Max = storedFuelCapacity;
            this.Settings.Initialize(DefaultFiltersNew);
        }

        public bool Accepts(Entity fuel)
        {
            return this.Settings.Accepts(fuel);
        }
        internal float GetTotalStoredFuel()
        {
            return this.StoredFuelItems.Sum(o => o.Material.Fuel.Value * o.Amount);
        }
        private void StoreFuel(ItemDef iD, MaterialDef mat, int actualAmountToAdd)
        {
            var existing = this.StoredFuelItems.FirstOrDefault(o => o.Item == iD && o.Material == mat);
            if (existing == null)
                this.StoredFuelItems.Add(new ItemMaterialAmount(iD, mat, actualAmountToAdd));
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
                    this.StoredFuelItems.RemoveAt(0);
            }
        }
        internal override void Remove(MapBase map, IntVec3 global, BlockEntity parent)
        {
            foreach (var i in this.StoredFuelItems)
            {
                var item = i.Create();
                map.Net.PopLoot(item, global, Vector3.Zero);
            }
            //if (map.Net is Net.Client)
            //    return;
            //foreach (var oa in this.StoredFuelItems.ToList())
            //{
            //    do
            //    {
            //        var matEntity = oa.Create();
            //        var amountToSpawn = Math.Min(oa.Amount, matEntity.StackMax);
            //        if (amountToSpawn == 0)
            //            throw new Exception();
            //        matEntity.StackSize = amountToSpawn;
            //        oa.Amount -= amountToSpawn;
            //        map.Net.PopLoot(matEntity, global, Vector3.Zero);
            //    } while (oa.Amount > 0);
            //    this.StoredFuelItems.Remove(oa); // just in case
            //}
        }
        internal override void OnDrop(GameObject actor, GameObject item, TargetArgs target, int quantity)
        {
            this.AddFuelNew(item, quantity);
        }
        public override void OnEntitySpawn(BlockEntity entity, MapBase map, IntVec3 global)
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
            if (quantity > desiredAmount)
                throw new Exception();
            var actualAmountToAdd = Math.Min(amount, desiredAmount);
            var actualFuelToAdd = actualAmountToAdd * fuel;
            item.StackSize -= actualAmountToAdd;

            // add fuel immediately or store item and consume it when power is requested?
            this.Fuel.Value += actualFuelToAdd;
            this.StoreFuel(item.Def, item.PrimaryMaterial, actualAmountToAdd);
        }
        public int GetCapacityFor(Entity item)
        {
            var fuel = item.Fuel;
            var fuelMissing = this.Fuel.Max - this.Fuel.Value;
            return (int)(fuelMissing / fuel);
        }
        internal override void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
            var bar = new Bar()
            {
                ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, this.Fuel.Percentage),
                Object = this.Fuel,
                Name = "Fuel: ",
            };
            bar.TextFunc = () => bar.Percentage.ToString("##0%");
            info.AddInfo(bar);
            var box = new ScrollableBoxNewNew(150, Label.DefaultHeight * 2, ScrollModes.Vertical);
            var boxcontents = new ListBoxObservable<ItemMaterialAmount>(this.StoredFuelItems);
            box.AddControls(boxcontents);
            info.AddInfo(box);
            info.AddTabAction("Fuel", this.ToggleFiltersGui);
        }
        
        static Control FiltersGui;
        void ToggleFiltersGui()
        {
            DefaultFiltersNew.SetOwner(this);
            if (FiltersGui is null)
            {
                FiltersGui = DefaultFiltersNew.GetGui();
                FiltersGui.ToPanel().ToWindow("Fuel filters");
            }
            FiltersGui.GetWindow().Toggle();
            FiltersGui.SetOnSelectedTargetChangedAction(t =>
            {
                if (this.Parent.Map.GetBlockEntity(t.Global) == this.Parent)
                    DefaultFiltersNew.SetOwner(this);
            });
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

        public bool Accepts(ItemDef item, MaterialDef material, Def variation)
        {
            return this.Settings.Accepts(item, material, variation);
        }

        public void FiltersGuiCallback(ItemDef item, MaterialDef material)
        {
            Packets.Send(this, item, material);
        }

        public void FiltersGuiCallback(ItemDef item, Def variation)
        {
            Packets.Send(this, item, variation);
        }

        public void FiltersGuiCallback(ItemCategory category)
        {
            Packets.Send(this, category);
        }

        public Dictionary<TargetArgs, int> GetPotentialHaulTargets(Actor actor, GameObject item, out int maxamount)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TargetArgs> GetPotentialHaulTargets(Actor actor, GameObject item)
        {
            throw new NotImplementedException();
        }

        static readonly StorageFilterCategoryNewNew DefaultFiltersNew =
            new StorageFilterCategoryNewNew("Wood")
                .AddChildren(Def.Database.Values.OfType<ItemDef>()
                    .Where(d => d.DefaultMaterialType == MaterialTypeDefOf.Wood)
                    .Select(d => new StorageFilterCategoryNewNew(d.Label).AddLeafs(d.DefaultMaterialType.SubTypes.Select(m => new StorageFilterNewNew(d, m)))));
    }
}
