using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.Towns.Forestry;
using Start_a_Town_.Particles;
using System.IO;

namespace Start_a_Town_
{
    class BlockGrass : Block
    {
        public override bool IsMinable => true;

        public override Color DirtColor
        {
            get
            {
                return Color.DarkOliveGreen;// *.5f;//.DarkOliveGreen;// Color.GreenYellow;
            }
        }
        public override ParticleEmitterSphere GetEmitter()
        {
            return base.GetDirtEmitter();
        }

        List<AtlasDepthNormals.Node.Token> Overlays = new List<AtlasDepthNormals.Node.Token>(3);
        public static List<AtlasDepthNormals.Node.Token> FlowerOverlays = new List<AtlasDepthNormals.Node.Token>();

        static public readonly double TramplingChance = 0.1f;

        public BlockGrass()
            : base(Block.Types.Grass, GameObject.Types.Grass, 0, 1, true, true)
        {
            //this.Material = Material.Soil;
            //this.MaterialType = MaterialType.Soil;
            this.LootTable = new LootTable(
                       
                        //new Loot(Material.Soil.CreateRawMaterialItem, chance: 1f, count: 1)
                        new Loot(()=>ItemFactory.CreateFrom(RawMaterialDef.Bags, MaterialDefOf.Soil), chance: 1f, count: 1)

                        );
                        //,
                        //new Loot(GameObject.Types.Cobblestones, chance: 0.25f, count: 1),
                        //new Loot(GameObject.Types.Twig, chance: 0.25f, count: 1));

            //foreach (var asset in new string[] { "blocks/grass1", "blocks/grass2", "blocks/grass3" })
            //    this.Variations.Add(Block.Atlas.Load(asset));
            //foreach (var asset in new string[] { "blocks/grass/grass1-overlay", "blocks/grass/grass2-overlay", "blocks/grass/grass3-overlay" })
            //    this.Overlays.Add(Block.Atlas.Load(asset));

            //this.AssetNames = "grass1, grass2, grass3";
            //this.AssetNames = "grass1-test3b, grass2-test3b, grass3-test3b";
            this.AssetNames = "grass/grass1, grass/grass2, grass/grass3, grass/grass4";

            foreach (var item in new AtlasDepthNormals.Node.Token[] { 
                Block.Atlas.Load("blocks/grass/grass1-overlay", Map.BlockDepthMap, Block.BlockMouseMap.Texture),
                Block.Atlas.Load("blocks/grass/grass2-overlay", Map.BlockDepthMap, Block.BlockMouseMap.Texture),
                Block.Atlas.Load("blocks/grass/grass3-overlay", Map.BlockDepthMap, Block.BlockMouseMap.Texture)})
                this.Overlays.Add(item);

            //this.FlowerOverlays.Add(Block.Atlas.Load("blocks/grass/flowersoverlayred", Block.SliceBlockDepthMap, Block.NormalMap));
            //this.FlowerOverlays.Add(Block.Atlas.Load("blocks/grass/flowersoverlayyellow", Block.SliceBlockDepthMap, Block.NormalMap));
            //this.FlowerOverlays.Add(Block.Atlas.Load("blocks/grass/flowersoverlaywhite", Block.SliceBlockDepthMap, Block.NormalMap));
            //this.FlowerOverlays.Add(Block.Atlas.Load("blocks/grass/flowersoverlaypurple", Block.SliceBlockDepthMap, Block.NormalMap));

            FlowerOverlays.Add(Block.Atlas.Load("blocks/grass/flowersoverlayred", Map.BlockDepthMap, Block.NormalMap));
            FlowerOverlays.Add(Block.Atlas.Load("blocks/grass/flowersoverlayyellow", Map.BlockDepthMap, Block.NormalMap));
            FlowerOverlays.Add(Block.Atlas.Load("blocks/grass/flowersoverlaywhite", Map.BlockDepthMap, Block.NormalMap));
            FlowerOverlays.Add(Block.Atlas.Load("blocks/grass/flowersoverlaypurple", Map.BlockDepthMap, Block.NormalMap));
            //foreach (var item in new Sprite[] { 
            //    new Sprite("blocks/grass/grass1-overlay", Map.BlockDepthMap),
            //    new Sprite("blocks/grass/grass2-overlay", Map.BlockDepthMap),
            //    new Sprite("blocks/grass/grass3-overlay", Map.BlockDepthMap)})
            //    this.Overlays.Add(item.AtlasToken);
        }
        //internal static void SyncGrowRandomFlower(IMap map, IntVec3 global)
        //{
        //    map.SetCellData(global, (byte)(map.Random.Next(FlowerOverlays.Count) + 1));
        //}
        internal static void GrowRandomFlower(IMap map, IntVec3 global)
        {
            var net = map.Net;
            if (net is Client)
                throw new Exception();
            byte data = (byte)(map.Random.Next(FlowerOverlays.Count) + 1);
            map.SyncSetCellData(global, data);
        }

        public override byte ParseData(string data)
        {
            return byte.Parse(data);
        }

        public override void OnMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Shovel:
                    this.Break(parent.Map, parent.Global);
                    //LootTable table = new LootTable(
                    //    new Loot(GameObject.Types.Soilbag, chance: 1f, count: 1),
                    //    new Loot(GameObject.Types.Cobble, chance: 0.25f, count: 1),
                    //    new Loot(GameObject.Types.Twig, chance: 0.25f, count: 1));

                    //e.Network.PopLoot(table, parent.Global, parent.Velocity);
                    //e.Network.Despawn(parent);
                    //e.Network.DisposeObject(parent);
                    return;

