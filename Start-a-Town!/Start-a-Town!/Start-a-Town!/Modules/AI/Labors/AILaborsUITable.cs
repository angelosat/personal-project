using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components;
using Start_a_Town_.Towns;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI
{
    class AILaborsUITable : GroupBox
    {
        Town Town;
        TableNew<GameObject> Table;
        Button BtnGenerateNpc;
        public AILaborsUITable(Town town)
        {
            this.Town = town;

            this.BtnGenerateNpc = new Button("Generate Npc") { LeftClickAction = () => Client.Instance.Send(PacketType.AIGenerateNpc, new byte[] { }) };

            this.Table = new TableNew<GameObject>()// {  }
                            .AddColumn(null, "Name", 100, o => new Label(o.Name) { Active = true, LeftClickAction = () => SelectNpc(o) }, 0);

            foreach (var labor in AILabor.All)
                this.Table.AddColumn(labor, labor.Name, 100, (item) =>
                {
                    var chk = new CheckBoxNew();
                    chk.Value = AIState.GetState(item).HasLabor(labor);
                    chk.LeftClickAction = () => ToggleLabor(item, labor);
                    return chk;
                });
            this.Table.Location = this.BtnGenerateNpc.BottomLeft;
            this.AddControls(BtnGenerateNpc);

        }

        private void SelectNpc(GameObject o)
        {
            var win = new NpcUI(o).ToWindow(o.Name);
            win.SmartPosition();
            win.Show();
        }

        private void Refresh()
        {
            //this.Controls.Remove(this.Table);//.Clear();
            this.Controls.Clear();

            this.AddControls(BtnGenerateNpc);
            //foreach (var agent in this.Town.Agents)
            //    this.Controls.Add(new Row(agent) { Location = this.Controls.BottomLeft });
            this.Table.Build(this.Town.Agents);//, a => a.Name);

            this.AddControls(this.Table);
            var win = this.GetWindow();
            if (win != null)
                win.Invalidate();
        }
        protected override void OnShow()
        {
            this.Refresh();
            this.Conform(this.Controls.ToArray());
            base.OnShow();
        }
        //class Row : Panel
        //{
        //    const int Spacing = 32;
        //    public Row(GameObject entity)
        //    {
        //        this.BackgroundStyle = BackgroundStyle.Window;
        //        this.AutoSize = true;
        //        var state = AIState.GetState(entity);
        //        var entitylabors = state.Labors;
        //        this.Tag = entity;
        //        Label name = new Label(entity.Name);
        //        this.Controls.Add(name);
        //        var offset = 0;
        //        foreach (var item in AILabor.All)
        //        {
        //            //var chkbox = new CheckBoxNew(item.Name) { Location = name.TopRight + new Vector2(offset, 0) }; //this.Controls.TopRight + 
        //            //offset += Spacing * 3;
        //            var chkbox = new CheckBoxNew(item.Name) { Location = this.Controls.TopRight + new Vector2(Spacing, 0) }; //this.Controls.TopRight + 
        //            chkbox.Value = entitylabors.Contains(item);
        //            chkbox.LeftClickAction = () => ToggleLabor(entity, item);
        //            chkbox.Tag = item;
        //            this.Controls.Add(chkbox);
        //        }
        //    }

        //    private void ToggleLabor(GameObject entity, AILabor item)
        //    {
        //        byte[] data = Network.Serialize(w =>
        //            {
        //                w.Write(entity.InstanceID);
        //                w.Write(item.Name);
        //            });
        //        Client.Instance.Send(PacketType.LaborToggle, data);
        //    }
        //}
        private void ToggleLabor(GameObject entity, AILabor item)
        {
            byte[] data = Network.Serialize(w =>
            {
                w.Write(entity.InstanceID);
                w.Write(item.Name);
            });
            Client.Instance.Send(PacketType.LaborToggle, data);
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Message.Types.AgentsUpdated:
                    this.Refresh();
                    break;

                case Message.Types.LaborsUpdated:
                    var entity = e.Parameters[0] as GameObject;
                    this.RefreshLabors(entity);
                    //this.Refresh();
                    //break;

                    //var state = AIState.GetState(entity);
                    //var row = this.Controls.Where(r => r is Row).Select(r=>r as Row).First(r => r.Tag as GameObject == entity);
                    //foreach(var labor in AILabor.All)
                    //{
                    //    var chkbox = row.Controls.Where(c => c is CheckBoxNew).Select(c => c as CheckBoxNew).First(c => c.Tag as AILabor == labor);
                    //    chkbox.Value = state.Labors.Contains(labor);
                    //}

                    break;

                default:
                    break;
            }
        }

        private void RefreshLabors(GameObject entity)
        {
            foreach(var labor in AILabor.All)
            {
                var chkbox = this.Table.GetItem(entity, labor) as CheckBoxNew;
                chkbox.Value = AIState.GetState(entity).HasLabor(labor);
            }
        }
    }
}
