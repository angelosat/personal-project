using System.Collections.Generic;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    class UIPlayerList : GroupBox
    {
        ListBoxNew<PlayerData, Button> List_Players;

        public UIPlayerList(IEnumerable<PlayerData> pList)
        {
            this.AutoSize = true;
            List_Players = new ListBoxNew<PlayerData, Button>(new Rectangle(0, 0, 150, 300));
            this.List_Players.MouseThrough = true;
            this.Refresh(pList);
            this.AddControls(List_Players);
        }

        public UIPlayerList Refresh(IEnumerable<PlayerData> pList)
        {
            List_Players.Build(pList, foo => foo.Name, (foo, ctrl) =>
            {
                ctrl.TextColorFunc = () => foo.Color;
                ctrl.OnUpdate = () =>
                {
                    ctrl.Text = foo.ID + ": " + foo.Name + " " + foo.Ping.ToString("###0ms");
                };
            });
            return this;
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.PlayerConnected:
                    this.List_Players.AddItem(e.Parameters[1] as PlayerData);
                    break;

                case Components.Message.Types.PlayerDisconnected:
                    this.List_Players.RemoveItem(e.Parameters[1] as PlayerData);
                    break;

                default:
                    break;
            }
        }
    }
}
