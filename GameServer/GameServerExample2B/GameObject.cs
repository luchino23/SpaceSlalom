using System;
namespace GameServerExample2B
{
    //public abstract class GameObject
    public abstract class GameObject
    {
        public float X;
        public float Y;
        public float Z;

        protected GameClient owner;
        protected Room room;
        protected GameServer server;

        public bool IsOwnedBy(GameClient client)
        {
            return owner == client;
        }

        public Room Room
        {
            get {
                return room;
            }
        }

        public void SetOwner(GameClient client)
        {
            owner = client;
        }

        protected uint internalObjectType;
        public uint ObjectType
        {
            get
            {
                return internalObjectType;

            }
        }

        protected static uint gameObjectCounter;
        protected uint internalId;
        public uint Id
        {
            get
            {
                return internalId;
            }
        }

        public GameObject()
        {
            //Console.WriteLine("spawned GameObject {0} of type {1}", Id, ObjectType);
        }

        public GameObject(uint objectType,GameServer server)
        {
            internalObjectType = objectType;
            internalId = ++gameObjectCounter;
            this.server = server;
            Console.WriteLine("Added GameObject {0} of type {1}", Id, ObjectType);
        }

        public void Register(GameServer server)
        {
            this.server = server;
        }

        public void SetPosition(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public virtual void Tick()
        {

        }
    }
}
