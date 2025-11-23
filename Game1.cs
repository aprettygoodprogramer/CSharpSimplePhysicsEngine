using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace CSharpSimplePhysicsEngine
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        public static SpriteBatch SpriteBatchInstance;

        private Texture2D ballTexture;
        private SpriteFont uiFont;
        private PhysicsWorld world;
        private MouseState _prevMouse;

        private float spawnMass = 5.0f;
        private float spawnSize = 50.0f;
        private bool showUI = true;
        private bool isPaused = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
        }

        protected override void Initialize()
        {
            base.Initialize();
            world = new PhysicsWorld(_graphics.PreferredBackBufferWidth, 650);
        }

        protected override void LoadContent()
        {
            SpriteBatchInstance = new SpriteBatch(GraphicsDevice);

            try { ballTexture = Content.Load<Texture2D>("recktangle"); }
            catch
            {
                ballTexture = new Texture2D(GraphicsDevice, 1, 1);
                ballTexture.SetData(new Color[] { Color.White });
            }

            try
            {
                uiFont = Content.Load<SpriteFont>("File");
                SimpleUI.Load(GraphicsDevice, uiFont);
            }
            catch
            {
                SimpleUI.Load(GraphicsDevice, null); 
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            MouseState mouse = Mouse.GetState();

            SimpleUI.Begin();

            if (!isPaused)
            {
                world.Update(deltaTime);
            }

            if (mouse.LeftButton == ButtonState.Pressed &&
                _prevMouse.LeftButton == ButtonState.Released &&
                mouse.X < 800)
            {
                Vector2 pos = new Vector2(mouse.X, mouse.Y);
                Vector2 size = new Vector2(spawnSize, spawnSize * 2);
                var obj = new PhysicsObject(pos, size, ballTexture, spawnMass);
                world.AddObject(obj);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.H) && _prevKey.IsKeyUp(Keys.H)) showUI = !showUI;

            _prevMouse = mouse;
            _prevKey = Keyboard.GetState();
            base.Update(gameTime);
        }
        private KeyboardState _prevKey;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(30, 30, 30));

            SpriteBatchInstance.Begin();

            SimpleUI.DrawFilledRect(new Rectangle(0, world.GroundHeight, world.ScreenWidth, 5), Color.Orange);

            foreach (var obj in world.Objects)
            {
                Vector2 origin = new Vector2(obj.Texture.Width / 2f, obj.Texture.Height / 2f);
                Vector2 scale = obj.Size / new Vector2(obj.Texture.Width, obj.Texture.Height);

                SpriteBatchInstance.Draw(
                    obj.Texture,
                    obj.Position,
                    null,
                    obj.Color,
                    obj.Angle,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }

            if (showUI) DrawMenu();

            SpriteBatchInstance.End();
            base.Draw(gameTime);
        }

        private void DrawMenu()
        {
            int panelX = 800;
            int width = 224;
            int height = _graphics.PreferredBackBufferHeight;

            SimpleUI.DrawFilledRect(new Rectangle(panelX, 0, width, height), new Color(50, 50, 50));
            SimpleUI.DrawBorder(new Rectangle(panelX, 0, width, height), 2, Color.Black);

            int startY = 50;
            int gap = 60;

            world.Gravity.Y = SimpleUI.Slider(new Rectangle(panelX + 10, startY, 200, 20), "Gravity Y", world.Gravity.Y, 0, 3000);
            startY += gap;

            spawnMass = SimpleUI.Slider(new Rectangle(panelX + 10, startY, 200, 20), "Spawn Mass", spawnMass, 1, 100);
            startY += gap;

            spawnSize = SimpleUI.Slider(new Rectangle(panelX + 10, startY, 200, 20), "Spawn Size", spawnSize, 20, 150);
            startY += gap;

            float iter = SimpleUI.Slider(new Rectangle(panelX + 10, startY, 200, 20), "Accuracy (Iter)", world.Iterations, 1, 30);
            world.Iterations = (int)iter;
            startY += gap;

            float subs = SimpleUI.Slider(new Rectangle(panelX + 10, startY, 200, 20), "Sub-Steps", world.SubSteps, 1, 20);
            world.SubSteps = (int)subs;
            startY += gap;

            startY += 20;
            if (SimpleUI.Button(new Rectangle(panelX + 10, startY, 200, 40), "Clear Objects"))
            {
                world.Clear();
            }
            startY += 50;

            string pauseText = isPaused ? "RESUME" : "PAUSE";
            if (SimpleUI.Button(new Rectangle(panelX + 10, startY, 200, 40), pauseText))
            {
                isPaused = !isPaused;
            }

            if (uiFont != null)
                SpriteBatchInstance.DrawString(uiFont, $"Objects: {world.Objects.Count}", new Vector2(panelX + 10, height - 50), Color.Gray);
        }
    }
}