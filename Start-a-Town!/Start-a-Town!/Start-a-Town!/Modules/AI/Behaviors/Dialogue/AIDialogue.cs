using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.Components;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI.Behaviors
{
    class AIDialogue : Behavior
    {
        public override string Name
        {
            get
            {
                return "AIDIalogue";
            }
        }
        public override object Clone()
        {
            return new AIDialogue();
        }
        public override Behavior Initialize(AIState state)
        {
            this.State = state;
            return this;
        }
        //GameObject Talker;
        //Progress Attention = new Progress();
        //Conversation Conversation;
        //float AttentionDecayDefault = 1;
        //float AttentionDecay = 1;
        AIState State;
        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            // todo: check if converstation is finished here and set it to null?
            if (state.Conversation != null)
            {
                if (state.Conversation.State == Components.Conversation.States.Finished)
                    state.Conversation = null;
                //if(state.Conversation.OutOfRange())
                
            }
            if(state.Talker != null)
                if(Vector3.Distance(parent.Global, state.Talker.Global) > 2)
                {
                    (parent.Net as Net.Server).AIHandler.AIConversationFinish(parent, "I really should get back to work.");
                    state.Talker = null;
                    return BehaviorState.Fail;
                }
            if (state.Attention.Value > 0)
            {
                //state.Attention.Value -= state.AttentionDecay;
                if (state.Attention.Value <= 0)
                {
                    //parent.Net.EventOccured(Components.Message.Types.DialogueEnd, parent);
                    //this.Talker = null;
                    //FinishConversation(parent, "I don't have time for this.");
                    (parent.Net as Net.Server).AIHandler.AIConversationFinish(parent, "I don't have time for this.");
                    return BehaviorState.Fail;
                }
                return BehaviorState.Success;
            }
            return BehaviorState.Fail;
        }

        public override void GetInteractions(GameObject parent, List<Interaction> actions)
        {
            InteractionCustom action = new InteractionCustom(
                "Talk",
                0,
                (a,t)=>this.Talk(parent, a));
            actions.Add(action);
            base.GetInteractions(parent, actions);
        }

        void Talk(GameObject parent, GameObject talker)
        {
            //if (talker == this.Talker)
            //    return;
            var server = parent.Net as Net.Server;
            if (server == null)
                return;

            if (this.State.Conversation != null)
                if (this.State.Conversation.State == Components.Conversation.States.InProgress)
                    return;
           
            //if (server != null)
            //{
                var dir = (talker.Global - parent.Global);
                dir.Normalize();
                server.AIHandler.AIChangeDirection(parent, dir);
                server.AIHandler.AIStopMove(parent);
            //}

            var dialogOptions = parent.GetDialogOptions(talker);
            
            var convo = new Conversation(parent, talker);
            //SpeechComponent.StartConversation(parent, convo);
            //SpeechComponent.StartConversation(talker, convo);
            convo.Start();

            this.State.Conversation = convo;
            this.State.Talker = talker;
            this.State.Attention.Value = this.State.Attention.Max = Engine.TargetFps * 3;// 10;
            //parent.Net.EventOccured(Components.Message.Types.DialogueStart, parent, talker, dialogOptions, convo, this.State.Attention);
            //send conversation start packet
            server.AIHandler.AIConversationStart(parent, talker);
            server.AIHandler.AIDialog(parent, talker, "Hello " + talker.Name, from op in dialogOptions select op.Value);//, this.State.Attention);
        }

        public override void GetRightClickAction(GameObject parent, Dictionary<PlayerInput, Interaction> actions)
        {
            base.GetRightClickAction(parent, actions);
            InteractionCustom action = new InteractionCustom(
                "Talk",
                0,
                (a,t)=>this.Talk(parent, a),
                new TaskConditions(new AllCheck(
                    new RangeCheck()
                    ))
                );

            //action.Conditions =
            //    new TaskConditions(new AllCheck(
            //        new RangeCheck()
            //        ));

            actions.Add(PlayerInput.RButton, action);
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            Message.Types msg = e.Type;
            switch (msg)
            {
                //case Message.Types.DialogueRequest:
                //    this.Talking = true;
                //    RefreshAttention();
                //    this.Partner = e.Sender;
                //    throw new NotImplementedException();

                //    //parent.PostMessage(Message.Types.DialogueStart, e.Sender, "How can I help you, " + e.Sender.Name + "?", parent.GetDialogueOptions(e.Sender));

                //    //  parent.PostMessage(Message.Types.StopWalking);


                //    parent.Transform.Direction = parent.Orientation(e.Sender);

                //    return true;
                //case Message.Types.DialogueEnd:
                //    this.Partner = null;
                //    //     this.Conversation = null;
                //    //  Log.Command(Log.EntryTypes.Chat, parent, "Goodbye!");
                //    this.Talking = false;
                //    Log.Command(Log.EntryTypes.DialogueEnd, parent);
                //    return true;


                case Message.Types.ConversationFinish:
                    this.State.Attention.Value = -1;
                    return true;

                //case Message.Types.DialogueOption:
                //    var sender = e.Parameters[0] as GameObject;
                //    var option = (string)e.Parameters[1];
                //    HandleCommunication(parent, sender, option);
                //    return true;


                default:
                    return true;
            }
        }

        protected override void HandleCommunication(GameObject parent, GameObject sender, string option)
        {
            switch(option)
            {
                case "Bye":
                    //var text = "See you later, " + sender.Name.Split(' ').First() + ".";
                    var text = "See you later, " + sender.Firstname + ".";
                    //FinishConversation(parent, text);
                    (parent.Net as Net.Server).AIHandler.AIConversationFinish(parent, text);
                    //parent.Net.EventOccured(Message.Types.Speech, "See you later, " + sender.Name.Trim().First() + ".");

                    //parent.Net.EventOccured(Message.Types.Chat, parent, "See you later, " + sender.Name.Split(' ').First() + ".");

                    break;

                default:
                    break;
            }
        }

        //private void FinishConversation(GameObject parent, string text)
        //{
        //    this.Conversation.Add(parent, text);
        //    this.Conversation.State = Components.Conversation.States.Finished;
        //    this.Conversation = null;
        //    this.Talker = null;
        //    //this.Attention.Value = 0;
        //}

        public override void GetDialogOptions(GameObject parent, GameObject speaker, List<DialogOption> options)
        {
            base.GetDialogOptions(parent, speaker, options);
            options.Add(new DialogOption("Bye", parent));
        }
        //class InteractionTalk:Interaction
        //{
        //    Action Action;
        //    public override object Clone()
        //    {
        //        return new InteractionTalk(action);
        //    }
        //    public InteractionTalk(Action action)
        //    {
        //        this.Name = "Talk";
        //        this.Action;
        //    }
        //}
    }
}
