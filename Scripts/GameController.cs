using static MonoTerrain.Scripts.GameHelper;
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
        public static Vector2 mouseScreenPosition { get; private set; }
        public static Vector2 mouseWorldPosition { //TODO: :( !!!!!!!!
            get {
                Vector2 mouseScreen = mouseScreenPosition;
                mouseScreen.Y *= -1;

                Vector2 screenToWorld = CameraController.Instance.Camera.ScreenToWorld(mouseScreen);
                screenToWorld.Y *= -1;
                return screenToWorld;
            }
        }

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private DebugMenu debugMenu;

        private GameIdentity cursor;

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
            IsMouseVisible = false;
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

            _ = new CameraController(Window, GraphicsDevice, Viewport);
            _ = new GameIdentityManager();

            TerrainGenerator = new TerrainGenerator();
            debugMenu = new DebugMenu(TerrainGenerator);
            OnUpdate += TerrainGenerator.LiveGenerateUpdate;

            base.Initialize();
        }

        protected override void LoadContent() {
            cursor = new GameIdentity("Cursor", "crosshair", 1);
            GameIdentityManager.Instance.ignoreViewMatrixIdentities.Add(cursor);
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
            if (KeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            CameraController.Instance.UpdateCamera(gameTime);
            GameIdentityManager.Instance.DrawGameIdentities(spriteBatch, GraphicsDevice);
            debugMenu.DrawDebugWindow(gameTime);

            Vector2 position = MouseState.Position.ToVector2() - CachedCenterPoint;
            position.Y *= -1;

            mouseScreenPosition = position;
            cursor.Transform.position = mouseScreenPosition;

            OnUpdate?.Invoke(gameTime);
            base.Update(gameTime);
        }
    }
#pragma warning restore IDE0090 
}