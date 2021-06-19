using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    

    class FunctionComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Function";
            }
        }

        GameObject Primary { get { return (GameObject)this["Primary"]; } set { this["Primary"] = value; } }
        GameObject Secondary { get { return (GameObject)this["Secondary"]; } set { this["Secondary"] = value; } }


        //List<Message.Types> Abilities { get { return (List<Message.Types>)this["Abilities"]; } set { this["Abilities"] = value; } }
        List<GameObjectSlot> Abilities { get { return (List<GameObjectSlot>)this["Abilities"]; } set { this["Abilities"] = value; } }

        public FunctionComponent()
        {
            this.Abilities = new List<GameObjectSlot>();
        }

        public FunctionComponent Initialize(params GameObjectSlot[] abilities)
        {
            this.Abilities = new List<GameObjectSlot>(abilities);
            return this;
        }
        FunctionComponent(params GameObjectSlot[] abilities)
        {
            this.Abilities = new List<GameObjectSlot>(abilities);
        }
        //public FunctionComponent(params Message.Types[] abilities)
        //{
        //    Abilities = new List<Message.Types>(abilities);
        //    this.Primary = Abilities.ElementAtOrDefault(0);
        //    this.Secondary = Abilities.ElementAtOrDefault(1);
        //}

        public override void GetActorTooltip(GameObject parent, GameObject actor, UI.Tooltip tooltip)
        {
            GroupBox box = new GroupBox(tooltip.Controls.Count > 0 ? tooltip.Controls.Last().BottomLeft : Vector2.Zero);
            Dictionary<AbilitySlot, GameObjectSlot> abilities = ControlComponent.GetAbility(Player.Actor);
            foreach (KeyValuePair<AbilitySlot, GameObjectSlot> ability in abilities)
            {
                Slot slot = new Slot(box.Controls.Count > 0 ? box.Controls.Last().BottomLeft : Vector2.Zero);
                slot.Tag = ability.Value;
                slot.SetBottomRightText(Ability.GetSlotText(ability.Key));
            }
            if (box.Controls.Count > 0)
                tooltip.Controls.Add(box);
        }

        static public bool HasAbility(GameObject parent, Message.Types abilityMsg)
        {
            if (parent == null)
                return false;
            FunctionComponent func;
            if(!parent.TryGetComponent<FunctionComponent>("Abilities", out func))
                return false;
            return func.Abilities.FindAll(ab => (Message.Types)ab.Object["Ability"]["Message"] == abilityMsg).Count > 0;
        }

        static public bool HasAbility(GameObject obj, Script.Types abilityID)
        {
            if (obj == null)
                return false;
            FunctionComponent func;
            if (!obj.TryGetComponent<FunctionComponent>("Abilities", out func))
                return false;
            return func.Abilities.FindAll(ab => (Script.Types)ab.Object["Ability"]["ID"] == abilityID).Count > 0;
        }
        public override object Clone()
        {
            return new FunctionComponent(Abilities.ToArray());
        }

        static public GameObjectSlot GetAbility(GameObject obj, int abilityIndex)
        {
            FunctionComponent f;
            if (!obj.TryGetComponent<FunctionComponent>("Abilities", out f))
                return GameObjectSlot.Empty;
            if(abilityIndex >= f.Abilities.Count)
                return GameObjectSlot.Empty;
            return f.Abilities[abilityIndex];
        }
        static public List<GameObjectSlot> GetAbilities(GameObject obj)
        {
            List<GameObjectSlot> list = new List<GameObjectSlot>();
            FunctionComponent f;
            if (!obj.TryGetComponent<FunctionComponent>("Abilities", out f))
                return new List<GameObjectSlot>();
            return f.Abilities;
        }
        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            string text = "Abilities:";
            foreach (GameObjectSlot ab in Abilities)
                text += " " + ab.Object.Name + ",";
            tooltip.Controls.Add(new Label(tooltip.Controls.Count>0 ? tooltip.Controls.Last().BottomLeft : Vector2.Zero, text.TrimEnd(',')));
        }
    }
}
