using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    public struct MyVertex : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration = new(
               new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), //position
               new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0), //fog
               new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Color, VertexElementUsage.Color, 1), //tint
               new VertexElement(sizeof(float) * 3 + 4 + 4, VertexElementFormat.Color, VertexElementUsage.Color, 2), //sunlight
               new VertexElement(sizeof(float) * 3 + 4 + 4 + 4, VertexElementFormat.Color, VertexElementUsage.Color, 3), //blocklight
               new VertexElement(sizeof(float) * 3 + 4 + 4 + 4 + 4, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0), //texcoord
               new VertexElement(sizeof(float) * 3 + 4 + 4 + 4 + 4 + sizeof(float) * 2, VertexElementFormat.Color, VertexElementUsage.Color, 4), //water
               new VertexElement(sizeof(float) * 3 + 4 + 4 + 4 + 4 + sizeof(float) * 2 + 4, VertexElementFormat.Color, VertexElementUsage.Color, 5), //material
               new VertexElement(sizeof(float) * 3 + 4 + 4 + 4 + 4 + sizeof(float) * 2 + 4 + 4, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 3)//, //local(?) block coordinates
               );
        
        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return MyVertex.VertexDeclaration; }
        }

        public Vector3 Position;
        public Color Fog;
        public Color Color;
        public Color SunLight;
        public Color BlockLight;
        public Vector2 TexCoord;
        public Color Water;
        public Color Material;
        public Vector3 BlockCoords;
        
        public MyVertex(Vector3 pos, Color color, Color sunlight, Vector4 blocklight, Vector2 texCoord)
        {
            this.Position = pos;
            this.Fog = Color.Transparent;
            this.Color = color;
            this.SunLight = sunlight;
            this.BlockLight = new Color(blocklight);
            this.TexCoord = texCoord;
            this.Water = Color.Transparent;
            this.Material = new Color(Vector4.One);
            this.BlockCoords = Vector3.Zero;
        }
        public MyVertex(Vector3 pos, Color fog, Color tint, Color sunlight, Vector4 blocklight, Vector2 texCoord)
        {
            this.Position = pos;
            this.Fog = fog;
            this.Color = tint;
            this.SunLight = sunlight;
            this.BlockLight = new Color(blocklight);
            this.TexCoord = texCoord;
            this.Water = Color.Transparent;
            this.Material = new Color(Vector4.One);
            this.BlockCoords = Vector3.Zero;
        }
        public MyVertex(Vector3 pos, Color fog, Color tint, Color sunlight, Vector4 blocklight, Vector4 water, Vector2 texCoord)
        {
            this.Position = pos;
            this.Fog = fog;
            this.Color = tint;
            this.SunLight = sunlight;
            this.BlockLight = new Color(blocklight);
            this.TexCoord = texCoord;
            this.Water = new Color(water);
            this.Material = new Color(Vector4.One);
            this.BlockCoords = Vector3.Zero;
            var cam = Net.Client.Instance.Map.Camera;
            float blockx = (float)(this.BlockCoords.X * cam.RotCos - this.BlockCoords.Y * cam.RotSin);
            float blocky = (float)(this.BlockCoords.X * cam.RotSin + this.BlockCoords.Y * cam.RotCos);
            this.BlockCoords = new Vector3(blockx, blocky, this.BlockCoords.Z);
        }
        public MyVertex(Vector3 pos, Color fog, Color tint, Color material, Color sunlight, Vector4 blocklight, Vector4 water, Vector2 texCoord)
        {
            this.Position = pos;
            this.Fog = fog;
            this.Color = tint;
            this.SunLight = sunlight;
            this.BlockLight = new Color(blocklight);
            this.TexCoord = texCoord;
            this.Water = new Color(water);
            this.Material = material;//.ToVector4();
            this.BlockCoords = Vector3.Zero;
        }
        public MyVertex(Vector3 pos, Color fog, Color tint, Vector4 material, Color sunlight, Vector4 blocklight, Vector4 water, Vector2 texCoord)
        {
            this.Position = pos;
            this.Fog = fog;
            this.Color = tint;
            this.SunLight = sunlight;
            this.BlockLight = new Color(blocklight);
            this.TexCoord = texCoord;
            this.Water = new Color(water);
            this.Material = new Color(material);
            this.BlockCoords = Vector3.Zero;
            var cam = Net.Client.Instance.Map.Camera;

            float blockx = (float)(this.BlockCoords.X * cam.RotCos - this.BlockCoords.Y * cam.RotSin);
            float blocky = (float)(this.BlockCoords.X * cam.RotSin + this.BlockCoords.Y * cam.RotCos);
            this.BlockCoords = new Vector3(blockx, blocky, this.BlockCoords.Z);
        }
        public MyVertex(Vector3 pos, Color fog, Color tint, Vector4 material, Color sunlight, Vector4 blocklight, Vector4 water, Vector2 texCoord, Block block, Vector3 blockGlobalCoords)
        {
            this.Position = pos;
            this.Fog = fog;
            this.Color = tint;
            this.SunLight = sunlight;
            this.BlockLight = new Color(blocklight);
            this.TexCoord = texCoord;
            this.Water = new Color(water);
            this.Material = new Color(material);
            this.BlockCoords = blockGlobalCoords;
        }
    }
}
