using System;
namespace GameServerExample2B
{
    public class SpaceShip : GameObject
    {
        public SpaceShip(GameServer server) : base(1,server)
        {
        }

        public override void Tick()
        {
            Packet packet = new Packet(server, 3, Id, Room.RoomId, X, Y, Z);
            packet.OneShot = true;
            server.SendToAllInARoom(packet,Room);
        }
    }
}
