using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.GameModes;
using Start_a_Town_.Towns.Forestry;
using Start_a_Town_.Particles;

namespace Start_a_Town_
{
    class BlockSoil : Block
    {
        public class Placer : BlockPlacer
        {
            protected override Block Block => BlockDefOf.Soil;
        }

        public override bool IsMinable => true;
        public override Color DirtColor
        {
            get
            {
                return Color.SaddleBrown;
            }
        }
        public override ParticleEmitterSphere GetEmitter()
        {
            return base.GetDirtEmitter();
        }

        public BlockSoil()
            : base(Block.Types.Soil, GameObject.Types.Soil)
        {
            this.RequiresConstruction = false;
            //this.Material = Material.Soil;
            //this.MaterialType = MaterialType.Soil;
            this.AssetNames = "soil/soil1, soil/soil2, soil/soil3, soil/soil4";
            //this.LootTable = new LootTable(
            //    //new Loot(GameObject.Types.Soilbag, chance: 1f, count: 1),
            //    new Loot(GameObject.Types.Cobblestones, chance: 0.25f, count: 1),
            //    new Loot(GameObject.Types.Twig, chance: 0.25f, count: 1)
            //    );
            this.Reagents.Add(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterialType(MaterialType.Soil), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks)));
            //this.Reagent = new ItemDefAmount(RawMaterialDef.Bags, 4);
            this.Ingredient = new Ingredient(RawMaterialDef.Bags, MaterialDefOf.Soil, null, 1);

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterialType(MaterialType.Soil), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                new BlockRecipe.Product(this)
                );
            Towns.Constructions.ConstructionsManager.Walls.Add(this.Recipe);

        }
        public override BlockRecipe GetRecipe()
        {
            return this.Recipe;
        }
       
        public override void RandomBlockUpdate(IObjectProvider net, IntVec3 global, Cell celll)//GameObject parent)
        {
            if (net.Map.GetBlock(global + IntVec3.UnitZ) != BlockDefOf.Air)
                return;
            if (net.Map.GetSunLight(global + IntVec3.UnitZ) < 8)
                return;

            // make grass grow anywhere, not just spread from existing grass
            BlockDefOf.Grass.Place(net.Map, global, 0, celll.Variation, 0);

            foreach (var n in global.GetNeighborsDiag())// Position.GetNeighbors(global))
            {
                if (!net.Map.TryGetCell(n, out Cell cell))
                    continue;
                if (cell.Block.Type != Block.Types.Grass)
                    continue;
                //this.Remove(net.Map, global);
                BlockDefOf.Grass.Place(net.Map, global, 0, celll.Variation, 0);
                return;
                //int varMax = Block.Grass.Variations.Count;
                //int rand;
                //if (!net.TryGetRandomValue(0, varMax, out rand))
                //    return;
                //byte[] data = Net.Network.Serialize(w =>
                //{
                //    new TargetArgs(global).Write(w);
                //    w.Write((int)Message.Types.SetBlockVariation);
                //    w.Write((byte)rand);
                //});
                //(net as Server).RemoteProcedureCall(data, global);
                //return;
            }
        }
        internal override void RemoteProcedureCall(IObjectProvider net, Vector3 vector3, Message.Types type, System.IO.BinaryReader r)
        {
            switch(type)
            {
                // TODO: move this to base Block class
                case Message.Types.SetBlockVariation:
                    var variation = r.ReadByte();
                    net.Map.GetCell(vector3).Variation = variation;
                    break;

                default:
                    break;
            }
        }

        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Soil;
        }
        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            return new List<Interaction>(){
                    new InteractionDigging(),
                new InteractionTilling(),
                new InteractionPlantTree()
            };
            //return new List<ScriptTask>(){
            //    new ScriptTask(
            //        "Digging",
            //        10,
            //        (a,t) =>this.Break(a.Net, t.Global),
            //        new TaskConditions(
            //            new RangeCheck(t=>t.Global, Interaction.DefaultRange),
            //            new SkillCheck(Skill.Digging)),
            //        Skill.Digging
            //        )
            //};
        }

        public override ContextAction GetRightClickAction(Vector3 global)
        {
            return new ContextAction(() => "Dig", () => { Net.Client.PlayerInteract(new TargetArgs(global)); });
        }
        public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, Interaction> list)
        {
            //base.GetPlayerActionsWorld(player, global, list);

            var mainhand = GearComponent.GetSlot(PlayerOld.Actor, GearType.Mainhand);
            if (mainhand.Object != null)
            {
                var skill = mainhand.Object.GetComponent<ToolAbilityComponent>();
                if (skill != null)
                {
                    if (ToolAbilityComponent.HasSkill(mainhand.Object, ToolAbilityDef.Digging))
                        list.Add(PlayerInput.RButton, new InteractionDigging());
                    else if (ToolAbilityComponent.HasSkill(mainhand.Object, ToolAbilityDef.Argiculture))
                        list.Add(PlayerInput.RButton, new InteractionTilling());
                }
            }
        }
        //public override List<ScriptTask> GetAvailableTasks(IObjectProvider net, Vector3 global)
        //{
        //    return new List<ScriptTask>(){
        //        new ScriptTask(
        //            "Digging",
        //            2,
        //            (a,t) =>this.Break(net, t.Global),
        //            new TaskConditionCollection(
        //                new RangeCondition(t=>t.Global, Interaction.DefaultRange),
        //                new SkillCheck(Skill.Digging)),
        //            Skill.Digging
        //            )
        //    };
        //}

        //static public readonly BlockConstruction Recipe = new BlockConstruction(
        //    Reaction.Reagent.Create(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterialType(MaterialType.Soil), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
        //    new BlockConstruction.Product(Block.Soil));

        protected override GameObject ToObject()
        {
            return this.Create(MaterialDefOf.Soil);
        }
        GameObject Create(Material mat)
        {
            GameObject obj = new GameObject();
            obj.AddComponent<DefComponent>().Initialize(Block.EntityIDRange + (int)this.Type, ObjectType.Block, this.GetName());
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
            //obj.AddComponent<BlockComponent>().Initialize(this);
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite(this.Variations.First().Name, Map.BlockDepthMap)
            {
                OriginGround = Block.OriginCenter,
                Joint = Block.Joint,
                MouseMap = BlockMouseMap,
                Overlays = new Dictionary<string, Sprite>() { { "Body", new Sprite(this.Variations.First().Name, Map.BlockDepthMap) { Tint = mat.Color } } }
            });
            obj.AddComponent<GuiComponent>().Initialize(new UI.Icon(obj.GetSprite()));
            obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", mat));
            return obj;
        }
    }
}
