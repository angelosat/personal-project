using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns.Forestry
{
    class GroveUI : GroupBox
    {
        static Dictionary<Grove, Window> OpenWindows = new Dictionary<Grove, Window>();

        Grove Grove;
        SliderNew SliderDensity;
        Panel Panel, PanelButtons;
        public GroveUI(Grove grove)
        {
            this.Grove = grove;
            var lbldensity = new Label("Density");
            this.SliderDensity = new SliderNew(Vector2.Zero, 100, .25f, 1, .05f, grove.TargetDensity) { Location = lbldensity.BottomLeft };
            this.SliderDensity.ValueSelectAction = SetDensity;
            this.Panel = new Panel() { AutoSize = true };
            this.Panel.AddControls(lbldensity, this.SliderDensity);

            this.PanelButtons = new Panel() { AutoSize = true, Location = this.Panel.BottomLeft };
            var btndelete = new Button("Delete");
            btndelete.LeftClickAction = Delete;
            this.PanelButtons.AddControls(btndelete);

            this.AddControls(this.Panel, this.PanelButtons);
        }

        private void SetDensity(float obj)
        {
            //Client.Instance.Send(PacketType.GroveEdit, Network.Serialize(w =>
            //{
            //    w.Write(this.Grove.ID); 
            //    w.Write(obj);
            //}));
            Client.Instance.Send(PacketType.GroveEdit, PacketGroveEdit.Write(this.Grove.ID, this.Grove.Name, obj));

        }

        private void Delete()
        {
            Client.Instance.Send(PacketType.ZoneGrove, PacketZone.Write(PlayerOld.Actor.RefID, this.Grove.ID, Vector3.Zero, 0, 0, true));
            this.GetWindow().Hide();
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.GroveEdited:
                    var grove = e.Parameters[0] as Grove;
                    if (grove == this.Grove)
                        this.Refresh();
                    break;


                default:
                    break;
            }
        }

        private new void Refresh()
        {
            this.SliderDensity.Value = this.Grove.TargetDensity;
            this.GetWindow().Label_Title.Text = this.Grove.Name;
        }

        private new Window ToWindow(string name = "")
        {
            Window existing;
            if (OpenWindows.TryGetValue(this.Grove, out existing))
                return existing;

            var win = base.ToWindow(name);
            win.AutoSize = true;
            win.Movable = true;
            win.Client.Controls.Add(this);


            var dialogRename = new DialogInput("Rename " + this.Grove.Name, Rename, 16, this.Grove.Name);
            win.Label_Title.LeftClickAction = () => dialogRename.ShowDialog();
            win.Label_Title.MouseThrough = false;
            win.Label_Title.Active = true;

            win.HideAction = () => OpenWindows.Remove(this.Grove);
            win.ShowAction = () => OpenWindows[this.Grove] = win;

            return win;
        }

        private void Rename(DialogInput dialog)
        {
            //Client.Instance.Send(Net.PacketType.StockpileRename, PacketStockpileRename.Write(this.Stockpile.ID, dialog.Input));
            Client.Instance.Send(PacketType.GroveEdit, PacketGroveEdit.Write(this.Grove.ID, dialog.Input, this.Grove.TargetDensity));
            dialog.Hide();
        }

        internal static Window GetWindow(Grove grove)
        {
            Window existing;
            if (OpenWindows.TryGetValue(grove, out existing))
                return existing;
            return new GroveUI(grove).ToWindow(grove.Name);
        }
    }
}
