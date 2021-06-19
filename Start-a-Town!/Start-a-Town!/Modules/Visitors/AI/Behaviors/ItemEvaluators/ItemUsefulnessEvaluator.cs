using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI
{
    static public class ItemUsefulnessEvaluator
    {
        //Func<Actor, Entity, int> Evaluator;

        //public ItemUsefulnessEvaluator(Func<Actor, Entity, int> evaluator)
        //{
        //    this.Evaluator = evaluator;
        //}

        static public int EvaluateTool(Actor actor, Entity item)
        {
            var tool = item.Def.ToolProperties;
            if (tool == null)
                return 0;
            var ability = tool.Ability;
            var similarOwnedItem = actor.Inventory.FindItems((Entity item) => item.Def.ToolProperties?.Ability.Def == ability.Def).FirstOrDefault();
            var benefit = ability.Efficiency - (similarOwnedItem?.Def.ToolProperties.Ability.Efficiency ?? 0); // TODO get total effeciency (multiplied by item level/quality?
            return benefit;
        }

        static public int EvaluateApparel(Actor actor, Entity item)
        {
            var apparel = item.Def.ApparelProperties;
            if (apparel == null)
                return 0;
            int benefit = 0;
            //var gear = actor.GetEquipmentSlot(apparel.GearType);
            var similarOwnedItem = actor.Inventory.FindItems((Entity item) => item.Def.ApparelProperties?.GearType == apparel.GearType).FirstOrDefault();
            benefit = apparel.ArmorValue - (similarOwnedItem?.Def.ApparelProperties.ArmorValue ?? 0);
            return benefit;
        }
        static Func<Actor, Entity, int>[] Evaluators = { EvaluateTool, EvaluateApparel };
        static public int Evaluate(Actor actor, Entity Item)
        {
            var score = Evaluators.Sum(e => e(actor, Item));
            return score;
        }
    }
}
