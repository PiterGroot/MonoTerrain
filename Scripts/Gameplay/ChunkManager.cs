using static MonoTerrain.Scripts.GameHelper;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoTerrain.Scripts.Gameplay {
    public class ChunkManager {

        public static int ChunkCounter { get; private set; }
        public static List<GameIdentity> chunkContainers = new List<GameIdentity>();
        public static Dictionary<int, List<TerrainGenerator.Tile>> chunks = new Dictionary<int, List<TerrainGenerator.Tile>>();

        public TerrainGenerator terrainGenerator;

        public ChunkManager(TerrainGenerator terrainGenerator) { 
            this.terrainGenerator = terrainGenerator;
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
    }
}
