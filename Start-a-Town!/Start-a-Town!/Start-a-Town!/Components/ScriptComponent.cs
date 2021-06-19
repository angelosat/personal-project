using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Objects;
using Start_a_Town_.Components;

namespace Start_a_Town_.Components
{
    public class ScriptComponent : Component
    {

        public Script Script;
        public Message.Types Condition;
        public object[] Args;

        //public override bool HandleMessage(Message msg)
        //{
        //    //object[] args = new object[] {msg.
        //    if(msg.Type == Condition)
        //        return Script.Execute(msg.Args);
        //    return false;
        //}

        public void SetScript(Message.Types condition, string scriptName, object[] args)
        {
            Script = GameObject.GetScript(scriptName);
            Condition = condition;
            Args = args;
        }

        public override object Clone()
        {
            ScriptComponent scr = new ScriptComponent();
            scr.Script = Script;
            scr.Args = Args;
            scr.Condition = Condition;
            return scr;
        }
    }
}
