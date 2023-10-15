using static MonoTerrain.Scripts.GameHelper;
using MonoGame.Extended.ViewportAdapters;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoTerrain.Scripts.Gameplay {
    
    public class CameraController {
        private int currentMouseWheelValue;

        private float startZoomValue = .7f;
        private int zoomSpeed = 3;

        public float MovementSpeed = 5;
        public OrthographicCamera Camera { get; private set; }

        public static CameraController Instance;
        public CameraController(GameWindow window, GraphicsDevice graphicsDevice, Viewport viewport) {
            var viewportAdapter = new BoxingViewportAdapter(window, graphicsDevice, viewport.Width, viewport.Height);
            Camera = new OrthographicCamera(viewportAdapter);

            Camera.MinimumZoom = 0.3f;
            Camera.MaximumZoom = 50;
            Camera.Zoom = startZoomValue;

            Instance = this;
        }

        public void UpdateCamera(GameTime gameTime) {
            KeyboardState keyboardState = GameController.Instance.KeyboardState;
            MouseState mouseState = GameController.Instance.MouseState;
            
            float currentMovementSpeed = keyboardState.IsKeyDown(Keys.LeftShift) ? MovementSpeed * 100 * 2 : MovementSpeed * 100;
            int previousMouseWheelValue = currentMouseWheelValue;
            currentMouseWheelValue = mouseState.ScrollWheelValue;

            if (currentMouseWheelValue > previousMouseWheelValue) {
                Camera.ZoomIn(1 / 12f * Camera.Zoom * zoomSpeed);
            }
            if (currentMouseWheelValue < previousMouseWheelValue) {
                Camera.ZoomOut(1 / 12f * Camera.Zoom * zoomSpeed);
            }
            Camera.Move(GetMovementDirection(keyboardState) * currentMovementSpeed * gameTime.GetElapsedSeconds());
        }

        private Vector2 GetMovementDirection(KeyboardState keyboardState) {
            var movementDirection = Vector2.Zero;
            if (keyboardState.IsKeyDown(Keys.S)) {
                movementDirection += Vector2.UnitY;
            }
            if (keyboardState.IsKeyDown(Keys.W)) {
                movementDirection -= Vector2.UnitY;
            }
            if (keyboardState.IsKeyDown(Keys.A)) {
                movementDirection -= Vector2.UnitX;
            }
            if (keyboardState.IsKeyDown(Keys.D)) {
                movementDirection += Vector2.UnitX;
            }

            if (movementDirection != Vector2.Zero)
                movementDirection.Normalize();

            return movementDirection;
        }

    }
}
