﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.PlayerControl;
using System.Windows.Forms;

namespace Start_a_Town_.Rooms
{
    public abstract class GameScreen : IDisposable, IKeyEventHandler
    {
        public float LoadingPercentage;
        public SpriteBatch spriteBatch;
        public UIManager WindowManager;
        public List<Timer2> Timers;
        public ToolManager ToolManager;
        public Camera Camera;
        public event InputEvent MouseMove, MouseLeftPress, MouseRightPress, MouseLeftUp, MouseLeftDown;
        //public event EventHandler<KeyEventArgs> KeyPress;
        //void OnKeyPress(object sender, KeyEventArgs e)
        //{
        //    if (KeyPress != null)
        //        KeyPress(sender, e);
        //}

        public GameScreen()
        {
            this.WindowManager = new UIManager();
           // InputHandlers = new Stack<IInputHandler>();
            KeyHandlers = new Stack<IKeyEventHandler>();
            Timers = new List<Timer2>();

            //Controller.TextInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(TextInput_KeyPress);
            //Controller.TextInput.KeyDown += new System.Windows.Forms.KeyEventHandler(TextInput_Key);
            //Controller.TextInput.KeyUp += new System.Windows.Forms.KeyEventHandler(TextInput_Key);
        }

        //void TextInput_Key(object sender, System.Windows.Forms.KeyEventArgs e)
        public virtual void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            foreach (IKeyEventHandler handler in KeyHandlers.ToList())
                handler.HandleKeyDown(e);
        }

        //void TextInput_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
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
        public virtual void HandleMouseWheel(HandledMouseEventArgs e)
        {
            foreach (IKeyEventHandler handler in KeyHandlers.ToList())
                if (!e.Handled)
                    handler.HandleMouseWheel(e);
        }

        //public virtual void HandleInput(InputState e)
        //{
        //    foreach (IKeyEventHandler handler in KeyHandlers.ToList())
        //        handler.HandleInput(e);
        //}


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

        void Controller_MouseMove()
        {
            OnMouseMove();
        }
        void Controller_MouseLeftPress()
        {
            OnMouseLeftPress();
        }
        void Controller_MouseRightPress()
        {
            OnMouseRightPress();
        }
        void Controller_MouseLeftUp()
        {
            OnMouseLeftUp();
        }
        void Controller_MouseLeftDown()
        {
            OnMouseLeftDown();
        }

        //static public void LoadContent();
        virtual public GameScreen Initialize()
        {
            return this;
        }
        virtual public void Update(GameTime gameTime)
        {
            List<Timer2> timers = Timers.ToList();
            foreach (Timer2 timer in timers)
                timer.Update(gameTime);
        }
        abstract public void Draw(SpriteBatch sb);


        public virtual void Dispose()
        {
            //Active = false; 
            GC.Collect();
        }

        public Stack<IKeyEventHandler> KeyHandlers;

    }
}
