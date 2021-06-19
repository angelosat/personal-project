﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public enum BorderStyle { None, Window, Panel, Tooltip, SpeechBubble }

    public class BackgroundStyle
    {
        public string Name;
        public Rectangle
                TopLeft,
                TopRight,
                BottomLeft,
                BottomRight,
                Top,
                Left,
                Right,
                Bottom,
                Center;
        public Color Color;
        public Texture2D SpriteSheet;
        public int Border;

        public override string ToString()
        {
            return Name;
        }

        //public BackgroundStyle()
        //{
        //}
        //public BackgroundStyle(BorderStyle style)
        //{
        //    switch (style)
        //    {
        //        case BorderStyle.Window:
        //            TopLeft = new Rectangle(0, 0, 11, 11);
        //            TopRight = new Rectangle(12, 0, 11, 11);
        //            BottomLeft = new Rectangle(0, 12, 11, 11);
        //            BottomRight = new Rectangle(12, 12, 11, 11);
        //            Top = new Rectangle(12, 0, 1, 11);
        //            Left = new Rectangle(0, 12, 11, 1);
        //            Right = new Rectangle(12, 11, 11, 1);
        //            Bottom = new Rectangle(11, 12, 1, 11);
        //            Center = new Rectangle(11, 11, 1, 1);
        //            Color = UIManager.uiTint;
        //            break;
        //        case BorderStyle.Panel:
        //            TopLeft = new Rectangle(0, 0, 19, 19);
        //            TopRight = new Rectangle(19, 0, 19, 19);
        //            BottomLeft = new Rectangle(0, 19, 19, 19);
        //            BottomRight = new Rectangle(19, 19, 19, 19);
        //            Top = new Rectangle(19, 0, 1, 19);
        //            Left = new Rectangle(0, 19, 19, 1);
        //            Right = new Rectangle(19, 19, 19, 1);
        //            Bottom = new Rectangle(19, 19, 1, 19);
        //            Center = new Rectangle(18, 18, 1, 1);
        //            Color = UIManager.uiTint;
        //            break;
        //        case BorderStyle.SpeechBubble:
        //            TopLeft = new Rectangle(0, 0, 11, 11);
        //            TopRight = new Rectangle(12, 0, 11, 11);
        //            BottomLeft = new Rectangle(0, 12, 11, 11);
        //            BottomRight = new Rectangle(12, 12, 11, 11);
        //            Top = new Rectangle(12, 0, 1, 11);
        //            Left = new Rectangle(0, 12, 11, 1);
        //            Right = new Rectangle(12, 11, 11, 1);
        //            Bottom = new Rectangle(11, 12, 1, 11);
        //            Center = new Rectangle(11, 11, 1, 1);
        //            Color = Color.White;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //public static BackgroundStyle Window
        //{
        //    get
        //    {
        //        BackgroundStyle regions = new BackgroundStyle();
        //        regions.TopLeft = new Rectangle(0, 0, 11, 11);
        //        regions.TopRight = new Rectangle(12, 0, 11, 11);
        //        regions.BottomLeft = new Rectangle(0, 12, 11, 11);
        //        regions.BottomRight = new Rectangle(12, 12, 11, 11);
        //        regions.Top = new Rectangle(12, 0, 1, 11);
        //        regions.Left = new Rectangle(0, 12, 11, 1);
        //        regions.Right = new Rectangle(12, 11, 11, 1);
        //        regions.Bottom = new Rectangle(11, 12, 1, 11);
        //        regions.Center = new Rectangle(11, 11, 1, 1);
        //        regions.Color = Color.White;
        //        regions.SpriteSheet = UIManager.frameSprite;
        //        regions.Border = 11;// 5;// 11;
        //        regions.Name = "Window";
        //        return regions;
        //    }
        //}
        public static readonly BackgroundStyle Window = new BackgroundStyle()
        {
            TopLeft = new Rectangle(0, 0, 11, 11),
            TopRight = new Rectangle(12, 0, 11, 11),
            BottomLeft = new Rectangle(0, 12, 11, 11),
            BottomRight = new Rectangle(12, 12, 11, 11),
            Top = new Rectangle(12, 0, 1, 11),
            Left = new Rectangle(0, 12, 11, 1),
            Right = new Rectangle(12, 11, 11, 1),
            Bottom = new Rectangle(11, 12, 1, 11),
            Center = new Rectangle(11, 11, 1, 1),
            Color = Color.White,
            SpriteSheet = UIManager.frameSprite,
            Border = 11,// 5;// 11;
            Name = "Window"
        };
        //public static readonly BackgroundStyle Window = new BackgroundStyle()
        //{
        //    TopLeft = new Rectangle(0, 0, 11, 11),
        //    TopRight = new Rectangle(12, 0, 11, 11),
        //    BottomLeft = new Rectangle(0, 12, 11, 11),
        //    BottomRight = new Rectangle(12, 12, 11, 11),
        //    Top = new Rectangle(12, 0, 1, 11),
        //    Left = new Rectangle(0, 12, 11, 1),
        //    Right = new Rectangle(12, 11, 11, 1),
        //    Bottom = new Rectangle(11, 12, 1, 11),
        //    Center = new Rectangle(11, 11, 1, 1),
        //    Color = Color.White,
        //    SpriteSheet = UIManager.frameSprite,
        //    Border = 11,
        //    Name = "Window"
        //};

        //public static BackgroundStyle Tooltip
        //{
        //    get
        //    {
        //        BackgroundStyle regions = new BackgroundStyle();
        //        regions.TopLeft = new Rectangle(0, 0, 11, 11);
        //        regions.TopRight = new Rectangle(12, 0, 11, 11);
        //        regions.BottomLeft = new Rectangle(0, 12, 11, 11);
        //        regions.BottomRight = new Rectangle(12, 12, 11, 11);
        //        regions.Top = new Rectangle(12, 0, 1, 11);
        //        regions.Left = new Rectangle(0, 12, 11, 1);
        //        regions.Right = new Rectangle(12, 11, 11, 1);
        //        regions.Bottom = new Rectangle(11, 12, 1, 11);
        //        regions.Center = new Rectangle(11, 11, 1, 1);
        //        regions.Color = Color.Black;
        //        regions.SpriteSheet = UIManager.frameSprite;
        //        regions.Border = 11;
        //        regions.Name = "Tooltip";
        //        return regions;
        //    }
        //}
        public static BackgroundStyle Tooltip = new BackgroundStyle()
        {
            TopLeft = new Rectangle(0, 0, 11, 11),
            TopRight = new Rectangle(12, 0, 11, 11),
            BottomLeft = new Rectangle(0, 12, 11, 11),
            BottomRight = new Rectangle(12, 12, 11, 11),
            Top = new Rectangle(12, 0, 1, 11),
            Left = new Rectangle(0, 12, 11, 1),
            Right = new Rectangle(12, 11, 11, 1),
            Bottom = new Rectangle(11, 12, 1, 11),
            Center = new Rectangle(11, 11, 1, 1),
            Color = Color.Black,
            SpriteSheet = UIManager.frameSprite,
            Border = 11,
            Name = "Tooltip"
        };
        //public static BackgroundStyle LargeButton
        //{
        //    get
        //    {
        //        BackgroundStyle regions = new BackgroundStyle();
        //        regions.Left = new Rectangle(0, 0, 6, 45);
        //        regions.Right = new Rectangle(7, 0, 6, 45);
        //        regions.BottomLeft = new Rectangle(0, 0, 0, 0);
        //        regions.BottomRight = new Rectangle(0, 0, 0, 0);
        //        regions.Top = new Rectangle(0, 0, 0, 0);
        //        regions.TopLeft = new Rectangle(0, 0, 0, 0);
        //        regions.TopRight = new Rectangle(0, 0, 0, 0);
        //        regions.Bottom = new Rectangle(0, 0, 0, 0);
        //        regions.Center = new Rectangle(6, 0, 1, 45);
        //        regions.Color = Color.White;
        //        regions.SpriteSheet = UIManager.LargeButton;
        //        regions.Name = "LargeButton";
        //        return regions;
        //    }
        //}

        public static BackgroundStyle LargeButton = new BackgroundStyle()
        {
            Left = new Rectangle(0, 0, 6, 45),
            Right = new Rectangle(7, 0, 6, 45),
            BottomLeft = new Rectangle(0, 0, 0, 0),
            BottomRight = new Rectangle(0, 0, 0, 0),
            Top = new Rectangle(0, 0, 0, 0),
            TopLeft = new Rectangle(0, 0, 0, 0),
            TopRight = new Rectangle(0, 0, 0, 0),
            Bottom = new Rectangle(0, 0, 0, 0),
            Center = new Rectangle(6, 0, 1, 45),
            Color = Color.White,
            SpriteSheet = UIManager.LargeButton,
            Name = "LargeButton"
        };

        //public static BackgroundStyle Panel
        //{
        //    get
        //    {
        //        BackgroundStyle regions = new BackgroundStyle();
        //        //regions.TopLeft = new Rectangle(0, 0, 19, 19);
        //        //regions.TopRight = new Rectangle(19, 0, 19, 19);
        //        //regions.BottomLeft = new Rectangle(0, 19, 19, 19);
        //        //regions.BottomRight = new Rectangle(19, 19, 19, 19);
        //        //regions.Top = new Rectangle(19, 0, 1, 19);
        //        //regions.Left = new Rectangle(0, 19, 19, 1);
        //        //regions.Right = new Rectangle(19, 19, 19, 1);
        //        //regions.Bottom = new Rectangle(19, 19, 1, 19);
        //        //regions.Center = new Rectangle(18, 18, 1, 1);
        //        regions.TopLeft = new Rectangle(0, 0, 19, 19);
        //        regions.TopRight = new Rectangle(19, 0, 19, 19);
        //        regions.BottomLeft = new Rectangle(0, 19, 19, 19);
        //        regions.BottomRight = new Rectangle(19, 19, 19, 19);
        //        regions.Top = new Rectangle(19, 0, 1, 19);
        //        regions.Left = new Rectangle(0, 19, 19, 1);
        //        regions.Right = new Rectangle(19, 19, 19, 1);
        //        regions.Bottom = new Rectangle(19, 19, 1, 19);
        //        regions.Center = new Rectangle(18, 18, 1, 1);
        //        regions.Color = Color.White;
        //        regions.SpriteSheet = UIManager.SlotSprite;
        //        regions.Border = 5;
        //        regions.Name = "Panel";
        //        return regions;
        //    }
        //}

        public static readonly BackgroundStyle Panel =  new BackgroundStyle()
        {
                TopLeft = new Rectangle(0, 0, 19, 19),
                TopRight = new Rectangle(19, 0, 19, 19),
                BottomLeft = new Rectangle(0, 19, 19, 19),
                BottomRight = new Rectangle(19, 19, 19, 19),
                Top = new Rectangle(19, 0, 1, 19),
                Left = new Rectangle(0, 19, 19, 1),
                Right = new Rectangle(19, 19, 19, 1),
                Bottom = new Rectangle(19, 19, 1, 19),
                Center = new Rectangle(18, 18, 1, 1),
                Color = Color.White,
                SpriteSheet = UIManager.SlotSprite,
                Border = 5,
                Name = "Panel"
        };

        //public static BackgroundStyle TickBox
        //{
        //    get
        //    {
        //        BackgroundStyle regions = new BackgroundStyle();
        //        int corner = 11;
        //        regions.Border = 5;
        //        regions.TopLeft = new Rectangle(0, 0, corner, corner);
        //        regions.TopRight = new Rectangle(corner, 0, corner, corner);
        //        regions.BottomLeft = new Rectangle(0, corner, corner, corner);
        //        regions.BottomRight = new Rectangle(corner, corner, corner, corner);
        //        regions.Top = new Rectangle(corner, 0, 1, corner);
        //        regions.Left = new Rectangle(0, corner, corner, 1);
        //        regions.Right = new Rectangle(corner, corner, corner, 1);
        //        regions.Bottom = new Rectangle(corner, corner, 1, corner);
        //        regions.Center = new Rectangle(corner - 1, corner - 1, 1, 1);
        //        regions.Color = Color.White;
        //        regions.SpriteSheet = UIManager.TextureTickBox;
        //        regions.Name = "TickBox";
        //        return regions;
        //    }
        //}


        const int TickBoxCorner = 11;
        public static readonly BackgroundStyle TickBox = new BackgroundStyle()
        {
            Border = 5,
            TopLeft = new Rectangle(0, 0, TickBoxCorner, TickBoxCorner),
            TopRight = new Rectangle(TickBoxCorner, 0, TickBoxCorner, TickBoxCorner),
            BottomLeft = new Rectangle(0, TickBoxCorner, TickBoxCorner, TickBoxCorner),
            BottomRight = new Rectangle(TickBoxCorner, TickBoxCorner, TickBoxCorner, TickBoxCorner),
            Top = new Rectangle(TickBoxCorner, 0, 1, TickBoxCorner),
            Left = new Rectangle(0, TickBoxCorner, TickBoxCorner, 1),
            Right = new Rectangle(TickBoxCorner, TickBoxCorner, TickBoxCorner, 1),
            Bottom = new Rectangle(TickBoxCorner, TickBoxCorner, 1, TickBoxCorner),
            Center = new Rectangle(TickBoxCorner - 1, TickBoxCorner - 1, 1, 1),
            Color = Color.White,
            SpriteSheet = UIManager.TextureTickBox,
            Name = "TickBox"
        };


        public void Draw(SpriteBatch sb, int width, int height, Color color)
        {
            sb.Draw(SpriteSheet, new Vector2(0), TopLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(width - 11, 0), TopRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(0, height - 11), BottomLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(width - 11, height - 11), BottomRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

            //top, left, right, bottom
            sb.Draw(SpriteSheet, new Rectangle(11, 0, width - 22, 11), Top, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(0, 11, 11, height - 22), Left, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(width - 11, 11, 11, height - 22), Right, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(11, height - 11, width - 22, 11), Bottom, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

            //center
            sb.Draw(SpriteSheet, new Rectangle(11, 11, width - 22, height - 22), Center, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        }

        //public void Draw(SpriteBatch sb, Rectangle bounds)
        //{
        //    sb.Draw(SpriteSheet, new Vector2(bounds.X, bounds.Y), TopLeft, Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
        //    sb.Draw(SpriteSheet, new Vector2(bounds.X + bounds.Width - 11, bounds.Y), TopRight, Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
        //    sb.Draw(SpriteSheet, new Vector2(bounds.X, bounds.Y + bounds.Height - 11), BottomLeft, Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
        //    sb.Draw(SpriteSheet, new Vector2(bounds.X + bounds.Width - 11, bounds.Y + bounds.Height - 11), BottomRight, Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

        //    //top, left, right, bottom
        //    sb.Draw(SpriteSheet, new Rectangle(bounds.X + 11, bounds.Y, bounds.Width - 22, 11), Top, Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        //    sb.Draw(SpriteSheet, new Rectangle(bounds.X, bounds.Y + 11, 11, bounds.Height - 22), Left, Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        //    sb.Draw(SpriteSheet, new Rectangle(bounds.X + bounds.Width - 11, bounds.Y + 11, 11, bounds.Height - 22), Right, Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        //    sb.Draw(SpriteSheet, new Rectangle(bounds.X + 11, bounds.Y + bounds.Height - 11, bounds.Width - 22, 11), Bottom, Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

        //    //center
        //    sb.Draw(SpriteSheet, new Rectangle(bounds.X + 11, bounds.Y + 11, bounds.Width - 22, bounds.Height - 22), Center, Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        //}

        public void Draw(SpriteBatch sb, Rectangle bounds, Color color, SpriteEffects sprFx = SpriteEffects.None)
        {
            sb.Draw(SpriteSheet, new Vector2(bounds.X, bounds.Y), TopLeft, color, 0, new Vector2(0, 0), 1, sprFx, 0);//.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X + bounds.Width - TopRight.Width, bounds.Y), TopRight, color, 0, new Vector2(0, 0), 1, sprFx, 0);// 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X, bounds.Y + bounds.Height - BottomLeft.Height), BottomLeft, color, 0, new Vector2(0, 0), 1, sprFx, 0);// 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X + bounds.Width - BottomRight.Height, bounds.Y + bounds.Height - BottomLeft.Height), BottomRight, color, 0, new Vector2(0, 0), 1, sprFx, 0);// 0.01f);

            //top, left, right, bottom
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y, bounds.Width - Left.Width - Right.Width, Top.Height), Top, color, 0, new Vector2(0, 0), sprFx, 0);// 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X, bounds.Y + Top.Height, Left.Width, bounds.Height - Top.Height - Bottom.Height), Left, color, 0, new Vector2(0, 0), sprFx, 0);// 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + bounds.Width - Right.Width, bounds.Y + Top.Height, Right.Width, bounds.Height - Top.Height - Bottom.Height), Right, color, 0, new Vector2(0, 0), sprFx, 0);// 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y + bounds.Height - Bottom.Height, bounds.Width - Left.Width - Right.Width, Bottom.Height), Bottom, color, 0, new Vector2(0, 0), sprFx, 0);// 0.01f);

            //center
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y + Top.Height, bounds.Width - Left.Width - Right.Width, bounds.Height - Top.Height - Bottom.Height), Center, color, 0, new Vector2(0, 0), sprFx, 0);// 0.01f);
        }

        public void Draw(SpriteBatch sb, Rectangle bounds, float opacity = 1)
        {
            Color color = Color.Lerp(Color.Transparent, Color, opacity);

            sb.Draw(SpriteSheet, new Vector2(bounds.X, bounds.Y), TopLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X + bounds.Width - TopRight.Width, bounds.Y), TopRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X, bounds.Y + bounds.Height - BottomLeft.Height), BottomLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X + bounds.Width - BottomRight.Height, bounds.Y + bounds.Height - BottomLeft.Height), BottomRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

            //top, left, right, bottom
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y, bounds.Width - Left.Width - Right.Width, Top.Height), Top, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X, bounds.Y + Top.Height, Left.Width, bounds.Height - Top.Height - Bottom.Height), Left, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + bounds.Width - Right.Width, bounds.Y + Top.Height, Right.Width, bounds.Height - Top.Height - Bottom.Height), Right, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y + bounds.Height - Bottom.Height, bounds.Width - Left.Width - Right.Width, Bottom.Height), Bottom, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

            //center
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y + Top.Height, bounds.Width - Left.Width - Right.Width, bounds.Height - Top.Height - Bottom.Height), Center, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        }


        public void Draw(SpriteBatch sb, Rectangle bounds, Color tint, float opacity)
        {
            Color color = Color.Lerp(Color.Transparent, Color.Lerp(Color , tint,0.1f), opacity);

            sb.Draw(SpriteSheet, new Vector2(bounds.X, bounds.Y), TopLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X + bounds.Width - TopRight.Width, bounds.Y), TopRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X, bounds.Y + bounds.Height - BottomLeft.Height), BottomLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X + bounds.Width - BottomRight.Height, bounds.Y + bounds.Height - BottomLeft.Height), BottomRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

            //top, left, right, bottom
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y, bounds.Width - Left.Width - Right.Width, Top.Height), Top, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X, bounds.Y + Top.Height, Left.Width, bounds.Height - Top.Height - Bottom.Height), Left, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + bounds.Width - Right.Width, bounds.Y + Top.Height, Right.Width, bounds.Height - Top.Height - Bottom.Height), Right, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y + bounds.Height - Bottom.Height, bounds.Width - Left.Width - Right.Width, Bottom.Height), Bottom, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

            //center
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y + Top.Height, bounds.Width - Left.Width - Right.Width, bounds.Height - Top.Height - Bottom.Height), Center, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        }
        

    }


}
