using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Rooms;
using Start_a_Town_.GameModes;
using Start_a_Town_.Components;

namespace Start_a_Town_.PlayerControl
{
    public class SelectionRectangle
    {
        public Rectangle Rectangle;
        public List<GameObject> Objects;
        public SelectionRectangle(int x, int y)
        {
            this.Rectangle = new Rectangle(x, y, 0, 0);
            this.Objects = new List<GameObject>();
        }
    }
    public class JobToolNew : ControlTool
    {
        //BoundingBox Box;// { get; set; }
        //Rectangle Selection;// { get; set; }
        SelectionRectangle Selection { get; set; }
        bool Dragging { get; set; }
        TownJob Job { get; set; }
        //TownJobStep Step { get; set; }
        Script Script { get; set; }
        
        public JobToolNew(TownJobStep step)
        {
            this.Job = Towns.TownJobEditWindow.Instance.Job;// new TownJob(new TownJobStep[] { step });
            this.Script = Ability.GetScript(step.Script);
            this.Selection = new SelectionRectangle(0, 0);
            this.Dragging = false;
          //  JobCreateWindow.Instance.Initialize(this.Job).Show();
        }
        public JobToolNew(TownJob job)
        {
            this.Job = job;
            this.Selection = new SelectionRectangle(0, 0);
            this.Dragging = false;
            Towns.TownJobEditWindow.Instance.Initialize(this.Job).Show();
        }

        //public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    if (e.Handled)
        //        return Messages.Default;
        //    this.Selection = new SelectionRectangle(e.X, e.Y);
        //    this.Dragging = true;
        //    return Messages.Default;
        //}

        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            // TODO: replace targetold with target
            //if (Controller.Instance.Mouseover.Object != TargetOld)
            //    return Messages.Default;

            //if(InputState.IsKeyDown(System.Windows.Forms.Keys.LControlKey))
            //{
            //    this.Selection.Objects.Remove(this.TargetOld);
            //    return Messages.Default;
            //}

            //this.TargetOld.TryGetComponent<InteractiveComponent>(c =>
            //{
            //    if (c.Abilities.Contains(this.Script.ID))
            //        this.Selection.Objects.Add(this.TargetOld);
            //});

            this.Dragging = false;
            return Messages.Default;
        }

        //public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    return Messages.Remove;
        //}

        //public override void Update(SceneState scene)
        //{
        //    base.Update(scene);
        //    this.Selection.Objects.Clear();
        //    int xmin = Math.Min(this.Selection.Rectangle.Left, this.Selection.Rectangle.Right);
        //    int xmax = Math.Max(this.Selection.Rectangle.Left, this.Selection.Rectangle.Right);
        //    int ymin = Math.Min(this.Selection.Rectangle.Top, this.Selection.Rectangle.Bottom);
        //    int ymax = Math.Max(this.Selection.Rectangle.Top, this.Selection.Rectangle.Bottom);
        //    var rect = new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
        //    foreach (var pair in scene.ObjectBounds)
        //        if (rect.Intersects(pair.Value))
        //            this.Selection.Objects.Add(pair.Key);
        //}
        public override void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //this.Box.Max = new Vector3(e.X, e.Y, 0);
            this.Selection.Rectangle.Width = e.X - this.Selection.Rectangle.X;
            this.Selection.Rectangle.Height = e.Y - this.Selection.Rectangle.Y;
        }

        internal override void DrawWorld(SpriteBatch sb, IMap map, Camera camera)
        {
            base.DrawWorld(sb, map, camera);
            //sb.Draw(Sprite.Default.Texture, UIManager.Mouse, Color.White);
           
            //if (!this.Dragging)
            //    return;
            ////int xmin = (int)Math.Min(this.Box.Min.X, this.Box.Max.X);
            ////int xmax = (int)Math.Max(this.Box.Min.X, this.Box.Max.X);
            ////int ymin = (int)Math.Min(this.Box.Min.Y, this.Box.Max.Y);
            ////int ymax = (int)Math.Max(this.Box.Min.Y, this.Box.Max.Y);
            //int xmin = Math.Min(this.Selection.Rectangle.Left, this.Selection.Rectangle.Right);
            //int xmax = Math.Max(this.Selection.Rectangle.Left, this.Selection.Rectangle.Right);
            //int ymin = Math.Min(this.Selection.Rectangle.Top, this.Selection.Rectangle.Bottom);
            //int ymax = Math.Max(this.Selection.Rectangle.Top, this.Selection.Rectangle.Bottom);
            //var rect = new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
            //rect.DrawHighlight(sb);

            foreach(var obj in this.Selection.Objects)
            {
                //obj.DrawMouseover(sb, camera);
                obj.TryGetComponent<SpriteComponent>(c =>
                {
                    sb.Draw(c.Sprite.Texture, camera.GetScreenPosition(obj.Global), c.Sprite.SourceRects[0][0], new Color(0, 255, 255, 127), 0, c.Sprite.Origin, camera.Zoom, SpriteEffects.None, 0);
                });
            }
        }

        internal override void DrawUI(SpriteBatch sb, Camera camera)
        {
            if (this.Script.IsNull())
                return;
            UIManager.DrawStringOutlined(sb, this.Script.Name, UIManager.Mouse + 16 * Vector2.UnitX);
        }
    }
}
