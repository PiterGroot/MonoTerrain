using static MonoTerrain.Scripts.GameHelper;
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
                if (TerrainGenerator.tiles.ContainsKey(mouseGridPosition)) {
                    TerrainGenerator.tiles[mouseGridPosition].Active = false;
                }
            }
        }
    }
}