                default:
                    break;
            }
        }

        AtlasDepthNormals.Node.Token GetFlowerOverlay(byte data)
        {
            var flowerIndex = data - 1; //because 0 is no flowers
            return FlowerOverlays[flowerIndex];
        }
        public override IEnumerable<byte> GetCraftingVariations()
        {
            var list = base.GetCraftingVariations();
            foreach (var i in list)
                yield return i;
            yield return 1;
            yield return 2;
            yield return 3;
            yield return 4;
            //list.AddRange(new byte[] { 1, 2, 3, 4 });
            //return list;
        }
        static void Trample(IMap map, IntVec3 global)
        {
            var cell = map.GetCell(global);
            BlockDefOf.Soil.Place(map, global, 0, cell.Variation, 0);
        }
       
        public override void OnSteppedOn(GameObject actor, Vector3 global)
        {
            var net = actor.NetNew;
            if (net is Client)
                return;
            if (actor.Map.Random.Chance(TramplingChance))
                Packets.SyncTrample(actor.Map, global);
            
        }
       
        public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, Interaction> list)
        {
           
            list.Add(PlayerInput.Activate, new InteractionClearGrass());
            var mainhand = GearComponent.GetSlot(PlayerOld.Actor, GearType.Mainhand);
            if (mainhand.Object != null)
            {
                var skill = mainhand.Object.GetComponent<ToolAbilityComponent>();
                if (skill != null)
                {
                    if (ToolAbilityComponent.HasSkill(mainhand.Object, ToolAbilityDef.Digging))
                        list[PlayerInput.RButton] = new InteractionDigging();
                    else if (ToolAbilityComponent.HasSkill(mainhand.Object, ToolAbilityDef.Argiculture))
                        list[PlayerInput.RButton] = new InteractionTilling();
                }
            }
               base.GetPlayerActionsWorld(player, global, list);
            
        }
        internal override ContextAction GetContextRB(GameObject player, Vector3 global)
        {
            var mainhand = GearComponent.GetSlot(PlayerOld.Actor, GearType.Mainhand);
            if (mainhand.Object != null)
            {
                var skill = mainhand.Object.GetComponent<ToolAbilityComponent>();
                if (skill != null)
                {
                    if (ToolAbilityComponent.HasSkill(mainhand.Object, ToolAbilityDef.Digging))
                        return new ContextAction(new InteractionDigging()) { Shortcut = PlayerInput.RButton };
                    else if (ToolAbilityComponent.HasSkill(mainhand.Object, ToolAbilityDef.Argiculture))
                        return new ContextAction(new InteractionTilling()) { Shortcut = PlayerInput.RButton };
                }
            }
            return base.GetContextRB(player, global);
        }
        internal override ContextAction GetContextActivate(GameObject player, Vector3 global)
        {
            return new ContextAction(new InteractionClearGrass()) { Shortcut = PlayerInput.Activate };
        }

        
        internal override float GetFertility(Cell cell)
        {
            if (cell.BlockData > 0) // if there are flowers grown, dont grow anything else (return fertility = 0)
                return 0;
            return base.GetFertility(cell);
        }
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Soil;
        }
       
        public override void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, float zoom, float depth, Cell cell)
        {
            //sb.DrawBlock(Block.Atlas.Texture, screenPos, this.Variations[cell.Variation].Rectangle, zoom, Color.White, sunlight, blocklight, depth);
            sb.DrawBlock(Block.Atlas.Texture, screenPos, this.Variations[cell.Variation], zoom, Color.White, sunlight, blocklight, depth);
        }
        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, float zoom, float depth, Cell cell)
        {
            //sb.DrawBlock(Block.Atlas.Texture, screenPos, this.Variations[cell.Variation].Rectangle, zoom, Color.White, sunlight, blocklight, depth);
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[cell.Variation], zoom, Color.White, sunlight, blocklight, depth);
        }

        public override MyVertex[] Draw(Chunk chunk, Vector3 blockcoords, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            return base.Draw(chunk, blockcoords, camera, screenBounds, sunlight, blocklight, fog, tint, depth, variation, orientation, data);
        }
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            base.Draw(canvas, chunk, blockCoordinates, camera, screenBounds, sunlight, blocklight, fog, tint, depth, variation, orientation, data);
            if (data == 0)
                return null;
            var fl = this.GetFlowerOverlay(data);
            return canvas.Opaque.DrawBlock(fl.Atlas.Texture, screenBounds, fl, camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Vector4.Zero, depth, this, blockCoordinates);
        }
        static BlockGrass() { }
        public class Packets
        {
            static readonly int PacketGrowRandomFlower, PacketTrample;
            static Packets()
            {
                PacketGrowRandomFlower = Network.RegisterPacketHandler(GrowRandomFlower);
                PacketTrample = Network.RegisterPacketHandler(SyncTrample);
            }
            public static void GrowRandomFlower(IMap map, IntVec3 global)
            {
                var net = map.Net;
                if(net is Server)
                    BlockGrass.GrowRandomFlower(map, global);
                net.GetOutgoingStream().Write(PacketGrowRandomFlower, global);
            }
            private static void GrowRandomFlower(IObjectProvider net, BinaryReader r)
            {
                var map = net.Map;
                var global = r.ReadIntVec3();
                if (net is Client)
                    BlockGrass.GrowRandomFlower(map, global);
                else
                    GrowRandomFlower(map, global);
            }
            public static void SyncTrample(IMap map, IntVec3 global)
            {
                var net = map.Net;
                if (net is Client)
                    throw new Exception();
                Trample(map, global);
                net.WriteToStream(PacketTrample, global);
            }
            private static void SyncTrample(IObjectProvider net, BinaryReader r)
            {
                var global = r.ReadIntVec3();
                if (net is Server)
                    throw new Exception();
                Trample(net.Map, global);
            }
        }
    }
}
