using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.UI
{
    public class UIMessage
    {
        public int Sender;
        public Messages Message;
        public Parameters Parameter;

        public UIMessage(Control recepient, Messages message, Parameters param)
        {
            Sender = recepient.ID;
            Message = message;
            Parameter = param;
        }
        public UIMessage(Control recepient, Messages message)
        {
            Sender = recepient.ID;
            Message = message;
            Parameter = 0;
        }
        public UIMessage(int ID, Messages message, Parameters param)
        {
            Sender = ID;
            Message = message;
            Parameter = param;
        }
        public UIMessage(int ID, Messages message)
        {
            Sender = ID;
            Message = message;
            Parameter = 0;
        }
    }

    public enum Messages
    {
        CTRL_COMMAND = 1
    }
    public enum Parameters
    {
        BTN_PRESS = 1,
        CB_DROPDOWN,
        CB_CLOSEUP
    }
}
