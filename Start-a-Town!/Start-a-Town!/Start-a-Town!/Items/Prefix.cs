using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Items
{
    class Prefix : Affix
    {
        Prefix(Types id, string name)
        {
            this.ID = id;
            this.Name = name;
        }

        static public Prefix Create(Types id, string name, Action<GameObject> onApply = null)
        {
            return new Prefix(id, name) { OnApply = onApply ?? (o => { }) };
        }

        //public override GameObject Apply(GameObject obj)
        //{
        //    obj.SetName(this.Name + " " + obj.Name);
        //    OnApply(obj);
        //    return obj;
        //}

        public override GameObject ApplyName(GameObject obj)
        {
            return obj.SetName(this.Name + " " + obj.Name);
        }
    }
}
