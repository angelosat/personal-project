using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Items
{
    public abstract class Affix
    {
        public enum Types
        {
            //prefix
            Tremendous,
            Magnificent,
            Unbreakable,
            Furious,
            Durable,

            //suffix
            Slaying,
            Doom,
            Fire,
            MillionTruths,
            Shoveling
        }

        public static void Load() { }

        public string Name { get; protected set; }
        public Types ID { get; set; }
        public Action<GameObject> OnApply = (obj) => { };

        public abstract GameObject ApplyName(GameObject obj);
        public GameObject Apply(GameObject obj)
        {
            ApplyName(obj);
            OnApply(obj);
            return obj;
        }

        //static public GetAffix(
    }
}
