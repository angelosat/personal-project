using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Rooms;

using Start_a_Town_.Components;

namespace Start_a_Town_.Control
{
    public class MessageTool : ControlTool
    {
        public Message.Types Message;
        public MessageTool()
        {
        }
        public MessageTool(GameObject target, Message.Types msg)
        {
            Target = target;
            Message = msg;
        }

        public override Messages OnMouseLeft(bool held)
        {
            Target.PostMessage(Message, Player.Actor);
            return Messages.Default;
        }

        //public override ControlTool.Messages MouseRightUp()
        //{
        //    return Messages.Remove;
        //}
    }
}
