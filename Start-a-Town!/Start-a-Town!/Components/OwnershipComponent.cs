﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class OwnershipComponent : EntityComponent
    {
        public override string ComponentName => "Ownership";
        public int Owner { get; private set; } = -1;

        public new OwnershipComponent Initialize(GameObject owner = null)
        {
            this.Owner = owner == null ? -1 : owner.RefID;
            return this;
        }
        public OwnershipComponent()
        {

        }
        
        OwnershipComponent(int owner)
        {
            this.Owner = owner;
        }

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
            tooltip.AddControlsBottomLeft(new UI.Label("Owner: " + (owner != null ? owner.Name : "<None>"), fill: Color.Lime));
        }

        static public bool Owns(GameObject owner, GameObject obj)
        {
            if (!obj.TryGetComponent("Ownership", out OwnershipComponent ownership))
                throw new Exception();
            return ownership.Owner == owner.RefID;
        }

        internal override void GetManagementInterface(GameObject gameObject, Control box)
        {
            var setOwnerBtn = new Button("Set Owner")
            {
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
            var alllist = new List<GameObject>() { null };
            alllist.AddRange(gameObject.Map.Town.GetAgents());
            
            var comp = gameObject.GetComponent<OwnershipComponent>();
            var setownercombo = new ComboBoxNewNew<GameObject>(150, "Owner",
                A => A?.Name ?? "None",
                o => PacketPlayerSetItemOwner.Send(Net.Client.Instance, gameObject.RefID, o != null ? o.RefID : -1),
                () => comp.Owner == -1 ? null : gameObject.Net.GetNetworkObject(comp.Owner),
                () => alllist.Prepend(null));

            setownercombo.OnGameEventAction = a =>
            {
                switch (a.Type)
                {
                    case Message.Types.NpcsUpdated:
                        alllist.Clear();
                        alllist.Add(null);
                        alllist.AddRange(gameObject.Map.Town.GetAgents());
                        break;
                    default:
                        break;
                }
            };
            box.AddControls(setownercombo);
        }

        public void SetOwner(GameObject parent, int actorID)
        {
            this.Owner = actorID;
            parent.Net.EventOccured(Message.Types.ItemOwnerChanged, parent.RefID);
        }
        static Control ActorList;
        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(new Label() { TextFunc = () => string.Format("Assigned to {0}", parent.Town.GetAgents().FirstOrDefault(a => a.GetPossesions().Contains(parent))?.Name ?? "none") });
        }
        internal override IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            var parent = this.Parent;
            ActorList = new ListBoxNew<Actor, Button>(200, 200, a => new Button(a?.Name ?? "none", () => PacketPlayerSetItemOwner.Send(Net.Client.Instance, parent.RefID, a?.RefID ?? -1)))
                                                               .AddItems(parent.Town.GetAgents().Prepend(null))
                                                               .ToPanelLabeled("Select owner")
                                                               .HideOnRightClick()
                                                               .HideOnLeftClick()
                                                               ;

            yield return ("Owner", () => ActorList.SetLocation(UIManager.Mouse).Toggle());
        }
    }
}
