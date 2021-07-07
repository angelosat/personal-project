using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    public class Nameplate : GroupBox, IContextable//ButtonBase
    {
        //public enum Types { Npc, Prop, Item }
        
        const int VertOffset = 10;
        //static bool _Enabled = true;
        //static public bool Enabled { get { return _Enabled; } set { _Enabled = value; } }

        float HideDelay, HideDelaySpeed;
        readonly float HideDelayMax = Engine.TicksPerSecond * 5;
        public bool AlwaysShow;

        static Dictionary<INameplateable, Nameplate> Plates = new();
        //static Dictionary<Nameplate, int> CachedPlates = new Dictionary<Nameplate, int>();

        Color QualityColor = Color.White;

        static public void Reset()
        {

            ScreenManager.CurrentScreen.WindowManager[LayerTypes.Nameplates].Clear();
         //   PlateManager.Controls.Clear();
        }

        static public Nameplate Create(INameplateable obj)
        {
            var plate = new Nameplate(obj);
            //obj.OnNameplateCreated(plate);
            if (obj is GameObject)
            {
                //if (entity.HasComponent<AttackComponent>())
                //    entity.OnHealthBarCreated(plate);
                //else
                obj.OnNameplateCreated(plate);
            }
            else
                obj.OnNameplateCreated(plate);
            plate.AlignVertically(HorizontalAlignment.Center);
            plate.Tag = obj;
            plate.MouseThrough = false;
            return plate;
        }

        public void OnFocus()
        {
            this.Show();
            this.HideDelay = this.HideDelayMax;
            this.HideDelaySpeed = 0;
        }
        public void OnFocusLost()
        {
            if (this.AlwaysShow)
                return;
            
            this.Hide();
            this.HideDelay = this.HideDelayMax;
            this.HideDelaySpeed = -1;
        }

        static public void Show(GameObject entity)
        {
            var plate = Nameplate.GetNameplate(entity);
            if (plate != null)
                plate.OnFocus();
        }
        static public void Hide(GameObject entity)
        {
            var plate = Nameplate.GetNameplate(entity);
            if (plate != null)
                plate.OnFocusLost();
        }
        public INameplateable Object;
        Nameplate(INameplateable obj)
        {
            this.Layer = LayerTypes.Nameplates;
            this.Object = obj;
            //this.Font = UIManager.FontBold;
            //this.Text = obj.Name;
            this.Active = true;
            QualityColor = obj.GetNameplateColor();
            this.LocationFunc = () =>
                {
                    Camera camera = obj.Map.Camera;
                    Rectangle rect = Object.GetScreenBounds(camera);
                    return new Vector2(rect.X + rect.Width / 2, rect.Y - this.Height - VertOffset) - this.Dimensions * new Vector2(.5f,0);
                };
            /// IF I USE THIS, THE COLLISION DETECTION LOOPS
            //LocationFunc = () =>
            //{
            //    Camera camera = ScreenManager.CurrentScreen.Camera;
            //    Rectangle rect = Object.GetBounds(camera);
            //    return new Vector2(rect.X + rect.Width / 2 - this.Width / 2, rect.Y - this.Height);
            //};
        }

        public override void Update()
        {
            base.Update();
            //return;
            //Camera camera = ScreenManager.CurrentScreen.Camera;
            //Rectangle rect = Object.GetBounds(camera);

            ////Vector2 loc = new Vector2(rect.X + (rect.Width - this.Width) / 2, rect.Y - this.Height - VertOffset);
            ////this.Location = loc;
            ////this.Location = new Vector2(rect.X + rect.Width / 2, rect.Y - this.Height - VertOffset);
            //this.Anchor = new Vector2(.5f, 0);
            //return;
            //if (this.AlwaysShow)
            //    return;
            //if (this.HideDelay > 0)
            //    this.HideDelay += this.HideDelaySpeed;
            //else
            //    this.Hide();
        }

        public override void OnBeforeDraw(SpriteBatch sb, Rectangle viewport)
        {
            if (PlayerOld.Actor != null)
            //if (this.Object == Player.Actor.GetComponent<AttackComponent>().Target)
            {
                var playertarget = PlayerOld.Actor.GetComponent<AttackComponent>().Target;
                if (this.Object is GameObject entity && playertarget != null)
                    if (entity.RefID == playertarget.RefID)
                    {
                        var border = this.BoundsScreen;
                        border.Inflate(1, 1);
                        border.DrawHighlight(sb, Color.Red * 0.5f, Vector2.Zero, 0);
                    }
            }
            //return;
            ////if (Controller.Instance.Mouseover.Object == this.Object)
            //if (Rooms.Ingame.Instance.ToolManager.ActiveTool != null)
            //    if (Rooms.Ingame.Instance.ToolManager.ActiveTool.Target != null)
            //        if (Rooms.Ingame.Instance.ToolManager.ActiveTool.Target.Object == this.Object)
            //            this.BoundsScreen.DrawHighlight(sb, QualityColor * 0.5f, Vector2.Zero, 0);

            ////this.Bounds.DrawHighlight(sb, (Controller.Instance.Mouseover.Object == this.Object ? QualityColor : Color.Black) * 0.5f, Vector2.Zero, 0);
        }

        public override void OnHitTestPass()
        {
            //Controller.Instance.MouseoverNext = new Mouseover() { Object = this.Object, Face = Vector3.Zero };
            Controller.Instance.MouseoverBlockNext.Object = this.Object;// = new Mouseover() { Object = this.Object, Face = Vector3.Zero };

        }

        public override void OnMouseEnter()
        {
            this.BackgroundColor = this.Object.GetNameplateColor() * 0.5f;// this.Object.GetInfo().GetQualityColor() * 0.5f;
            base.OnMouseEnter();
        }

        public override void OnMouseLeave()
        {
            this.BackgroundColor = Color.Black * 0.5f;
            base.OnMouseLeave();
        }

        //public override void OnPaint(SpriteBatch sb)
        //{
        //    UIManager.DrawStringOutlined(sb, this.Text, new Vector2(1, 1), Anchor, this.Object.GetNameplateColor(), TextOutline, Font);// this.Object.GetInfo().GetQualityColor(), Outline, Font);
        //}

        //public override void Draw(SpriteBatch sb)
        //{
        //    if (!Enabled)
        //        if (!InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu))
        //            return;
        //    base.Draw(sb);
        //    DrawHighlight(sb, this.BoundsScreen, (Controller.Instance.Mouseover.Object == this.Object ? QualityColor : Color.Black) * 0.5f, 1);

        //}
        

        private void TryDrawHighlight(SpriteBatch sb)
        {
            if (Rooms.Ingame.Instance.ToolManager.ActiveTool != null)
                if (Rooms.Ingame.Instance.ToolManager.ActiveTool.Target != null)
                    if (Rooms.Ingame.Instance.ToolManager.ActiveTool.Target.Object == this.Object)
                    {
                        this.BoundsScreen.DrawHighlight(sb, QualityColor * 0.5f, Vector2.Zero, 0);
                        return;
                    }
            this.BoundsScreen.DrawHighlight(sb, Color.Black * 0.5f, Vector2.Zero, 0);
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
        static public void UpdatePlates(Camera camera, SceneState scene)
        {
            Update(scene);
            UpdateCollisions(camera);
        }
        static public Nameplate GetNameplate(INameplateable entity)
        {
            //var dic = ScreenManager.Current.WindowManager.Layers[LayerTypes.Nameplates].ToDictionary(foo => (foo as Nameplate).Object, foo => foo as Nameplate);
            //return dic.GetValueOrDefault(entity);
            return Plates.GetValueOrDefault(entity);
        }
        private static void Update(SceneState scene)
        {
            //var dic = ScreenManager.Current.WindowManager.Layers[LayerTypes.Nameplates].ToDictionary(foo => (foo as Nameplate).Object, foo => foo as Nameplate);
            var nextplates = new Dictionary<INameplateable, Nameplate>();// = Plates.ToDictionary(f => f.Key, f => f.Value);
            // create nameplates for any new objects in view
            var sceneEntities = from o in scene.ObjectsDrawn select o as INameplateable;
            foreach (var obj in sceneEntities)
            {
                if (!Plates.TryGetValue(obj, out Nameplate plate))
                {
                    plate = Nameplate.Create(obj);
                    //if (plate.AlwaysShow)
                        plate.Show();
                }
                else
                    Plates.Remove(obj);
                
                nextplates.Add(obj, plate);
            }
            //foreach (var plate in Plates)
            //    plate.Value.Hide();

            Plates = nextplates;
        }

        private static void UpdateOld(SceneState scene)
        {
            //var dic = ScreenManager.Current.WindowManager.Layers[LayerTypes.Nameplates].ToDictionary(foo => (foo as Nameplate).Object, foo => foo as Nameplate);

            // create nameplates for any new objects in view
            var sceneEntities = from o in scene.ObjectsDrawn select o as INameplateable;
            foreach (var obj in sceneEntities)
            {
                //Nameplate plate;
         //       if (!dic.TryGetValue(obj, out plate))

                //if (!dic.Remove(obj))
                //    Nameplate.Create(obj).Show();

                if (!Plates.ContainsKey(obj))
                {
                    var plate = Nameplate.Create(obj);//.Show();
                    Plates.Add(obj, plate);
                    if (plate.AlwaysShow)
                        plate.Show();
                }
            }
            // remove nameplates from objects who aren't in view anymore (which are in the dictionary but arent in the scene object list)
            foreach (var plate in Plates.Values.ToList())
            {
                //if (sceneEntities.Contains(plate.Object))
                if (scene.ObjectsDrawn.Contains(plate.Object))
                    continue;
                Plates.Remove(plate.Object);
                plate.Hide();
            }
            //// remove nameplates from objects who aren't in view anymore (which remained in the dictionary)
            //foreach (var obj in dic)
            //{
            //    Plates.Remove(obj.Key);
            //    obj.Value.Hide();
            //}
        }

        static void UpdateCollisions(Camera camera)
        {
            var handled = new List<Nameplate>();
            float comparer(Nameplate foo) => foo.Object.Global.GetDrawDepth(Engine.Map, camera);// GetDepth(camera); //foo => foo.Location.Y
            IOrderedEnumerable<Nameplate> orderedPlates = ScreenManager.Current.WindowManager.Layers[LayerTypes.Nameplates].Select(s => s as Nameplate).OrderBy(comparer);// Nameplates.Values.OrderBy(comparer);
            var toHandle = new Queue<Nameplate>(orderedPlates);
            while (toHandle.Count > 0)
            {
                Nameplate plate = toHandle.Dequeue();
                foreach (var tocheck in orderedPlates)// foo.Object.Global.GetDepth(camera)))
                {
                    if (tocheck.MouseThrough)
                        continue;
                    if (Collision2(plate, tocheck, camera))
                        toHandle.Enqueue(tocheck);
                }
                    toHandle = new Queue<Nameplate>(toHandle.OrderBy(comparer));// foo.Object.Global.GetDepth(camera))); //-
            }

        }
        static bool Collision2(Nameplate b1, Nameplate toMove, Camera camera)
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
        static bool Collision(Nameplate b1, Nameplate b2, Camera camera)
        {
            if (b1 == b2)
                return false;
            if (!b1.BoundsScreen.Intersects(b2.BoundsScreen))
                return false;
            Rectangle intersection = Rectangle.Intersect(b1.BoundsScreen, b2.BoundsScreen);
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
            //if (b1.Object.Global.GetDepth(camera) > b2.Object.Global.GetDepth(camera))
            if (b1.Object.Global.GetDrawDepth(Engine.Map, camera) > b2.Object.Global.GetDrawDepth(Engine.Map, camera))
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

            back.Location.Y = front.Location.Y - back.BoundsScreen.Height;
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
            if (this.Object is not ITooltippable tooltippable)
                return;
            tooltippable.GetTooltipInfo(tooltip);
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
            IContextable contextable = this.Object as IContextable;
            if (contextable is null)
                return;// throw new NullReferenceException("Nameplate's object is null");
            contextable.GetContextActions(a);
        }

        //static public CheckBox CheckBox
        //{ get { return new CheckBox("Nameplates", Nameplate.Enabled) { LeftClickAction = () => Nameplate.Enabled = !Nameplate.Enabled }; } }

        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Message.Types.NameChanged:
                case Message.Types.StackSizeChanged:
                    var entity = e.Parameters[0] as GameObject;
                    Nameplate plate;
                    if (!Plates.TryGetValue(entity, out plate))
                        break;
                    plate.ClearControls();
                    entity.OnNameplateCreated(plate);
                    break;

                default:
                    break;
            }
        }
        public override Control Invalidate(bool invalidateChildren = false)
        {
            return base.Invalidate(invalidateChildren);
        }
        //public override void DrawOnCamera(SpriteBatch sb, Camera camera)
        //{
        //    base.Draw(sb, UIManager.Bounds);
        //}
        //public override void DrawOnCamera(SpriteBatch sb, Camera camera)
        //{
        //    if (camera.Zoom <= 1) //3
        //        return;
        //    //TryDrawHighlight(sb);
        //    //this.BoundsScreen.DrawHighlightBorder(sb);
        //    base.Draw(sb, UIManager.Bounds);
        //}
        
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            if(UISelectedInfo.IsSelected(this.Object as GameObject))
                this.BoundsScreen.DrawHighlightBorder(sb, thickness:2, padding: 2);
        }
    }
}
