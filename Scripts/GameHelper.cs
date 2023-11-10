using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using MonoGame.Extended;
using System.Timers;
using System.Linq;
using System;

namespace MonoTerrain.Scripts
{
#pragma warning disable IDE1006 
    public static class GameHelper
    {
        public static SpriteBatch spriteBatch { get; set; }
        public static GraphicsDevice GraphicsDevice { get; set; }

        public static Vector2 CachedCenterPoint { get; private set; }

        private static Viewport viewport;
        private static GameController gameController;

        public static GameController GameController
        {
            get => gameController;
            set
            {
                if (value == null) return;
                
                gameController = value;
                GameController.OnUpdate += OnUpdate;
                TextDrawer.defaultSpriteFont = gameController.Content.Load<SpriteFont>("font");
                viewport = value.Viewport;
                CachedCenterPoint = new Vector2(viewport.Width / 2, viewport.Height / 2);
            }
        }

        /// <summary>
        /// Waits for some amount of time (seconds) and invokes onComplete callback
        /// </summary>
        public static void WaitFor(float duration, Action onWaitComplete)
        {
            Timer timer = new Timer(duration * 1000);
            timer.Start();

            timer.Elapsed += (sender, e) =>
            {
                timer.Dispose();
                onWaitComplete?.Invoke();
            };
        }

        public static void print(object message) {
            Debug.WriteLine(message.ToString());
        }

        public static void DrawStaticTexture(Texture2D texture, Vector2 position, Color color, float rotation, Vector2 scale) {
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            spriteBatch.Draw(texture, position, null, color, rotation, origin, scale, SpriteEffects.None, 0);
        }

        public static bool IsMouseInsideWindow() {
            MouseState mouseState = Mouse.GetState();
            return GraphicsDevice.Viewport.Bounds.Contains(mouseState.Position);
        }

        public static void PlaySFX(Song sfx, bool doSafetyChecks = true) {
            if (doSafetyChecks) {
                if (IsMouseInsideWindow() && GameController.IsActive) {
                    MediaPlayer.Play(sfx);
                }
            }
            else MediaPlayer.Play(sfx);
        }

        public static void ExitWithDebugMessage(object message) {
            Debug.WriteLine("");
            Debug.WriteLine($"{message}");
            Debug.WriteLine("");
            Environment.Exit(0);
        }

        private static void OnUpdate(GameTime gameTime) {
            TextDrawer.UpdateTextLabels();
            FramesHelper.Update(gameTime.GetElapsedSeconds());
        }

        public static Vector2 GetCenterPoint() {
            return new Vector2(viewport.Width / 2, viewport.Height / 2);
        }

        public static void SetMouseVisible(bool state) {
            GameController.IsMouseVisible = state;
        }

        public static class TextDrawer {
            public static SpriteFont defaultSpriteFont { get; set; }
            private static Dictionary<string, LabelData> textLabels = new Dictionary<string, LabelData>();

            public static void DrawTextLabelOnce(SpriteFont font, object message, Vector2 position) {
                spriteBatch.DrawString(font, message.ToString(), position, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
            }

            public static void DrawTextLabelOnce(SpriteFont font, object message, Vector2 position, Vector2 scale, float rotation = 0) {
                spriteBatch.DrawString(font, message.ToString(), position, Color.White, rotation, Vector2.Zero, scale, SpriteEffects.None, 0);
            }

            public static void DrawTextLabelOnce(SpriteFont font, object message, Vector2 position, Color textColor, Vector2 scale, float rotation = 0) {
                spriteBatch.DrawString(font, message.ToString(), position, textColor, rotation, Vector2.Zero, scale, SpriteEffects.None, 0);
            }

            public static void DestroyText(string labelId) {
                if (textLabels.ContainsKey(labelId)) {
                    textLabels.Remove(labelId);
                }
                else {
                    ExitWithDebugMessage($"Text label '{labelId}' is already instantiated");
                }
            }

            public static void InstantiateTextLabel(string labelId, object message, Vector2 position, Color textColor, Vector2 scale, float rotation = 0) {
                InstantiateTextLabel(defaultSpriteFont, labelId, message, position, textColor, scale, rotation);   
            }

            public static void UpdateTextLabel(string labelId, object newMessage) {
                textLabels[labelId].textMessage = newMessage;
            }

            public static void SetState(string labelId, bool show) {
                textLabels[labelId].textColor = show ? Color.White : Color.Transparent;
            }

            public static void InstantiateTextLabel(SpriteFont font, string labelId, object message, Vector2 position, Color textColor, Vector2 scale, float rotation = 0) {
                if (!textLabels.ContainsKey(labelId)) {
                    textLabels.Add(labelId, new LabelData(labelId, message, position, textColor, rotation, scale, font));
                }
                else {
                    ExitWithDebugMessage($"Text label '{labelId}:[{message}]' cannot be destroyed because it does not exist");
                }
            }

            public static void UpdateTextLabels() {
                foreach (LabelData label in textLabels.Values) {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                    DrawTextLabelOnce(label.textFont, label.textMessage,
                        label.textPosition, label.textColor, label.textScale, label.textRotation);
                    spriteBatch.End();
                }
            }

            public class LabelData {
                public string labelId;
                public object textMessage;
                public Vector2 textPosition;
                public Color textColor;
                public float textRotation;
                public Vector2 textScale;
                public SpriteFont textFont;

                public LabelData(string id, object message, Vector2 position, Color color, float rotation, Vector2 scale, SpriteFont font) {
                    labelId = id;
                    textMessage = message;
                    textPosition = position;
                    textColor = color;
                    textRotation = rotation;
                    textScale = scale;
                    textFont = font;
                }
            }
        }

