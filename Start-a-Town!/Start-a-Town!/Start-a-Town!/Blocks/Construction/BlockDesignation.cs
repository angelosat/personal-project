using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Graphics;
using Start_a_Town_.Blocks;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Blocks
{
    partial class BlockDesignation : Block
    {
        public BlockDesignation()
            : base(Types.Construction, 1, 0, false, false)
        {
            this.AssetNames = "highlightfull";
        }
        public override Material GetMaterial(byte blockdata)
        {
            return Material.LightWood;
        }
        public override MyVertex[] Draw(MySpriteBatch opaquemesh, MySpriteBatch nonopaquemesh, MySpriteBatch transparentMesh, Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            var entity = chunk.Map.GetBlockEntity(blockCoordinates) as Entity;
            var block = entity.Product.Product;
            return transparentMesh.DrawBlock(Block.Atlas.Texture, screenBounds, block.Variations.First(), camera.Zoom, fog, Color.White, sunlight, blocklight, depth, blockCoordinates);
        }
        public override BlockEntity GetBlockEntity()
        {
            return new Entity();
        }
        public static void Place(GameModes.IMap map, Vector3 global, byte data, int variation, int orientation, BlockConstruction.ProductMaterialPair product)
        {
            map.SetBlock(global, Types.Construction, data, variation);
            map.GetCell(global).Orientation = orientation; // TODO: keep or remove orientation field for cells afterall???
            var entity = new Entity(product);
            map.AddBlockEntity(global, entity);
            map.GetTown().ConstructionsManager.HandleBlock(map, global);
        }
        public override void Remove(IMap map, Vector3 global)
        {
            var entity = map.GetBlockEntity(global) as Entity;
            foreach (var mat in entity.Materials)
            {
                if (mat.Amount == 0)
                    continue;
                //for (int i = 0; i < mat.Amount; i++)
                //{
                    var matEntity = GameObject.Create(mat.ObjectID);
                    matEntity.SetStack(mat.Amount);
                    map.GetNetwork().PopLoot(matEntity, global, Vector3.Zero);
                //}
            }

            base.Remove(map, global);
            map.GetTown().ConstructionsManager.HandleBlock(map, global);

        }
        public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, Interaction> list)
        {
            //base.GetPlayerActionsWorld(map, global, list);
            var haul = PersonalInventoryComponent.GetHauling(Player.Actor);
            var haulObj = haul.Object;
            if (haulObj != null)
            {
                var entity = player.Map.GetBlockEntity(global) as Entity;
                //if (entity.Material.Filter(haulObj))
                //{
                //    list.Add(PlayerInput.RButton, new DropCarriedSnap());// new InteractionAddMaterial());
                //    return;
                //}
                ItemRequirement mat;
                mat = entity.Materials.FirstOrDefault(f => f.ObjectID == (int)(haulObj.ID));
                if (mat == null)
                    return;
                if (mat.Remaining == 0)
                    return;

                list.Add(PlayerInput.RButton, new DropCarriedSnap());// new InteractionAddMaterial());
                return;
                //foreach (var mat in entity.Materials)
                //    if ((int)input.Object.ID == mat.ObjectID)
                //        if (mat.Remaining > 0)
                //            return true;
            }
            if (this.MaterialsPresent(player.Map, global))
                list.Add(PlayerInput.RButton, new InteractionBuild());
        }
        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            var entity = map.GetBlockEntity(global) as Entity;
            return new List<Interaction>(){
                new InteractionBuild(),
                new InteractionAddMaterial(entity)
            };
        }
        public override void OnDrop(GameObject actor, GameObject dropped, TargetArgs target)
        {
            var entity = actor.Map.GetBlockEntity(target.Global) as Entity;
            if (entity.Material.Filter(dropped))
            {
                ItemRequirement mat;
                mat = entity.Materials.First(foo=>foo.ObjectID == (int)dropped.ID);
                //foreach (var mat in entity.Materials)
                    //if ((int)dropped.ID == mat.ObjectID)
                        if (mat.Remaining > 0)
                        {
                            var obj = dropped;
                            int toTake = Math.Min(mat.Remaining, dropped.StackSize);
                            mat.Amount += toTake;
                            //actor.Net.EventOccured(Message.Types.InventoryChanged, t.Object);
                            //if(dropped.StackSize == toTake)
                            //    actor.Net.DisposeObject(obj);
                            //else
                                dropped.StackSize -= toTake;
                        }
                return;
            }
            base.OnDrop(actor, dropped, target);
        }

        public bool MaterialsPresent(IMap map, Vector3 global)
        {
            var entity = map.GetBlockEntity(global) as Entity;
            return entity.MaterialsPresent();
            //foreach (var mat in entity.Materials)
            //    if (mat.Remaining > 0)
            //        return false;
            //return true;
        }

        public class InteractionBuild : Interaction
        {
            public InteractionBuild()
                : base(
                "Build",
                4)
            {
                this.Animation = new Graphics.Animations.AnimationTool();
            }
            static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                    RangeCheck.One
                    ,
                    new MaterialsPresent(),
                    new SkillCheck(Components.Skills.Skill.Building)
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
                var entity = a.Map.GetBlockEntity(t.Global) as Entity;
                var block = entity.Product.Product;
                a.Map.SetBlock(t.Global, block.Type);
            }

            class MaterialsPresent : ScriptTaskCondition
            {
                public override bool Condition(GameObject actor, TargetArgs target)
                {
                    var global = target.Global;
                    var block = actor.Map.GetBlock(global) as BlockDesignation;
                    if (block == null)
                        throw new Exception();
                    var entity = actor.Map.GetBlockEntity(global) as Entity;
                    if (entity == null)
                        throw new Exception();
                    return entity.MaterialsPresent();
                }
                public override bool AITrySolve(GameObject agent, TargetArgs target, Components.AI.AIState state, List<AI.AIInstruction> instructions)
                {
                    //throw new NotImplementedException();
                    var global = target.Global;
                    var block = agent.Map.GetBlock(global) as BlockDesignation;
                    if (block == null)
                        throw new Exception();
                    var entity = agent.Map.GetBlockEntity(global) as Entity;
                    if (entity == null)
                        throw new Exception();

                    //var nearbyItems = state.NearbyEntities;
                    //var item = (from i in nearbyItems
                    //            let mat = entity.Materials.FirstOrDefault(f => f.ObjectID == (int)i.ID)
                    //            where mat != null
                    //            where mat.Remaining > 0
                    //            select i).FirstOrDefault();
                    //if (item == null)
                    //{
                    //    instruction = null;
                    //    return false;
                    //}
                    var instruction = new AI.AIInstruction(target, new InteractionAddMaterial(entity));
                    instructions.Add(instruction);
                    return false;
                }
                //public override bool AITrySolve(GameObject agent, TargetArgs target, Components.AI.AIState state, out AI.AIInstruction instruction)
                //{
                //    //throw new NotImplementedException();
                //    var global = target.Global;
                //    var block = agent.Map.GetBlock(global) as BlockDesignation;
                //    if (block == null)
                //        throw new Exception();
                //    var entity = agent.Map.GetBlockEntity(global) as Entity;
                //    if (entity == null)
                //        throw new Exception();

                //    //var nearbyItems = state.NearbyEntities;
                //    //var item = (from i in nearbyItems
                //    //            let mat = entity.Materials.FirstOrDefault(f => f.ObjectID == (int)i.ID)
                //    //            where mat != null
                //    //            where mat.Remaining > 0
                //    //            select i).FirstOrDefault();
                //    //if (item == null)
                //    //{
                //    //    instruction = null;
                //    //    return false;
                //    //}
                //    instruction = new AI.AIInstruction(target, new InteractionAddMaterial(entity));
                //    return false;
                //}
            }
            public override object Clone()
            {
                return new InteractionBuild();
            }

            

       
        }
        class InteractionAddMaterial : Interaction
        {
            Entity Entity;
            public InteractionAddMaterial(Entity entity)
                : base(
                "Add Material",
                .4f)
            {
                //this.Name = "Add Material";
                //this.Length = 1;
                this.Animation = new Graphics.Animations.AnimationPlaceItem();
                this.Entity = entity;
            }

            /// <summary>
            /// TODO: find a way to have static conditions for this one. maybe pass the interaction to the conditions as an argument so i can check local variables?
            /// </summary>
            public override TaskConditions Conditions
            {
                get
                {
                    return new TaskConditions(new AllCheck(
                    new RangeCheck(t => t.Global, InteractionOld.DefaultRange)
                    ,
                        new IsHauling(this.Entity.MaterialValid)
                    ));
                }
            }
            public override void Start(GameObject a, TargetArgs t)
            {
                base.Start(a, t);
                var haul = a.GetComponent<HaulComponent>();
                a.Body.FadeOutAnimation(haul.AnimationHaul, this.Seconds / 2f);// 1f);
                a.Body.Start(this.Animation);
            }
            //bool IsMaterialValid(GameObject a, TargetArgs t)
            //{
            //    var entity = a.Map.GetBlockEntity(t.Global) as Entity;
            //    var haulObj = PersonalInventoryComponent.GetHauling(a).Object;
            //    return entity.Material.Filter(haulObj);
            //}
            public override void Perform(GameObject actor, TargetArgs target)
            {
                var block = actor.Map.GetBlock(target.Global);
                var hauled = PersonalInventoryComponent.GetHauling(actor);
                var hauledObj = hauled.Object;
                hauled.Clear();
                actor.Map.GetBlock(target.Global).OnDrop(actor, hauledObj, target);
            }
            public override object Clone()
            {
                return new InteractionAddMaterial(this.Entity);
            }
        }
    }
}
