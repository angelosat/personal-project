using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Crafting.Blocks;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using Start_a_Town_.Tokens;
using Start_a_Town_.Crafting;
using Start_a_Town_.AI;

namespace Start_a_Town_.Blocks.Smeltery
{
    partial class BlockSmeltery : BlockWorkstation// Block, IBlockWorkstation
    {
        public override AILabor Labor { get { return AILabor.Smelter; } }

        public override Material GetMaterial(byte blockdata)
        {
            return Material.Stone;
        }
        public BlockSmeltery()
            : base(Types.Smeltery)
        {
            this.AssetNames = "smoothstone";
            //this.Material = Material.Stone;
            //this.MaterialType = MaterialType.Mineral;
            this.Recipe = new BlockConstruction(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfSubType(ItemSubType.Rock),
                        Reaction.Reagent.IsOfMaterial(Material.Stone),
                        Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                    new BlockConstruction.Product(this),
                    Components.Skills.Skill.Building);
            this.Tokens.Add(new IsWorkstation(IsWorkstation.Types.Smeltery));
        }

        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            var list = new List<Interaction>();
            list.Add(new InteractionAddMaterial());
            //list.Add(new BlockSmeltery.InteractionCraft());
            list.Add(new InteractionCraft());
            return list;
        }

        //public override BlockEntity GetBlockEntity()
        public override BlockEntityWorkstation GetWorkstationBlockEntity()
        {
            return new Entity(4, 4, 4);
        }

        internal override void RemoteProcedureCall(Net.IObjectProvider net, Vector3 vector3, Components.Message.Types type, System.IO.BinaryReader r)
        {
            var entity = net.Map.GetBlockEntity(vector3) as Entity;
            if (entity == null)
                throw new Exception();
            int dataLength = (int)(r.BaseStream.Length - r.BaseStream.Position); // TODO
            byte[] args = r.ReadBytes(dataLength);
            entity.HandleRemoteCall(net.Map, vector3, ObjectEventArgs.Create(type, args));
        }
        
        private static void ShowUI(Vector3 global)
        {
            var entity = Net.Client.Instance.Map.GetBlockEntity(global) as Entity;
            var window = new WindowEntityInterface(entity, "Smeltery", () => global);
            var ui = new WorkstationUI(global, entity);
            window.Client.Controls.Add(ui);
            window.Show();
        }
        
        public class InteractionActivate : Interaction
        {
            public InteractionActivate()
            {
                this.Name = "Use";
            }
            public override void Perform(GameObject a, TargetArgs t)
            {
                if (a.Net is Net.Client)
                {
                    ShowUI(t.Global);
                }
            }
            public override object Clone()
            {
                return new InteractionActivate();
            }
        }

        public class InteractionCraftOld : Interaction
        {
            public InteractionCraftOld()
                : base("Produce", 4)
            {
                
            }
            static readonly TaskConditions conds =
                    new TaskConditions(
                        new AllCheck(
                            RangeCheck.One,
                            new MaterialsPresent()
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
                var block = a.Map.GetBlock(t.Global) as BlockSmeltery;
                block.Produce(a, t.Global);
            }
            public override object Clone()
            {
                return new BlockSmeltery.InteractionCraftOld();
            }
        }
    }
}
