using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace Start_a_Town_
{
    static public class RectangleHelper
    {
        static public Rectangle FindBestUncoveredRectangle(this IBoundedCollection container, int w, int h)
        {
            //int w = (int)dimensions.X, h = (int)dimensions.Y;
            var freeRects = GetFreeRectangles(container);
            Rectangle bestRect = new Rectangle(0, 0, w, h);
            var minSize = int.MaxValue;
            var size = w * h;
            var ordered = freeRects.OrderBy(r => new Vector2(r.X, r.Y).LengthSquared()).ToList();
            foreach (var rect in ordered)
            {
                if (w <= rect.Width && h <= rect.Height)
                    return rect;
                //else
                //    continue;

                //var rectsize = rect.Width * rect.Height;
                //if (rectsize < size)
                //    continue;
                //if (rectsize > minSize)
                //    continue;
                //minSize = rectsize;
                //bestRect = rect;
            }
            return bestRect;
        }
        static List<Rectangle> GetFreeRectangles(IBoundedCollection container)
        {
            List<Rectangle> freeRects = new List<Rectangle>() { container.ContainerSize };
            var divided = true;
            var windows = container.Children;// this.Layers[LayerTypes.Windows].Where(c => c is Window).ToList();
            windows.Reverse();
            while (divided)
            {
                divided = false;

                foreach (var control in windows) // TODO: fix HUD being a control itself
                {
                    foreach (var rect in freeRects.ToList())
                    {
                        var bounds = control.Bounds;
                        if (rect.Intersects(bounds))
                        {
                            freeRects.Remove(rect);
                            var divisions = DivideScreenQuad(rect, bounds);
                            foreach (var div in divisions)
                                if (!freeRects.Contains(div))
                                    freeRects.Add(div);
                        }
                    }
                }
            }
            return freeRects;
        }
        static List<Rectangle> DivideScreenQuad(Rectangle screen, Rectangle rect)
        {
            var right = rect.X + rect.Width;
            var bottom = rect.Y + rect.Height;
            var dx = screen.X + screen.Width - right;
            var dy = screen.Y + screen.Height - bottom;

            //var list = new List<Rectangle>(){
            //    new Rectangle(screen.X, screen.Y, screen.Width, rect.Y),
            //    new Rectangle(screen.X, bottom, screen.Width, dy),
            //    new Rectangle(screen.X, screen.Y, rect.X, screen.Height),
            //    new Rectangle(right, screen.Y, dx, screen.Height)
            //};
            var list = new List<Rectangle>();
            if (rect.Y > screen.Y)
                list.Add(new Rectangle(screen.X, screen.Y, screen.Width, rect.Y - screen.Y));
            if (bottom < screen.Y + screen.Height)
                list.Add(new Rectangle(screen.X, bottom, screen.Width, dy));
            if (rect.X > screen.X)
                //list.Add(new Rectangle(screen.X, screen.Y, rect.X, screen.Height));
                list.Add(new Rectangle(screen.X, screen.Y, rect.X - screen.X, screen.Height));
            if (right < screen.X + screen.Width)
                list.Add(new Rectangle(right, screen.Y, dx, screen.Height));

            return list;
        }
    }
}
