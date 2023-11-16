using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace MonoTerrain.Scripts.Gameplay {
    public class ChunkManager {

        private int chunkClampValue;
        
        public int CurrentChunk { get; private set; }
        public int ChunkCounter { get; private set; }
        public bool autoToggleChunks = true;

        public List<GameIdentity> chunkContainers = new List<GameIdentity>();

        public TerrainGenerator terrainGenerator;

        public static ChunkManager instance;
        public ChunkManager(TerrainGenerator terrainGenerator) {
            instance = this;
            this.terrainGenerator = terrainGenerator;
            CameraController.Instance.onMovePosition += HandleMoveCamera;
            terrainGenerator.onTerrainGenerated += HandleTerrainGenerated;
        }

        public void SetAllChunks(bool state) {
            int l = chunkContainers.Count;
            for (int i = 0; i < l; i++) {
                chunkContainers[i].Active = state;
            }
        }

        public void CreateChunkContainer() {
            float xPos = (ChunkCounter * TerrainGenerator.chunkSize) + TerrainGenerator.chunkSize * .5f;
            float yPos = terrainGenerator.height * .5f;

            ChunkCounter++;
            GameIdentity chunk = new GameIdentity($"Chunk {ChunkCounter}");

            Vector2 chunkPosition = TerrainGenerator.GetGridTilePosition(xPos, 0);
            chunkPosition.Y = -yPos;

            chunk.Transform.position = chunkPosition;
            chunkContainers.Add(chunk);
        }

        private void HandleTerrainGenerated() {
            chunkClampValue = chunkContainers.Count - 1;
            ToggleChunks(CurrentChunk = GetNearestChunkIndex(CameraController.GetCameraPosition()));
        }

        private void HandleMoveCamera(Vector2 newPosition) {
            if (!autoToggleChunks) return;
            ToggleChunks(CurrentChunk = GetNearestChunkIndex(newPosition));
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

        public int GetNearestChunkIndex(Vector2 originPosition) {
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
