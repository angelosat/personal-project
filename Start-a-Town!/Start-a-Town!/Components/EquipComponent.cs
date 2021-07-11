using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class EquipComponent : EntityComponent
    {
        public override string ComponentName => "Equippable";
          

        public Dictionary<Stat.Types, float> Stats;
        public GearType Type;
        public Resource Durability;
        public EquipComponent()
        {
            this.Stats = new Dictionary<Stat.Types, float>();
            this.Type = null;
            this.Durability = new Resource(ResourceDef.Durability);
        }

        public EquipComponent Initialize(GearType slot)
        {
            this.Type = slot;
            return this;
        }

        public EquipComponent Initialize(params Tuple<Stat.Types, float>[] stats)
        {
            foreach (var stat in stats)
                this.Stats[stat.Item1] = stat.Item2;
            return this;
        }
       
        public override string GetTooltipText()
        {
            return "Right click: Equip";
        }

        public override object Clone()
        {
            throw new Exception();
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
            tooltip.Controls.Add(new Label(this.Durability.ToString())
            {
                Location = tooltip.Controls.BottomLeft,
                Font = UIManager.FontBold,
                TextColorFunc = () =>
                {
                    if (this.Durability.Percentage > 0.5)
                        return Color.Lerp(Color.Yellow, Color.Lime, (this.Durability.Percentage - 0.5f) * 2);
                    else
                        return Color.Lerp(Color.Red, Color.Yellow, this.Durability.Percentage * 2);
                }
            });
        }

        public override void GetInteractions(GameObject parent, List<Interaction> actions)
        {
            actions.Add(new Equip());
            actions.Add(new EquipFromInventory());
        }

        public override void GetInventoryActions(GameObject actor, GameObjectSlot parentSlot, List<ContextAction> actions)
        {
            var work = new EquipFromInventory();
            actions.Add(new ContextAction(() => work.Name, () => actor.GetComponent<WorkComponent>().Perform(actor, work, new TargetArgs(parentSlot))));
        }

        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> list)
        {
            list.Add(PlayerInput.ActivateHold, new Equip());
        }
        internal override ContextAction GetContextActivate(GameObject parent, GameObject player)
        {
            return new ContextAction(new Equip()) { Shortcut = PlayerInput.ActivateHold };
        }
       
        internal override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
        }
        internal override void Load(SaveTag save)
        {
        }

        public override void Write(System.IO.BinaryWriter io)
        {
        }
        public override void Read(System.IO.BinaryReader io)
        {
        }
    }
}
