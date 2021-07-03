using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Blocks.Sapling
{
    class BlockSapling : Block
    {
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.ShrubStem;
        }
        public BlockSapling():base(Block.Types.Sapling, opaque: false, solid: false)
        {
            this.AssetNames = "sapling";
        }
        public override void Break(GameObject actor, Vector3 global)
        {
            base.Break(actor, global);
            //var sapling = ItemTemplate.Sapling.Factory.Create();
            //actor.Net.PopLoot(sapling, global, Vector3.Zero);
        }
        
        public override void RandomBlockUpdate(IObjectProvider net, IntVec3 global, Cell cell)
        {
            net.Map.GetBlock(global).Remove(net.Map, global);
            net.Spawn(GameObject.Create(GameObject.Types.Tree), global);
        }

        public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, Interaction> list)
        {
            list.Add(PlayerInput.RButton, new InteractionRemove());
        }
        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            List<Interaction> list = new List<Interaction>(){
                new InteractionRemove()
            };
            return list;
        }
        public class InteractionRemove : InteractionPerpetual
        {
            public InteractionRemove()
                : base("Remove")
            {
            }
            static readonly TaskConditions conds = new TaskConditions(
                    new AllCheck(
                        RangeCheck.Sqrt2
                        ));
            public override TaskConditions Conditions
            {
                get
                {
                    return conds;
                }
            }

            public override void OnUpdate(GameObject a, TargetArgs t)
            {
                a.Map.GetBlock(t.Global).Break(a, t.Global);
                this.Finish(a, t);
            }
            public override object Clone()
            {
                return new InteractionRemove();
            }
        }
        
    }
}
