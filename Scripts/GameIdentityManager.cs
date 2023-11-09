using Microsoft.Xna.Framework.Graphics;
using MonoTerrain.Scripts.Gameplay;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;

namespace MonoTerrain.Scripts {
    public class GameIdentityManager {

        private Viewport viewport;
        private Vector2 positionOffset;

        public int CreatedIdentities { get; set; }
        private Dictionary<int, GameIdentity> ActiveGameIdentities { get; set; }
        public static GameIdentityManager Instance;

        public GameIdentityManager() {
            ActiveGameIdentities = new Dictionary<int, GameIdentity>();
            viewport = GameController.Instance.Viewport;
            positionOffset = new Vector2(viewport.Width / 2f, viewport.Height / 2f); 
            Instance = this;
        }

        public void InstantiateIdentity(GameIdentity gameIdentity, Vector2 position, bool skipSelfCheck = false) {
            if (skipSelfCheck) {
                gameIdentity.Transform.position = position;
                ActiveGameIdentities.Add(gameIdentity.IdentityId, gameIdentity);
                UpdateGameIdentitiesOrder(gameIdentity);
                return;
            }
            if (!ActiveGameIdentities.ContainsKey(gameIdentity.IdentityId)) {
                gameIdentity.Transform.position = position;
                ActiveGameIdentities.Add(gameIdentity.IdentityId, gameIdentity);
                UpdateGameIdentitiesOrder(gameIdentity);
            }
            else {
                string message = $"GameIdentity {gameIdentity.Name}[{gameIdentity.IdentityId}] is already instantiated";
                GameHelper.ExitWithDebugMessage(message);
            }
        }
        
        public void DestroyIdentity(GameIdentity gameIdentity, bool skipSelfCheck = false) 
            => DestroyIdentity(gameIdentity.IdentityId, skipSelfCheck);
       
        public void DestroyIdentity(int identityId, bool skipSelfCheck = false) {
            if (skipSelfCheck) {
                ActiveGameIdentities.Remove(identityId);
                return;
            }

            if (ActiveGameIdentities.ContainsKey(identityId)) {
                ActiveGameIdentities.Remove(identityId);
                //UpdateGameIdentitiesOrder(ActiveGameIdentities[identityId]);
            }
            else {
                GameIdentity identity = ActiveGameIdentities[identityId];
                string message = $"GameIdentity {identity.Name}[{identity.IdentityId}] cannot be destroyed because it does not exist";
                GameHelper.ExitWithDebugMessage(message);
            }
        }

        private void UpdateGameIdentitiesOrder(GameIdentity gameIdentity) { //TODO: too expensive, needs rework
            if (gameIdentity.RenderOrder == -1) return;
            
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

                int l = identity.Children.Count;
                for (int i = 0; i < l; i++) {
                    if (!identity.Children[i].Active) continue;
                    DrawIdentity(spriteBatch, identity.Children[i]);
                }
            }
            spriteBatch.End();
        }

        private void DrawIdentity(SpriteBatch batch, GameIdentity gameIdentity) {
            Vector2 position = new Vector2(gameIdentity.Transform.position.X, -gameIdentity.Transform.position.Y);
            batch.Draw(gameIdentity.Visual.targetTexture, position + positionOffset, null, 
            gameIdentity.Visual.textureColor, gameIdentity.Transform.rotation, gameIdentity.Transform.originOffset,
            gameIdentity.Transform.scale, SpriteEffects.None, 0);
        }
    }
}
