using static MonoTerrain.Scripts.GameHelper;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace MonoTerrain.Scripts.Gameplay {
    public class TerrainGenerator {
        public static int[,] map;
        private OpenSimplexNoise simplexNoise;
        private ChunkManager chunkManager;
        public Action onTerrainGenerated;

        public static readonly float tileSize = .6f;
        public static readonly int tileTextureSize = 16;
        public static readonly int chunkSize = 100;

        private readonly bool generateOnAwake = true;
        private readonly bool randomizeConfig = false;
        private readonly bool randomSeed = false;

        private bool isLiveGenerating = false;
        public bool resetCameraPosition = false;

        public Vector2 seedMinMax = new Vector2(1, 999999);

        public int grassDepthMin = 2;
        public int grassDepthMax = 5;

        /// <summary>
        /// Terrain config settings
        /// </summary>

        public int seed = 12345;

        public int width = 1000;
        public int height = 600;

        private readonly int liveWidth = 200;
        private readonly int liveHeight = 500;

        private int prevWidth;
        private int prevHeight;
       
        private readonly int heightReduction = 10;

        public int octaves = 4;
        public float persistence = 0.3f;
        public float lacunarity = 5;
        public float smoothness = 90;

        /// <summary>
        /// Terrain config settings
        /// </summary>

        private Dictionary<int, TilePreset> tileLibrary = new Dictionary<int, TilePreset>() {
            { 1, new TilePreset("Stone", "cube", -1, Color.Gray) },
            { 2, new TilePreset("Grass", "cube", -1, Color.Green) },
        };

        public TerrainGenerator() {
            chunkManager = new ChunkManager(this);

            if (generateOnAwake) 
                Generate(true);
        }

        public void LiveGenerateUpdate(GameTime _) {
            if (!isLiveGenerating) return;
            Generate(false);
        }

        public void LiveGenerate() {
            bool current = isLiveGenerating;
            isLiveGenerating = !isLiveGenerating;

            if (current && !isLiveGenerating) {
                width = prevWidth;
                height = prevHeight;

                Generate(true);
            }
            else
                CameraController.Instance.TeleportTo(ChunkManager.instance.chunkContainers[0].Transform.position);

            prevWidth = width;
            prevHeight = height;
        }

        public void Generate(bool isSetup) {
            if (isLiveGenerating) {
                width = liveWidth;
                height = liveHeight;
            }

            map = null;

            chunkManager.ClearData();

            simplexNoise = new OpenSimplexNoise(seed);
            RandomHandler.SetSeed(seed);

            if (randomSeed) seed = RandomHandler.GetRandomIntNumber(0, 99999);
            if (randomizeConfig) {
                octaves = RandomHandler.GetRandomIntNumber(1, 9);
                persistence = RandomHandler.GetRandomFloatingNumber(0f, 1f);
                lacunarity = RandomHandler.GetRandomFloatingNumber(1f, 5f);
                smoothness = RandomHandler.GetRandomFloatingNumber(1, 100f);
            }

            map = InitializeMap(width, height);
            map = ApplyNoisePass(map, octaves, persistence, lacunarity, smoothness);
            
            PopulateMap();

            foreach (GameIdentity identity in chunkManager.chunkContainers) {
                GameIdentityManager.Instance.InstantiateIdentity(identity, identity.Transform.position);
            }

            if (resetCameraPosition || isSetup) 
                CameraController.Instance.Camera.Position = GetGridTilePosition(width / 2, 0);

            onTerrainGenerated?.Invoke();
        }

        private int[,] InitializeMap(int width, int height) {
            int[,] map = new int[width, height];
            for (int x = 0; x < width; x++) for (int y = 0; y < height; y++) map[x, y] = 0;

            return map;
        }

        private void PopulateMap() {
            for (int x = 0; x < map.GetUpperBound(0); x++) {
                if (x % chunkSize == 0) chunkManager.CreateChunkContainer();
                for (int y = 0; y < map.GetUpperBound(1); y++) {
                    int tileKey = map[x, y];
                    if (tileKey == 0) continue; //ignore air tile
                    
                    tileLibrary[map[x, y]].InstantiateTile(x, y, chunkManager);
                }
            }
        }

        private int[,] ApplyNoisePass(int[,] map, int octaves, float persistence, float lacunarity, float smoothness) {
            for (int x = 0; x < width; x++) {
                float amplitude = 1f;
                float frequency = 1f;
                float totalHeight = 0f;

                for (int octave = 0; octave < octaves; octave++) {
                    int perlinHeight = (int)Math.Round(simplexNoise.Evaluate(x * frequency / smoothness, 0) * height / heightReduction);

                    amplitude *= persistence;
                    frequency *= lacunarity;
                    totalHeight += perlinHeight * amplitude;
                }

                totalHeight += height / heightReduction;
                
                for (int y = 0; y < totalHeight; y++) {
                    int randomGrassDepth = RandomHandler.GetRandomIntNumber(grassDepthMin, grassDepthMax);
                    if(y >= Math.Floor(totalHeight) - randomGrassDepth) {
                        map[x, y] = 2;
                        continue;
                    }
                    map[x, y] = 1;
                }
            }

            return map;
        }

        public static Vector2 GetGridTilePosition(float xPos, float yPos) {
            return new Vector2(xPos * tileTextureSize * tileSize, yPos * tileTextureSize * tileSize);
        }


        private struct TilePreset {
            public string tileName;
            public string tileTexture;
            public int tileRenderOrder;
            public Color tileColor;

            public TilePreset(string tileName, string tileTexture, int tileRenderOrder, Color tileColor){
                this.tileName = tileName;
                this.tileTexture = tileTexture;
                this.tileRenderOrder = tileRenderOrder;
                this.tileColor = tileColor;
            }

            public void InstantiateTile(int x, int y, ChunkManager chunkManager) {
                GameIdentity tile = new GameIdentity(tileName, tileTexture, tileRenderOrder);
                Vector2 tilePosition = GetGridTilePosition(x, y);
                tile.Transform.position = tilePosition;
                
                tile.Transform.SetScale(Vector2.One * tileSize);
                tile.Visual.textureColor = tileColor;

                chunkManager.chunkContainers[chunkManager.ChunkCounter - 1].Children.Add(tile);
            }
        }
    }
}