        public static class RandomHandler {
            private static Random random;
            
            public static void SetSeed(int seed) => random = new Random(seed);
            
            public static void InitializeRandom() {
                random = new Random();
            }

            public static Color RandomColor() {
                int r = random.Next(0, 255);
                int g = random.Next(0, 255);
                int b = random.Next(0, 255);

                return new Color(r, g, b, 255);
            }

            public static float GetRandomFloatingNumber(float minimum, float maximum) {
                if (random == null) InitializeRandom();
                return (float)random.NextDouble() * (maximum - minimum) + minimum;
            }

            public static int GetRandomIntNumber(int minimum, int maximum) {
                if (random == null) InitializeRandom();
                return random.Next(minimum, maximum + 1);
            }
        }

        public static class InputHandler
        {
            private static bool mousePressed = false;
            private static bool mouseReleased = false;

            /// <summary>
            /// Invokes callback when left mouse is pressed
            /// </summary>
            /// <param name="onMouseDown">Invoked callback</param>
            public static void OnMouseDown(Action onMouseDown)
            {
                MouseState currentMouseState = Mouse.GetState();
                if (currentMouseState.LeftButton == ButtonState.Pressed && mousePressed)
                {
                    onMouseDown?.Invoke();
                    mousePressed = false;
                }

                if (currentMouseState.LeftButton == ButtonState.Released)
                {
                    mousePressed = true;
                }
            }

            /// <summary>
            /// Invokes callback when left mouse is released
            /// </summary>
            /// <param name="onMouseUp">Invoked callback</param>
            public static void OnMouseUp(Action onMouseUp)
            {
                MouseState currentMouseState = Mouse.GetState();
                if (currentMouseState.LeftButton == ButtonState.Pressed)
                {
                    mouseReleased = true;
                }
                if (currentMouseState.LeftButton == ButtonState.Released && mouseReleased)
                {
                    onMouseUp?.Invoke();
                    mouseReleased = false;
                }
            }
        }

        public static class FramesHelper {
            public static long TotalFrames { get; private set; }
            public static float TotalSeconds { get; private set; }
            public static float AverageFramesPerSecond { get; private set; }
            public static float CurrentFramesPerSecond { get; private set; }

            public const int MaximumSamples = 100;

            private static readonly Queue<float> sampleBuffer = new();

            public static void Update(float deltaTime) {
                CurrentFramesPerSecond = 1.0f / deltaTime;

                sampleBuffer.Enqueue(CurrentFramesPerSecond);

                if (sampleBuffer.Count > MaximumSamples) {
                    sampleBuffer.Dequeue();
                    AverageFramesPerSecond = sampleBuffer.Average(i => i);
                }
                else {
                    AverageFramesPerSecond = CurrentFramesPerSecond;
                }

                TotalFrames++;
                TotalSeconds += deltaTime;
            }
        }

        public static class Vector2Helper {
            public static Vector2 Up = new Vector2(0, 1);
            public static Vector2 Down = new Vector2(0, -1);
            public static Vector2 Right = new Vector2(1, 0);
            public static Vector2 Left = new Vector2(-1, 0);
        }
    }
#pragma warning restore IDE1006s
}
