using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Towns;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Modules.AI.Net;
using System.IO;

namespace Start_a_Town_.AI
{
    public class JobsManager : TownComponent
    {
        class Packets
        {
            static public void Init()
            {
                Server.RegisterPacketHandler(PacketType.LaborToggle, HandleLaborToggle);
                Client.RegisterPacketHandler(PacketType.LaborToggle, HandleLaborToggle);

                Server.RegisterPacketHandler(PacketType.JobModifyRequest, HandleJobModRequest);
                Client.RegisterPacketHandler(PacketType.JobSync, HandleJobSync);
            }

            private static void HandleJobModRequest(IObjectProvider net, BinaryReader r)
            {
                var server = net as Server;
                var player = server.GetPlayer(r.ReadInt32());
                var actor = server.GetNetworkObject(r.ReadInt32()) as Actor;
                var jobDef = Def.GetDef<JobDef>(r.ReadString());
                var job = actor.GetJob(jobDef);
                //job.Priority = r.ReadInt32();
                //job.Enabled = r.ReadBoolean();
                job.Read(r);
                net.EventOccured(Components.Message.Types.JobUpdated, actor, job.Def);
                SyncJob(player, actor, job);
            }

            public static void SendPriorityModify(PlayerData player, Actor actor, Job job, int priority)//bool enabled, int priority)
            {
                var net = actor.Net;
                if (net is Server)
                {
                    //job.Enabled = enabled; 
                    job.Priority = (byte)priority;
                    net.EventOccured(Components.Message.Types.JobUpdated, actor, job.Def);
                    SyncJob(player, actor, job);
                }
                else
                {
                    var w = net.GetOutgoingStream();
                    w.Write((int)PacketType.JobModifyRequest, player.ID, actor.RefID, job.Def.Name, priority); //enabled, priority);
                }
            }
            public static void SendLaborToggle(PlayerData player, Actor actor, JobDef jobDef)
            {
                var net = actor.Net;
                if (net is Server)
                {
                    actor.ToggleJob(jobDef);
                    net.EventOccured(Components.Message.Types.JobUpdated, actor, jobDef);
                }
                net.GetOutgoingStream().Write((int)PacketType.LaborToggle, player.ID, actor.RefID, jobDef.Name);
            }
            private static void HandleLaborToggle(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var actor = net.GetNetworkObject(r.ReadInt32()) as Actor;
                var jobDef = Def.GetDef<JobDef>(r.ReadString());
                if (net is Client)
                {
                    actor.ToggleJob(jobDef);
                    net.EventOccured(Components.Message.Types.JobUpdated, actor, jobDef);
                }
                else
                    SendLaborToggle(player, actor, jobDef);
            }
            public static void SyncJob(PlayerData player, Actor actor, Job job)
            {
                var net = actor.Net as Server;
                var w = net.GetOutgoingStream();
                w.Write((int)PacketType.JobSync, player.ID, actor.RefID);
                w.Write(job.Def.Name);
                job.Write(w);
            }
            private static void HandleJobSync(IObjectProvider net, BinaryReader r)
            {
                var client = net as Client;
                var player = client.GetPlayer(r.ReadInt32());
                var actor = client.GetNetworkObject(r.ReadInt32()) as Actor;
                var jobDef = Def.GetDef<JobDef>(r.ReadString());
                var job = actor.GetJob(jobDef);
                job.Read(r);
                net.EventOccured(Components.Message.Types.JobUpdated, actor, jobDef);
            }
        }
        //public List<GameObject> Agents = new List<GameObject>();
        Window WindowLabors;
        readonly Lazy<Control> UILabors;

        public override string Name
        {
            get { return "Labors"; }
        }
        static JobsManager()
        {
            Packets.Init();
        }
        public JobsManager(Town town)
        {
            this.Town = town;
            this.UILabors = new Lazy<Control>(this.CreateJobsTable);
        }
        
        internal override IEnumerable<Tuple<string, Action>> OnQuickMenuCreated()
        {
            yield return new Tuple<string, Action>("Labors", this.ToggleLaborsWindow);
        }

