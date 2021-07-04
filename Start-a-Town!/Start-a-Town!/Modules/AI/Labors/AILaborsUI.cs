using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using Start_a_Town_.Towns;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI
{
    class AILaborsUI : GroupBox
    {
        Town Town;
        public AILaborsUI(Town town)
        {
            this.Town = town;
            //Refresh();
                //foreach (var item in AILabor.All)
                //{

                //}
            
        }

        private new void Refresh()
        {
            this.Controls.Clear();
            //foreach (var agent in this.Town.Agents)
            //    this.Controls.Add(new Row(agent) { Location = this.Controls.BottomLeft });
            foreach (var id in this.Town.Agents)
                this.Controls.Add(new Row(Client.Instance.GetNetworkObject(id)) { Location = this.Controls.BottomLeft });
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
        class Row : Panel
        {
            const int Spacing = 32;
            public Row(GameObject entity)
            {
                this.BackgroundStyle = BackgroundStyle.Window;
                this.AutoSize = true;
                var state = AIState.GetState(entity);
                var entitylabors = state.Labors;
                this.Tag = entity;
                Label name = new Label(entity.Name);
                this.Controls.Add(name);
                //var offset = 0;
                foreach (var item in JobDefOf.All)
                {
                    //var chkbox = new CheckBoxNew(item.Name) { Location = name.TopRight + new Vector2(offset, 0) }; //this.Controls.TopRight + 
                    //offset += Spacing * 3;
                    var chkbox = new CheckBoxNew(item.Name) { Location = this.Controls.TopRight + new Vector2(Spacing, 0) }; //this.Controls.TopRight + 
                    chkbox.Value = entitylabors.Contains(item);
                    chkbox.LeftClickAction = () => ToggleLabor(entity, item);
                    chkbox.Tag = item;
                    this.Controls.Add(chkbox);
                }
            }

            private void ToggleLabor(GameObject entity, JobDef item)
            {
                byte[] data = Network.Serialize(w =>
                    {
                        w.Write(entity.RefID);
                        w.Write(item.Name);
                    });
                Client.Instance.Send(PacketType.LaborToggle, data);
            }
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Message.Types.NpcsUpdated:
                    this.Refresh();
                    break;

                case Message.Types.LaborsUpdated:
                    var entity = e.Parameters[0] as GameObject;
                    
                    //this.Refresh();
                    //break;

                    var state = AIState.GetState(entity);
                    var row = this.Controls.Where(r => r is Row).Select(r=>r as Row).First(r => r.Tag as GameObject == entity);
                    foreach(var labor in JobDefOf.All)
                    {
                        var chkbox = row.Controls.Where(c => c is CheckBoxNew).Select(c => c as CheckBoxNew).First(c => c.Tag as JobDef == labor);
                        chkbox.Value = state.Labors.Contains(labor);
                    }

                    break;

                default:
                    break;
            }
        }
    }
}
