using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Modules.Construction
{
    class InteractionConstruct : Interaction
    {
        //BlockConstruction.ProductMaterialPair Construction;
        IConstructionProduct Construction;

        //public InteractionConstruct(BlockConstruction.ProductMaterialPair construction):base("Construct", 1)
        public InteractionConstruct(IConstructionProduct construction)
            : base("Construct", 1)
        {
            //this.Name = "Construct";
            //this.Seconds = 1;
            this.Construction = construction;
            this.Verb = "Constructing";
        }
        /// <summary>
        /// TODO: STATIC
        /// </summary>
        public override TaskConditions Conditions
        {
            get
            {
                return new TaskConditions(
                    new AllCheck(
                    //new SkillCheck(Components.Skills.Skill.Building),
                        new SkillCheck(this.Construction.GetSkill()),
                    //new ScriptTaskCondition("Materials", this.DetectMaterialsOld, Message.Types.InteractionFailed))
                        new ScriptTaskCondition("Materials", this.DetectMaterials, Message.Types.InteractionFailed))
                        );
            }
        }
        public override void Perform(GameObject a, TargetArgs t)
        {
            //if (!this.ConsumeMaterials(a, t))
            //if (!this.ConsumeMaterials(a))
            if (!a.GetComponent<PersonalInventoryComponent>().Take(this.Construction.GetReq()))
                return;
            //a.Net.SyncSetBlock(t.FinalGlobal, this.Construction.Product.Type, this.Construction.Data);
            this.Construction.SpawnProduct(a.Map, t.FinalGlobal);
        }

        public bool DetectMaterials(GameObject actor, TargetArgs t)
        {
            var reqs = this.Construction.GetReq();
            return actor.GetComponent<PersonalInventoryComponent>().Has(reqs);
            //var container = PersonalInventoryComponent.GetContents(actor);

            //foreach (var item in reqs)
            //{
            //    int amountFound = 0;
            //    foreach (var found in from slot in container where slot.HasValue where (int)slot.Object.ID == item.ObjectID select slot.Object)
            //        amountFound += found.StackSize;
            //    if (amountFound < item.Max)
            //        return false;
            //}
            //return true;
        }
        public bool DetectMaterialsOld(GameObject actor, TargetArgs t)
        {
            //var req = this.Construction.GetReq();
            //req.Amount = 0;
            //var nearbyMaterials = this.GetNearbyMaterials(actor);
            //foreach (var mat in nearbyMaterials)
            //{
            //    if (mat.GetInfo().ID == req.ObjectID)
            //        req.Amount += mat.StackSize;
            //}
            //return req.Amount >= req.Max;

            var reqs = this.Construction.GetReq();

            foreach (var req in reqs)
            {
                req.Amount = 0;
                var nearbyMaterials = this.GetNearbyMaterials(actor);
                foreach (var mat in nearbyMaterials)
                {
                    if (mat.GetInfo().ID == req.ObjectID)
                        req.Amount += mat.StackSize;
                }
                //return req.Amount >= req.Max;
                if (req.Amount < req.Max)
                    return false;
            }
            return true;
        }
        //bool ConsumeMaterials(GameObject parent)
        //{
        //    //var container = parent.GetComponent<PersonalInventoryComponent>().Slots.Slots;
        //    var container = PersonalInventoryComponent.GetContents(parent);

        //    var net = parent.Net;
        //    var reqs = this.Construction.GetReq();

        //    foreach (var item in reqs)
        //    {
        //        int amountRemaining = item.Max;
        //        foreach (var found in from slot in container where slot.HasValue where (int)slot.Object.ID == item.ObjectID select slot)
        //        {
        //            int amountToTake = Math.Min(found.Object.StackSize, amountRemaining);
        //            amountRemaining -= amountToTake;
        //            //found.Object.StackSize -= amountToTake;

        //            //if(found.Object.StackSize == 0)
        //            if (amountToTake == found.Object.StackSize)
        //            {
        //                net.Despawn(found.Object);
        //                net.DisposeObject(found.Object);
        //                found.Clear();
        //                //net.SyncDisposeObject(found);
        //            }
        //            else
        //                found.Object.StackSize -= amountToTake;
        //            if (amountRemaining == 0)
        //                break;
        //        }
        //    }
        //    return true;
        //}
        //bool ConsumeMaterials(GameObject actor, TargetArgs t)
        //{
        //    var reqs = this.Construction.GetReq();
        //    var nearbyMaterials = this.GetNearbyMaterials(actor);
        //    if (!this.DetectMaterialsOld(actor, t))
        //        return false;
        //    foreach (var req in reqs)
        //    {
        //        var remaining = req.Max;
        //        foreach (var mat in nearbyMaterials)
        //        {
        //            if (mat.GetInfo().ID == req.ObjectID)
        //            {
        //                // consume mat
        //                while (remaining > 0 && mat.StackSize > 0)
        //                {
        //                    remaining--;
        //                    var newStackSize = mat.StackSize - 1;
        //                    if (newStackSize == 0)
        //                    {
        //                        actor.Net.Despawn(mat);
        //                        actor.Net.DisposeObject(mat);
        //                        break;
        //                    }
        //                    else
        //                        mat.StackSize = newStackSize;
        //                }
        //                if (remaining == 0)
        //                    break;
        //            }
        //        }
        //    }
        //    return true;
        //}
        //bool ConsumeMaterials(GameObject actor, TargetArgs t)
        //{
        //    var req = this.Construction.GetReq();
        //    var nearbyMaterials = this.GetNearbyMaterials(actor);
        //    if (!this.DetectMaterials(actor, t))
        //        return false;
        //    var remaining = req.Max;
        //    foreach (var mat in nearbyMaterials)
        //    {
        //        if (mat.GetInfo().ID == req.ObjectID)
        //        {
        //            // consume mat
        //            while (remaining > 0 && mat.StackSize > 0)
        //            {
        //                remaining--;
        //                var newStackSize = mat.StackSize - 1;
        //                if (newStackSize == 0)
        //                {
        //                    actor.Net.Despawn(mat);
        //                    actor.Net.DisposeObject(mat);
        //                    break;
        //                }
        //                else
        //                    mat.StackSize = newStackSize;
        //            }
        //            if (remaining == 0)
        //                break;
        //        }
        //    }
        //    return true;
        //}
        List<GameObject> GetNearbyMaterials(GameObject actor)
        {
            return actor.GetNearbyObjects(r => r <= 2, o => o.Type == ObjectType.Material);
        }

        public override object Clone()
        {
            return new InteractionConstruct(this.Construction);
        }
    }
}
