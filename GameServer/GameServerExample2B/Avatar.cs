using System;
namespace GameServerExample2B
{
    public class SpaceShip : GameObject
    {
        public SpaceShip(GameServer server) : base(0,server)
        {
            Random random = new Random();

            randomSpawnY = random.Next(-4, 4);
            SetPosition(1, randomSpawnY);
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
            //Packet packet = new Packet(server, 3, this.Id, room.RoomId, Position.X, Position.Y);
            //packet.OneShot = true;
            //server.SendToAllInARoom(packet, room);
        }
    }
}
