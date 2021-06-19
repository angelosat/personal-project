using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    //class PartyMembers : List<GameObjectSlot>
    //{
    //    public PartyMembers(int capacity)
    //    {
    //        this.Capacity = capacity;
    //        int i = 0;
    //        while (i < capacity)
    //        {
    //            this.Add(GameObjectSlot.Empty);
    //            i++;
    //        }
    //    }
    //    public override string ToString()
    //    {
    //        string text = "";
    //        foreach (GameObjectSlot slot in this)
    //            text += slot.ToString() + "\n";
    //        //if (text.Length > 0)
    //       //     text = text.Remove(text.Length - 1);
    //   //     text.TrimEnd('\n');
    //        return text.TrimEnd('\n');
    //    }
    //}
    class PartyComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Party";
            }
        }

        public ItemContainer Members { get { return (ItemContainer)this["Members"]; } set { this["Members"] = value; } }

        public PartyComponent()
        {
            this.Members = new ItemContainer(1);
        }

        public PartyComponent Initialize(byte capacity)
        {
            Members = new ItemContainer(capacity);
            return this;
        } 

        public PartyComponent(byte capacity)
        {
            Members = new ItemContainer(capacity);
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            GameObjectSlot slot;
            switch (e.Type)
            {
                case Message.Types.Followed:
                    slot = Members.Find(foo => !foo.HasValue);
                    if (slot == null)
                        return true;
                    slot.Object = e.Sender;
                    return true;
                case Message.Types.Unfollowed:
                    slot = Members.Find(foo => foo.Object == e.Sender);
                    if (slot == null)
                        return true;
                    slot.Clear();
                    return true;
                //case Message.Types.Query:
                //    Query(parent, e);
                //    return true;
                case Message.Types.Activate:
                    throw new NotImplementedException();
                    //e.Sender.PostMessage(Message.Types.Activate, parent);
                    return true;
                default:
                    return false;
            }
        }
        public override object Clone()
        {
            PartyComponent comp = new PartyComponent((byte)this.Members.Capacity);
            //for (int i = 0; i < Members.Capacity; i++)
            //    comp.Members[i].Object = this.Members[i].Object;
            return comp;
        }

        //public override void Query(GameObject parent, GameObjectEventArgs e)
        //{
        //    //if (parent == e.Sender)
        //    //    return;
        //    //List<Interaction> list = e.Parameters[0] as List<Interaction>;
        //    //list.Add(new Interaction(TimeSpan.Zero, Message.Types.Activate, parent, name: "Follow")); 
        //}

    }
}
