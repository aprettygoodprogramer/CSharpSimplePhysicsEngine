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
        private float scaleFactor;
        private Vector2 currentPositition;
        private Object _object;
        private List<RectangleObject> rectangleObjects;
        private RectangleObject RectangleTest;
        private Vector2 Size;
        private Microsoft.Xna.Framework.Vector2 Position;
        private MouseState _previousMouseState;
        private Vector2 MousePos;
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
            Size.X = 100;
            Size.Y = 100;
            Position.X = 200;
            Position.Y = 200;
            // Initialize any logic-related variables
            currentPositition = new Vector2(100, 100); // Example initial position
            scaleFactor = 1.0f; // Example scale factor
            base.Initialize();
            RectangleTest = new RectangleObject(Position, ballTexture);
            rectangleObjects.Add(RectangleTest);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the texture for the ball
            ballTexture = Content.Load<Texture2D>("recktangle");

            // Ensure currentPositition is properly initialized
            currentPositition = new Vector2(100, 100);

            // Initialize the Object instance
            _object = new Object(currentPositition, ballTexture);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState currentMouseState = Mouse.GetState();

            if (currentMouseState.LeftButton == ButtonState.Pressed &&
                _previousMouseState.LeftButton == ButtonState.Released)
            {
                MousePos.X = Mouse.GetState().X;
                MousePos.Y = Mouse.GetState().Y;

                RectangleTest = new RectangleObject(MousePos, ballTexture);
                rectangleObjects.Add(RectangleTest);

            }

            _previousMouseState = currentMouseState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            //_spriteBatch.Draw(ballTexture, new Rectangle(200, 100, 50, 100), Color.White);
            foreach (Object obj in rectangleObjects)
            {
                _spriteBatch.Draw(ballTexture, new Rectangle((int)obj.position.X, (int)obj.position.Y, 50, 100), Color.White);
            }


            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
