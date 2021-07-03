using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.UI.Editor;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.Net;
using Start_a_Town_.Editor;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class Hud : GroupBox
    {
        public new void Initialize()
        {
            //new Towns.TownsUI().InitHud(Instance);
            foreach (var item in Game1.Instance.GameComponents) //GetGameComponents())//
                item.InitHUD(this);
        }

        Dictionary<Message.Types, Action<GameEvent>> UIEvents = new();
        internal void RegisterEventHandler(Message.Types type, Action<GameEvent> action)
        {
            this.UIEvents.Add(type, action);
        }

        //UICameraSettings Elevation;
        UINpcFrameContainer UnitFrames;
        readonly Label Fps;//, MapTime;
        public UIContextActions ContextActions;
        public Panel PartyFrame;
        public QuickBar QuickBar;
        public HotBar HotBar;
        public UnitFrame PlayerUnitFrame;
        public Panel Box_Buttons;
        //Window Window_Prefab;
        public UIChat Chat;
        public Label Time;
        IngameMenu IngameMenu;
        UIQuickSlots QuickSlots;
        ScrollbarVNew ZLevelDrawBar;
        IconButton BtnPlayers;
        //UIPlayerList UIPlayers;
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
            #region Buttons
            IconButton Btn_Prefabs = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 0, 32),
                LeftClickAction = () =>
                {// Editor.BldgLoadWindow.Instance.Location = Editor.BldgLoadWindow.Instance.CenterScreen; Editor.BldgLoadWindow.Instance.Toggle(); },
                    new BldgLoadWindow((fileinfo) =>
                    {
                        Bldg bldg = new Bldg().Load(fileinfo);
                        var dic = bldg.ToDictionary();//.OrderBy(foo=>-foo.Key.GetDepth(ScreenManager.CurrentScreen.Camera));
                        EmptyTool tool = new EmptyTool()
                        {
                            LeftClick = (target) =>
                            {
                                bldg.Apply(Engine.Map, target.Global);
                                return ControlTool.Messages.Default;
                            },
                        };
                        tool.DrawAction = (sb, cam) =>
                        {
                            //if (tool.TargetOld.IsNull())
                            //    return;
                            //Vector3 global = tool.TargetOld.Global;
                            if (tool.Target == null)
                                return;
                            Vector3 global = tool.Target.Global;
                            foreach (var block in dic)
                            {
                                Vector3 target = global + block.Key;
                                //BlockComponent.Blocks[block.Value].Entity["Sprite"].DrawPreview(sb, cam, target, Color.White, target.GetDrawDepth(Engine.Map, cam));//GetDepth(cam));
                            }
                        };
                        ScreenManager.CurrentScreen.ToolManager.ActiveTool = tool;
                    }).Show();
                },
                HoverFunc = () => "Prefabs"// [" + GlobalVars.KeyBindings.Build + "]"
            };
            //IconButton Btn_Structures = new IconButton()
            //{
            //    Location = Btn_Prefabs.TopRight,
            //    BackgroundTexture = UIManager.DefaultIconButtonSprite,
            //    Icon = new Icon(UIManager.Icons32, 12, 32),
            //  //  Anchor = Vector2.UnitX,
            //    //LeftClickAction = () => BlocksWindow.Instance.Toggle(),// 
            //    //LeftClickAction = () => BuildingsWindow.Instance.Toggle(), //StructuresWindow.Instance.Toggle(),
            //    LeftClickAction = () => StructureWindow.Instance.Toggle(),
            //    //LeftClickAction = () => TerrainWindow.Instance.Toggle(),// BuildingsWindow.Instance.Toggle(), //StructuresWindow.Instance.Toggle(),
            //    HoverFunc = () => "Build [" + GlobalVars.KeyBindings.Build + "]"
            //};
            //IconButton Btn_Construct = new IconButton()
            //{
            //    Location = Btn_Structures.TopRight,
            //    BackgroundTexture = UIManager.DefaultIconButtonSprite,
            //    Icon = new Icon(UIManager.Icons32, 12, 32),
            //    LeftClickAction = () => ConstructionsWindow.Instance.Toggle(),
            //    HoverFunc = () => "Construct [" + GlobalVars.KeyBindings.Build + "]"
            //};
            IconButton Btn_Craft = new IconButton()
            {
                //Location = Btn_Construct.TopRight,
                //Location = Btn_Structures.TopRight,
                Location = Btn_Prefabs.TopRight,
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                LeftClickAction = () => { },//CraftWindow.Instance.Toggle(),
                HoverFunc = () => "Craft [" + GlobalVars.KeyBindings.Build + "]"
            };
            IconButton Btn_Inventory = new IconButton()
            {
                Location = Btn_Craft.TopRight,//Location,
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 1, 32),
                //  Anchor = Vector2.UnitX,
                LeftClickAction = () => InvWindow.Toggle(PlayerOld.Actor),
                HoverFunc = () => "Inventory [" + GlobalVars.KeyBindings.Inventory + "]"
            };
            IconButton Btn_Needs = new IconButton()
            {
                Location = Btn_Inventory.TopRight,//Location,
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 10, 32),
                // Anchor = Vector2.UnitX,
                LeftClickAction = () => NeedsWindow.Toggle(PlayerOld.Actor),
                HoverFunc = () => "Needs [" + GlobalVars.KeyBindings.Needs + "]"
            };
            //IconButton Btn_Npcs = new IconButton()
            //{
            //    Location = Btn_Needs.TopRight,//Location,
            //    BackgroundTexture = UIManager.DefaultIconButtonSprite,
            //    Icon = new Icon(UIManager.Icons32, 11, 32),
            //  //  Anchor = Vector2.UnitX,
            //    LeftClickAction = () => NpcInfoWindow.Instance.Toggle(),
            //    HoverFunc = () => "Npcs [" + GlobalVars.KeyBindings.Npcs + "]"
            //};
            IconButton Btn_Jobs = new IconButton()
            {
                Location = Btn_Needs.TopRight,//Location,
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 9, 32),
                //   Anchor = Vector2.UnitX,
                //LeftClickAction = () => JobBoardWindow.Instance.Toggle(),
                HoverFunc = () => "Jobs [" + GlobalVars.KeyBindings.Jobs + "]"
            };
            //IconButton Btn_Stockpile = new IconButton()
            //{
            //    Location = Btn_Jobs.TopRight,
            //    BackgroundTexture = UIManager.DefaultIconButtonSprite,
            //    Icon = new Icon(UIManager.Icons32, 12, 32),
            //    LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new Towns.Stockpiles.StockpileTool(s => { }),
            //    HoverFunc = () => "Stockpile"
            //};

            #endregion
            IconButton BTN_Options = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 0, 32),
                HoverFunc = () => "Menu [" + GlobalVars.KeyBindings.Menu + "]",
                LeftClickAction = BTN_Options_Click
            };
            //           BTN_Options.Location = new Vector2(WindowManager.ScreenWidth - BTN_Options.Width, WindowManager.ScreenHeight - BTN_Options.Height);
            this.BtnPlayers = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 0, 32),
                HoverFunc = () => "Menu [" + GlobalVars.KeyBindings.Menu + "]",
                LeftClickAction = () => TogglePlayerList(net)
            };

            this.UnitFrames = new UINpcFrameContainer() { LocationFunc = () => new Vector2(UIManager.Width / 2, 0), Anchor = Vector2.UnitX * .5f };// { BackgroundColorFunc = () => Color.Black * .5f };
            PartyFrame = new Panel();

            //CreatePrefabWindow();
            this.HotBar = PlayerOld.Instance.HotBar;// new HotBar();
            this.HotBar.Location = new Vector2((UIManager.Width - this.HotBar.Width) / 2f, 0);
            //this.HotBar.Location = new Vector2((UIManager.Width - this.HotBar.Width) / 2f, UIManager.Height - this.HotBar.Height);

            //this.QuickBar = new QuickBar() { Count = 10 };
            //QuickBar.Initialize();
            //this.QuickBar.Location = new Vector2((WindowManager.ScreenWidth - this.QuickBar.Width) / 2f, 0);

            Box_Buttons = new Panel() { AutoSize = true, Location = UIManager.Size, Color = Color.Black };
            Box_Buttons.AddControlsHorizontally(
                BtnPlayers,
                BTN_Options
                );
            //Btn_Craft, Btn_Inventory, Btn_Needs, Btn_Npcs, Btn_Jobs, 
            //Btn_Prefabs);
            Box_Buttons.Anchor = Vector2.One;
            this.Box_Buttons.SetMousethrough(true, false);
            Controls.Add(Box_Buttons);
            //Elevation = new UICameraSettings(camera);
            //Elevation.Location = camWidget.BottomLeft;// uiSpeed.BottomLeft;

            var camWidget = new CameraWidget(camera)
            {
                LocationFunc = () =>
new Vector2(UIManager.Width, 0),
                Anchor = new Vector2(1, 0)
            };

            var uiSpeed = new UIGameSpeed(net)
            {
                //Location = this.Box_Buttons.Location - this.Box_Buttons.Dimensions*this.Box_Buttons.Anchor//,//.TopRight, 
                LocationFunc = () => this.Box_Buttons.TopRight,
                Anchor = Vector2.One
            };// { Location = camWidget.BottomLeft };

            this.Time = new Label()
            {
                LocationFunc = () => uiSpeed.TopRight,
                Anchor = Vector2.One,
                BackgroundColorFunc = () => Color.Black * .5f,
                TextFunc = () => string.Format("Day {0}, {1:%h}h", (int)net.Map.World.Clock.TotalDays, net.Map.World.Clock)
                //TextFunc = () => string.Format("Day {0}\n{1:%h}h {1:%m}m", (int)net.Map.Time.TotalDays, net.Map.Time) }; //"Day {0}\n{1:hh\\:mm}"
                //"Day {0}\n{1:hh\\:mm}"
            };

            //MapTime = new Label("Map time: " + Engine.Map.Time + "\nDarkness: " + Engine.Map.SkyDarkness + 
            //    "\n" + (Engine.Average.TotalMilliseconds / (float)Engine.TargetFps).ToString("0.00 ns"));
            //MapTime.Halign = HorizontalAlignment.Right;
            Fps = new FpsCounter();// { Location = MapTime.BottomLeft };

            this.Chat = UIChat.Instance;// new Chat();
            this.IngameMenu = new IngameMenu();
            GameModes.GameMode.Current.OnIngameMenuCreated(this.IngameMenu);

            this.ContextActions = new UIContextActions() { Location = new Vector2(UIManager.Width / 2, UIManager.Height / 2) };
            //Controller.MouseoverObjectChanged += Controller_MouseoverObjectChanged;
            this.ZLevelDrawBar = new ScrollbarVNew(Map.MaxHeight, Map.MaxHeight, 1, 16, 1,
                 () => Map.MaxHeight - Rooms.Ingame.GetMap().Camera.DrawLevel,
                 () => 1 / (float)Map.MaxHeight,
                 () => Rooms.Ingame.GetMap().Camera.DrawLevel / Map.MaxHeight,
                 v => Rooms.Ingame.GetMap().Camera.DrawLevel = Map.MaxHeight - v);
            //this.ZLevelDrawBar = new ScrollbarVNew(Map.MaxHeight, Map.MaxHeight, 1, 16, 1,
            //    () => Map.MaxHeight - camera.DrawLevel,
            //    () => 1 / (float)Map.MaxHeight,
            //    () => camera.DrawLevel / Map.MaxHeight,
            //    v => camera.DrawLevel = Map.MaxHeight - v);

            this.ZLevelDrawBar.Location = this.ZLevelDrawBar.RightCenterScreen;

            //this.UIPlayers = new UIPlayerList(net.GetPlayers()) { Location = camWidget.Location, Anchor = Vector2.UnitX };

            this.ToolHelp = new UIToolHelp() { Location = this.ZLevelDrawBar.BottomRight };

            Controls.Add(
                //Fps, 
                this.ZLevelDrawBar,
                //Elevation,
                camWidget, uiSpeed,
                //PartyFrame, 
                //HotBar,
                //this.UIPlayers,
                this.ToolHelp,
                Chat,
                ContextActions
                , this.Time
                , this.UnitFrames
                );//LogWindow.Instance); // MapTime, ActionBar.Instance, QuickBar
            FloatingBars = new Dictionary<GameObject, FloatingBar>();
            //Log.Instance.EntryAdded += new EventHandler<LogEventArgs>(Log_EntryAdded);
            //Client.Instance.GameEvent += Client_GameEvent;
        }
        //UIPlayerList WindowPlayers;
        Control WindowPlayers;

        //internal static void AddToSelection(TargetArgs target)
        //{
        //    UISelectedInfo.AddToSelection(target);
        //}

        //internal static void Select(TargetArgs target)
        //{
        //    if(target.Type != TargetType.Null)
        //        UISelectedInfo.Refresh(target);
        //}

        private void TogglePlayerList(IObjectProvider net)
        {
            if (this.WindowPlayers == null)
            {
                this.WindowPlayers = new UIPlayerList(net.GetPlayers())
                    .ToWindow("Players")
                    .Transparent()
                    .AnchorTo(this.BtnPlayers.ScreenLocation + this.BtnPlayers.TopRight, Vector2.One);
                this.WindowPlayers.Layer = LayerTypes.Hud;
                //this.WindowPlayers.AnchorTo(this.BtnPlayers.ScreenLocation + this.BtnPlayers.TopRight, Vector2.One);
            }
            this.WindowPlayers.Toggle();
        }


        internal override void OnGameEvent(GameEvent e)
        {
            //    base.OnGameEvent(e);
            //}
            //void Client_GameEvent(object sender, GameEvent e)
            //{
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
                    //int dmg = (int)e.Args.Parameters[0];
                    //FloatingText floating = new FloatingText(e.Recipient, dmg.ToString()) { Font = UIManager.FontBold, TextColorFunc = () => Color.Red };

                    int dmg = (int)e.Parameters[1];
                    GameObject recipient = (GameObject)e.Parameters[0];
                    FloatingText floating = new FloatingText(recipient, dmg.ToString()) { Font = UIManager.FontBold, TextColorFunc = () => Color.Red };
                    floating.Show();
                    break;

                //case Message.Types.JobStepFinished:
                //    GameObject target = e.Parameters[1] as GameObject;
                //    Script.Types scriptID = (Script.Types)e.Parameters[2];
                //    Script script = Ability.GetScript(scriptID);
                //    FloatingText.Manager.Create(target, "Job updated", ft => ft.Font = UIManager.FontBold);
                //    break;

                case Message.Types.DurabilityLoss:
                    var target = e.Parameters[0] as GameObject;
                    //FloatingText.Manager.Create(Player.Actor, target.Name + " has lost some durability.", ft => ft.Font = UIManager.FontBold);
                    //Client.Console.Write(target.Name + " has lost some durability.");
                    break;

                case Message.Types.NoDurability:
                    target = e.Parameters[0] as GameObject;
                    SpeechBubbleOld.Create(target, "Broken equipment");
                    break;

                case Message.Types.InteractionFailed:
                    break;
                //target = e.Parameters[0] as GameObject;
                //ScriptTaskCondition condition = e.Parameters[1] as ScriptTaskCondition;
                //SpeechBubbleOld.Create(target, "Failed: " + condition.Name);
                //break;

                case Message.Types.InvalidTarget:
                    target = e.Parameters[0] as GameObject;
                    SpeechBubbleOld.Create(target, "Invalid target");
                    break;

                case Message.Types.InvalidTargetType:
                    target = e.Parameters[0] as GameObject;
                    SpeechBubbleOld.Create(target, "Invalid target type");
                    break;

                case Message.Types.WrongTool:
                    target = e.Parameters[0] as GameObject;
                    SpeechBubbleOld.Create(target, "Wrong tool equipped");
                    break;

                case Message.Types.TooDense:
                    target = e.Parameters[0] as GameObject;
                    SpeechBubbleOld.Create(target, "Material too dense");
                    break;

                case Message.Types.InsufficientMaterials:
                    target = e.Parameters[0] as GameObject;
                    SpeechBubbleOld.Create(target, "Not enough materials");
                    break;

                case Message.Types.ScriptMismatch:
                    target = e.Parameters[0] as GameObject;
                    SpeechBubbleOld.Create(target, "Script Mismatch");
                    break;

                case Message.Types.OutOfRange:
                    target = e.Parameters[0] as GameObject;
                    //SpeechBubble.Create(target, "Out of range");
                    FloatingText.Manager.Create(target, "Out of range", ft => ft.Font = UIManager.FontBold);
                    break;

                case Message.Types.InteractionInterrupted:
                    target = e.Parameters[0] as GameObject;
                    var interaction = e.Parameters[1] as Interaction;
                    //SpeechBubble.Create(target, "Interrupted");
                    //FloatingText.Manager.Create(target, "Interrupted" + " " + interaction.Name, ft => ft.Font = UIManager.FontBold);
                    FloatingText.Manager.Create(target, interaction.Name + " interrupted!", ft => ft.Font = UIManager.FontBold);
                    break;

                case Message.Types.TargetNotInventoryable:
                    target = e.Parameters[0] as GameObject;
                    SpeechBubbleOld.Create(target, "This doesn't fit in my inventory");
                    break;

                case Message.Types.Memorization:
                    target = e.Parameters[0] as GameObject;
                    var bp = e.Parameters[1] as GameObject;
                    int value = (int)e.Parameters[2];
                    new FloatingText(target, value.ToString("+#;-#;0") + " " + bp.Name) { Font = UIManager.FontBold, TextColorFunc = () => Color.Gold }.Show();
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

                case Message.Types.DialogueStart:
                    var entity1 = e.Parameters[0] as GameObject;
                    var entity2 = e.Parameters[1] as GameObject;
                    var options = e.Parameters[2] as List<DialogOption>;
                    var convo = e.Parameters[3] as Conversation;
                    var attention = e.Parameters.ElementAtOrDefault(4) as IProgressBar;

                    GameObject player, ai;
                    if (entity1 == PlayerOld.Actor)
                    {
                        player = entity1;
                        ai = entity2;
                    }
                    else
                    {
                        player = entity2;
                        ai = entity1;
                    }

                    var b1 = SpeechBubble.ShowNew(player, "Hi " + ai.Name, options, convo, attention);
                    var aitext = "Hello " + player.Name;
                    //var b2 = SpeechBubble.ShowNew(ai, aitext, convo);
                    player.Net.EventOccured(Message.Types.ChatEntity, ai, aitext);
                    break;

                case Message.Types.InventoryChanged:
                    //this.ContextActions.Refresh();
                    break;

                case Message.Types.Dialogue:
                    PacketDialogueOptions p = e.Parameters[0] as PacketDialogueOptions;
                    var b = new SpeechBubble(p.Parent, p.Text, p.DialogOptions);//, p.Attention);
                    b.Show();
                    break;

                //case Message.Types.ManageEquipment:
                //    var npc = e.Parameters[0] as GameObject;
                //    var window = new WindowEntityInterface(npc, npc.Name, () => npc.Global);
                //    var ui = new Components.Inventory.InventoryInterface().Initialize(npc);
                //    window.Client.Controls.Add(ui);
                //    window.Show();
                //    break;

                case Message.Types.DoorLockToggled:
                    var doorglobal = (Vector3)e.Parameters[0];
                    //bool locked, open;
                    //int part;
                    //Blocks.BlockDoor.Read(Player.Actor.Map.GetData(doorglobal), out locked, out open, out part);
                    bool locked = BlockDoor.IsLocked(PlayerOld.Actor.Map.GetData(doorglobal));
                    FloatingText.Manager.Create(() => doorglobal, "Door " + (locked ? "Locked" : "Unlocked"), ft => ft.Font = UIManager.FontBold);
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
            //FloatingText floating = new FloatingText(parent, "Received: " + item.Name) { Font = UIManager.FontBold, TextColorFunc = item.GetInfo().GetQualityColor };
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
            //FloatingText floating = new FloatingText(parent, "Received: " + item.Name) { Font = UIManager.FontBold, TextColorFunc = item.GetInfo().GetQualityColor };
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
            //FloatingText floating = new FloatingText(parent, "Lost: " + amount.ToString() + " " + item.Name) { Font = UIManager.FontBold, TextColorFunc = () => Color.Red };
            FloatingTextEx floating = new FloatingTextEx(parent,
                new FloatingTextEx.Segment("Lost " + amount.ToString() + "x ", Color.Red),
                new FloatingTextEx.Segment(item.GetInfo().Name, item.GetInfo().GetQualityColor())
                );
            Client.Instance.Log.Write("Lost " + amount.ToString() + "x " + item.GetInfo().Name);
            floating.Show();
        }
        //private void CreatePrefabWindow()
        //{
        //    Window_Prefab = new Window() { Title = "Prefabs", AutoSize = true };

        //}

        public override void Update()
        {
            base.Update();
            if (Engine.Map == null)
                return;
            long ticks = (int)TimeSpan.FromSeconds((int)(Engine.Map.GetDayTimeNormal() * 60)).Ticks;

            //MapTime.Location = new Vector2(Elevation.Left - MapTime.Width, 0);
            //Fps.Location = MapTime.BottomLeft;

            // manual checking of alt key
            //Nameplate.Enabled = InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu);
        }
        Dictionary<GameObject, FloatingBar> FloatingBars;
        void Log_EntryAdded(object sender, LogEventArgs e)
        {
            switch (e.Entry.Type)
            {
                case Log.EntryTypes.Damage:
                    //GameObject attacked = e.Entry.Values[1] as GameObject;
                    //StatsComponent dmgStats = e.Entry.Values[2] as StatsComponent;
                    //FloatingText floating = new FloatingText(attacked, Damage.GetTotal(dmgStats) + " damage"); 
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
            bar.Finished -= bar_Finished;
            FloatingBars.Remove(bar.Object);
        }
        public void Initialize(GameObject obj)
        {
            Controls.Remove(PartyFrame);
            PartyFrame.Controls.Clear();
            PartyFrame.AutoSize = true;

            PlayerUnitFrame = new UnitFrame().Track(obj);
            PartyFrame.Controls.Add(PlayerUnitFrame);



            //PartyComponent partyComp;
            //if (!obj.TryGetComponent<PartyComponent>("Party", out partyComp))
            //    return;
            //foreach (GameObjectSlot memberSlot in partyComp.Members)
            //    if (memberSlot.HasValue)
            //        PartyFrame.Controls.Add(new UnitFrame() { Location = PartyFrame.Controls.Last().BottomLeft }.Track(memberSlot.Object));

            Controls.Add(PartyFrame);
            PartyFrame.Invalidate(true);
            
            PlayerOld.Instance.HotBar.Initialize(PlayerOld.Actor);
        }

        private void InitializeQuickslots(GameObject obj)
        {
            this.Controls.Remove(this.QuickSlots);
            this.QuickSlots = new UIQuickSlots(obj);
            this.QuickSlots.Location = new Vector2((UIManager.Width - this.QuickSlots.Width) / 2f, 0);
            this.Controls.Add(this.QuickSlots);
            // TODO: fix bug where object icons aren't drawn inside slots sometimes when game starts
            this.QuickSlots.Invalidate(true);
        }
        public void AddUnitFrame(GameObject obj)
        {
            PartyFrame.Controls.Add(new UnitFrame() { Location = PartyFrame.Controls.Last().BottomLeft }.Track(obj));
        }
        public void RemoveUnitFrame(GameObject obj)
        {
            PartyFrame.Controls.Remove(PartyFrame.Controls.Find(frame => frame.Tag == obj));
        }
        public Slot Slot_lastinteraction;
        public static int DefaultHeight = UIManager.DefaultIconButtonSprite.Height;
        void BTN_Skills_Click(object sender, EventArgs e)
        {
            SkillsWindow.Instance.Toggle();
        }
        void BTN_Character_Click(object sender, EventArgs e)
        {

        }
        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb, UIManager.Bounds);
        }
        void BTN_Options_Click()
        {
            //IngameMenu.Instance.Toggle();
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
            {
                UI.Editor.ObjectsWindowDefs.Instance.Toggle();
                //UI.Editor.ObjectsWindow.Instance.Toggle();
            }

            //if (pressed.Contains(GlobalVars.KeyBindings.Inventory))
            //    InvWindow.Toggle(Player.Actor);

            if (pressed.Contains(System.Windows.Forms.Keys.Oemtilde))
                ServerConsole.Instance.Toggle();

            //if (pressed.Contains(GlobalVars.KeyBindings.Build))
            //    UI.Editor.TerrainWindow.Instance.Toggle();
            if (pressed.Contains(KeyBind.Build.Key))
                //UISelectedInfo.Select(Client.Instance.Map.Town.ConstructionsManager);
                Client.Instance.Map.Town.ConstructionsManager.WindowBuild.Toggle();
        }
        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;
            switch (e.KeyData)
            {
                //case System.Windows.Forms.Keys.LMenu:
                //    Nameplate.Enabled = false;
                //    break;
                case System.Windows.Forms.Keys.Tab:
                    //Net.UIPlayerList.Instance.Hide();
                    //this.Controls.Remove(Net.UIPlayerList.Instance);
                    break;

                case System.Windows.Forms.Keys.Escape:
                    //IngameMenu.Instance.Toggle();
                    //var winds = (from control in this.WindowManager.Layers[this.Layer] where control is Window select control).ToList();
                    var winds = (from control in this.WindowManager.Layers[LayerTypes.Windows] where control is Window select control).ToList();
                    UISelectedInfo.ClearTargets();
                    if (winds.Count == 0)
                        this.IngameMenu.ToggleDialog();// Toggle();
                    foreach (var win in winds)
                        win.Hide();
                    //this.IngameMenu.ToggleDialog();// Toggle();
                    break;

                //case System.Windows.Forms.Keys.Oemtilde:
                //    UIServer.Instance.Hide();
                //    break;

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
