using System;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using UI;

namespace Start_a_Town_
{
    public enum DragDropEffects { None = 0, Copy = 1, Move = 2, Link = 4 }

    class DragDropManager : InputHandler, IKeyEventHandler
    {
        public DragEventArgs Action;
        public object Source;
        public DragDropEffects Effects;
        protected object _Item;
        public object Item
        {
            get { return _Item; }
            set
            {
                _Item = value;
            }
        }
        Icon Icon;
        static DragDropManager _Instance;
        public static DragDropManager Instance => _Instance = new DragDropManager();
       
        public Control Control;
        Action CreateAction;

        public void Update()
        {
            if(this.Control is not null)
            {
                if(!this.Control.HitTest())
                {
                    this.Control = null;
                    this.CreateAction();
                }
            }
            if (Item is null)
                return;
            GameObjectSlot slot = Item as GameObjectSlot;
            if (slot.StackSize == 0)
                Clear();
        }

        static public void Start(Control control, Action createAction)
        {
            Instance.Control = control;
            Instance.CreateAction = createAction;
        }

        DragDropManager() 
        {
        }

        public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Clear())
                e.Handled = true;
        }
        public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Action == null)
                return;
            IDropTarget target = Controller.Instance.MouseoverBlock.Object as IDropTarget;
            if (target == null)
                return;

            e.Handled = true;
            DragDropEffects result = target.Drop(Action);

            switch (result)
            {
                case DragDropEffects.None:
                    break;
                case DragDropEffects.Move:
                    this.Action = null;
                    break;
                    // TODO: put a bool to check wether the dragdrop stops after dropping
                case DragDropEffects.Link:
                    this.Action = null;
                    break;

            }
        }
        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.Control = null;
            base.HandleLButtonUp(e);
        }
      
        static public void Create(DragEventArgs action)
        {
            Instance.Action = action;
        }

        public void Draw(SpriteBatch sb)
        {
            Action?.Draw(sb);
        }

        public bool Clear()
        {
            if (this.Action is null)
                return false;
            this.Action = null;
            return true;
        }
    }
}
