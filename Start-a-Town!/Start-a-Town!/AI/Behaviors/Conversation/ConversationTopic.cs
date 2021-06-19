using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class ConversationTopic : Def
    {

        static public readonly ConversationTopic Guidance = new("Guidance")
        {
            Apply = (actor, convo) =>
            {
                var pop = actor.Map.World.Population;
                foreach (var p in convo.GetParticipants().Where(a => a != actor))
                {
                    p.ModifyNeed(VisitorNeedsDefOf.Guidance, n => 50);
                    var props = pop.GetVisitorProperties(p);
                    //props.RequiresGuidance = false;
                    //props.Guide = null;
                    actor.Net.Write(string.Format("{0} received guidance by {1}", p.Name, actor.Name));
                }
            },
            ApplyNew = (source, target) =>
            {
                //source.Map.World.Population.GetVisitorProperties(target).RequiresGuidance = false;
                target.ModifyNeed(VisitorNeedsDefOf.Guidance, n => n + 15);
                target.Net.Report(string.Format("{0} received guidance by {1}", source.Name, target.Name));
            },
            Tick = (source, target) =>
            {
                //source.is
            }
        };
        static public readonly ConversationTopic Riches = new("Riches");
        static public readonly ConversationTopic Fame = new("Fame");
        static public readonly ConversationTopic Lore = new("Lore");


        //public Action<Actor, Actor> Apply;
        public Action<Actor, ConversationNew> Apply;
        public Action<Actor, Actor> ApplyNew;
        public Action<Actor, Actor> Tick;

        public int MaxTicks = 5;

        public ConversationTopic(string name) : base(name)
        {
        }
        static ConversationTopic()
        {
            Def.Register(Guidance);
            Def.Register(Riches);
            Def.Register(Fame);
            Def.Register(Lore);
        }
    }

    public class ConversationNew
    {
        //public Actor ParticipantA, ParticipantB; //TODO extend participants to a list?
        readonly List<Actor> Participants = new(2);

        internal void Finish()
        {
            this.Finished = true;
            //foreach (var p in this.Participants)
            //    p.GetState().ConversationNew = null;
        }
        internal void Start()
        {
            //foreach (var a in this.Participants)
            //    a.GetState().ConversationNew = this;

        }
        public int Step;
        internal bool Finished;

        public ConversationNew(Actor initiator, params Actor[] otherParticipants)
        {
            if (otherParticipants.Contains(initiator))
                throw new Exception();
            this.Participants.Add(initiator);
            this.Participants.AddRange(otherParticipants);
        }
        public Actor CurrentTalker
        {
            get
            {
                var participantCount = this.Participants.Count;
                return this.Participants[Step % participantCount];
            }
        }
        public void Advance(Actor actor, ConversationTopic topic)
        {
            if (this.CurrentTalker != actor)
                throw new Exception();
            topic.Apply(actor, this);
            actor.Net.Report(string.Format("conversation advanced {0}", topic.ToString()));
            this.Step++;
        }
        public IEnumerable<Actor> GetParticipants()
        {
            foreach (var p in this.Participants)
                yield return p;
        }

      
    }
}
