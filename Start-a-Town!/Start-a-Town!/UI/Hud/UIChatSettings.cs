using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class UIChatSettings : Panel
    {
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
            Engine.Config.SetValue("Interface/Timestamps", this.Chat.Console.TimeStamp);
            Engine.SaveConfig();
        }
    }
}
