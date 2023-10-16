﻿using static MonoTerrain.Scripts.GameHelper;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace MonoTerrain.Scripts.Gameplay {
    public class TerrainGenerator {

        private int[,] map;
        private OpenSimplexNoise simplexNoise;
        
        public static float tileSize = 1;

        private readonly bool generateOnAwake = true;
        private readonly bool randomizeConfig = false;
        private readonly bool randomSeed = true;
        
        /// <summary>
        /// Terrain config settings
        /// </summary>
       
        public static int seed = 12345;

        public static readonly int width = 375;
        public static readonly int height = 350;
       
        private readonly int heightReduction = 10;

        public static int octaves = 4;
        public static float persistence = 0.3f;
        public static float lacunarity = 5;
        public static float smoothness = 90;

        /// <summary>
        /// Terrain config settings
        /// </summary>

        private Dictionary<int, TilePreset> tileLibrary = new Dictionary<int, TilePreset>() {
            { 1, new TilePreset("Stone", "cube", 1, Color.Gray) },
            { 2, new TilePreset("Grass", "cube", 1, Color.Green) },
        };

        public TerrainGenerator() {
            if (generateOnAwake) Generate();
        }

        private void Generate() {
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
            CameraController.Instance.Camera.Position = GetGridPosition(width / 2, 0, 16 * tileSize);

            if (!randomizeConfig) return;

            print("Seed " + seed);
            print("Octaves " + octaves);
            print("Persistence " + persistence);
            print("Lacunarity " + lacunarity);
            print("Smoothness " + smoothness);
        }

        private int[,] InitializeMap(int width, int height) {
            int[,] map = new int[width, height];
            for (int x = 0; x < width; x++) for (int y = 0; y < height; y++) map[x, y] = 0;

            return map;
        }

        private void PopulateMap() {
            for (int x = 0; x < map.GetUpperBound(0); x++) {
                for (int y = 0; y < map.GetUpperBound(1); y++) {
                    int tileKey = map[x, y];
                    if (tileKey == 0) continue; //ignore air tile
                    
                    tileLibrary[map[x, y]].InstantiateTile(x, y, 16);
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
                    int randomGrassDepth = RandomHandler.GetRandomIntNumber(2, 5);
                    if(y >= Math.Floor(totalHeight) - randomGrassDepth) {
                        map[x, y] = 2;
                        continue;
                    }
                    map[x, y] = 1;
                }
            }

            return map;
        }

        public static Vector2 GetGridPosition(float xPos, float yPos, float tileSize) {
            return new Vector2(xPos * tileSize, yPos * tileSize);
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

            public void InstantiateTile(int x, int y, int tileTextureHeight) {
                GameIdentity tile = new GameIdentity(tileName, tileTexture, tileRenderOrder);
                
                tile.Transform.SetScale(Vector2.One * tileSize);
                tile.Visual.textureColor = tileColor;

                Vector2 tilePosition = GetGridPosition(x, y, tileTextureHeight * tileSize);
                GameIdentityManager.Instance.InstantiateIdentity(tile, tilePosition);
            }
        }
    }
}
