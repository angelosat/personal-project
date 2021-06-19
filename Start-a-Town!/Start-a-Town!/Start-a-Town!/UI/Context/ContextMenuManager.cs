using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;

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
    }

    

    /// <summary>
    /// void GetContextActions(ContextArgs a)
    /// </summary>
    public interface IContextable
    {
        //IEnumerable<ContextAction> GetContextActions(params object[] p);
        void GetContextActions(ContextArgs a);
    }

    class ContextMenuManager : InputHandler, IKeyEventHandler
    {
        static ContextMenuManager _Instance;
        public static ContextMenuManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ContextMenuManager();
                return _Instance;
            }
        }

        static IContextable Object;

        static float DelayInterval = Engine.TargetFps / 2f;
        static float Delay;
      //  static bool Pressed;

        static public void Update()
        {
            if (Object == null)
                return;
            Delay -= 1;//GlobalVars.DeltaTime;
            if (Delay > 0)
                return;
          //List<ContextAction> actions = new List<ContextAction>(Object.GetContextActions());
            ContextArgs a = new ContextArgs() { Parameters = new object[] { Controller.Instance.Mouseover.Face } }; // TODO: possibility that the mouseover face has changed since getting the Object
            Object.GetContextActions(a);
            foreach (var comp in Game1.Instance.GameComponents)
                comp.OnContextMenuCreated(Object, a);
            ContextMenu2.Instance.Initialize(a);
            Object = null;
        }

        public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (ContextMenu2.Instance.Hide())
            {
                e.Handled = true;
                return;
            }
            Object = Controller.Instance.Mouseover.Object as IContextable;
            Delay = DelayInterval;
        }

        public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            Object = null;
        }
    }
}
