﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Graphics
{
    public partial class AtlasWithDepth
    {
        public class Node
        {
            public class Token : IAtlasNodeToken
            {
                public string Name { get; set; }
                public Node Node { get; set; }
                
                public Texture2D Texture { get; set; }
                public Texture2D DepthTexture { get; set; }
                public Texture2D DepthMask { get; set; }


                public bool Billboard;
                public float BillboardLayer;
                public bool GeneratedDepth;

                public void OnDeviceReset()
                {
                    if (!this.GeneratedDepth)
                        return;
                    if (this.Billboard)
                        this.DepthTexture = Borders.GenerateDepthTexture(this.Texture, this.BillboardLayer);
                    else
                        this.DepthTexture = Borders.GenerateDepthTexture(this.Texture, this.DepthMask);
                }
               

                //Rectangle _Rectangle;// { get; set; }
                //public Rectangle Rectangle
                //{
                //    get { return _Rectangle; }
                //    set
                //    {
                //        this._Rectangle = value;
                //        this.TopLeftUV = new Vector2(value.Left / (float)AtlasWithDepth.Size, value.Top / (float)AtlasWithDepth.Size);
                //        this.TopRightUV = new Vector2(value.Right / (float)AtlasWithDepth.Size, value.Top / (float)AtlasWithDepth.Size);
                //        this.BottomLeftUV = new Vector2(value.Left / (float)AtlasWithDepth.Size, value.Bottom / (float)AtlasWithDepth.Size);
                //        this.BottomRightUV = new Vector2(value.Right / (float)AtlasWithDepth.Size, value.Bottom / (float)AtlasWithDepth.Size);
                //    }
                //}
                public void SetRectangle(Rectangle rect)
                {
                    this.Rectangle = rect;
                    this.TopLeftUV = new Vector2(rect.Left / (float)AtlasWithDepth.Size, rect.Top / (float)AtlasWithDepth.Size);
                    this.TopRightUV = new Vector2(rect.Right / (float)AtlasWithDepth.Size, rect.Top / (float)AtlasWithDepth.Size);
                    this.BottomLeftUV = new Vector2(rect.Left / (float)AtlasWithDepth.Size, rect.Bottom / (float)AtlasWithDepth.Size);
                    this.BottomRightUV = new Vector2(rect.Right / (float)AtlasWithDepth.Size, rect.Bottom / (float)AtlasWithDepth.Size);
                }
                //public Vector2 TopLeftUV, TopRightUV, BottomLeftUV, BottomRightUV;



                public Color[] ColorArray { get; set; }
                public Token(AtlasWithDepth atlas, string name)
                {
                    this.Atlas = atlas;
                    this.Name = name;
                    this.Texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + name);
                    this.GeneratedDepth = true;
                    this.Billboard = true;
                    this.BillboardLayer = 0.5f; // WARNING
                    this.DepthTexture = Borders.GenerateDepthTexture(this.Texture, this.BillboardLayer);
                    this.ColorArray = CreateColorArray(this.Texture);
                }
                public Token(AtlasWithDepth atlas, string name, string depthname)
                {
                    this.Atlas = atlas;
                    this.Name = name;
                    this.Texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + name);
                    this.DepthTexture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + depthname);
                    this.ColorArray = CreateColorArray(this.Texture);
                }
                public Token(AtlasWithDepth atlas, string name, Texture2D depthtexture)
                {
                    this.Atlas = atlas;
                    this.Name = name;
                    //this.Texture = texture;
                    this.Texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + name);
                    this.DepthTexture = Borders.GenerateDepthTexture(this.Texture, depthtexture);
                    this.DepthMask = depthtexture;
                    this.GeneratedDepth = true;
                    this.ColorArray = CreateColorArray(this.Texture);
                }
                public Token(AtlasWithDepth atlas, string name, float depthLayer)
                {
                    this.Atlas = atlas;
                    this.Name = name;
                    this.Texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + name);
                    this.DepthTexture = Borders.GenerateDepthTexture(this.Texture, depthLayer);
                    this.GeneratedDepth = true;
                    this.Billboard = true;
                    this.BillboardLayer = depthLayer;
                    this.ColorArray = CreateColorArray(this.Texture);
                }
                Color[] CreateColorArray(Texture2D tex)
                {
                    Rectangle source = tex.Bounds;
                    Color[] spriteMap = new Color[source.Width * source.Height];
                    tex.GetData(0, source, spriteMap, 0, source.Width * source.Height);
                    return spriteMap;
                }
                public override string ToString()
                {
                    return this.Name;
                }
            }

            //public Texture2D Texture { get; set; }
            //public Texture2D DepthTexture { get; set; }

            public Rectangle Rectangle { get; set; }
            public Token CurrentToken { get; set; }
            public Texture2D Texture { get { return this.CurrentToken.Texture; } }
            public Texture2D DepthTexture { get { return this.CurrentToken.DepthTexture; } }

            public List<Node> Children = new List<Node>(2);
            public Node(Rectangle rectangle)
            {
                this.Rectangle = rectangle;
            }
            public Node(int x, int y, int w, int h)
            {
                this.Rectangle = new Rectangle(x, y, w, h);
            }
            public int Width { get { return this.Rectangle.Width; } }
            public int Height { get { return this.Rectangle.Height; } }
            public int X { get { return this.Rectangle.X; } }
            public int Y { get { return this.Rectangle.Y; } }

            public List<Node> GetChildren()
            {
                List<Node> list = new List<Node>() { this };
                foreach (var child in this.Children)
                    list.AddRange(child.GetChildren());
                return list;
            }

            public Node FindNode(int w, int h)
            {
                //if (this.Texture != null)
                if (this.CurrentToken != null)
                {
                    //var node = this.Children[0].FindNode(w, h) ?this.Children[1].FindNode(w, h);
                    //if (node.IsNull())
                    //    return this.Children[1].FindNode(w, h);
                    return this.Children[0].FindNode(w, h) ?? this.Children[1].FindNode(w, h);
                }               //if(this.Children[0].FindNode(w, h)))
                if (w <= this.Width && h <= this.Height)
                    return this;
                return null;
            }
            public void Split(int w, int h)
            {
                if (w < h)
                {
                    // perform vertical slice
                    Node right = new Node(new Rectangle(this.X + w, this.Y, this.Width - w, this.Height));
                    Node left = new Node(new Rectangle(this.X, this.Y + h, w, this.Height - h));
                    this.Children.Add(right);
                    this.Children.Add(left);
                }
                else
                {
                    // perform horizontal slice
                    Node bottom = new Node(new Rectangle(this.X, this.Y + h, this.Width, this.Height - h));
                    Node top = new Node(new Rectangle(this.X + w, this.Y, this.Width - w, h));
                    this.Children.Add(top);
                    this.Children.Add(bottom);
                }
            }
        }

    }
}
