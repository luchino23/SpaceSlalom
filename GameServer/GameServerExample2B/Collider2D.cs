using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerExample2B
{
    public struct CollisionInfo
    {
        public float deltaX;
        public float deltaY;
        public Collider2D collider;
    }

    public class Collider2D
    {
        public GameObject OwnerGameObject;
        public CollisionInfo collisionInfo;

        public Vector2 Position
        {
            get { return OwnerGameObject.Position; }
        }

        public float Width
        {
            get
            {
                return OwnerGameObject.Width;
            }
        }

        public float Height
        {
            get { return OwnerGameObject.Height; }
        }

        public float HalfWidth
        {
            get { return Width / 2; }
        }
        public float HalfHeight
        {
            get { return Height / 2; }
        }

        public uint CollisionType;
        public uint CollisionMask;

        public Collider2D(GameObject gameObject)
        {
            OwnerGameObject = gameObject;
        }

        public bool Collides(Collider2D collider)
        {

            float distanceX = collider.Position.X - Position.X;
            float distanceY = collider.Position.Y - Position.Y;

            float deltaX = Math.Abs(distanceX) - (HalfWidth + collider.HalfWidth);
            float deltaZ = Math.Abs(distanceY) - (HalfHeight + collider.HalfHeight);

            if (deltaX <= 0 && deltaZ <= 0)
            {
                collisionInfo.deltaX = -deltaX;
                collisionInfo.deltaY = -deltaZ;
                collisionInfo.collider = collider;
                return true;
            }
            return false;
        }

        public void AddCollision(uint mask)
        {
            CollisionMask |= mask;
        }
    }
}

