using System;

namespace Start_a_Town_
{
    [Obsolete]
    public struct Time : IComparable
    {
        DateTime Value;
        public Time(DateTime value)
        {
            this.Value = value;
        }
        public override string ToString()
        {
            return this.Value.ToString("MMM dd, HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-GB"));
        }
        public int CompareTo(object obj)
        {
            return this.CompareTo((Time)obj);
        }
        public int CompareTo(Time obj)
        {
            return Value.CompareTo(obj.Value);
        }
    }
}
