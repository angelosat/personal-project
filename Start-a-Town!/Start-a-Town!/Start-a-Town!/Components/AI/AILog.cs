using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.AI
{
    public class AILog
    {
        const int Capacity = 64;
        List<Entry> Inner = new List<Entry>();

        //public Entry Write(DateTime time, string text)
        //{
        //    var entry = new Entry(time, text);
        //    this.Inner.Add(entry);
        //    return entry;
        //}
        static public void TryWrite(GameObject entity, string text)
        {
            if (!entity.HasComponent<AIComponent>())
                return;
            var state = AIState.GetState(entity);
            //if (state == null)
            //    return;
            state.History.WriteEntry(entity, text);
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
        public List<Entry> GetEntries()
        {
            return this.Inner.ToList();
        }

        public class Entry
        {
            public DateTime Time;
            public string Text;
            public Entry(DateTime time, string text)
            {
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
