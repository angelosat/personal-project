using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.AI
{
    class AICommunication : Behavior
    {
        bool Talking { get { return (bool)this["Talking"]; } set { this["Talking"] = value; } }
        //float AttentionSpan { get { return (float)this["AttentionSpan"]; } set { this["AttentionSpan"] = value; } }
        TimeSpan AttentionSpan { get { return (TimeSpan)this["AttentionSpan"]; } set { this["AttentionSpan"] = value; } }
        GameObject Partner { get { return (GameObject)this["Partner"]; } set { this["Partner"] = value; } }
       

        public override string Name
        {
            get
            {
                string text = "Talking with: " + (this.Partner != null ? Partner.Name : "<null>") + " " + this.AttentionSpan.TotalSeconds.ToString("#0.00s");
               // string text = "Conversation: " + (this.Conversation != null ? Conversation.ToString() : "<null>") + " " + this.AttentionSpan.TotalSeconds.ToString("#0.00s");
                return text;
            }
        }

        public AICommunication()
        {
            this.Talking = false;
            RefreshAttention();
        }

        public override BehaviorState Execute(GameObject parent, AIState state)//Net.IObjectProvider net, GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            //GameObject parent = state.Parent;
            //Net.IObjectProvider net = state.Net;
            Personality personality = state.Personality;
            Knowledge knowledge = state.Knowledge;
            if (!Talking)
                return BehaviorState.Success;
            if (Vector3.Distance(this.Partner.Global, parent.Global) > Math.Sqrt(3))
            {
                throw new NotImplementedException();
                //GameObject.PostMessage(parent, Message.Types.DialogueEnd, parent, "Goodbye!");
                
                return BehaviorState.Success;
            }
            //this.AttentionSpan = this.AttentionSpan.Subtract(new TimeSpan(0, 0, 0, 0, (int)(GlobalVars.DeltaTime * 1000 / Engine.TargetFps)));
            this.AttentionSpan = this.AttentionSpan.Subtract(new TimeSpan(0, 0, 0, 0, (int)(1000 / Engine.TargetFps)));
            if (this.AttentionSpan.Milliseconds < 0)
            {
                //GameObject.PostMessage(parent, Message.Types.DialogueEnd, parent, "I don't have all day.");
                throw new NotImplementedException();
                return BehaviorState.Success;
            }
            return BehaviorState.Running;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            Message.Types msg = e.Type;
            switch (msg)
            {
                case Message.Types.DialogueRequest:
                    this.Talking = true;
                    RefreshAttention();
                    this.Partner = e.Sender;
                    throw new NotImplementedException();

                    //parent.PostMessage(Message.Types.DialogueStart, e.Sender, "How can I help you, " + e.Sender.Name + "?", parent.GetDialogueOptions(e.Sender));

                  //  parent.PostMessage(Message.Types.StopWalking);


                    parent.Transform.Direction = parent.Orientation(e.Sender);

                    return true;
                case Message.Types.DialogueEnd:
                    this.Partner = null;
               //     this.Conversation = null;
                  //  Log.Command(Log.EntryTypes.Chat, parent, "Goodbye!");
                    this.Talking = false;
                    Log.Command(Log.EntryTypes.DialogueEnd, parent);
                    return true;
                case Message.Types.DialogueOption:
            //        Conversation dialogue = e.Parameters[0] as Conversation;
                    DialogueOption option = (DialogueOption)e.Parameters[0];//1];
                    DialogueOptionCollection options;// = new DialogueOptionCollection();
                    string text;
                   // if (HandleConversation(parent, e.Sender, option, out text, out options) == Conversation.States.Finished)
                    if(option.Handler(parent, e.Sender, option.Text, out text, out options) == Conversation.States.Finished)
                        throw new NotImplementedException();
                        //GameObject.PostMessage(parent, Message.Types.DialogueEnd, e.Sender, text);
                    else
                    {
                        RefreshAttention();
                        throw new NotImplementedException();
                        //GameObject.PostMessage(parent, Message.Types.Dialogue, e.Sender, text, options.ToArray());
                    }
                    return true;


                default:
                    return true;
            }
        }
        public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        {
            //List<Interaction> list = e.Parameters[0] as List<Interaction>;
            list.Add(new InteractionOld(timespan: TimeSpan.Zero, message: Message.Types.DialogueRequest, source: parent, name: "Talk"));
        }

        void RefreshAttention()
        {
            this.AttentionSpan = new TimeSpan(0, 0, 20);
        }

        public override Conversation.States HandleConversation(GameObject parent, GameObject speaker, string option, out string speech, out DialogueOptionCollection options)
        {
            speech = "";
            switch (option)
            {
                case "Needs":
                   // speech = parent["Needs"].ToString();
                    Log.Command(Log.EntryTypes.Needs, parent);
                    break;
                case "Goodbye":
                    speech = "Goodbye!";
                    options = new DialogueOptionCollection();
                    return Conversation.States.Finished;
                case "Abilities":
                    speech = ControlComponent.GetAbilities(parent).ToString();
                    break;
                case "Inventory":
                    speech = "Take a look.";
                    //GameObject.PostMessage(speaker, Message.Types.ManageEquipmentOk, parent);
                    Log.Command(Log.EntryTypes.Inventory, parent);
                    break;
                default:
                    speech = "I don't know about " + option + ".";
                    break;
            }
            speech += "Anything else I can help you with?";
           // options = new List<string>() { "Needs", "Abilities", "Inventory", "Goodbye" };
            //options.AddRange(new DialogueOptionCollection() { 
            //    "Needs".ToDialogueOption(), 
            //    "Abilities".ToDialogueOption(), 
            //    "Inventory".ToDialogueOption(), 
            //    "Goodbye".ToDialogueOption() 
            //});
            options = parent.GetDialogueOptions(speaker);
            return Conversation.States.InProgress;// options.Count > 0;
        }

        void GetDialogueUI(UI.Control bubble)
        {
        }

        public override void GetDialogueOptions(GameObject parent, GameObject speaker, DialogueOptionCollection options)
        {
            options.AddRange(new DialogueOption[]{
                "Needs".ToDialogueOption(HandleConversation), 
                "Abilities".ToDialogueOption(HandleConversation), 
                "Inventory".ToDialogueOption(HandleConversation), 
                "Goodbye".ToDialogueOption(HandleConversation)
            });
        }

        public override object Clone()
        {
            return new AICommunication();
        }
    }
}
