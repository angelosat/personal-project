using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
//using System.Timers;
using System.Threading;
using System.Diagnostics;
namespace Start_a_Town_.Net
{

    public class UdpConnection 
    {

        //TimeSpan _RTT;

        public EndPoint IP;// { get; set; }
        public Packet Partial;// { get; set; }
        public PlayerData Player;// { get; set; }
        public Socket Socket;// { get; set; }
        public string Name;// { get; set; }
        public byte[] Buffer;// { get; set; }
        public TimeSpan RTT;//{get;set;}
        public Stopwatch Ping;// { get; set; }
       // public Timer Timeout { get; set; }
        //{
        //    get { return _RTT; }
        //    set
        //    {
        //        _RTT = value;
        //        NotifyPropertyChanged("RTT");
        //    }
        //}
        //public UdpConnection()
        //{
        //    this.PingTimer = new Stopwatch();
        //}
        public UdpConnection(string name, Socket socket)
        //: this()
        {

            this.Name = name;
            this.Socket = socket;
        }
        public UdpConnection(string name, EndPoint ip)
        //: this()
        {
            this.Name = name;
            this.IP = ip;
        }
    }

    
}
