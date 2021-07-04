using System;
using System.Linq;
using Start_a_Town_.Net;
using System.IO;

namespace Start_a_Town_
{
    public partial class QuestsManager
    {
        static class Packets
        {
            static public void Init()
            {
                // TODO
                Server.RegisterPacketHandler(PacketType.QuestCreate, ReceiveQuestCreate);
                Client.RegisterPacketHandler(PacketType.QuestCreate, ReceiveQuestCreate);

                Server.RegisterPacketHandler(PacketType.QuestRemove, ReceiveRemoveQuest);
                Client.RegisterPacketHandler(PacketType.QuestRemove, ReceiveRemoveQuest);

                Server.RegisterPacketHandler(PacketType.QuestCreateObjective, ReceiveQuestCreateObjective);
                Client.RegisterPacketHandler(PacketType.QuestCreateObjective, ReceiveQuestCreateObjective);

                Server.RegisterPacketHandler(PacketType.QuestRemoveObjective, ReceiveQuestRemoveObjective);
                Client.RegisterPacketHandler(PacketType.QuestRemoveObjective, ReceiveQuestRemoveObjective);

                Server.RegisterPacketHandler(PacketType.QuestGiverAssign, ReceiveQuestGiverAssign);
                Client.RegisterPacketHandler(PacketType.QuestGiverAssign, ReceiveQuestGiverAssign);

                Server.RegisterPacketHandler(PacketType.QuestModify, ReceiveQuestModify);
                Client.RegisterPacketHandler(PacketType.QuestModify, ReceiveQuestModify);
            }
            public static void SendQuestModify(IObjectProvider net, PlayerData player, QuestDef quest, int maxConcurrentModValue)
            {
                if (net is Server)
                    quest.MaxConcurrent = maxConcurrentModValue;
                net.GetOutgoingStream().Write((int)PacketType.QuestModify, player.ID, quest.ID, maxConcurrentModValue);
            }
            private static void ReceiveQuestModify(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var maxConcurrentModValue = r.ReadInt32();
                if (net is Client)
                    quest.MaxConcurrent = maxConcurrentModValue;
                else
                    SendQuestModify(net, player, quest, maxConcurrentModValue);
            }

            public static void SendQuestGiverAssign(IObjectProvider net, PlayerData player, QuestDef quest, Actor actor)
            {
                if(net is Server)
                    quest.Giver = actor;
                net.GetOutgoingStream().Write((int)PacketType.QuestGiverAssign, player.ID, quest.ID, actor?.RefID ?? -1);
            }
            private static void ReceiveQuestGiverAssign(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var actorid = r.ReadInt32();
                var actor = actorid == -1 ? null : net.GetNetworkObject(actorid) as Actor;
                if (net is Client)
                    quest.Giver = actor;
                else
                    SendQuestGiverAssign(net, player, quest, actor);
            }

            public static void SendQuestObjectiveRemove(IObjectProvider net, PlayerData player, QuestDef quest, QuestObjective qobj)
            {
                var index = quest.GetObjectives().ToList().FindIndex(i => i == qobj);
                if (net is Server server)
                    quest.RemoveObjective(qobj);
                var w = net.GetOutgoingStream();
                w.Write(PacketType.QuestRemoveObjective);
                w.Write(player.ID);
                w.Write(quest.ID);
                w.Write(index);
            }
            private static void ReceiveQuestRemoveObjective(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var objectiveIndex = r.ReadInt32();
                var objective = quest.GetObjectives().ElementAt(objectiveIndex);
                if (net is Server)
                    SendQuestObjectiveRemove(net, player, quest, objective);
                else
                    quest.RemoveObjective(objective);
            }

            public static void SendQuestCreateObjective(IObjectProvider net, PlayerData player, QuestDef quest, QuestObjective qobj)
            {
                if (net is Server server)
                {
                    quest.AddObjective(qobj);
                }
                var w = net.GetOutgoingStream();
                w.Write(PacketType.QuestCreateObjective);
                w.Write(player.ID);
                w.Write(quest.ID);
                w.Write(qobj.GetType().FullName);
                qobj.Write(w);
            }
            private static void ReceiveQuestCreateObjective(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var qObj = Activator.CreateInstance(Type.GetType(r.ReadString()), quest) as QuestObjective;
                qObj.Read(r);
                if (net is Server)
                {
                    SendQuestCreateObjective(net, player, quest, qObj);
                }
                else
                {
                    quest.AddObjective(qObj);
                }
            }
            internal static void SendAddQuestGiver(IObjectProvider net, int playerID)
            {
                var w = net.GetOutgoingStream();
                w.Write(PacketType.QuestCreate);
                w.Write(playerID);
                if (net is Server server)
                {
                    var manager = server.Map.Town.QuestManager;
                    var q = manager.CreateQuest();
                    manager.AddQuest(q);
                    w.Write(q.ID);
                }
                //else
                //    w.Write(-1);
            }
            private static void ReceiveQuestCreate(IObjectProvider net, BinaryReader r)
            {
                var playerID = r.ReadInt32();
                if (net is Server server)
                    SendAddQuestGiver(server, playerID);
                else
                {
                    var questID = r.ReadInt32();
                    var manager = net.Map.Town.QuestManager;
                    manager.AddQuest(questID);
                }
            }
            internal static void RemoveQuest(QuestsManager manager, int playerID, QuestDef quest)
            {
                var net = manager.Town.Net;
                var w = net.GetOutgoingStream();
                w.Write(PacketType.QuestRemove);
                //w.Write(net.GetPlayer().ID);
                w.Write(playerID);
                w.Write(quest.ID);
                if(net is Server server)
                {
                    manager.RemoveQuest(quest.ID);
                }
            }
            static void ReceiveRemoveQuest(IObjectProvider net, BinaryReader r)
            {
                var manager = net.Map.Town.QuestManager;
                var player = net.GetPlayer(r.ReadInt32());
                var questID = r.ReadInt32();
                if (net is Server server)
                    RemoveQuest(manager, player.ID, manager.GetQuest(questID)); // LOL 1
                else
                    manager.RemoveQuest(questID); // LOL 2
            }
        }
    }
}
