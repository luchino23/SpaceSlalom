using System;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace GameServerExample2B
{
    public class GameServer
    {
        private delegate void GameCommand(byte[] data, EndPoint sender);

        private Dictionary<byte, GameCommand> commandsTable;

        private Dictionary<EndPoint, GameClient> clientsTable;

        private Dictionary<uint,GameObject> gameObjectToDelete;

        public GameClient GetClientFromID(uint id)
        {
            foreach (GameClient client in clientsTable.Values)
                if (client.Id == id)
                    return client;
            return null;
        }
        public const byte WELCOME_COMMAND = 1;


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

            Packet welcome = new Packet(this, WELCOME_COMMAND, newClient.Id, room.RoomId);
            newClient.Enqueue(welcome);
            Console.WriteLine("Welcome"); 
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

            if (room.gameObjectsTable.ContainsKey(netId) && room != null && room.gameObjectsTable[netId].IsActive)
            {
                GameObject gameObject = room.gameObjectsTable[netId];
                if (gameObject.IsOwnedBy(client))
                {
                    float x = BitConverter.ToSingle(data, 9);
                    float y = BitConverter.ToSingle(data, 13);
                    gameObject.SetPosition(x, y);
                    Packet moveOtherPlayer = new Packet(this, 12, netId, roomId, x, y);
                    if (netId == 1)
                    {
                        room.Player2.Enqueue(moveOtherPlayer);
                    }
                    else if (netId == 2)
                    {
                        room.Player1.Enqueue(moveOtherPlayer);
                    }
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
            gameObjectToDelete = new Dictionary<uint, GameObject>();
            GameObject.gameObjectCounter = 0;
            commandsTable[0] = Join;
            //welcome [1]
            //spawn[2]
            commandsTable[3] = Update;
            //commandsTable[5] = JoinRoom;
            //commandsTable[6] = SpawnAsteroids;
            //commandsTable[7] = UpdateAsteroids;
            commandsTable[8] = Ready;
            //commandsTable[9] = DestroyAsteroid;
            //commandsTable[10] = SpaceShipCollision;
            commandsTable[253] = StatusServer;
           // commandsTable[255] = Ack;
        }

        private void Ready(byte[] data, EndPoint sender)
        {
            uint clientId = BitConverter.ToUInt32(data, 1);
            GameClient client = GetClientFromID(clientId);
            uint roomId = BitConverter.ToUInt32(data, 5);         
            Room room = GetRoomFromId(roomId);
            if(room != null && room.RoomContainsThisClient(clientId))
                room.SetPlayersReady(clientId);

            if(clientId == 1)
            {
                Packet avatarSpawn = SpawnAvatar(room, client);
                room.Player1.Enqueue(avatarSpawn);
            }

            else if(clientId == 2)
            {
                SpawnAvatar2(room, client);
                Packet avatarSpawn = SpawnAvatar(room, client);
                room.Player2.Enqueue(avatarSpawn);
            }
        }

        public void GameStart(Room room)
        {
            if (!room.GameStarted)
            {
                Console.WriteLine("Game start in room " + room.RoomId);
                Packet startGamePacket = new Packet(this, 5);

                Send(startGamePacket, room.Player1.GetEndPoint());
                Send(startGamePacket, room.Player2.GetEndPoint());

                room.StartGame();
            }
        }

        public void SpawnAsteroids(Room room)
        {          
            Asteroids asteroid = Spawn<Asteroids>(room.RoomId);
            room.RegisterGameObject(asteroid,asteroid.Id);      
            Packet asteroidSpawn = new Packet(this, 6, asteroid.ObjectType, asteroid.Id, room.RoomId, asteroid.Position.X, asteroid.Position.Y);
           
            Console.WriteLine("lenght Asteroid : " + asteroidSpawn.GetData().Length);
            SendToAllInARoom(asteroidSpawn,room);
        }

        public void MoveAsteroids(Room room)
        {
            foreach (KeyValuePair<uint, GameObject> gO in room.gameObjectsTable)
            {
                if (gO.Key == 0 || gO.Key == 1)
                    continue;
                Packet moveAsteroid = new Packet(this, 13, gO.Value.ObjectType, gO.Value.Id, room.RoomId, gO.Value.Position.X, gO.Value.Position.Y);
                SendToAllInARoom(moveAsteroid, room);
            }
        }

        public Packet SpawnAvatar(Room room, GameClient client)
        {
            if (room.Player1.GameObject == null)
            {
                SpaceShip player = Spawn<SpaceShip>(room.RoomId);
                player.SetOwner(client);
                GetRoomFromId(room.RoomId).RegisterGameObject(player, player.Id);               
                Packet avatarSpawn = new Packet(this, 2, player.ObjectType, player.Id, room.RoomId, player.Position.X, player.Position.Y);
                return avatarSpawn;
            }
            else
            {
                Packet avatarSpawn = new Packet(this, 2, room.Player1.GameObject.ObjectType, room.Player1.GameObject.Id, room.RoomId, room.Player1.GameObject.Position.X, room.Player1.GameObject.Position.Y);
                return avatarSpawn;
            }
        }

        public void SpawnAvatar2(Room room, GameClient client)
        {
            SpaceShip2 player2 = Spawn<SpaceShip2>(room.RoomId);
            player2.SetOwner(client);
            GetRoomFromId(room.RoomId).RegisterGameObject(player2, player2.Id);
            Packet avatarSpawn = new Packet(this, 11, player2.ObjectType, player2.Id, room.RoomId, player2.Position.X, player2.Position.Y);
            SendToAllInARoom(avatarSpawn, room);
        }

        public void OnAsteroidCollision(Room room)
        {            
            for (uint i = 1; i < room.gameObjectsTable.Count - 1; i++)
            {
                {
                    for (uint j = i + 1; j < room.gameObjectsTable.Count; j++)
                    {
                        if(room.gameObjectsTable[i].IsActive && room.gameObjectsTable[i].isCollisionAffected)
                        {
                            if (room.gameObjectsTable[i].collider.Collides(room.gameObjectsTable[j].collider))
                            {
                                room.gameObjectsTable[i].OnCollide();
                                room.gameObjectsTable[j].OnCollide();
                                var myKey1 = room.gameObjectsTable.FirstOrDefault(x => x.Value == room.gameObjectsTable[i]).Key;
                                var myKey2 = room.gameObjectsTable.FirstOrDefault(x => x.Value == room.gameObjectsTable[j]).Key;
                                gameObjectToDelete.Add(myKey1,room.gameObjectsTable[i]);
                                gameObjectToDelete.Add(myKey2,room.gameObjectsTable[j]);
                            }

                            foreach (KeyValuePair<uint,GameObject> gameObject in gameObjectToDelete)
                            {
                                room.gameObjectsTable.Remove(gameObject.Key);
                            }
                            gameObjectToDelete.Clear();
                        }
                    }
                }
            }
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
                OnAsteroidCollision(room);
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
          //  packet.OneShot = true;
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

        //public bool RegisterGameObject(GameObject gameObject, uint roomID, uint id)
        //{
        //    Room room = GetRoomFromId(roomID);
        //    if (room != null)
        //    {
        //        return room.RegisterGameObject(gameObject, id);
        //    }
        //    return false;
        //}

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
            //RegisterGameObject(newGameObject, roomId, newGameObject.Id);
            newGameObject.Room = Rooms[(int)roomId];
            return newGameObject;
        }
    }
}
