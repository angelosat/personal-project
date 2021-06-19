using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptUnpack : Script
    {
        public override Script.Types ID
        {
            get
            {
                return Script.Types.Unpack;
            }
        }
        public override string Name
        {
            get
            {
                return "Unpack";
            }
        }

        public ScriptUnpack()
        {
            this.AddComponent(new ScriptAnimation(Graphics.AnimationCollection.Working));
            
            //this.AddComponent(new ScriptTargetFilter(t => t.IsBlock()));
            this.AddComponent(new ScriptTimer(a => GetTimeInMs(a.Actor), "Unpacking", 1));//, this.Success));
            this.AddComponent(new ScriptRangeCheck(DefaultRangeCheck, InteractionOld.DefaultRange));
            this.AddComponent(new ScriptEvaluations(this.Fail,
                new ScriptEvaluation(
                    a =>
                    {
                        if (a.Target.Object.IsBlock())
                            return true;
                        ConstructionFootprint comp;
                        if (!a.Target.Object.TryGetComponent<ConstructionFootprint>(out comp))
                            return false;
                        //return comp.Product.Object.ID == a.Actor.GetComponent<GearComponent>().Holding.Object.GetComponent<PackageComponent>().Content.Object.ID;
                        return comp.Product.Object.ID == a.Actor.GetComponent<HaulComponent>().Holding.Object.GetComponent<PackageComponent>().Content.Object.ID;

                    }, Message.Types.InvalidTarget)));
            
        }

        public override void OnSuccess(ScriptArgs args)
        {
            //GameObjectSlot holding = args.Actor.GetComponent<GearComponent>().Holding;
            //GameObject content = args.Actor.GetComponent<GearComponent>().Holding.Object.GetComponent<PackageComponent>().Content.Object;
            GameObjectSlot holding = args.Actor.GetComponent<HaulComponent>().Holding;
            GameObject content = args.Actor.GetComponent<HaulComponent>().Holding.Object.GetComponent<PackageComponent>().Content.Object;
            content.Global = args.Target.FinalGlobal;
            args.Net.Spawn(content);
            args.Net.DisposeObject(holding.Object);
            holding.Clear();
            //args.Target.Object.GetComponent<PackageComponent>().Unpack(args.Net, args.Target.Object);
            if (args.Target.Object.HasComponent<ConstructionFootprint>())
            {
                args.Net.Despawn(args.Target.Object);
                args.Net.DisposeObject(args.Target.Object);
            }
        }

        public override object Clone()
        {
            return new ScriptUnpack();
        }
    }
}
