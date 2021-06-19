using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Modules.AI.Net.Packets;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    public class AILog
    {
        const int Capacity = 64;
        //List<Entry> Inner = new List<Entry>();
        readonly ObservableCollection<Entry> Inner = new();

        static public void SyncWrite(GameObject entity, string text)
        {
            //var entry = new Entry(DateTime.Now, text);
            //state.History.Inner.Add(entry);
            var net = entity.NetNew;
            if (net is not Server)
                return;
            AIState state = AIState.GetState(entity);
            PacketAILogWrite.Send(net as Server, entity.RefID, text);
            state.History.Write(text);
            //return state.History.WriteEntry(entity, text);
        }
        static public bool TryWrite(GameObject entity, string text)
        {
            if (!entity.HasComponent<AIComponent>())
                return false;
            var state = AIState.GetState(entity);
            //if (state == null)
            //    return;
            state.History.WriteEntry(entity as Actor, text);
            return true;
        }
        public Entry WriteEntry(GameObject entity, string text)
        {
            var entry = new Entry(DateTime.Now, text);
            this.Inner.Add(entry);
            if (this.Inner.Count > Capacity)
                this.Inner.RemoveAt(0);
            entity.Net.Map.EventOccured(Message.Types.AILogUpdated, entity, this, entry);
            return entry;
        }
        public Entry Write(string text)
        {
            var entry = new Entry(DateTime.Now, text);
            //text.ToConsole();
            this.Inner.Add(entry);
            if (this.Inner.Count > Capacity)
                this.Inner.RemoveAt(0);
            return entry;
        }
        
        public List<Entry> GetEntries()
        {
            return this.Inner.ToList();
        }
        
        public class UI
        {
            static readonly Lazy<TableScrollableCompactNewNew<Entry>> EntriesGUI = new(()=> new TableScrollableCompactNewNew<Entry>(10)//, BackgroundStyle.TickBox)
                    .AddColumn(null, "Time", (int)UIManager.Font.MeasureString("HH:mm:ss").X, (e) => new Label(e.Time.ToString("HH:mm:ss")), 0)
                    //.AddColumn(null, "Description", 400, (e) => new Label(e.Text.Wrap(400)), 0));
                    .AddColumn(null, "Description", 400, (e) => new GroupBox().AddControlsLineWrap(Label.ParseNew(e.Text)), 0));

            static public Control GetGUI(Actor actor)
            {
                return EntriesGUI.Value.Bind(actor.Log.Inner);
                //return EntriesGUI.Value.Bind(actor.Log.Inner, e => e.Time, Comparer<DateTime>.Default);
            }
        }

        public class Entry
        {
            //public Actor Actor;
            public DateTime Time;
            public string Text;
            public Entry(
                //Actor actor, 
                DateTime time, string text)
            {
                //this.Actor = actor;
                this.Time = time;
                this.Text = text;
            }
            public override string ToString()
            {
                //return this.Text;// Time.ToString() + ": " + this.Text;
                //return new DateTime(Time.Ticks).ToString("HH:mm:ss") + ": " + this.Text;
                return this.Time.ToString("HH:mm:ss") + ": " + this.Text;
            }
        }

        
    }
}
