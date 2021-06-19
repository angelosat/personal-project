using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Housing
{
    class ResidenceUI : GroupBox
    {
        static Dictionary<Residence, Window> OpenWindows = new Dictionary<Residence, Window>();
        internal static Window GetWindow(Residence res)
        {
            Window existing;
            if (OpenWindows.TryGetValue(res, out existing))
                return existing;
            return new ResidenceUI(res).ToWindow(res.Name);
        }
        DialogInput DialogRename;
        Window WindowOwnership;
        Residence Residence;
        Label LblOwner;
        public ResidenceUI(Residence r)
        {
            this.WindowOwnership = new HouseOwnershipUI(r).ToWindow("Select owner");
            this.Residence = r;
            var owner = r.Town.GetNpc(r.Owner);
            this.LblOwner = new Label(string.Format("Owner: {0}", owner != null ? owner.Name : "none"));
            var pnlowner = new Panel() { ClientSize = new Rectangle(0, 0, 150, Label.DefaultHeight) };
            //pnlowner.ResizeToClientSize();
            pnlowner.AddControls(this.LblOwner);

            var btnrename = new Button("Rename") { LeftClickAction = BtnRenameClick };
            var btnedit = new Button("Edit") { LeftClickAction = () => ToolManager.SetTool(new ToolZoningPositions(Edit, () => r.Positions.ToList())) };// { Location = btnrename.BottomLeft };
            var btnownership = new Button("Ownership") { LeftClickAction = BtnOwnershipClick };// { Location = btnedit.BottomLeft };
            var btndelete = new Button("Delete") { LeftClickAction = BtnDeleteClick };
            var pnlbtns = new Panel() { AutoSize = true };
            pnlbtns.AddControls(btnrename, btnedit, btnownership, btndelete);
            pnlbtns.Controls.AlignVertically();

            this.AddControls(pnlowner, pnlbtns);
            this.Controls.AlignVertically();
        }

        public void BtnDeleteClick()
        {
            //var dialogdel = new DialogInput("Delete " + this.Residence.Name + " ?", Rename,);
            var del = new MessageBox(string.Format("Delete {0} ?", this.Residence.Name), "Are you sure?", Delete);
            del.ShowDialog();
        }

        private void BtnOwnershipClick()
        {
            this.WindowOwnership.ToggleSmart();
        }

        private void Edit(Vector3 pos, int w, int h, bool remove)
        {
            Client.Instance.Send(PacketType.ResidenceEdit, PacketResidenceAdd.Write(PlayerOld.Actor.RefID, this.Residence.ID, pos, w, h, remove));
        }
        public void Delete()
        {
            Client.Instance.Send(PacketType.ResidenceDelete, Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                w.Write(this.Residence.ID);
            }));
        }
        public void BtnRenameClick()
        {
            this.DialogRename = new DialogInput("Rename " + this.Residence.Name, Rename, 32, this.Residence.Name);
            this.DialogRename.ShowDialog();
        }

        private void Rename(string obj)
        {
            this.DialogRename.Hide();
            Client.Instance.Send(PacketType.ResidenceRename, Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                w.Write(this.Residence.ID);
                w.Write(obj);
            }));
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.ResidenceUpdated:
                    var res = e.Parameters[0] as Residence;
                    if (res != this.Residence)
                        break;
                    this.Refresh();
                    break;

                case Components.Message.Types.ResidenceRemoved:
                    res = e.Parameters[0] as Residence;
                    if (res != this.Residence)
                        break;
                    this.GetWindow().Hide();
                    break;

                default:
                    break;
            }
        }

        private new void Refresh()
        {
            this.GetWindow().Title = this.Residence.Name;
            var owner = this.Residence.Town.GetNpc(this.Residence.Owner);
            this.LblOwner.Text = string.Format("Owner: {0}", owner != null ? owner.Name : "none");
        }
        public override Window ToWindow(string name = "", bool closable = true, bool movable = true)
        {
            var win = base.ToWindow(name);
            win.HideAction = () => OpenWindows.Remove(this.Residence);
            win.ShowAction = () => OpenWindows[this.Residence] = win;
            return win;
        }
    }
}
