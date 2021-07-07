using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class InvWindow : Window
    {
        static Dictionary<GameObject, InvWindow> _OpenInventories;
        static Dictionary<GameObject, InvWindow> OpenInventories
        {
            get
            {
                if (_OpenInventories == null)
                    _OpenInventories = new Dictionary<GameObject, InvWindow>();
                return _OpenInventories;
            }
        }

        Panel Panel_Slots, Panel_Equipment, Panel_Details, Panel_Conditions, Panel_Tabs;
        GroupBox Box_RadioButtons, Box_Body, Box_Gear, Box_Stats;
        ScrollableBox Box_Skills;
        //GroupBox Box_Skills;
        RadioButton Rad_Body, Rad_Gear;
        Rectangle DragBoxFromMouseDown = Rectangle.Empty;
        PersonalInventoryComponent Inventory;
        public static int LineMax = 4;
        //ListView SlotList;
        GameObject Actor;
        Slot DropSlot;
        Slot HaulSlot;

        static public InvWindow Invalidate(GameObject obj)
        {
            InvWindow win;
            if (!OpenInventories.TryGetValue(obj, out win))
                return null;
            return (InvWindow)win.Invalidate(true);
        }

        static public InvWindow Show(GameObject obj)
        {
            
            InvWindow win;
            if (!OpenInventories.TryGetValue(obj, out win))
            {
                win = new InvWindow().Initialize(obj);
                OpenInventories[obj] = win;
            }
            win.Location = win.BottomRightScreen;
            win.Show();
            return win;
        }
        static public InvWindow Toggle(GameObject obj)
        {
            InvWindow win;
            if (!OpenInventories.TryGetValue(obj, out win))
            {
                win = new InvWindow().Initialize(obj);
                OpenInventories[obj] = win;
            }
            win.Location = win.BottomRightScreen;
            win.Toggle();
            win.Tag = obj;
            return win;
        }
        public override bool Hide()
        {
            OpenInventories.Remove(this.Actor);
            Log.Instance.EntryAdded -= Log_EntryAdded;
            //foreach (Control control in Panel_Tabs.Controls)
            //    control.MouseLeftPress -= tab_MouseLeftPress;
            return base.Hide();
        }

        public InvWindow Initialize(GameObject obj)
        {
            Actor = obj;
            if (Actor == null)
                return this;

            Title = Actor.Name;

            InitEquipment(Actor);
            InitTabs(Actor);
            InitStats(Actor);
            //InitSkills(Actor);

            InitInvSlots();
            Panel_Details.Tag = Actor.GetComponent<StatsComponent>("Stats");

            this.HaulSlot = new InventorySlot(Actor.GetComponent<HaulComponent>().GetSlot(), Actor) { Location = this.Client.Controls.BottomLeft };
            //this.HaulSlot = new Slot() { Location = this.Client.Controls.BottomLeft, Tag = Actor.GetComponent<HaulComponent>().GetSlot()};//.Slot };
            Label haulslotlabel = new Label(this.HaulSlot.TopRight, "Hauling");
            this.Client.Controls.Add(this.HaulSlot, haulslotlabel);

            this.DropSlot = new Slot(this.Client.Controls.BottomLeft)
            {
                DragDropAction = args =>
                {
                    var a = args as DragDropSlot;
                    //Client.PlayerDropInventory(a.Source.ID, a.Source.StackSize);
                    // use the index from ALL actor's children, or use index from the actor's backpack?
                    //var childIndex = (byte)Player.Actor.GetChildren().IndexOf(a.Source);//.First(slot => slot == a.Source).ID;
                    //Client.PlayerDropInventory(childIndex, a.Source.StackSize);
                    var childIndex = PlayerOld.Actor.GetChildren().IndexOf(a.SourceTarget.Slot);//.First(slot => slot == a.Source).ID;
                    if (childIndex == -1)
                        return DragDropEffects.None;
                    Net.Client.PlayerDropInventory((byte)childIndex, a.SourceTarget.Slot.StackSize);

                    
                    //Client.PostPlayerInput(Message.Types.DropInventoryItem, w =>
                    //{
                    //    w.Write((int)a.Source.ID);
                    //    w.Write(a.Source.StackSize);
                    //});
                    return DragDropEffects.Move;
                }
            };
            Label dropLabel = new Label(this.DropSlot.TopRight, "Drag item here to drop");
            //this.AutoSize = true;
            this.Client.Controls.Add(this.DropSlot, dropLabel);
            return this;
        }

        void Rad_Body_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        private void InitInvSlots()
        {
            Inventory = Actor.GetComponent<PersonalInventoryComponent>();

            Container container = Inventory.Slots;

            Controls.Remove(Panel_Slots);
            Panel_Slots.Controls.Clear();
            for (int i = 0; i < container.Slots.Count; i++)
            {
                GameObjectSlot invSlot = container.Slots[i];
                int slotid = i; // must be here for corrent variable capturing with anonymous method
                Panel_Slots.Controls.Add(new InventorySlot(invSlot, Actor, i)
                {
                    Location = new Vector2((i % LineMax) * UIManager.SlotSprite.Width, (i / LineMax) * UIManager.SlotSprite.Height),
                    ContextAction = (a) =>
                    {
                        GameObject obj = invSlot.Object;
                        if (obj is null)
                            return;

                        obj.GetInventoryContext(a, slotid);
                        //return;

                        //var interactions = obj.Query(Actor);

                        //a.Actions.AddRange(interactions.Select(inter => new ContextAction(() => inter.Name, (() =>
                        //{
                        //    //GameObject.PostMessage(Actor, new ObjectEventArgs(Message.Types.BeginInteraction, null, inter, Vector3.Zero, Actor["Inventory"]["Holding"] as GameObjectSlot));

                        //    return true;
                        //}))
                        //{
                        //    ControlInit = (ContextAction act, Button btn) =>
                        //    {
                        //        btn.TooltipFunc = tooltip => inter.GetTooltipInfo(tooltip);
                        //        btn.TextColorFunc = () =>
                        //        {
                        //            List<Condition> failed = new List<Condition>();
                        //            inter.TryConditions(Player.Actor, obj, failed);
                        //            return failed.Count > 0 ? Color.Red : Color.White;
                        //        };
                        //    }
                        //}));
                    },
                    DragDropAction = args =>
                    {
                        var a = args as DragDropSlot;
                        if (a.Effects == DragDropEffects.None)
                            return DragDropEffects.None;
                        //Client.PlayerInventoryOperation(Player.Actor, a.Source, invSlot, a.Slot.StackSize);
                        Net.Client.PlayerInventoryOperationNew(a.SourceTarget, new TargetArgs(invSlot), a.DraggedTarget.Slot.StackSize);

                        //Client.PlayerInventoryOperationNew(a.Source, invSlot, a.Slot.StackSize);

                        return DragDropEffects.Move;

                        //Client.PlayerInventoryOperationOld(Player.Actor, w =>
                        //{
                        //    ArrangeChildrenArgs.Write(w, new TargetArgs(a.Parent), new TargetArgs(a.Source.Object), new TargetArgs(a.Slot.Object), a.Source.ID, invSlot.ID, (byte)a.Slot.StackSize);
                        //});
                        //return DragDropEffects.Move;

                        //Client.PostPlayerInput(Message.Types.ArrangeChildren, w =>
                        //{
                        //    ArrangeChildrenArgs.Write(w, new TargetArgs(a.Parent), new TargetArgs(a.Source.Object), new TargetArgs(a.Slot.Object), a.Source.ID, invSlot.ID, (byte)a.Slot.StackSize);
                        //});
                        //return DragDropEffects.Move;


                        //Client.PostPlayerInput(Message.Types.ContainerOperation, w =>
                        //{
                        //    ContainerOperationArgs.Write(w, new TargetArgs(a.Parent), new TargetArgs(a.Slot.Object), a.Source.Container.ID, a.Source.ID, invSlot.Container.ID, invSlot.ID, (byte)a.Slot.StackSize);
                        //});
                        //return DragDropEffects.Move;


                        //Client.PostPlayerInput(Message.Types.ContainerOperation, w =>
                        //{
                        //    ArrangeInventoryEventArgs.Write(w, new TargetArgs(a.Parent), new TargetArgs(a.Source.Object), new TargetArgs(a.Slot.Object), invSlot.Container.ID, invSlot.ID, (byte)a.Slot.StackSize);
                        //});
                        //GameObjectSlot old = invSlot.Clone();
                        //// decide whether to chain dragdrop operations
                        //// TODO: HANDLE SLOT OVERFLOW
                        //if (old.HasValue)
                        //    if (old.Object.ID != a.Slot.Object.ID)
                        //    {
                        //        //(DateTime.Now.ToString() + "continue dragdrop").ToConsole();
                        //        GameObjectSlot clone = new GameObjectSlot(old.Object, old.StackSize);// Tag.Clone();
                        //        old.Clear();
                        //        DragDropManager.Create(new DragDropSlot(Actor, old, clone, DragDropEffects.Move | DragDropEffects.Link));
                        //        return DragDropEffects.None;
                        //    }
                        ////(DateTime.Now.ToString() + "stop dragdrop").ToConsole();
                        //return DragDropEffects.Move;
                    }
                });
            }
        }

        void InitTabs(GameObject actor)
        {
            Panel_Tabs.Controls.Clear();
            RadioButton tab;

            tab = new RadioButton("Attributes", Panel_Tabs.Controls.TopRight)
            {
                Tag = Box_Stats,
                Checked = true,
                LeftClickAction = () =>
                {
                    Panel_Details.Controls.Clear();
                    //ScrollableBox box = new ScrollableBox(Panel_Details.ClientSize);
                    //box.Controls.Add(actor["Attributes"].GetStats().ToLabel());
                    //Panel_Details.Controls.Add(box);
                    Panel_Details.Controls.Add(new Components.Attributes.AttributesInterface(actor.GetComponent<AttributesComponent>()));
                }
            };
            Panel_Tabs.Controls.Add(tab);
            tab.PerformLeftClick();
            tab = new RadioButton("Stats", Panel_Tabs.Controls.TopRight)
            {
                Tag = Box_Stats,
                //Checked = true,
                LeftClickAction = () => { Panel_Details.Controls.Clear(); Panel_Details.Controls.Add(Box_Stats); }
            };

            Panel_Tabs.Controls.Add(tab);

            //Panel_Tabs.Controls.Add(new RadioButton("Crafting")
            //{
            //    Location = Panel_Tabs.Controls.TopRight,
            //    Tag = CraftInterfaceNew.Instance,
            //    LeftClickAction = () => { Panel_Details.Controls.Clear(); Panel_Details.Controls.Add(CraftInterfaceNew.Instance); }
            //});

            Panel_Details.Location = Panel_Tabs.BottomLeft;
            Panel_Details.Conform((from btn in Panel_Tabs.Controls select btn.Tag as Control).ToArray());
        }
        void InitStats(GameObject entity)
        {
            Box_Stats.Controls.Clear();
            entity.GetComponent<StatsComponentNew>().GetUI(entity, Box_Stats, new List<EventHandler<GameEvent>>());
            //entity["Stats"].GetUI(entity, Box_Stats, new List<EventHandler<ObjectEventArgs>>());

            return;
        }
        //void InitSkills(GameObject actor)
        //{
        //    Box_Skills.Client.Controls.Clear();
        //    var ui = new Components.Skills.New.SkillsUI();
        //    ui.Refresh(actor);
        //    Box_Skills.Client.Controls.Add(ui);
        //    //this.Box_Skills = new Components.Skills.New.SkillsUI();
        //    return;

        //    //List<GameObjectSlot> skills = actor.GetComponent<SkillsComponent>().Skills.Select(sk => sk.ToObject().ToSlot()).ToList();// actor["Skills"].GetProperty<List<GameObjectSlot>>("Skills");
        //    //foreach (var skill in skills)
        //    //{
        //    //    Box_Skills.Add(new SlotWithText(Box_Skills.Client.Controls.BottomLeft) { Tag = skill });
        //    //}
        //}
        void InitConditions()
        {
            if (Panel_Conditions != null)
            {
                foreach (Control buff in Panel_Conditions.Controls)
                    buff.DrawTooltip -= buff_DrawTooltip;
                Panel_Conditions.Controls.Clear();

            }

            List<GameObject> conditions = (List<GameObject>)Actor["Conditions"]["Conditions"];
            int j = 0;

            foreach(GameObject cond in conditions)
            {
                Icon icon = cond["Gui"].GetProperty<Icon>("Icon");
                PictureBox buff = new PictureBox(new Vector2(0, j), icon.SpriteSheet, icon.SourceRect, HorizontalAlignment.Left, VerticalAlignment.Top);

                j += icon.SourceRect.Height;
                Panel_Conditions.Controls.Add(buff);
                buff.Tag = cond;
                buff.CustomTooltip = true;
                buff.DrawTooltip += new EventHandler<TooltipArgs>(buff_DrawTooltip);

            }

        }

        void buff_DrawTooltip(object sender, TooltipArgs e)
        {
            //((sender as PictureBox).Tag as GameObject).TooltipGroups(e.Tooltip);
            GameObject obj = (sender as PictureBox).Tag as GameObject;
            string text = obj["Condition"].ToString();
            e.Tooltip.Controls.Add(new Label(text.Remove(text.Length - 1)));
        }

        void InitEquipment(GameObject actor)
        {
            InitBody(actor);
            InitGear(actor);
        }
        void InitBody(GameObject actor)
        {
            Box_Body.Controls.Clear();

            BodyComponent body = actor.GetComponent<BodyComponent>("Equipment");
            //Dictionary<string, object> bodyParts = body.Properties;
            int n = 0;

            foreach (var bodypart in body.BodyParts)
            {
                BodyPart bdprt = bodypart.Value as BodyPart;
                Slot bdSlot = new Slot(new Vector2(0, (n++) * Slot.DefaultHeight));

                bdSlot.DrawTooltip += new EventHandler<TooltipArgs>(slot_DrawTooltip);

                bdSlot.CustomTooltip = true;

                Label label = new Label(new Vector2(bdSlot.Right, bdSlot.Top + bdSlot.Height / 2), bodypart.Key, HorizontalAlignment.Left, VerticalAlignment.Center);

                bdSlot.Tag = bdprt.Base;
         //       bdSlot.MouseRightUp += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(eqSlot_MouseRightUp);
                Box_Body.Controls.Add(bdSlot, label);
            }
        }

        void InitGear(GameObject actor)
        {
            Box_Gear.Controls.Clear();

            BodyComponent body = actor.GetComponent<BodyComponent>("Equipment");
            //Dictionary<string, object> bodyParts = body.Properties;
            int n = 0;
            //foreach (var gear in actor.GetComponent<GearComponent>().EquipmentSlots)
            foreach (var gear in actor.GetComponent<GearComponent>().Equipment.Slots)
            {
                //Slot bdSlot = new Slot(new Vector2(0, (n++) * Slot.DefaultHeight))
                //{
                //    RightClickAction = () =>
                //    {
                //        //Client.PlayerUnequip(new TargetArgs(gear.Value));
                //        Client.PlayerUnequip(new TargetArgs(gear));
                //    }
                //};
                var bdSlot = new SlotEquipment(gear, actor, gear.ID) { Location = new Vector2(0, (n++) * Slot.DefaultHeight) };
                //bdSlot.DrawTooltip += new EventHandler<TooltipArgs>(slot_DrawTooltip);
                //bdSlot.CustomTooltip = true;

                Label label = new Label(new Vector2(bdSlot.Right, bdSlot.Top + bdSlot.Height / 2), gear.Name, HorizontalAlignment.Left, VerticalAlignment.Center);

                //bdSlot.Tag = gear.Value;// bdprt.Wearing;
                //bdSlot.Tag = gear;// bdprt.Wearing;
                Box_Gear.Controls.Add(bdSlot, label);
            }

            //Slot holdSlot = new Slot(new Vector2(0, (n++) * Slot.DefaultHeight))
            //{
            //    RightClickAction = () =>
            //    {
            //        Client.PostPlayerInput(Message.Types.StoreCarried);
            //    }
            //}.SetTag(actor.GetComponent<GearComponent>().Holding);
            //;

            //Box_Gear.Controls.Add(holdSlot, new Label(holdSlot.CenterRight, "Holding", HorizontalAlignment.Left, VerticalAlignment.Center));
        }

       // void InitGear(GameObject actor)
       // {
       //     //foreach (Slot slot in Box_Gear.Controls.FindAll(foo => foo is Slot))
       //     //    slot.MouseRightUp -= eqSlot_MouseRightUp;
           
       //     Box_Gear.Controls.Clear();


       //     BodyComponent body = actor.GetComponent<BodyComponent>("Equipment");
       //     //Dictionary<string, object> bodyParts = body.Properties;
       //     int n = 0;

       //     foreach (var pair in body.BodyParts)
       //     {
       //         BodyPart bdprt = pair.Value as BodyPart;
       //         Slot bdSlot = new Slot(new Vector2(0, (n++) * Slot.DefaultHeight))
       //         {
       //             RightClickAction = () =>
       //             {
       //                 //Client.PostPlayerInput(Message.Types.Unequip, w => w.Write(inventorySlotID));
       //                 Client.PostPlayerInput(Message.Types.Unequip, w => w.Write(pair.Key));
       //             }
       //         };

       //         bdSlot.DrawTooltip += new EventHandler<TooltipArgs>(slot_DrawTooltip);
       //         bdSlot.CustomTooltip = true;

       //         Label label = new Label(new Vector2(bdSlot.Right, bdSlot.Top + bdSlot.Height / 2), pair.Key, HorizontalAlignment.Left, VerticalAlignment.Center);

       //         bdSlot.Tag = bdprt.Wearing;
       //     //    bdSlot.MouseRightUp += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(eqSlot_MouseRightUp);
       //         Box_Gear.Controls.Add(bdSlot, label);
       //     }

       //     Slot holdSlot = new Slot(new Vector2(0, (n++) * Slot.DefaultHeight))
       //     {
       //         RightClickAction = () =>
       //         {
       //             Client.PostPlayerInput(Message.Types.StoreCarried);
       //         }
       //     }.SetTag(actor.GetComponent<GearComponent>().Holding);
       //     ;
       ////     holdSlot.MouseRightUp +=new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(eqSlot_MouseRightUp);
       //     Box_Gear.Controls.Add(holdSlot, new Label(holdSlot.CenterRight, "Holding", HorizontalAlignment.Left, VerticalAlignment.Center));

       // }

        void InitBody()
        {
            foreach (Slot slot in Panel_Equipment.Controls.FindAll(foo => foo is Slot))
            {
           //     slot.MouseRightUp -= eqSlot_MouseRightUp;
                Box_RadioButtons.Controls.Remove(slot);
            }

            BodyComponent body = Actor.GetComponent<BodyComponent>("Equipment");

            Dictionary<string, object> bodyParts = body.Properties;
            int n = 0;

            foreach (KeyValuePair<string, object> pair in bodyParts)
            {
                BodyPart bdprt = pair.Value as BodyPart;
                Slot eqSlot = new Slot(new Vector2(0, (n++) * Slot.DefaultHeight));
                eqSlot.DrawTooltip += new EventHandler<TooltipArgs>(slot_DrawTooltip);
                eqSlot.CustomTooltip = true;
                Label label = new Label(new Vector2(eqSlot.Right, eqSlot.Top + eqSlot.Height / 2), pair.Key, HorizontalAlignment.Left, VerticalAlignment.Center);

                eqSlot.Tag = Rad_Body.Checked ? bdprt.Base : bdprt.Wearing;

              //  eqSlot.MouseRightUp += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(eqSlot_MouseRightUp);
                Box_RadioButtons.Controls.Add(eqSlot, label);
            }
        }

        //void eqSlot_MouseRightUp(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    Slot uiSlot = (sender as Slot);
        //    uiSlot.Alpha = Color.White;
        //    GameObjectSlot slot = (sender as Slot).Tag as GameObjectSlot;
        //    if (slot == null)
        //        return;
        //    if (slot.Object == null)
        //        return;
        //    throw new NotImplementedException();
        //    //GameObject.PostMessage(Actor, Message.Types.Receive, null, slot, slot.Object);
        //    //GameObject.PostMessage(Actor, Message.Types.Dropped, null, GameObjectSlot.Empty, null);

        //    return;
        //}

        void slot_MouseRightUp(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            Slot uiSlot = (sender as Slot);
            uiSlot.Alpha = Color.White;
            GameObjectSlot slot = (sender as Slot).Tag as GameObjectSlot;
            if (slot == null)
                return;
            if (slot.Object == null)
                return;
            throw new NotImplementedException();
            ////Actor.PostMessage(Message.Types.Hold, null, slot, slot.Object);
            //return;
        }

        void slot_DrawTooltip(object sender, TooltipArgs e)
        {
            GameObjectSlot slot = (sender as Slot).Tag as GameObjectSlot;
            GameObject obj = slot.Object as GameObject;
            if (obj == null)
                return;

            slot.GetTooltipInfo(e.Tooltip);
            float bottom = 0;
            foreach (Control control in e.Tooltip.Controls)
            {
                bottom = Math.Max(bottom, control.Bottom);
            }
            if (PlayerOld.Actor == null)
                return;

            foreach (EntityComponent comp in obj.Components.Values)
            {
                string invText = comp.GetInventoryText(obj, PlayerOld.Actor);
                if (invText.Length == 0)
                    continue;
                Label label = new Label(new Vector2(0, e.Tooltip.ClientSize.Bottom), invText);
                GroupBox box = new GroupBox();
                box.Controls.Add(label);
                e.Tooltip.Controls.Add(box);
            }
            if (slot.StackSize > 1)
                e.Tooltip.Controls.Add(new Label(new Vector2(0, e.Tooltip.Controls.Last().Bottom), "Shift+Left click: Split"));

        }

        void slot_KeyPress(object sender, KeyPressEventArgs2 e)
        {
            Slot slot = sender as Slot;
            if (e.Key == GlobalVars.KeyBindings.QuickSlot1)
                PlayerOld.Instance.Tool.Hotbar[0] = slot.Tag as GameObjectSlot;
            else if (e.Key == GlobalVars.KeyBindings.QuickSlot2)
                PlayerOld.Instance.Tool.Hotbar[1] = slot.Tag as GameObjectSlot;
        }

        void slot_MouseRightPress(object sender, EventArgs e)
        {
            Slot uiSlot = (sender as Slot);
            uiSlot.Alpha = Color.Gold;

            GameObjectSlot slot = (sender as Slot).Tag as GameObjectSlot;
            if (slot == null)
                return;
            if (slot.Object == null)
                return;
        }

        //void slot_MouseMove(object sender, EventArgs e)
        //{
        //    if (Controller.Instance.msCurrent.LeftButton == ButtonState.Pressed)
        //        if (DragBoxFromMouseDown != Rectangle.Empty && !DragBoxFromMouseDown.Contains(new Rectangle(Controller.Instance.msCurrent.X, Controller.Instance.msCurrent.Y, 1, 1)))
        //        {
        //            Slot slot = sender as Slot;
        //            DragDropManager.Create(slot.Tag, slot, DragDropEffects.Move | DragDropEffects.Link);
        //            DragBoxFromMouseDown = Rectangle.Empty;
        //        }
        //}

        public InvWindow()  
            : base()
        {
            Title = "Inventory";
            Movable = true;
            AutoSize = true;
            Anchor = new Vector2(1, 1);

            Log.Instance.EntryAdded += new EventHandler<LogEventArgs>(Log_EntryAdded);

            Box_Body = new GroupBox();
            Box_Body.AutoSize = true;
            
            Box_Gear = new GroupBox();
            Box_Gear.AutoSize = true;

            Rad_Gear = new RadioButton("Gear", Vector2.Zero);
            Rad_Gear.CheckedChanged += new EventHandler<EventArgs>(Rad_Gear_CheckedChanged);
            Rad_Gear.Tag = Box_Gear;
            Rad_Body = new RadioButton("Body", Rad_Gear.TopRight);
            Rad_Body.Checked = true;
            Rad_Body.CheckedChanged += new EventHandler<EventArgs>(Rad_Gear_CheckedChanged);
            Rad_Body.Tag = Box_Body;
            Box_RadioButtons = new GroupBox();
            Box_RadioButtons.AutoSize = true;
            Box_RadioButtons.Controls.Add(Rad_Gear, Rad_Body);

            Panel_Equipment = new Panel(Box_RadioButtons.BottomLeft, new Vector2(4 * UIManager.SlotSprite.Width, 8 * Slot.DefaultHeight));
            Panel_Equipment.Controls.Add(Box_Body);
            Panel_Equipment.ClientSize = new Rectangle(0, 0, 4 * UIManager.SlotSprite.Width, 8 * Slot.DefaultHeight);

            Panel_Tabs = new Panel() { Location = new Vector2(Panel_Equipment.TopRight.X, Box_RadioButtons.Top), AutoSize = true };//  ClientDimensions = new Vector2(Panel_Equipment.ClientDimensions.X, Label.DefaultHeight) };//   new Vector2(Panel_Equipment.Right, 0), new Vector2(Panel_Equipment.Width, Button.DefaultHeight));
            Panel_Tabs.BackgroundStyle = BackgroundStyle.TickBox;

            //Panel_Details = new Panel() { Location = Panel_Tabs.BottomLeft, Dimensions = new Vector2(200, 300) };
            Panel_Details = new Panel() { Location = Panel_Tabs.BottomLeft };//, AutoSize = true};//, Dimensions = new Vector2(400, 300) }; 
            Box_Stats = new GroupBox();
            Box_Skills = new ScrollableBox(Panel_Details.ClientSize);

            Panel_Conditions = new Panel(new Vector2(Panel_Tabs.Right, 0));
            Panel_Conditions.ClientSize = new Rectangle(0, 0, 32, Panel_Conditions.ClientSize.Height);

            Panel_Slots = new Panel() { Location = Panel_Equipment.BottomLeft };// new Vector2(0, Panel_Equipment.Bottom)) { AutoSize = true, Color = Color.Black };
            Panel_Slots.ClientSize = new Rectangle(0, 0, 4 * UIManager.SlotSprite.Width, 4 * UIManager.SlotSprite.Width);

            Client.Controls.Add(Box_RadioButtons, Panel_Equipment, Panel_Details, Panel_Slots, Panel_Tabs); //,Panel_Conditions

            this.Location = BottomRightScreen;
            
        }
        //public override void Update()
        //{
        //    base.Update();
        //}
        void Rad_Gear_CheckedChanged(object sender, EventArgs e)
        {
            Panel_Equipment.Controls.Clear();
            Panel_Equipment.Controls.Add((sender as RadioButton).Tag as GroupBox);
        }

        void Log_EntryAdded(object sender, LogEventArgs e)
        {
            if (e.Entry.Type == Log.EntryTypes.Equip ||
                e.Entry.Type == Log.EntryTypes.Unequip ||
                e.Entry.Type == Log.EntryTypes.Buff ||
                e.Entry.Type == Log.EntryTypes.Debuff ||
                e.Entry.Type == Log.EntryTypes.Skill)
            {
                InitStats(Tag as GameObject);//Panel_Stats.Tag as Component);
                InitConditions();
            }
        }

        //public override bool Close()
        //{
        //    Log.Instance.EntryAdded -= Log_EntryAdded;
        //    foreach (Control control in Panel_Tabs.Controls)
        //        control.MouseLeftPress -= tab_MouseLeftPress;
        //    return base.Close();
        //}

    }
}
