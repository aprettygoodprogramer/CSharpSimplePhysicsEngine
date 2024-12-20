using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpSimplePhysicsEngine
{
    class Object
    {
        private Microsoft.Xna.Framework.Vector2 currPos;
        private Texture2D ballTexture;

        public Object(Vector2 position, Texture2D texture)
        {
            this.texture = texture;
            this.position = position; 
        }

        public Vector2 position { get; set; }
        public Texture2D texture { get; set; }
    }
}
