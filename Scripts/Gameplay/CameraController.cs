﻿using static MonoTerrain.Scripts.GameHelper;
using MonoGame.Extended.ViewportAdapters;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tweening;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;

namespace MonoTerrain.Scripts.Gameplay {
    
    public class CameraController {
        private readonly Tweener teleportTween = new Tweener();

        private float startZoomValue = 1f;

        private Vector2 moveDirection;
        private float movementLerpSpeed = 10;
        public float MovementSpeed = 10;

        public OrthographicCamera Camera { get; private set; }
        public Action<Vector2> onMovePosition;

        public static CameraController Instance;
        public CameraController(GameWindow window, GraphicsDevice graphicsDevice, Viewport viewport) {
            var viewportAdapter = new BoxingViewportAdapter(window, graphicsDevice, viewport.Width, viewport.Height);
            Camera = new OrthographicCamera(viewportAdapter);

            Camera.Zoom = startZoomValue;

            Instance = this;
        }

        public void UpdateCamera(GameTime gameTime) {
            KeyboardState keyboardState = GameController.Instance.KeyboardState;
            MouseState mouseState = GameController.Instance.MouseState;

            float currentMovementSpeed = keyboardState.IsKeyDown(Keys.LeftControl) ? MovementSpeed * 100 * 2 : MovementSpeed * 100;

            Vector2 rawDirection = GetMovementDirection(keyboardState);

            moveDirection = Vector2.Lerp(moveDirection, rawDirection, movementLerpSpeed * gameTime.GetElapsedSeconds());
            Camera.Position += moveDirection * currentMovementSpeed * gameTime.GetElapsedSeconds();
            
            if(rawDirection != Vector2.Zero)
                onMovePosition?.Invoke(Camera.Position);

            teleportTween.Update(gameTime.GetElapsedSeconds());
        }

        public void TeleportTo(Vector2 teleportPosition, bool instant = false) {
            if(instant)
                Camera.Position = teleportPosition;
            else {
                teleportTween.CancelAndCompleteAll();
                teleportTween.TweenTo(Camera, transform => transform.Position, teleportPosition, .35f, 0)
                    .Easing(EasingFunctions.CircleInOut);
            }
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

        public static Vector2 GetCameraPosition() {
            Vector2 position = Instance.Camera.Position;
            position.Y *= -1;

            return position;
        }
    }
}
