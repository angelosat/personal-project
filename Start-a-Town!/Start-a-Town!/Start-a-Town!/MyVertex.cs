using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    public struct MyVertex : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
               new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), //position
               new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0), //fog
               new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Color, VertexElementUsage.Color, 1), //tint
               new VertexElement(sizeof(float) * 3 + 4 + 4, VertexElementFormat.Color, VertexElementUsage.Color, 2), //sunlight
               new VertexElement(sizeof(float) * 3 + 4 + 4 + 4, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1), //blocklight
               new VertexElement(sizeof(float) * 3 + 4 + 4 + 4 + sizeof(float) * 4, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0), //texcoord
               new VertexElement(sizeof(float) * 3 + 4 + 4 + 4 + sizeof(float) * 4 + sizeof(float) * 2, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2), //water
               new VertexElement(sizeof(float) * 3 + 4 + 4 + 4 + sizeof(float) * 4 + sizeof(float) * 2 + sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3), //material
               new VertexElement(sizeof(float) * 3 + 4 + 4 + 4 + sizeof(float) * 4 + sizeof(float) * 2 + sizeof(float) * 4 + sizeof(float) * 4, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 4) //local(?) block coordinates
               );

        // last working one (without fog)
        //public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
        //    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        //    new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
        //    new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Color, VertexElementUsage.Color, 1),
        //    //new VertexElement(sizeof(float) * 3 + 4 + 4, VertexElementFormat.Color, VertexElementUsage.Color, 2),
        //    //new VertexElement(sizeof(float) * 3 + 4 + 4 + 4, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)//, //color texture
        //    new VertexElement(sizeof(float) * 3 + 4 + 4, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
        //    new VertexElement(sizeof(float) * 3 + 4 + 4 + sizeof(float) * 4, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)//, //color texture
        ////    new VertexElement(sizeof(float) * 3 + 4 + 4 + sizeof(float) * 2, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1) //depth texture
        //    );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return MyVertex.VertexDeclaration; }
        }

        public Vector3 Position;
        public Color Fog;
        public Color Color;
        public Color SunLight;	// TODO: pack flags instead of color in vertex for lit face (FASTER!) //NO! the value is scalar
        public Vector4 BlockLight;	// TODO: pack flags instead of color in vertex for lit face (FASTER!) //NO! the value is scalar
        public Vector2 TexCoord;
        public Vector4 Water;
        public Vector4 Material;
        public Vector3 BlockCoords;
        //     public Vector2 DepthTexCoord;

        public MyVertex(Vector3 pos, Color color, Color sunlight, Vector4 blocklight, Vector2 texCoord) //: this(pos, color, light, texCoord, texCoord)// { }
        //public MyVertex(Vector3 pos, Color color, Color light, Vector2 texCoord, Vector2 depthTexCoord)
        {
            this.Position = pos;
            this.Fog = Color.Transparent;
            this.Color = color;
            this.SunLight = sunlight;
            this.BlockLight = blocklight;// new Vector4(blocklight.R / 255f, blocklight.G / 255f, blocklight.B / 255f, blocklight.A / 255f);// blocklight.ToVector4();
            this.TexCoord = texCoord;
            this.Water = Color.Transparent.ToVector4();
            //  this.DepthTexCoord = depthTexCoord;
            this.Material = Vector4.One;
            this.BlockCoords = Vector3.Zero;
        }
        public MyVertex(Vector3 pos, Color fog, Color tint, Color sunlight, Vector4 blocklight, Vector2 texCoord) //: this(pos, color, light, texCoord, texCoord)// { }
        //public MyVertex(Vector3 pos, Color color, Color light, Vector2 texCoord, Vector2 depthTexCoord)
        {
            this.Position = pos;
            this.Fog = fog;
            this.Color = tint;
            this.SunLight = sunlight;
            this.BlockLight = blocklight;// new Vector4(blocklight.R / 255f, blocklight.G / 255f, blocklight.B / 255f, blocklight.A / 255f);// blocklight.ToVector4();
            this.TexCoord = texCoord;
            this.Water = Color.Transparent.ToVector4();
            this.Material = Vector4.One;
            //  this.DepthTexCoord = depthTexCoord;
            this.BlockCoords = Vector3.Zero;
        }
        public MyVertex(Vector3 pos, Color fog, Color tint, Color sunlight, Vector4 blocklight, Vector4 water, Vector2 texCoord)
        {
            this.Position = pos;
            this.Fog = fog;
            this.Color = tint;
            this.SunLight = sunlight;
            this.BlockLight = blocklight;
            this.TexCoord = texCoord;
            this.Water = water;
            this.Material = Vector4.One;
            this.BlockCoords = Vector3.Zero;
        }
        public MyVertex(Vector3 pos, Color fog, Color tint, Color material, Color sunlight, Vector4 blocklight, Vector4 water, Vector2 texCoord)
        {
            this.Position = pos;
            this.Fog = fog;
            this.Color = tint;
            this.SunLight = sunlight;
            this.BlockLight = blocklight;
            this.TexCoord = texCoord;
            this.Water = water;
            this.Material = material.ToVector4();
            this.BlockCoords = Vector3.Zero;
        }
        public MyVertex(Vector3 pos, Color fog, Color tint, Vector4 material, Color sunlight, Vector4 blocklight, Vector4 water, Vector2 texCoord)
        {
            this.Position = pos;
            this.Fog = fog;
            this.Color = tint;
            this.SunLight = sunlight;
            this.BlockLight = blocklight;
            this.TexCoord = texCoord;
            this.Water = water;
            this.Material = material;
            this.BlockCoords = Vector3.Zero;
        }
        public MyVertex(Vector3 pos, Color fog, Color tint, Vector4 material, Color sunlight, Vector4 blocklight, Vector4 water, Vector2 texCoord, Vector3 blockCoords)
        {
            this.Position = pos;
            this.Fog = fog;
            this.Color = tint;
            this.SunLight = sunlight;
            this.BlockLight = blocklight;
            this.TexCoord = texCoord;
            this.Water = water;
            this.Material = material;
            this.BlockCoords = blockCoords;
        }
        //public Color BlockLight;	// TODO: pack flags instead of color in vertex for lit face (FASTER!) //NO! the value is scalar
        //public Vector2 TexCoord;
        ////     public Vector2 DepthTexCoord;

        //public MyVertex(Vector3 pos, Color color, Color sunlight, Color blocklight, Vector2 texCoord) //: this(pos, color, light, texCoord, texCoord)// { }
        ////public MyVertex(Vector3 pos, Color color, Color light, Vector2 texCoord, Vector2 depthTexCoord)
        //{
        //    this.Position = pos;
        //    this.Color = color;
        //    this.SunLight = sunlight;
        //    this.BlockLight = blocklight;
        //    this.TexCoord = texCoord;
        //    //  this.DepthTexCoord = depthTexCoord;
        //}


    }
}
