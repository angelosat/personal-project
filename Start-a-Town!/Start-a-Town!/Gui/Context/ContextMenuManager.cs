﻿using System;
using System.Collections.Generic;
using Start_a_Town_.UI;
using UI;

namespace Start_a_Town_
{
    public class ContextArgs : EventArgs
    {
        public List<ContextAction> Actions;
        public Action<Button> ControlInit = btn => { };
        public object[] Parameters;

        public ContextArgs()
        {
            this.Actions = new List<ContextAction>();
        }
        public ContextArgs(params ContextAction[] actions)
        {
            this.Actions = new List<ContextAction>(actions);
        }
    }
  
    public interface IContextable
    {
        void GetContextActions(GameObject playerEntity, ContextArgs a);
    }

    class ContextMenuManager : InputHandler, IKeyEventHandler
    {
        static ContextMenuManager _Instance;
        public static ContextMenuManager Instance => _Instance ??= new ContextMenuManager();
        static IContextable Object;
        static float DelayInterval = Engine.TicksPerSecond / 2f;
        static float Delay;

        static public void PopUp(params ContextAction[] a)
        {
            ContextMenu2.Instance.Initialize(new ContextArgs(a));
        }

        public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (ContextMenu2.Instance.Hide())
            {
                e.Handled = true;
                return;
            }
            Object = Controller.Instance.MouseoverBlock.Object as IContextable;
            Delay = DelayInterval;
        }

        public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            Object = null;
        }
    }
}
