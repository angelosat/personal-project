using System;
using System.IO;

namespace Start_a_Town_.Net
{
    public class EventSnapshot
    {
        public TimeSpan Time { get; set; }
        public GameObject Recipient { get; set; }
        public byte[] ObjectEventArgsData { get; set; }

        public EventSnapshot()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectEventArgsData">A serialized ObjectEventArgs.</param>
        public EventSnapshot(byte[] objectEventArgsData)
        {
            this.ObjectEventArgsData = objectEventArgsData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="objectEventArgsData">A serialized ObjectEventArgs.</param>
        public EventSnapshot(GameObject recipient, byte[] objectEventArgsData)
        {
            this.Recipient = recipient;
            this.ObjectEventArgsData = objectEventArgsData;
        }

    }
}
