﻿using static MonoTerrain.Scripts.GameHelper;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace MonoTerrain.Scripts.Gameplay {
    public class TerrainGenerator {
        private int[,] map;
        private OpenSimplexNoise simplexNoise;
        private ChunkManager chunkManager;
        public Action onTerrainGenerated;

        public static readonly float tileSize = 1;
        public static readonly int tileTextureSize = 16;
        public static readonly int chunkSize = 100;

        private readonly bool generateOnAwake = true;
        private readonly bool randomizeConfig = false;
        private readonly bool randomSeed = false;

        public bool resetCameraPosition;

        public Vector2 seedMinMax = new Vector2(1, 999999);

        public int grassDepthMin = 2;
        public int grassDepthMax = 5;

        /// <summary>
        /// Terrain config settings
        /// </summary>

        public int seed = 12345;

        public int width = 900;
        public int height = 350;
       
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
            if (generateOnAwake) Generate(true);
        }

        public void Generate(bool isSetup) {
            map = null;

            if(chunkManager != null) {
                foreach (GameIdentity identity in chunkManager.chunkContainers) {
                    GameIdentityManager.Instance.DestroyIdentity(identity);
                }
            }
            chunkManager = new ChunkManager(this);

            if (randomSeed) seed = RandomHandler.GetRandomIntNumber(0, 99999);
            if (randomizeConfig) {
                octaves = RandomHandler.GetRandomIntNumber(1, 9);
                persistence = RandomHandler.GetRandomFloatingNumber(0f, 1f);
                lacunarity = RandomHandler.GetRandomFloatingNumber(1f, 5f);
                smoothness = RandomHandler.GetRandomFloatingNumber(1, 100f);
            }

            RandomHandler.SetSeed(seed);
            simplexNoise = new OpenSimplexNoise(seed);

            map = InitializeMap(width, height);
            map = ApplyNoisePass(map, octaves, persistence, lacunarity, smoothness);
            
            PopulateMap();

            foreach (GameIdentity identity in chunkManager.chunkContainers) {
                identity.Active = false;
                GameIdentityManager.Instance.InstantiateIdentity(identity, identity.Transform.position, true);
            }

            if (resetCameraPosition || isSetup) 
                CameraController.Instance.Camera.Position = GetGridTilePosition(width / 2, 0, 16 * tileSize);

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
                    
                    tileLibrary[map[x, y]].InstantiateTile(x, y, tileTextureSize, chunkManager);
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

        public static Vector2 GetGridTilePosition(float xPos, float yPos, float tileSize) {
            return new Vector2(xPos * tileSize, yPos * tileSize);
        }

        public static Vector2 GetGridMousePosition() {
            return new Vector2(
                (float)Math.Round(GameController.mouseWorldPos.X / tileTextureSize) * tileTextureSize,
                (float)Math.Round(GameController.mouseWorldPos.Y / tileTextureSize) * tileTextureSize
            );
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

            public void InstantiateTile(int x, int y, int tileTextureHeight, ChunkManager chunkManager) {
                GameIdentity tile = new GameIdentity(tileName, tileTexture, tileRenderOrder);
                Vector2 tilePosition = GetGridTilePosition(x, y, tileTextureHeight * tileSize);
                tile.Transform.position = tilePosition;
                
                tile.Transform.SetScale(Vector2.One * tileSize);
                tile.Visual.textureColor = tileColor;

                chunkManager.chunks[chunkManager.ChunkCounter].Add(new Tile(tilePosition, tile));
                chunkManager.chunkContainers[chunkManager.ChunkCounter - 1].Children.Add(tile);
            }
        }

        public struct Tile {
            public Vector2 position;
            public GameIdentity tileIdentity;

            public Tile(Vector2 position, GameIdentity identity) {
                this.position = position;
                tileIdentity = identity;
            }
        }
    }
}
