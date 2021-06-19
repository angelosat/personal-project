using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.GameModes;
using Start_a_Town_.Towns.Forestry;

namespace Start_a_Town_
{
    class BlockSoil : Block
    {
        public override Color DirtColor
        {
            get
            {
                return Color.SaddleBrown;
            }
        }
        public override Components.Particles.ParticleEmitterSphere GetEmitter()
        {
            return base.GetDirtEmitter();
        }

        public BlockSoil()
            : base(Block.Types.Soil, GameObject.Types.Soil)
        {
            //this.Material = Material.Soil;
            //this.MaterialType = MaterialType.Soil;
            this.AssetNames = "soil/soil1, soil/soil2, soil/soil3, soil/soil4";
            this.LootTable = new LootTable(
                //new Loot(GameObject.Types.Soilbag, chance: 1f, count: 1),
                new Loot(GameObject.Types.Cobble, chance: 0.25f, count: 1),
                new Loot(GameObject.Types.Twig, chance: 0.25f, count: 1)
                );

            this.Recipe = new BlockConstruction(
                Reaction.Reagent.Create(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterialType(MaterialType.Soil), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                new BlockConstruction.Product(this)
                );
        }

        public override void RandomBlockUpdate(Net.IObjectProvider net, Vector3 global, Cell celll)//GameObject parent)
        {
            if (net.Map.GetBlock(global + Vector3.UnitZ) != Block.Air)
                return;
            if (net.Map.GetSunLight(global + Vector3.UnitZ) < 8)
                return;
            foreach (var n in global.GetNeighborsDiag())// Position.GetNeighbors(global))
            {
                Cell cell;
                //if (!n.TryGetCell(net.Map, out cell))
                if (!net.Map.TryGetCell(n, out cell))
                    continue;
                if (cell.Block.Type != Block.Types.Grass)
                    continue;
                //if ((global + Vector3.UnitZ).GetSunLight(net.Map) < 8)
               
               // net.SyncSetBlock(global, Block.Types.Grass); // TOTO: deprecate syncsetblock
                this.Remove(net.Map, global);
                Block.Grass.Place(net.Map, global, 0, celll.Variation, 0);

                //int varMax = Block.TileSprites[Block.Types.Grass].SourceRects.GetUpperBound(0) + 1;
                int varMax = Block.Grass.Variations.Count;
                //net.TryGetRandomValue(0, varMax, r => global.GetCell(net.Map).Variation = (byte)r);
                // TODO: WHAT IS THIS???
                //net.TryGetRandomValue(0, varMax, r => net.Map.GetCell(global).Variation = (byte)r);
                int rand;
                if (!net.TryGetRandomValue(0, varMax, out rand))
                    return;
                byte[] data = Net.Network.Serialize(w =>
                {
                    new TargetArgs(global).Write(w);
                    w.Write((int)Message.Types.SetBlockVariation);
                    w.Write((byte)rand);
                });
                (net as Net.Server).RemoteProcedureCall(data, global);
                return;
            }
        }
        internal override void RemoteProcedureCall(Net.IObjectProvider net, Vector3 vector3, Message.Types type, System.IO.BinaryReader r)
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
            return Material.Soil;
        }
        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            return new List<Interaction>(){
                    new InteractionDigging(),
                new Tilling(),
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
            //base.GetPlayerActionsWorld(map, global, list);
            base.GetPlayerActionsWorld(player, global, list);

            var mainhand = GearComponent.GetSlot(Player.Actor, GearType.Mainhand);
            if (mainhand.Object != null)
            {
                var skill = mainhand.Object.GetComponent<SkillComponent>();
                if (skill != null)
                {
                    if (SkillComponent.HasSkill(mainhand.Object, Skill.Digging))
                        list.Add(PlayerInput.RButton, new InteractionDigging());
                    else if (SkillComponent.HasSkill(mainhand.Object, Skill.Argiculture))
                        list.Add(PlayerInput.RButton, new Tilling());
                }
            }
        }
        //public override List<ScriptTask> GetAvailableTasks(Net.IObjectProvider net, Vector3 global)
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
            return this.Create(Material.Soil);
        }
        GameObject Create(Material mat)
        {
            GameObject obj = new GameObject();
            obj.AddComponent<GeneralComponent>().Initialize(Block.EntityIDRange + (int)this.Type, ObjectType.Block, this.GetName());
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
            obj.AddComponent<BlockComponent>().Initialize(this);
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite(this.Variations.First().Name, Map.BlockDepthMap)
            {
                Origin = Block.OriginCenter,
                Joint = Block.Joint,
                MouseMap = BlockMouseMap,
                Overlays = new Dictionary<string, Sprite>() { { "Body", new Sprite(this.Variations.First().Name, Map.BlockDepthMap) { Tint = mat.Color } } }
            });
            obj.AddComponent<GuiComponent>().Initialize(new Icon(obj.GetSprite()));
            obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", mat));
            return obj;
        }
    }
}
