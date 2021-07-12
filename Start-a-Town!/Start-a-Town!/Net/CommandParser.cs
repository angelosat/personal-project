using Microsoft.Xna.Framework;

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
                case "teleport":
                    int x=  int.Parse(p[1]);
                    int y= int.Parse(p[2]);
                    int z = int.Parse(p[3]);
                    var pos = new Vector3(x,y,z);
                    if (!net.Map.IsInBounds(pos))
                        break;
                    player.ControllingEntity.MoveTo(pos);
                    byte[] data = Network.Serialize(w =>
                    {
                        w.Write(player.ControllingEntity.RefID);
                        w.Write(pos);
                    });
                    if (net is Server)
                        (net as Server).Enqueue(PacketType.ChangeEntityPosition, data, SendType.OrderedReliable);
                    break;


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
