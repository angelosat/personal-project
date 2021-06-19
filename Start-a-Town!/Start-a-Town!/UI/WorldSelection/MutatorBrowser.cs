using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI.WorldSelection
{
    class MutatorBrowser : GroupBox
    {
        class PanelMutatorList : Panel
        {
            public HashSet<Terraformer> Selected = new HashSet<Terraformer>(Terraformer.Defaults);

            public Action<Terraformer> Callback = value => { };
            ListBox<Terraformer, CheckBox> List;
            public PanelMutatorList(int w, int h)
            {
                //CheckBox chk_showSelected = new CheckBox("Selected", true);
                //CheckBox chk_showUnselected = new CheckBox("Unselected", true);
                this.AutoSize = true;
                this.List = new ListBox<Terraformer, CheckBox>(w, h);
            //    this.Refresh();
                this.Controls.Add(this.List);
            }

            public void Refresh()
            {
                this.List.Build(Terraformer.All, item => item.Name, (item, btn) =>
                {
                    btn.LeftClickAction = () =>
                    {
                        this.Callback(item);
                        this.Selected.Add(item);
                    };
                    btn.Checked = this.Selected.Contains(item);
                });
            }

            public void Refresh(bool filter)
            {
                this.List.Build(from item in Terraformer.All
                                where this.Selected.Contains(item) == filter
                                select item,
                                item => item.Name,
                                (item, btn) =>
                                {
                                    btn.LeftClickAction = () =>
                                    {
                                        this.Callback(item); 
                                        this.Selected.Add(item);
                                    };
                                    btn.Checked = this.Selected.Contains(item);
                                });
            }
            public void Refresh(bool selected, bool unselected)
            {
                this.List.Build(from item in Terraformer.All
                                let contained = this.Selected.Contains(item)
                                where contained == selected || contained == !unselected
                                select item,
                                item => item.Name,
                                (item, btn) =>
                                {
                                    btn.LeftClickAction = () =>
                                    {
                                        this.Callback(item);
                                        this.Selected.Add(item);
                                    };
                                    btn.Checked = this.Selected.Contains(item);
                                });
            }
        }

        class PanelMutatorProperties : Panel
        {
            public PanelMutatorProperties(int w, int h)
            {
                this.Size = new Rectangle(0, 0, w, h);
                //this.AutoSize = true;
            }

            public void Refresh(Terraformer mutator)
            {
                this.Controls.Clear();
                this.Controls.Add(new Label(mutator.Name) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
                var ui = mutator.GetUI();
                ui.Location = this.Controls.BottomLeft;
                this.Controls.Add(ui);
            }
        }
        PanelMutatorList MutatorList;
        public MutatorBrowser()
        {
            GroupBox filters = new GroupBox();
            var chkselect = new CheckBox("Selected", true) { Location = filters.Controls.BottomLeft };
            var chkunselect = new CheckBox("Unselected", true) { Location = chkselect.TopRight };
            filters.Controls.Add(chkselect, chkunselect);
            this.Controls.Add(filters);
            this.MutatorList = new PanelMutatorList(filters.Width, 400) { Location = this.Controls.BottomLeft };
            var panelprops = new PanelMutatorProperties(MutatorList.Width, MutatorList.Height) { Location = MutatorList.TopRight };

            chkselect.ValueChangedFunction = value => MutatorList.Refresh(chkselect.Checked, chkunselect.Checked);
            chkunselect.ValueChangedFunction = chkselect.ValueChangedFunction;

            MutatorList.Callback = item => panelprops.Refresh(item);
            MutatorList.Refresh();
            this.Controls.Add(MutatorList, panelprops);
        }
        public List<Terraformer> GetSelected()
        {
            var finallist = Terraformer.All.Where(this.MutatorList.Selected.Contains).ToList();
            return finallist;
            //return this.MutatorList.Selected.ToList();
        }
    }
}
