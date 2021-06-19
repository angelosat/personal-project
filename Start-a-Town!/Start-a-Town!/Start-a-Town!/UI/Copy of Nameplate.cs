using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class Nameplate : Label, IContextable//ButtonBase
    {
        static Dictionary<GameObject, Nameplate> Nameplates = new Dictionary<GameObject, Nameplate>();
        static Queue<GameObject> PlatesToShow = new Queue<GameObject>();

        static bool _Enabled = true;
        static public bool Enabled { get { return _Enabled; } set { _Enabled = value; } }

        Color QualityColor = Color.White;

        static public void Reset()
        {
            Nameplates = new Dictionary<GameObject, Nameplate>();
         //   PlateManager.Controls.Clear();
        }
        static public void Initialize(Chunk chunk)
        {
            foreach (var obj in chunk.GetObjects())
            {
               // Show(obj);
                Toggle(obj, !(bool)obj["Sprite"]["Hidden"]);
            }
        }

        static public void Enqueue(GameObject obj)
        {
            PlatesToShow.Enqueue(obj);
        }

        static Nameplate Show(GameObject obj)
        {
            //Nameplate plate = Nameplates.GetValueOrDefault(obj) ?? new Nameplate(obj);
            Nameplate plate;
            if (!Nameplates.TryGetValue(obj, out plate))
            {
                plate = new Nameplate(obj);
                Nameplates[obj] = plate;
            }
            plate.Show();
            return plate;
        }

        static public bool Hide(GameObject obj)
        {
            Nameplate plate;
            if (!Nameplates.TryGetValue(obj, out plate))
            {
                PlatesToShow = new Queue<GameObject>(PlatesToShow.Where(foo => foo != obj));
                return false;
            }
            Nameplates.Remove(obj);
            plate.Hide();
            return true;
        }

        static public bool Toggle(GameObject obj, bool toggle)
        {
            if (toggle)
                Enqueue(obj);
            else
                Hide(obj);
            return toggle;
        }

        GameObject Object;
        Nameplate(GameObject obj)
        {
          //  this.WindowManager = PlateManager;

            this.Layer = "Nameplates";
            this.Object = obj;
          //  this.TextBackground = Color.Black * 0.5f;
            this.BackgroundColor = Color.Black * 0.5f;
         //   this.FontStyle = System.Drawing.FontStyle.Bold;
            this.Font = UIManager.FontBold;
            this.Text = obj.Name;
            this.Active = true;
            QualityColor = InfoComponent.GetQualityColor(obj);
            //Vector2 textSize = UIManager.FontBold.MeasureString(this.Text);
            //Width = (int)textSize.X + 2;
            //Height = (int)textSize.Y + 2;
        }

        public override void Update()
        {
            //if (ContextDelay > 0)
            //{
            //    if (Pressed)
            //        ContextDelay -= GlobalVars.DeltaTime;
            //    if (ContextDelay < 0)
            //        ContextMenu.Instance.Initialize(Player.Actor, new Mouseover() { Object = this.Object, Face = new Vector3(0, 0, 0) });
            //}

            if (MouseHover)
                Controller.Instance.MouseoverNext = new Mouseover() { Object = this.Object, Face = Vector3.Zero };
            Camera camera = ScreenManager.CurrentScreen.Camera;
            //Sprite sprite;
            //if (!Object.TryGetSprite(out sprite))
            //{
            //    (this.Object.Name + " doesn't have a sprite").ToConsole();
            //    return;
            //}
            Rectangle rect = camera.GetScreenBounds(Object.Global, (Object["Sprite"]["Sprite"] as Sprite).GetBounds());
            Vector2 loc = new Vector2(rect.X + rect.Width / 2 - this.Width / 2, rect.Y - this.Height);
            this.Location = loc;
            base.Update();
        }

        public override void OnMouseEnter()
        {
            this.BackgroundColor = this.Object.GetInfo().GetQualityColor() * 0.5f;
            base.OnMouseEnter();
        }

        public override void OnMouseLeave()
        {
            this.BackgroundColor = Color.Black * 0.5f;
            base.OnMouseLeave();
        }

        public override void OnPaint(SpriteBatch sb)
        {
            UIManager.DrawStringOutlined(sb, this.Text, new Vector2(1, 1), Anchor, this.Object.GetInfo().GetQualityColor(), Outline, Font);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (!Enabled)
                return;
            base.Draw(sb);
            DrawHighlight(sb, this.Bounds, (Controller.Instance.Mouseover.Object == this.Object ? QualityColor : Color.Black) * 0.5f, 1);
          //  Label.DrawText(sb, TextSprite, ScreenLocation, null, Width, QualityColor, Opacity, HorizontalAlignment.Center);

        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            if (!Enabled)
                return;
            base.Draw(sb, viewport);
        }
        //static public void DrawPlates(SpriteBatch sb)
        //{
        //    foreach (var plate in Nameplates)
        //        plate.Value.Draw(sb);
        //}

        //public void Collision(Camera camera)
        //{
        //    foreach (var plate in Nameplates.Values.OrderBy(foo=>foo.Object.Global.GetDepth(camera)))
        //        Collision(this, plate, camera);
        //}
        static public void UpdatePlates(Camera camera)
        {
            while (PlatesToShow.Count > 0)
                Show(PlatesToShow.Dequeue());
            foreach (var plate in Nameplates)
                plate.Value.Update();

            UpdateCollisions(camera);
        }
        static void UpdateCollisions(Camera camera)
        {
            List<Nameplate> handled = new List<Nameplate>();
            Func<Nameplate, float> comparer = foo => foo.Object.Global.GetDepth(camera); //foo => foo.Location.Y
            IOrderedEnumerable<Nameplate> orderedPlates = Nameplates.Values.OrderBy(comparer);
            Queue<Nameplate> toHandle = new Queue<Nameplate>(orderedPlates);
            while (toHandle.Count > 0)
            {
                Nameplate plate = toHandle.Dequeue();
                foreach (var tocheck in orderedPlates)// foo.Object.Global.GetDepth(camera)))
                    if (Collision2(plate, tocheck, camera))
                        toHandle.Enqueue(tocheck);
                toHandle = new Queue<Nameplate>(toHandle.OrderBy(comparer));// foo.Object.Global.GetDepth(camera))); //-
            }

            //foreach (var plate in Nameplates.Values.OrderBy(foo => foo.Object.Global.GetDepth(camera)))
            //{
            //    //plate.Collision(camera);

            //    foreach (Nameplate handledPlate in newList)
            //        Collision2(plate, handledPlate, camera);

            //    newList.Add(plate);
            //}
            

        }
        static bool Collision2(Nameplate b1, Nameplate toMove, Camera camera)
        {
            if (b1 == toMove)
                return false;
            if (!b1.Bounds.Intersects(toMove.Bounds))
                return false;

            Vector2 plateCenter = new Vector2(b1.Bounds.Center.X, b1.Bounds.Center.Y);
            Vector2 toMoveCenter = new Vector2(toMove.Bounds.Center.X, toMove.Bounds.Center.Y);
            Vector2 d = toMoveCenter - plateCenter;

            if (Math.Abs(d.X) > Math.Abs(d.Y)) // offset on x axis
            {
                if (d.X > 0)
                    toMove.Location.X = b1.Location.X + b1.Bounds.Width + 1;
                else
                    toMove.Location.X = b1.Location.X - toMove.Bounds.Width - 1;
            }
            else // offset on y axis
            {
                if (d.Y < 0)
                    toMove.Location.Y = b1.Location.Y - toMove.Bounds.Height - 1;
                else
                    toMove.Location.Y = b1.Location.Y + b1.Bounds.Height + 1;
            }
            return true;
        }
        static bool Collision(Nameplate b1, Nameplate b2, Camera camera)
        {
            if (b1 == b2)
                return false;
            if (!b1.Bounds.Intersects(b2.Bounds))
                return false;
            Rectangle intersection = Rectangle.Intersect(b1.Bounds, b2.Bounds);
            //Vector2 c1 = new Vector2(b1.Bounds.Center.X, b1.Bounds.Center.Y), c2 = new Vector2(b2.Bounds.Center.X, b2.Bounds.Center.Y);
            //Vector2 d = c2 - c1;
            //if (d.X > d.Y)
            //{
            //    if (d.X > 0)
            //        b1.Location.X = b2.Location.X + b2.Bounds.Width;
            //    else
            //        b1.Location.X = b2.Location.X - b1.Bounds.Width;
            //}
            //else
            //{
            //    if (d.Y > 0)
            //        b1.Location.Y = b2.Location.Y - b1.Bounds.Height;
            //    else
            //        b1.Location.Y = b2.Location.Y + b2.Bounds.Height;
            //}

          //  Vector2 frontCenter, backCenter, d;
            Nameplate front, back;
            if (b1.Object.Global.GetDepth(camera) > b2.Object.Global.GetDepth(camera))
            {                
                front = b2;
                back = b1;
            }
            else
            {
                front = b1;
                back = b2;
            }
            //backCenter = new Vector2(back.Bounds.Center.X, back.Bounds.Center.Y);
            //frontCenter = new Vector2(front.Bounds.Center.X, front.Bounds.Center.Y);
            //d = backCenter - frontCenter;
            //if (Math.Abs(d.X) > Math.Abs(d.Y)) // offset on x axis
            //{
            //    if (d.X > 0)
            //        back.Location.X = front.Location.X + front.Bounds.Width + 1;
            //    else
            //        back.Location.X = front.Location.X - back.Bounds.Width - 1;
            //}
            //else
            //{
            //    if (d.Y > 0)
            //        back.Location.Y = front.Location.Y - back.Bounds.Height;
            //    else
            //        back.Location.Y = front.Location.Y + front.Bounds.Height;
            //}

            back.Location.Y = front.Location.Y - back.Bounds.Height;
            return true;
            //if (b1.Object.Global.GetDepth(camera) > b2.Object.Global.GetDepth(camera))
            //    b1.Location.Y = b2.Location.Y - b1.Bounds.Height;
            //else
            //    b2.Location.Y = b1.Location.Y - b2.Bounds.Height;
        }

        //public override void OnMouseEnter()
        //{
        //    this.Color = Color.Red;
        //}
        //public override void OnMouseLeave()
        //{
        //    this.Color = Color.White;
        //}


        public override void GetTooltipInfo(Tooltip tooltip)
        {
            Object.GetTooltipInfo(tooltip);
            tooltip.Tag = this.Object;
            //if (Object == Player.Actor)
            //    return;
            //tooltip.Controls.Add(new Label(tooltip.Controls.Last().BottomLeft, "Hold [" + GlobalVars.KeyBindings.Interact.ToString() + "] for \navailable interactions"));
        }

        //public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    if(ContextDelay>0)
        //        Start_a_Town_.Control.InteractionTool.RepeatInteraction(Player.Actor, Object);
        //  //  ContextDelay = 0;
        //    Pressed = false;
        //    //base.HandleRButtonUp(e);
        //    //if (Player.Actor != null)
        //    //    ContextMenu.Instance.Initialize(Player.Actor, new Mouseover() { Object = this.Object, Face = new Vector3(0, 0, 0) });// this.Object);
        //}

        //public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    if (e.Handled)
        //        return;
        //    ContextDelay = ContextDelayMax;
        //    Pressed = true;
        //    base.HandleRButtonDown(e);
        //}

        //public IEnumerable<ContextAction> GetContextActions(params object[] p)
        //{
        //    if (this.Object.IsNull())
        //        throw new NullReferenceException("Nameplate's object is null");
        //    return this.Object.GetContextActions(p);
        //}

        public void GetContextActions(ContextArgs a)
        {
            if (this.Object.IsNull())
                throw new NullReferenceException("Nameplate's object is null");
            this.Object.GetContextActions(a);
        }
    }
}
