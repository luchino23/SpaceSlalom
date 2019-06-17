using System;
namespace GameServerExample2B
{
    public class SpaceShip : GameObject
    {
        Collider2D collider;

        public SpaceShip(GameServer server) : base(0,server)
        {
            Random random = new Random();
            collider = new Collider2D(this);
            randomSpawnY = random.Next(-4, 4);
            SetPosition(1, randomSpawnY);
            IsActive = true;
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

        public bool CheckCollisionWith(Collider2D collider)
        {
            return (this.collider.CollisionMask & collider.CollisionType) != 0;
        }

        public GameObject GetGameObject()
        {
            return this;
        }

        public Collider2D GetCollider()
        {
            return collider;
        }

        public bool GetIsActive()
        {
            return IsActive;
        }

        public bool GetIsCollisionAffected()
        {
            return isCollisionAffected;
        }

        public void SetIsActive(bool boolean)
        {
            IsActive = boolean;
        }

        public void SetIsCollisionAffected(bool boolean)
        {
            isCollisionAffected = boolean;
        }

        public override void Tick(Room room)
        {
            //Packet packet = new Packet(server, 3, this.Id, room.RoomId, Position.X, Position.Y);
            //packet.OneShot = true;
            //server.SendToAllInARoom(packet, room);
        }

       
    }
}
