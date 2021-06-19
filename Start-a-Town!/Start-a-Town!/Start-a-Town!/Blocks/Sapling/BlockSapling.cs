using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Graphics.Animations;
using Start_a_Town_.Components.Items;

namespace Start_a_Town_.Blocks.Sapling
{
    class BlockSapling : Block
    {
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Twig;
        }
        public BlockSapling():base(Block.Types.Sapling, opaque: false, solid: false)
        {
            this.AssetNames = "sapling";
            //this.Material = Material.Twig;

        }
        public override void Break(Start_a_Town_.GameObject actor, Microsoft.Xna.Framework.Vector3 global)
        {
            base.Break(actor, global);
            var sapling = ItemTemplate.Sapling.Factory.Create();
            actor.Net.PopLoot(sapling, global, Vector3.Zero);
        }
        //public override void Remove(GameModes.IMap map, Vector3 global)
        //{
        //    base.Remove(map, global);
        //    var sapling = ItemTemplate.Sapling.Factory.Create();
        //    map.Net.PopLoot(sapling, global, Vector3.Zero);
        //}
        public override void RandomBlockUpdate(Net.IObjectProvider net, Vector3 global, Cell cell)
        {
            net.Map.GetBlock(global).Remove(net.Map, global);
            net.Spawn(GameObject.Create(GameObject.Types.Tree), global);
        }

        public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, Interaction> list)
        {
            list.Add(PlayerInput.RButton, new InteractionRemove());
        }
        public override List<Interaction> GetAvailableTasks(GameModes.IMap map, Vector3 global)
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
                //a.Map.RemoveBlock(t.Global);
                a.Map.GetBlock(t.Global).Break(a, t.Global);
                //var sapling = ItemTemplate.Sapling.Factory.Create();
                //a.Net.PopLoot(sapling, t.Global, Vector3.Zero);
                this.State = States.Finished;
            }
            public override object Clone()
            {
                return new InteractionRemove();
            }
        }
        public class InteractionRemoveOld : Interaction
        {
            public InteractionRemoveOld()
                :base("Remove", .4f)
            {
                this.RunningType = RunningTypes.Continuous;
                this.Animation = new AnimationTool(() => this.Done = true);
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
            public override void Perform(GameObject a, TargetArgs t)
            {
                if (!Done)
                    return;
                //a.Map.RemoveBlock(t.Global);
                a.Map.GetBlock(t.Global).Break(a, t.Global);
                //var sapling = ItemTemplate.Sapling.Factory.Create();
                //a.Net.PopLoot(sapling, t.Global, Vector3.Zero);
                this.State = States.Finished;
            }
            bool Done;
            public override object Clone()
            {
                return new InteractionRemoveOld();
            }
        }
    }
}
