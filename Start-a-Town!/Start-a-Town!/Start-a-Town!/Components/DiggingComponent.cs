using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class DiggingComponent : Component
    {
        public DiggingComponent()
        {
            Properties.Add("Health", 100f);
        }
        public override bool HandleMessage(GameObject parent, GameObject sender, Message.Types msg)
        {
            switch (msg)
            {
                case Message.Types.Attack:
                   // Log.Write(Log.EntryTypes.Default, "diggy");
                    this["Health"] = GetProperty<float>("Health") - GlobalVars.DeltaTime;
                    if (GetProperty<float>("Health") < 0)
                    {
                        parent.HandleMessage(parent, Message.Types.Death);
                        return true;
                    }
                   // Log.Write(Log.EntryTypes.Default, GetProperty<float>("Health").ToString());
                    return false;
                    break;
                default:
                    break;
            }
            return true;
        }

        public override object Clone()
        {
            DiggingComponent comp = new DiggingComponent();
            foreach (KeyValuePair<string, object> parameter in Properties)
            {
                comp[parameter.Key] = parameter.Value;
            }
            return comp;
        }

    }
}
