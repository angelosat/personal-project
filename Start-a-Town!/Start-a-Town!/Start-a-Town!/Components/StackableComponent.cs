using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components
{
    class StackableComponent : Component
    {
        public override string ComponentName
        {
            get { return "Stackable"; }
        }
        public override object Clone()
        {
            return new StackableComponent(this);
        }

        public int Capacity = 1;
        //public int Quantity;
        public StackableComponent()
        {

        }
        public StackableComponent(StackableComponent toCopy)
        {
            this.Capacity = toCopy.Capacity;
        }
        public StackableComponent(int capacity)
        {
            this.Capacity = capacity;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch(e.Type)
            {
                case Message.Types.Insert:
                    var toInsertSlot = (e.Parameters[0] as GameObjectSlot);
                    var toInsert = toInsertSlot.Object;
                    if(toInsert == null)
                        break;
                    if (toInsert.GetID() != parent.GetID())
                        break;
                    //if(toInsert.StackSize + parent.StackSize > this.Capacity)
                    //    break;
                    var remainder = toInsert.StackSize + parent.StackSize - this.Capacity;
                    if(remainder > 0)
                    {
                        parent.StackSize = this.Capacity;
                        toInsert.StackSize = remainder;
                    }
                    else
                    {
                        parent.StackSize += toInsert.StackSize;
                        //dispose input
                        //parent.Net.SyncDisposeObject(toInsert);
                        toInsertSlot.Dispose();
                    }

                    break;

                default:
                    break;
            }
            return false;
        }

        public void SetStacksize(GameObject parent, int quantity)
        {
            parent.StackSize = Math.Min(quantity, this.Capacity);
        }
    }
}
