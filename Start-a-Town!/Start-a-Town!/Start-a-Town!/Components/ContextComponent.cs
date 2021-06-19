using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class ContextOption
    {
        public string Name;
        public Message.Types Message;

        public ContextOption(string name = "<undefined>", Message.Types message = Components.Message.Types.Activate)
        {
            Name = name;
            Message = message;
        }

        public override string ToString()
        {
            return Name + " " + Message;
        }
    }
    class ContextComponent : Component
    {
        public List<ContextOption> Options;

        public ContextComponent(params object[] options)
        {
            if (options.Length % 2 != 0)
                throw (new ArgumentException());
            Options = new List<ContextOption>();
            ContextOption option;
            Queue<object> queue = new Queue<object>(options);
            while (queue.Count > 0)
            {
                option = new ContextOption((string)queue.Dequeue(), (Message.Types)queue.Dequeue());
                Options.Add(option);
                Console.WriteLine(option);
            }
            //Console.WriteLine(Options);
        }

        public override object Clone()
        {
            ContextComponent newComp = new ContextComponent();
            foreach (ContextOption option in Options)
                newComp.Options.Add(new ContextOption(option.Name, option.Message));
            return newComp;
        }
    }
}
