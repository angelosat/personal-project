namespace Start_a_Town_.Net
{
    interface ICommandParser
    {
        bool Execute(IObjectProvider net, string command);
    }
    class CommandParser
    {
        static public bool Execute(IObjectProvider net, PlayerData player, string command)
        {
            var p = command.Split(' ');
            var type = p[0];
            switch (type)
            {
                default:
                    GameModes.GameMode.Current.ParseCommand(net, command);
                    break;
            }
            return false;
        }

        static public bool Execute(IObjectProvider net, string command)
        {
            var p = command.Split(' ');
            var type = p[0];
            switch (type)
            {
                case "fog":
                    var client = net as Client;
                    if(client!=null)
                    {
                        ScreenManager.CurrentScreen.Camera.FogLevel = int.Parse(p[1]);
                    }
                    if (net is Server)
                        (net as Server).Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), SendType.OrderedReliable);
                    break;

                default:
                    GameModes.GameMode.Current.ParseCommand(net, command);
                    break;
            }
            return false;
        }
    }
}
