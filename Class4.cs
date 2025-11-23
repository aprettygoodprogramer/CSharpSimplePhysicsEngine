using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace CSharpSimplePhysicsEngine
{
    public static class SimpleUI
    {
        public static Texture2D Pixel;
        public static SpriteFont Font;
        private static MouseState _currentMouse;
        private static MouseState _prevMouse;

        public static void Load(GraphicsDevice graphics, SpriteFont font)
        {
            Pixel = new Texture2D(graphics, 1, 1);
            Pixel.SetData(new Color[] { Color.White });
            Font = font;
        }

        public static void Begin()
        {
            _prevMouse = _currentMouse;
            _currentMouse = Mouse.GetState();
        }

        public static bool Button(Rectangle rect, string text)
        {
            bool hover = rect.Contains(_currentMouse.Position);
            bool clicked = hover && _currentMouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;

            Color color = hover ? Color.Gray : Color.DarkGray;
            if (clicked) color = Color.White;

            DrawFilledRect(rect, color);
            DrawBorder(rect, 1, Color.Black);

            if (Font != null)
            {
                Vector2 size = Font.MeasureString(text);
                Vector2 pos = new Vector2(rect.X + (rect.Width - size.X) / 2, rect.Y + (rect.Height - size.Y) / 2);
                Game1.SpriteBatchInstance.DrawString(Font, text, pos, Color.White);
            }
            return clicked;
        }

        public static float Slider(Rectangle rect, string label, float value, float min, float max)
        {
            DrawFilledRect(rect, Color.Black * 0.5f);
            DrawBorder(rect, 1, Color.White * 0.5f);

            float range = max - min;
            float percent = (value - min) / range;
            int knobWidth = 10;
            int availWidth = rect.Width - knobWidth;
            int knobX = (int)(rect.X + (availWidth * percent));

            Rectangle knobRect = new Rectangle(knobX, rect.Y, knobWidth, rect.Height);

            bool hover = rect.Contains(_currentMouse.Position);
            bool down = _currentMouse.LeftButton == ButtonState.Pressed;

            if (hover && down)
            {
                float mouseRelative = _currentMouse.X - rect.X - (knobWidth / 2);
                float newPercent = MathHelper.Clamp(mouseRelative / availWidth, 0, 1);
                value = min + (newPercent * range);
            }

            DrawFilledRect(knobRect, down ? Color.Yellow : (hover ? Color.LightGray : Color.White));

            if (Font != null)
            {
                string valueText = value < 10 ? value.ToString("0.00") : value.ToString("0");
                string fullText = $"{label}: {valueText}";
                Game1.SpriteBatchInstance.DrawString(Font, fullText, new Vector2(rect.X, rect.Y - 20), Color.White);
            }
            return value;
        }

        public static void DrawFilledRect(Rectangle rect, Color color)
        {
            if (Pixel != null) Game1.SpriteBatchInstance.Draw(Pixel, rect, color);
        }

        public static void DrawBorder(Rectangle rect, int thickness, Color color)
        {
            DrawFilledRect(new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            DrawFilledRect(new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            DrawFilledRect(new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            DrawFilledRect(new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }
    }
}