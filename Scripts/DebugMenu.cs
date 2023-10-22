using static MonoTerrain.Scripts.GameHelper;
using MonoTerrain.Scripts.Gameplay;
using Microsoft.Xna.Framework;
using MonoGame.ImGuiNet;
using ImGuiNET;
using System;

namespace MonoTerrain.Scripts {
    public class DebugMenu {
        private TerrainGenerator terrainGenerator;
        public DebugMenu(TerrainGenerator terrainGenerator) => this.terrainGenerator = terrainGenerator;

        public void DrawDebugWindow(GameTime gameTime) {
            ImGuiRenderer guiRenderer = GameController.Instance.guiRenderer;
            guiRenderer.BeforeLayout(gameTime);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(265, 500));
            ImGui.Begin("MonoTerrain - Generation Config", ImGuiWindowFlags.NoResize);

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
            ImGui.SliderFloat("Smoothness", ref terrainGenerator.smoothness, 1f, 100f);

            ImGui.Checkbox("Reset Camera Postion", ref terrainGenerator.resetCameraPosition);

            if (ImGui.Button("Generate")) terrainGenerator.Generate(); ImGui.SameLine();
            ImGui.Checkbox("Auto Generate", ref terrainGenerator.autoGenerate); ImGui.NewLine();

            ImGui.Text("Info");
            ImGui.Text("FPS: "); ImGui.SameLine();
            ImGui.TextDisabled($"{FramesHelper.AverageFramesPerSecond:F2}");

            ImGui.Text("Position: "); ImGui.SameLine();
            float xPos = CameraController.Instance.Camera.Position.X;
            float yPos = CameraController.Instance.Camera.Position.Y;

            ImGui.TextDisabled($"X [{xPos:F2}]"); ImGui.SameLine();
            ImGui.TextDisabled($"Y [{yPos:F2}]");

            ImGui.Text("Selected tile: "); ImGui.SameLine();
            ImGui.ColorButton("Grass", new System.Numerics.Vector4(0, 255, 0, 255));

            ImGui.End();
            guiRenderer.AfterLayout();
        }
    }
}
