using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using UI;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public enum DragDropEffects { None = 0, Copy = 1, Move = 2, Link = 4 }

    //public interface IDragDropAction
    //{
    //    DragDropEffects Perform(object target);
    //    bool Cancel();
    //    void Draw(SpriteBatch sb);
    //    DragDropEffects Effects { get; set; }
    //}

    public class DragEventArgs : EventArgs
    {
        public object Item;
        public object Source;
        public DragDropEffects Effects;
        public virtual void Draw(SpriteBatch sb) { }
    }

    public class DragDropSlot : DragEventArgs//IDragDropAction
    {
        Icon Icon;
        public GameObject Parent;
        new public GameObjectSlot Source;
        public GameObjectSlot Slot;
        RenderTarget2D Texture;
        public TargetArgs SourceTarget, DraggedTarget;

        public DragDropSlot(GameObject parent, TargetArgs source, TargetArgs dragged, DragDropEffects effects)
        {
            this.Parent = parent;
            this.SourceTarget = source;
            this.DraggedTarget = dragged;
            this.Effects = effects;
            //this.Icon = this.Slot.Object.GetComponent<GuiComponent>().Icon;
            this.Icon = dragged.Slot.Object.GetIcon();
            var rect = dragged.Slot.Object.Body.GetMinimumRectangle();
            this.Texture = new RenderTarget2D(Game1.Instance.GraphicsDevice, rect.Width, rect.Height);
            dragged.Slot.Object.Body.RenderNewererest(dragged.Slot.Object, this.Texture);
        }
        public DragDropSlot(GameObject parent, GameObjectSlot source, GameObjectSlot slot, DragDropEffects effects)
        {
            this.Parent = parent;
            this.Source = source;
            this.Slot = slot;
            this.Effects = effects;
            //this.Icon = this.Slot.Object.GetComponent<GuiComponent>().Icon;
            this.Icon = this.Slot.Object.GetIcon();
        }

        public DragDropEffects Perform(object target)
        {
            return DragDropEffects.None;
        }

        public override void Draw(SpriteBatch sb)
        {
            Vector2 ScreenLocation = Controller.Instance.MouseLocation / UIManager.Scale;
            if (this.Icon != null)
            {
                //Icon.Draw(sb, ScreenLocation);
               // this.DrawEntity(sb);
            }
            if (this.Texture != null)
            {
                //sb.Draw(UIManager.Highlight, new Rectangle((int)ScreenLocation.X, (int)ScreenLocation.Y, this.Texture.Width, this.Texture.Height), Color.White);
                sb.Draw(this.Texture, ScreenLocation, Color.White);
            }
            UIManager.DrawStringOutlined(sb, this.DraggedTarget.Slot.Object.StackSize.ToString(), ScreenLocation + new Vector2(UI.Slot.DefaultHeight), Vector2.One, UIManager.FontBold);

            //UIManager.DrawStringOutlined(sb, Slot.Object.StackSize.ToString(), ScreenLocation + new Vector2(UI.Slot.DefaultHeight), Vector2.One, UIManager.FontBold);
        }

        void DrawEntity(SpriteBatch sb)
        {
            var obj = this.DraggedTarget.Object;
            if (obj == null)
                return;
            GraphicsDevice gd = sb.GraphicsDevice;
            var sprite = obj.Body.Sprite;
            var loc = Controller.Instance.MouseLocation / UIManager.Scale;
            Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            MySpriteBatch mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"]; //EntitiesUI"];//
            //sb.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = false };
            fx.Parameters["Viewport"].SetValue(new Vector2(UIManager.Width, UIManager.Height));
            gd.Textures[0] = Sprite.Atlas.Texture;
            //gd.Textures[1] = Sprite.Atlas.DepthTexture;
            fx.CurrentTechnique.Passes["Pass1"].Apply();

            var body = obj.Body;
            var scale = 1;// (float)Math.Min(1, this.Width / sprite.GetSourceRect().Width);

            loc += sprite.OriginGround;// new Vector2(rect.Width, rect.Height) / 2f;
            //body.DrawTree(this.Tag.Object, sb, loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, SpriteEffects.None, 1f, 0.5f);
            body.DrawGhost(obj, mysb, loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, .5f);

            mysb.Flush();
        }
    }

    class DragDropManager : InputHandler, IKeyEventHandler
    {
        public DragEventArgs Action;
        //public static IDropTarget TopMostDropTarget;
        public object Source;
        public DragDropEffects Effects;
        protected object _Item;
        public Object Item
        {
            get { return _Item; }
            set
            {
                _Item = value;
                //if (_Item != null)
                //{

                //    GameObjectSlot objSlot = _Item as GameObjectSlot;
                //    GameObject obj = objSlot.Object;
                //    GuiComponent gui;
                //    if (obj.TryGetComponent<GuiComponent>("Gui", out gui))
                //    {
                //        Icon = gui.GetProperty<Icon>("Icon");
                //    }
                //}
                //else
                //    Icon = null;

            }
        }
        Icon Icon;
        //public Texture2D Sprite;
        //public Rectangle SourceRect;
        static DragDropManager _Instance;
        public static DragDropManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new DragDropManager();
                return _Instance;   
            }
        }
        public Control Control;
        Action CreateAction;

        public void Update()
        {
            if(this.Control != null)
            {
                if(!this.Control.HitTest())
                {
                    this.Control = null;
                    this.CreateAction();
                }
            }
            if (Item == null)
                return;
            GameObjectSlot slot = Item as GameObjectSlot;
            if (slot.StackSize == 0)
                Clear();
        }
        public void UpdateOld()
        {
            //GameObjectSlot dragobj = Source as GameObjectSlot;
            //if (dragobj == null)
            //    return;
            //if (dragobj.StackSize == 0)
            //    Clear();
            if (Item == null)
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

        //public DragDrop(Object data)
        //{
        //    Data = data;
        //}
        DragDropManager() 
        {
            //Controller.MouseLeftRelease += new InputEvent(Controller_MouseLeftRelease);
           // Game1.TextInput.RMouseDown += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(TextInput_RMouseDown);
        }

        //void TextInput_RMouseDown(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    if(this.Clear())
        //    e.Handled = true;
        //}
        public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //if (this.Action.IsNull())
            //    return;
            //if (this.Action.Cancel())
            //{
            //    e.Handled = true;
            //    this.Action = null;
            //}
            if (this.Clear())
                e.Handled = true;
        }
        public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
          
            //if (this.Item.IsNull())
            //    return;
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
                    //this.Action.Cancel();
                    break;
                case DragDropEffects.Move:
                    this.Action = null;
                    break;

                    // TODO: put a bool to check wether the dragdrop stops after dropping
                case DragDropEffects.Link:
                    this.Action = null;
                    break;

            }

            //if ((Action.Effects & DragDropEffects.Move) == DragDropEffects.Move)
            //    this.Action = null;

            
        }
        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.Control = null;
            base.HandleLButtonUp(e);
        }
        static public void Create(object item, object source, DragDropEffects effects)
        {
            Instance.Item = item;
            Instance.Source = source;
            Instance.Effects = effects;

            // changed the source and use it as the gameobject owner of the slot
            //GameObjectSlot objSlot = source as GameObjectSlot;
            GameObjectSlot objSlot = item as GameObjectSlot;
            if (objSlot == null)
                return;
            if (!objSlot.HasValue)
                throw new ArgumentNullException();
            Instance.Icon = objSlot.Object["Gui"]["Icon"] as Icon;
            if (Instance.Icon == null)
                throw new Exception(objSlot.Object.Name + " doesn't have an Icon");
        }

        static public void Create(DragEventArgs action)
        {
            Instance.Action = action;
        }

        public void Draw(SpriteBatch sb)
        {
            if (Action == null)
                return;
            Action.Draw(sb);
            //return;
            ////if (Sprite != null)
            ////    sb.Draw(Sprite, Controller.Instance.MouseLocation, SourceRect, Color.White);
            //Vector2 ScreenLocation = Controller.Instance.MouseLocation / UIManager.Scale;
            //if (Icon != null)
            //    Icon.Draw(sb, ScreenLocation);
            //GameObjectSlot slot = Item as GameObjectSlot;
            //if (slot == null)
            //    return;
            //UIManager.DrawStringOutlined(sb, slot.StackSize.ToString(), ScreenLocation + new Vector2(Slot.DefaultHeight), Vector2.One, UIManager.FontBold);
        }

        public bool Clear()
        {
            if (this.Action == null)
                return false;
            //if (this.Action.Cancel())
            //{
                this.Action = null;
                return true;
            //}

            //return false;
        }

        //public bool Cancel()
        //{
        //    return false;
        //}

        //static public DragEventArgs Args
        //{ get { return new DragEventArgs(Instance.Item, Instance.Source, Instance.Effects); } }
    }
}
