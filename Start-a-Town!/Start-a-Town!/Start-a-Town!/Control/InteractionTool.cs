using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.PlayerControl
{
    class InteractionTool : ControlTool
    {
       // float t, Length;
        InteractionOld Interaction;
        
        GameObject Actor;
     //   GameObject Tool;
     //   Components.Message.Types Message;
        System.Windows.Forms.Keys Key;
        public InteractionTool(GameObject target, System.Windows.Forms.Keys key, InteractionOld interaction)
        {
            //this.TargetOld = target;
            //Face = Controller.Instance.Mouseover.Face;
            Actor = Player.Actor;
            this.Interaction = interaction;
            this.Key = key;
        }
        public InteractionTool(System.Windows.Forms.Keys key)
        {
            Actor = Player.Actor;
            this.Key = key; 
        }
        //public InteractionTool(GameObject target, AbilitySlot abilityslot, System.Windows.Forms.Keys key, params object[] parameters) //: this(target, (int)abilityslot) 
        //{
        //    this.Key = key;
        //    this.Target = target;
        //    Message.Types message = (Components.Message.Types)ControlComponent.GetAbility(Player.Actor)[abilityslot].Object["Ability"]["Message"];

        //    List<object> p = new List<object>() {Target, message, Controller.Instance.Mouseover.Face };
        //    p.AddRange(parameters);
        //    Player.Actor.HandleMessage(Components.Message.Types.Begin, null, p.ToArray());//, new GameObjectEventArgs(Action, Player.Actor, Controller.Instance.Mouseover.Face));

        //    //Player.Actor.HandleMessage(Components.Message.Types.Begin, null, Target, message, Controller.Instance.Mouseover.Face);//, new GameObjectEventArgs(Action, Player.Actor, Controller.Instance.Mouseover.Face));
        //}

        static public void RepeatInteraction(GameObject actor, GameObject target) 
        {
            Message.Types msg = Message.Types.BeginInteraction;
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu))
            {
                PartyComponent party;
                if (Player.Actor.TryGetComponent<PartyComponent>("Party", out party))
                {
                    GameObjectSlot memberSlot = party.Members.FirstOrDefault();
                    if (memberSlot != null)
                        if (memberSlot.HasValue)
                        {
                            actor = memberSlot.Object;
                            msg = Message.Types.Order;
                        }
                }
            }

            List<InteractionOld> interactions = new List<InteractionOld>();
            //target.HandleMessage(Message.Types.Query, actor, interactions);
            target.Query(actor, interactions);
            if (!Player.LastAbilityUsed.HasValue)
                return;
            Message.Types interactionMsg = (Message.Types)Player.LastAbilityUsed.Object["Ability"]["Message"];
            InteractionOld interaction = interactions.FirstOrDefault(i => i.Message == interactionMsg);
            if (interaction == null)
            {
                Console.WriteLine("NO INTERACTION FOUND (" + interactionMsg + ")");
                return;
            }
            throw new NotImplementedException();
            //GameObject.PostMessage(actor, msg, null, interaction, Controller.Instance.Mouseover.Face, actor["Inventory"]["Holding"] as GameObjectSlot); 
        }

        public InteractionTool(GameObject target, AbilitySlot abilityslot, System.Windows.Forms.Keys key) //: this(target, (int)abilityslot) 
        {
            Actor = Player.Actor;
            Message.Types msg = Message.Types.BeginInteraction;
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu))
            {
                PartyComponent party;
                if (Player.Actor.TryGetComponent<PartyComponent>("Party", out party))
                {
                    GameObjectSlot memberSlot = party.Members.FirstOrDefault();
                    if (memberSlot != null)
                        if (memberSlot.HasValue)
                        {
                            Actor = memberSlot.Object;
                            msg = Message.Types.Order;
                        }
                }
            }

            

            this.Key = key;
            //this.TargetOld = target;


            // TODO: this throws error if ability at specific slot is null
        //    Message.Types message = (Components.Message.Types)ControlComponent.GetAbility(Actor)[abilityslot].Object["Ability"]["Message"];
            Message.Types message;
            if (!ControlComponent.TryGetAbility(Actor, abilityslot, out message))
                return;

           // Interaction inter;
            //List<Interaction> interactions = target.GetActions(Actor, target, null, Controller.Instance.Mouseover.Face, DragDropManager.Instance.Item as GameObjectSlot);  
            List<InteractionOld> interactions = new List<InteractionOld>();
            target.Query(Actor, interactions);
            Interaction = interactions.FirstOrDefault(i => i.Message == message);
            if (Interaction == null)
            {
                Console.WriteLine("NO INTERACTION FOUND (" + message + ")");
                return;
            }
            //Face = Controller.Instance.Mouseover.Face;
            throw new NotImplementedException();
            //Actor.PostMessage(msg, null, Interaction, Controller.Instance.Mouseover.Face, Actor["Inventory"]["Holding"] as GameObjectSlot);//, new GameObjectEventArgs(Action, Player.Actor, Controller.Instance.Mouseover.Face));


        }
        public InteractionTool(GameObject target, Message.Types message, System.Windows.Forms.Keys key) //: this(target, (int)abilityslot) 
        {
            Actor = Player.Actor;
            Message.Types msg = Message.Types.BeginInteraction;
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu))
            {
                PartyComponent party;
                if (Player.Actor.TryGetComponent<PartyComponent>("Party", out party))
                {
                    GameObjectSlot memberSlot = party.Members.FirstOrDefault();
                    if (memberSlot != null)
                        if (memberSlot.HasValue)
                        {
                            Actor = memberSlot.Object;
                            msg = Message.Types.Order;
                        }
                }
            }

            this.Key = key;
            //this.TargetOld = target;

            InteractionOld inter;
            //List<Interaction> interactions = target.GetActions(Actor, target, null, Controller.Instance.Mouseover.Face, DragDropManager.Instance.Item as GameObjectSlot);  
            List<InteractionOld> interactions = new List<InteractionOld>();
            target.Query(Actor, interactions);
            inter = interactions.FirstOrDefault(i => i.Message == message);
            if (inter == null)
            {
                Console.WriteLine("NO INTERACTION FOUND (" + message + ")");
                return;
            }
            throw new NotImplementedException();
            //Actor.PostMessage(msg, null, inter, Controller.Instance.Mouseover.Face);//, new GameObjectEventArgs(Action, Player.Actor, Controller.Instance.Mouseover.Face));


        }


        //public InteractionTool(GameObject target, int abilityIndex)//, float length = 100)
        //{
        //    this.Target = target;

        //    List<GameObjectSlot> ToolAbilities = (List<GameObjectSlot>)Player.Actor["Abilities"]["Abilities"];
        //    if (ToolAbilities.Count > 1)
        //    {
        //        Message = (Components.Message.Types)ToolAbilities[abilityIndex].Object["Ability"]["Message"];
        //        Player.Actor.HandleMessage(Components.Message.Types.Begin, null, Target, Message, Controller.Instance.Mouseover.Face);//, new GameObjectEventArgs(Action, Player.Actor, Controller.Instance.Mouseover.Face));
        //    }

        //}
        public override void HandleInput(InputState e)
        {
            if (Interaction == null)
            {
                ToolManager.Instance.ActiveTool = null;
                return;
            }

            if (InputState.IsKeyDown(Key))
            {   
                if (Actor["Control"]["Task"] == null)
                    ToolManager.Instance.ActiveTool = null;
                
                //if (Interaction.Range >= 0)
                //{
                    //Vector3 difference = (Interaction.Source.Global - Actor.Global);
                    //float length = difference.Length();
                   // if (length > Interaction.Range)
                if (!Interaction.Range(Player.Actor, Interaction.Source))//length))
                    {
                        Vector3 difference = (Interaction.Source.Global - Actor.Global);
                        float length = difference.Length();
                        difference.Normalize();
                        difference.Z = 0;
                        throw new NotImplementedException();
                        //Actor.PostMessage(Message.Types.MoveToObject, Actor, Interaction.Source, Interaction.Range, 1f);
                        return;
                    }
                //}

                Actor.PostMessage(Components.Message.Types.Perform);

            }
            else
            {
                Actor.PostMessage(Message.Types.Interrupt);
                ToolManager.Instance.ActiveTool = null;
            }
        }

        //public override ControlTool.Messages OnMouseLeft(bool held)
        //{
        //    Actor.HandleMessage(Message.Types.Interrupt);

        //    return ControlTool.Messages.Remove;
        //}

        //public override ControlTool.Messages MouseRight(bool held)
        //{
        //    //t -= GlobalVars.DeltaTime;
        //    //if (t <= 0)
        //    //{
        //    //    // if (Target.HandleMessage(Player.Actor, Action))//;//Components.Message.Types.Activate);
        //    //    //if (Player.Actor.GetComponent<ControlComponent>("Control").Perform(Target, new GameObjectEventArgs(Action, Player.Actor)) == Message.Types.True)
        //    //    if(Player.Actor.HandleMessage(Message.Types.Perform, null, Target, new GameObjectEventArgs(Action, Player.Actor))) //new GameObjectEventArgs(Action, Player.Actor, Target)))
        //    //        return Messages.Remove;

        //    //    t = Length;
        //    //}
        //    //return Messages.Default;
        //    //if (Player.Actor.HandleMessage(Message.Types.Perform, null))
        //    ////if (Player.Actor["Control"].HandleMessage(Message.Types.Perform))
        //    //    return Messages.Remove;
        //    Player.Actor.HandleMessage(Components.Message.Types.Perform, null);
        //    if (Player.Actor["Control"]["Task"] == null)
        //        return Messages.Remove;
                
        //    return Messages.Default;
        //}

        //public override ControlTool.Messages MouseRightUp()
        //{
        //    return Messages.Remove;
        //}

        internal override void DrawWorld(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, IMap map, Camera camera)
        {
            if (Player.Actor["Control"]["Task"] != null)
                Player.Actor.DrawInterface(sb, camera);
            //Rectangle bounds = camera.GetScreenBounds(Player.Actor.Global, Player.Actor["Sprite"].GetProperty<Sprite>("Sprite").GetBounds());
            //Bar.Draw(sb, new Vector2(bounds.X + bounds.Width / 2f, bounds.Y) - new Vector2(Bar.DefaultWidth / 2, Bar.DefaultHeight / 2), Bar.DefaultWidth, 1 - t / Length);
        }
    }
}
