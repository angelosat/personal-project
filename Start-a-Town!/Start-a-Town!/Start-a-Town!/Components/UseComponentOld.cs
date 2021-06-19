using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public struct UseArgs
    {
        public GameObject Parent, Actor, Target;
        public Vector3 Face;

        public UseArgs(GameObject parent, GameObject actor, GameObject target, Vector3 face)
        {
            this.Parent = parent;
            this.Actor = actor;
            this.Target = target;
            this.Face = face;
        }
    }

    class UseComponentOld : Component
    {
        public override string ComponentName
        {
            get { return "UseOld"; }
        }
        public Action<UseArgs> Use { get { return (Action<UseArgs>)this["Use"]; } set { this["Use"] = value; } }
        public Func<string> Description { get { return (Func<string>)this["Description"]; } set { this["Description"] = value; } }
        public GameObjectSlot Ability
        {
            get { return (GameObjectSlot)this["Script"]; }
            set
            {
                this["Script"] = value;
                if (value.HasValue)
                    this.AbilityID = (Script.Types)value.Object["Script"]["ID"];
            }
        }
        public Script.Types AbilityID { get { return (Script.Types)this["AbilityID"]; } set { this["AbilityID"] = value; } }
        public List<Script.Types> Abilities { get { return (List<Script.Types>)this["Abilities"]; } set { this["Abilities"] = value; } }

        public List<Script> InstantiatedScripts { get { return (List<Script>)this["Scripts"]; } set { this["Scripts"] = value; } }

        public UseComponentOld()
        {
            //  this.Use = (a) => { };
            this.Description = () => { return ""; };
            this.Ability = GameObjectSlot.Empty;
            this.Abilities = new List<Script.Types>();
            this.InstantiatedScripts = new List<Script>();
        }

        public UseComponentOld Initialize(params Script.Types[] abilities)
        {
            this.Abilities.AddRange(abilities);
            this.InstantiatedScripts.AddRange(abilities.ToList().Select(a => Components.Ability.GetScript(a)));
            return this;
        }
        public UseComponentOld Initialize(params Script[] scripts)
        {
            this.InstantiatedScripts.AddRange(scripts);
            return this;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Use:
                    if (this.Ability.HasValue)
                    {
                        //AbilityComponent.Perform(this.Ability.Object, new ActionArgs(e.Sender, e.Parameters[0] as GameObject, (Vector3)e.Parameters[1]));

                        throw new NotImplementedException();
                        //AbilityComponent.Perform(e.Network, this.Ability.Object, new ActionArgs(e.Parameters[0] as GameObject, e.Parameters[1] as GameObject, (Vector3)e.Parameters[2]));
                        return true;
                    }
                    //Use(
                    //    new UseArgs(
                    //        parent,
                    //        e.Sender,
                    //        //e.Parameters[0] as GameObject,
                    //        e.Parameters[0] as GameObject,
                    //        (Vector3)e.Parameters[1]));
                    return true;
                default:
                    return base.HandleMessage(parent, e);
            }
        }

        public override object Clone()
        {
            return new UseComponentOld()
            {
                //    Use = this.Use,
                Description = this.Description,
                Ability = this.Ability,
                Abilities = this.Abilities, // TODO WARNING!!! maybe create a new list? (mutable)
                InstantiatedScripts = this.InstantiatedScripts
            };
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            if (Ability.HasValue)
            {
                tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, "Use:"));
                // tooltip.Controls.Add(new SlotWithText(tooltip.Controls.BottomLeft) { Tag = this.Ability });
                Panel panel = new Panel(tooltip.Controls.BottomLeft) { AutoSize = true };
                this.Ability.Object.GetTooltip(panel);
                tooltip.Controls.Add(panel);
                return;
            }

            tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, "Use: " + this.Description())
            {
                TextColorFunc = () => { return Color.Lime; }
            });

            //fill: Color.Lime,
            //outline: Color.Black));
        }

        static public Script.Types GetAbilityID(GameObject obj)
        {
            return (Script.Types)obj["Use"]["AbilityID"];
        }

        static public bool HasAbility(GameObject obj, Script.Types abilityID)
        {
            if (obj == null)
                return false;
            UseComponentOld use;
            if (!obj.TryGetComponent<UseComponentOld>(out use))
                return false;
            //return use.Abilities.FindAll(ab => (Script.Types)ab.Object["Ability"]["ID"] == abilityID).Count > 0;
            return use.Abilities.Contains(abilityID);
        }
        static public bool HasScript(GameObject obj, Script.Types scriptID)
        {
            if (obj == null)
                return false;
            UseComponentOld use;
            if (!obj.TryGetComponent<UseComponentOld>(out use))
                return false;
            //return use.Abilities.Contains(scriptID);
            return use.InstantiatedScripts.Exists(scr => scr.ID == scriptID);
        }

        //internal override void GetAvailableActions(List<Script> list)
        //{
        //    list.AddRange(this.InstantiatedScripts);
        //}

        //static public Script HasScript(GameObject obj, Script.Types scriptID)
        //{
        //    if (obj == null)
        //        return null;
        //    UseComponent use;
        //    if (!obj.TryGetComponent<UseComponent>(out use))
        //        return null;
        //    //return use.Abilities.Contains(scriptID);
        //    return use.InstantiatedScripts.FirstOrDefault(scr => scr.ID == scriptID);
        //}
    }
}
