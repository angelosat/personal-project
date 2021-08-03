using Start_a_Town_.UI;
using System;

namespace Start_a_Town_
{
    class StorageFilterNewNew : ILabeled, IListable
    {
        public StorageFilterCategoryNewNew Parent;
        public StorageFilterCategoryNewNew Root => this.Parent.Root;
        internal Stockpile Owner => this.Root.Owner;
        public string Label { get; set; }
        public Predicate<Entity> Condition;
        public bool Enabled => this.Owner.Accepts(this.Item, this.Material);
        readonly ItemDef Item;
        readonly MaterialDef Material;

        public StorageFilterNewNew(ItemDef item, MaterialDef mat)
        {
            this.Item = item;
            this.Material = mat;
            this.Label = mat.Label + " " + item.Label;
            this.Condition = o => o.Def == item && o.PrimaryMaterial == mat;
        }
        public StorageFilterNewNew(ItemDef item)
        {
            this.Item = item;
            this.Label = item.Label;
            this.Condition = o => o.Def == item;
        }

        public Control GetListControlGui()
        {
            return new CheckBoxNew(this.Label)
            {
                TickedFunc = () => this.Enabled,
                LeftClickAction = () => {
                    this.Root.FindLeafIndex(this, out var i);
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
