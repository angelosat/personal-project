//////////////////////////////////////////////////////////////////////////////////////////////////
//   BEGIN MySpriteBatch.cs
//////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Start_a_Town_
{

    public class MySpriteBatch
    {

        VertexPositionColorTexture[] vertices;
        short[] indices;
        int vertexCount = 0;
        int indexCount = 0;
        Texture2D texture;
        VertexDeclaration declaration;
        GraphicsDevice device;

        //  these should really be properties
        public Matrix World;
        public Matrix View;
        public Matrix Projection;
        public Effect Effect;

        public MySpriteBatch(GraphicsDevice device)
        {
            this.device = device;
            this.vertices = new VertexPositionColorTexture[256];
            this.indices = new short[vertices.Length * 3 / 2];
        }

        public void ResetMatrices(int width, int height)
        {
            this.World = Matrix.Identity;
            this.View = new Matrix(
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, -1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, -1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
            this.Projection = Matrix.CreateOrthographicOffCenter(
                0, width, -height, 0, 0, 1);
        }
        //public void Draw(Texture2D texture, Vector2 screenPos, Rectangle srcRectangle, float rotation, Vector2 origin, float scale, Color color, SpriteEffects sprFx, float depth)
        //{
        //    Matrix flip = Matrix.CreateScale(new Vector3(-1, 1, 1));
        //}
        public void Draw(Texture2D texture, Vector2 screenPos, Rectangle srcRectangle, float rotation, Vector2 origin, float scale, Color color, SpriteEffects sprFx, float depth)
        {
            //SpriteBatch sb = new SpriteBatch();
            //sb.Draw()
            Matrix flip;
            if (sprFx == SpriteEffects.FlipHorizontally)
            {
                float flipOffset = (float)Math.Floor(srcRectangle.Width / 2f); 
                float flipOffset2 = (float)Math.Ceiling(srcRectangle.Width / 2f);
                flip = Matrix.CreateTranslation(new Vector3(-flipOffset, 0, 0)) * Matrix.CreateScale(new Vector3(-1, 1, 1)) * Matrix.CreateTranslation(new Vector3(flipOffset2, 0, 0));// Matrix.CreateTranslation(new Vector3(srcRectangle.X / 2, 0, 0)) ;//* Matrix.CreateScale(new Vector3(sprFx == SpriteEffects.FlipHorizontally ? -1 : 1, 1, 1));
            }
            else flip = Matrix.CreateScale(1);
            Matrix rotate = Matrix.CreateRotationZ(rotation);
            Matrix toScreen = Matrix.CreateTranslation(new Vector3(screenPos, 0));
            //Matrix toOrigin = Matrix.CreateTranslation(new Vector3((sprFx == SpriteEffects.FlipHorizontally) ? - origin : origin - new Vector2(srcRectangle.X, srcRectangle.Y), 0));
            Matrix toOrigin = Matrix.CreateTranslation(new Vector3(-origin , 0));
            Matrix toScale = Matrix.CreateScale(scale);
            Matrix final = flip * toOrigin * toScale * rotate * toScreen;// *scale;
            //Matrix final = toOrigin * toScale * toScreen;
            //Vector4 destRectangle = new Vector4(screenPos.X, screenPos.Y, screenPos.X + scale * srcRectangle.Width, screenPos.Y + scale * srcRectangle.Height);
            //Vector2 tl = new Vector2(destRectangle.X, destRectangle.Y);
            //Vector2 tr = new Vector2(destRectangle.Z, destRectangle.Y);
            //Vector2 bl = new Vector2(destRectangle.X, destRectangle.W);
            //Vector2 br = new Vector2(destRectangle.Z, destRectangle.W);
            Vector2 tl = new Vector2(0, 0);
            Vector2 tr = new Vector2(srcRectangle.Width, 0);
            Vector2 bl = new Vector2(0, srcRectangle.Height);
            Vector2 br = new Vector2(srcRectangle.Width, srcRectangle.Height);
            //Vector4.Transform(ref destRectangle, ref rot, out destRectangle);
            tl = Vector2.Transform(tl, final);
            tr = Vector2.Transform(tr, final);
            bl = Vector2.Transform(bl, final);
            br = Vector2.Transform(br, final);
            if (this.texture != null && this.texture != texture)
                this.Flush();
            this.texture = texture;

            //  ensure space for my vertices and indices.
            this.EnsureSpace(6, 4);

            //  add the new indices
            indices[indexCount++] = (short)(vertexCount + 0);
            indices[indexCount++] = (short)(vertexCount + 1);
            indices[indexCount++] = (short)(vertexCount + 3);
            indices[indexCount++] = (short)(vertexCount + 1);
            indices[indexCount++] = (short)(vertexCount + 2);
            indices[indexCount++] = (short)(vertexCount + 3);
            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(tl, depth)
                , color, GetUV(srcRectangle.Left, srcRectangle.Top));
            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(tr, depth)
                , color, GetUV(srcRectangle.Right, srcRectangle.Top));
            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(br, depth)
                , color, GetUV(srcRectangle.Right, srcRectangle.Bottom));
            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(bl, depth)
                , color, GetUV(srcRectangle.Left, srcRectangle.Bottom));
        }
        public void Draw(Texture2D texture, Vector2 screenPos, Rectangle srcRectangle, float scale, Color color, float depth)
        {
            Vector4 destRectangle = new Vector4(screenPos.X, screenPos.Y, screenPos.X + scale * srcRectangle.Width, screenPos.Y + scale * srcRectangle.Height);
            this.Draw(texture, srcRectangle, destRectangle, color, depth);
        }
        public void Draw(Texture2D texture, Rectangle srcRectangle, Vector4 dstRectangle, Color color, float depth)
        {
            if (this.texture != null && this.texture != texture)
                this.Flush();
            this.texture = texture;

            //  ensure space for my vertices and indices.
            this.EnsureSpace(6, 4);

            //  add the new indices
            indices[indexCount++] = (short)(vertexCount + 0);
            indices[indexCount++] = (short)(vertexCount + 1);
            indices[indexCount++] = (short)(vertexCount + 3);
            indices[indexCount++] = (short)(vertexCount + 1);
            indices[indexCount++] = (short)(vertexCount + 2);
            indices[indexCount++] = (short)(vertexCount + 3);
            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(dstRectangle.X, dstRectangle.Y, depth)
                , color, GetUV(srcRectangle.Left, srcRectangle.Top));
            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(dstRectangle.Z, dstRectangle.Y, depth)
                , color, GetUV(srcRectangle.Right, srcRectangle.Top));
            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(dstRectangle.Z, dstRectangle.W, depth)
                , color, GetUV(srcRectangle.Right, srcRectangle.Bottom));
            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(dstRectangle.X, dstRectangle.W, depth)
                , color, GetUV(srcRectangle.Left, srcRectangle.Bottom));
            //vertices[vertexCount++] = new VertexPositionColorTexture(
            //    new Vector3(dstRectangle.Left, dstRectangle.Top, depth)
            //    , color, GetUV(srcRectangle.Left, srcRectangle.Top));
            //vertices[vertexCount++] = new VertexPositionColorTexture(
            //    new Vector3(dstRectangle.Right, dstRectangle.Top, depth)
            //    , color, GetUV(srcRectangle.Right, srcRectangle.Top));
            //vertices[vertexCount++] = new VertexPositionColorTexture(
            //    new Vector3(dstRectangle.Right, dstRectangle.Bottom, depth)
            //    , color, GetUV(srcRectangle.Right, srcRectangle.Bottom));
            //vertices[vertexCount++] = new VertexPositionColorTexture(
            //    new Vector3(dstRectangle.Left, dstRectangle.Bottom, depth)
            //    , color, GetUV(srcRectangle.Left, srcRectangle.Bottom));
        }
        public void DrawBlock(Texture2D texture, Rectangle srcRectangle, Rectangle dstRectangle, Color light, Color tint, float scale, float depth)
        {
            //  if the texture changes, we flush all queued sprites.
            if (this.texture != null && this.texture != texture)
                this.Flush();
            this.texture = texture;

            //  ensure space for my vertices and indices.
            this.EnsureSpace(6, 4);

            //  add the new indices
            indices[indexCount++] = (short)(vertexCount + 0);
            indices[indexCount++] = (short)(vertexCount + 1);
            indices[indexCount++] = (short)(vertexCount + 3);
            indices[indexCount++] = (short)(vertexCount + 1);
            indices[indexCount++] = (short)(vertexCount + 2);
            indices[indexCount++] = (short)(vertexCount + 3);

            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(dstRectangle.Left, dstRectangle.Top, depth)
                , Color.Red, GetUV(srcRectangle.Left, srcRectangle.Top));
            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(dstRectangle.Right, dstRectangle.Top, depth)
                , Color.Green, GetUV(srcRectangle.Right, srcRectangle.Top));
            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(dstRectangle.Right, dstRectangle.Bottom, depth)
                , Color.Blue, GetUV(srcRectangle.Right, srcRectangle.Bottom));
            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(dstRectangle.Left, dstRectangle.Bottom, depth)
                , Color.White, GetUV(srcRectangle.Left, srcRectangle.Bottom));

            // add the new vertices
            //vertices[vertexCount++] = new VertexPositionColorTexture(
            //    new Vector3(dstRectangle.Left, dstRectangle.Top, depth)
            //    , color, GetUV(srcRectangle.Left, srcRectangle.Top));
            //vertices[vertexCount++] = new VertexPositionColorTexture(
            //    new Vector3(dstRectangle.Right * scale, dstRectangle.Top, depth)
            //    , color, GetUV(srcRectangle.Right, srcRectangle.Top));
            //vertices[vertexCount++] = new VertexPositionColorTexture(
            //    new Vector3(dstRectangle.Right * scale, dstRectangle.Bottom * scale, depth)
            //    , color, GetUV(srcRectangle.Right, srcRectangle.Bottom));
            //vertices[vertexCount++] = new VertexPositionColorTexture(
            //    new Vector3(dstRectangle.Left, dstRectangle.Bottom * scale, depth)
            //    , color, GetUV(srcRectangle.Left, srcRectangle.Bottom));


        }
        public void Draw(Texture2D texture, Rectangle srcRectangle, Rectangle dstRectangle, Color color)
        {
            //  if the texture changes, we flush all queued sprites.
            if (this.texture != null && this.texture != texture)
                this.Flush();
            this.texture = texture;

            //  ensure space for my vertices and indices.
            this.EnsureSpace(6, 4);

            //  add the new indices
            indices[indexCount++] = (short)(vertexCount + 0);
            indices[indexCount++] = (short)(vertexCount + 1);
            indices[indexCount++] = (short)(vertexCount + 3);
            indices[indexCount++] = (short)(vertexCount + 1);
            indices[indexCount++] = (short)(vertexCount + 2);
            indices[indexCount++] = (short)(vertexCount + 3);

            // add the new vertices
            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(dstRectangle.Left, dstRectangle.Top, 0)
                , color, GetUV(srcRectangle.Left, srcRectangle.Top));
            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(dstRectangle.Right, dstRectangle.Top, 0)
                , color, GetUV(srcRectangle.Right, srcRectangle.Top));
            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(dstRectangle.Right, dstRectangle.Bottom, 0)
                , color, GetUV(srcRectangle.Right, srcRectangle.Bottom));
            vertices[vertexCount++] = new VertexPositionColorTexture(
                new Vector3(dstRectangle.Left, dstRectangle.Bottom, 0)
                , color, GetUV(srcRectangle.Left, srcRectangle.Bottom));
        }

        //public void Draw(Texture2D texture, Rectangle srcRectangle, Rectangle dstRectangle, Color color)
        //{
        //    //  if the texture changes, we flush all queued sprites.
        //    if (this.texture != null && this.texture != texture)
        //        this.Flush();
        //    this.texture = texture;

        //    //  ensure space for my vertices and indices.
        //    this.EnsureSpace(6, 4);

        //    //  add the new indices
        //    indices[indexCount++] = (short)(vertexCount + 0);
        //    indices[indexCount++] = (short)(vertexCount + 1);
        //    indices[indexCount++] = (short)(vertexCount + 3);
        //    indices[indexCount++] = (short)(vertexCount + 1);
        //    indices[indexCount++] = (short)(vertexCount + 2);
        //    indices[indexCount++] = (short)(vertexCount + 3);

        //    // add the new vertices
        //    vertices[vertexCount++] = new VertexPositionColorTexture(
        //        new Vector3(dstRectangle.Left, dstRectangle.Top, 0)
        //        , color, GetUV(srcRectangle.Left, srcRectangle.Top));
        //    vertices[vertexCount++] = new VertexPositionColorTexture(
        //        new Vector3(dstRectangle.Right, dstRectangle.Top, 0)
        //        , color, GetUV(srcRectangle.Right, srcRectangle.Top));
        //    vertices[vertexCount++] = new VertexPositionColorTexture(
        //        new Vector3(dstRectangle.Right, dstRectangle.Bottom, 0)
        //        , color, GetUV(srcRectangle.Right, srcRectangle.Bottom));
        //    vertices[vertexCount++] = new VertexPositionColorTexture(
        //        new Vector3(dstRectangle.Left, dstRectangle.Bottom, 0)
        //        , color, GetUV(srcRectangle.Left, srcRectangle.Bottom));

        //    //  we premultiply all vertices times the world matrix.
        //    //  the world matrix changes alot and we don't want to have to flush
        //    //  every time it changes.
        //    Matrix world = this.World;
        //    for (int i = vertexCount - 4; i < vertexCount; i++)
        //        Vector3.Transform(ref vertices[i].Position, ref world, out vertices[i].Position);
        //}

        Vector2 GetUV(float x, float y)
        {
            return new Vector2(x / (float)texture.Width, y / (float)texture.Height);
        }

        void EnsureSpace(int indexSpace, int vertexSpace)
        {
            if (indexCount + indexSpace >= indices.Length)
                Array.Resize(ref indices, Math.Max(indexCount + indexSpace, indices.Length * 2));
            if (vertexCount + vertexSpace >= vertices.Length)
                Array.Resize(ref vertices, Math.Max(vertexCount + vertexSpace, vertices.Length * 2));
        }

        public void FlushOld()
        {
            if (this.vertexCount > 0)
            {
                //Effect fx = Game1.Instance.Effect;
                //fx.CurrentTechnique = fx.Techniques["RealTime"];

                Effect fx = Game1.Instance.Content.Load<Effect>("blur");
                fx.CurrentTechnique = fx.Techniques["Normal"];

                fx.Parameters["BlockWidth"].SetValue(Block.Width + 2 * Graphics.Borders.Thickness);
                fx.Parameters["BlockHeight"].SetValue(Block.Height + 2 * Graphics.Borders.Thickness);
                fx.Parameters["AtlasWidth"].SetValue(Block.BlockAtlas.Texture.Width);
                fx.Parameters["AtlasHeight"].SetValue(Block.BlockAtlas.Texture.Height);
                fx.Parameters["Viewport"].SetValue(new Vector2(device.Viewport.Width, device.Viewport.Height));
                //fx.Parameters["TileVertEnsureDraw"].SetValue(Block.Depth / (float)Block.Height);
                device.Textures[0] = Block.BlockAtlas.Texture;
                device.Textures[2] = Map.ShaderMouseMap;
                device.DepthStencilState = DepthStencilState.Default;
                fx.CurrentTechnique.Passes["Pass1"].Apply();
                //if (fx.GraphicsDevice == device)
                //    "ok".ToConsole();
                device.DrawUserIndexedPrimitives<VertexPositionColorTexture>(
                    PrimitiveType.TriangleList, this.vertices, 0, this.vertexCount,
                    this.indices, 0, this.indexCount / 3);

                this.vertexCount = 0;
                this.indexCount = 0;
            }
        }
        public void Flush()
        {
            if (this.vertexCount > 0)
            {
                device.DrawUserIndexedPrimitives<VertexPositionColorTexture>(
                    PrimitiveType.TriangleList, this.vertices, 0, this.vertexCount,
                    this.indices, 0, this.indexCount / 3);

                this.vertexCount = 0;
                this.indexCount = 0;
            }
        }
        //public void Flush()
        //{
        //    if (this.vertexCount > 0)
        //    {
        //        if (this.declaration == null || this.declaration.IsDisposed)
        //            this.declaration = new VertexDeclaration(device, VertexPositionColorTexture.VertexElements);

        //        device.VertexDeclaration = this.declaration;

        //        Effect effect = this.Effect;
        //        //  set the only parameter this effect takes.
        //        effect.Parameters["viewProjection"].SetValue(this.View * this.Projection);
        //        effect.Parameters["diffuseTexture"].SetValue(this.texture);

        //        EffectTechnique technique = effect.CurrentTechnique;
        //        effect.Begin();
        //        EffectPassCollection passes = technique.Passes;
        //        for (int i = 0; i < passes.Count; i++)
        //        {
        //            EffectPass pass = passesIdea;
        //            pass.Begin();

        //            device.DrawUserIndexedPrimitives<VertexPositionColorTexture>(
        //                PrimitiveType.TriangleList, this.vertices, 0, this.vertexCount,
        //                this.indices, 0, this.indexCount / 3);

        //            pass.End();
        //        }
        //        effect.End();

        //        this.vertexCount = 0;
        //        this.indexCount = 0;
        //    }
        //}

    }

}
//////////////////////////////////////////////////////////////////////////////////////////////////
//   END MySpriteBatch.cs
//////////////////////////////////////////////////////////////////////////////////////////////////
