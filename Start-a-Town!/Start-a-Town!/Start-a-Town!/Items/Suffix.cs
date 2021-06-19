using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Items
{
	class Suffix : Affix
    {
         Suffix(Types id, string name)
        {
            this.ID = id;
            this.Name = name;
        }

        //public override GameObject Apply(GameObject obj)
        //{
        //    return obj.SetName(obj.Name + " of " + this.Name);
        //}

         static public Suffix Create(Types id, string name, Action<GameObject> onApply = null)
        {
            return new Suffix(id, name) { OnApply = onApply ?? (o => { }) };
        }

        public override GameObject ApplyName(GameObject obj)
        {
            return obj.SetName(obj.Name + " of " + this.Name);
        }
	}
}
