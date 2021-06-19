using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Towns;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using System.IO;
using System.Text;

namespace Start_a_Town_
{
    public class QuestsManager : TownComponent
    {
        static class Packets
        {
            static public void Init()
            {
                Server.RegisterPacketHandler(PacketType.QuestCreate, ReceiveQuestCreate);
                Client.RegisterPacketHandler(PacketType.QuestCreate, ReceiveQuestCreate);

                Server.RegisterPacketHandler(PacketType.QuestRemove, ReceiveRemoveQuest);
                Client.RegisterPacketHandler(PacketType.QuestRemove, ReceiveRemoveQuest);

                Server.RegisterPacketHandler(PacketType.QuestCreateObjective, ReceiveQuestCreateObjective);
                Client.RegisterPacketHandler(PacketType.QuestCreateObjective, ReceiveQuestCreateObjective);

                Server.RegisterPacketHandler(PacketType.QuestRemoveObjective, ReceiveQuestRemoveObjective);
                Client.RegisterPacketHandler(PacketType.QuestRemoveObjective, ReceiveQuestRemoveObjective);

                Server.RegisterPacketHandler(PacketType.QuestGiverAssign, ReceiveQuestGiverAssign);
                Client.RegisterPacketHandler(PacketType.QuestGiverAssign, ReceiveQuestGiverAssign);

                Server.RegisterPacketHandler(PacketType.QuestModify, ReceiveQuestModify);
                Client.RegisterPacketHandler(PacketType.QuestModify, ReceiveQuestModify);


                //ReceiveQuestGiverAssignID = Server.RegisterPacketHandler(ReceiveQuestGiverAssign);
                //ReceiveQuestGiverAssignID = Client.RegisterPacketHandler(ReceiveQuestGiverAssign);

                //var g = typeof(Packets);
                //$"{g.FullName} :: {g.GetMethod("Init").Name}".ToConsole();
            }
            public static void SendQuestModify(IObjectProvider net, PlayerData player, QuestDef quest, int maxConcurrentModValue)
            {
                if (net is Server)
                    quest.MaxConcurrent = maxConcurrentModValue;
                net.GetOutgoingStream().Write((int)PacketType.QuestModify, player.ID, quest.ID, maxConcurrentModValue);
            }
            private static void ReceiveQuestModify(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var maxConcurrentModValue = r.ReadInt32();
                if (net is Client)
                    quest.MaxConcurrent = maxConcurrentModValue;
                else
                    SendQuestModify(net, player, quest, maxConcurrentModValue);
            }

            public static void SendQuestGiverAssign(IObjectProvider net, PlayerData player, QuestDef quest, Actor actor)
            {
                if(net is Server)
                    quest.Giver = actor;
                //var w = net.GetOutgoingStream();
                //w.Write(PacketType.QuestGiverAssign);
                //w.Write(player.ID);
                //w.Write(quest.ID);
                //w.Write(actor?.InstanceID ?? -1);
                net.GetOutgoingStream().Write((int)PacketType.QuestGiverAssign, player.ID, quest.ID, actor?.RefID ?? -1);
            }
            private static void ReceiveQuestGiverAssign(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var actorid = r.ReadInt32();
                var actor = actorid == -1 ? null : net.GetNetworkObject(actorid) as Actor;
                if (net is Client)
                    quest.Giver = actor;
                else
                    SendQuestGiverAssign(net, player, quest, actor);
            }

            public static void SendQuestObjectiveRemove(IObjectProvider net, PlayerData player, QuestDef quest, QuestObjective qobj)
            {
                var index = quest.GetObjectives().ToList().FindIndex(i => i == qobj);
                if (net is Server server)
                    quest.RemoveObjective(qobj);
                var w = net.GetOutgoingStream();
                w.Write(PacketType.QuestRemoveObjective);
                w.Write(player.ID);
                w.Write(quest.ID);
                w.Write(index);
            }
            private static void ReceiveQuestRemoveObjective(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var objectiveIndex = r.ReadInt32();
                var objective = quest.GetObjectives().ElementAt(objectiveIndex);
                if (net is Server)
                    SendQuestObjectiveRemove(net, player, quest, objective);
                else
                    quest.RemoveObjective(objective);
            }

