using static MonoTerrain.Scripts.GameHelper;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using MonoTerrain.Scripts.Gameplay;
using Microsoft.Xna.Framework;
using System;

namespace MonoTerrain.Scripts
{
#pragma warning disable IDE0090 

    /// <summary>
    /// Code written by Piter Groot / pitergroot.nl
    /// </summary>
    
    public class GameController : Game
    {
        public Action<GameTime> OnUpdate { get; set; }
        public Viewport Viewport { get; private set; }
        public KeyboardState KeyboardState { get; private set; }
        public MouseState MouseState { get; private set; }

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private bool showDebugInfo;

        public static GameController Instance;
        public GameController()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content/Assets/Textures";

            graphics.SynchronizeWithVerticalRetrace = false;

            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;

            IsFixedTimeStep = false;
            Window.IsBorderless = false;
        }

        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Viewport = GraphicsDevice.Viewport;

            GameHelper.GameController = this;
            GameHelper.GraphicsDevice = GraphicsDevice;
            GameHelper.spriteBatch = spriteBatch;

            Instance = this;

            new CameraController(Window, GraphicsDevice, Viewport);
            new GameIdentityManager();

            new TerrainGenerator();
            
            base.Initialize();
        }

        protected override void LoadContent() {
            Vector2 textSize = Vector2.One * .5f;
            TextDrawer.InstantiateTextLabel("Seed", "Seed:" + TerrainGenerator.seed, Vector2.Zero, Color.White, textSize);
            TextDrawer.InstantiateTextLabel("Size", "Size:" + TerrainGenerator.width + " " + TerrainGenerator.height, Vector2Helper.Up * 20, Color.White, textSize);
            TextDrawer.InstantiateTextLabel("Octaves", "Octaves:" + TerrainGenerator.octaves, Vector2Helper.Up * 40, Color.White, textSize);
            TextDrawer.InstantiateTextLabel("Persistence", "Persistence:" + TerrainGenerator.persistence, Vector2Helper.Up * 60, Color.White, textSize);
            TextDrawer.InstantiateTextLabel("Lacunarity", "Lacunarity:" + TerrainGenerator.lacunarity, Vector2Helper.Up * 80, Color.White, textSize);
            TextDrawer.InstantiateTextLabel("Smoothness", "Smoothness:" + TerrainGenerator.smoothness, Vector2Helper.Up * 100, Color.White, textSize);
            
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
            if (KeyboardState.IsKeyDown(Keys.Back))
                Exit();

            CameraController.Instance.UpdateCamera(gameTime);
            GameIdentityManager.Instance.DrawGameIdentities(spriteBatch, GraphicsDevice);

            OnUpdate?.Invoke(gameTime);
            base.Update(gameTime);
        }
    }
#pragma warning restore IDE0090 
}