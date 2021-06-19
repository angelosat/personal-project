using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptPickUp : Script
    {
        public override Script.Types ID
        {
            get
            {
                return Script.Types.PickUp;
            }
        }
        public override string Name
        {
            get
            {
                return "Pick Up";
            }
        }
        public ScriptPickUp()
        {
            this.AddComponent(new ScriptEvaluations(this.Fail,
                new ScriptEvaluation(
                    a =>
                    {
                        //return a.Target.Object.HasComponent<PhysicsComponent>();
                        PhysicsComponent c;
                        if (a.Target.Object.TryGetComponent<PhysicsComponent>(out c))
                            return c.Size == ObjectSize.Inventoryable;
                        return false;
                    },
                    Message.Types.InvalidTarget
                    )));
        }
        public override bool Evaluate(ScriptArgs args)
        {
            if (args.Target.Object.IsNull())
                return false;
            return base.Evaluate(args);
        }
        //public override void OnSuccess(ScriptArgs args)
        //{
        //    //base.Success(args);
        //    "pickup".ToConsole();
        //}
        //public override void Start(ScriptArgs args)
        
        public override void OnStart(ScriptArgs args)
        {
          //  base.Start(args);

            // if hauling something, drop it and return
            //if (args.Actor.TryGetComponent<PersonalInventoryComponent>(a =>
            //{
            //    args.Net.PostLocalEvent(args.Actor, Message.Types.drop)
            //}))

            //if(args.Actor.GetComponent<PersonalInventoryComponent>().Throw(args.Net, Vector3.Zero, args.Actor))
            //    return;
            //if (!args.Target.Object.IsNull())
            //    //return;
            args.Net.PostLocalEvent(args.Actor, Message.Types.Insert, args.Target.Object.ToSlot());
            Finish(args);
        }
        public override object Clone()
        {
            return new ScriptPickUp();
        }
    }
}
