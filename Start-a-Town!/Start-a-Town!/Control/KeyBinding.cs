using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public enum PlayerActions { LB, Interact, PickUp, Activate, Drop, Throw, Seathe }//, ThrowAll }
    public class PlayerInput
    {
        static public Dictionary<PlayerActions, Keys> KeyBindings = new Dictionary<PlayerActions, Keys>(){
            {PlayerActions.PickUp, GlobalVars.KeyBindings.PickUp},
            {PlayerActions.Activate, GlobalVars.KeyBindings.Activate},
            {PlayerActions.Drop, GlobalVars.KeyBindings.Drop},
            {PlayerActions.Throw, GlobalVars.KeyBindings.Throw},
            //{PlayerActions.ThrowAll, GlobalVars.KeyBindings.Throw},
            {PlayerActions.LB, System.Windows.Forms.Keys.LButton},
            {PlayerActions.Interact, System.Windows.Forms.Keys.RButton},
            {PlayerActions.Seathe, System.Windows.Forms.Keys.X}
        };
        
        static public Keys GetKey(PlayerActions actions)
        {
            return KeyBindings[actions];
        }
        public Keys GetKey()
        {
            return KeyBindings[this.Action];
        }

        static public readonly PlayerInput Drop = new PlayerInput(PlayerActions.Drop);
        static public readonly PlayerInput DropHold = new PlayerInput(PlayerActions.Drop, true);
        static public readonly PlayerInput Throw = new PlayerInput(PlayerActions.Throw);
        static public readonly PlayerInput ThrowHold = new PlayerInput(PlayerActions.Throw, true);
        static public readonly PlayerInput LButton = new PlayerInput(PlayerActions.LB);
        static public readonly PlayerInput RButton = new PlayerInput(PlayerActions.Interact);
        static public readonly PlayerInput RButtonHold = new PlayerInput(PlayerActions.Interact, true);
        static public readonly PlayerInput Activate = new PlayerInput(PlayerActions.Activate);
        static public readonly PlayerInput ActivateHold = new PlayerInput(PlayerActions.Activate, true);
        static public readonly PlayerInput PickUp = new PlayerInput(PlayerActions.PickUp);
        static public readonly PlayerInput PickUpHold = new PlayerInput(PlayerActions.PickUp, true);
        static public readonly PlayerInput Seathe = new PlayerInput(PlayerActions.Seathe);
        static public readonly PlayerInput SeatheHold = new PlayerInput(PlayerActions.Seathe, true);

        public static Dictionary<PlayerInput, Func<GameObject, TargetArgs, Interaction>> DefaultInputs = new Dictionary<PlayerInput, Func<GameObject, TargetArgs, Interaction>>()
        {
            {PlayerInput.Drop, (a,t)=>new DropCarried()},
            {DropHold, (a,t)=>new DropCarried(true)},
            {Throw, (a,t)=>new InteractionThrow()},
            {ThrowHold, (a,t)=>new InteractionThrow(true)},
            {RButton, GetRB},
            {Activate, GetActivate},
            {ActivateHold, GetActivateHold},
            {PickUp, (a,t)=>GetAction(a, t, new PlayerInput(PlayerActions.PickUp))},
            {PickUpHold, (a,t)=>GetAction(a, t, new PlayerInput(PlayerActions.PickUp, true))},
            {Seathe, GetSeathe}
        };

        
        //public static Dictionary<PlayerInput, Func<GameObject, TargetArgs, PlayerInput, Interaction>> DefaultInputs = new Dictionary<PlayerInput,Func<GameObject,TargetArgs,PlayerInput,Interaction>>()
        //{
        //    {new PlayerInput(PlayerActions.Drop), (a,t,i)=>new DropCarried()},
        //    {new PlayerInput(PlayerActions.Drop, true), (a,t,i)=>new DropCarried(true)},
        //    {new PlayerInput(PlayerActions.Throw), (a,t,i)=>new Throw()},
        //    {new PlayerInput(PlayerActions.Throw, true), (a,t,i)=>new Throw(true)},
        //    {new PlayerInput(PlayerActions.RB), GetRB},
        //    {new PlayerInput(PlayerActions.Activate), GetActivate}
        //};
        static public Interaction GetDefaultInput(GameObject actor, TargetArgs target, PlayerInput input)
        {
            var action = DefaultInputs.FirstOrDefault(foo => foo.Key.Action == input.Action && foo.Key.Hold == input.Hold);
            if (action.Value == null)
                return null;
            return action.Value(actor, target) as Interaction;
        }
        static Interaction GetRB(GameObject parent, TargetArgs a)
        {
            // first try get interaction from hauled item, (then from mainhand item?), then from target
            var hauled = PersonalInventoryComponent.GetHauling(parent);
            if (hauled.Object != null)
            {
                var hauledAction = hauled.Object.GetHauledActions(a).FirstOrDefault();
                if (hauledAction != null)
                    return hauledAction;
            }
            //var frommainhand = GetInteractionFromMainHand(parent, a);
            //if (frommainhand != null)
            //    return frommainhand;
            return GetInteractionFromTarget(parent, a);
        }
        static Interaction GetRBold(GameObject parent, TargetArgs a)
        {
            //return GetInteractionFromEquippedItem(parent, a);

            /* trying to just get interaction from target and have the tool completely passive TEMP */
            //return GetInteractionFromEquippedItem(parent, a) ?? GetInteractionFromTarget(parent, a);
            return GetInteractionFromTarget(parent, a);

            //var action = GetInteractionFromEquippedItem(parent, a);
            //if (action == null)
            //    action = GetInteractionFromTarget(parent, a);
            //return action;



            //return a.GetContextActionWorld(net, new PlayerInput(PlayerActions.RB));
            //return GetInteractionFromEquippedSkill(parent, a);
        }

        private static Interaction GetInteractionFromMainHand(GameObject parent, TargetArgs a)
        {
            var tool = GearComponent.GetSlot(parent, GearType.Mainhand).Object;//.EquipmentSlots[GearType.Mainhand].Object;
            if (tool == null)
                return null;
            ToolAbilityDef skill = null;
            if (!tool.TryGetComponent<Components.ToolAbilityComponent>(c => skill = c.Skill))
                return null;
            if (skill.IsNull())
                return null;
            return skill.GetInteraction(parent, a);
        }
        private static Interaction GetInteractionFromEquippedSkill(GameObject parent, TargetArgs a)
        {
            //var tool = parent.GetComponent<Components.GearComponent>().Holding.Object;//.EquipmentSlots[GearType.Mainhand].Object;
            var tool = parent.GetComponent<Components.HaulComponent>().Holding.Object;//.EquipmentSlots[GearType.Mainhand].Object;

            if (tool.IsNull())
                return null;
            ToolAbilityDef skill = null;
            if (!tool.TryGetComponent<Components.ToolAbilityComponent>(c => skill = c.Skill))
                return null;
            if (skill.IsNull())
                return null;
            return skill.GetInteraction(parent, a);
        }
        private static Interaction GetInteractionFromEquippedItem(GameObject parent, TargetArgs a)
        {
            //var tool = parent.GetComponent<Components.GearComponent>().Holding.Object;
            //var tool = parent.GetComponent<Components.HaulComponent>().Holding.Object;
            var tool = GearComponent.GetSlot(parent, GearType.Mainhand).Object;

            if (tool == null)
                return null;
            //var actions = tool.GetEquippedActions();
            var actions = tool.GetEquippedActionsWithTarget(parent, a);
            var action = actions.FirstOrDefault();
            return action;
        }
        private static Interaction GetInteractionFromHauledItem(GameObject parent, TargetArgs a)
        {
            //var tool = parent.GetComponent<Components.GearComponent>().Holding.Object;
            var tool = parent.GetComponent<Components.HaulComponent>().Holding.Object;

            if (tool == null)
                return null;
            //var actions = tool.GetEquippedActions();
            var actions = tool.GetEquippedActionsWithTarget(parent, a);
            var action = actions.FirstOrDefault();
            return action;
        }
        private static Interaction GetInteractionFromTarget(GameObject parent, TargetArgs a)
        {
            var action = a.GetContextActionWorld(parent, PlayerInput.RButton);
            return action;
        }
        static Interaction GetActivate(GameObject a, TargetArgs t)
        {
            return t.GetContextActionWorld(a, PlayerInput.Activate);//new PlayerInput(PlayerActions.Activate));
        }
        static Interaction GetActivateHold(GameObject a, TargetArgs t)
        {
            // put currently hauled item in backpack
            var hauled = a.GetComponent<HaulComponent>().GetSlot();//.Slot;
            if (hauled.Object != null)
                return new InteractionStoreHauled();


            // if no hauling, get interaction from current target
            return t.GetContextActionWorld(a, PlayerInput.ActivateHold);// new PlayerInput(PlayerActions.Activate, true));
        }
        static Interaction GetAction(GameObject a, TargetArgs t, PlayerInput input)
        {
            return t.GetContextActionWorld(a, input);
        }
        static Interaction GetSeathe(GameObject a, TargetArgs t)
        {
            var inv = a.GetComponent<PersonalInventoryComponent>();
            var hauled = inv.GetHauling();//.Slot;
            if (hauled.Object != null)
                //if (inv.Slots.Capacity > 0)
                    return new InteractionStoreHauled();
            return null;
        }
        // TODO: remove this!
        //static Dictionary<PlayerActions, Interaction> DefaultActions = new Dictionary<PlayerActions, Interaction>(){
        //    {PlayerActions.Drop, new DropCarried()},
        //    {PlayerActions.Throw, new Throw()},
        //    //{PlayerActions.ThrowAll, new Throw(true)},
        //    {PlayerActions.RB, new UseItem()}
        //};
        //static public Interaction GetDefaultAction(PlayerActions action)
        //{
        //    if (!DefaultActions.ContainsKey(action))
        //        return null;
        //    return DefaultActions[action].Clone() as Interaction;
        //}

        public bool Hold;
        public PlayerActions Action;
        PlayerInput(PlayerActions action, bool hold = false)
        {
            this.Action = action;
            this.Hold = hold;
        }
        public void Write(BinaryWriter w)
        {
            w.Write((int)this.Action);
            w.Write(this.Hold);
        }
        public PlayerInput(BinaryReader r)
        {
            this.Action = (PlayerActions)r.ReadInt32();
            this.Hold = r.ReadBoolean();
        }
        public override string ToString()
        {
            //return this.Action.ToString() + (this.Hold ? " (hold)" : "");
            return this.GetKey().ToString() + (this.Hold ? " (hold)" : "");
        }

    }

    //public class KeyBinding
    //{
    //    public Keys Key;
    //    //public PlayerActions Action;
    //    //public KeyBinding(Keys Key, PlayerActions action)
    //    //{
    //    //    this.Key = Key;
    //    //    this.Action = action;
    //    //}
    //    public bool Hold;
    //    //public Action Action;
    //    public PlayerActions Action;

    //    public KeyBinding(Keys key, bool hold = false)
    //    {
    //        this.Key = key;
    //        this.Hold = hold;
    //    }
    //    //public KeyBinding(Keys key, bool hold, Action action)
    //    //    : this(key, hold)
    //    //{
    //    //    this.Action = action;
    //    //}
    //    public KeyBinding(Keys key, bool hold, PlayerActions action)
    //        : this(key, hold)
    //    {
    //        this.Action = action;
    //    }
    //    public override string ToString()
    //    {
    //        return this.Key.ToString() + (this.Hold ? " (hold)" : "");
    //    }

    //}
}
