﻿using Microsoft.Xna.Framework.Graphics;
using MonoTerrain.Scripts.Gameplay;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;

namespace MonoTerrain.Scripts {
    public class GameIdentityManager {

        private Viewport viewport;
        private Dictionary<int, GameIdentity> ActiveGameIdentities { get; set; }

        public static GameIdentityManager Instance;
        public GameIdentityManager() {
            ActiveGameIdentities = new Dictionary<int, GameIdentity>();
            viewport = GameController.Instance.Viewport;
            Instance = this;
        }

        public void InstantiateIdentity(GameIdentity gameIdentity, Vector2 position) {
            if (!ActiveGameIdentities.ContainsKey(gameIdentity.UniqueId)) {
                gameIdentity.Transform.position = position;
                ActiveGameIdentities.Add(gameIdentity.UniqueId, gameIdentity);
                UpdateGameIdentitiesOrder();
            }
            else {
                string message = $"GameIdentity {gameIdentity.Name}[{gameIdentity.UniqueId}] is already instantiated";
                GameHelper.ExitWithDebugMessage(message);
            }
        }

        public void DestroyIdentity(GameIdentity gameIdentity) {
            if (ActiveGameIdentities.ContainsKey(gameIdentity.UniqueId)) {
                ActiveGameIdentities.Remove(gameIdentity.UniqueId);
                UpdateGameIdentitiesOrder();
            }
            else {
                string message = $"GameIdentity {gameIdentity.Name}[{gameIdentity.UniqueId}] cannot be destroyed because it does not exist";
                GameHelper.ExitWithDebugMessage(message);
            }
        }

        public bool IsUniqueIdentity(int identityId) => !ActiveGameIdentities.ContainsKey(identityId);

        private void UpdateGameIdentitiesOrder() {
            List<KeyValuePair<int, GameIdentity>> identityList = ActiveGameIdentities.ToList();
            identityList.Sort((identityA, identityB) => identityA.Value.RenderOrder.CompareTo(identityB.Value.RenderOrder));

            ActiveGameIdentities = identityList.ToDictionary(key => key.Key, value => value.Value);
        }

        public void DrawGameIdentities(SpriteBatch spriteBatch, GraphicsDevice device) {
            device.Clear(Color.CornflowerBlue);
            
            Matrix cameraMatrix = CameraController.Instance.Camera.GetViewMatrix();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: cameraMatrix);

            foreach (GameIdentity identity in ActiveGameIdentities.Values) {
                if (!identity.Active) continue;
                DrawIdentity(spriteBatch, identity);
            }

            spriteBatch.End();
        }

        private void DrawIdentity(SpriteBatch batch, GameIdentity gameIdentity) {
            Vector2 positionOffset = new Vector2(viewport.Width / 2f, viewport.Height / 2f);
            Vector2 position = new Vector2(gameIdentity.Transform.position.X, -gameIdentity.Transform.position.Y);

            batch.Draw(gameIdentity.Visual.targetTexture, position + positionOffset, null, 
            gameIdentity.Visual.textureColor, gameIdentity.Transform.rotation, gameIdentity.Transform.originOffset,
            gameIdentity.Transform.scale, SpriteEffects.None, 0);
        }
    }
}
