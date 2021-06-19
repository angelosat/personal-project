using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.Resources
{
    class Durability : Resource
    {
        public override string ComponentName
        {
            get { return "Durability"; }
        }

        public override string Name
        {
            get { return "Durability"; }
        }

        public override Resource.Types ID
        {
            get { return Resource.Types.Durability; }
        }

        public override string Description
        {
            get { return "Basic Durability resource"; }
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            return base.HandleMessage(parent, e);
        }

        void Break(GameObject parent)
        {

        }

        public override string Format
        {
            get
            {
                return "##0";
            }
        }

        public override object Clone()
        {
            return new Durability();
        }
    }
}
