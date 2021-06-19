using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class OwnershipComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Ownership";
            }
        }

        public GameObject Owner { get { return (GameObject)this["Owner"]; } set { this["Owner"] = value; } }

        public OwnershipComponent()
        {
            this.Owner = null;
        }

        public OwnershipComponent Initialize(GameObject owner = null)
        {
            this.Owner = owner;
            return this;
        }
        OwnershipComponent(GameObject owner = null)
        {
            this.Owner = owner;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.SetOwner:
                    GameObject newOwner = e.Parameters[0] as GameObject;
                    if (Owner != null)
                        if (Owner != newOwner)
                        {
                         //   Owner.HandleMessage(Message.Types.ModifyNeed, parent, "Bed", -100);
                            //Owner.PostMessage(Message.Types.Ownership, parent, parent);
                            e.Network.PostLocalEvent(this.Owner, ObjectEventArgs.Create(Message.Types.Ownership, new TargetArgs(parent), parent));
                        }
                    Owner = newOwner;
                    if (Owner != null)
                    {
                    //    Owner.HandleMessage(Message.Types.ModifyNeed, parent, "Bed", 100);
                        //Owner.PostMessage(Message.Types.Ownership, parent, parent);
                        e.Network.PostLocalEvent(this.Owner, ObjectEventArgs.Create(Message.Types.Ownership, new TargetArgs(parent), parent));
                    }
                    break;
                case Message.Types.Constructed:
                    Owner = e.Parameters[0] as GameObject;
                    break;
                //case Message.Types.Query:
                //    Query(parent, e);
                //    break;
                default:
                    return false;
            }
            return true;
        }

        public override void Query(GameObject parent, List<InteractionOld> list)
        {
            list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.ManageEquipment, source: parent, name: "Set owner", range: (a1,a2) => true));
        }

        public override object Clone()
        {
            return new OwnershipComponent(Owner);
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            tooltip.Controls.Add(new UI.Label(tooltip.Controls.Count > 0 ? tooltip.Controls.Last().BottomLeft : Vector2.Zero, "Owner: " + (Owner != null ? Owner.Name : "<None>"), fill: Color.Lime));
        }

        static public bool Owns(GameObject owner, GameObject obj)
        {
            OwnershipComponent ownership;
            if(!obj.TryGetComponent<OwnershipComponent>("Ownership", out ownership))
                return false;
            if (ownership.Owner == null)
                return true;
            return ownership.Owner == owner;
        }
    }
}
