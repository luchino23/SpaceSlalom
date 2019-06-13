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

        public GameClient GetClientFromID(uint id)
        {
            foreach (GameClient client in clientsTable.Values)
                if (client.Id == id)
                    return client;
            return null;
        }

        private IGameTransport transport;
        private IMonotonicClock clock;

        public IMonotonicClock Clock
        {
            get
            {
                return clock;
            }
        }

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
                Rooms.Add(room);

                Console.WriteLine("Create room with id {0}", roomIdInServer);
                Console.WriteLine("Number of Rooms created {0}", Rooms.Count);
                return room;
            }
            else
                return null;
        }

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
            Room room = GetEmptyRoom();
            clientsTable[sender] = newClient;
            room.AddGameClient(newClient);

            SpaceShip avatar = Spawn<SpaceShip>(room.RoomId);
            
            avatar.SetOwner(newClient);
            Packet welcome = new Packet(this, 1, newClient.Id, room.RoomId);
            welcome.NeedAck = true;
            newClient.Enqueue(welcome);
            Console.WriteLine("Welcome");


            //spawn all server's objects in the new client
            foreach (GameObject gameObject in room.gameObjectsTable.Values)
            {
                if (gameObject == avatar)
                    continue;
                Packet spawn = new Packet(this, 2, gameObject.ObjectType, gameObject.Id,room.RoomId,gameObject.Position.X, gameObject.Position.Y);//, gameObject.Position.Z);
                spawn.NeedAck = true;


                SendToOthersRoom(spawn, room, newClient);



                Console.WriteLine("Spawn");
            }

            // informs the other clients about the new one
            Packet newClientSpawned = new Packet(this, 2, avatar.ObjectType, avatar.Id,room.RoomId,avatar.Position.X, avatar.Position.Y);//, avatar.Position.Z);
            newClientSpawned.NeedAck = true;
            SendToOthersRoom(newClientSpawned, room, newClient);

            Console.WriteLine("client {0} joined with avatar {1}", newClient, avatar.Id);

            Packet JoinRoomPacket = new Packet(this, 11, roomIdInServer);
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
                    //float z = BitConverter.ToSingle(data, 17);
                    gameObject.SetPosition(x, y);
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
            commandsTable[8] = Ready;
            //commandsTable[9] = DestroyAsteroid;
            //commandsTable[10] = SpaceShipCollision;
            
            commandsTable[253] = StatusServer;
            commandsTable[255] = Ack;
        }

        private void Ready(byte[] data, EndPoint sender)
        {
            uint clientId = BitConverter.ToUInt32(data, 1);
            GameClient client = GetClientFromID(clientId);
            uint roomId = BitConverter.ToUInt32(data, 5);         
            Room room = GetRoomFromId(roomId);
            if(room != null && room.RoomContainsThisClient(clientId))
                room.SetPlayersReady(clientId);
        }

        public void GameStart(Room room)
        {
            if (!room.GameStarted)
            {
                Console.WriteLine("Game start in room " + room.RoomId);
                Packet startGamePacket = new Packet(this, 5);
                startGamePacket.NeedAck = true;

                Send(startGamePacket, room.Player1.GetEndPoint());
                Send(startGamePacket, room.Player2.GetEndPoint());

                room.StartGame();
            }
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
            Asteroids asteroid = Spawn<Asteroids>(room.RoomId);      
            Packet asteroidSpawn = new Packet(this, 6, asteroid.ObjectType, asteroid.Id,room.RoomId, asteroid.Position.X, asteroid.Position.Y);
            Console.WriteLine("lenght: " + asteroidSpawn.GetData().Length);
            SendToAllInARoom(asteroidSpawn,room);
        }

        //public void SpawnAvatar(Room room)
        //{
        //    SpaceShip player = Spawn<SpaceShip>(room.RoomId);
        //    Packet avatarSpawn = new Packet(this, 2, player.ObjectType, player.Id, room.RoomId, player.Position.X, player.Position.Y);
        //    Console.WriteLine("lenght: " + avatarSpawn.GetData().Length);
        //    SendToAllInARoom(avatarSpawn, room);
        //}

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
        
        public bool RegisterGameObject(GameObject gameObject, uint roomID, uint id)
        {
            Room room = GetRoomFromId(roomID);
            if(room != null)
            {
                return room.RegisterGameObject(gameObject, id);
            }
            return false;
        }
        
        //public bool RegisterGameObject(GameObject gameObject, uint roomId)
        //{
        //    Room room = GetRoomFromId(roomId);
        //    if (room != null)
        //    {
        //        GetRoomFromId(roomId).RegisterGameObject(gameObject);
        //        return true;
        //    }
        //    return false;
        //}

        public T Spawn<T>(uint roomId) where T : GameObject
        {
            object[] ctorParams = { this };
            T newGameObject = Activator.CreateInstance(typeof(T), ctorParams) as T;
            RegisterGameObject(newGameObject, roomId, newGameObject.Id);
            return newGameObject;
        }
    }
}
