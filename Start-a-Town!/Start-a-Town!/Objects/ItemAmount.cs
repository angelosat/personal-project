namespace Start_a_Town_
{
    public struct ItemAmount
    {
        Entity _Item;
        int _Amount;
        public Entity Item => this._Item;
        public int Amount => this._Amount;

        public static bool operator ==(ItemAmount a, ItemAmount b)
        {
            return a.Item == b.Item && a.Amount == b.Amount;
        }
        public static bool operator !=(ItemAmount a, ItemAmount b)
        {
            return !(a == b);
        }
        public bool Equals(ItemAmount other)
        {
            return this == other;
        }
        public override int GetHashCode()
        {
            return (this.Item, this.Amount).GetHashCode();
        }
    }
}
