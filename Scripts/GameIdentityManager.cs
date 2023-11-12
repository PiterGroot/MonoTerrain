using Microsoft.Xna.Framework.Graphics;
using MonoTerrain.Scripts.Gameplay;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoTerrain.Scripts {
    public class GameIdentityManager {

        private Viewport viewport;
        private Vector2 positionOffset;

        public int CreatedIdentities { get; set; }

        public List<GameIdentity> ignoreViewMatrixIdentities = new List<GameIdentity>();
        private Dictionary<int, GameIdentity> ActiveGameIdentities { get; set; }

        public static GameIdentityManager Instance;

        public GameIdentityManager() {
            ActiveGameIdentities = new Dictionary<int, GameIdentity>();
            viewport = GameController.Instance.Viewport;
            positionOffset = new Vector2(viewport.Width / 2f, viewport.Height / 2f); 
            Instance = this;
        }

        public void InstantiateIdentity(GameIdentity gameIdentity, Vector2 position) {
            gameIdentity.Transform.position = position;
            ActiveGameIdentities.Add(gameIdentity.IdentityId, gameIdentity);
        }

        public void DestroyChildIdentity(int parentId, GameIdentity childIdentity) {
            ActiveGameIdentities[parentId].Children.Remove(childIdentity);
            CreatedIdentities--;
        }

        public void DestroyIdentity(GameIdentity gameIdentity) 
            => DestroyIdentity(gameIdentity.IdentityId);
       
        public void DestroyIdentity(int identityId) {
            ActiveGameIdentities.Remove(identityId);
            CreatedIdentities--;
        }

        public void DrawGameIdentities(SpriteBatch spriteBatch, GraphicsDevice device) {
            device.Clear(Color.CornflowerBlue);

            //draw identities with applied camera maxtrix
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
            
            //draw identities without applied camera matrix
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            foreach (GameIdentity identity in ignoreViewMatrixIdentities) {
                if (!identity.Active) continue;
                DrawIdentity(spriteBatch, identity);
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
