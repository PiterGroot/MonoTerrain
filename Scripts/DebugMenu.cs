using static MonoTerrain.Scripts.GameHelper;
using Microsoft.Xna.Framework.Input;
using MonoTerrain.Scripts.Gameplay;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.ImGuiNet;
using ImGuiNET;
using System;

namespace MonoTerrain.Scripts {
    public class DebugMenu {
        private TerrainGenerator terrainGenerator;

        private readonly List<WindowTab> windowTabs;
        private Action currentWindowTab;

        private bool drawChunkWindow;

        private int currentSelectedChunk;
        private string[] chunkStrings;

        public DebugMenu(TerrainGenerator terrainGenerator) {
            this.terrainGenerator = terrainGenerator;
            windowTabs = new List<WindowTab>() {
                { new WindowTab("Shaping", ShowShapingTab) },
                { new WindowTab("Decoration", ShowDecorationTab) },
            };
            currentWindowTab += windowTabs[0].drawWindow;
            
            chunkStrings = new string[ChunkManager.instance.ChunkCounter];
            for (int i = 0; i < chunkStrings.Length; i++) {
                chunkStrings[i] = $"Chunk {i}";
            }
        }

        public void DrawDebugWindow(GameTime gameTime) {
            ImGuiRenderer guiRenderer = GameController.Instance.guiRenderer;
            guiRenderer.BeforeLayout(gameTime);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(275, 500));
            ImGui.Begin("MonoTerrain - Terrain Tool", ImGuiWindowFlags.NoResize);

            DrawWindowTabs();
            ImGui.NewLine();

            currentWindowTab?.Invoke(); ImGui.NewLine();

            if (ImGui.Button("Generate")) terrainGenerator.Generate(false); ImGui.SameLine();

            SetMouseVisible(ImGui.IsWindowHovered() || ImGui.IsAnyItemHovered());
            
            ImGui.End();
            DrawGameInfoWindow();

            if (drawChunkWindow)
                DrawChunkInspector();

            guiRenderer.AfterLayout();
        }

        private void DrawWindowTabs() {
            int tabs = windowTabs.Count;
            for (int i = 0; i < tabs; i++) {
                if (ImGui.Button(windowTabs[i].windowName)) {
                    currentWindowTab = null;
                    currentWindowTab += windowTabs[i].drawWindow;
                }
                if (i < tabs - 1) ImGui.SameLine();
            }
        }

        private void DrawChunkInspector() {
            ImGui.Begin("Chunk Inspector");
            ImGui.Text("Chunks");

            if (ImGui.Button("Load all")) ChunkManager.instance.SetAllChunks(true); ImGui.SameLine();
            if (ImGui.Button("Unload all")) ChunkManager.instance.SetAllChunks(false); ImGui.SameLine();
            ImGui.Checkbox("Auto (un)load chunks", ref ChunkManager.instance.autoToggleChunks);

            ImGui.ListBox(string.Empty, ref currentSelectedChunk, chunkStrings, ChunkManager.instance.ChunkCounter);
            ImGui.SameLine();
            
            ImGui.BeginGroup();
            if (ImGui.Button("Load")) ChunkManager.instance.chunkContainers[currentSelectedChunk].Active = true;
            ImGui.SameLine();
            if (ImGui.Button("Unload")) ChunkManager.instance.chunkContainers[currentSelectedChunk].Active = false;

            if (ImGui.Button("Snap")) 
                CameraController.Instance.TeleportTo(ChunkManager.instance.chunkContainers[currentSelectedChunk].Transform.position);
            
            ImGui.EndGroup();
            ImGui.End();
        }

        private void DrawGameInfoWindow() {
            ImGui.SetWindowPos(new System.Numerics.Vector2(465, 0));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(265, 265));
            ImGui.Begin("MonoTerrain - Debug Window", ImGuiWindowFlags.NoResize);

            ImGui.Text("Info");
            ImGui.Text("FPS: "); ImGui.SameLine();
            ImGui.TextDisabled($"{FramesHelper.AverageFramesPerSecond:F2}");

            ImGui.Text("Position: "); ImGui.SameLine();

            Vector2 camPosition = CameraController.GetCameraPosition();
            float xPos = camPosition.X;
            float yPos = camPosition.Y;

            ImGui.TextDisabled($"X [{xPos:F2}]"); ImGui.SameLine();
            ImGui.TextDisabled($"Y [{yPos:F2}]");

            ImGui.Text("Mouse world pos: "); ImGui.SameLine();

            string mouseX = GameController.mouseWorldPosition.X.ToString("F2");
            string mouseY = GameController.mouseWorldPosition.Y.ToString("F2");

            ImGui.TextDisabled(mouseX + " " + mouseY);
           
            ImGui.Text("Selected tile: "); ImGui.SameLine();
            ImGui.ColorButton("Grass", new System.Numerics.Vector4(0, 255, 0, 255));

            ImGui.Checkbox("Reset Camera Postion", ref terrainGenerator.resetCameraPosition);

            if (ImGui.Button("Snap to zero point")) CameraController.Instance.Camera.Position = Vector2.Zero;
            ImGui.SameLine();
            if (ImGui.Button("Reset Mouse Postion")) {
                Vector2 center = GetCenterPoint();
                Mouse.SetPosition((int)center.X, (int)center.Y);
            }

            if (ImGui.Button("Open Chunk Inspector"))
                drawChunkWindow = !drawChunkWindow;
            ImGui.End();
        }

        private void ShowDecorationTab() {
            ImGui.Text("Grass depth"); 
            ImGui.PushItemWidth(75); 
            ImGui.InputInt("Min", ref terrainGenerator.grassDepthMin, 1); ImGui.SameLine();
            ImGui.InputInt("Max", ref terrainGenerator.grassDepthMax, 1);
        }

        private void ShowShapingTab() {
            ImGui.PushItemWidth(100);
            ImGui.InputInt("World seed", ref terrainGenerator.seed); ImGui.SameLine();

            terrainGenerator.seed = Math.Clamp(terrainGenerator.seed, (int)terrainGenerator.seedMinMax.X, (int)terrainGenerator.seedMinMax.Y);
            if (ImGui.Button("Random")) terrainGenerator.seed = RandomHandler.GetRandomIntNumber(0, 999999);

            ImGui.PushItemWidth(75);
            ImGui.InputInt("Width", ref terrainGenerator.width, 0); ImGui.SameLine();
            ImGui.InputInt("Height", ref terrainGenerator.height, 0);

            ImGui.NewLine();

            ImGui.PushItemWidth(160);
            ImGui.InputInt("Ocataves", ref terrainGenerator.octaves, 1);
            ImGui.SliderFloat("Persistence", ref terrainGenerator.persistence, .1f, 1f);
            ImGui.SliderFloat("Lacunarity", ref terrainGenerator.lacunarity, 1f, 5f);
            ImGui.SliderFloat("Smoothness", ref terrainGenerator.smoothness, 1f, 200f);
        }

        private struct WindowTab {
            public string windowName;
            public Action drawWindow;

            public WindowTab(string windowName, Action windowToDraw, bool drawOnNewLine = false) {
                this.windowName = windowName;
                drawWindow = windowToDraw;
            }
        }
    }
}
