using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerExample2B
{
    class SpaceShip2 : GameObject
    {
        public SpaceShip2(GameServer server) : base(0,server)
        {
            Random random = new Random();
            IsActive = true;
            isCollisionAffected = true;
            randomSpawnY = random.Next(-4, 4);
            SetPosition(-5, randomSpawnY);
            Width = 1f;
            Height = 0.4f;
        }
        private float randomSpawnY;

        public float RandomSpawnY
        {
            get
            {
                return randomSpawnY;
            }
        }

        public override void SetVelocity(float x, float y)
        {
            throw new NotImplementedException();
        }

        public override void Tick(Room room)
        {
            //Packet packet = new Packet(server, 12, this.Id, room.RoomId, Position.X, Position.Y);
            //packet.OneShot = true;
            //server.SendToAllInARoom(packet, room);
        }
    }
}

