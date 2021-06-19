using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Modules.Crafting;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    public class ItemToolDef
    {
        public ToolAbility Ability;
        public readonly HashSet<JobDef> AssociatedJobs = new();
        //public ItemToolDef(string name, ToolAbility ability) : base(name)
        //{
        //    //this.StackCapacity = 1;
        //    //this.StorageCategory = StorageCategory.Tools;
        //    this.Ability = ability;
        //    //this.ObjType = ObjectType.Equipment;
        //    this.CraftingIngredientIndices = new Dictionary<BoneDef, ReactionIngredientIndex>(){
        //        {BoneDef.EquipmentHandle, new ReactionIngredientIndex(HandleIngredientIndex) } ,
        //        {BoneDef.EquipmentHead, new ReactionIngredientIndex(HeadIngredientIndex) } };
        //}
        public ItemToolDef(ToolAbility ability)
        {
            this.Ability = ability;
            //this.CraftingIngredientIndices = new Dictionary<BoneDef, ReactionIngredientIndex>(){
            //    {BoneDef.EquipmentHandle, new ReactionIngredientIndex(HandleIngredientIndex) } ,
            //    {BoneDef.EquipmentHead, new ReactionIngredientIndex(HeadIngredientIndex) } };
        }
        public ItemToolDef AssociateJob(params JobDef[] jobs)
        {
            foreach (var j in jobs)
                this.AssociatedJobs.Add(j);
            return this;
        }
        //public Func<Dictionary<string, GameObject>, GameObject> Factory;
        
    }
}
