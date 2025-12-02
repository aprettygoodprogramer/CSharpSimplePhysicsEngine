using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CSharpSimplePhysicsEngine
{
    public class PhysicsWorld
    {
        public List<PhysicsObject> Objects;
        public PhysicsObject Floor;
        public Vector2 Gravity;
        public int GroundHeight;
        public int ScreenWidth;

        public int Iterations = 15;
        public int SubSteps = 3;

        private struct CollisionManifold
        {
            public PhysicsObject A;
            public PhysicsObject B;
            public Vector2 Normal;
            public float Depth;
            public Vector2 Contact1;
        }

        public PhysicsWorld(int screenWidth, int groundHeight)
        {
            Objects = new List<PhysicsObject>();
            Gravity = new Vector2(0, 980f);
            ScreenWidth = screenWidth;
            GroundHeight = groundHeight;

            // Floor is a static rectangle
            Vector2 floorSize = new Vector2(screenWidth * 10, 1000);
            Vector2 floorPos = new Vector2(screenWidth / 2, groundHeight + (floorSize.Y / 2));
            Floor = new PhysicsObject(floorPos, floorSize, null, 0f);
            Floor.UpdateVertices();
        }

        public void AddObject(PhysicsObject obj) => Objects.Add(obj);
        public void Clear() => Objects.Clear();

        public void Update(float deltaTime)
        {
            float subDelta = deltaTime / SubSteps;

            for (int s = 0; s < SubSteps; s++)
            {
                foreach (var obj in Objects)
                {
                    if (obj.InverseMass == 0) continue;

                    obj.Velocity += Gravity * subDelta;
                    obj.AngularVelocity *= obj.AngularDamping;

                    obj.Position += obj.Velocity * subDelta;
                    obj.Angle += obj.AngularVelocity * subDelta;

                    obj.UpdateVertices();


                    if (CheckCollision(obj, Floor, out CollisionManifold m))
                    {
                        ResolveCollision(m);
                    }

                    // Screen Bounds
                    if (obj.Position.X < 0) { obj.Position.X = 0; obj.Velocity.X *= -0.5f; }
                    if (obj.Position.X > 800) { obj.Position.X = 800; obj.Velocity.X *= -0.5f; }
                }

               
                for (int k = 0; k < Iterations; k++)
                {
                    for (int i = 0; i < Objects.Count; i++)
                    {
                        for (int j = i + 1; j < Objects.Count; j++)
                        {
                            if (CheckCollision(Objects[i], Objects[j], out CollisionManifold m))
                            {
                                ResolveCollision(m);
                            }
                        }
                    }
                }
            }
        }

        private bool CheckCollision(PhysicsObject A, PhysicsObject B, out CollisionManifold manifold)
        {
            // Default init
            manifold = new CollisionManifold { A = A, B = B };

            if (A.ShapeType == PhysicsObject.ObjectType.Circle && B.ShapeType == PhysicsObject.ObjectType.Circle)
            {
                return IntersectCircles(A, B, out manifold);
            }
            else if (A.ShapeType == PhysicsObject.ObjectType.Rectangle && B.ShapeType == PhysicsObject.ObjectType.Rectangle)
            {
                return IntersectPolygons(A, B, out manifold);
            }
            else if (A.ShapeType == PhysicsObject.ObjectType.Rectangle && B.ShapeType == PhysicsObject.ObjectType.Circle)
            {
                // Box vs Circle
                return IntersectCirclePolygon(B, A, out manifold);
            }
            else if (A.ShapeType == PhysicsObject.ObjectType.Circle && B.ShapeType == PhysicsObject.ObjectType.Rectangle)
            {
                // Circle vs Box
                return IntersectCirclePolygon(A, B, out manifold);
            }

            return false;
        }

        private bool IntersectCircles(PhysicsObject A, PhysicsObject B, out CollisionManifold m)
        {
            m = new CollisionManifold { A = A, B = B };

            Vector2 diff = B.Position - A.Position;
            float distSq = diff.LengthSquared();
            float radiusSum = A.Radius + B.Radius;

            if (distSq >= radiusSum * radiusSum) return false;

            float dist = (float)Math.Sqrt(distSq);

            Vector2 normal;
            if (dist == 0.0f) normal = Vector2.UnitY;
            else normal = diff / dist;

            m.Normal = normal;
            m.Depth = radiusSum - dist;
            m.Contact1 = A.Position + (normal * A.Radius);

            return true;
        }

        private bool IntersectPolygons(PhysicsObject A, PhysicsObject B, out CollisionManifold manifold)
        {
            manifold = new CollisionManifold { A = A, B = B };
            Vector2 normal = Vector2.Zero;
            float minDepth = float.MaxValue;
            int bestAxisIndex = -1;

            Vector2[] axes = new Vector2[4];
            axes[0] = Vector2.Normalize(A.Vertices[1] - A.Vertices[0]);
            axes[1] = Vector2.Normalize(A.Vertices[2] - A.Vertices[1]);
            axes[2] = Vector2.Normalize(B.Vertices[1] - B.Vertices[0]);
            axes[3] = Vector2.Normalize(B.Vertices[2] - B.Vertices[1]);

            for (int i = 0; i < 4; i++)
            {
                ProjectVertices(A.Vertices, axes[i], out float minA, out float maxA);
                ProjectVertices(B.Vertices, axes[i], out float minB, out float maxB);

                if (minA >= maxB || minB >= maxA) return false;

                float axisDepth = Math.Min(maxB - minA, maxA - minB);
                if (axisDepth < minDepth)
                {
                    minDepth = axisDepth;
                    normal = axes[i];
                    bestAxisIndex = i;
                }
            }

            if (Vector2.Dot(B.Position - A.Position, normal) < 0) normal = -normal;

            manifold.Normal = normal;
            manifold.Depth = minDepth;

            PhysicsObject incidentObj = (bestAxisIndex < 2) ? B : A;
            Vector2 searchDirection = (bestAxisIndex < 2) ? -normal : normal;
            Vector2 bestVertex = incidentObj.Vertices[0];
            float maxDot = Vector2.Dot(bestVertex, searchDirection);

            for (int i = 1; i < 4; i++)
            {
                float dot = Vector2.Dot(incidentObj.Vertices[i], searchDirection);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    bestVertex = incidentObj.Vertices[i];
                }
            }
            manifold.Contact1 = bestVertex;

            return true;
        }

        private bool IntersectCirclePolygon(PhysicsObject circle, PhysicsObject poly, out CollisionManifold m)
        {
            m = new CollisionManifold { A = circle, B = poly };


            Vector2 center = circle.Position;
            Vector2 relCenter = center - poly.Position;

            float ang = -poly.Angle;
            float s = (float)Math.Sin(ang);
            float c = (float)Math.Cos(ang);

            // Manual rotation matrix
            Vector2 localCenter = new Vector2(
                relCenter.X * c - relCenter.Y * s,
                relCenter.X * s + relCenter.Y * c
            );

            Vector2 halfSize = poly.Size / 2f;
            Vector2 closestLocal = Vector2.Clamp(localCenter, -halfSize, halfSize);

            Vector2 distanceVec = localCenter - closestLocal;
            float distSq = distanceVec.LengthSquared();
            float r = circle.Radius;

            if (distSq > r * r) return false;

            float dist = (float)Math.Sqrt(distSq);
            Vector2 normalLocal;

            if (dist < 0.0001f)
            {
                float dX = halfSize.X - Math.Abs(localCenter.X);
                float dY = halfSize.Y - Math.Abs(localCenter.Y);

                if (dX < dY)
                {
                    m.Depth = dX + r;
                    normalLocal = localCenter.X < 0 ? new Vector2(-1, 0) : new Vector2(1, 0);
                    closestLocal.X = localCenter.X < 0 ? -halfSize.X : halfSize.X;
                }
                else
                {
                    m.Depth = dY + r;
                    normalLocal = localCenter.Y < 0 ? new Vector2(0, -1) : new Vector2(0, 1);
                    closestLocal.Y = localCenter.Y < 0 ? -halfSize.Y : halfSize.Y;
                }
            }
            else
            {
                m.Depth = r - dist;
                normalLocal = distanceVec / dist; 
            }


            ang = poly.Angle;
            s = (float)Math.Sin(ang);
            c = (float)Math.Cos(ang);

            Vector2 normalWorld = new Vector2(
                normalLocal.X * c - normalLocal.Y * s,
                normalLocal.X * s + normalLocal.Y * c
            );

            Vector2 contactLocalRotated = new Vector2(
                closestLocal.X * c - closestLocal.Y * s,
                closestLocal.X * s + closestLocal.Y * c
            );

            m.Normal = -normalWorld; 
            m.Contact1 = poly.Position + contactLocalRotated;

            return true;
        }

  
        private void ProjectVertices(Vector2[] vertices, Vector2 axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                float proj = Vector2.Dot(vertices[i], axis);
                if (proj < min) min = proj;
                if (proj > max) max = proj;
            }
        }

        private void ResolveCollision(CollisionManifold m)
        {
            PhysicsObject A = m.A;
            PhysicsObject B = m.B;
            Vector2 normal = m.Normal;
            Vector2 contact = m.Contact1;

            const float percent = 0.8f;
            const float slop = 0.01f;
            Vector2 correction = Math.Max(m.Depth - slop, 0.0f) / (A.InverseMass + B.InverseMass) * percent * normal;

            if (A.InverseMass > 0) A.Position -= A.InverseMass * correction;
            if (B.InverseMass > 0) B.Position += B.InverseMass * correction;

            A.UpdateVertices();
            B.UpdateVertices();

            Vector2 rA = contact - A.Position;
            Vector2 rB = contact - B.Position;

            Vector2 rv = B.Velocity + new Vector2(-B.AngularVelocity * rB.Y, B.AngularVelocity * rB.X) -
                         (A.Velocity + new Vector2(-A.AngularVelocity * rA.Y, A.AngularVelocity * rA.X));

            float velAlongNormal = Vector2.Dot(rv, normal);

            if (velAlongNormal > 0) return;


            float raCrossN = rA.X * normal.Y - rA.Y * normal.X;
            float rbCrossN = rB.X * normal.Y - rB.Y * normal.X;
            float invMassSum = A.InverseMass + B.InverseMass +
                               (raCrossN * raCrossN) * A.InverseInertia +
                               (rbCrossN * rbCrossN) * B.InverseInertia;

            float e = Math.Min(A.Restitution, B.Restitution);
            if (velAlongNormal > -120.0f) e = 0.0f; 

            float j = -(1 + e) * velAlongNormal;
            j /= invMassSum;

            Vector2 impulse = j * normal;


            if (A.InverseMass > 0)
            {
                A.Velocity -= A.InverseMass * impulse;
                A.AngularVelocity -= A.InverseInertia * (rA.X * impulse.Y - rA.Y * impulse.X);
            }
            if (B.InverseMass > 0)
            {
                B.Velocity += B.InverseMass * impulse;
                B.AngularVelocity += B.InverseInertia * (rB.X * impulse.Y - rB.Y * impulse.X);
            }


            rv = B.Velocity + new Vector2(-B.AngularVelocity * rB.Y, B.AngularVelocity * rB.X) -
                 (A.Velocity + new Vector2(-A.AngularVelocity * rA.Y, A.AngularVelocity * rA.X));

            Vector2 tangent = rv - (Vector2.Dot(rv, normal) * normal);

            if (tangent.LengthSquared() > 0.0001f)
            {
                tangent.Normalize();

                float raCrossT = rA.X * tangent.Y - rA.Y * tangent.X;
                float rbCrossT = rB.X * tangent.Y - rB.Y * tangent.X;
                float invMassSumT = A.InverseMass + B.InverseMass +
                                    (raCrossT * raCrossT) * A.InverseInertia +
                                    (rbCrossT * rbCrossT) * B.InverseInertia;

                float jt = -Vector2.Dot(rv, tangent);
                jt /= invMassSumT; 

                float mu = (float)Math.Sqrt(A.Friction * A.Friction + B.Friction * B.Friction);

                Vector2 frictionImpulse;
                if (Math.Abs(jt) < j * mu)
                    frictionImpulse = jt * tangent; 
                else
                    frictionImpulse = -j * mu * tangent; 

                if (A.InverseMass > 0)
                {
                    A.Velocity -= A.InverseMass * frictionImpulse;
                    A.AngularVelocity -= A.InverseInertia * (rA.X * frictionImpulse.Y - rA.Y * frictionImpulse.X);
                }
                if (B.InverseMass > 0)
                {
                    B.Velocity += B.InverseMass * frictionImpulse;
                    B.AngularVelocity += B.InverseInertia * (rB.X * frictionImpulse.Y - rB.Y * frictionImpulse.X);
                }
            }
        }
    }
}