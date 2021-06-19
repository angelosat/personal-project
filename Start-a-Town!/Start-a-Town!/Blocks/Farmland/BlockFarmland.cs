using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.GameModes;
using Start_a_Town_.Components;
using Start_a_Town_.Towns.Farming;
using Start_a_Town_.Graphics;
using Start_a_Town_.Net;

namespace Start_a_Town_.Blocks
{
    partial class BlockFarmland : Block
    {
        public override bool IsMinable => true;
        AtlasDepthNormals.Node.Token[] Textures;
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Soil;
        }
        public BlockFarmland()
            : base(Types.Farmland, GameObject.Types.Farmland)
        {
            //AssetNames = "farmland1, farmland2";
            this.Textures = new AtlasDepthNormals.Node.Token[2];
            this.Textures[0] = Block.Atlas.Load("blocks/farmland", Map.BlockDepthMap, Block.NormalMap);
            this.Textures[1] = Block.Atlas.Load("blocks/farmlandSowed", Map.BlockDepthMap, Block.NormalMap);
            this.Variations.Add(this.Textures[0]);
        }

        public override BlockEntity CreateBlockEntity()
        {
            return new BlockFarmlandEntity();
        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return this.Textures[data];
        }

        public static bool IsSeeded(byte data)
        {
            return data == 1;
        }
        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            var list = new List<Interaction>();
            list.Add(new InteractionPlantSeed()); // commented out until i figure out how to seperate ai planting job on farmlands and player planting anywhere
            list.Add(new Components.Vegetation.PlantableComponent.InteractionPlantNew());
            return list;
        }
        public override void OnDrop(GameObject actor, GameObject dropped, TargetArgs target, int amount = -1)
        {
            if (dropped.HasComponent<SeedComponent>())
                Plant(actor.Map, target.Global, dropped);
            else
                base.OnDrop(actor, dropped, target, amount);
            return;
            dropped.GetComponent<Components.Vegetation.PlantableComponent>().PlantAction(actor, target);

            //return;
            //Plant(actor.Map, target.Global, dropped);
        }
        static public void Plant(IMap map, Vector3 global, GameObject obj)
        {
            var plantdef = obj.GetComponent<SeedComponent>().Plant;
                var plant = plantdef.CreatePlant();
                plant.SyncInstantiate(map.Net);
                plant.SyncSpawn(map, global.Above());


            //BlockDefOf.Soil.Place(map, global, 0, map.GetCell(global).Variation, 0);
            var placer = new BlockSoil.Placer();
            placer.Place(map, global);
            map.Town.ZoneManager.GetZoneAt(global)?.Invalidate();
            obj.StackSize--;
            //return;

            //if (obj == null)
            //    throw new ArgumentNullException();
            //if (!obj.TryGetComponent<SeedComponent>(out var seed))
            //    throw new ArgumentException();
            //var plantID = seed.PlantDef; //(int)obj.ID;
            //var entity = new BlockFarmlandEntity(plantID, seed.Level);
            //map.AddBlockEntity(global, entity);
            //map.SetCellData(global, 1);
            //obj.StackSize--;
            //map.EventOccured(Message.Types.FarmSeedSowed, global);
        }
        //static public void Plant(IMap map, Vector3 global, GameObject obj)
        //{
        //    //var comp = obj.GetComponent<Components.Vegetation.PlantableComponent>();

        //    if (obj == null)
        //        throw new ArgumentNullException();
        //    SeedComponent seed;
        //    if (!obj.TryGetComponent<SeedComponent>(out seed))
        //        throw new ArgumentException();
        //    var plantID = (int)seed.Product; //(int)obj.ID;
        //    //var sproutMax = seed.Level * Engine.TicksPerSecond * 2;// *200; //2;
        //    var entity = new BlockFarmlandEntity(plantID, seed.Level);
        //    map.AddBlockEntity(global, entity);
        //    // change block texure
        //    //map.GetCell(global).Variation = 1;

        //    map.SetCellData(global, 1);
        //    //map.GetCell(global).BlockData = 1;
        //    //map.GetChunk(global).Valid = false;
        //    obj.StackSize--;
        //    map.EventOccured(Message.Types.FarmSeedSowed, global);
        //}
        static public void Fertilize(IMap map, Vector3 global, float potency)
        {
            var entity = map.GetBlockEntity(global) as BlockFarmland.BlockFarmlandEntity;
            if (entity == null)
                throw new Exception();
            entity.Sprout.Value -= entity.Sprout.Max * potency;
        }
        public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, Interaction> list)
        {
            var hauled = PersonalInventoryComponent.GetHauling(player).Object;
            if (hauled != null)
                if (hauled.HasComponent<SeedComponent>())
                {
                    list.Add(PlayerInput.RButton, new InteractionPlantSeed());// commented out until i figure out how to seperate ai planting job on farmlands and player planting anywhere
                    return;
                }
            base.GetPlayerActionsWorld(player, global, list);
        }
        internal override ContextAction GetContextRB(GameObject player, Vector3 global)
        {
            var hauled = PersonalInventoryComponent.GetHauling(player).Object;
            if (hauled != null)
                if (hauled.HasComponent<SeedComponent>())
                    return new ContextAction(new InteractionPlantSeed()) { Shortcut = PlayerInput.RButton }; // commented out until i figure out how to seperate ai planting job on farmlands and player planting anywhere
            return null;
        }
    }
}
