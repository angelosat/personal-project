using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Forestry
{
    class ChoppingManagerUI : GroupBox
    {
        ListBox<Grove, Label> ListGroves;
        ChoppingManager Manager;
        IconButton BtnDesignate;
        public ChoppingManagerUI(ChoppingManager manager)
        {
            this.Manager = manager;
            var panelgroves = new Panel() { AutoSize = true };
            this.ListGroves = new ListBox<Grove, Label>(80, 200);
            Refresh();
            panelgroves.AddControls(this.ListGroves);

            var panelbuttons = new Panel { Location = panelgroves.TopRight, AutoSize = true };
            this.BtnDesignate = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Designate chopping\n\nLeft click & drag: Add chopping\nCtrl+Left click: Remove chopping",// "Add/Remove stockpiles",
                LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool =
                    //new ToolZoning(this.ToolCreateZone, manager.Town.GetZones) { IsValid = (g) => true }
                    new ToolDesignatePositions(this.Add, this.Manager.GetPositions) { ValidityCheck = IsPositionValid }
            };
            var btnGrove = new IconButton()
            {
                Location = this.BtnDesignate.TopRight,
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Designate grove\n\nLeft click & drag: Add grove\nCtrl+Left click: Remove grove",// "Add/Remove stockpiles",
                LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool =
                    new ToolZoning(this.ToolCreateZone, () => manager.Town.ChoppingManager.GetGroves().Cast<Zone>().ToList()) { IsValid = (g) => true }
                //new ToolDesignatePositions(this.Add, this.Manager.GetPositions) { ValidityCheck = IsPositionValid }
            };
            panelbuttons.AddControls(this.BtnDesignate, btnGrove);
            this.Controls.Add(panelgroves, panelbuttons);//this.BtnDesignate, btnGrove);
        }

        private void Refresh()
        {
            this.ListGroves.Build(this.Manager.GetGroves(), g => g.Name, (g, b) => b.LeftClickAction = () => OpenGrove(g));
        }

        private bool IsPositionValid(Vector3 arg)
        {
            return !Block.IsBlockSolid(this.Manager.Town.Map, arg + Vector3.UnitZ);
        }

        private void Add(Vector3 start, Vector3 end, bool value)
        {
            Client.Instance.Send(PacketType.ChoppingDesignation, PacketChoppingDesignation.Write(Player.Actor.Network.ID, start, end, !value));
        }

        void ToolCreateZone(Vector3 global, int w, int h, bool remove)
        {
            var grove = this.Manager.Town.ChoppingManager.GetGroveAt(global);
            if(grove!=null)
            {
                if(remove)
                {
                    Client.Instance.Send(PacketType.ZoneGrove, PacketZone.Write(Player.Actor.InstanceID, grove.ID, global, w, h, remove));
                    return;
                }
                OpenGrove(grove);
                return;
            }
            Client.Instance.Send(PacketType.ZoneGrove, PacketZone.Write(Player.Actor.InstanceID, 0, global, w, h, remove));
        }
        private void OpenGrove(Grove grove)
        {
            Window win = GroveUI.GetWindow(grove);
            if (win != null)
                win.Toggle();
            return;

            //var win = grove.GetInterface().ToWindow(grove.Name);
            //win.Show();
            //DialogRenameGrove = new DialogInput("Rename " + grove.Name, (txt) => RenameGrove(grove, txt), 16, grove.Name);
            //win.Label_Title.LeftClickAction = () => DialogRenameGrove.ShowDialog();
            //win.Label_Title.MouseThrough = false;
            //win.Label_Title.Active = true;
        }

        //DialogInput DialogRenameGrove;
        //private void RenameGrove(Grove grove, string txt)
        //{
        //    Client.Instance.Send(PacketType.GroveEdit, PacketGroveEdit.Write(grove.ID, txt, grove.TargetDensity));
        //    DialogRenameGrove.Hide();
        //}

        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.GrovesUpdated:
                    this.Refresh();
                    break;

                case Components.Message.Types.GroveAdded:
                    int senderID = (int)e.Parameters[0];
                    Grove grove = e.Parameters[1] as Grove;
                    if (senderID == Player.Actor.InstanceID)
                        OpenGrove(grove);
                    break;

                default:
                    break;
            }
        }
    }
}