            public static void SendQuestCreateObjective(IObjectProvider net, PlayerData player, QuestDef quest, QuestObjective qobj)
            {
                if (net is Server server)
                {
                    quest.AddObjective(qobj);
                }
                var w = net.GetOutgoingStream();
                w.Write(PacketType.QuestCreateObjective);
                w.Write(player.ID);
                w.Write(quest.ID);
                w.Write(qobj.GetType().FullName);
                qobj.Write(w);
            }
            private static void ReceiveQuestCreateObjective(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var qObj = Activator.CreateInstance(Type.GetType(r.ReadString()), quest) as QuestObjective;
                qObj.Read(r);
                if (net is Server)
                {
                    SendQuestCreateObjective(net, player, quest, qObj);
                }
                else
                {
                    quest.AddObjective(qObj);
                }
            }
            internal static void SendAddQuestGiver(IObjectProvider net, int playerID)
            {
                var w = net.GetOutgoingStream();
                w.Write(PacketType.QuestCreate);
                w.Write(playerID);
                if (net is Server server)
                {
                    var manager = server.Map.Town.QuestManager;
                    var q = manager.CreateQuest();
                    manager.AddQuest(q);
                    w.Write(q.ID);
                }
                //else
                //    w.Write(-1);
            }
            private static void ReceiveQuestCreate(IObjectProvider net, BinaryReader r)
            {
                var playerID = r.ReadInt32();
                if (net is Server server)
                    SendAddQuestGiver(server, playerID);
                else
                {
                    var questID = r.ReadInt32();
                    var manager = net.Map.Town.QuestManager;
                    manager.AddQuest(questID);
                }
            }
            internal static void RemoveQuest(QuestsManager manager, int playerID, QuestDef quest)
            {
                var net = manager.Town.Net;
                var w = net.GetOutgoingStream();
                w.Write(PacketType.QuestRemove);
                //w.Write(net.GetPlayer().ID);
                w.Write(playerID);
                w.Write(quest.ID);
                if(net is Server server)
                {
                    manager.RemoveQuest(quest.ID);
                }
            }
            static void ReceiveRemoveQuest(IObjectProvider net, BinaryReader r)
            {
                var manager = net.Map.Town.QuestManager;
                var player = net.GetPlayer(r.ReadInt32());
                var questID = r.ReadInt32();
                if (net is Server server)
                    RemoveQuest(manager, player.ID, manager.GetQuest(questID)); // LOL 1
                else
                    manager.RemoveQuest(questID); // LOL 2
            }
        }

        readonly Lazy<Window> UIWindowQuests;
        //readonly Lazy<Control> UIEditQuest;
        readonly Lazy<Control> UIAssignQuests;
        //readonly Lazy<Control> UIVisitorQuestsView;

      
        public override string Name => "Quests";
        int QuestGiverIDSequence = 1;
        readonly List<QuestDef> Quests = new();
        readonly Dictionary<int, QuestGiverProperties> QuestGiverProperties = new();
        readonly Dictionary<int, int> PendingQuestRequests = new();
        static QuestsManager()
        {
            Packets.Init();
        }
        public QuestsManager(Town town) : base(town)
        {
            this.UIWindowQuests = new Lazy<Window>(this.CreateUI);
            //this.UIEditQuest = new Lazy<Control>(this.CreateUIEditQuest);
            this.UIAssignQuests = new Lazy<Control>(this.CreateQuestGiverAssignmentWindow);
            //this.UIVisitorQuestsView = new Lazy<Control>(this.CreateVisitorQuestsView);
            //var citizens = town.GetAgents();
            //foreach (var c in citizens)
            //    this.QuestGiverProperties.Add(c, new Start_a_Town_.QuestGiverProperties(c));
        }

        QuestDef CreateQuest()
        {
            var qg = new QuestDef(this, QuestGiverIDSequence++);
            //this.Givers.Add(qg);
            return qg;
        }
        
        private void AddQuest(QuestDef qg)
        {
            this.Quests.Add(qg);
            //this.Town.Net.EventOccured(Components.Message.Types.QuestGiverAdded, qg);
            //this.Town.Net.EventOccured(Components.Message.Types.QuestGiversUpdated, new QuestGiver[] { qg }, new QuestGiver[] { });
            this.ReportQuestsUpdated(new QuestDef[] { qg }, new QuestDef[] { });
        }
        private void AddQuest(int id)
        {
            if (this.Town.Map.Net is not Client)
                throw new Exception();
            this.AddQuest(new QuestDef(this, id));
        }
        private void RemoveQuest(int questID)
        {
            var qg = this.GetQuest(questID);
            this.Quests.Remove(qg);
            //this.Town.Net.EventOccured(Components.Message.Types.QuestGiverRemoved, qg);
            //this.Town.Net.EventOccured(Components.Message.Types.QuestGiversUpdated, new QuestGiver[] { }, new QuestGiver[] { qg });
            this.ReportQuestsUpdated(new QuestDef[] {  }, new QuestDef[] { qg });
        }
        void ReportQuestsUpdated(QuestDef[] added, QuestDef[] removed)
        {
            this.Town.Net.EventOccured(Components.Message.Types.QuestDefsUpdated, added, removed);
        }
        public QuestDef GetQuest(int id)
        {
            return this.Quests.FirstOrDefault(q => q.ID == id);
        }
        public IEnumerable<QuestDef> GetQuestDefs()
        {
            foreach (var g in this.Quests)
                yield return g;
        }
       
