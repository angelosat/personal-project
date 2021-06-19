using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Components.Resources;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class EquipComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Equippable";
            }
        }

        public Dictionary<Stat.Types, float> Stats { get { return (Dictionary<Stat.Types, float>)this["Stats"]; } set { this["Stats"] = value; } }
        //public List<Script.Types> Abilities { get { return (List<Script.Types>)this["Abilities"]; } set { this["Abilities"] = value; } }
        public GearType Type { get { return (GearType)this["Type"]; } set { this["Type"] = value; } }
        //public ResourceDef Durability { get { return (Durability)this["Durability"]; } set { this["Durability"] = value; } }
        public Resource Durability;
        public EquipComponent()
        {
            //this.Abilities = new List<Script.Types>();
            this.Stats = new Dictionary<Stat.Types, float>();
            this.Type = null;
            this.Durability = new Resource(ResourceDef.Durability);
                //ResourceDef.Create(ResourceDef.ResourceTypes.Durability, 20, 20);// new Durability() { Max = 20, Value = 20 };


            // TODO: put default values instead of null
            //Properties.Add("Slot", null);
            //this.Stats = new Dictionary<Stat.Types, float>();
            //this.Durability = Resource.Create(Resource.Types.Durability, 20);
            //Properties.Add("Use", null);
        }

        public EquipComponent Initialize(GearType slot)//, params Script.Types[] abilities)
        {
            this.Type = slot;
            return this;
            //return AddAbilities(abilities);
        }

        public EquipComponent Initialize(params Tuple<Stat.Types, float>[] stats)
        {
            foreach (var stat in stats)
                this.Stats[stat.Item1] = stat.Item2;
            return this;
        }
        //public EquipComponent Initialize(GearType slot, IEnumerable<Script.Types> abilities, Dictionary<Stat.Types, float> stats)
        //{
        //    this.Type = slot;
        //    this.AddAbilities(abilities);
        //    foreach (var stat in stats)
        //        this.Stats[stat.Key] = stat.Value;
        //    return this;
        //}
        public bool Equip(GameObject actor, GameObject parent)
        {
            InventoryComponent inv;
            if (actor.TryGetComponent<InventoryComponent>("Inventory", out inv))
            {
                //StatsComponent parentstats, actorstats;
                //if (actor.TryGetComponent<StatsComponent>("Stats", out actorstats))
                //{
                //    // TODO: combine stats in the same component maybe
                //    if (parent.TryGetComponent<StatsComponent>("Stats", out parentstats))
                //    {
                //        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                //        {
                //            //actorstats.Parameters[parameter.Key].Value += parameter.Value.Value;
                //            actorstats.Properties[parameter.Key] = actorstats.GetProperty<float>(parameter.Key) + (float)parameter.Value;
                //            // Console.WriteLine(actorstats.Parameters[parameter.Key].Value);
                //        }
                //    }

                //    if (parent.TryGetComponent<StatsComponent>("Skills", out parentstats))
                //        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                //            actorstats.Properties[parameter.Key] = actorstats.GetProperty<float>(parameter.Key) + (float)parameter.Value;

                //    if (parent.TryGetComponent<StatsComponent>("Damage", out parentstats))
                //        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                //            actorstats.Properties[parameter.Key] = actorstats.GetProperty<float>(parameter.Key) + (float)parameter.Value;
                //}
                StatsComponent parentstats;
                if (parent.TryGetComponent<StatsComponent>("Skills", out parentstats))
                    //if (actor.TryGetComponent<StatsComponent>("Skills", out actorstats))
                    if (actor.Components.ContainsKey("Skills"))
                        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                            actor["Skills"][parameter.Key] = (float)actor["Skills"][parameter.Key] + (float)parameter.Value;
                if (parent.TryGetComponent<StatsComponent>("Damage", out parentstats))
                    // if (actor.TryGetComponent<StatsComponent>("Damage", out actorstats))
                    if (actor.Components.ContainsKey("Stats"))
                        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                            actor["Stats"][parameter.Key] = (float)actor["Stats"][parameter.Key] + (float)parameter.Value;
                //actorstats.Properties[parameter.Key] = actorstats.GetProperty<float>(parameter.Key) + (float)parameter.Value;
                //foreach (GameObjectSlot abilitySlot in parent["Abilities"].GetProperty<List<GameObjectSlot>>("Abilities"))
                //{

                //}
                return true;
            }
            return false;
        }

        public bool Unequip(GameObject actor, GameObject parent)
        {
            InventoryComponent inv;
            if (actor.TryGetComponent<InventoryComponent>("Inventory", out inv))
            {
                // inv.Equipment[Slot].Object = parent;

                //StatsComponent parentstats, actorstats;
                //if (actor.TryGetComponent<StatsComponent>("Stats", out actorstats))
                //{
                //    if (parent.TryGetComponent<StatsComponent>("Stats", out parentstats))
                //    {
                //        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                //        {
                //            //actorstats.Parameters[parameter.Key].Value += parameter.Value.Value;
                //            actorstats.Properties[parameter.Key] = actorstats.GetProperty<float>(parameter.Key) - (float)parameter.Value;
                //            //Console.WriteLine(actorstats.Parameters[parameter.Key].Value);
                //        }
                //    }

                //    if (parent.TryGetComponent<StatsComponent>("Skills", out parentstats))
                //        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                //            actorstats.Properties[parameter.Key] = actorstats.GetProperty<float>(parameter.Key) - (float)parameter.Value;

                //    if (parent.TryGetComponent<StatsComponent>("Damage", out parentstats))
                //        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                //            actorstats.Properties[parameter.Key] = actorstats.GetProperty<float>(parameter.Key) - (float)parameter.Value;
                //}

                StatsComponent parentstats;
                if (parent.TryGetComponent<StatsComponent>("Skills", out parentstats))
                    if (actor.Components.ContainsKey("Skills"))
                        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                            if (actor["Skills"].Properties.ContainsKey(parameter.Key)) // TODO
                                actor["Skills"][parameter.Key] = (float)actor["Skills"][parameter.Key] - (float)parameter.Value;
                if (parent.TryGetComponent<StatsComponent>("Damage", out parentstats))
                    if (actor.Components.ContainsKey("Stats"))
                        foreach (KeyValuePair<string, object> parameter in parentstats.Properties)
                            if (actor["Stats"].Properties.ContainsKey(parameter.Key))
                                actor["Stats"][parameter.Key] = (float)actor["Stats"][parameter.Key] - (float)parameter.Value;

                return true;
            }
            return false;
        }


        public override string GetTooltipText()
        {
            return "Right click: Equip";
        }



        public override object Clone()
        {
            throw new Exception();
            //EquipComponent phys = new EquipComponent().Initialize(this.Type, this.Abilities, this.Stats);
            //phys.Durability = new Resource(ResourceDef.Durability);

            //return phys;

        }

        //public override string GetInventoryText(GameObject actor)
        //{
        //    return "Right click: Equip";
        //}

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            InventoryComponent inv;
            GameObjectSlot slot;
            BodyComponent body;
            BodyPart bodyPart;
            string slotName;
            switch (msg)
            {
                //case Message.Types.Wear:
                //    //throw new NotImplementedException();
                //    //e.Sender.PostMessage(Message.Types.EquipItem, parent, parent.Exists ? parent.ToSlot() : InventoryComponent.GetFirstOrDefault(e.Sender, foo => foo == parent));

                //    return true;

                case Message.Types.Equip:

                    //GameObject.PostMessage(e.Data.Translate<SenderEventArgs>().Sender, Message.Types.Hold, parent, parent.Exists ? parent.ToSlot() : InventoryComponent.GetFirstOrDefault(e.Sender, foo => foo == parent), parent);
                    // TODO: find out how to handle slots in network
                    sender = e.Parameters[0] as GameObject;
                    e.Network.PostLocalEvent(sender, ObjectEventArgs.Create(Message.Types.Hold, new object[] { parent }));
                    //e.Data.Translate<SenderEventArgs>(e.Network).Sender.PostMessage(e.Network, Message.Types.Hold, w =>
                    //{
                    //    w.Write(parent.NetworkID);
                    //});

                   
                    return true;

                //case Message.Types.Unequip:
                //    //      UI.NotificationArea.Write("Unequipped: " + parent.GetInfo().GetProperty<string>("Name"));
                //    inv = sender.GetComponent<InventoryComponent>("Inventory");
                //    body = sender.GetComponent<BodyComponent>("Equipment");
                //    //slot = inv.GetProperty<Dictionary<string, GameObjectSlot>>("Equipment")[GetProperty<string>("Slot")];
                //    slotName = GetProperty<string>("Slot");
                //    BodyPart bp = body.GetProperty<BodyPart>(slotName);

                //    bodyPart = body.GetProperty<BodyPart>(slotName);
                //    //if (inv.TryGive(slot.Object))

                //    if (parent == bodyPart.Base.Object)
                //        return false;
                //    if (inv.TryGive(bodyPart.Wearing.Object))
                //    {
                //        //slot.Object = null;
                //        bodyPart.Wearing.Object = null;
                //        Unequip(sender, parent);
                //        if (bodyPart.Base != null)
                //            Equip(sender, bodyPart.Base.Object);
                //        Log.Enqueue(Log.EntryTypes.Unequip, sender, parent, bp.Object);
                //    }
                //    return false;

                default:
                    return false;
            }
        }

        //public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        //{
        //    //  List<Interaction> list = e.Parameters[0] as List<Interaction>;
        //    //list.Add(new Interaction(new TimeSpan(0, 0, 0, 0, 500), Message.Types.Equip, name: "Equip", effect: new InteractionEffect("Equipped"), source: parent));
        //    list.Add(new InteractionOld(new TimeSpan(0, 0, 0, 0, 500), Message.Types.Equip, name: "Equip",
        //        range: (actor, target) =>
        //        {
        //            return target.IsSpawned ? InteractionOld.DefaultRangeCheck(actor, target) : InventoryComponent.HasObject(actor, foo => foo == target);
        //        }, //range: -1,
        //        effect: new NeedEffectCollection() { new AIAdvertisement("Equipped") },
        //        source: parent
        //        //cond: new InteractionCondition("Holding", true, AI.PlanType.FindNearest, finalCondition: agent => InventoryComponent.HasObject(agent, obj => obj == parent))));
        //        //));
        //        ));
        //    //cond: new InteractionConditionCollection(
        //    //new InteractionCondition(agent => InventoryComponent.HasObject(agent, obj => obj == parent),
        //    //    new Precondition("Holding", i => i.Source == parent, AI.PlanType.FindNearest)))));

        //    list.Add(new InteractionOld(new TimeSpan(0, 0, 0, 0, 500), Message.Types.Wear, name: "Wear", range: (actor, target) =>
        //    {
        //        return target.IsSpawned ? InteractionOld.DefaultRangeCheck(actor, target) : InventoryComponent.HasObject(actor, foo => foo == target);
        //        //return InventoryComponent.HasObject(a1, foo => foo == a2);
        //    },
        //      //  effect: new NeedEffectCollection() { new NeedEffect("Equipped") },
        //        source: parent
        //        ));
        //}

        //public EquipComponent AddAbilities(params Script.Types[] scripts)
        //{
        //    this.Abilities.AddRange(scripts);
        //    return this;
        //}
        //public EquipComponent AddAbilities(IEnumerable<Script.Types> scripts)
        //{
        //    this.Abilities.AddRange(scripts);
        //    return this;
        //}
        static public EquipComponent Add(GameObject obj, params Tuple<Stat.Types, float>[] stats)
        {
            EquipComponent equip;
            if (!obj.TryGetComponent<EquipComponent>("Equip", out equip))
            {
                equip = new EquipComponent();
                obj["Equip"] = equip;
            }
            foreach (var stat in stats)
                equip.Stats[stat.Item1] = stat.Item2;
            return equip;
        }

        public override string GetStats()
        {
            string text = "";
            foreach(var stat in this.Stats)
            {
                var st = Stat.GetStat(stat.Key);
                text += stat.Value.ToString("##0.##" + (st.Type == Stat.BonusType.Percentile ? "%" : "")) + " " + st.Name;
            }
            return text;
        }

        static public void GetStats(GameObjectSlot objSlot, StatCollection stats)
        {
            if (!objSlot.HasValue)
                return;
            GetStats(objSlot.Object, stats);
        }
        static public void GetStats(GameObject obj, StatCollection stats)
        {
            EquipComponent eq;
            if (!obj.TryGetComponent<EquipComponent>("Equip", out eq))
                return;
            foreach (var stat in eq.Stats)
                stats[stat.Key] = stats.GetValueOrDefault(stat.Key) + stat.Value;
        }

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            //foreach (var stat in this.Stats)
            //{
            //    float value = stat.Value;
            //    var st = Stat.GetStat(stat.Key);
            //    bool good = value > 0;
            //    tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, stat.Value.ToString(st.Type == Stat.BonusType.Percentile ? "+##0.##%;-##0.##%;+0%" : "+##0.##;-##0.##;+0") + " " + st.Name)
            //    {
            //        TextColorFunc = () => good ? Color.Lime : Color.Red
            //    });
            //}

            tooltip.Controls.Add(new Label(this.Durability.ToString())
            {
                Location = tooltip.Controls.BottomLeft,
                Font = UIManager.FontBold,
                TextColorFunc = () =>
                {
                    //return Color.Lerp(Color.Red, Color.Lime, this.Durability.Percentage);
                    if (this.Durability.Percentage > 0.5)
                        return Color.Lerp(Color.Yellow, Color.Lime, (this.Durability.Percentage - 0.5f) * 2);
                    else
                        return Color.Lerp(Color.Red, Color.Yellow, this.Durability.Percentage * 2);
                }
            });

            //tooltip.Controls.Add("Equipping grants:".ToLabel(tooltip.Controls.BottomLeft));
            //foreach (var index in this.Abilities)
            //{
            //    Script script = Script.Registry[index];
            //    tooltip.Controls.Add(script.Name.ToLabel(tooltip.Controls.BottomLeft));
            //}
        }

        //static public string GetSlot(GameObject obj)
        //{
        //    EquipComponent eq;
        //    if (!obj.TryGetComponent<EquipComponent>("Equip", out eq))
        //        return "";
        //    return eq.Slot;
        //}

        public override void GetInteractions(GameObject parent, List<Interaction> actions)
        {
            //actions.Add(new Interactions.Equip());
            //actions.Add(new Interactions.EquipFromWorld());
            actions.Add(new Equip());
            actions.Add(new EquipFromInventory());
            //actions.Add(new Unequip());
        }

        public override void GetInventoryContext(GameObject actor, List<ContextAction> actions, int inventorySlotID)
        {
            actions.Add(new ContextAction(() => "Hold", () =>
            {
                Net.Client.PostPlayerInput(Message.Types.HoldInventoryItem, w => w.Write(inventorySlotID));
                //return true;
            }));
            actions.Add(new ContextAction(() => "Equip", () =>
            {
                Net.Client.PostPlayerInput(Message.Types.EquipInventoryItem, w => w.Write(inventorySlotID));
                //return true;
            }));
        }

        public override void GetInventoryActions(GameObject actor, GameObjectSlot parentSlot, List<ContextAction> actions)
        {
            var work = new EquipFromInventory();
            actions.Add(new ContextAction(() => work.Name, () => actor.GetComponent<WorkComponent>().Perform(actor, work, new TargetArgs(parentSlot))));
        }

        //public override void GetPlayerActions(Dictionary<KeyBinding, Interaction> list)
        //{
        //    list.Add(new KeyBinding(GlobalVars.KeyBindings.Activate, true), new Equip());
        //}
        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> list)
        {
            //list.Add(PlayerInput.ActivateHold, new EquipFromWorld());// new Equip());
            list.Add(PlayerInput.ActivateHold, new Equip());// new Equip());

        }
        internal override ContextAction GetContextActivate(GameObject parent, GameObject player)
        {
            return new ContextAction(new Equip()) { Shortcut = PlayerInput.ActivateHold };
        }
        //internal override List<SaveTag> Save()
        //{
        //    //var list = new List<SaveTag>() { this.Durability.Save("Durability") };
        //    //return list;
        //}
        //internal override IEnumerable<ContextAction> GetInventoryContextActions(GameObject actor)
        //{
        //    yield return new ContextAction(() => "Equip", () => "test equip context".ToConsole());
        //}
        internal override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
        }
        internal override void Load(SaveTag save)
        {
            //save.TryGetTag("Durability", tag => this.Durability = ResourceDef.Create(tag));
        }

        public override void Write(System.IO.BinaryWriter io)
        {
            //this.Durability.Write(io);
        }
        public override void Read(System.IO.BinaryReader io)
        {
            //this.Durability.Read(io);
        }
    }
}
