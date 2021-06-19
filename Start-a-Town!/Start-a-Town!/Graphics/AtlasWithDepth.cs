using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Graphics
{
    public partial class AtlasWithDepth : AtlasBase
    {
        public HashSet<Node.Token> ToLoad = new HashSet<Node.Token>();
        public string Name { get; protected set; }
        public AtlasWithDepth(string name)
        {
            this.Name = name;
        }

        Dictionary<string, Node.Token> Tokens = new Dictionary<string, Node.Token>();
        //public List<Node.Token> Load(params string[] assetNames)
        //{
        //    List<Node.Token> tokens = new List<Node.Token>();
        //    foreach (var asset in assetNames)
        //        tokens.Add(this.Load(asset));
        //    return tokens;
        //}
        public Node.Token Load(string assetName)
        {
            Node.Token token;
            if (this.Tokens.TryGetValue(assetName, out token))
                return token;
            token = new Node.Token(this, assetName);
            this.Tokens.Add(assetName, token);
            this.ToLoad.Add(token);
            return token;
        }
        public Node.Token Load(string assetName, string depthMap)
        {
            //Node.Token token = new Node.Token(assetName);
            //this.ToLoad.Add(token);
            Node.Token token;
            if (this.Tokens.TryGetValue(assetName, out token))
                return token;
            token = new Node.Token(this, assetName, depthMap);
            this.Tokens.Add(assetName, token);
            this.ToLoad.Add(token);
            return token;
        }
        public Node.Token Load(string assetName, Texture2D depthtexture)
        {
            Node.Token token;
            if (this.Tokens.TryGetValue(assetName, out token))
            {
                //token.DepthTexture = depthtexture;
                return token;
            }
            token = new Node.Token(this, assetName, depthtexture);
            this.Tokens.Add(assetName, token);
            this.ToLoad.Add(token);
            return token;
        }
        public Node.Token Load(string assetName, float depthLayer)
        {
            Node.Token token;
            if (this.Tokens.TryGetValue(assetName, out token))
            {
                return token;
            }
            token = new Node.Token(this, assetName, depthLayer);
            this.Tokens.Add(assetName, token);
            this.ToLoad.Add(token);
            return token;
        }

        //public Texture2D Texture;// { get; set; }
        //public Texture2D DepthTexture;// { get; set; }

        static public readonly int Size = 1024;
        List<Node> Nodes = new List<Node>() { new Node(0, 0, Size, Size) };// 2048, 2048) };
        Node RootNode = new Node(0, 0, Size, Size);
        //static public Vector2 Add(string texturePath)
        //{
        //    Texture2D tex = Game1.Instance.Content.Load<Texture2D>(texturePath);
        //    return Add2(tex);
        //}
        public void Add(Node.Token token)
        {
            //this.AddTexture(token, out token.Rectangle);
            Rectangle rect;
            this.AddTexture(token, out rect);
            //token.Rectangle = rect;
            token.SetRectangle(rect);
            return;
        }

        public Node AddTexture(Node.Token token, out Rectangle rect)
        {
            Texture2D tex = token.Texture;
            Node emptyNode = this.RootNode.FindNode(tex.Width, tex.Height); // find first empty node that can fit it
            if (emptyNode == null)
                throw new Exception("Could not pack texture to atlas!");
            emptyNode.CurrentToken = token;
            //emptyNode.Texture = tex;
            //emptyNode.DepthTexture = token.DepthTexture;
            emptyNode.Split(tex.Width, tex.Height);
            rect = new Rectangle(emptyNode.X, emptyNode.Y, tex.Width, tex.Height);
            return emptyNode;
        }

        public void Flush()
        {
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            RenderTarget2D texture = new RenderTarget2D(gfx, Size, Size);
            gfx.SetRenderTarget(texture);
            gfx.Clear(Color.Transparent);
            SpriteBatch sb = new SpriteBatch(gfx);
            sb.Begin();
            var nodes = this.RootNode.GetChildren();

            foreach (var node in nodes)
            {
                if (node.Texture.IsNull())
                    continue;
                Vector2 loc = new Vector2(node.Rectangle.X, node.Rectangle.Y);
                sb.Draw(node.Texture, loc, Color.White);
            }
            sb.End();
            gfx.SetRenderTarget(null);
            using (FileStream stream = new FileStream(GlobalVars.SaveDir + "atlas.png", FileMode.OpenOrCreate))
            {
                texture.SaveAsPng(stream, texture.Width, texture.Height);
                stream.Close();
            }


            //int n = 0;
            //using (texture)
            //{
            //    foreach (var node in nodes)
            //    {
            //        if (node.Texture.IsNull())
            //            continue;
            //        Vector2 loc = new Vector2(node.Rectangle.X, node.Rectangle.Y);
            //        sb.Draw(node.Texture, loc, Color.White);

            //        sb.End();
            //        gfx.SetRenderTarget(null);

            //        using (FileStream stream = new FileStream(GlobalVars.SaveDir + n++.ToString("00") + "atlas.png", FileMode.OpenOrCreate))
            //        {
            //            texture.SaveAsPng(stream, texture.Width, texture.Height);
            //            stream.Close();
            //        }
            //        gfx.SetRenderTarget(texture);
            //        sb.Begin();
            //    }
            //    sb.End();
            //    gfx.SetRenderTarget(null);
            //}

           
        }

        List<Node.Token> Sort()
        {
            var sorted = this.ToLoad.ToList();
            sorted.Sort((tex1, tex2) =>
            {
                int volume1 = tex1.Texture.Width * tex1.Texture.Height;
                int volume2 = tex2.Texture.Width * tex2.Texture.Height;
                if (volume1 <= volume2)
                    return 1;// -1;
                else
                    return -1;// 1;
            });
            return sorted;
        }

        internal void OnDeviceLost()
        {
            foreach (var token in this.Tokens.Values)
                token.OnDeviceReset();
            this.Bake();
        }

        internal void Initialize()
        {
            this.RootNode = new Node(0, 0, Size, Size);
            var sorted = this.Sort();
            foreach (var tokens in sorted)
                this.Add(tokens);
            //this.Bake(); // i currently bake the texture when the device resets, and the device resets when the game starts, so it will get baked anyway at the start of the first draw frame
        }

        //int n = 0;
        internal void Bake()
        {
            //n++;
            //n.ToConsole();
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            RenderTarget2D texture = new RenderTarget2D(gfx, Size, Size);
            RenderTarget2D depthtexture = new RenderTarget2D(gfx, Size, Size);

            gfx.SetRenderTarget(texture);
            gfx.Clear(Color.Transparent);
            SpriteBatch sb = new SpriteBatch(gfx);
            sb.Begin();
            var nodes = this.RootNode.GetChildren();

            foreach (var node in nodes)
            {
                //if (node.Texture.IsNull())
                if(node.CurrentToken == null)
                    continue;
                Vector2 loc = new Vector2(node.Rectangle.X, node.Rectangle.Y);
                sb.Draw(node.Texture, loc, Color.White);
                if (node.Texture.IsDisposed)
                    "ASDSAD".ToConsole();
            }
            sb.End();
            gfx.SetRenderTarget(null);
            using (FileStream stream = new FileStream(GlobalVars.SaveDir + this.Name + "Atlas.png", FileMode.OpenOrCreate))
            {
                texture.SaveAsPng(stream, texture.Width, texture.Height);
                stream.Close();
            }

            //bake depth texture
            gfx.SetRenderTarget(depthtexture);
            gfx.Clear(Color.Transparent);
            sb = new SpriteBatch(gfx);
            sb.Begin();
            nodes = this.RootNode.GetChildren();

            foreach (var node in nodes)
            {
                //if (node.DepthTexture.IsNull())
                if (node.CurrentToken == null)
                    continue;
                Vector2 loc = new Vector2(node.Rectangle.X, node.Rectangle.Y);
                sb.Draw(node.DepthTexture, loc, Color.White);
                if (node.DepthTexture.IsDisposed)
                    "ASDSAD".ToConsole();
            }
            sb.End();
            gfx.SetRenderTarget(null);
            using (FileStream stream = new FileStream(GlobalVars.SaveDir + this.Name + "AtlasDepth.png", FileMode.OpenOrCreate))
            {
                depthtexture.SaveAsPng(stream, depthtexture.Width, depthtexture.Height);
                stream.Close();
            }
            this.DepthTexture = depthtexture;
            this.Texture = texture;
        }

        //void texture_ContentLost(object sender, EventArgs e)
        //{
        //    this.Bake();
        //}

        public void Begin(GraphicsDevice gd)
        {
            gd.Textures[0] = this.Texture;
            gd.Textures[1] = this.DepthTexture;
        }

        internal void Begin()
        {
            Game1.Instance.GraphicsDevice.Textures[0] = this.Texture;
            Game1.Instance.GraphicsDevice.Textures[1] = this.DepthTexture;
        }
    }
}
