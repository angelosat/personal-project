using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Graphics
{
    public partial class Atlas
    {
        public class Node
        {
            public class Token : IAtlasNodeToken
            {
                public string Name { get; set; }
                public Node Node { get; set; }
                public Texture2D Texture { get; set; }
                //public Rectangle Rectangle;// { get; set; }

                public Color[] ColorArray { get; set; }


                public Token(Atlas atlas, string path, bool grayscale = false)
                {
                    this.Atlas = atlas;
                    this.Name = path + (grayscale ? "-grayscale" : "");
                    if (Borders.Thickness > 0)
                        this.Texture = Borders.GenerateOutline(Game1.Instance.Content.Load<Texture2D>(path));
                    else
                        this.Texture = Game1.Instance.Content.Load<Texture2D>(path);
                    if (grayscale)
                        this.Texture = this.Texture.ToGrayscale();
                    //this.ColorArray = grayscale ? this.Texture.ToGrayscaleArray() : CreateColorArray(this.Texture);
                    this.ColorArray = CreateColorArray(this.Texture);
                }

                public Token(Atlas atlas, string name, Texture2D texture)
                {
                    this.Atlas = atlas;
                    this.Name = name;
                    this.Texture = texture;
                    this.ColorArray = CreateColorArray(this.Texture);
                }
                Color[] CreateColorArray(Texture2D tex, bool grayscale = false)
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

            public Texture2D Texture { get; set; }
            public Rectangle Rectangle { get; set; }
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
                if (this.Texture != null)
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
