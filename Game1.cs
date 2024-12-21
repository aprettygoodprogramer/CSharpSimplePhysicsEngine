using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CSharpSimplePhysicsEngine
{
    public class Game1 : Game
    {
        Texture2D ballTexture;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private List<RectangleObject> rectangleObjects;
        private MouseState _previousMouseState;
        private Vector2 gravity = new Vector2(0, 980f);
        public Game1()
        {
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            rectangleObjects = new List<RectangleObject>();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            ballTexture = Content.Load<Texture2D>("recktangle");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState currentMouseState = Mouse.GetState();

            if (currentMouseState.LeftButton == ButtonState.Pressed &&
                _previousMouseState.LeftButton == ButtonState.Released)
            {
                Vector2 mousePos = new Vector2(currentMouseState.X, currentMouseState.Y);

                Rectangle newRectangle = new Rectangle((int)mousePos.X, (int)mousePos.Y, 50, 100);

                bool overlaps = false;
                foreach (var obj in rectangleObjects)
                {

                    if (newRectangle.Intersects(obj.BoundingBox))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    rectangleObjects.Add(new RectangleObject(mousePos, ballTexture));
                }
            }

            foreach (var obj in rectangleObjects)
            {
                obj.Vel += gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                obj.Position += obj.Vel * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (obj.Position.Y + obj.Size.Y > 500)
                {
                    obj.Position = new Vector2(obj.Position.X, 500 - obj.Size.Y);
                    obj.Vel = new Vector2(obj.Vel.X, 0);
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            foreach (var obj in rectangleObjects)
            {
                _spriteBatch.Draw(obj.Texture, obj.BoundingBox, Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}