        public void ToggleLaborsWindow()
        {
            //if(this.WindowLabors == null)
            //{
            //    this.WindowLabors = new UILaborsTable(this.Town).ToWindow("Labors");
            //    this.WindowLabors.Location = Microsoft.Xna.Framework.Vector2.Zero;
            //    //this.WindowLabors.Layer = LayerTypes.Hud;
            //}
            //this.WindowLabors.Toggle();
            var window = this.UILabors.Value.GetWindow() ?? new Window("Jobs", this.UILabors.Value);
            window.Toggle();
        }

        internal override void OnTargetSelected(IUISelection info, ISelectable target)
        {
            base.OnTargetSelected(info, target);
        }

       
        Control CreateJobsTable()
        {
            var box = new GroupBox();
            var tableBox = new GroupBox();
            var tableAuto = new TableScrollableCompactNewNew<Actor>(8, true)// {  }
                            .AddColumn(null, "Name", 100, o => new Label(o.Name, () => { }));// SelectNpc(o) });//, 0);
            var tableManual = new TableScrollableCompactNewNew<Actor>(8, true)// {  }
                           .AddColumn(null, "Name", 100, o => new Label(o.Name, () => { }));// SelectNpc(o) });//, 0);
            var player = this.Player;
            //this.PanelTable = new Panel() { AutoSize = true };
            foreach (var labor in JobDefOf.All)
            {
                var ic = labor.Icon;

                var icon = new PictureBox(ic.SpriteSheet, ic.SourceRect) { HoverText = labor.Name };//, BackgroundColorFunc = ()=>Color.White *.5f };
                var iconManual = new PictureBox(ic.SpriteSheet, ic.SourceRect) { HoverText = labor.Name };//, BackgroundColorFunc = ()=>Color.White *.5f };

                tableAuto.AddColumn(labor, icon, CheckBox.CheckedRegion.Width, (actor) =>
                {
                    var state = AIState.GetState(actor);
                    var job = state.GetJob(labor);
                    var ch =  new CheckBoxNew
                    {
                        TickedFunc = () => job.Enabled,
                        LeftClickAction = () => Packets.SendLaborToggle(player, actor, labor)
                        //LeftClickAction = () => Packets.SendPriorityModify(player, actor, job, 0)
                    };
                    return ch;
                }, 0);
                tableManual.AddColumn(labor, iconManual, CheckBox.CheckedRegion.Width, (actor) =>
                {
                    var state = AIState.GetState(actor);
                    var job = state.GetJob(labor);
                    //return IconButton.CreateSmall((char)job.Priority, () => { });
                    var btn = new Button(CheckBox.CheckedRegion.Width)
                    {
                        TextFunc = () => { var val = job.Priority; return job.Enabled ? val.ToString() : ""; },
                        LeftClickAction = () => Packets.SendPriorityModify(player, actor, job, job.Priority + 1), //job.Enabled, 
                        RightClickAction = () => Packets.SendPriorityModify(player, actor, job, job.Priority - 1) //job.Enabled, 
                    };
                    return btn;
                }, 0);
            }
            var net = this.Town.Net;
            var actors = this.Town.Agents.Select(id => net.GetNetworkObject(id) as Actor);
            tableAuto.AddItems(actors);//, a => a.Name);
            tableManual.AddItems(actors);//, a => a.Name);

            var currentTable = tableAuto;

            tableBox.AddControls(tableAuto);
            var btnTogglePriorities = new CheckBoxNew("Manual priorities") { TickedFunc = () => currentTable == tableManual, LeftClickAction = switchTables };
            box.AddControlsVertically(
                btnTogglePriorities,
                tableBox);
            //box.AddControls(tableAuto);

            box.ListenTo(Components.Message.Types.JobUpdated, args =>
            {
                var a = args[0] as Actor;
                var j = args[1] as JobDef;
                tableAuto.GetItem(a, j).Validate();
                tableManual.GetItem(a, j).Validate();
            });

            return box;

            void switchTables()
            {
                tableBox.ClearControls();
                currentTable = currentTable == tableManual ? tableAuto : tableManual;
                tableBox.AddControls(currentTable);
            }
        }
        
    }
}
