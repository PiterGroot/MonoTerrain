using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace MonoTerrain.Scripts.Gameplay {
    public class WorldInteractor {
        public WorldInteractor(GameController gameController) {
            gameController.OnUpdate += OnUpdate;
        }

        private void OnUpdate(GameTime gameTime) {
            if (GameController.Instance.MouseState.LeftButton == ButtonState.Pressed) {
                Vector2 mouseGridPosition = TerrainGenerator.GetGridMousePosition();

                int currentChunk = ChunkManager.instance.CurrentChunk;
                GameIdentity chunkContainer = ChunkManager.instance.chunkContainers[currentChunk];

                if (chunkContainer.GetChildByPosition(mouseGridPosition, out GameIdentity tile)) {
                    GameIdentityManager.Instance.DestroyChildIdentity(chunkContainer.IdentityId, tile);
                }
            }
        }
    }
}
