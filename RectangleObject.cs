using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CSharpSimplePhysicsEngine
{
    class RectangleObject
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Texture2D Texture { get; set; }

        public Vector2 Vel { get; set; }


        public Rectangle BoundingBox => new Rectangle(
            (int)Position.X,
            (int)Position.Y,
            (int)Size.X,
            (int)Size.Y
        );

        public RectangleObject(Vector2 position, Texture2D texture)
        {
            Position = position;
            Texture = texture;
            Size = new Vector2(50, 100); 
        }
    }
}
