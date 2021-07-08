using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Graphics;

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
            : base(Types.Farmland)
        {
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
        
        public override void OnDrop(GameObject actor, GameObject dropped, TargetArgs target, int amount = -1)
        {
            if (dropped.HasComponent<SeedComponent>())
                Plant(actor.Map, target.Global, dropped);
            else
                base.OnDrop(actor, dropped, target, amount);
        }
        static public void Plant(IMap map, Vector3 global, GameObject obj)
        {
            var plantdef = obj.GetComponent<SeedComponent>().Plant;
                var plant = plantdef.CreatePlant();
                plant.SyncInstantiate(map.Net);
                plant.SyncSpawn(map, global.Above());

            var placer = new BlockSoil.Placer();
            placer.Place(map, global);
            map.Town.ZoneManager.GetZoneAt(global)?.Invalidate();
            obj.StackSize--;
        }
        
        static public void Fertilize(IMap map, Vector3 global, float potency)
        {
            var entity = map.GetBlockEntity(global) as BlockFarmland.BlockFarmlandEntity;
            if (entity == null)
                throw new Exception();
            entity.Sprout.Value -= entity.Sprout.Max * potency;
        }
    }
}
