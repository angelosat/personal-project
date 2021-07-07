using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public delegate Conversation.States DialogueOptionHandler(GameObject parent, GameObject speaker, string option, out string speech, out DialogueOptionCollection options);
    public class DialogueOptionCollection : List<DialogueOption> { }
    public struct DialogueOption
    {
        public string Text;
        public Func<bool> Condition;
        public DialogueOptionHandler Handler;
        public override string ToString()
        {
            return Text;
        }
        public DialogueOption(string text, DialogueOptionHandler handler, Func<bool> condition)
        {
            this.Text = text;
            this.Handler = handler;
            this.Condition = condition;
        }
        public DialogueOption(string text, DialogueOptionHandler handler)
        {
            this.Text = text;
            this.Handler = handler;
            this.Condition = new Func<bool>(() => true);
        }
    }
    class DialogueEntry
    {
        GameObject Speaker;
        DateTime Timestamp;
        string Speech;
        public DialogueEntry(DateTime time, GameObject speaker, string speech)
        {
            this.Speaker = speaker;
            this.Timestamp = time;
            this.Speech = speech;
        }
    }
    public class Conversation
    {
        public enum States { InProgress, Finished }

        public event EventHandler<EventArgs> OptionSelected;
        void OnOptionSelected(object option)
        {
            if (OptionSelected != null)
                OptionSelected(option, EventArgs.Empty);
        }

        SortedList<DateTime, DialogueEntry> Entries;
        GameObject Parent, Speaker;
        public States State;
      //  UI.SpeechBubble Bubble;

        public Conversation(GameObject parent, GameObject speaker)
        {
            this.Parent = parent;
            this.Speaker = speaker;
            this.Entries = new SortedList<DateTime, DialogueEntry>();
        }
        public void Start()
        {
            SpeechComponent.StartConversation(this.Parent, this);
            SpeechComponent.StartConversation(this.Speaker, this);
        }
        public void Add(GameObject speaker, string text, params DialogueOption[] options)
        {
            DateTime now = DateTime.Now;
            Entries.Add(now, new DialogueEntry(now, speaker, text));
            //Log.Command(Log.EntryTypes.Dialogue, now, speaker, text, options);

            //this.Bubble = UI.SpeechBubble.Create(speaker, text, options);
            //this.Bubble.OptionSelected += new EventHandler(Bubble_OptionSelected);
            //this.Bubble.Paused = true;
            speaker.Net.EventOccured(Message.Types.ChatEntity, speaker, text);

        }

        public void Speak(GameObject speaker, string text, params DialogueOption[] options)
        {
            DateTime now = DateTime.Now;
            Entries.Add(now, new DialogueEntry(now, speaker, text));
            //(speaker.Net as Server).AIHandler.AIDialog(speaker, speaker, text, options);
        }

        public bool OutOfRange()
        {
            return Vector3.Distance(this.Parent.Global, this.Speaker.Global) > 5;
        }

        public void Update(string text, params DialogueOption[] options)
        {
            Log.Command(Log.EntryTypes.Dialogue, DateTime.Now, this.Parent, text, options);
            //if (options.Length == 0)
            //    End(text);
            //else
            //    this.Bubble.Initialize(text, options);
        }

        //void Bubble_OptionSelected(object sender, EventArgs e)
        //{
        //    GameObject.PostMessage(this.Speaker, Message.Types.Speak, this.Speaker, (string)sender);
        //    GameObject.PostMessage(this.Parent, Message.Types.DialogueOption, this.Speaker, this, (string)sender);
        //}
        //void End()
        //{
        //    //this.Bubble.OptionSelected -= Bubble_OptionSelected;
        //    //this.Bubble.Paused = false;
        //    //this.Bubble.Hide();
        //}
        //public void End(string text)
        //{
        //    End();
        //    Log.Command(Log.EntryTypes.Chat, this.Parent, text);
        //    //this.Bubble = UI.SpeechBubble.Create(Parent, text);
        //}
    }
    [Obsolete]
    class SpeechComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Speech";
            }
        }

        public SpeechComponent()
        {

        }

        //static public event EventHandler<ObjectEventArgs> Speech;
        //static void OnSpeech(GameObject speaker, ObjectEventArgs a)
        //{
        //    UI.SpeechBubbleOld.Create(speaker, (string)a.Parameters[0]);
        //    if (Speech != null)
        //        Speech(speaker, a);
        //}

        public Conversation Conversation { get { return (Conversation)this["Conversation"]; } set { this["Conversation"] = value; } }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            Message.Types msg = e.Type;
            string text;
            //string[] options;
            DialogueOption[] options;
            switch (msg)
            {
                case Message.Types.DialogueEnd:
                    Log.Command(Log.EntryTypes.Chat, parent, (string)e.Parameters[0]);
                    this.Conversation = null;
                    return true;

                case Message.Types.ConversationFinish:
                    this.FinishConversation(parent, (string)e.Parameters[0]);
                    return true;

                default:
                    return true;
            }
        }

        static public void StartConversation(GameObject parent, Conversation convo)
        {
            SpeechComponent speech = parent.GetComponent<SpeechComponent>();
            if(speech == null)
            {
                convo.State = Conversation.States.Finished;
                return;
            }
            // do something with existing convo?
            speech.Conversation = convo;
        }

        //static public void Say(GameObject parent, string text)
        //{
        //    SpeechComponent speech = parent.GetComponent<SpeechComponent>();
        //    if (speech == null)
        //    {
        //        return;
        //    }
        //    speech.Conversation.Add(parent, text);
        //    parent.Net.EventOccured(Message.Types.Chat, parent, speech);
        //}

        /// <summary>
        /// don't add entry to conversation here, add it withing an AI behavior if necessary??
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="text"></param>
        /// 
        void FinishConversation(GameObject parent, string text)
        {
            this.Conversation.Add(parent, text);
            //if (this.Conversation != null)
                this.Conversation.State = Components.Conversation.States.Finished;
            this.Conversation = null;
        }

        static public void Say(GameObject entity, string text)
        {
            //var comp = entity.GetComponent<SpeechComponent>();
            entity.Net.EventOccured(Message.Types.ChatEntity, entity, text);
        }

        public override object Clone()
        {
            return new SpeechComponent();
        }
    }
}
