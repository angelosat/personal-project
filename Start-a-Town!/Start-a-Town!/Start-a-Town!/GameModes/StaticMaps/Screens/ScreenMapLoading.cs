using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Rooms;
using Start_a_Town_.GameModes.StaticMaps.UI;

namespace Start_a_Town_.GameModes.StaticMaps.Screens
{
    class ScreenMapLoading : GameScreen
    {
        StaticMapLoadingWidget Widget;
        private StaticMap Map;
        StaticMapLoadingProgressToken ProgressToken;
        //GameObject Actor;
        //public ScreenMapLoading()
        //{
            
        //}

        public ScreenMapLoading(StaticMap map)//, GameObject actor)
        {
            //this.Actor = actor;
            // TODO: Complete member initialization
            this.Widget = new StaticMapLoadingWidget();//{ Location = this.Widget.CenterScreen };
            this.Widget.Location = this.Widget.CenterScreen;
            //this.WindowManager
            this.Widget.Show(this.WindowManager);
            this.Map = map;
            //Task.Factory.StartNew(() => map.Load((text, perc) => this.Widget.Refresh(text, perc)));
            this.ProgressToken = new StaticMapLoadingProgressToken(this.Widget.Refresh);
            Task.Factory.StartNew(() => map.Load(this.ProgressToken));//(text, perc) => this.Widget.Refresh(text, perc)));
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            this.WindowManager.Update(gameTime);
        }
        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (Game1.Instance.IsActive)
                this.EnterMap(this.Map);
        }
        void EnterMap(IMap map)
        {
            if (map == null)
                return;
            if (this.ProgressToken.Percentage < 1)
                return;
            //XDocument xml = Engine.Settings.ToXDocument();

            //var profile = xml.Root.Element("Profile");
            //if (profile.IsNull())
            //{
            //    profile = new XElement("Profile");
            //    xml.Root.Add(profile);
            //}
            //var xLastWorld = profile.Element("LastWorld");
            //if (xLastWorld.IsNull())
            //    profile.Add(new XElement("LastWorld", map.GetWorld().GetName()));
            //else
            //    xLastWorld.Value = map.GetWorld().GetName();

            //Engine.Settings = xml.ToXmlDocument();
            Engine.Config.GetOrCreateElement("Profile").GetOrCreateElement("LastWorld").Value = map.GetWorld().GetName();

            //GameObject character = Window_Character.Tag as GameObject;
            //if (character.IsNull())
            //    return;

            Rooms.WorldScreen.Instance.Map = null;
            //Initialize(Rooms.WorldScreen.Instance.Map);

            //Engine.PlayGame();

            /// add screen only after map has been received and loaded by client!!!
            //Rooms.Ingame ingame = new Rooms.Ingame();
            //ScreenManager.Add(ingame.Initialize());
            ///

            Net.Server.LoadMap(map); // is this needed???
            
            string localHost = "127.0.0.1";
            Net.Client.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
        }

        public override void Draw(SpriteBatch sb)
        {
            Game1.Instance.GraphicsDevice.Clear(Color.DarkSlateBlue);
            this.WindowManager.Draw(sb);
        }
    }
}
