using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    class UIPlayerList : Window
    {
        #region Singleton
        static UIPlayerList _Instance;
        static public UIPlayerList Instance
        {
            get
            {
                if (_Instance.IsNull())
                    _Instance = new UIPlayerList();
                return _Instance;
            }
        }
        #endregion

        ListBox<PlayerData, Button> List_Players;
        PlayerList List;

        UIPlayerList()
        {
            //this.Size = new Rectangle(0, 0, 150, 400);
            //this.Name = "Player list";

            //Client.PlayersChanged += new EventHandler(Client_PlayersChanged);

            //List_Players = new ListBox<PlayerData, Button>(ClientSize);
            //List_Players.Build(Client.Players, foo => foo.Name, (foo, ctrl) => { });
            //Location = CenterScreen;

            Panel Panel = new Panel() { AutoSize = true };

            this.AutoSize = true;
            this.Title = "Players";
            Closable = false;
           // Net.Client.PlayersChanged += new EventHandler(Client_PlayersChanged);

            List_Players = new ListBox<PlayerData, Button>(new Rectangle(0, 0, 150, 300));
            //List_Players.Build(Net.Client.Players, foo => foo.Name + " " + foo.Connection.RTT.TotalMilliseconds.ToString("###0ms"), (foo, ctrl) =>
            //{
            //    //foo.Connection.PropertyChanged += (sender, p) =>
            //    ctrl.OnUpdate = () =>
            //    {
            //        ctrl.Text = foo.Name + " " + foo.Connection.RTT.TotalMilliseconds.ToString("###0ms");
            //    };
            //});

            Panel.Controls.Add(List_Players);
            this.Client.Controls.Add(Panel);//List_Players);
            Location = CenterScreen;
        }

        void Client_PlayersChanged(object sender, EventArgs e)
        {
            Refresh(List);
        }
        
        // WARNING: restrict the use of show to this particular method
        public bool Show(PlayerList pList)
        {
            this.List = pList;
            pList.PlayersChanged += new EventHandler(Client_PlayersChanged);
            Refresh(pList);
            return base.Show();
        }

        public override bool Hide()
        {
            if (!this.List.IsNull())
                this.List.PlayersChanged -= Client_PlayersChanged;
            return base.Hide();
        }

        private void Refresh(PlayerList pList)
        {
            List_Players.Build(pList.GetList(), foo => foo.Name, (foo, ctrl) =>
            {
                ctrl.OnUpdate = () =>
                {
                    //ctrl.Text = foo.Name + " " + foo.Connection.RTT.TotalMilliseconds.ToString("###0ms");
                    ctrl.Text = foo.ID + ": " + foo.Name + " " + foo.Ping.ToString("###0ms"); //" id:" + f
                };
            });
        }

        //public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        //{
        //    if (InputState.IsKeyDown(System.Windows.Forms.Keys.Tab))
        //        base.Draw(sb, viewport);
        //}

       // public override void Dispose()
       // {
       ////     Net.Client.PlayersChanged -= Client_PlayersChanged;
       // }
    }
}
