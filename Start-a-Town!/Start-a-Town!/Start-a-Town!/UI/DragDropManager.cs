using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

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

        //public DragEventArgs(object item, object source, DragDropEffects effects)
        //{
        //    this.Item = item;
        //    this.Source = source;
        //    this.Effects = effects;
        //}
        public virtual bool Cancel()
        {
            return false;
        }
        public virtual void Draw(SpriteBatch sb) { }
    }

    public class DragDropSlot : DragEventArgs//IDragDropAction
    {
        Icon Icon;
        public GameObject Parent;
        new public GameObjectSlot Source;
        public GameObjectSlot Slot;

        public TargetArgs SourceTarget, DraggedTarget;

        public DragDropSlot(GameObject parent, TargetArgs source, TargetArgs dragged, DragDropEffects effects)
        {
            this.Parent = parent;
            this.SourceTarget = source;
            this.DraggedTarget = dragged;
            this.Effects = effects;
            //this.Icon = this.Slot.Object.GetComponent<GuiComponent>().Icon;
            this.Icon = dragged.Slot.Object.GetIcon();
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

        public override bool Cancel()
        {
            if (this.Source.IsNull())
                return true;
            if (!Source.HasValue || (Source.Object == Slot.Object))
            {
                // MUST SEND INPUT TO SERVER
                Net.Client.PostPlayerInput(Message.Types.ContainerOperation, w =>
                {
                    ArrangeInventoryEventArgs.Write(w, new TargetArgs(this.Parent), new TargetArgs(this.Source.Object), new TargetArgs(this.Slot.Object), this.Source.Container.ID, this.Source.ID, (byte)this.Slot.StackSize);
                });
                //Source.Set(Slot.Object, Source.StackSize + Slot.StackSize);
                
                return true;
            }
            else
            {
                // if the source slot it occupied by a new item other than the dragged one, give object to container normally
                // OR IF THE SOURCE SLOT IS NULL (item originates from a split stack operation)
                Net.Client.PostPlayerInput(this.Parent, Message.Types.ReceiveItem, w =>
                {
                    TargetArgs.Write(w, Slot.Object);
                    w.Write((byte)Slot.StackSize);
                });
            }
            //Source.StackSize + Slot.StackSize;
            return true;
        }

        public override void Draw(SpriteBatch sb)
        {
            Vector2 ScreenLocation = Controller.Instance.MouseLocation / UIManager.Scale;
            if (this.Icon != null)
            {
                //Icon.Draw(sb, ScreenLocation);
                this.DrawEntity(sb);
            }
            UIManager.DrawStringOutlined(sb, this.DraggedTarget.Slot.StackSize.ToString(), ScreenLocation + new Vector2(UI.Slot.DefaultHeight), Vector2.One, UIManager.FontBold);

            //UIManager.DrawStringOutlined(sb, Slot.StackSize.ToString(), ScreenLocation + new Vector2(UI.Slot.DefaultHeight), Vector2.One, UIManager.FontBold);
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
            gd.Textures[1] = Sprite.Atlas.DepthTexture;
            fx.CurrentTechnique.Passes["Pass1"].Apply();

            var body = obj.Body;
            var scale = 1;// (float)Math.Min(1, this.Width / sprite.GetSourceRect().Width);

            loc += sprite.Origin;// new Vector2(rect.Width, rect.Height) / 2f;
            //body.DrawTree(this.Tag.Object, sb, loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, SpriteEffects.None, 1f, 0.5f);
            body.DrawTree(obj, mysb, loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);

            mysb.Flush();
        }
    }

    class DragDropManager : InputHandler, IKeyEventHandler
    {
        public DragEventArgs Action;
        public static IDropTarget TopMostDropTarget;
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
        public Texture2D Sprite;
        public Rectangle SourceRect;
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

        public void Update()
        {
            //GameObjectSlot dragobj = Source as GameObjectSlot;
            //if (dragobj == null)
            //    return;
            //if (dragobj.StackSize == 0)
            //    Clear();
            if (Item.IsNull())
                return;
            GameObjectSlot slot = Item as GameObjectSlot;
            if (slot.StackSize == 0)
                Clear();
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
            if (this.Action.IsNull())
                return;
            IDropTarget target = Controller.Instance.Mouseover.Object as IDropTarget;
            if (target.IsNull())
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
        static public void Create(object item, object source, DragDropEffects effects)
        {
            Instance.Item = item;
            Instance.Source = source;
            Instance.Effects = effects;

            // changed the source and use it as the gameobject owner of the slot
            //GameObjectSlot objSlot = source as GameObjectSlot;
            GameObjectSlot objSlot = item as GameObjectSlot;
            if (objSlot.IsNull())
                return;
            if (!objSlot.HasValue)
                throw new ArgumentNullException();
            Instance.Icon = objSlot.Object["Gui"]["Icon"] as Icon;
            if (Instance.Icon.IsNull())
                throw new Exception(objSlot.Object.Name + " doesn't have an Icon");
        }

        static public void Create(DragEventArgs action)
        {
            Instance.Action = action;
        }

        public void Draw(SpriteBatch sb)
        {
            if (Action.IsNull())
                return;
            Action.Draw(sb);
            return;
            //if (Sprite != null)
            //    sb.Draw(Sprite, Controller.Instance.MouseLocation, SourceRect, Color.White);
            Vector2 ScreenLocation = Controller.Instance.MouseLocation / UIManager.Scale;
            if (Icon != null)
                Icon.Draw(sb, ScreenLocation);
            GameObjectSlot slot = Item as GameObjectSlot;
            if (slot.IsNull())
                return;
            UIManager.DrawStringOutlined(sb, slot.StackSize.ToString(), ScreenLocation + new Vector2(Slot.DefaultHeight), Vector2.One, UIManager.FontBold);
        }

        public bool Clear()
        {
            if (this.Action.IsNull())
                return false;
            //if (this.Action.Cancel())
            //{
                this.Action = null;
                return true;
            //}

            return false;
        }

        //public bool Cancel()
        //{
        //    return false;
        //}

        //static public DragEventArgs Args
        //{ get { return new DragEventArgs(Instance.Item, Instance.Source, Instance.Effects); } }
    }
}
