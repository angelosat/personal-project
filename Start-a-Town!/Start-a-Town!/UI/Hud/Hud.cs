using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class Hud : GroupBox
    {
        public new void Initialize()
        {
            foreach (var item in Game1.Instance.GameComponents)
                item.InitHUD(this);
        }

        Dictionary<Message.Types, Action<GameEvent>> UIEvents = new();
        internal void RegisterEventHandler(Message.Types type, Action<GameEvent> action)
        {
            this.UIEvents.Add(type, action);
        }
        public static int DefaultHeight = UIManager.DefaultIconButtonSprite.Height;
        Control WindowPlayers;
        UINpcFrameContainer UnitFrames;
        public UIContextActions ContextActions;
        public Panel PartyFrame;
        public HotBar HotBar;
        public UnitFrame PlayerUnitFrame;
        public Panel Box_Buttons;
        public UIChat Chat;
        public Label Time;
        IngameMenu IngameMenu;
        ScrollbarVNew ZLevelDrawBar;
        IconButton BtnPlayers;
        public UIToolHelp ToolHelp;
        public void AddButton(IconButton btn)
        {
            btn.Location = this.Box_Buttons.Controls.TopRight;
            this.Box_Buttons.Controls.Add(btn);
            this.Box_Buttons.Location = UIManager.Size;
            this.Box_Buttons.Anchor = Vector2.One;
        }

        public Hud(IObjectProvider net, Camera camera)
        {
            this.SetMousethrough(true);
            
            IconButton BTN_Options = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 0, 32),
                HoverFunc = () => "Menu [" + GlobalVars.KeyBindings.Menu + "]",
                LeftClickAction = BTN_Options_Click
            };
            this.BtnPlayers = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 0, 32),
                HoverFunc = () => "Menu [" + GlobalVars.KeyBindings.Menu + "]",
                LeftClickAction = () => TogglePlayerList(net)
            };

            this.UnitFrames = new UINpcFrameContainer() { LocationFunc = () => new Vector2(UIManager.Width / 2, 0), Anchor = Vector2.UnitX * .5f };
            PartyFrame = new Panel();
            this.HotBar = PlayerOld.Instance.HotBar;
            this.HotBar.Location = new Vector2((UIManager.Width - this.HotBar.Width) / 2f, 0);

            Box_Buttons = new Panel() { AutoSize = true, Location = UIManager.Size, Color = Color.Black };
            Box_Buttons.AddControlsHorizontally(
                BtnPlayers,
                BTN_Options
                );
            Box_Buttons.Anchor = Vector2.One;
            this.Box_Buttons.SetMousethrough(true, false);
            Controls.Add(Box_Buttons);

            var camWidget = new CameraWidget(camera)
            {
                LocationFunc = () => new Vector2(UIManager.Width, 0),
                Anchor = new Vector2(1, 0)
            };

            var uiSpeed = new UIGameSpeed(net)
            {
                LocationFunc = () => this.Box_Buttons.TopRight,
                Anchor = Vector2.One
            };

            this.Time = new Label()
            {
                LocationFunc = () => uiSpeed.TopRight,
                Anchor = Vector2.One,
                BackgroundColorFunc = () => Color.Black * .5f,
                TextFunc = () => string.Format("Day {0}, {1:%h}h", (int)net.Map.World.Clock.TotalDays, net.Map.World.Clock)
            };

            this.Chat = UIChat.Instance;
            this.IngameMenu = new IngameMenu();
            GameModes.GameMode.Current.OnIngameMenuCreated(this.IngameMenu);

            this.ContextActions = new UIContextActions() { Location = new Vector2(UIManager.Width / 2, UIManager.Height / 2) };
            this.ZLevelDrawBar = new ScrollbarVNew(MapBase.MaxHeight, MapBase.MaxHeight, 1, 16, 1,
                 () => MapBase.MaxHeight - Rooms.Ingame.GetMap().Camera.DrawLevel,
                 () => 1 / (float)MapBase.MaxHeight,
                 () => Rooms.Ingame.GetMap().Camera.DrawLevel / MapBase.MaxHeight,
                 v => Rooms.Ingame.GetMap().Camera.DrawLevel = MapBase.MaxHeight - v);

            this.ZLevelDrawBar.Location = this.ZLevelDrawBar.RightCenterScreen;

            this.ToolHelp = new UIToolHelp() { Location = this.ZLevelDrawBar.BottomRight };

            Controls.Add(
                this.ZLevelDrawBar,
                camWidget, uiSpeed,
                this.ToolHelp,
                Chat,
                ContextActions
                , this.Time
                , this.UnitFrames
                );
            FloatingBars = new Dictionary<GameObject, FloatingBar>();
        }

        private void TogglePlayerList(IObjectProvider net)
        {
            if (this.WindowPlayers is null)
            {
                this.WindowPlayers = new UIPlayerList(net.GetPlayers())
                    .ToWindow("Players")
                    .Transparent()
                    .AnchorTo(this.BtnPlayers.ScreenLocation + this.BtnPlayers.TopRight, Vector2.One);
                this.WindowPlayers.Layer = LayerTypes.Hud;
            }
            this.WindowPlayers.Toggle();
        }


        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.NotEnoughSpace:
                    NotEnoughSpace(e.Parameters[0] as GameObject);
                    break;

                case Message.Types.ItemGot:
                    OnItemGot(e.Parameters[0] as GameObject, e.Parameters[1] as GameObject);
                    break;

                case Message.Types.ItemLost:
                    OnItemLost(e.Parameters[0] as GameObject, e.Parameters[1] as GameObject, (int)e.Parameters[2]);
                    break;

                case Message.Types.HealthLost:
                    int dmg = (int)e.Parameters[1];
                    GameObject recipient = (GameObject)e.Parameters[0];
                    FloatingText floating = new FloatingText(recipient, dmg.ToString()) { Font = UIManager.FontBold, TextColorFunc = () => Color.Red };
                    floating.Show();
                    break;

                case Message.Types.NoDurability:
                    var target = e.Parameters[0] as GameObject;
                    SpeechBubbleOld.Create(target, "Broken equipment");
                    break;

                case Message.Types.InvalidTarget:
                    target = e.Parameters[0] as GameObject;
                    SpeechBubbleOld.Create(target, "Invalid target");
                    break;

                case Message.Types.InvalidTargetType:
                    target = e.Parameters[0] as GameObject;
                    SpeechBubbleOld.Create(target, "Invalid target type");
                    break;

                case Message.Types.OutOfRange:
                    target = e.Parameters[0] as GameObject;
                    FloatingText.Manager.Create(target, "Out of range", ft => ft.Font = UIManager.FontBold);
                    break;

                case Message.Types.InteractionInterrupted:
                    target = e.Parameters[0] as GameObject;
                    var interaction = e.Parameters[1] as Interaction;
                    //SpeechBubble.Create(target, "Interrupted");
                    //FloatingText.Manager.Create(target, "Interrupted" + " " + interaction.Name, ft => ft.Font = UIManager.FontBold);
                    FloatingText.Manager.Create(target, interaction.Name + " interrupted!", ft => ft.Font = UIManager.FontBold);
                    break;

                case Message.Types.ChatEntity:
                    var entity = e.Parameters[0] as GameObject;
                    string txt = (string)e.Parameters[1];
                    this.Chat.Write(new Log.Entry(Log.EntryTypes.Chat, entity, txt));
                    SpeechBubble.ShowNew(entity, txt);
                    break;

                case Message.Types.ChatPlayer:
                    var name = e.Parameters[0] as string;
                    txt = (string)e.Parameters[1];
                    this.Chat.Write(new Log.Entry(Log.EntryTypes.ChatPlayer, name, txt));
                    break;

                default:
                    if (this.UIEvents.TryGetValue(e.Type, out var val))
                        val(e);
                    base.OnGameEvent(e);
                    break;
            }
        }

        private void NotEnoughSpace(GameObject parent)
        {
            if (parent != PlayerOld.Actor)
                return;
            var txt = "Not enough space";
            FloatingTextEx floating = new FloatingTextEx(parent,
                new FloatingTextEx.Segment(txt, Color.White)
                );
            Client.Instance.Log.Write(txt);
            floating.Show();
        }

        private void OnItemGot(GameObject parent, GameObject item)
        {
            if (parent != PlayerOld.Actor)
                return;
            FloatingTextEx floating = new FloatingTextEx(parent,
                new FloatingTextEx.Segment("Received ", Color.Lime),
                new FloatingTextEx.Segment(item.Name, item.GetInfo().GetQualityColor())
                );
            Client.Instance.Log.Write("Received: " + item.Name);
            floating.Show();
        }
        private void OnItemLost(GameObject parent, GameObject item, int amount)
        {
            if (parent != PlayerOld.Actor)
                return;
            FloatingTextEx floating = new FloatingTextEx(parent,
                new FloatingTextEx.Segment("Lost " + amount.ToString() + "x ", Color.Red),
                new FloatingTextEx.Segment(item.GetInfo().Name, item.GetInfo().GetQualityColor())
                );
            Client.Instance.Log.Write("Lost " + amount.ToString() + "x " + item.GetInfo().Name);
            floating.Show();
        }
      
        public override void Update()
        {
            base.Update();
            if (Engine.Map is null)
                return;
        }
        Dictionary<GameObject, FloatingBar> FloatingBars;
        void Log_EntryAdded(object sender, LogEventArgs e)
        {
            switch (e.Entry.Type)
            {
                case Log.EntryTypes.Damage:
                    GameObject target = e.Entry.Values[1] as GameObject;
                    Attack attack = e.Entry.Values[2] as Attack;
                    FloatingText floating = new FloatingText(target, attack.Value + " damage");
                    Controls.Add(floating);
                    break;
                default:
                    break;
            }
        }
        void bar_Finished(object sender, EventArgs e)
        {
            FloatingBar bar = sender as FloatingBar;
            FloatingBars.Remove(bar.Object);
        }
        public void Initialize(GameObject obj)
        {
            Controls.Remove(PartyFrame);
            PartyFrame.Controls.Clear();
            PartyFrame.AutoSize = true;

            PlayerUnitFrame = new UnitFrame().Track(obj);
            PartyFrame.Controls.Add(PlayerUnitFrame);

            Controls.Add(PartyFrame);
            PartyFrame.Invalidate(true);
            
            PlayerOld.Instance.HotBar.Initialize(PlayerOld.Actor);
        }

        public void AddUnitFrame(GameObject obj)
        {
            PartyFrame.Controls.Add(new UnitFrame() { Location = PartyFrame.Controls.Last().BottomLeft }.Track(obj));
        }
        public void RemoveUnitFrame(GameObject obj)
        {
            PartyFrame.Controls.Remove(PartyFrame.Controls.Find(frame => frame.Tag == obj));
        }
       
        void BTN_Options_Click()
        {
            this.IngameMenu.ShowDialog();// Toggle();Toggle();
        }
        public override void Reposition(Vector2 ratio)
        {
            foreach (Control ctrl in Controls)
                ctrl.Reposition(ratio);
        }

        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            base.HandleKeyDown(e);
            if (e.Handled)
                return;
            List<System.Windows.Forms.Keys> pressed = Controller.Input.GetPressedKeys();
            if (pressed.Contains(GlobalVars.KeyBindings.ObjectBrowser))
                UI.Editor.ObjectsWindowDefs.Instance.Toggle();

            if (pressed.Contains(System.Windows.Forms.Keys.Oemtilde))
                ServerConsole.Instance.Toggle();

            if (pressed.Contains(KeyBind.Build.Key))
                Client.Instance.Map.Town.ConstructionsManager.WindowBuild.Toggle();
        }
        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;
            switch (e.KeyData)
            {
                case System.Windows.Forms.Keys.Tab:
                    break;

                case System.Windows.Forms.Keys.Escape:
                    var winds = (from control in this.WindowManager.Layers[LayerTypes.Windows] where control is Window select control).ToList();
                    UISelectedInfo.ClearTargets();
                    if (winds.Count == 0)
                        this.IngameMenu.ToggleDialog();
                    foreach (var win in winds)
                        win.Hide();
                    break;

                default:
                    base.HandleKeyUp(e);
                    break;
            }
            return;
        }
        public override void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.Handled)
                return;
            switch (e.KeyChar)
            {
                case '/':
                    if (this.Chat.TextBox.Enabled)
                    {
                        this.Chat.TextBox.Text += "/";
                        return;
                    }
                    this.Chat.TextBox.Text = "/";
                    this.Chat.StartTyping();
                    break;

                case '\r':
                    if (this.Chat.TextBox.Enabled)
                    {
                        this.Chat.TextBox.EnterFunc(this.Chat.TextBox.Text);
                        this.Chat.TextBox.Text = "";
                        return;
                    }
                    this.Chat.StartTyping();
                    break;

                default:
                    base.HandleKeyPress(e);
                    break;
            }
        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
    }
}
