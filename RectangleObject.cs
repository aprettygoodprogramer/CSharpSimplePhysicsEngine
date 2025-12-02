using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CSharpSimplePhysicsEngine
{
    public class PhysicsObject
    {
        public Vector2 Position;
        public Vector2 Size;
        public Vector2 Velocity;
        public float Angle;
        public float AngularVelocity;

        // Physics Properties
        public float Mass;
        public float InverseMass;
        public float Inertia;
        public float InverseInertia;

        public float Restitution = 0.1f;      // Bounciness 
        public float Friction = 0.2f;         // Grip 
        public float AngularDamping = 0.95f;  // Air resistance 

        public Texture2D Texture;
        public Color Color;
        public Vector2[] Vertices = new Vector2[4];
        public enum ObjectType {Circle, Rectangle }
        public ObjectType ShapeType;
        public float Radius;
        //box
        public PhysicsObject(Vector2 position, Vector2 size, Texture2D texture, float mass)
        {
            ShapeType = ObjectType.Rectangle;
            Position = position;
            Size = size;
            Texture = texture;
            Mass = mass;

            if (mass != 0)
            {
                InverseMass = 1.0f / mass;
                Inertia = (mass * (size.X * size.X + size.Y * size.Y)) / 12.0f;
                InverseInertia = 1.0f / Inertia;
            }
            else
            {
                InverseMass = 0.0f;
                InverseInertia = 0.0f;
            }

            Velocity = Vector2.Zero;
            Angle = 0.0f;
            AngularVelocity = 0.0f;

            // Color changes based on mass (White -> Red)
            float t = MathHelper.Clamp((mass - 1) / 20f, 0f, 1f);
            Color = Color.Lerp(Color.White, Color.Red, t);
        }
        //circle
        public PhysicsObject(Vector2 position, float radius, Texture2D texture, float mass)
        {
            ShapeType = ObjectType.Circle; 
            Position = position;
            Radius = radius;

            Size = new Vector2(radius * 2, radius * 2);

            Texture = texture;
            Mass = mass;

            if (mass != 0)
            {
                InverseMass = 1.0f / mass;
                Inertia = 0.5f * mass * (radius * radius);
                InverseInertia = 1.0f / Inertia;
            }
            else
            {
                InverseMass = 0.0f;
                InverseInertia = 0.0f;
            }

            Velocity = Vector2.Zero;
            Angle = 0.0f;
            AngularVelocity = 0.0f;

            float t = MathHelper.Clamp((mass - 1) / 20f, 0f, 1f);
            Color = Color.Lerp(Color.White, Color.Red, t);
        }


        public void UpdateVertices()
        {
            float w = Size.X / 2.0f;
            float h = Size.Y / 2.0f;

            Vector2[] local = new Vector2[]
            {
                new Vector2(-w, -h),
                new Vector2(w, -h),
                new Vector2(w, h),
                new Vector2(-w, h)
            };

            Matrix mat = Matrix.CreateRotationZ(Angle) * Matrix.CreateTranslation(Position.X, Position.Y, 0);

            for (int i = 0; i < 4; i++)
            {
                Vertices[i] = Vector2.Transform(local[i], mat);
            }
        }
    }
}