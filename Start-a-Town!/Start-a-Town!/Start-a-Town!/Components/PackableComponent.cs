using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class PackableComponent : Component
    {
        public override string ComponentName
        {
            get { return "Packable"; }
        }
        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Build://Activate:
                   // Map.RemoveObject(parent);
                    parent.Despawn();

                    GameObject package = GameObject.Create(GameObject.Types.Package);
                    Chunk.AddObject(package, parent.Map, parent.Global);


                    //package.PostMessage(Message.Types.SetContent, parent, parent);
                    e.Network.PostLocalEvent(package, ObjectEventArgs.Create(Message.Types.SetContent, new TargetArgs(parent), parent));

                    break;

                //case Message.Types.Query:
                //    ////Dictionary<Message.Types, float> lengths = e.Parameters[0] as Dictionary<Message.Types, float>;
                //    ////lengths.Add(Message.Types.Mechanical, 100f);
                //    //Dictionary<Message.Types, Interaction> lengths = e.Parameters[0] as Dictionary<Message.Types, Interaction>;
                //    //lengths.Add(Message.Types.Mechanical, new Interaction(Message.Types.Mechanical, "Packing", length: 100f));
                //    Query(parent, e);
                //    return true;
                //    break;

                default:
                    break;
            }
            return true;
        }

        public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        {
           // List<Interaction> list = e.Parameters[0] as List<Interaction>;
            list.Add(new InteractionOld(new TimeSpan(0, 0, 1), Message.Types.Build, parent, "Pack", "Packing"));
            
        }
        
        public override object Clone()
        {
            return new PackableComponent();
        }

        //public override string GetWorldText(GameObject actor)
        //{
        //    return "Right click: Pack";
        //}
    }
}
