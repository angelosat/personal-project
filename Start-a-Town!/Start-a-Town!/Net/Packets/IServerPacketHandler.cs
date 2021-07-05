namespace Start_a_Town_.Net
{
    public interface IServerPacketHandler
    {
        void HandlePacket(Server server, Packet packet);
    }
}
