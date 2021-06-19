﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Blocks
{
    class BlockRoof : Block
    {
        static Texture2D Depth1 = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/roof/depth1height19");
        static Texture2D Depth2 = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/roof/depth2height19");
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.LightWood;
        }
        AtlasDepthNormals.Node.Token[] Parts = new AtlasDepthNormals.Node.Token[4];
        public BlockRoof():base(Block.Types.Roof, opaque: false)
        {
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
            //this.MaterialType = MaterialType.Wood;
            //this.Material = Material.LightWood;
            this.Parts[0] = Block.Atlas.Load("blocks/roof/roof1height19", Depth1, Block.NormalMap);
            this.Parts[3] = Block.Atlas.Load("blocks/roof/roof2height19", Depth2, Block.NormalMap);
            this.Parts[2] = Block.Atlas.Load("blocks/roof/roof3height19", Map.BlockDepthMap, Block.NormalMap);
            this.Parts[1] = Block.Atlas.Load("blocks/roof/roof4height19", Map.BlockDepthMap, Block.NormalMap);
            this.Variations.Add(this.Parts.First());

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfSubType(ItemSubType.Planks, ItemSubType.Ingots)
                        )),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            Towns.Constructions.ConstructionsManager.Walls.Add(this.Recipe);
        }
        public override float GetHeight(byte data, float x, float y)
        {
            //return (1 - y);
            switch(data)
            {
                case 0:
                    return (1 - y);

                case 1:
                    return (1 - x);

                case 2:
                    return y;
                    
                case 3:
                    return x;

                default:
                    break;
            }
            return 1;
        }
        public override float GetHeight(float x, float y)
        {
            throw new Exception();
        }
        static byte GetData(int rotation)
        {
            return (byte)rotation;
        }
        //public override List<byte> GetVariations()
        //{
        //    return new List<byte>() { 0, 1, 2, 3 };
        //}
        public override AtlasDepthNormals.Node.Token GetPreviewToken(int variation, int orientation, int cameraRotation, byte data)
        {
            var o = (orientation + cameraRotation) % 4;
            return this.Parts[o];
        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            var o = (data + cameraRotation) % 4;
            return this.Parts[o];
        }
        public override void Place(IMap map, Vector3 global, byte data, int variation, int orientation, bool notify = true)
        {
            base.Place(map, global, GetData(orientation), variation, orientation, notify);
        }
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            if (this == BlockDefOf.Air)
                return null;
            var material = this.GetColorVector(data);
            var token = this.GetToken(variation, orientation, (int)camera.Rotation, data);// maybe change the method to accept double so i don't have to cast the camera rotation to int?
            return canvas.Opaque.DrawBlock(Block.Atlas.Texture, screenBounds,
                token,
                camera.Zoom, fog, tint, material, sunlight, blocklight, Vector4.Zero, depth, this, blockCoordinates);
        }
    }
}
