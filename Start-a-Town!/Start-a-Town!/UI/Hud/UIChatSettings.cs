using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class UIChatSettings : Panel
    {
        //static UIChatSettings Instance;
        //static UIChatSettings()
        //{
        //    Instance = new UIChatSettings();
        //}
        //internal static void Refresh(UIChat chat)
        //{
        //    Instance.Chat = chat;
        //}
        UIChat Chat;
        CheckBox ChkTimestamps;
        public UIChatSettings(UIChat chat)
        {
            this.AutoSize = true;
            this.Chat = chat;
            this.ChkTimestamps = new CheckBox("Timestamps") { ValueFunction = () => this.Chat.Console.TimeStamp, LeftClickAction = ToggleTimestamps };
            this.AddControls(ChkTimestamps);
        }

        public void ToggleTimestamps()
        {
            this.Chat.Console.TimeStamp = !this.Chat.Console.TimeStamp;
            //Engine.Config.GetOrCreateElement("Interface").GetOrCreateElement("Timestamps").SetValue(this.Chat.Console.TimeStamp.ToString());
            //Engine.Config.Root.GetOrCreateElements("Interface", "Timestamps").SetValue(this.Chat.Console.TimeStamp.ToString());
            Engine.Config.SetValue("Interface/Timestamps", this.Chat.Console.TimeStamp);
            Engine.SaveConfig();
        }
    }
}
