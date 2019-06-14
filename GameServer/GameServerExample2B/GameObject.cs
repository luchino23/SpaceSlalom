﻿using System;
namespace GameServerExample2B
{

    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

    }

    //public abstract class GameObject
    public abstract class GameObject
    {
        //public float X;
        //public float Y;
        //public float Z;

        public Vector2 Position;

        public Vector2 Velocity;

        private float width;
        public float Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        public float Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }
        private float height;

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

        public GameObject(uint objectType,GameServer server, GameClient client = null)
        {
            internalObjectType = objectType;
            internalId = ++gameObjectCounter;
            this.server = server;

            Console.WriteLine("Added GameObject {0} of type {1}", Id, ObjectType);

            if (client != null)
                this.owner = client;

            server.RegisterGameObject(this, internalId, this.Id);
        }

        public void SetPosition(float x, float y)
        {
            Position.X = x;
            Position.Y = y;          
        }

        public abstract void SetVelocity(float x,float y);
        
            //Velocity.X = x;
            //Velocity.Y = y;
        

        public virtual void Update()
        {
            //Position.X += Velocity.X * (server.Now * 3);
            //Position.Y += Velocity.Y * (server.Now * 3);
        }

        public virtual void Tick(Room room)
        {
            
        }
    }
}
