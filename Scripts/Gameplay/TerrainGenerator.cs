using static MonoTerrain.Scripts.GameHelper;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.ImGuiNet;
using ImGuiNET;
using System;

namespace MonoTerrain.Scripts.Gameplay {
    public class TerrainGenerator {

        public static List<GameIdentity> tiles = new List<GameIdentity>();
        private int[,] map;
        private OpenSimplexNoise simplexNoise;
        
        public static float tileSize = 1;

        private readonly bool generateOnAwake = true;
        private readonly bool randomizeConfig = false;
        private readonly bool randomSeed = false;
        private bool autoGenerate;
        private bool resetCameraPosition = true;

        private Vector2 seedMinMax = new Vector2(1, 999999);

        /// <summary>
        /// Terrain config settings
        /// </summary>

        public static int seed = 12345;

        public static int width = 875;
        public static int height = 350;
       
        private readonly int heightReduction = 10;

        public static int octaves = 4;
        public static float persistence = 0.3f;
        public static float lacunarity = 5;
        public static float smoothness = 90;

        /// <summary>
        /// Terrain config settings
        /// </summary>

        private Dictionary<int, TilePreset> tileLibrary = new Dictionary<int, TilePreset>() {
            { 1, new TilePreset("Stone", "cube", -1, Color.Gray) },
            { 2, new TilePreset("Grass", "cube", -1, Color.Green) },
        };

        public TerrainGenerator() {
            GameController.Instance.OnUpdate += OnUpdate;
            if (generateOnAwake) Generate();
        }

        private void OnUpdate(GameTime gameTime) {
            if(autoGenerate) Generate();
        }

        private void Generate() {
            map = null;

            for (int i = 0; i < tiles.Count; i++) {
                GameIdentityManager.Instance.DestroyIdentity(tiles[i]);
            }
            tiles.Clear();

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

            if(resetCameraPosition) 
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

        public void DrawDebugWindow(GameTime gameTime) {
            ImGuiRenderer guiRenderer = GameController.Instance.guiRenderer;
            guiRenderer.BeforeLayout(gameTime);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(260, 500));
            ImGui.Begin("MonoTerrain - Generation Config", ImGuiWindowFlags.NoResize);

            ImGui.PushItemWidth(100);
            ImGui.InputInt("World seed", ref seed); ImGui.SameLine();

            seed = Math.Clamp(seed, (int)seedMinMax.X, (int)seedMinMax.Y);
            if (ImGui.Button("Random")) seed = RandomHandler.GetRandomIntNumber(0, 999999);

            ImGui.PushItemWidth(75);
            ImGui.InputInt("Width", ref width, 0); ImGui.SameLine();
            ImGui.InputInt("Height", ref height, 0);

            ImGui.NewLine();

            ImGui.PushItemWidth(160);
            ImGui.InputInt("Ocataves", ref octaves, 1);
            ImGui.SliderFloat("Persistence", ref persistence, .1f, 1f);
            ImGui.SliderFloat("Lacunarity", ref lacunarity, 1f, 5f);
            ImGui.SliderFloat("Smoothness", ref smoothness, 1f, 100f);

            ImGui.Checkbox("Reset Camera Postion", ref resetCameraPosition);

            if (ImGui.Button("Generate")) Generate(); ImGui.SameLine();
            ImGui.Checkbox("Auto Generate", ref autoGenerate);
            
            ImGui.End();
            guiRenderer.AfterLayout();
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
                tiles.Add(tile);
            }
        }
    }
}
