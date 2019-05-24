using System;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;

namespace GameServerExample2B
{
    public class GameServer
    {

        private delegate void GameCommand(byte[] data, EndPoint sender);

        private Dictionary<byte, GameCommand> commandsTable;

        private Dictionary<EndPoint, GameClient> clientsTable;

        private IGameTransport transport;
        private IMonotonicClock clock;

        private uint roomId;
        public List<Room> Rooms;

        public uint NumClients
        {
            get
            {
                return (uint)clientsTable.Count;
            }
        }

       

        public Room GetEmptyRoom()
        {
            
            if (Rooms != null)
            {
                //find empty room
                for (int i = 0; i < Rooms.Count; i++)
                {
                    if (!Rooms[i].IsOccupy)
                        return Rooms[i];
                }
                //create an empty room
                Room room = new Room(this, roomId);
                roomId++;
                return room;
            }
            else
                return null;
        }

        //private void JoinRoom(byte[] data, EndPoint sender)
        //{
        //    GameClient newClient = new GameClient(this, sender);
        //    uint roomId = GetRoomIdFromClient(newClient);

        //    if(roomId < uint.MaxValue)
        //    {
        //        Packet JoinRoomPacket = new Packet(this, 5, roomId);
        //    }
        //}

        private void Join(byte[] data, EndPoint sender)
        {
            // check if the client has already joined
            if (clientsTable.ContainsKey(sender))
            {
                GameClient badClient = clientsTable[sender];
                badClient.Malus++;
                return;
            }

            GameClient newClient = new GameClient(this,sender);
            
            newClient.JoinInTheRoom(GetEmptyRoom());

            Room room = newClient.Room;
            room.JoinRoom(newClient);


            //Avatar avatar = Spawn<Avatar>();
            GameObject avatar = new GameObject();
            avatar.SetOwner(newClient);
            Packet welcome = new Packet(this, 1, avatar.ObjectType, avatar.Id, avatar.X, avatar.Y, avatar.Z);
            welcome.NeedAck = true;
            newClient.Enqueue(welcome);
            clientsTable[sender] = newClient;

            // spawn all server's objects in the new client
            foreach (GameObject gameObject in room.gameObjectsTable.Values)
            {
                if (gameObject == avatar)
                    continue;
                Packet spawn = new Packet(this, 2, gameObject.ObjectType, gameObject.Id, gameObject.X, gameObject.Y, gameObject.Z);
                spawn.NeedAck = true;
                SendToOthersRoom(spawn, room, newClient);
            }


            // informs the other clients about the new one
            Packet newClientSpawned = new Packet(this, 2, avatar.ObjectType, avatar.Id, avatar.X, avatar.Y, avatar.Z);
            newClientSpawned.NeedAck = true;
            SendToOthersRoom(newClientSpawned, room,newClient);

            Console.WriteLine("client {0} joined with avatar {1}", newClient, avatar.Id);

            Packet JoinRoomPacket = new Packet(this, 5, roomId);
            JoinRoomPacket.NeedAck = true;
            newClient.Enqueue(JoinRoomPacket);
            newClient.Process();
        }

        private void Ack(byte[] data, EndPoint sender)
        {
            if (!clientsTable.ContainsKey(sender))
            {
                return;
            }

            GameClient client = clientsTable[sender];
            uint packetId = BitConverter.ToUInt32(data, 1);
            client.Ack(packetId);
        }

        private void Update(byte[] data, EndPoint sender)
        {
            if (!clientsTable.ContainsKey(sender))
            {
                return;
            }
            GameClient client = clientsTable[sender];
            
            uint netId = BitConverter.ToUInt32(data, 1);
            uint roomId = BitConverter.ToUInt32(data, 5);
            Room room = GetRoomFromId(roomId);

            if (room.gameObjectsTable.ContainsKey(netId) && room != null)
            {
                GameObject gameObject = room.gameObjectsTable[netId];
                if (gameObject.IsOwnedBy(client))
                {
                    float x = BitConverter.ToSingle(data, 9);
                    float y = BitConverter.ToSingle(data, 13);
                    float z = BitConverter.ToSingle(data, 17);
                    gameObject.SetPosition(x, y, z);
                }
            }
        }

        public GameServer(IGameTransport gameTransport,IMonotonicClock clock)
        {
            Rooms = new List<Room>();
            transport = gameTransport;
            this.clock = clock;
            clientsTable = new Dictionary<EndPoint, GameClient>();
            commandsTable = new Dictionary<byte, GameCommand>();
            commandsTable[0] = Join;
            commandsTable[3] = Update;
            //commandsTable[5] = JoinRoom;
            commandsTable[253] = StatusServer;
            commandsTable[255] = Ack;
        }

        public void Run()
        {
            
            Console.WriteLine("server started");
            while (true)
            {
                SingleStep();
            }
        }

        private float currentNow;

        public float Now
        {
            get
            {
                return currentNow;
            }
        }

        public void StatusServer(byte[] data, EndPoint sender)
        {
            if(data != null)
            {
                
                Console.WriteLine("Online" + sender.AddressFamily);
            }
        }

        public void SingleStep()
        {
          //  currentNow = clock.GetNow();
            EndPoint sender = transport.CreateEndPoint();
            byte[] data = transport.Recv(256, ref sender);
            if (data != null)
            {
                byte gameCommand = data[0];
                if (commandsTable.ContainsKey(gameCommand))
                {
                    commandsTable[gameCommand](data, sender);
                }
            }

            foreach (GameClient client in clientsTable.Values)
            {
                client.Process();
            }

            foreach (Room room in Rooms)
            {
                foreach (GameObject gameObject in room.gameObjectsTable.Values)
                gameObject.Tick();
            }
        }

        public bool Send(Packet packet, EndPoint endPoint)
        {
            return transport.Send(packet.GetData(), endPoint);
        }

        public Room GetRoomFromId(uint id)
        {
            foreach (Room room in Rooms)
            {
                if(room.RoomId == id)
                {
                    return room;
                }
            }
            return null;
        }

        public uint GetRoomIdFromClient(GameClient client)
        {
            foreach (Room room in Rooms)
            {
                if (room.Player1 == client || room.Player2 == client)
                {
                    return room.RoomId;
                }
            }
            return uint.MaxValue;
        }

        public void SendToOthersRoom(Packet packet, Room room, GameClient except = null)  //sendToAllExcpetOne
        {
            if (except != room.Player1 && room.Player1 != null)
                room.Player1.Enqueue(packet);
            else if (except != room.Player2 && room.Player1 != null)
                room.Player2.Enqueue(packet);
        }

        public void SendToAllInARoom(Packet packet, Room room) //sendToAllClients
        {
            room.Player1.Enqueue(packet);
            room.Player2.Enqueue(packet);
        }

       //public bool RegisterGameObject(GameObject gameObject,uint roomID,Room room)
       //{
       //     room.RegisterGameObject(gameObject, roomID);
       //}

        //public T Spawn<T>() where T : GameObject
        //{
        //    object[] ctorParams = { this };
        //    T newGameObject = Activator.CreateInstance(typeof(T), ctorParams) as T;
        //    //RegisterGameObject(newGameObject);
        //    return newGameObject;
        //}
    }
}
