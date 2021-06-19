using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    class NameplatesContainer : Control
    {
        public override Control Invalidate(bool invalidateChildren = false)
        {
            return this;
        }
        public void UpdateCollisions(IMap map, Camera camera)
        {
            List<Nameplate> handled = new List<Nameplate>();
            Func<Nameplate, float> comparer = foo => foo.Object.Global.GetDrawDepth(foo.Object.Map, camera);
            IOrderedEnumerable<Nameplate> orderedPlates = this.Controls.Select(s => s as Nameplate).OrderBy(comparer);
            Queue<Nameplate> toHandle = new Queue<Nameplate>(orderedPlates);
            while (toHandle.Count > 0)
            {
                Nameplate plate = toHandle.Dequeue();
                foreach (var tocheck in orderedPlates)
                {
                    if (tocheck.MouseThrough)
                        continue;
                    Collision2(plate, tocheck, camera);
                    //if (Collision2(plate, tocheck, camera))
                    //    if(!toHandle.Contains(tocheck))
                    //        toHandle.Enqueue(tocheck);
                }
                //toHandle = new Queue<Nameplate>(toHandle.OrderBy(comparer));
            }

        }
        bool Collision2(Nameplate b1, Nameplate toMove, Camera camera)
        {
            if (b1 == toMove)
                return false;
            if (!b1.BoundsScreen.Intersects(toMove.BoundsScreen))
                return false;

            Vector2 plateCenter = new Vector2(b1.BoundsScreen.Center.X, b1.BoundsScreen.Center.Y);
            Vector2 toMoveCenter = new Vector2(toMove.BoundsScreen.Center.X, toMove.BoundsScreen.Center.Y);
            Vector2 d = toMoveCenter - plateCenter;

            if (Math.Abs(d.X) > Math.Abs(d.Y)) // offset on x axis
            {
                if (d.X > 0)
                    toMove.Location.X = b1.Location.X + b1.BoundsScreen.Width + 1;
                else
                    toMove.Location.X = b1.Location.X - toMove.BoundsScreen.Width - 1;
            }
            else // offset on y axis
            {
                if (d.Y < 0)
                    toMove.Location.Y = b1.Location.Y - toMove.BoundsScreen.Height - 1;
                else
                    toMove.Location.Y = b1.Location.Y + b1.BoundsScreen.Height + 1;
            }
            return true;
        }

        //public override void DrawOnCamera(SpriteBatch sb, Camera camera)
        //{
        //    if (camera.Zoom <= 1)//3)
        //        return;
        //    //TryDrawHighlight(sb);
        //    //this.BoundsScreen.DrawHighlightBorder(sb);
        //    base.Draw(sb, UIManager.Bounds);
        //}
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, UIManager.Bounds);
        }
    }
}
