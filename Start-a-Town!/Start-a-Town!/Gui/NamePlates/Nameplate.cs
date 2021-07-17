﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using UI;

namespace Start_a_Town_.UI
{
    public class Nameplate : GroupBox, IContextable
    {
        const int VertOffset = 10;
        public bool AlwaysShow;

        static Dictionary<INameplateable, Nameplate> Plates = new();

        static public void Reset()
        {
            ScreenManager.CurrentScreen.WindowManager[UIManager.LayerNameplates].Clear();
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
            this.Layer = UIManager.LayerNameplates;
            this.Object = obj;
            this.Active = true;
            this.LocationFunc = () =>
            {
                Camera camera = obj.Map.Camera;
                Rectangle rect = Object.GetScreenBounds(camera);
                return new Vector2(rect.X + rect.Width / 2, rect.Y - this.Height - VertOffset) - this.Dimensions * new Vector2(.5f, 0);
            };
        }

        public override void OnBeforeDraw(SpriteBatch sb, Rectangle viewport)
        {
            var obj = this.Tag as GameObject;
            var actor = obj.Net.GetPlayer()?.ControllingEntity;
            if (actor is null)
                return;
            var playertarget = actor.GetComponent<AttackComponent>().Target;
            if (this.Object is GameObject entity && playertarget != null)
                if (entity.RefID == playertarget.RefID)
                {
                    var border = this.BoundsScreen;
                    border.Inflate(1, 1);
                    border.DrawHighlight(sb, Color.Red * 0.5f, Vector2.Zero, 0);
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

        
        static public Nameplate GetNameplate(INameplateable entity)
        {
            return Plates.GetValueOrDefault(entity);
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

        public override void GetTooltipInfo(Control tooltip)
        {
            if (this.Object is not ITooltippable tooltippable)
                return;
            tooltippable.GetTooltipInfo(tooltip);
            tooltip.Tag = this.Object;
        }

        public void GetContextActions(GameObject playerEntity, ContextArgs a)
        {
            IContextable contextable = this.Object as IContextable;
            if (contextable is null)
                return;
            contextable.GetContextActions(playerEntity, a);
        }

        public override Control Invalidate(bool invalidateChildren = false)
        {
            return base.Invalidate(invalidateChildren);
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            if (UISelectedInfo.IsSelected(this.Object as GameObject))
                this.BoundsScreen.DrawHighlightBorder(sb, thickness: 2, padding: 2);
        }
    }
}
