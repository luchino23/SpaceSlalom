using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerExample2B
{
    public class Room
    {
        private GameServer server;

        private List<GameClient> clientsTable;
        private List<Asteroids> asteroidsList;

        public List<Asteroids> AsteroidsList()
        {
            return asteroidsList;
        }

        public bool RoomContainsThisClient(uint id)
        {
            foreach (GameClient client in clientsTable)
                if (client.Id == id)
                    return true;
            return false;
        }
        public List<GameClient> GetClientTable()
        {
            return clientsTable;
        }
        public GameClient GetClient(uint id)
        {
            foreach (GameClient client in clientsTable)
                if (client.Id == id)
                    return client;
            return null;
        }
        
        public GameClient Player1
        {
            get { return clientsTable[0]; }
        }

        public GameClient Player2        
        {
            get
            {
                if (clientsTable.Count > 1)
                    return clientsTable[1];
                return null;
            }
        }

        public const int ROOM_MAXSIZE = 2;
        private bool gameStarted;
        public bool GameStarted { get { return gameStarted; } }
        public void StartGame()
        {
            gameStarted = true;
        }

        private uint id;
        public uint RoomId
        {
            get { return id; }
        }

        private float asteroidTimeSpawn;

        public Dictionary<uint, GameObject> gameObjectsTable;
        
        public uint NumGameObjects
        {
            get
            {
                return (uint)gameObjectsTable.Count;
            }
        }

      

        public bool RegisterGameObject(GameObject gameObject, uint id)
        {
            if (!gameObjectsTable.ContainsKey(id))
            {
                gameObjectsTable.Add(id, gameObject);

                Console.WriteLine("Added in room : {0} GameObject Type : {1}, Id : {2})", RoomId, gameObject.ObjectType, gameObject.Id);
                return true;

            }
            return false;
        }

        public void UpdateRoom()
        {
            if (gameStarted)
            {
                foreach (GameObject gameObj in gameObjectsTable.Values)
                {
                    gameObj.Tick(this);
                    server.MoveAsteroids(this);

                }

                if (server.Now >= asteroidTimeSpawn)
                {
                    server.SpawnAsteroids(this);
                    //server.SpawnAvatar(this);
                    SetSpawnTimer();
                }

                if (Player1 != null && Player2 != null)
                    if (Player1.IsReady && Player2.IsReady && !gameStarted)
                    {
                        server.GameStart(this);
                        //server.SpawnAvatar(this);
                    }

               
            }
        }
       

        public GameObject GetGameObjectFromId(uint id)
        {
            if (gameObjectsTable.ContainsKey(id))
                return gameObjectsTable[id];
            return null;
        }

        public bool ContainsClient(GameClient client)
        {
            return clientsTable.Contains(client);
        }

        public int CountClient()
        {
            return clientsTable.Count();
        }
       
        public void SetPlayersReady(uint clientId)
        {          
            if(RoomContainsThisClient(clientId))
            {
                GameClient client = GetClient(clientId);

                
                client.SetReady(true);

                Console.WriteLine(client.IsReady + "client");

                if (Player1 != null && Player2 != null)
                    if (Player1.IsReady && Player2.IsReady && !gameStarted)
                    {
                        server.GameStart(this);
                        server.SpawnAvatar(this,client);
                        server.SpawnAvatar2(this,client);
                    }

            }
        }

        public bool IsOccupy
        {
            get
            {
                return clientsTable.Count >= ROOM_MAXSIZE;
            }
        }

        public void AddGameClient(GameClient newGameClient)
        {
            if (clientsTable.Count < ROOM_MAXSIZE)
            {
                clientsTable.Add(newGameClient);
                if (newGameClient == Player1)
                    newGameClient.JoinInTheRoom(this);
                else if (newGameClient == Player2)
                    newGameClient.JoinInTheRoom(this);
            }
        }

        public Room(GameServer server, uint roomId)
        {
            clientsTable = new List<GameClient>(ROOM_MAXSIZE);
            gameObjectsTable = new Dictionary<uint, GameObject>();
            asteroidsList = new List<Asteroids>();
            this.server = server;
            this.id = roomId;
            gameStarted = false;
        }

        private void SetSpawnTimer()
        {
            Random random = new Random();
            float offset = random.Next(5, 10);
           // offset *= 1;
            asteroidTimeSpawn = server.Now + offset;

        }

        public void JoinRoom(GameClient client)
        {
         
            if(!this.ContainsClient(client))
            {
                for (int i = 0; i < clientsTable.Count; i++)
                {
                    if(clientsTable[i] == null)
                    {
                        clientsTable[i] = client;
                        return;
                    }
                    
                }
            }     
          
        }
    }
}
