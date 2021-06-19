using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptPlanting : Script
    {
        public override Script.Types ID
        {
            get
            {
                return Script.Types.Planting;
            }
        }
        public override string Name
        {
            get
            {
                return "Planting";
            }
        }
        FarmlandComponent Farmland;// { get; set; }
        GameObjectSlot Seed;
        public ScriptPlanting()
        {
            //this.AddComponent(new ScriptTimer())
            this.AddComponent(new ScriptTargetFilter(t => t.TryGetComponent<FarmlandComponent>(out this.Farmland)));
            this.AddComponent(new ScriptRangeCheck(a => DefaultRangeCheck(a.Actor, a.Target, InteractionOld.DefaultRange)));
            this.AddComponent(new ScriptTimer(a => this.GetTimeInMs(a.Actor), "Planting", 1));//, this.Success));
            this.AddComponent(new ScriptEvaluations(this.Fail,
                new ScriptEvaluation(a =>
                {
                    //GameObject obj;
                    //if (GearComponent.TryGetObject(a.Actor, GearType.Hauling, out obj))
                    GameObject obj = a.Actor.GetComponent<HaulComponent>().GetObject();//.Slot.Object;
                    if (obj != null)
                        return obj.HasComponent<SeedComponent>();
                    return false;
                })));
            this.AddComponent(new ScriptAnimation(Graphics.AnimationCollection.Working));
        }
        public override void OnStart(ScriptArgs args)
        {
            //base.Start(args);
            //Seed = args.Actor.GetComponent<GearComponent>().Holding;
            Seed = args.Actor.GetComponent<HaulComponent>().Holding;

        }
        public override void Success()
        {
            base.Success();
            this.Farmland.Plant(this.ArgsSnapshot.Target.Object, this.ArgsSnapshot.Actor, this.Seed);
            SkillOld.Award(this.ArgsSnapshot.Net, this.ArgsSnapshot.Actor, this.ArgsSnapshot.Target.Object, SkillOld.Types.Argiculture, 1);
        }
        public override object Clone()
        {
            return new ScriptPlanting();
        }
    }
}
