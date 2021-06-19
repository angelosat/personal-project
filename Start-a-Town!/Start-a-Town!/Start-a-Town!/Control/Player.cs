﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using System.Xml;
using Start_a_Town_.PlayerControl;

namespace Start_a_Town_
{
    public class Player : Component, IDisposable
    {


        public override string ComponentName
        {
            get { return "Player"; }
        }
        public override string ToString()
        {
            return Actor != null ? Actor.ToString() : "null";
        }

        public override object Clone()
        {
            return this;
        }

        public HotBar HotBar { get; set; }

        //public Selection Selection;
        //public Selection NextSelection;
        public DefaultTool Tool;
        ObjectOrientationTool2 OrientationTool;

        static GameObject _Actor;
        public static GameObject Actor
        {
            get { return _Actor; }
            set
            {
                //if (!_Actor.IsNull())
                //    _Actor.GetComponent<ControlComponent>().MessageReceived -= Instance.Player_MessageReceived;
                _Actor = value;
                //if (!value.IsNull())
                //    value.GetComponent<ControlComponent>().MessageReceived += Instance.Player_MessageReceived;
            }
        }
        public Vector2 StartPosition;
        
        static public string PlayerName;
        static Player _Instance;
        static public Player Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Player();
                return _Instance;
            }
        }


        void Log_EntryAdded(object sender, LogEventArgs e)
        {
            switch (e.Entry.Type)
            {
                case Log.EntryTypes.CellChanged:
                    GameObject tileObj = e.Entry.Values[0] as GameObject;
                    if (Controller.Instance.Mouseover.Object == tileObj)
                        Controller.Instance.Mouseover.Object = null;
                    //GameObject obj ;
                    //if(Controller.Instance.Mouseover.TryGet(out obj))
                    //    if(obj == tileObj)
                    //        Controller.Instance.Mouseover.Reset();
                    break;
                case Log.EntryTypes.Equip:
                case Log.EntryTypes.Unequip:
                    GameObject actor = e.Entry.Values[0] as GameObject;
                    if (actor == Player.Actor)
                        ActionBar.Instance.Initialize();
                    break;
                default:
                    break;
            }
        }

        //static public Dictionary<AbilitySlot, GameObjectSlot> GetAbilities()
        //{
        //    GameObjectSlot dragging = DragDropManager.Instance.Item as GameObjectSlot;
        //    Dictionary<AbilitySlot, GameObjectSlot> list = new Dictionary<AbilitySlot, GameObjectSlot>();
        //    GameObjectSlot hauling = (GameObjectSlot)Player.Actor["Inventory"]["Holding"];
        //    list.Add(AbilitySlot.Primary, hauling.Object == null ? Player.Actor["Equipment"].GetProperty<GameObjectSlot>(Stat.Mainhand.Name).Object["Abilities"].GetProperty<List<GameObjectSlot>>("Abilities")[0] : FunctionComponent.GetAbility(hauling.Object, 0));//hauling.Object["Abilities"].GetProperty<List<GameObjectSlot>>("Abilities")[0]);
        //    list.Add(AbilitySlot.Secondary, hauling.Object == null ? LastAbilityUsed : FunctionComponent.GetAbility(hauling.Object, 1));
        //    list.Add(AbilitySlot.PickUp, hauling.Object == null ? Ability.PickingUp : Ability.Drop);
        //    list.Add(AbilitySlot.Function3, Ability.ManageEquipment);
        //    return list;
        //}
        //static public Dictionary<AbilitySlot, GameObjectSlot> GetAbilities(GameObject target)
        //{
        //    Dictionary<AbilitySlot, GameObjectSlot> list = new Dictionary<AbilitySlot, GameObjectSlot>();
        //    GameObjectSlot hauling = (GameObjectSlot)Player.Actor["Inventory"]["Holding"];
        //    List<Interaction> targetActions = new List<Interaction>();
        //    target.Query(Player.Actor, targetActions);
        //    list.Add(AbilitySlot.Primary, targetActions.Exists(foo => foo.Message == Message.Types.Attack) ? Ability.Attack : Ability.Walk);
        //    list.Add(AbilitySlot.Secondary, targetActions.Exists(foo => foo.Message == (Message.Types)LastAbilityUsed.Object["Ability"]["Message"]) ? LastAbilityUsed : GameObjectSlot.Empty);
        //    list.Add(AbilitySlot.PickUp, targetActions.Exists(foo => foo.Message == Message.Types.PickUp) ? Ability.PickingUp : (hauling.HasValue) ? Ability.Drop : GameObjectSlot.Empty);
        //    list.Add(AbilitySlot.Throw, (hauling.HasValue) ? Ability.Throw : GameObjectSlot.Empty);
        //    return list;
        //}
        //static public Dictionary<GameObjectSlot, System.Windows.Forms.Keys> GetAbilitiesKeys()
        //{

        //    GameObjectSlot dragging = DragDropManager.Instance.Item as GameObjectSlot;
        //    Dictionary<GameObjectSlot, System.Windows.Forms.Keys> list = new Dictionary<GameObjectSlot, System.Windows.Forms.Keys>();
        //    GameObjectSlot hauling = (GameObjectSlot)Player.Actor["Inventory"]["Holding"];
        //    list.Add(hauling.Object == null ? Player.Actor["Equipment"].GetProperty<BodyPart>(Stat.Mainhand.Name).Object["Abilities"].GetProperty<List<GameObjectSlot>>("Abilities")[0] : FunctionComponent.GetAbility(hauling.Object, 0), System.Windows.Forms.Keys.LButton);
        //    //list.Add(System.Windows.Forms.Keys.RButton, hauling.Object == null ? LastAbilityUsed : FunctionComponent.GetAbility(hauling.Object, 1));

        //    list.Add(LastAbilityUsed, System.Windows.Forms.Keys.RButton);
        //    list.Add(hauling.Object == null ? Ability.PickingUp : Ability.Drop, System.Windows.Forms.Keys.F);
        //    list.Add(hauling.Object == null ? GameObjectSlot.Empty : Ability.Throw, System.Windows.Forms.Keys.G);
        //   // list.Add(AbilitySlot.Function3, Ability.ManageEquipment);
        //    return list;
        //}
        //static public Dictionary<System.Windows.Forms.Keys, GameObjectSlot> GetKeysAbilities()
        //{
        //    Dictionary<System.Windows.Forms.Keys, GameObjectSlot> list = new Dictionary<System.Windows.Forms.Keys, GameObjectSlot>();
        //    GameObjectSlot hauling = (GameObjectSlot)Player.Actor["Inventory"]["Holding"];
        //    list.Add(System.Windows.Forms.Keys.LButton, hauling.Object == null ? Player.Actor["Equipment"].GetProperty<BodyPart>(Stat.Mainhand.Name).Object["Abilities"].GetProperty<List<GameObjectSlot>>("Abilities")[0] : FunctionComponent.GetAbility(hauling.Object, 0));
        //    list.Add(System.Windows.Forms.Keys.RButton, LastAbilityUsed);
        //    list.Add(System.Windows.Forms.Keys.E, Ability.Activate);
        //    list.Add(System.Windows.Forms.Keys.F, Ability.PickingUp);
        //    list.Add(System.Windows.Forms.Keys.Q, Ability.Drop);
        //    list.Add(System.Windows.Forms.Keys.G, hauling.Object == null ? GameObjectSlot.Empty : Ability.Throw);
        //    return list;
        //}

        void Instance_TooltipBuild(object sender, TooltipArgs e)
        {
            GameObject actor = Player.Actor;
            if (actor == null)
                return;
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu))
            {
                PartyComponent party;
                if (Player.Actor.TryGetComponent<PartyComponent>("Party", out party))
                {
                    GameObjectSlot memberSlot = party.Members.FirstOrDefault();
                    if (memberSlot != null)
                        if (memberSlot.HasValue)
                            actor = memberSlot.Object;
                }
            }

            GroupBox box;
            GameObject obj = e.Tooltip.Tag as GameObject;// Controller.Instance.MouseoverLast as GameObject;
            if (obj == null)
                return;
            
            string text;// = "Available interactions for: \n" + actor.Name + "\n";
            foreach (Component comp in obj.Components.Values)
            {
                text = comp.GetWorldText(obj, actor);
                if (text.Length == 0)
                    continue;
                e.Tooltip.Controls.Add(new Label(new Vector2(0, e.Tooltip.Controls.Count > 0 ? e.Tooltip.Controls.Last().Bottom : 0), text));
            }

            List<InteractionOld> objActions = obj.Query(actor, Controller.Instance.Mouseover.Face);//obj, null, Controller.Instance.Mouseover.Face, DragDropManager.Instance.Item as GameObjectSlot);

            box = new GroupBox(e.Tooltip.Controls.Count > 0 ? e.Tooltip.Controls.Last().BottomLeft : Vector2.Zero);
            //Dictionary<AbilitySlot, GameObjectSlot> playerAbilities = GetAbilities(obj);// GetAbilities();// ControlComponent.GetAbility(actor); //LastAbilityUsed != null ? LastAbilityUsed.ToSlot() : Ability.Activate;// ; 
            //foreach (KeyValuePair<AbilitySlot, GameObjectSlot> ability in playerAbilities)
            //{
            //    if (ability.Value.Object == null)
            //        continue;
            //    Message.Types msg = (Message.Types)ability.Value.Object["Ability"]["Message"];
            //    Interaction match = objActions.Find(foo => foo.Message == msg);
            //    if (match == null)
            //        continue;
            //    Slot slot = new Slot(box.Controls.Count > 0 ? box.Controls.Last().BottomLeft : Vector2.Zero);
            //    slot.Tag = ability.Value;
            //    slot.SetBottomRightText(Ability.GetSlotText(ability.Key));
            //    box.Controls.Add(new Label(slot.CenterRight, match.ToString(actor), HorizontalAlignment.Left, VerticalAlignment.Center), slot);
            //}

          //  Dictionary<GameObjectSlot, System.Windows.Forms.Keys> playerAbilities = GetAbilitiesKeys();
            InteractionOld match = objActions.Find(foo => foo.Message == (Message.Types)LastAbilityUsed.Object["Ability"]["Message"]);
            if (match != null)
            {
                Slot slot = new Slot(box.Controls.Count > 0 ? box.Controls.Last().BottomLeft : Vector2.Zero);
                slot.Tag = LastAbilityUsed;
                slot.SetBottomRightText(GlobalVars.KeyBindings.Interact.ToString());//Ability.GetSlotText(ability.Key));
                box.Controls.Add(new Label(slot.CenterRight, match.ToString(actor), HorizontalAlignment.Left, VerticalAlignment.Center), slot);
            }

            if (box.Controls.Count > 0)
            {
                e.Tooltip.Controls.Add(box);
                
            }
            if(objActions.Count>0)
                e.Tooltip.Controls.Add(new Label(e.Tooltip.Controls.Last().BottomLeft, "Hold [" + GlobalVars.KeyBindings.Interact.ToString() + "] for \navailable interactions"));
            return;
            text = "";
            int oldLength = 0;

            //if(obj.Components.ContainsKey("Health"))
            //{
            //    //box.Controls.Add(new Label("Right click to attack"));
            //    text += "Right click to attack bitch";
            //}
            //if (obj.Components.ContainsKey("Tile"))
            //{
            //    //box.Controls.Add(new Label("Right click to attack bitch"));
            //    text += "this is a motherflippin tile";
            //}
            bool first = true;
            foreach (KeyValuePair<string, Component> comp in obj.Components)
            {
                string compText = comp.Value.GetTooltipText();
                if (compText.Length == 0)
                    continue;
                if (!first)
                    text += "\n";
                first = false;
                text += compText;
            }

            if (text.Length > 0)
            {
                int left = 0, bottom = 0;
                foreach (GroupBox b in e.Tooltip.Controls)
                {
                    left = Math.Min(left, b.Left);
                    bottom = Math.Max(left, b.Bottom);
                }
                //Console.WriteLine(left + " " + bottom);
                box = new GroupBox(new Vector2(left, bottom));
                box.Controls.Add(new Label(text));
                e.Tooltip.Controls.Add(box);
            }

            
        }
        //GameObject Target;

        // TODO: make interaction result a separate object that carries the info of failure reasons etc
        Construction3Window ConstructionWindow;
        public void Player_MessageReceived(object sender, ObjectEventArgs e)
        {
            Window win;
            switch (e.Type)
            {
                case Message.Types.UIConstruction:
                    StructuresWindowOld.Instance.Show();
                    GameObject temp = GameObject.Create(GameObject.Types.Air);
                    //temp.Global = e.Sender.Global + (Vector3)e.Parameters[0];
                    throw new Exception("Obsolete position handling");

                    //inter.Source = temp;
                    StructuresWindowOld.Instance.Tag = e.Sender.Global + (Vector3)e.Parameters[0];//new GameObjectEventArgs(Message.Types.BeginInteraction, null, inter, face);//// inter;
                    return;

                case Message.Types.Dialogue:
                    throw new NotImplementedException();
                    //GameObject.PostMessage(Actor, Message.Types.Speak, Actor, "I need a favor...", e.Parameters.Cast<string>().ToArray());
                    return;

                case Message.Types.Speak:
                    // TODO: code for broadcasting speech to nearby objects
                    // todo: only create speechbubble on clients
                    Log.Command(Log.EntryTypes.Chat, Actor, (string)e.Parameters[0]);
                    return;

                case Message.Types.Build:
                    //Console.WriteLine(DateTime.Now + " WRAIA");
                    //Rooms.Ingame.Instance.WindowManager.ToggleSingletonWindow<CraftingWindow>();

                    //CraftingWindow.Instance.Show();
                    ConstructionWindow = new Construction3Window(e.Sender.Global, e.Sender, e.Parameters[0] as GameObject);
                    ConstructionWindow.Show();
                    ConstructionWindow.ConstructionSelected += new EventHandler<BlueprintEventArgs>(ConstructionWindow_ConstructionSelected);
                    ConstructionWindow.Hidden += new EventHandler<EventArgs>(ConstructionWindow_Closed);
                    //(win as Construction3Window).SelectedItemChanged += new EventHandler<EventArgs>(Player_SelectedItemChanged);
                    //(new Construction3Window(e.Sender as GameObject)).Show();
                    return;
                //case Message.Types.Craft:
                case Message.Types.Interface:
                    //GameObject source = e.Parameters[0] as GameObject;
                    //source.GetUi().Show();

                    TargetArgs source = e.Parameters[0] as TargetArgs;
                    source.Object.GetUi().Show();
                    return;

                case Message.Types.UISetBlueprint:
                    // Target = e.Sender;
                    CraftingWindow.Instance.Initialize(e.Sender);//["Container"].GetProperty<List<GameObjectSlot>>("Slots").ToList());
                    CraftingWindow.Instance.Show();
                    //CraftingWindow.Instance.SelectedItemChanged += new EventHandler<EventArgs>(CraftingWindow_SelectedItemChanged);
                    //CraftingWindow.Instance.Hidden += new EventHandler<EventArgs>(CraftingWindow_Hidden);
                    CraftingWindow.Instance.Tag = e.Sender;
                    return;
                case Message.Types.UIJobBoard:
                    //JobBoardWindow.Instance.Initialize(e.Sender).Show();
                    JobBoardWindow.Instance.Show();
                    return;
                case Message.Types.UIContainer:
                    //if (ContainerWindows.ContainsKey(e.Sender))
                    //{
                    //    e.Sender.HandleMessage(new GameObjectEventArgs(Message.Types.ContainerClose, Player.Actor));
                    //    return;
                    //}
                    win = (new ContainerWindow(e.Sender as GameObject));
                    win.Show();
                    ContainerWindows.Add(e.Sender, win);
                    win.Hidden += new EventHandler<EventArgs>(win_Closed);
                    return;
                case Message.Types.ContainerClose:
                    if (!ContainerWindows.TryGetValue(e.Sender, out win))
                        return;
                    ContainerWindows.Remove(e.Sender);
                    win.Close();
                    throw new NotImplementedException();
                    //e.Sender.PostMessage(Message.Types.ContainerClose, Player.Actor);
                    return;
                case Message.Types.UIOwnership:
                    OwnershipWindow.Instance.Tag = e.Sender;
                    OwnershipWindow.Instance.Filter = e.Parameters[0] as Predicate<GameObject>;
                    OwnershipWindow.Instance.Show();
                    //   ObjectInterfaceWindows.Add(NpcWindow.Instance);
                    return;

                //case Message.Types.BeginInteraction:
                //    Interaction inter = e.Parameters[0] as Interaction;
                //    if (!inter.Range(Player.Actor, inter.Source))
                //        return;
                //    return;

                case Message.Types.ManageEquipmentOk:
                    new InvWindow().Initialize(e.Sender).Show();
                    return;
                case Message.Types.Followed:
                    //Hud.Instance.AddUnitFrame(e.Sender);
                    Rooms.Ingame.Instance.Hud.AddUnitFrame(e.Sender);
                    return;
                case Message.Types.Unfollowed:
                    //Hud.Instance.RemoveUnitFrame(e.Sender);
                    Rooms.Ingame.Instance.Hud.RemoveUnitFrame(e.Sender);

                    //   Hud.Instance.Initialize(Actor);
                    return;
                //case Message.Types.Receive:
                //    BuildWindow.Instance.Refresh(e.Parameters[0] as GameObjectSlot);
                case Message.Types.ChangeOrientation:
                    //Rooms.Ingame.Instance.ToolManager.ActiveTool = OrientationTool;
                    //OrientationTool.Object = e.Sender;
                    return;


                //case Message.Types.Receive:
                //    GameObjectSlot objSlot = e.Parameters[0] as GameObjectSlot;
                //    GameObject obj = objSlot.Object;
                //    if (obj["Physics"].GetProperty<int>("Size") > 0)
                //    {
                //        float str = StatsComponent.GetStat(Player.Actor, Stat.Strength.Name);
                //        float weight = (float)obj["Physics"]["Weight"];
               
                //    }
                //    throw new NotImplementedException();
                //    //GameObject.PostMessage(Actor, Message.Types.UpdateAbilities);
                //    return;

                case Message.Types.Hold:
                case Message.Types.Throw:
                case Message.Types.Dropped:
                    //throw new NotImplementedException();
                    //GameObject.PostMessage(Actor, Message.Types.UpdateAbilities);
                    return;
                case Message.Types.UpdateAbilities:
                    ActionBar.Instance.Refresh();
                    //InvWindow.Initialize(Actor);
                   // InvWindow.Show(Actor);
                   // InvWindow.Invalidate(Actor);
                    return;


                //case Message.Types.SkillAward:
                //    //Hud.Instance.Controls.Add(new FloatingText(Actor, Skill.SkillRegistry.Find(sk => sk.ID == (Skill.Types)e.Parameters[0]).Name + " experience gained (" + (int)e.Parameters[1] + ")"));
                //    Skill skill = Skill.SkillRegistry.Find(sk => sk.ID == (Skill.Types)e.Parameters[0]);
                //    int amount = (int)e.Parameters[1];
                //    Log.Enqueue(Log.EntryTypes.Default, Skill.SkillRegistry.Find(sk => sk.ID == (Skill.Types)e.Parameters[0]).Name + " experience gained (" + amount + ")");
                //    FloatingText.Create(Player.Actor, amount.ToString("+#;-#;0") + " " + skill.Name);
                //    return;

                default:
                    return;
            }
        }

        void CraftingWindow_Hidden(object sender, EventArgs e)
        {
            CraftingWindow.Instance.SelectedItemChanged -= CraftingWindow_SelectedItemChanged;
            CraftingWindow.Instance.Tag = null;
        }

        void CraftingWindow_SelectedItemChanged(object sender, EventArgs e)
        {
            if (CraftingWindow.Instance.Tag != null)
                throw new NotImplementedException();
                //(CraftingWindow.Instance.Tag as GameObject).PostMessage(Message.Types.SetBlueprint, Player.Actor, CraftingWindow.SelectedItem);
            else
                Log.Enqueue(Log.EntryTypes.System, "CraftingWindow.Tag is null!");

        }

        void ConstructionWindow_ConstructionSelected(object sender, BlueprintEventArgs e)
        {
            if( ConstructionWindow.Construction == null)
                return;
            throw new NotImplementedException();
            //ConstructionWindow.Construction.PostMessage(Message.Types.SetBlueprint, Player.Actor, e.Blueprint, e.Variation, e.Orientation);
            ConstructionWindow.ConstructionSelected -= ConstructionWindow_ConstructionSelected;
        }

        void ConstructionWindow_Closed(object sender, EventArgs e)
        {
            ConstructionWindow = null;
        }


        void win_Closed(object sender, EventArgs e)
        {
            ContainerWindow win = sender as ContainerWindow;
            ContainerWindows.Remove(win.Container);
            win.Container.PostMessage(new ObjectEventArgs(Message.Types.ContainerClose, Player.Actor));
            win.Hidden -= win_Closed;
        }
        Dictionary<GameObject, Window> ContainerWindows = new Dictionary<GameObject, Window>();
        List<Window> ObjectInterfaceWindows = new List<Window>();
        //void Actor_Movement(object sender, MovementArgs e)
        //{
        //    if (e.LastCell.Chunk != e.NextCell.Chunk)
        //        OnChunkChanged();
        //}

        //public Player(int x, int y)
        //{
        //    ChunkLoader.Request(new Vector2(0, 0), ChunkLoaded2);
        //   // Instance = this;
        //    Selection = new Selection();
        //    NextSelection = new Selection();
        //}

        Player()
        {
            this.HotBar = new UI.HotBar();
            // TODO: Complete member initialization
            Log.Instance.EntryAdded += new EventHandler<LogEventArgs>(Log_EntryAdded);

            OrientationTool = new ObjectOrientationTool2();
            OrientationTool.MouseLeft += new EventHandler<EventArgs>(OrientationTool_MouseLeft);

        //    TooltipManager.Instance.TooltipBuild += new EventHandler<TooltipArgs>(Instance_TooltipBuild);

        }

        

        //void Instance_SelectedItemChanged(object sender, EventArgs e)
        //{
        //    Interaction inter = ContextMenu.Instance.SelectedItem as Interaction;
        //    Player.Actor.HandleMessage(Message.Types.BeginInteraction, null, inter);
        // //   JobBoardComponent.JobList.Add(
        //    ContextMenu.Instance.Toggle();
        //}

        void OrientationTool_MouseLeft(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            if (ToolManager.Instance.ActiveTool == null)
                ToolManager.Instance.ActiveTool = new DefaultTool();
            if (ConstructionWindow != null)
                if (!Start_a_Town_.PlayerControl.DefaultTool.CanReach(Player.Actor, ConstructionWindow.Construction))
                {
                    ConstructionWindow.Hidden -= ConstructionWindow_Closed;
                    ConstructionWindow.Close();
                    ConstructionWindow = null;
                }

            if (CraftingWindow.Instance.Tag != null)
                if (!Start_a_Town_.PlayerControl.DefaultTool.CanReach(Player.Actor, CraftingWindow.Instance.Tag as GameObject))
                    CraftingWindow.Instance.Hide();

            foreach (Window window in ObjectInterfaceWindows.ToList())
            {
                if (Vector3.Distance(Actor.Global, (window.Tag as GameObject).Global) >= 2)
                {
                    //window.Tag = null;
                    ObjectInterfaceWindows.Remove(window);
                    window.Hide();
                }
            }
        }

        //public void Update()
        //{
        //    //Selection.Copy(NextSelection, Selection);
        //    //NextSelection.Clear();
        //    Selection = NextSelection;
        //    NextSelection = new Selection();
        //}
        //static public Player Instance;
        //void OnChunkChanged()
        //{
        //    if (ChunkChanged != null)
        //        ChunkChanged(this, EventArgs.Empty);
        //}
        //public void ChunkLoaded(Chunk chunk)
        //{
        //    //chunk.Validation += new EventHandler<EventArgs>(chunk_Validation);
        //    chunk.HeightMapCalculated+=new EventHandler<EventArgs>(chunk_HeightMapCalculated);
        //}

        //void chunk_HeightMapCalculated(object sender, EventArgs e)


        //public void Control(GameObject actor)
        //{
        //    Control(Engine.Map, actor);
        //}
        //public void Control(Map map, GameObject actor)
        //{
        //    if (Actor != null)
        //        Actor.GetComponent<ControlComponent>("Control").MessageReceived -= Player_MessageReceived;

        //    Actor = actor;
        //    Hud.Instance.Initialize(Actor);

        //    // TODO: bug: when loading chunks from files, chunks are requested twice from chunkloader
        //    PositionComponent posComp = Actor.Transform;
        //    Position pos;
        //    Vector3 global = new Vector3(map.Global, 0);
        //        global = actor.Global;

        //    ActionBar.Instance.Refresh();

        //    //  ChunkLoader.NotifyChunkLoaded(map, Chunk.GetChunkCoords(global), ChunkLoaded);
        //}
        static public GameObjectSlot LastAbilityUsed = GameObjectSlot.Empty; // Ability.Activate;//
        //void ContextMenu_SelectedItemChanged(object sender, EventArgs e)
        //{
        //    if (Player.Actor == null)
        //        return;
        //    ContextMenu menu = sender as ContextMenu;
        //    Interaction inter = menu.SelectedItem as Interaction;
        //    //  LastInteraction = inter.Message;
        //    Vector3 face = (ContextMenu.Instance.Tag as Mouseover).Face;


        //    if (Controller.Input.GetKeyDown(System.Windows.Forms.Keys.LMenu))
        //    {
        //        JobBoardComponent.Enqueue(Job.Create(inter));
        //        JobBoardWindow.Instance.RefreshPostedJobs();
        //    }
        //    else
        //    {
        //        //Actor.HandleMessage(Message.Types.BeginInteraction, null, inter);
        //        GameObjectSlot abilitySlot;
        //        //if (ControlComponent.HasAbility(Actor, inter.Message))
        //        if (ControlComponent.TryGetAbility(Actor, inter.Message, out abilitySlot))
        //        {
        //            LastAbilityUsed = abilitySlot;
        //            ActionBar.Instance.Refresh();

        //        }
        //        GameObject.PostMessage(Actor, new GameObjectEventArgs(Message.Types.BeginInteraction, null, inter, face, Actor["Inventory"]["Holding"] as GameObjectSlot));
        //    }
        //}

        void StructuresWindow_BuildClick(object sender, EventArgs e)
        {
            GameObject bpObj = StructuresWindowOld.Instance.SelectedItem as GameObject;
            Blueprint bp = bpObj["Blueprint"]["Blueprint"] as Blueprint;

            GameObject obj = GameObjectDb.Construction;
            throw new NotImplementedException();
            //obj.PostMessage(Message.Types.SetBlueprint, Actor, bp, (int)bpObj["Sprite"]["Variation"], (int)bpObj["Sprite"]["Orientation"]);

            throw new Exception("Obsolete position handling");
            //obj.Spawn(Player.Actor.Map, obj.Global);
            

            // TODO: don't send a spawn message in order to keep it as a ghost object until the first material is applied
            StructuresWindowOld.Instance.Hide();
            BuildWindow.Instance.Show();
        }

        void StructuresWindow_SelectedItemChanged(object sender, EventArgs e)
        {
            StructuresWindowOld.BuildClick -= StructuresWindow_SelectedItemChanged;
            if (StructuresWindowOld.Instance.Tag == null)
                return;
            StructuresWindowOld.Instance.Hide();

            ObjectEventArgs args = StructuresWindowOld.Instance.Tag as ObjectEventArgs;
            object[] p = new object[args.Parameters.Length + 1];
            args.Parameters.CopyTo(p, 0);
            p[args.Parameters.Length] = StructuresWindowOld.Instance.SelectedItem as GameObject;
            args.Parameters = p;

            GameObject bpObj = StructuresWindowOld.Instance.SelectedItem as GameObject;
            Blueprint bp = bpObj["Blueprint"]["Blueprint"] as Blueprint;

            GameObject obj = GameObjectDb.Construction;
            throw new NotImplementedException();
            //obj.PostMessage(Message.Types.SetBlueprint, Actor, bp, (int)bpObj["Sprite"]["Variation"], (int)bpObj["Sprite"]["Orientation"]);

            throw new Exception("Obsolete position handling");

            return;

            Actor.PostMessage(args);
        }

        //void ContextMenu_ContextMenuInit(object sender, EventArgs e)
        //{
        //    //ScrollableList list = (sender as ContextMenu).Controls.First() as ScrollableList;
        //    ControlCollection controls = (sender as ContextMenu).Controls.First().Controls;
        //    foreach (UI.Control control in controls)
        //    {
        //        //add a button to each control to create a job instead, or create a single button that changes position depending on which button the mouse is over
        //    }
        //}

        public void Dispose()
        {
            Actor = null;
            TooltipManager.Instance.TooltipBuild -= Instance_TooltipBuild;
            //ContextMenu.Instance.ContextMenuInit -= ContextMenu_ContextMenuInit;
            //ContextMenu.Instance.SelectedItemChanged -= ContextMenu_SelectedItemChanged;
        }

    }
}
