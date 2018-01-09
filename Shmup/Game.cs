using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Shmup
{
    /// <summary>
    /// The type that manages and runs the game
    /// </summary>
    public class Shmup : Microsoft.Xna.Framework.Game
    {
        public static FileStream LogFile { get; private set; }

        public static GraphicsDeviceManager Graphics { get; protected set; }
        private SpriteBatch spriteBatch;
        private SpriteSheet spriteSheet;

        public static FrameCounter FrameCounter { get; private set; }

        public const double MULTIPLAYER_TICKRATE = 60;
        public bool InMultiplayer { get; set; }
        
        public static int Width { get; set; }
        public static int Height { get; set; }

        private static HomeScreen homeScreen;
        private static GameScreen gameScreen;
        private static PauseScreen pauseScreen;
        private static SettingsScreen settingsScreen;
        public static Screen activeScreen;
        public static Screen previousScreen;

        private KeyListener pauseListener;

        /// <summary>
        /// Instantiates the graphics manager
        /// </summary>
        public Shmup()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            FrameCounter = new FrameCounter();

            Width = SettingsManager.Current.width;
            Height = SettingsManager.Current.height;
            Graphics.PreferredBackBufferWidth = Width;
            Graphics.PreferredBackBufferHeight = Height;
            Graphics.IsFullScreen = SettingsManager.Current.fullScreen;
            Graphics.SynchronizeWithVerticalRetrace = false;

            this.IsMouseVisible = SettingsManager.Current.isMouseVisible;

            LogFile = new FileStream("Shmup\\debug.log", FileMode.Append);

            Graphics.ApplyChanges();
            base.Initialize();
        }

        public static void Log(string text, bool newLine = true)
        {
            if (newLine) text += Environment.NewLine;
            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            LogFile.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Set the target framerate (FPS) for the screen
        /// </summary>
        /// <param name="frames">The target number of frames per second.</param>
        private void SetFPS(int frames)
        {
            double timeDelay = 1000d / (double)frames;
            if (timeDelay >= 2 || timeDelay > 1000d / 60d)
            {
                this.IsFixedTimeStep = true;
                //Graphics.SynchronizeWithVerticalRetrace = false;
                this.TargetElapsedTime = TimeSpan.FromMilliseconds(timeDelay);
            }
            else
            {
                this.IsFixedTimeStep = true;
                //Graphics.SynchronizeWithVerticalRetrace = false;
                this.TargetElapsedTime = TimeSpan.FromMilliseconds(1000d / 60d);
            }
            Graphics.ApplyChanges();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteSheet = new SpriteSheet(this.Content.Load<Texture2D>("sprites-white"));
            Fonts.Arial = this.Content.Load<SpriteFont>("Arial");
            Fonts.ArialLarge = this.Content.Load<SpriteFont>("ArialLarge");
            Fonts.TimesNewRoman = this.Content.Load<SpriteFont>("TimesNewRoman");
            Fonts.SegoeUIMono = this.Content.Load<SpriteFont>("SegoeUIMono");
            
            gameScreen = new GameScreen(spriteSheet);
            homeScreen = new HomeScreen(gameScreen.Show, StartMultiplayer, ShowSettingsScreen, this.Exit);
            homeScreen.Show();
            pauseScreen = new PauseScreen(homeScreen.Show, TogglePauseScreen, ShowSettingsScreen, gameScreen.RestartGame, this.Exit);
            settingsScreen = new SettingsScreen(BackButton);
            pauseListener = new KeyListener(Keys.Escape, TogglePauseScreen);
            settingsScreen.SetSettingValues();
        }

        /// <summary>
        /// Go back to the previous screen
        /// </summary>
        private void BackButton()
        {
            previousScreen.Show();
            settingsScreen.SetSettingValues();
        }

        /// <summary>
        /// Start the multiplayer mode
        /// </summary>
        private void StartMultiplayer()
        {

        }

        /// <summary>
        /// Show the settings creen
        /// </summary>
        private void ShowSettingsScreen()
        {
            settingsScreen.Show();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            LogFile.Close();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        double updateTimer = 0;
        protected override void Update(GameTime gameTime)
        {
            //Shmup.Log(String.Format("FPS: {0}\tUpdate Time: {1}", FrameCounter.AverageFramesPerSecond, gameTime.ElapsedGameTime.TotalMilliseconds));
            if (InMultiplayer && updateTimer > 1000d / (double)MULTIPLAYER_TICKRATE)
            {
                updateTimer = 0;
            }
            updateTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (settingsScreen.updateFps)
            {
                SetFPS(SettingsManager.Current.targetFps);
                settingsScreen.updateFps = true;
            }
            pauseListener.Update(gameTime, Mouse.GetState(), Keyboard.GetState());
            if (activeScreen != null && activeScreen.GetType() == typeof(GameScreen)) gameScreen.UpdateGame(gameTime);
            if (activeScreen != null) activeScreen.Update(gameTime, Mouse.GetState(), Keyboard.GetState());
            base.Update(gameTime);
        }

        /// <summary>
        /// Toggle the pause screen
        /// </summary>
        bool inGame = true;
        private void TogglePauseScreen()
        {
            if (inGame) { pauseScreen.Show(); } else { gameScreen.Show(); }
            inGame ^= true;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            FrameCounter.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            if (activeScreen != null) GraphicsDevice.Clear(activeScreen.BackgroundColour);
            else GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            if (activeScreen != null && activeScreen.GetType() == typeof(GameScreen)) gameScreen.DrawGame(gameTime, spriteBatch);
            if (activeScreen != null) activeScreen.Draw(spriteBatch);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }

    /// <summary>
    /// A class that stores the instances of the renderable fonts
    /// </summary>
    static class Fonts
    {
        public static SpriteFont Arial;
        public static SpriteFont ArialLarge;
        public static SpriteFont TimesNewRoman;
        public static SpriteFont SegoeUIMono;
    }
}
