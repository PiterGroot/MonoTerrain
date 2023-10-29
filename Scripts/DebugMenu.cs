using static MonoTerrain.Scripts.GameHelper;
using Microsoft.Xna.Framework.Input;
using MonoTerrain.Scripts.Gameplay;
using Microsoft.Xna.Framework;
using MonoGame.ImGuiNet;
using ImGuiNET;
using System;

namespace MonoTerrain.Scripts {
    public class DebugMenu {
        private TerrainGenerator terrainGenerator;
        public DebugMenu(TerrainGenerator terrainGenerator) => this.terrainGenerator = terrainGenerator;

        private bool showShapingTab = true;
        private bool showDecorationTab;

        public void DrawDebugWindow(GameTime gameTime) {
            ImGuiRenderer guiRenderer = GameController.Instance.guiRenderer;
            guiRenderer.BeforeLayout(gameTime);
            terrainGenerator.AutoUpdate(gameTime);

            ImGui.SetNextWindowSize(new System.Numerics.Vector2(265, 500));
            ImGui.Begin("MonoTerrain - Terrain Tool", ImGuiWindowFlags.NoResize);

            if (ImGui.Button("Shaping")) showShapingTab = !showShapingTab;
            ImGui.SameLine();

            if (ImGui.Button("Decoration")) showDecorationTab = !showDecorationTab;
            ImGui.NewLine();

            if (showShapingTab) ShowShapingTab();
            if (showDecorationTab) ShowDecorationTab();

            SetMouseVisible(ImGui.IsWindowHovered());

            ImGui.End();

            ImGui.SetWindowPos(new System.Numerics.Vector2(465, 0));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(265, 265));
            ImGui.Begin("MonoTerrain - Debug Window", ImGuiWindowFlags.NoResize);

            DrawGameInfoWindow();

            ImGui.End();
            guiRenderer.AfterLayout();
        }

        private void DrawGameInfoWindow() {
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
        }

        private void ShowDecorationTab() {
            ImGui.Text("Selected tile: "); ImGui.SameLine();
            ImGui.ColorButton("Grass", new System.Numerics.Vector4(0, 255, 0, 255));
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

            ImGui.Checkbox("Reset Camera Postion", ref terrainGenerator.resetCameraPosition);

            if (ImGui.Button("Snap to zero point")) CameraController.Instance.Camera.Position = Vector2.Zero;
            ImGui.SameLine();
            if (ImGui.Button("Reset Mouse Postion")) {
                Vector2 center = GetCenterPoint();
                Mouse.SetPosition((int)center.X, (int)center.Y);
            }

            if (ImGui.Button("Generate")) terrainGenerator.Generate(); ImGui.SameLine();
            ImGui.Checkbox("Auto Generate", ref terrainGenerator.autoGenerate); ImGui.NewLine();
        }
    }
}
