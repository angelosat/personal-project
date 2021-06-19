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
    public partial class Atlas : IAtlas
    {
        public HashSet<Node.Token> ToLoad = new HashSet<Node.Token>();
        //public Node.Token Load(Node.Token token)// Texture2D tex)
        //{
        //    this.ToLoad.Add(token);
        //}
        public string Name { get; protected set; }
        public Atlas(string name)
        {
            this.Name = name;
        }

        Dictionary<string, Node.Token> Tokens = new Dictionary<string, Node.Token>();
        public List<Node.Token> Load(params string[] assetNames)
        {
            List<Node.Token> tokens = new List<Node.Token>();
            foreach (var asset in assetNames)
                tokens.Add(this.Load(asset));
            return tokens;
        }
        public Node.Token Load(string assetName)
        {
            //Node.Token token = new Node.Token(assetName);
            //this.ToLoad.Add(token);
            Node.Token token;
            if (this.Tokens.TryGetValue(assetName, out token))
                return token;
            token = new Node.Token(this, assetName);
            this.Tokens.Add(assetName, token);
            this.ToLoad.Add(token);
            return token;
        }
        public Node.Token Load(string assetName, string depthName)
        {
            //Node.Token token = new Node.Token(assetName);
            //this.ToLoad.Add(token);
            Node.Token token;
            if (this.Tokens.TryGetValue(assetName, out token))
                return token;
            token = new Node.Token(this, assetName);
            this.Tokens.Add(assetName, token);
            this.ToLoad.Add(token);
            return token;
        }
        public Node.Token Load(string assetName, Texture2D texture)
        {
            //Node.Token token = new Node.Token(assetName);
            //this.ToLoad.Add(token);
            Node.Token token;
            if (this.Tokens.TryGetValue(assetName, out token))
            {
                token.Texture = texture;
                return token;
            }
            token = new Node.Token(this, assetName, texture);
            this.Tokens.Add(assetName, token);
            this.ToLoad.Add(token);
            return token;
        }

        public Texture2D Texture { get; set; }

        static int Size = 1024;//512
        int Width, Height;
        List<Node> Nodes = new List<Node>() { new Node(0, 0, Size, Size) };// 2048, 2048) };
        Node RootNode = new Node(0, 0, Size, Size);
        //static public Vector2 Add(string texturePath)
        //{
        //    Texture2D tex = Game1.Instance.Content.Load<Texture2D>(texturePath);
        //    return Add2(tex);
        //}
        public void Add(Node.Token token)
        {
            this.AddTexture(token.Texture, out token.Rectangle);
            return;

            Texture2D tex = token.Texture;
            Node emptyNode = this.RootNode.FindNode(tex.Width, tex.Height); // find first empty node that can fit it
            if (emptyNode.IsNull())
                throw new Exception("Could not pack texture to atlas!");
            emptyNode.Texture = tex;
            
            token.Rectangle = new Rectangle(emptyNode.X, emptyNode.Y, tex.Width, tex.Height);
            emptyNode.Split(tex.Width, tex.Height);
            token.Node = emptyNode;
            //return emptyNode;



            //if (sprite.Width < sprite.Height)
            //{
            //    // perform vertical slice
            //    Node right = new Node(new Rectangle(node.X + sprite.Width, node.Y, node.Width - sprite.Width, node.Height));
            //    Node left = new Node(new Rectangle(node.X, node.Y + sprite.Height, sprite.Width, node.Height - sprite.Height));
            //    node.Children.Add(right);
            //    node.Children.Add(left);
            //}
            //else
            //{
            //    // perform horizontal slice
            //    Node bottom = new Node(new Rectangle(node.X, node.Y + sprite.Height, node.Width, node.Height - sprite.Height));
            //    Node top = new Node(new Rectangle(node.X + sprite.Width, node.Y, node.Width - sprite.Width, sprite.Height));
            //    node.Children.Add(top);
            //    node.Children.Add(bottom);
            //}
           
        }

        public Node AddTexture(Texture2D tex, out Rectangle rect)
        {
            Node emptyNode = this.RootNode.FindNode(tex.Width, tex.Height); // find first empty node that can fit it
            if (emptyNode.IsNull())
                throw new Exception("Could not pack texture to atlas!");
            emptyNode.Texture = tex;
            emptyNode.Split(tex.Width, tex.Height);
            rect = new Rectangle(emptyNode.X, emptyNode.Y, tex.Width, tex.Height);
            return emptyNode;
        }

        public Vector2 Add2(Texture2D sprite)
        {
            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(this.RootNode);
            //Stack<Node> queue = new Stack<Node>();
            //queue.Push(Instance.RootNode);
            while (queue.Count > 0)
            {
                Node node = queue.Dequeue();
                //Node node = queue.Pop();
                if (node.Texture != null)
                {
                    node.Children.ForEach(c => queue.Enqueue(c));
                    //node.Children.ForEach(c => queue.Push(c));
                    continue;
                }

                if (node.Width < sprite.Width || node.Height < sprite.Height)
                //if (!(sprite.Width <= node.Width && sprite.Height <= node.Height))
                {
                    //node.Children.ForEach(c => queue.Enqueue(c));
                    //node.Children.ForEach(c => queue.Push(c));
                    continue;
                }
                node.Texture = sprite;

                //if (sprite.Width < sprite.Height)
                //{
                //    // perform vertical slice
                //    Node right = new Node(new Rectangle(node.X + sprite.Width, node.Y, node.Width - sprite.Width, node.Height));
                //    Node left = new Node(new Rectangle(node.X, node.Y + sprite.Height, sprite.Width, node.Height - sprite.Height));
                   
                //    node.Children.Add(right);
                //    node.Children.Add(left);
                //}
                //else
                //{
                    // perform horizontal slice
                    Node bottom = new Node(new Rectangle(node.X, node.Y + sprite.Height, node.Width, node.Height - sprite.Height));
                    Node top = new Node(new Rectangle(node.X + sprite.Width, node.Y, node.Width - sprite.Width, sprite.Height));
                    node.Children.Add(top);
                    node.Children.Add(bottom);
                    
                //}
                return new Vector2(node.X, node.Y);
            }
            throw new Exception("Could not pack texture to atlas!");

            //foreach (var node in Nodes)
            //{
            //    if (node.Texture != null)
            //        continue;

            //    if (node.Width < sprite.Width || node.Height < sprite.Height)
            //        continue;

            //    node.Texture = sprite;

            //    if (sprite.Width >= sprite.Height)
            //    {
            //        // perform vertical slice
            //        Node right = new Node(new Rectangle(node.X + sprite.Width, node.Y, node.Width - sprite.Width, node.Height));
            //        Node left = new Node(new Rectangle(node.X, node.Y + sprite.Height, sprite.Width, node.Height - sprite.Height));
            //        node.Children.Add(right);
            //        node.Children.Add(left);
            //    }
            //    else
            //    {
            //        // perform horizontal slice
            //        Node bottom = new Node(new Rectangle(node.X, node.Y + sprite.Height, node.Width, node.Height - sprite.Height));
            //        Node top = new Node(new Rectangle(node.X + sprite.Width, node.Y, node.Width - sprite.Width, sprite.Height));
            //        node.Children.Add(bottom);
            //        node.Children.Add(top);
            //    }
            //    return new Vector2(node.X, node.Y);
            //}
            //throw new Exception("Could not fit texture to atlas!");
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

        internal void Initialize()
        {
            this.RootNode = new Node(0, 0, Size, Size);
            var sorted = this.Sort();
            foreach (var tokens in sorted)
                this.Add(tokens);
            this.Bake();
        }

        internal void Bake()
        {
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            //if (this.Texture != null)
            //    (this.Texture as RenderTarget2D).ContentLost -= texture_ContentLost;
            RenderTarget2D texture = new RenderTarget2D(gfx, Size, Size);
            //texture.ContentLost += texture_ContentLost;
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
            this.Texture = texture;
        }

        //void texture_ContentLost(object sender, EventArgs e)
        //{
        //    this.Bake();
        //}
    }
}
