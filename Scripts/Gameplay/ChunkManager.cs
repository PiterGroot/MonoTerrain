using static MonoTerrain.Scripts.GameHelper;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace MonoTerrain.Scripts.Gameplay {
    public class ChunkManager {

        private int chunkClampValue;
        public static int ChunkCounter { get; private set; }
        public static List<GameIdentity> chunkContainers = new List<GameIdentity>();
        public static Dictionary<int, List<TerrainGenerator.Tile>> chunks = new Dictionary<int, List<TerrainGenerator.Tile>>();

        public TerrainGenerator terrainGenerator;

        public ChunkManager(TerrainGenerator terrainGenerator) { 
            this.terrainGenerator = terrainGenerator;
            CameraController.Instance.onMovePosition += HandleMoveCamera;
            terrainGenerator.onTerrainGenerated += HandleTerrainGenerated;
        }

        public void CreateChunkContainer() {
            float xPos = (ChunkCounter * TerrainGenerator.chunkSize) + TerrainGenerator.chunkSize * .5f;
            float yPos = terrainGenerator.height * .5f;

            ChunkCounter++;
            chunks.Add(ChunkCounter, new List<TerrainGenerator.Tile>());
            GameIdentity chunk = new GameIdentity($"Chunk {ChunkCounter}");

            Vector2 chunkPosition = TerrainGenerator.GetGridTilePosition(xPos, 0, 16 * 1);
            chunkPosition.Y = -yPos;

            chunk.Transform.position = chunkPosition;
            chunkContainers.Add(chunk);
        }

        private void HandleTerrainGenerated() {
            chunkClampValue = chunkContainers.Count - 1;
            ToggleChunks(GetNearestChunkIndex(CameraController.Instance.Camera.Position));
        }

        private void HandleMoveCamera(Vector2 newPosition) {
            ToggleChunks(GetNearestChunkIndex(CameraController.Instance.Camera.Position));
        }

        private void ToggleChunks(int centerChunk) {
            int center = centerChunk;
            int east = Math.Clamp(centerChunk + 1, 0, chunkClampValue);
            int west = Math.Clamp(centerChunk - 1, 0, chunkClampValue);

            chunkContainers[center].Active = true;
            chunkContainers[east].Active = true;
            chunkContainers[west].Active = true;
            
            int l = chunkContainers.Count;
            for (int i = 0; i < l; i++) {
                if (i == center || i == east || i == west) continue;
                chunkContainers[i].Active = false;
            }
        }

        private int GetNearestChunkIndex(Vector2 originPosition) {
            int closestChunkIndex = -1;
            float closestDistanceSqr = float.PositiveInfinity;
            Vector2 currentPosition = originPosition;

            for (int i = 0; i < chunkContainers.Count; i++) {
                GameIdentity chunkContainer = chunkContainers[i];
                Vector2 directionToTarget = chunkContainer.Transform.position - currentPosition;
                float dSqrToTarget = directionToTarget.LengthSquared();
                if (dSqrToTarget < closestDistanceSqr) {
                    closestDistanceSqr = dSqrToTarget;
                    closestChunkIndex = i;
                }
            }

            return closestChunkIndex;
        }
    }
}
