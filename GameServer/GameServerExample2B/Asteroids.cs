using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerExample2B
{
    public class Asteroids : GameObject
    {
        public Asteroids(GameServer server) : base(1, server)
        {
           
            //SetVelocity(vel.X, vel.Y);

            //pos.X = 100;

            Random random = new Random();
            
            randomSpawnY = random.Next(-4, 4);
            SetLifeTime();
            SetPosition(10,randomSpawnY);
        }

        private float lifeTime;

        public float LifeTime
        {
            get
            {
                return lifeTime;
            }
        }

        private float lifeTimeOnServer;

        public float LifeTimeOnServer
        {
            get
            {
                return lifeTimeOnServer;
            }
        }

        private float randomSpawnY;

        public float RandomSpawnY
        {
            get
            {
                return randomSpawnY;
            }
        }
        

        public void SetLifeTime()
        {
            Random random = new Random();
            lifeTime = random.Next(8, 30);
            lifeTimeOnServer = server.Now + lifeTime + 2;
            lifeTime /= 10f;
        }

        public override void Tick(Room room)
        {
            //Packet packet = new Packet(server, 7,  Id, room.RoomId, lifeTime, randomSpawnY);
            //Packet packet = new Packet(server, 6, Id, Room.RoomId, lifeTime, randomSpawnY); fineVita
            //packet.OneShot = true;
            //server.SendToAllInARoom(packet, Room);

            //if(lifeTimeOnServer <= server.Now)
            //{
            //    //destroy Asteroids
            //}
            SetVelocity(Position.X, Position.Y);

        }
    
        public override void SetVelocity(float x, float y)
        {
            Position.X -=  0.000000012f * server.Now;
            Position.Y = y;
        }
    }
}