        internal void HandleQuestReceiver(Actor receiver, QuestDef q)
        {
            //q.HandleQuestReceiver(actor);
            var giver = q.Giver;
            this.GetQuestGiverProperties(q.Giver).HandleReceiver(receiver);
            this.PendingQuestRequests.Add(giver.RefID, receiver.RefID);
            //if(!this.QuestGiverProperties.TryGetValue(giver, out QuestGiverProperties props))
            //{
            //    props = new QuestGiverProperties(giver);
            //    this.QuestGiverProperties.Add(giver, props);
            //}
            //props.HandleReceiver(actor);
            //this.QuestGiverProperties[giver.InstanceID].HandleReceiver(actor);
        }
        public void RemoveQuestReceiver(QuestDef q)
        {
            //q.RemoveQuestReceiver(actor);
            var giver = q.Giver;
            this.GetQuestGiverProperties(q.Giver).RemoveReceiver();
            this.PendingQuestRequests.Remove(giver.RefID);
            //return;
            //this.QuestGiverProperties[giver.InstanceID].RemoveReceiver(actor);
        }
        public void RemoveQuestReceiver(int qID)
        {
            //q.RemoveQuestReceiver(actor);
            var giver = this.GetQuest(qID).Giver;
            this.GetQuestGiverProperties(giver).RemoveReceiver();

            this.PendingQuestRequests.Remove(giver.RefID);
            //return;
            //this.QuestGiverProperties[giver.InstanceID].RemoveReceiver(actor);
        }
        internal Actor GetNextQuestReceiver(Actor giver)
        {
            var id = this.QuestGiverProperties[giver.RefID].GetNextQuestReceiverID();
            return id != -1 ? this.Town.Net.GetNetworkObject(id) as Actor : null;
        }
        public IEnumerable<Actor> GetAllVisitorsOnQuest(QuestDef quest)
        {
            return this.Town.Map.World.Population.Find(v => v.HasQuest(quest)).Select(v => v.Actor);
        }
        public void QuestModified(QuestDef quest)
        {
            foreach (var v in this.Town.Map.World.Population.Find(v => v.HasQuest(quest)))
                v.AbandonQuest(quest);
        }
        internal QuestGiverProperties GetQuestGiverProperties(Actor actor)
        {
            return this.QuestGiverProperties[actor.RefID];
        }
        
        internal override void OnCitizenAdded(int actorID)
        {
            this.QuestGiverProperties.Add(actorID, new QuestGiverProperties(actorID));
        }
        internal override void OnCitizenRemoved(int actorID)
        {
            this.QuestGiverProperties.Remove(actorID);
        }
        internal override IEnumerable<Tuple<string, Action>> OnQuickMenuCreated()
        {
            //var win = new Lazy<Window>(this.CreateUI);
            var win = this.UIWindowQuests;
            yield return new Tuple<string, Action>("Quests", () => win.Value.Toggle());
        }
        internal override IEnumerable<(string name, Action action)> GetInfoTabs(ISelectable selected)
        {
            if (selected is not TargetArgs target)
                yield break;
            if (target.Object is not Actor actor)
                yield break;
            if (actor.IsCitizen)
            {
                //this.UIAssignQuests.Value.GetData(actor);
                yield return("Quests", () => ShowUI(this.UIAssignQuests, $"Assign quests to {actor.Name}"));
            }
            else
            {
                //this.UIVisitorQuestsView.Value.GetData(actor);
                //info.AddTabAction("Quests", () => ShowUI(ActorActiveQuestsGUI.GetData(actor), $"Quests received by {actor.Name}"));
            }
        }
        internal override void OnTargetSelected(IUISelection info, ISelectable selected)
        {
            if (selected is not TargetArgs target)
                return;
            if (target.Object is not Actor actor)
                return;
            if (actor.IsCitizen)
            {
                this.UIAssignQuests.Value.GetData(actor);
                //info.AddTabAction("Quests", () => ShowUI(this.UIAssignQuests, $"Assign quests to {actor.Name}"));
            }
            else
            {
            }
        }

