using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;

namespace Start_a_Town_.Net
{
    class ServerConsole : GroupBox
    {
        static ServerConsole _Instance;
        static public ServerConsole Instance
        {
            get
            {
                if (_Instance.IsNull())
                    _Instance = new ServerConsole();
                return _Instance;
            }
        }

        ServerConsole()
        {
            Panel console = new Panel() { AutoSize = true };
            console.Controls.Add(Server.Instance.Log);

            Panel input = new Panel() { Location = console.BottomLeft, AutoSize = true };
            TextBox txtbox = new TextBox()
            {
                Width = Server.Instance.Log.Width,
                EnterFunc = (text) =>
                {
                    if (text.Length > 0)
                        Server.Command(text);
                },// Server.Console.Write(text),
                InputFunc = (prev, c) =>
                {
                    //if (!char.IsControl(c)) 
                    //    return prev + c;
                    return char.IsControl(c) ? prev : (prev + c);
                }
            };
        //    txtbox.Enabled = true;
            input.Controls.Add(txtbox);

            this.Controls.Add(console, input);
        }
    }
}
