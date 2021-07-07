using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    public class Nameplate : GroupBox, IContextable
    {
        const int VertOffset = 10;
        public bool AlwaysShow;

        static Dictionary<INameplateable, Nameplate> Plates = new();

        static public void Reset()
        {
            ScreenManager.CurrentScreen.WindowManager[LayerTypes.Nameplates].Clear();
        }

        static public Nameplate Create(INameplateable obj)
        {
            var plate = new Nameplate(obj);
            if (obj is GameObject)
            {
                obj.OnNameplateCreated(plate);
            }
            else
                obj.OnNameplateCreated(plate);
            plate.AlignVertically(HorizontalAlignment.Center);
            plate.Tag = obj;
            plate.MouseThrough = false;
            return plate;
        }

        static public void Show(GameObject entity)
        {
            var plate = Nameplate.GetNameplate(entity);
            if (plate is not null)
                plate.OnFocus();
        }
        static public void Hide(GameObject entity)
        {
            var plate = Nameplate.GetNameplate(entity);
            if (plate is not null)
                plate.OnFocusLost();
        }
        public INameplateable Object;
        Nameplate(INameplateable obj)
        {
            this.Layer = LayerTypes.Nameplates;
            this.Object = obj;
            this.Active = true;
            this.LocationFunc = () =>
                {
                    Camera camera = obj.Map.Camera;
                    Rectangle rect = Object.GetScreenBounds(camera);
                    return new Vector2(rect.X + rect.Width / 2, rect.Y - this.Height - VertOffset) - this.Dimensions * new Vector2(.5f,0);
                };
        }

        public override void OnBeforeDraw(SpriteBatch sb, Rectangle viewport)
        {
            if (PlayerOld.Actor is not null)
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
        }
        public override void OnHitTestPass()
        {
            Controller.Instance.MouseoverBlockNext.Object = this.Object;
        }
        public override void OnMouseEnter()
        {
            this.BackgroundColor = this.Object.GetNameplateColor() * 0.5f;
            base.OnMouseEnter();
        }
        public override void OnMouseLeave()
        {
            this.BackgroundColor = Color.Black * 0.5f;
            base.OnMouseLeave();
        }
        public void OnFocus()
        {
            this.Show();
        }
        public void OnFocusLost()
        {
            if (this.AlwaysShow)
                return;

            this.Hide();
        }

        static public Nameplate GetNameplate(INameplateable entity)
        {
            return Plates.GetValueOrDefault(entity);
        }
        private static void Update(SceneState scene)
        {
            var nextplates = new Dictionary<INameplateable, Nameplate>();
            var sceneEntities = from o in scene.ObjectsDrawn select o as INameplateable;
            foreach (var obj in sceneEntities)
            {
                if (!Plates.TryGetValue(obj, out Nameplate plate))
                {
                    plate = Nameplate.Create(obj);
                    plate.Show();
                }
                else
                    Plates.Remove(obj);

                nextplates.Add(obj, plate);
            }

            Plates = nextplates;
        }

        public override void GetTooltipInfo(Tooltip tooltip)
        {
            if (this.Object is not ITooltippable tooltippable)
                return;
            tooltippable.GetTooltipInfo(tooltip);
            tooltip.Tag = this.Object;
        }

        public void GetContextActions(ContextArgs a)
        {
            IContextable contextable = this.Object as IContextable;
            if (contextable is null)
                return;
            contextable.GetContextActions(a);
        }

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
        
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            if(UISelectedInfo.IsSelected(this.Object as GameObject))
                this.BoundsScreen.DrawHighlightBorder(sb, thickness:2, padding: 2);
        }
    }
}
