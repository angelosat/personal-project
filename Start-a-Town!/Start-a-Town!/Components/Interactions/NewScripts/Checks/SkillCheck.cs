using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.AI;
using Start_a_Town_.UI;
using Start_a_Town_.AI;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    /// <summary>
    /// Checks if the item held in actor's mainhand enables a particular skill.
    /// </summary>
    public class SkillCheck : ScriptTaskCondition
    {
        ToolAbilityDef Skill {get;set;}
        public SkillCheck(ToolAbilityDef skill)
            : base("Skill")
        {
            this.Skill = skill;
            this.ErrorEvent = Message.Types.WrongTool;
        }
        GameObjectSlot CachedSlot;
        //GameObject CachedObject;
        public override bool Condition(GameObject actor, TargetArgs target)
        {
            if (this.Skill == null)
                return true;
            //if (CachedSlot.IsNull())
            //{
                CachedSlot = GearComponent.GetSlot(actor, GearType.Mainhand);
            //}
            var tool = CachedSlot.Object;
            //if (tool != CachedObject)
            //    return false;
            if (tool == null)
                return false;
            return ToolAbilityComponent.HasSkill(tool, this.Skill);
        }
        public override void GetTooltip(UI.Control tooltip)
        {
            if (this.Skill == null)
                return;
            tooltip.Controls.Add(new Label("Requires: " + this.Skill.Name) { Location = tooltip.Controls.BottomLeft });
        }

        //public override AIInstruction AIGetPreviousStep(GameObject agent, TargetArgs target, AIState state)
        //{
        //    var nearbyItems = state.NearbyEntities;
        //    var item = (from i in nearbyItems
        //                where SkillComponent.HasSkill(i, this.Skill)
        //                select i).FirstOrDefault();
        //    if (item == null)
        //        return null;
        //    return new AIInstruction(new TargetArgs(item), new PickUp());
        //}

        public override void AIInit(GameObject agent, TargetArgs target, AIState state)
        {
            if (this.Condition(agent, target))
                return;

            var nearbyItems = state.NearbyEntities;
            var item = (from i in nearbyItems
                        where ToolAbilityComponent.HasSkill(i, this.Skill)
                        select i).FirstOrDefault();
            if (item == null)
                return;

            var instruction = new AIInstruction(new TargetArgs(item), new Equip());
            //state.GetJob().Instructions.Enqueue(instruction);
            state.GetJobOld().AddStep(instruction);
        }
        public override bool AITrySolve(GameObject agent, TargetArgs target, AIState state, List<AIInstruction> instructions)
        {
            var nearbyItems = state.NearbyEntities;
            var item = (from i in nearbyItems
                        where !AIState.IsItemReserved(i)
                        where ToolAbilityComponent.HasSkill(i, this.Skill)
                        select i).FirstOrDefault();
            if (item == null)
            {
                //instruction = null;
                return false;
            }
            var instruction = new AIInstruction(new TargetArgs(item), new Equip());
            AIState.ReserveItem(item, agent);
            instructions.Add(instruction);
            return false;
        }
        //public override bool AITrySolve(GameObject agent, TargetArgs target, AIState state, out AIInstruction instruction)
        //{
        //    var nearbyItems = state.NearbyEntities;
        //    var item = (from i in nearbyItems
        //                where !AIState.IsItemReserved(i)
        //                where SkillComponent.HasSkill(i, this.Skill)
        //                select i).FirstOrDefault();
        //    if (item == null)
        //    {
        //        instruction = null;
        //        return false;
        //    }
        //    instruction = new AIInstruction(new TargetArgs(item), new Equip());
        //    return false;
        //}
    }
}
