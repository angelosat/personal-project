using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.UI
{
    public class UIEventArgs : EventArgs
    {
        public Element Sender;
        public int Message;

        public UIEventArgs(Element sender)
        {
            Sender = sender;
        }
        public UIEventArgs(int message)
        {
            Message = message;
        }
    }
}
