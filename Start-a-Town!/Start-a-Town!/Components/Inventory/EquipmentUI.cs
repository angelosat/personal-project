using Start_a_Town_.UI;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class EquipmentUI : GroupBox
    {
        Panel PanelList;
        ListBoxNew<Entity, Button> GearList;
        const int MaxRows = 8;
        Actor Actor;
        public EquipmentUI()
        {
            this.PanelList = new Panel() { AutoSize = true };
            this.GearList = new ListBoxNew<Entity, Button>(200, Button.DefaultHeight * MaxRows);
            this.PanelList.AddControls(this.GearList);
            this.AddControls(PanelList);
        }

        public void Refresh(Actor actor)
        {
            this.Actor = actor;
            this.GearList.Clear();
            if (actor == null)
                return;
            var gear = actor.GetGear();
            this.GearList.AddItems(gear, (System.Func<Entity, Button>)(o =>
            {
                var btn = ButtonHelper.CreateFromItemCompact(o);
                btn.LeftClickAction = () => {
                    ContextMenuManager.PopUp(new ContextAction(() => "Unequip", (System.Action)(() =>
                    {
                        PacketInventoryEquip.Send((INetwork)actor.Net, actor.RefID, o.RefID);
                    })));
                };
                return btn;
            }));
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.ActorGearUpdated:
                    var actor = e.Parameters[0] as Actor;
                    if (actor == this.Actor)
                    {
                        var newItem = e.Parameters[1] as Entity;
                        var prevItem = e.Parameters[2] as Entity;
                        this.GearList.AddItems(newItem);
                        this.GearList.RemoveItem(prevItem);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
