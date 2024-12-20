using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSimplePhysicsEngine
{
    class RectangleObject : Object
    {
        //private Microsoft.Xna.Framework.Vector2 size;

        public Vector2 Size { get; set; }

        public RectangleObject(Microsoft.Xna.Framework.Vector2 position, Texture2D texture)
            : base(position, texture)
        {
            
        }


    }
}
