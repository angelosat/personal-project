using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Start_a_Town_
{
    public enum PlayerActions { LB, Interact, PickUp, Activate, Drop, Throw, Seathe }
    public class PlayerInput
    {
        public static Dictionary<PlayerActions, Keys> KeyBindings = new()
        {
            { PlayerActions.PickUp, GlobalVars.KeyBindings.PickUp },
            { PlayerActions.Activate, GlobalVars.KeyBindings.Activate },
            { PlayerActions.Drop, GlobalVars.KeyBindings.Drop },
            { PlayerActions.Throw, GlobalVars.KeyBindings.Throw },
            { PlayerActions.LB, Keys.LButton },
            { PlayerActions.Interact, Keys.RButton },
            { PlayerActions.Seathe, Keys.X }
        };

        public static Keys GetKey(PlayerActions actions)
        {
            return KeyBindings[actions];
        }
        public Keys GetKey()
        {
            return KeyBindings[this.Action];
        }

        public static readonly PlayerInput Drop = new(PlayerActions.Drop);
        public static readonly PlayerInput DropHold = new(PlayerActions.Drop, true);
        public static readonly PlayerInput Throw = new(PlayerActions.Throw);
        public static readonly PlayerInput ThrowHold = new(PlayerActions.Throw, true);
        public static readonly PlayerInput LButton = new(PlayerActions.LB);
        public static readonly PlayerInput RButton = new(PlayerActions.Interact);
        public static readonly PlayerInput RButtonHold = new(PlayerActions.Interact, true);
        public static readonly PlayerInput Activate = new(PlayerActions.Activate);
        public static readonly PlayerInput ActivateHold = new(PlayerActions.Activate, true);
        public static readonly PlayerInput PickUp = new(PlayerActions.PickUp);
        public static readonly PlayerInput PickUpHold = new(PlayerActions.PickUp, true);
        public static readonly PlayerInput Seathe = new(PlayerActions.Seathe);
        public static readonly PlayerInput SeatheHold = new(PlayerActions.Seathe, true);

        public static Dictionary<PlayerInput, Func<GameObject, TargetArgs, Interaction>> DefaultInputs = new()
        {
            {Drop, (a,t)=>new DropCarried()},
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

        public static Interaction GetDefaultInput(GameObject actor, TargetArgs target, PlayerInput input)
        {
            var action = DefaultInputs.FirstOrDefault(foo => foo.Key.Action == input.Action && foo.Key.Hold == input.Hold);
            if (action.Value == null)
                return null;
            return action.Value(actor, target);
        }
        static Interaction GetRB(GameObject parent, TargetArgs a)
        {
            // first try get interaction from hauled item, (then from mainhand item?), then from target
            var hauled = parent.Inventory.HaulSlot;// PersonalInventoryComponent.GetHauling(parent);
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
