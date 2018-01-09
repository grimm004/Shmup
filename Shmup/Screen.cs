using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Shmup
{
    // This contains all the functionality to render and update screens,
    // each screen can have various screen item objects (for example text labels and buttons).
    // The individual screen types are stored in seperate classes to the master screen class
    // which contains all the core functionality for the screens, this way each inheatited screen
    // does not worry about having to duplicate the code to update and draw all of the screen
    // items, it only needs to focus on making them in its constructor.
    // If the screen master-class was un-abstracted, screens could be made just by creating a
    // screen type and externally adding screen items to it.

    /// <summary>
    /// A class that contains all the core functionality for a window
    /// </summary>
    public abstract class Screen
    {
        // Define the background colour for the screen
        public Color BackgroundColour { get; protected set; }
        // Define the list of ScreenItem objects (Text, Buttons, Etc)
        private List<ScreenItem> screenItems = new List<ScreenItem>();

        /// <summary>
        /// Draw all the screen items to the window.
        /// </summary>
        /// <param name="spriteBatch">The active spritebatch used to draw the screen items.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Loop through the screen items
            foreach (ScreenItem item in screenItems)
            {
                // Draw each item
                item.Draw(spriteBatch);
            }
        }

        /// <summary>
        /// Update all the screen items.
        /// </summary>
        /// <param name="mouseState">Information about the mouse and its buttons.</param>
        /// <param name="keyboardState">Information about the keyboard and its keys.</param>
        public void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
        {
            // Loop through the screen items
            foreach (ScreenItem item in screenItems)
            {
                // Update each item
                item.Update(gameTime, mouseState, keyboardState);
            }
        }

        public void AddItem(ScreenItem item)
        {
            screenItems.Add(item);
        }

        public void Show()
        {
            Shmup.previousScreen = Shmup.activeScreen;
            Shmup.activeScreen = this;
        }

        public void Hide()
        {
            Shmup.activeScreen = null;
        }
    }

    /// <summary>
    /// The screen-type class that contains all of the screen items for the home screen. 
    /// </summary>
    public class HomeScreen : Screen
    {
        public HomeScreen(Action ShowGameScreenCommand, Action ShowMultiplayerScreenCommand, Action ShowSettingsScreenCommand, Action ExitCommand)
        {
            BackgroundColour = Color.White;
            //joe was here
            new Text(String.Format("Shmup V{0}", Program.VERSION), Fonts.ArialLarge, new Vector2(50, 50), Color.Black, .5f, Vector2.Zero, 0, this);
            new Button("Singleplayer", Fonts.Arial, Color.White, new Vector2(50, 100), Color.Multiply(Color.Green, 0.9f), Color.Multiply(Color.Green, 1.1f), 2, Color.Black, ShowGameScreenCommand, this);
            new Button("Multiplayer", Fonts.Arial, Color.White, new Vector2(50, 150), Color.Multiply(Color.Green, 0.9f), Color.Multiply(Color.Green, 1.1f), 2, Color.Black, ShowMultiplayerScreenCommand, this);
            new Button("Settings", Fonts.Arial, Color.Black, new Vector2(50, 200), Color.Multiply(Color.Orange, 0.9f), Color.Multiply(Color.Orange, 1.1f), 2, Color.Black, ShowSettingsScreenCommand, this);
            new Button("Exit", Fonts.Arial, Color.Black, new Vector2(50, 400), Color.Red, Color.DarkRed, 2, Color.Black, ExitCommand, this);
        }
    }

    /// <summary>
    /// The screen-type class that contains all of the screen items for the in-game screen. 
    /// </summary>
    public class GameScreen : Screen
    {
        public World world;
        public HUD hud;
        private SpriteSheet ss;

        public GameScreen(SpriteSheet spriteSheet)
        {
            BackgroundColour = Color.Black;
            ss = spriteSheet;
            world = new World(spriteSheet, SettingsManager.Current.starCount, SettingsManager.Current.maxLaserCount, SettingsManager.Current.asteroidCount);
            hud = new HUD(this);
        }

        public void UpdateGame(GameTime gameTime)
        {
            world.Update(gameTime);
        }

        public void DrawGame(GameTime gameTime, SpriteBatch spriteBatch)
        {
            world.Draw(gameTime, spriteBatch);
        }

        public void RestartGame()
        {
            world = new World(ss, SettingsManager.Current.starCount, SettingsManager.Current.maxLaserCount, SettingsManager.Current.asteroidCount);
        }
    }

    /// <summary>
    /// The screen-type class that contains all of the screen items for the end game screen. 
    /// </summary>
    public class GameOverScreen : Screen
    {
        Action showGameScreenCommand;

        public GameOverScreen(Action ShowHomeScreenCommand, Action ShowGameScreenCommand, Action ShowSettingsScreenCommand, Action ExitCommand)
        {
            showGameScreenCommand = ShowGameScreenCommand;

            new Text("Game Over", Fonts.Arial, new Vector2(50, 50), Color.Black, this);

            //new Button("Restart Game", textFont, Color.Black, new Vector2(50, 100), Color.Red, Color.DarkRed, 2, Color.Black, graphics, this);

            new Button("Settings", Fonts.Arial, Color.Black, new Vector2(50, 150), Color.Multiply(Color.Orange, 0.9f), Color.Multiply(Color.Orange, 1.1f), 2, Color.Black, ShowSettingsScreenCommand, this);

            Button homeScreenButton = new Button("Home Screen", Fonts.Arial, Color.Black, new Vector2(50, 400), Color.Red, Color.DarkRed, 2, Color.Black, ShowHomeScreenCommand, this);

            Button exitButton = new Button("Exit", Fonts.Arial, Color.Black, new Vector2(homeScreenButton.borderRectangle.Right + 5, 400), Color.Red, Color.DarkRed, 2, Color.Black, ExitCommand, this);
        }
    }

    /// <summary>
    /// The screen-type class that contains all of the screen items for the pause screen. 
    /// </summary>
    public class PauseScreen : Screen
    {
        public PauseScreen(Action ShowHomeScreenCommand, Action ShowGameScreenCommand, Action ShowSettingsScreenCommand, Action RestartGame, Action ExitCommand)
        {
            BackgroundColour = Color.White;

            new Text("Game Paused", Fonts.Arial, new Vector2(50, 50), Color.Black, this);

            new Button("Continue Game", Fonts.Arial, Color.Black, new Vector2(50, 100), Color.Red, Color.DarkRed, 2, Color.Black, ShowGameScreenCommand, this);
            
            new Button("Restart Game", Fonts.Arial, Color.Black, new Vector2(50, 150), Color.Red, Color.DarkRed, 2, Color.Black, RestartGame, this);

            new Button("Settings", Fonts.Arial, Color.Black, new Vector2(50, 200), Color.Multiply(Color.Orange, 0.9f), Color.Multiply(Color.Orange, 1.1f), 2, Color.Black, ShowSettingsScreenCommand, this);

            Button homeScreenButton = new Button("Home Screen", Fonts.Arial, Color.Black, new Vector2(50, 400), Color.Red, Color.DarkRed, 2, Color.Black, ShowHomeScreenCommand, this);

            new Button("Exit", Fonts.Arial, Color.Black, new Vector2(homeScreenButton.borderRectangle.Right + 5, 400), Color.Red, Color.DarkRed, 2, Color.Black, ExitCommand, this);
        }
    }

    /// <summary>
    /// The screen-type class that contains all of the screen items for the settings screen. 
    /// </summary>
    public class SettingsScreen : Screen
    {
        public Slider targetFpsSlider;
        public Slider screenWidthSlider;
        public Slider screenHeightSlider;
        public CheckBox fullScreenCheckBox;
        public bool updateFps;

        public SettingsScreen(Action BackButtonCommand)
        {
            updateFps = false;
            BackgroundColour = Color.White;

            Text title = new Text("Settings", Fonts.Arial, new Vector2(50, 50), Color.Black, this);

            targetFpsSlider = new Slider(new Vector2(200, title.Start.Y + title.Size.Y + 20), new Vector2(200, 5), new Vector2(10, 25), 60, Constants.MAXFPS, Color.DarkRed, Color.Red, Color.Green, Color.Black, displayText: new Text("Target FPS", Fonts.Arial), initialValue: SettingsManager.Current.targetFps, displayRound: 0, screen: this);
            screenWidthSlider = new Slider(new Vector2(200, targetFpsSlider.start.Y + targetFpsSlider.size.Y), new Vector2(200, 5), new Vector2(10, 25), GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, Color.DarkRed, Color.Red, Color.Green, Color.Black, displayText: new Text("Target Width", Fonts.Arial), initialValue: SettingsManager.Current.width, displayRound: 0, screen: this);
            screenHeightSlider = new Slider(new Vector2(200, screenWidthSlider.start.Y + screenWidthSlider.size.Y), new Vector2(200, 5), new Vector2(10, 25), GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, Color.DarkRed, Color.Red, Color.Green, Color.Black, displayText: new Text("Target Height", Fonts.Arial), initialValue: SettingsManager.Current.height, displayRound: 0, screen: this);
            fullScreenCheckBox = new CheckBox(new Text("Fullscreen", Fonts.Arial), SettingsManager.Current.fullScreen, 2, 2, 20, new Vector2(200, screenHeightSlider.start.Y + screenHeightSlider.size.Y), Color.Black, Color.Red, Color.Black, Color.White, this);

            Button backButton = new Button("Back", Fonts.Arial, Color.Black, new Vector2(50, 400), Color.Red, Color.DarkRed, 2, Color.Black, BackButtonCommand, this);
            new Button("Apply", Fonts.Arial, Color.Black, new Vector2(backButton.start.X + backButton.text.Size.X + 10, backButton.start.Y), Color.Red, Color.DarkRed, 2, Color.Black, Apply, this);
        }

        public void SetSettingValues()
        {
            targetFpsSlider.SetSliderValue(SettingsManager.Current.targetFps);
            screenWidthSlider.SetSliderValue(SettingsManager.Current.width);
            screenHeightSlider.SetSliderValue(SettingsManager.Current.height);
            fullScreenCheckBox.Checked = SettingsManager.Current.fullScreen;
        }

        public void Apply()
        {
            SettingsManager.Current.targetFps = (int)targetFpsSlider.Value;
            SettingsManager.Current.width = (int)screenWidthSlider.Value;
            SettingsManager.Current.height = (int)screenHeightSlider.Value;
            SettingsManager.Current.fullScreen = (bool)fullScreenCheckBox.Checked;
            SettingsManager.CreateSettingsFile(Constants.CONFIGFILE);
            Shmup.Width = SettingsManager.Current.width;
            Shmup.Height = SettingsManager.Current.height;
            Shmup.Graphics.PreferredBackBufferWidth = Shmup.Width;
            Shmup.Graphics.PreferredBackBufferHeight = Shmup.Height;
            Shmup.Graphics.IsFullScreen = SettingsManager.Current.fullScreen;
            Shmup.Graphics.ApplyChanges();
            updateFps = true;
            World.GenerateStars(SettingsManager.Current.starCount);
        }
    }
}
