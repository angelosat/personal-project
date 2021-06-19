using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class TownJobManager
    {
        //TownJobManager(IObjectProvider net)
        //{
        //    net.GameEvent += net_GameEvent;
        //}

        //void net_GameEvent(object sender, GameEvent e)
        //{
        //    switch (e.Type)
        //    {
        //        case Message.Types.ScriptFinished:
        //            GameObject actor = e.Parameters[0] as GameObject;
        //            TargetArgs target = e.Parameters[1] as TargetArgs;
        //            Script.Types script = (Script.Types)e.Parameters[2];
        //            var found = this.Steps.FirstOrDefault(step => step.Target.Object == target.Object && step.Script == script);
        //            if (found.IsNull())
        //                return;
        //            this.Steps.Remove(found);
        //            break;

        //        default:
        //            break;
        //    }
        //    return;
        //}
    }
}
