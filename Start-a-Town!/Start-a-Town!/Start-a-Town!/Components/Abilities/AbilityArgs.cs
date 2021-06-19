using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components
{
    public class ScriptArgs
    {
        public Net.IObjectProvider Net;
        public GameObject Actor;
        public TargetArgs Target; //Actor,
        public object[] Parameters;
        public byte[] Args;
        //public AbilityArgs(GameObject actor)
        //{
        //    this.Actor = actor; //new TargetArgs(actor);// 
        //    this.Target = TargetArgs.Empty;// actor;
        //  //  this.Parameters = new object[0];// new List<object>();
        //}
        public ScriptArgs(Net.IObjectProvider net, GameObject actor)
        {
            this.Net = net;
            this.Actor = actor; //new TargetArgs(actor);// 
            this.Target = TargetArgs.Empty;// actor;
            this.Args = new byte[0];
            //  this.Parameters = new object[0];// new List<object>();
        }
        public ScriptArgs(GameObject actor, GameObject target)
        {
            //this.Actor = actor;
            //this.Target = target;
            this.Actor = actor; //new TargetArgs(actor);// 
            this.Target = new TargetArgs(target);// actor;
        //    this.Parameters = new object[0];//new List<object>();
        }
        public ScriptArgs(GameObject actor, TargetArgs target, object[] parameters)
        {
            this.Actor = actor; //new TargetArgs(actor);// 
            this.Target = target;
            this.Parameters = parameters;// new List<object>(parameters);
        }
        public ScriptArgs(IObjectProvider net, GameObject actor, TargetArgs target)
        {
            this.Net = net;
            this.Actor = actor; //new TargetArgs(actor);// 
            this.Target = target;
            this.Args = new byte[0];
        }
        public ScriptArgs(IObjectProvider net, GameObject actor, TargetArgs target, byte[] parameters)
        {
            this.Net = net;
            this.Actor = actor; //new TargetArgs(actor);// 
            this.Target = target;
            this.Args = parameters;// new List<object>(parameters);
        }
        public ScriptArgs(IObjectProvider net, GameObject actor, TargetArgs target, Action<BinaryWriter> byteWriter)
        {
            this.Net = net;
            this.Actor = actor; //new TargetArgs(actor);// 
            this.Target = target;
            using(BinaryWriter w = new BinaryWriter(new MemoryStream()))
            {
                byteWriter(w);
                this.Args = (w.BaseStream as MemoryStream).ToArray();
            }
            
        }
    }
}
