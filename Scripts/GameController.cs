using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoTerrain.Scripts.Gameplay;
using Microsoft.Xna.Framework;
using MonoGame.ImGuiNet;
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
        public ImGuiRenderer guiRenderer { get; private set; }
        public TerrainGenerator TerrainGenerator { get; private set; }

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private DebugMenu debugMenu;

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
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Viewport = GraphicsDevice.Viewport;

            guiRenderer = new ImGuiRenderer(this);
            guiRenderer.RebuildFontAtlas();


            GameHelper.GameController = this;
            GameHelper.GraphicsDevice = GraphicsDevice;
            GameHelper.spriteBatch = spriteBatch;

            Instance = this;

            new CameraController(Window, GraphicsDevice, Viewport);
            new GameIdentityManager();

            TerrainGenerator = new TerrainGenerator();
            debugMenu = new DebugMenu(TerrainGenerator);
            
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
            if (KeyboardState.IsKeyDown(Keys.Back))
                Exit();

            CameraController.Instance.UpdateCamera(gameTime);
            GameIdentityManager.Instance.DrawGameIdentities(spriteBatch, GraphicsDevice);
            debugMenu.DrawDebugWindow(gameTime);

            OnUpdate?.Invoke(gameTime);
            base.Update(gameTime);
        }
    }
#pragma warning restore IDE0090 
}