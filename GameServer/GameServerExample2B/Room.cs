using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerExample2B
{
    public class Room
    {
        private GameClient[] clients;

        private GameServer server;

         public GameClient Player1
         {
            get { return clients[0]; }
         }
        public GameClient Player2
        {
            get { return clients[1]; }
        }

        private uint id;
        public uint RoomId
        {
            get { return id; }
        }

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

        public void RegisterGameObject(GameObject gameObject,uint id)
        {
            gameObjectsTable.Add(id, gameObject);
        }

        //public GameObject GetIdOfGameObject(uint id)
        //{
        //    if (gameObjectsTable.ContainsKey(id))
        //    {
        //        gameObjectsTable.Add(id);
        //    }
        //}
      

        public bool IsOccupy
        {
            get
            {
                return Player1 == null && Player2 == null;
            }
        }

        public Room(GameServer server, uint roomId)
        {
            clients = new GameClient[2];
            gameObjectsTable = new Dictionary<uint, GameObject>();
            this.server = server;
            this.id = roomId;

        }

        public void JoinRoom(GameClient client)
        {
            if (Player1 == null)
                clients[0] = client;
            if (Player2 == null)
                clients[1] = client;

            client.JoinInTheRoom(this);
        }

        
    }
}
