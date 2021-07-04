using System.Collections.Generic;

namespace Start_a_Town_
{
    public class Utility
    {
        public enum Types { Sleeping, Eating };

        static public IEnumerable<Types> All()
        {
            yield return Types.Sleeping;
            yield return Types.Eating;
        }
    }
}
