using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Towns;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    class TownJobEditWindow: Window
    {
        static TownJobEditWindow _Instance;
        static public TownJobEditWindow Instance
        {
            get
            {
                if (_Instance.IsNull())
                    _Instance = new TownJobEditWindow();
                return _Instance;
            }
        }

        TextBox Txt_Name { get; set; }
        ListBox<TownJobStep, Button> Joblist { get; set; }
        public TownJob Job { get; private set; }

        TownJobEditWindow()
        {
            this.Job = new TownJob(Net.Client.Instance) { Creator = Player.Actor };

            Title = "Create Job";
            this.AutoSize = true;
            this.Movable = true;

            Panel panel_name = new Panel() { AutoSize = true };
            Txt_Name = new TextBox(200) { Text = "Job" + TownJobCollection.JobID };
            panel_name.Controls.Add(Txt_Name);

            Panel panel_joblist = new Panel() { Location = panel_name.BottomLeft, AutoSize = true };

            this.Joblist = new ListBox<TownJobStep, Button>(new Rectangle(0, 0, 200, 400));
            panel_joblist.Controls.Add(this.Joblist);

            Panel panel_buttons = new Panel() { Location = panel_joblist.BottomLeft, AutoSize = true };

            Button btn_clear = new Button(Vector2.Zero, panel_joblist.ClientSize.Width, "Clear")
            {
                LeftClickAction = () =>
                {
                    if (string.IsNullOrWhiteSpace(this.Name))
                        return;
                    this.Job.Steps.Clear();
                    this.Refresh();
                },
                HoverFunc = () =>
                {
                    return string.IsNullOrWhiteSpace(this.Name) ? "Invalid name" : "";
                }
            };
            Button btn_save = new Button(btn_clear.BottomLeft, panel_joblist.ClientSize.Width, "Save")
            {
                LeftClickAction = () =>
                {
                    this.Job.Name = Txt_Name.Text;
                    Net.Client.Request(Net.PacketType.JobCreate, this.Job.Write);

                    this.Job = new TownJob(Net.Client.Instance);
                    this.Refresh();

                    this.Hide();
                }
            };

            panel_buttons.Controls.Add(btn_clear, btn_save);

            this.Client.Controls.Add(panel_name, panel_joblist, panel_buttons);
            this.Location = CenterScreen * new Vector2(0.5f, 1);
        }

        public TownJobEditWindow Initialize(TownJob job)
        {
            this.Joblist.Build(job.Steps,
                j =>
                    Ability.GetScript(j.Script).Name + ": " + j.Target.Object.Name,
                (j, c) => { }
            );
            return this;
        }
        public TownJobEditWindow Refresh()
        {
            this.Initialize(this.Job);
            return this;
        }

        public TownJobEditWindow Show(TownJobStep step)
        {
            //this.Job = new TownJob(new TownJobStep[] { step });
            this.Show();
            this.Job.Steps.Add(step);
            this.Refresh();
            // keep only one instance of the tool stored?
            Start_a_Town_.PlayerControl.ToolManager.Instance.ActiveTool = new Start_a_Town_.PlayerControl.JobToolNew(step);
            return this;
        }
        public TownJobEditWindow Show(TownJob job)
        {
            this.Show();
            this.Job = job;
            this.Refresh();
            return this;
        }
        public override bool Hide()
        {
            Start_a_Town_.PlayerControl.ToolManager.Instance.ActiveTool = null;
            return base.Hide();
        }
    }
}
