using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    class PlaceBlock : Interaction
    {
        GameObject HeldBlock;

        public PlaceBlock()
            : base(
                "Place",
                2
                )
        {

        }
        static readonly TaskConditions conds = new TaskConditions(
                    new AllCheck(
                        new TargetTypeCheck(TargetType.Position),
                        new RangeCheck(t => t.Global, max: InteractionOld.DefaultRange))
                );
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }
        public override void Start(GameObject a, TargetArgs t)
        {
            //this.HeldBlock = a.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling].Object;
            this.HeldBlock = a.GetComponent<HaulComponent>().GetObject();//.Slot.Object;

        }
        public override void Perform(GameObject a, TargetArgs t)
        {
            var blockComp = HeldBlock.GetComponent<BlockComponent>();
            //blockComp.Place(this.BlockEntity, a, t.Global + t.Face);

            // consume held item
            //var currentHeldItem = a.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling].Object;
            var currentHeldItem = a.GetComponent<HaulComponent>().GetObject();//.Slot.Object;

            if (currentHeldItem != this.HeldBlock)
                return; // held item changed, cancel action

            // check wether to consume item based on something affecting the character or a cheat mode for example
            //a.GetComponent<HaulComponent>().Slot.StackSize--;
            currentHeldItem.StackSize--;

            var orientation = 0;
            a.Net.SyncSetBlock(t.Global + t.Face, blockComp.Block.Type, blockComp.Data, orientation);
        }
        void Place(GameObject a, TargetArgs t)
        {
            var blockComp = HeldBlock.GetComponent<BlockComponent>();
            blockComp.Place(this.HeldBlock, a, t.Global);
            //a.Net.SetBlock(t.Global, blockComp.Type, blockComp.Data);
        }
        //    void Place(GameObject parent, GameObject actor, Vector3 location)
        //{
        //    actor.Net.SetBlock(location, this.Block.Type, this.Data);
        //}

        public override object Clone()
        {
            return new PlaceBlock();
        }
    }
}
