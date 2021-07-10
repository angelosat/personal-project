﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using System.Windows.Forms;
using UI;

namespace Start_a_Town_.Rooms
{
    public abstract class GameScreen : IDisposable, IKeyEventHandler
    {
        public float LoadingPercentage;
        public SpriteBatch spriteBatch;
        public UIManager WindowManager;
        public List<Timer2> Timers;
        public ToolManager ToolManager;
        public virtual Camera Camera
        {
            get { return Net.Client.Instance.Map.Camera; }
            set { }
        }
        
        public event InputEvent MouseMove, MouseLeftPress, MouseRightPress, MouseLeftUp, MouseLeftDown;
        

        public GameScreen()
        {
            this.WindowManager = new UIManager();
            KeyHandlers = new Stack<IKeyEventHandler>();
            Timers = new List<Timer2>();
        }

        public virtual void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            foreach (IKeyEventHandler handler in KeyHandlers.ToList())
                handler.HandleKeyDown(e);
        }

        public virtual void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            foreach (IKeyEventHandler handler in KeyHandlers.ToList())
                if (!e.Handled)
                    handler.HandleKeyPress(e);
        }

        public virtual void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            foreach (IKeyEventHandler handler in KeyHandlers.ToList())
                handler.HandleKeyUp(e);
        }
        public virtual void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (IKeyEventHandler handler in KeyHandlers.ToList())
                handler.HandleMouseMove(e);
        }
        public virtual void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (IKeyEventHandler handler in KeyHandlers.ToList())
                handler.HandleLButtonDown(e);
        }
        public virtual void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (IKeyEventHandler handler in KeyHandlers.ToList())
                handler.HandleLButtonUp(e);
        }
        public virtual void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (IKeyEventHandler handler in KeyHandlers.ToList())
                handler.HandleRButtonDown(e);
        }
        public virtual void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (IKeyEventHandler handler in KeyHandlers.ToList())
                handler.HandleRButtonUp(e);
        }
        public virtual void HandleMiddleUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (IKeyEventHandler handler in KeyHandlers.ToList())
                handler.HandleMiddleUp(e);
        }
        public virtual void HandleMiddleDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (IKeyEventHandler handler in KeyHandlers.ToList())
                handler.HandleMiddleDown(e);
        }
        
        public virtual void HandleMouseWheel(HandledMouseEventArgs e)
        {
            foreach (IKeyEventHandler handler in KeyHandlers.ToList())
                if (!e.Handled)
                    handler.HandleMouseWheel(e);
        }
        public void HandleLButtonDoubleClick(HandledMouseEventArgs e)
        {
            foreach (IKeyEventHandler handler in KeyHandlers.ToList())
                if (!e.Handled)
                    handler.HandleLButtonDoubleClick(e);
        }

        public event EventHandler<KeyPressEventArgs2> KeyPress;
        void OnKeyPress(object sender, KeyPressEventArgs2 e)
        {
            if (KeyPress != null)
                KeyPress(sender, e);
        }
        void OnMouseMove()
        {
            if (MouseMove != null)
                MouseMove();
        }
        void OnMouseLeftPress()
        {
            if (MouseLeftPress != null)
                MouseLeftPress();
        }
        void OnMouseRightPress()
        {
            if (MouseRightPress != null)
                MouseRightPress();
        }
        void OnMouseLeftUp()
        {
            if (MouseLeftUp != null)
                MouseLeftUp();
        }
        void OnMouseLeftDown()
        {
            if (MouseLeftDown != null)
                MouseLeftDown();
        }


        void Controller_KeyPress(object sender, KeyPressEventArgs2 e)
        {
            OnKeyPress(sender, e);
        }

        virtual public GameScreen Initialize(IObjectProvider net)
        {
            return this;
        }
        virtual public void Update(Game1 game, GameTime gt)
        {
            List<Timer2> timers = Timers.ToList();
            foreach (Timer2 timer in timers)
                timer.Update(gt);
        }
        abstract public void Draw(SpriteBatch sb);


        public virtual void Dispose()
        {
            GC.Collect();
        }

        public Stack<IKeyEventHandler> KeyHandlers;

        internal virtual void OnGameEvent(GameEvent e)
        {
            this.WindowManager.OnGameEvent(e);
        }
    }
}