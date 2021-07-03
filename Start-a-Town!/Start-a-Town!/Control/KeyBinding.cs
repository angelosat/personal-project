using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public enum PlayerActions { LB, Interact, PickUp, Activate, Drop, Throw, Seathe }
    [Obsolete]
    public class PlayerInput
    {
        static public Dictionary<PlayerActions, Keys> KeyBindings = new Dictionary<PlayerActions, Keys>(){
            {PlayerActions.PickUp, GlobalVars.KeyBindings.PickUp},
            {PlayerActions.Activate, GlobalVars.KeyBindings.Activate},
            {PlayerActions.Drop, GlobalVars.KeyBindings.Drop},
            {PlayerActions.Throw, GlobalVars.KeyBindings.Throw},
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
            return GetInteractionFromTarget(parent, a);
        }

        private static Interaction GetInteractionFromTarget(GameObject parent, TargetArgs a)
        {
            var action = a.GetContextActionWorld(parent, PlayerInput.RButton);
            return action;
        }
        static Interaction GetActivate(GameObject a, TargetArgs t)
        {
            return t.GetContextActionWorld(a, PlayerInput.Activate);
        }
        static Interaction GetActivateHold(GameObject a, TargetArgs t)
        {
            // put currently hauled item in backpack
            var hauled = a.GetComponent<HaulComponent>().GetSlot();
            if (hauled.Object != null)
                return new InteractionStoreHauled();

            // if no hauling, get interaction from current target
            return t.GetContextActionWorld(a, PlayerInput.ActivateHold);
        }
        static Interaction GetAction(GameObject a, TargetArgs t, PlayerInput input)
        {
            return t.GetContextActionWorld(a, input);
        }
        static Interaction GetSeathe(GameObject a, TargetArgs t)
        {
            var inv = a.GetComponent<PersonalInventoryComponent>();
            var hauled = inv.GetHauling();
            if (hauled.Object != null)
                    return new InteractionStoreHauled();
            return null;
        }
        
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
            return this.GetKey().ToString() + (this.Hold ? " (hold)" : "");
        }
    }
}
