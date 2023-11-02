﻿using static MonoTerrain.Scripts.GameHelper;
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

        private int xCamTeleportPosition;
        private int yCamTeleportPosition;

        public DebugMenu(TerrainGenerator terrainGenerator) {
            this.terrainGenerator = terrainGenerator;
            windowTabs = new List<WindowTab>() {
                { new WindowTab("Shaping", ShowShapingTab) },
                { new WindowTab("Decoration", ShowDecorationTab) },
            };
            currentWindowTab += windowTabs[0].drawWindow;
            
            chunkStrings = new string[terrainGenerator.chunkCounter];
            for (int i = 0; i < chunkStrings.Length; i++) {
                chunkStrings[i] = $"Chunk {i}";
            }
        }

        public void DrawDebugWindow(GameTime gameTime) {
            ImGuiRenderer guiRenderer = GameController.Instance.guiRenderer;
            guiRenderer.BeforeLayout(gameTime);
            terrainGenerator.AutoUpdate();
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(275, 500));
            ImGui.Begin("MonoTerrain - Terrain Tool", ImGuiWindowFlags.NoResize);

            DrawWindowTabs();
            ImGui.NewLine();

            currentWindowTab?.Invoke(); ImGui.NewLine();

            ImGui.Value("je", 1);
            if (ImGui.Button("Generate")) terrainGenerator.Generate(); ImGui.SameLine();
            ImGui.Checkbox("Auto Generate", ref terrainGenerator.autoGenerate); ImGui.NewLine();

            SetMouseVisible(ImGui.IsWindowHovered() || ImGui.IsAnyItemHovered());
            
            ImGui.End();
            DrawGameInfoWindow();
            ImGui.End();

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

            ImGui.ListBox(string.Empty, ref currentSelectedChunk, chunkStrings, terrainGenerator.chunkCounter);
            ImGui.SameLine();
            
            ImGui.BeginGroup();
            if (ImGui.Button("Load")) TerrainGenerator.chunkContainers[currentSelectedChunk].Active = true;
            ImGui.SameLine();
            if (ImGui.Button("Unload")) TerrainGenerator.chunkContainers[currentSelectedChunk].Active = false;


            if (ImGui.Button("Snap")) {
                
                //Vector2 snapPosition = terrainGenerator.GetGridTilePosition(width / 2, 0, 16 * tileSize)
                //CameraController.Instance.TeleportTo()
            }
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
            float xPos = CameraController.Instance.Camera.Position.X;
            float yPos = CameraController.Instance.Camera.Position.Y;

            ImGui.TextDisabled($"X [{xPos:F2}]"); ImGui.SameLine();
            ImGui.TextDisabled($"Y [{yPos:F2}]");

            ImGui.Text("Mouse pos: "); ImGui.SameLine();

            ImGui.TextDisabled((GameController.mouseWorldPos * CameraController.Instance.Camera.Zoom).ToString());
           
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
            
            ImGui.NewLine();
            ImGui.Text("Teleport Camera");

            ImGui.PushItemWidth(75);
            ImGui.InputInt("x", ref xCamTeleportPosition, 0); ImGui.SameLine();
            ImGui.InputInt("y", ref yCamTeleportPosition, 0); ImGui.SameLine();
            
            if (ImGui.Button("Teleport")) {
                Vector2 position = TerrainGenerator.GetGridTilePosition(xCamTeleportPosition, yCamTeleportPosition, TerrainGenerator.tileSize);
                CameraController.Instance.TeleportTo(position);
            }
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
