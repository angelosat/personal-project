﻿using Start_a_Town_.UI;
using System;

namespace Start_a_Town_
{
    public class StorageFilterNewNew : ILabeled, IListable
    {
        public StorageFilterCategoryNewNew Parent;
        public StorageFilterCategoryNewNew Root => this.Parent.Root;
        internal Stockpile Owner => this.Root.Owner;
        public string Label { get; set; }
        public bool Enabled => this.Owner.Accepts(this.Item, this.Material, this.Variation);
        readonly ItemDef Item;
        readonly MaterialDef Material;
        readonly Def Variation;

        public StorageFilterNewNew(ItemDef item, MaterialDef mat)
        {
            this.Item = item;
            this.Material = mat;
            this.Label = mat.Label + " " + item.Label;
        }
        public StorageFilterNewNew(string label, ItemDef item, Def variation)
        {
            this.Item = item;
            this.Variation = variation;
            this.Label = label;
        }
        public StorageFilterNewNew(ItemDef item)
        {
            this.Item = item;
            this.Label = item.Label;
        }
        public Control GetListControlGui()
        {
            return new CheckBoxNew(this.Label)
            {
                TickedFunc = () => this.Enabled,
                LeftClickAction = () => {
                    this.Root.FindLeafIndex(this, out var i);
                    if (this.Variation is not null)
                        PacketStorageFiltersNew.Send(this.Owner, this.Item, this.Variation as Def);
                    else
                        PacketStorageFiltersNew.Send(this.Owner, this.Item, this.Material);
                    //PacketStorageFiltersNew.Send(this.Owner, null, new int[] { i });
                }
            };
        }
    }
    [Obsolete]
    class StorageFilterNew : ILabeled, IListable
    {
        public StorageFilterCategoryNew Parent;
        public StorageFilterCategoryNew Root => this.Parent.Root;

        public string Label { get; set; }
        public Predicate<Entity> Condition;
        public bool Enabled = true;
        internal Stockpile Owner => this.Root.Owner;

        public StorageFilterNew(ItemDef item, MaterialDef mat, bool enabled = true)
        {
            this.Label = mat.Label + " " + item.Label;
            this.Condition = o => o.Def == item && o.PrimaryMaterial == mat;
            this.Enabled = enabled;
        }
        public StorageFilterNew(ItemDef item, bool enabled = true)
        {
            this.Label = item.Label;
            this.Condition = o => o.Def == item;
            this.Enabled = enabled;
        }
        
        public Control GetListControlGui()
        {
            return new CheckBoxNew(this.Label)
            {
                TickedFunc = () => this.Enabled,
                LeftClickAction = () => {
                    this.Root.FindLeafIndex(this, out var i);
                    PacketStorageFiltersNew.Send(this.Owner, null, new int[] { i });
                }
            };
        }
    }
}
