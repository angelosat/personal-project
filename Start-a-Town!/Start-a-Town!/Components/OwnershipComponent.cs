using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class OwnershipComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Ownership";
            }
        }

        public int Owner { get; private set; } = -1;

        public new OwnershipComponent Initialize(GameObject owner = null)
        {
            this.Owner = owner == null ? -1 : owner.RefID;
            return this;
        }
        public OwnershipComponent()
        {

        }
        //OwnershipComponent(GameObject owner = null)
        //{
        //    this.Owner = owner == null ? -1 : owner.InstanceID;
        //}
        OwnershipComponent(int owner)
        {
            this.Owner = owner;
        }
        //public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        //{
        //    switch (e.Type)
        //    {
        //        case Message.Types.SetOwner:
        //            GameObject newOwner = e.Parameters[0] as GameObject;
        //            //if (Owner != null)
        //            if (this.Owner != newOwner.InstanceID)
        //            {
        //                //   Owner.HandleMessage(Message.Types.ModifyNeed, parent, "Bed", -100);
        //                //Owner.PostMessage(Message.Types.Ownership, parent, parent);
        //                e.Network.PostLocalEvent(parent.Net.GetNetworkObject(this.Owner), ObjectEventArgs.Create(Message.Types.Ownership, new TargetArgs(parent), parent));
        //            }
        //            Owner = newOwner.InstanceID;
        //            if (Owner != null)
        //            {
        //                //    Owner.HandleMessage(Message.Types.ModifyNeed, parent, "Bed", 100);
        //                //Owner.PostMessage(Message.Types.Ownership, parent, parent);
        //                e.Network.PostLocalEvent(parent.Net.GetNetworkObject(this.Owner), ObjectEventArgs.Create(Message.Types.Ownership, new TargetArgs(parent), parent));
        //            }
        //            break;
        //        case Message.Types.Constructed:
        //            Owner = (e.Parameters[0] as GameObject).InstanceID;
        //            break;
        //        //case Message.Types.Query:
        //        //    Query(parent, e);
        //        //    break;
        //        default:
        //            return false;
        //    }
        //    return true;
        //}

        //public override void Query(GameObject parent, List<InteractionOld> list)
        //{
        //    list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.ManageEquipment, source: parent, name: "Set owner", range: (a1, a2) => true));
        //}

        public override object Clone()
        {
            return new OwnershipComponent(this.Owner);
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Owner);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Owner = r.ReadInt32();
        }
        internal override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.Owner.Save("Owner"));
        }
        internal override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<int>("Owner", v => this.Owner = v);
        }

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            if (parent.Net == null)
                return;
            var owner = parent.Net.GetNetworkObject(this.Owner);
            //tooltip.Controls.Add(new UI.Label(tooltip.Controls.Count > 0 ? tooltip.Controls.Last().BottomLeft : Vector2.Zero, "Owner: " + (owner != null ? owner.Name : "<None>"), fill: Color.Lime));
            tooltip.AddControlsBottomLeft(new UI.Label("Owner: " + (owner != null ? owner.Name : "<None>"), fill: Color.Lime));
        }

        static public bool Owns(GameObject owner, GameObject obj)
        {
            if (!obj.TryGetComponent("Ownership", out OwnershipComponent ownership))
                throw new Exception();// return false;
            //if (ownership.Owner == null)
            //    return true;
            return ownership.Owner == owner.RefID;
        }

        internal override void GetManagementInterface(GameObject gameObject, Control box)
        {

            var setOwnerBtn = new Button("Set Owner")
            {
                //Location = lblOwner.BottomLeft,
                LeftClickAction = () =>
                {
                    var listNpc = new ListBoxNewNoBtnBase<GameObject, Label>(150, 400);
                    listNpc.AddItem(null, (o, l) =>
                    {
                        l.Text = "None";
                        l.LeftClickAction = () =>
                        {
                            PacketPlayerSetItemOwner.Send(Net.Client.Instance, gameObject.RefID, -1);
                        };
                    });
                    foreach (var npc in gameObject.Map.Town.GetAgents())
                    {
                        listNpc.AddItem(npc, (o, l) =>
                        {
                            l.Text = o.Name;
                            l.LeftClickAction = () =>
                            {
                                PacketPlayerSetItemOwner.Send(Net.Client.Instance, gameObject.RefID, o.RefID);
                            };
                        });
                    }
                    listNpc.Toggle();
                }
            };
            //var listbox = new ListBox<GameObject, Button>(150, 400);
            var alllist = new List<GameObject>() { null };
            alllist.AddRange(gameObject.Map.Town.GetAgents());
            //listbox.Build(alllist, ob => ob != null ? ob.Name : "none", (o, b) =>
            //{
            //    b.LeftClickAction = () => {
            //        Components.Inventory.PacketPlayerSetItemOwner.Send(Client.Instance, gameObject.InstanceID, o != null ? o.InstanceID : -1);               
            //    };
            //});
            var comp = gameObject.GetComponent<OwnershipComponent>();
            var lblOwner = new Label("Owner: ");// {  TextFunc = () => (comp.Owner == null ? "none" : comp.Owner.Name) };
            //var setownercombo = new ComboBox<GameObject>(listbox, (A) => { return A != null ? A.Name : "None"; }) { 
            //    //Location = setOwnerBtn.BottomLeft,
            //    Location = lblOwner.TopRight,
            //    TextFunc = () => (comp.Owner == null ? "none" : comp.Owner.Name) };
            var setownercombo = new ComboBox<GameObject>(alllist, 150, 400,
                (A) => { return A != null ? A.Name : "None"; },
                (o, b) =>
                {
                    b.LeftClickAction = () =>
                    {
                        PacketPlayerSetItemOwner.Send(Net.Client.Instance, gameObject.RefID, o != null ? o.RefID : -1);
                    };
                }
                )
            {
                //Location = setOwnerBtn.BottomLeft,
                Location = lblOwner.TopRight,
                //TextFunc = () => (comp.Owner == null ? "none" : comp.Owner.Name)
                TextFunc = () => (comp.Owner == -1 ? "none" : gameObject.Net.GetNetworkObject(comp.Owner).Name)
            };

            //Label lblOwner = new Label("Owner: ") { TextFunc = () => "Owner: " + (comp.Owner == null ? "" : comp.Owner.Name) };
            setownercombo.OnGameEventAction = (a) =>
            {
                switch (a.Type)
                {
                    case Message.Types.NpcsUpdated:
                        alllist.Clear();
                        alllist.Add(null);
                        alllist.AddRange(gameObject.Map.Town.GetAgents());
                        //listbox.Build(alllist, ob => ob != null ? ob.Name : "none", (o, b) =>
                        //setownercombo.List.Build(alllist, ob => ob != null ? ob.Name : "none", (o, b) =>
                        //{
                        //    b.LeftClickAction = () => {
                        //        Components.Inventory.PacketPlayerSetItemOwner.Send(Client.Instance, gameObject.InstanceID, o != null ? o.InstanceID : -1);
                        //    };
                        //});
                        setownercombo.List.Build(alllist, setownercombo.List.SelectedTextFunc, setownercombo.List.OnControlInit);
                        break;
                    default:
                        break;
                }
            };
            //Label lblOwner = new Label("Owner: ") { Location = setOwnerBtn.TopRight, TextFunc = () => (comp.Owner == null ? "none" : comp.Owner.Name) };
            box.AddControls(
                //setOwnerBtn, 
                lblOwner, setownercombo);
        }


        public void SetOwner(GameObject parent, int actorID)
        {
            //if (this.Owner != -1)
            //{
            //    if (this.Owner != actor.InstanceID)
            //        NpcComponent.RemovePossesion(parent.Net.GetNetworkObject(this.Owner), parent);
            //    this.Owner = actor.InstanceID;
            //}
            this.Owner = actorID;// actor != null ? actor.InstanceID : -1;
            parent.Net.EventOccured(Message.Types.ItemOwnerChanged, parent.RefID);
        }
        static Control ActorList;
        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(new Label() { TextFunc = () => string.Format("Assigned to {0}", parent.Town.GetAgents().FirstOrDefault(a => a.GetPossesions().Contains(parent))?.Name ?? "none") });
            // TODO 
            //ActorList = new ListBoxNew<Actor, Button>(200, 200, a => new Button(a?.Name ?? "none", () => PacketPlayerSetItemOwner.Send(Net.Client.Instance, parent.RefID, a?.RefID ?? -1)))//.AddItem(null, a=>"None", (a, b) => b.LeftClickAction = () => PacketPlayerSetItemOwner.Send(Net.Client.Instance, parent.RefID, -1))
            //                                                                                                                                                                               //.Build(parent.Town.GetAgents(), a => a.Name, (a, b) => b.LeftClickAction= ()=> PacketPlayerSetItemOwner.Send(Net.Client.Instance, parent.RefID, a.RefID))// .GetPossesions().Add(parent))
            //                                                   .AddItems(parent.Town.GetAgents().Prepend(null))//, a => a?.Name ?? "none", (a, b) => b.LeftClickAction= ()=> PacketPlayerSetItemOwner.Send(Net.Client.Instance, parent.RefID, a?.RefID ?? -1))// .GetPossesions().Add(parent))
            //                                                   .ToPanelLabeled("Select owner")
            //                                                   .HideOnRightClick()
            //                                                   .HideOnLeftClick()
            //                                                   ;

            //info.AddTabAction("Owner", () => ActorList.SetLocation(UIManager.Mouse).Toggle());
        }
        internal override IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            var parent = this.Parent;
            ActorList = new ListBoxNew<Actor, Button>(200, 200, a => new Button(a?.Name ?? "none", () => PacketPlayerSetItemOwner.Send(Net.Client.Instance, parent.RefID, a?.RefID ?? -1)))//.AddItem(null, a=>"None", (a, b) => b.LeftClickAction = () => PacketPlayerSetItemOwner.Send(Net.Client.Instance, parent.RefID, -1))
                                                                                                                                                                                           //.Build(parent.Town.GetAgents(), a => a.Name, (a, b) => b.LeftClickAction= ()=> PacketPlayerSetItemOwner.Send(Net.Client.Instance, parent.RefID, a.RefID))// .GetPossesions().Add(parent))
                                                               .AddItems(parent.Town.GetAgents().Prepend(null))//, a => a?.Name ?? "none", (a, b) => b.LeftClickAction= ()=> PacketPlayerSetItemOwner.Send(Net.Client.Instance, parent.RefID, a?.RefID ?? -1))// .GetPossesions().Add(parent))
                                                               .ToPanelLabeled("Select owner")
                                                               .HideOnRightClick()
                                                               .HideOnLeftClick()
                                                               ;

            yield return ("Owner", () => ActorList.SetLocation(UIManager.Mouse).Toggle());
        }
    }
}
