using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using System;
using System.Linq;

namespace Start_a_Town_
{
    class InventoryUI : GroupBox
    {
        static InventoryUI Instance;
        EquipmentUI EquipmentSlots;
        static readonly int LineMax = 4;
        Panel PanelSlots;
        public InventoryUI()
        {
            this.PanelSlots = new Panel() { AutoSize = true };
            EquipmentSlots = new EquipmentUI();
        }
        public InventoryUI(Actor actor)
        {
            this.PanelSlots = new Panel() { AutoSize = true };
            this.InitInvSlots(actor);
        }
        public void Refresh(Actor actor)
        {
            this.InitInvSlots(actor);
            
        }
        private void InitInvSlots(Actor actor)
        {
            var Inventory = actor.GetComponent<PersonalInventoryComponent>();

            Container container = Inventory.Slots;

            Controls.Remove(this.PanelSlots);
            this.PanelSlots.Controls.Clear();
            for (int i = 0; i < container.Slots.Count; i++)
            {
                GameObjectSlot invSlot = container.Slots[i];
                int slotid = i; // must be here for correct variable capturing with anonymous method
                var slot = new InventorySlot(invSlot, actor, i)
                {
                    Location = new Vector2((i % LineMax) * UIManager.SlotSprite.Width, (i / LineMax) * UIManager.SlotSprite.Height),
                    ContextAction = (a) =>
                    {
                        GameObject obj = invSlot.Object;
                        if (obj == null)
                            return;
                        obj.GetInventoryContext(a, slotid);
                        return;

                        //var interactions = obj.Query(actor);

                        //a.Actions.AddRange(interactions.Select(inter => new ContextAction(() => inter.Name, (() =>
                        //{
                        //    //GameObject.PostMessage(Actor, new ObjectEventArgs(Message.Types.BeginInteraction, null, inter, Vector3.Zero, Actor["Inventory"]["Holding"] as GameObjectSlot));

                        //    return;// true;
                        //}))
                        //{
                        //    ControlInit = (ContextAction act, Button btn) =>
                        //    {
                        //        btn.TooltipFunc = tooltip => inter.GetTooltipInfo(tooltip);
                        //        btn.TextColorFunc = () =>
                        //        {
                        //            List<Condition> failed = new List<Condition>();
                        //            inter.TryConditions(actor, obj, failed);
                        //            return failed.Count > 0 ? Color.Red : Color.White;
                        //        };
                        //    }
                        //}));
                    },
                    DragDropAction = args =>
                    {
                        "npc inventory rearranging disabled".ToConsole();
                        return DragDropEffects.None;
                        var a = args as DragDropSlot;
                        if (a.Effects == DragDropEffects.None)
                            return DragDropEffects.None;
                        Net.Client.PlayerInventoryOperationNew(a.SourceTarget, new TargetArgs(invSlot), a.DraggedTarget.Slot.StackSize);
                        //Client.PlayerInventoryOperationNew(a.Source, invSlot, a.Slot.StackSize);
                        return DragDropEffects.Move;
                    },
                    LeftClickAction = () =>
                    {
                        UISelectedInfo.Refresh(invSlot.Object);
                    }
                };
                //if (invSlot.Object != null)
                //{
                //    var obj = invSlot.Object;
                //    var contextActions = obj.GetInventoryContextActions(actor);
                //    //slot.RightClickAction = () => ContextMenuManager.PopUp(new ContextAction("Drop", () => this.Drop(actor, obj)));
                //    slot.RightClickAction = () => ContextMenuManager.PopUp(contextActions.ToArray());
                //}
                
                slot.RightClickAction = () =>
                {
                    if (invSlot.Object != null)
                    {
                        var contextActions = invSlot.Object.GetInventoryContextActions(actor);
                        ContextMenuManager.PopUp(contextActions.ToArray());
                    }
                };
                

                //if (invSlot.Object != null)
                //{
                //    slot.RightClickAction = () => ContextMenuManager.PopUp(new ContextAction("Drop", () => this.Drop(actor, invSlot.Object)));
                //}
                this.PanelSlots.Controls.Add(slot);
            }
            this.AddControls(this.PanelSlots);

            var customizationClient = new GroupBox();
            //var uicolors = new Window(string.Format("Edit {0}", actor.Name), new UICharacterCustomization(actor)) { Movable = true, Closable = true };
            var colorsui = new UICharacterCustomization(actor);
            customizationClient.AddControls(colorsui);
            customizationClient.AddControlsBottomLeft(new Button("Apply", customizationClient.Width) { LeftClickAction = () => PacketEditAppearance.Send(actor, colorsui.Colors) });

            var uicolors = new Window(string.Format("Edit {0}", actor.Name), customizationClient) { Movable = true, Closable = true };


            var btncolors = new Button("Change colors") { Location = this.PanelSlots.BottomLeft, LeftClickAction = () => uicolors.SetLocation(UIManager.Mouse).Toggle() };
            this.AddControls(btncolors);
        }

        static public Control GetUI(Actor actor)
        {
            Window window;
            if (Instance == null)
            {
                Instance = new InventoryUI();
                window = new Window(Instance) { Closable = true, Movable = true };
                window.SnapToMouse();
            }
            else
                window = Instance.GetWindow();
            Instance.Tag = actor;
            window.Title = string.Format("{0} inventory", actor.Name);
            Instance.InitInvSlots(actor);

            Instance.RemoveControls(Instance.EquipmentSlots);
            Instance.EquipmentSlots.Refresh(actor);
            Instance.EquipmentSlots.Location = Instance.PanelSlots.TopRight;
            Instance.AddControls(Instance.EquipmentSlots);

            window.Validate(true);
            return window;
        }
        static InventoryUI GUI;
        static public Control GetGUI()
        {
            if(GUI == null)
            {
                GUI = new InventoryUI() { Name = "Inventory" };
                GUI.SetGetDataAction(o =>
                {
                    var actor = o as Actor;
                    GUI.InitInvSlots(actor);
                    GUI.RemoveControls(GUI.EquipmentSlots);
                    GUI.EquipmentSlots.Refresh(actor);
                    GUI.EquipmentSlots.Location = GUI.PanelSlots.TopRight;
                    GUI.AddControls(GUI.EquipmentSlots);
                });
            }
            return GUI;
        }
        //internal override void OnGameEvent(GameEvent e)
        //{
        //    switch (e.Type)
        //    {
        //        case Message.Types.SelectedChanged:
        //            var target = e.Parameters[0] as TargetArgs;
        //            //if(target.Object == this.Tag)
        //            //    break;
        //            var actor = target.Object as Actor;
        //            if (actor != null && this.Tag != actor)
        //                GetUI(actor);
        //            break;

        //        default:
        //            break;
        //    }
        //}
        internal override void OnSelectedTargetChanged(TargetArgs target)
        {
            var actor = target.Object as Actor;
            if (actor != null && this.Tag != actor)
                GetUI(actor);
        }
        private bool Drop(GameObject actor, GameObject item)
        {
            PacketInventoryDrop.Send(item.Net, actor.RefID, item.RefID, item.StackSize);
            return true;
        }
    }
}
