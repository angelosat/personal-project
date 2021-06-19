using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Vegetation;
using Start_a_Town_.Graphics.Animations;

namespace Start_a_Town_.Towns.Forestry
{
    class InteractionPlantTree : Interaction
    {
        public InteractionPlantTree()
            : base("PlantTree", 2)
        {
        }

        static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                RangeCheck.One,
                new BlockCheck(b => b == Block.Soil || b == Block.Grass),
                new IsHauling(foo => { if (foo != null)return foo.HasComponent<SaplingComponent>(); else return false; })
                ));
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }

        public override void Perform(GameObject a, TargetArgs t)
        {
            var slot = PersonalInventoryComponent.GetHauling(a);
            //if (slot.Object != this.Parent)
            //    return;
            slot.Consume();
            //slot.Clear();
            //Block.Sapling.Place(a.Map, t.FaceGlobal, 0, 0, 0);
            Block.Sapling.Place(a.Map, t.Global + Vector3.UnitZ, 0, 0, 0);

        }

        //bool SeedValid(GameObject hauled)
        //{
        //    throw new Exception();
        //    if (hauled == null)
        //        return false;
        //    //var parent = seed.Parent;
        //    return true;// this.SeedType == hauled.ID;
        //}
        public override object Clone()
        {
            return new InteractionPlantTree();
        }
    }
}
