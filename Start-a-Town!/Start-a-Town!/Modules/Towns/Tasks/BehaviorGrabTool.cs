using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    class BehaviorGrabTool : BehaviorSelector
    {
        public BehaviorGrabTool()
        {
            this.Children = new List<Behavior>(){
                    BehaviorHelper.ToolNotRequired(),
                    new BehaviorCustom()
                    {
                        //SuccessCondition = (a) =>
                        //{
                        //    var tool = a.CurrentTask.GetTarget(TargetIndex.Tool);
                        //    var result = tool.Object != null && a.GetEquipmentSlot(GearType.Mainhand) == tool.Object;
                        //    return result;
                        //},
                        //FailCondition=(a)=>{
                        //    var tool = a.CurrentTask.GetTarget(TargetIndex.Tool);
                        //    return tool.Object != null;
                        //}

                        FailCondition=(a)=>{
                            var tool = a.CurrentTask.GetTarget(TargetIndex.Tool);
                            var equipped = a.GetEquipmentSlot(GearType.Mainhand);
                            var result = tool.Object != null && equipped != tool.Object;
                            //if(result)
                            //    a.NetNew.SyncReport($"{this} failed because tool stored in task [{tool}] is different from equipped tool [{equipped}]");
                            return result;
                        },
                        Label = "ToolAlreadyEquipped"

                    },
                    new BehaviorSequence(
                        new BehaviorSelector(
                            new BehaviorItemIsInInventory(TargetIndex.Tool),
                            new BehaviorGetAtNewNew(TargetIndex.Tool)),
                        new BehaviorInteractionNew(TargetIndex.Tool, new Equip())//,
                        //BehaviorReserve.Release(tool)
                        )
            };
        }
    }
}
