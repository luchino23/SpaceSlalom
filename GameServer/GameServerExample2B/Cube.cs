using System;
namespace GameServerExample2B
{
    public class Cube : GameObject
    {
        public Cube(GameServer server) : base(2,server)
        {
        }

        public override void SetVelocity(float x, float y)
        {
            throw new NotImplementedException();
        }
    }
}
