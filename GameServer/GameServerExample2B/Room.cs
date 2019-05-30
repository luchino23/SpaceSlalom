using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerExample2B
{
    public class Room
    {
        private List <GameClient> clients;

        private GameServer server;

         public GameClient Player1
         {
            get { return clients[0]; }
         }
        public GameClient Player2
        {
            get { return clients[1]; }
        }

        private bool gameStarted;

        public bool GameStarted
        {
            get
            {
                return gameStarted;
            }
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

        public void RegisterGameObject(GameObject gameObject)
        {
            if (gameObjectsTable.ContainsKey(gameObject.Id))
                throw new Exception("GameObject already registered");
            gameObjectsTable[gameObject.Id] = gameObject;
        }

        public bool RegisterGameObject(GameObject gameObject,uint id)
        {
            if (!gameObjectsTable.ContainsKey(id))
            {
                gameObjectsTable.Add(id, gameObject);

                Console.WriteLine("Spawned GameObject Type : {0}, Id : {1})", gameObject.GetType(), gameObject.Id);
                return true;
                
            }
            return false;
        }

        public void UpdateRoom()
        {

            if (GameStarted)
            {
                foreach (GameObject gameObj in gameObjectsTable.Values)
                {
                    gameObj.Tick();
                }

                if (server.Now >= asteroidTimeSpawn)
                {
                    server.SpawnAsteroids(this);
                    SetSpawnTimer();
                }
            }
        
            
        }
       

        //public GameObject GetIdOfGameObject(uint id)
        //{
        //    if (gameObjectsTable.ContainsKey(id))
        //    {
        //        gameObjectsTable.Add(id);
        //    }
        //}
       public bool ContainsClient(GameClient client)
       {
            return clients.Contains(client);
       }

       public int CountClient()
       {
            return clients.Count();
       } 

        public void SetPlayerReady(GameClient client, bool isReady)
        {
            if(Player1 == client)
            {
                Player1.SetReady(isReady);
            }
            else if( Player2 == client)
            {
                Player2.SetReady(isReady);
            }

            if(Player1.IsReady && Player2.IsReady)
            {
                server.StartGame(RoomId);
            }
        }

        public bool IsOccupy
        {
            get
            {
                return Player1 == null && Player2 == null;
            }
        }

        public Room(GameServer server, uint roomId)
        {
            clients = new List<GameClient>(2);
            gameObjectsTable = new Dictionary<uint, GameObject>();
            this.server = server;
            this.id = roomId;

        }

        private void SetSpawnTimer()
        {
            Random random = new Random();
            float offset = random.Next(3, 10);
            asteroidTimeSpawn += offset;

        }

        public void JoinRoom(GameClient client)
        {
         
            if(!this.ContainsClient(client))
            {
                if (clients.Count < 2)
                {
                    clients.Add(client);
                    client.JoinInTheRoom(this);
                }
            }
            
           
          

          
        }

        
    }
}