        private void ShowUI(Lazy<Control> lazyUI, string title)
        {
            var window = lazyUI.Value.GetWindow() ?? lazyUI.Value.ToWindow(title);
            window.Toggle();
            //if (lazyUI.Value.DataSource == actor && window.IsOpen)
            //{
            //    window.Hide();
            //    return;
            //}
            //lazyUI.Value.GetData(actor);
            //window.Show();
        }
        private void ShowUI(Control gui, string title)
        {
            var window = gui.GetWindow() ?? gui.ToPanelLabeled().ToWindow(title);
            window.Toggle();
        }
        Window CreateUI()
        {
            var box = new GroupBox();
            //var uiEditQuest = CreateUIEditQuest();
            //var winEditQust = uiEditQuest.ToWindow();
            var qlist = new TableScrollableCompactNewNew<QuestDef>(10)
                .AddColumn(new(), "text", 150, qg => new Label(qg).SetLeftClickAction(lbl =>
                {
                    //uiEditQuest.GetData(qg);
                    //winEditQust.Show();
                    qg.ShowGUI();
                }))
                .AddColumn(new(), "text", Icon.X.AtlasToken.Rectangle.Width, (table, qg) => IconButton.CreateCloseButton().SetLeftClickAction(btn => Packets.RemoveQuest(this, this.Town.Net.GetPlayer().ID, qg)))
                ;

            var net = this.Town.Net;
            var btnCreate = new Button("Create", () => Packets.SendAddQuestGiver(net, net.GetPlayer().ID));
        
            qlist.AddItems(this.Quests);
            qlist.ListenTo(Components.Message.Types.QuestDefsUpdated, args =>
            {
                var added = args[0] as QuestDef[];
                var removed = args[1] as QuestDef[];
                qlist.AddItems(added);
                qlist.RemoveItems(removed);
            });
            
            box.AddControlsVertically(
                btnCreate,
                qlist.ToPanelLabeled("Quest list"));
            return box.ToWindow("Quests").MoveToScreenCenter() as Window;
        }
        

        Control CreateQuestGiverAssignmentWindow()
        {
            var box = new GroupBox();
            Actor actor = null;
            //var questsAvailable = new ListBoxNew<QuestDef, Button>(200, 100);
            //var questsAssigned = new ListBoxNew<QuestDef, Button>(200, 100);
            var questsAvailable = new TableScrollableCompactNewNew<QuestDef>(16)
                .AddColumn(new(), "questname", 200, q => new Label(q, () => edit(q)), 0)
                .AddColumn(new(), "assign", 16, q => IconButton.CreateSmall(Icon.ArrowRight, () => assign(q), $"Assign {q} to {actor.Name}"), 0);
            var editbtnwidth = Button.GetWidth(UIManager.Font, "Edit");
            var questsAssigned = new TableScrollableCompactNewNew<QuestDef>(16)
                .AddColumn(new(), "questname", 200, q => new Label(q, () => edit(q)), 0)
                .AddColumn(new(), "unassign", 16, q => IconButton.CreateSmall(Icon.Cross, () => unassign(q), $"Assign {q} to {actor.Name}"), 0);

            box.SetGetDataAction(o =>
            {
                if (o is not Actor a)
                    return;
                actor = a;
                questsAvailable.ClearItems();
                questsAssigned.ClearItems();

                var assigned = this.Quests.Where(q => q.Giver == a).ToList();
                questsAssigned.AddItems(assigned);
                questsAvailable.AddItems(this.Quests.Except(assigned));
                //foreach (var q in this.QuestDefs)
                //{
                //    if (q.Actor == a)
                //        questsAssigned.AddItems(a);
                //    else
                //        questsAvailable.AddItems(a);
                //}
            });

            box.AddControlsHorizontally(
                questsAvailable.ToPanelLabeled("Available")
                ,
                questsAssigned.ToPanelLabeled("Assigned")
                );
            box.ListenTo(Components.Message.Types.QuestDefAssigned, args =>
            {
                var q = args[0] as QuestDef;
                if (q.Giver == actor)
                {
                    questsAvailable.RemoveItems(q);
                    questsAssigned.AddItems(q);
                }
                else
                {
                    questsAssigned.RemoveItems(q);
                    questsAvailable.AddItems(q);
                }
            });
            return box;//.ToWindow("Assign quests");



            void edit(QuestDef q)
            {
                //this.UIEditQuest.Value.GetData(q);
                //var window = this.UIEditQuest.Value.GetWindow() ?? this.UIEditQuest.Value.ToWindow($"Edit {q}");
                //window.Show();
                q.ShowGUI();
            }
            void assign(QuestDef q)
            {
                var net = this.Town.Map.Net;
                Packets.SendQuestGiverAssign(net, net.GetPlayer(), q, actor);
            }
            void unassign(QuestDef q)
            {
                var net = this.Town.Map.Net;
                Packets.SendQuestGiverAssign(net, net.GetPlayer(), q, null);
            }
        }
        //Control CreateVisitorQuestsView()
        //{
        //    var box = new GroupBox();

