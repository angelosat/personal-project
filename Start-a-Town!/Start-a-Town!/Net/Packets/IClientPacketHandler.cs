namespace Start_a_Town_.Net
{
    public interface IClientPacketHandler
    {
        void HandlePacket(Client client, Packet packet);
    }
}
