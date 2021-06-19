using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;
using Start_a_Town_.Towns.Forestry;

namespace Start_a_Town_
{
    class BlockGrass : Block
    {
        public override Color DirtColor
        {
            get
            {
                return Color.DarkOliveGreen;// *.5f;//.DarkOliveGreen;// Color.GreenYellow;
            }
        }
        public override Components.Particles.ParticleEmitterSphere GetEmitter()
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
                //new Loot(GameObject.Types.Soilbag, chance: 1f, count: 1),
                //new Loot(this.Material.ProcessingChain.First().ID, chance: 1f, count: 1),
                        new Loot(Material.Soil.ProcessingChain.First().ID, chance: 1f, count: 1),
                        new Loot(GameObject.Types.Cobble, chance: 0.25f, count: 1),
                        new Loot(GameObject.Types.Twig, chance: 0.25f, count: 1));

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
        public override List<byte> GetVariations()
        {
            var list = base.GetVariations();
            list.AddRange(new byte[] { 1, 2, 3, 4 });
            return list;
        }
        static void Trample(GameObject actor, Vector3 global, double chance)
        {
            if (chance < TramplingChance)
            {
                //global.TrySetCell(net, Block.Types.Soil, net.Map.GetData(global));
                var cell = actor.Map.GetCell(global);
                Block.Soil.Place(actor.Map, global, 0, cell.Variation, 0);
            }
        }
        public override void OnSteppedOn(GameObject actor, Vector3 global)
        {
            //if (chance < TramplingChance)
            //    global.TrySetCell(net, Block.Types.Soil, net.Map.GetData(global));
            var server = actor.Net as Server;
            if (server == null)
                return;
            double ran = server.GetRandom().NextDouble();
            Trample(actor, global, ran);
            //server.RandomEvent(new TargetArgs(global), e, rnd => StepOn(e.Network, global, actor, rnd));
            byte[] data = Network.Serialize(w =>
            {
                new TargetArgs(global).Write(w);
                w.Write((int)Message.Types.StepOn);
                w.Write(actor.Network.ID);
                w.Write(ran);
            });
            server.RemoteProcedureCall(data, global);
        }
        internal override void RemoteProcedureCall(IObjectProvider net, Vector3 global, Message.Types type, System.IO.BinaryReader r)
        {
            switch (type)
            {
                case Message.Types.StepOn:
                    var actor = net.GetNetworkObject(r.ReadInt32());
                    var ran = r.ReadDouble();
                    Trample(actor, global, ran);
                    return;

                default:
                    break;
            }
        }
        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            return new List<Interaction>(){
                new InteractionClearGrass(),
                new InteractionDigging(),
                new Tilling(),
                new InteractionPlantTree()
            };
        }
        //public override ContextAction GetRightClickAction(Vector3 global)
        //{
        //    return new ContextAction(() => "Dig", () => { Net.Client.PlayerInteract(new TargetArgs(global)); });
        //}

        public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, Interaction> list)
        {
            base.GetPlayerActionsWorld(player, global, list);
            if (list.ContainsKey(PlayerInput.RButton))
                return;
            list.Add(new PlayerInput(PlayerActions.Activate), new InteractionClearGrass());
            var mainhand = GearComponent.GetSlot(Player.Actor, GearType.Mainhand);
            if(mainhand.Object!=null)
            {
                var skill = mainhand.Object.GetComponent<SkillComponent>();
                if(skill != null)
                {
                    if(SkillComponent.HasSkill(mainhand.Object, Skill.Digging))
                        list.Add(PlayerInput.RButton, new InteractionDigging());
                    else if (SkillComponent.HasSkill(mainhand.Object, Skill.Argiculture))
                        list.Add(PlayerInput.RButton, new Tilling());
                }
            }
            //else
            //    base.GetPlayerActionsWorld(map, global, list);
            
        }
        internal override ContextAction GetContextActivate(GameObject player, Vector3 global)
        {
            return new ContextAction(new InteractionClearGrass()) { Shortcut = PlayerInput.Activate };
        }

        //public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, ContextAction> list)
        //{
        //    base.GetPlayerActionsWorld(player, global, list);
        //    if (list.ContainsKey(PlayerInput.RButton))
        //        return;
        //    list.Add(new PlayerInput(PlayerActions.Activate), new ContextAction(new InteractionClearGrass()));
        //    var mainhand = GearComponent.GetSlot(Player.Actor, GearType.Mainhand);
        //    if (mainhand.Object != null)
        //    {
        //        var skill = mainhand.Object.GetComponent<SkillComponent>();
        //        if (skill != null)
        //        {
        //            if (SkillComponent.HasSkill(mainhand.Object, Skill.Digging))
        //                list.Add(PlayerInput.RButton, new ContextAction(new InteractionDigging()));
        //            else if (SkillComponent.HasSkill(mainhand.Object, Skill.Argiculture))
        //                list.Add(PlayerInput.RButton, new ContextAction(new Tilling()));
        //        }
        //    }
        //    //else
        //    //    base.GetPlayerActionsWorld(map, global, list);

        //}
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Soil;
        }
        //public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Vector2 screenPos, Color color, float zoom, float depth, Cell cell)
        //{
        //    //base.Draw(sb, screenPos, sourceRect, color, zoom, depth, cell);
        //    sb.Draw(Block.Atlas.Texture, screenPos, this.Variations[cell.Variation].Rectangle, color, 0, Vector2.Zero, zoom, SpriteEffects.None, depth);
        //}
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
            if (data == 0)
                return null;
            var fl = this.GetFlowerOverlay(data);// this.FlowerOverlays.First();
            //var zz = z + 1;
            //var bounds = camera.GetScreenBoundsVector4(x, y, zz, Block.Bounds, Vector2.Zero);
            //var d = new Vector3(x, y, zz).GetDrawDepth(Client.Instance.Map, camera);
            //camera.SpriteBatch.DrawBlock(fl.Atlas.Texture, bounds, fl, camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Vector4.Zero, d);
            return chunk.VertexBuffer.DrawBlock(fl.Atlas.Texture, screenBounds, fl, camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Vector4.Zero, depth);

        }
        public override MyVertex[] Draw(MySpriteBatch mesh, MySpriteBatch nonopaquemesh, MySpriteBatch transparentMesh, Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            base.Draw(mesh, nonopaquemesh, transparentMesh, chunk, blockCoordinates, camera, screenBounds, sunlight, blocklight, fog, tint, depth, variation, orientation, data);
            if (data == 0)
                return null;
            var fl = this.GetFlowerOverlay(data);
            return mesh.DrawBlock(fl.Atlas.Texture, screenBounds, fl, camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Vector4.Zero, depth, blockCoordinates);
        }
    }
}
