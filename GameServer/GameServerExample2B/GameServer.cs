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

        private uint roomIdInServer;
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
                Room room = new Room(this, roomIdInServer);
                roomIdInServer++;              

                Console.WriteLine("Create room with id {0}", roomIdInServer);

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
            clientsTable[sender] = newClient;
            newClient.Id = (uint)clientsTable.Count;

            Room room = newClient.Room;            


            SpaceShip avatar = Spawn<SpaceShip>(room.RoomId);
            
            avatar.SetOwner(newClient);
            Packet welcome = new Packet(this, 1, room.RoomId, newClient.Id);
            welcome.NeedAck = true;
            newClient.Enqueue(welcome);
            Console.WriteLine("Welcome");


            // spawn all server's objects in the new client
            foreach (GameObject gameObject in room.gameObjectsTable.Values)
            {
                if (gameObject == avatar)
                    continue;
                Packet spawn = new Packet(this, 2, gameObject.ObjectType, gameObject.Id, gameObject.X, gameObject.Y, gameObject.Z);
                spawn.NeedAck = true;
                SendToOthersRoom(spawn, room, newClient);

                Console.WriteLine("Spawn");
            }


            // informs the other clients about the new one
            Packet newClientSpawned = new Packet(this, 2, avatar.ObjectType, avatar.Id, avatar.X, avatar.Y, avatar.Z);
            newClientSpawned.NeedAck = true;
            SendToOthersRoom(newClientSpawned, room,newClient);

            Console.WriteLine("client {0} joined with avatar {1}", newClient, avatar.Id);

            Packet JoinRoomPacket = new Packet(this, 5, roomIdInServer);
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
            //welcome [1]
            //spawn[2]
            commandsTable[3] = Update;
            //commandsTable[5] = JoinRoom;
            //commandsTable[6] = SpawnAsteroids;
            commandsTable[7] = UpdateAsteroids;
            commandsTable[8] = SetReady;
            //commandsTable[9] = DestroyAsteroid;
            //commandsTable[10] = SpaceShipCollision;
            
            commandsTable[253] = StatusServer;
            commandsTable[255] = Ack;
        }


        public void StartGame(uint roomId)
        {
            Room room = GetRoomFromId(roomId);

            if(clock.GetNow() == 7)
            {
                
            }

        }

        private void SetReady(byte[] data, EndPoint sender)
        {
            if (!clientsTable.ContainsKey(sender))
            {
                return;
            }

            uint roomId = BitConverter.ToUInt32(data, 1);
            Room room = GetRoomFromId(roomId);

            GameClient client = new GameClient(this, sender);

            bool ready = BitConverter.ToBoolean(data, 5);

            if (room.ContainsClient(client))
            {
                room.SetPlayerReady(client, ready);
            }

            StartGame(roomId);
           

            //Packet 
            //(client, roomid)
            //if(client in this.roomId)
            //if(p1.isready and p2.isready)
            //start
        }

        private void UpdateAsteroids(byte[] data, EndPoint sender)
        {
            if (!clientsTable.ContainsKey(sender))
            {
                return;
            }
            GameClient client = clientsTable[sender];

           // Packet packet = (7, roomId, pos.y);
        }

        public void SpawnAsteroids(Room room)
        {           

            Asteroids asteroid = Spawn<Asteroids>(roomIdInServer);
            
            Packet asteroidSpawn = new Packet(this, 6, asteroid.ObjectType, asteroid.Id, asteroid.X, asteroid.Y);
            SendToAllInARoom(asteroidSpawn, room);


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
            currentNow = clock.GetNow();
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
                room.UpdateRoom();
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
            else if (room.CountClient() > 1)
            {
               if( except != room.Player2 && room.Player2 != null)
               {
                    room.Player2.Enqueue(packet);
               }
            }
          
        }

        public void SendToAllInARoom(Packet packet, Room room) //sendToAllClients
        {
            room.Player1.Enqueue(packet);
            room.Player2.Enqueue(packet);
        }

        public bool RegisterGameObject(GameObject gameObject, uint roomID)
        {
            Room room = GetRoomFromId(roomID);
            if(room != null)
            {
                return room.RegisterGameObject(gameObject, roomID);
            }
            return false;
        }

        public T Spawn<T>(uint roomId) where T : GameObject
        {
            object[] ctorParams = { this };
            T newGameObject = Activator.CreateInstance(typeof(T), ctorParams) as T;
            RegisterGameObject(newGameObject, roomId);
            return newGameObject;
        }
    }
}