        //    var qlist = new ListBoxNewNoBtnBase<QuestDef, Label>(250, 250, q => new Label(q));

        //    box.SetGetDataAction(o =>
        //    {
        //        var actor = o as Actor;
        //        if (box.Tag == actor)
        //            return;
        //        box.Tag = actor;
        //        var props = actor.GetVisitorProperties();
        //        var quests = props.GetQuests();
        //        qlist.Clear();
        //        qlist.AddItems(quests);
        //    });

        //    box.AddControlsVertically(qlist.ToPanelLabeled("Quests"));

        //    return box;
        //}

        static ListBoxObservable<QuestDef, Label> _ActorActiveQuestsGUI;
        static public ListBoxObservable<QuestDef, Label> ActorActiveQuestsGUI
        {
            get
            {
                var boxw = 250;
                var boxh = 250;

                return _ActorActiveQuestsGUI ??=
                    new ListBoxObservable<QuestDef, Label>(boxw, boxh, q => new Label(q.Name, q.ShowGUI))
                    { Name = "Active quests" }
                    .SetGetDataAction(o => ActorActiveQuestsGUI.Bind((o as Actor).GetVisitorProperties().Quests)) as ListBoxObservable<QuestDef, Label>;
            }
        }

        static Control ActorGUI;
        static Control CreateActorGUI()
        {
            //var boxw = 300;
            //var boxh = 300;

            //var listQuests = new ListBoxNewNoBtnBase<QuestDef, Label>(boxw, boxh, q => new Label(q.Name)) { Name = "Quests" };
            //var listQuests = ActorActiveQuestsGUI ??= 
            //    new ListBoxObservable<QuestDef, Label>(boxw, boxh, q => new Label(q.Name, q.ShowGUI))
            //    { Name = "Active quests" }
            //    .SetGetDataAction(o => ActorActiveQuestsGUI.Bind((o as Actor).GetVisitorProperties().Quests)) as ListBoxObservable<QuestDef, Label>;
            var listQuests = ActorActiveQuestsGUI;
            var box = UIHelper.ToTabbedContainer(listQuests);

            box.SetGetDataAction(o =>
            {
                listQuests.GetData(o);
                var props = (o as Actor).GetVisitorProperties();
                //listQuests.Bind(props.Quests);
                box.GetWindow().SetTitle(props.Actor.Name);
            });
            box.ToWindow("Visitor Properties");
            return box;
        }
        static Control GetActorGUI()
        {
            return ActorGUI ??= CreateActorGUI();
            //gui.GetData(actor);
        }
        internal void ShowActorGUI(Actor actor)
        {
            var gui = GetActorGUI();// GUI ??= CreateGUI();
            gui.GetData(actor);
            gui.GetWindow().Show();
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this.QuestGiverIDSequence);
            this.Quests.Write(w);
            this.PendingQuestRequests.Write(w);
        }
        public override void Read(BinaryReader r)
        {
            this.QuestGiverIDSequence = r.ReadInt32();
            //this.QuestDefs.ReadMutable(r);
            //this.QuestDefs.Initialize(r);
            this.Quests.InitializeNew(r, this);
            this.PendingQuestRequests.Read(r);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            this.QuestGiverIDSequence.Save(tag, "QuestIDSequence");
            this.Quests.SaveNewBEST(tag, "Quests");
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue("QuestIDSequence", out this.QuestGiverIDSequence);
            this.Quests.TryLoadList<List<QuestDef>, QuestDef>(tag, "Quests", this);
        }
    }
}
