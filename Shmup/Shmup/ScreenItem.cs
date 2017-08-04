using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Shmup
{
    // Screen items are objects that are programmed to directly interface with the screen objects,
    // providing an easy way of creating, updateing and drawing commonly used 'widgets' to the screen.
    // These sreen items can use eachother as well, for example the Button item uses a text item as an
    // overlay. Like with the screen class, these classes inhearate from the ScreenItem class and so
    // must implement the Draw and Update methods (whether they are needed or not), this means that
    // in the screen class all screen items can be stored in one array under one datatype, and all be
    // efficiently updated and drawn as a batch.

    /// <summary>
    /// A class that contains all the core functionality of a screen item
    /// </summary>
    public abstract class ScreenItem
    {
        public ScreenItem(Screen screen = null)
        {
            if (screen != null)
            {
                AddToScreen(screen);
            }
        }

        public void AddToScreen(Screen screen)
        {
            screen.AddItem(this);
        }

        /// <summary>
        /// Draw the screen item to the window
        /// </summary>
        /// <param name="spriteBatch">Used to draw the item(s).</param>
        public abstract void Draw(SpriteBatch spriteBatch);
        /// <summary>
        /// Update the screen item.
        /// </summary>
        /// <param name="mouseState">Provide information about the mouse, and its buttons.</param>
        /// <param name="keyboardState">Provide information about the keyboard, and its keys.</param>
        public abstract void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState);
    }

    /// <summary>
    /// A class contains all the core functionality of a text 'label', that outputs text to the window
    /// </summary>
    public class Text : ScreenItem
    {
        // Define the variable to store the text to be displayed
        public string TextString { get; set; }
        // Define the variable to store the font to be displayed
        public SpriteFont font;
        // Define the coordinates for the text to start at
        public Vector2 Start { get; set; }
        // Define the colour of the text
        public Color Colour { get; set; }
        public Vector2 Origin { get; set; }
        public float Rotation { get; set; }
        public float Scale { get; set; }

        public Vector2 Size { get { return font.MeasureString(TextString); } }

        public bool doDraw = true;

        public void Show()
        {
            doDraw = true;
        }

        public void Hide()
        {
            doDraw = false;
        }

        /// <summary>
        /// Construct the text item
        /// </summary>
        /// <param name="outputText">The text to be outputted to the window.</param>
        /// <param name="textFont">The font object used to render the text.</param>
        /// <param name="textStart">The starting coordinates of the text.</param>
        /// <param name="textColour">The colour of the text.</param>
        public Text(string outputText, SpriteFont textFont, Vector2 textStart, Color textColour, Screen screen = null) : base(screen)
        {
            TextString = outputText;
            font = textFont;
            Start = textStart;
            Colour = textColour;
            Origin = Vector2.Zero;
            Rotation = 0;
            Scale = 1;
        }

        public Text(string outputText, SpriteFont textFont, Color textColour, Screen screen = null) : base(screen)
        {
            TextString = outputText;
            font = textFont;
            Start = Vector2.Zero;
            Colour = textColour;
            Origin = Vector2.Zero;
            Rotation = 0;
            Scale = 1;
        }

        public Text(string outputText, SpriteFont textFont, Screen screen = null) : base(screen)
        {
            TextString = outputText;
            font = textFont;
            Start = Vector2.Zero;
            Colour = Color.Black;
            Origin = Vector2.Zero;
            Rotation = 0;
            Scale = 1;
        }

        public Text(SpriteFont textFont, Color textColour, Screen screen = null) : base(screen)
        {
            TextString = "";
            font = textFont;
            Start = Vector2.Zero;
            Colour = textColour;
            Origin = Vector2.Zero;
            Rotation = 0;
            Scale = 1;
        }

        public Text(SpriteFont textFont, Screen screen = null) : base(screen)
        {
            TextString = "";
            font = textFont;
            Start = Vector2.Zero;
            Colour = Color.Black;
            Origin = Vector2.Zero;
            Rotation = 0;
            Scale = 1;
        }
        public Text(string outputText, SpriteFont textFont, Vector2 textStart, Color textColour, float scale, Vector2 origin, float rotation, Screen screen = null) : base(screen)
        {
            TextString = outputText;
            font = textFont;
            Start = textStart;
            Colour = textColour;
            Origin = origin;
            Rotation = rotation;
            Scale = scale;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the text to the window
            if (doDraw) spriteBatch.DrawString(font, TextString, Start, Colour, Rotation, Origin, Scale, SpriteEffects.None, 0);
        }

        public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState) { }
    }

    /// <summary>
    /// This class contains all the core functionality of a button in the window,
    /// that when pressed will run a set command (method).
    /// </summary>
    public class Button : ScreenItem
    {
        // Define the Text to be shown in the button
        public Text text;
        // Define the start point for the button
        public Vector2 start;
        // Define the colour of the button when the mouse is not hovering over it
        public Color standardColour;
        // Define the colour of the button for when the mouse is hovering over it
        public Color hoverColour;
        // Defint the border size (in pixels)
        int borderSize;
        // Defint the colour of the border
        public Color borderColour;
        // Define the command that the button will run after being pressed
        public Action command;
        // Define the ColouredRectangle objects that will store the parts of the button
        public ColouredRectangle rectangle;
        public ColouredRectangle borderRectangle;
        // Define the command control boolean variables (used to stop the command being run multiple times per button press)
        bool allowCommand = true;
        bool commandSwitch = false;

        /// <summary>
        /// Construct the button.
        /// </summary>
        /// <param name="buttonStart">The start coordinates of the button.</param>
        /// <param name="buttonStandardColour">The colour of the button (when the mouse is not hovering over it).</param>
        /// <param name="buttonHoverColour">The colour of the button (when the mouse if hovering over it).</param>
        /// <param name="buttonBorderSize">The size of the button's border (in pixels).</param>
        /// <param name="buttonBorderColour">The colour of the button's border.</param>
        /// <param name="buttonCommand">The command for the button to run when clicked.</param>
        public void SetStandardAttributes(Vector2 buttonStart, Color buttonStandardColour, Color buttonHoverColour, int buttonBorderSize, Color buttonBorderColour, Action buttonCommand)
        {
            // Construct the above defined values (this method refactors code from the actual contructors (as there are two of them))
            start = buttonStart;
            text.Start = start;
            standardColour = buttonStandardColour;
            hoverColour = buttonHoverColour;
            borderSize = buttonBorderSize;
            borderColour = buttonBorderColour;
            command = buttonCommand;

            // Get the text size (in pixels) from the text item 
            Vector2 textSize = text.font.MeasureString(text.TextString);
            // Get the size that the inner rectangle will be
            Vector2 size = new Vector2(textSize.X + 4, textSize.Y);
            // Construct the rectangles
            rectangle = new ColouredRectangle(start, size, standardColour);
            borderRectangle = new ColouredRectangle(new Vector2(rectangle.Left - borderSize, rectangle.Top - borderSize), new Vector2(rectangle.Width + (2 * borderSize), rectangle.Height + (2 * borderSize)), borderColour);
        }

        /// <summary>
        /// Construct the button with a pre-initialized text item.
        /// </summary>
        /// <param name="textItem">The text item to be drawn on top of the button.</param>
        public Button(Text textItem, Vector2 buttonStart, Color buttonStandardColour, Color buttonHoverColour, int buttonBorderSize, Color buttonBorderColour, Action buttonCommand, Screen screen = null) : base(screen)
        {
            text = textItem;
            SetStandardAttributes(buttonStart, buttonStandardColour, buttonHoverColour, buttonBorderSize, buttonBorderColour, buttonCommand);
        }

        /// <summary>
        /// Construct the button, automatically creating the text item.
        /// </summary>
        /// <param name="textString">The text to be displayed on the button.</param>
        /// <param name="font">The font object to render the text.</param>
        /// <param name="textColour">The colour of the text.</param>
        public Button(string textString, SpriteFont font, Color textColour, Vector2 buttonStart, Color buttonStandardColour, Color buttonHoverColour, int buttonBorderSize, Color buttonBorderColour, Action buttonCommand, Screen screen = null) : base(screen)
        {
            // Create a text item with the requested text information
            text = new Text(textString, font, new Vector2(0, 0), textColour);
            SetStandardAttributes(buttonStart, buttonStandardColour, buttonHoverColour, buttonBorderSize, buttonBorderColour, buttonCommand);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the border rectangle,
            borderRectangle.Draw(spriteBatch);
            // then the inner rectangle,
            rectangle.Draw(spriteBatch);
            // and finally the text
            text.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
        {
            // Update the text (this does nothing but keeps things consistent)
            text.Update(gameTime, mouseState, keyboardState);

            // Create local variables (for ease of programming) for the mouse's current position
            int mouseX = mouseState.X;
            int mouseY = mouseState.Y;
            // Get the state of the left mouse button
            ButtonState leftButtonState = mouseState.LeftButton;
            // Create a local variable to store a boolean version of the state of the left button
            bool leftButtonDown = false;
            if (leftButtonState == ButtonState.Pressed) { leftButtonDown = true; }

            // is the mouse within the X and Y boundaries of the inner rectangle
            bool mouseInRectangleX = ((rectangle.Left < mouseX) && (mouseX < rectangle.Right));
            bool mouseInRectangleY = ((rectangle.Top < mouseY) && (mouseY < rectangle.Bottom));

            // Check if the command should be run
            if (leftButtonDown && commandSwitch == false) { allowCommand = true; commandSwitch = true; }
            else if (leftButtonDown) { allowCommand = false; commandSwitch = true; }
            else { allowCommand = false; commandSwitch = false; }

            // Set the colour of the rectangle if the mouse is within the dimensions of the rectangle
            if (mouseInRectangleX && mouseInRectangleY)
            {
                rectangle.Colour = hoverColour;
                if (allowCommand) command();
            }
            else
            {
                rectangle.Colour = standardColour;
            }
        }
    }

    /// <summary>
    /// This class contains all the core attributes to create useful, renderable rectangles
    /// that can be drawn to the screen.
    /// </summary>
    public class ColouredRectangle : ScreenItem
    {
        // Define the texture for the rectangle
        Texture2D rect;
        // Define the start coordinates of the rectangle
        public Vector2 coordinates;
        // Define the size of the rectangle
        private Vector2 size;
        public Vector2 Size { get { return size; } set { size = value; RenderRect(); } }
        // Define the colour of the rectangle
        private Color colour;
        public Color Colour { get { return colour; } set { colour = value; RenderRect(); } }

        /// <summary>
        /// Construct the coloured rectangle.
        /// </summary>
        /// <param name="graphics">The graphics device manager.</param>
        /// <param name="startCoords">The start coordinates of the rectangle.</param>
        /// <param name="rectSize">The size of the rectangle.</param>
        /// <param name="rectColour">The colour of the rectangle.</param>
        public ColouredRectangle(Vector2 startCoords, Vector2 rectSize, Color rectColour)
        {
            // Construct the above variables
            coordinates = startCoords;
            size = rectSize;
            rect = new Texture2D(Shmup.Graphics.GraphicsDevice, (int)size.X, (int)size.Y);
            colour = rectColour;
            RenderRect();
        }

        public void RenderRect()
        {

            Color[] data = new Color[(int)(size.X * size.Y)];
            for (int i = 0; i < data.Length; ++i) data[i] = colour;
            rect.SetData(data);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the rectangle
            spriteBatch.Draw(rect, coordinates, Color.White);
        }
        public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState) { }

        public float X { get { return coordinates.X; } set { coordinates.X = value; } }
        public float Y { get { return coordinates.Y; } set { coordinates.Y = value; } }
        public float Width { get { return size.X; } }
        public float Height { get { return size.Y; } }

        // Define variaous properties which are calculated on request
        public float Left { get { return X; } set { X = value; } }
        public float Right { get { return X + Width; } set { X = value - Width; } }
        public float Top { get { return Y; } set { Y = value; } }
        public float Bottom { get { return Y + size.Y; } set { Y = value - Height; } }
        public float CentreX { get { return X + (Width / 2); } set { X = value - (Width / 2); } }
        public float CentreY { get { return Y + (Height / 2); } set { Y = value - (Height / 2); } }
        public Vector2 Centre { get { return new Vector2(CentreX, CentreY); } set { CentreX = value.X; CentreY = value.Y; } }
    }

    /// <summary>
    /// This class contains all the functionality of a key listener,
    /// it will listen for when a (defined) key is pressed, and run
    /// a command. It has an attribute (bool singleTrigger) that will
    /// stop the defined command from being run every update (spammed)
    /// until the key is next pressed.
    /// </summary>
    public class KeyListener : ScreenItem
    {
        // The key to be listened for
        public Keys key;
        // The command to be run if the key is being pressed
        public Action command;
        // If the command should be run once, of if the command should be run once per update
        public bool singleTrigger;

        // Command control variables (stop the command being spammed once per update if requested)
        bool keyIsDown = false;
        bool allowCommand = true;
        bool commandSwitch = false;

        public bool IsKeyDown { get { return keyIsDown; } }

        /// <summary>
        /// Construct the key listener.
        /// </summary>
        /// <param name="keyToListen">The key to listen for.</param>
        /// <param name="commandOnKeyPress">The method (action) to be run upon keypress.</param>
        /// <param name="triggerOnce">True if the command is only to be run once per keypress, false if the command is to be run once per update while the key is down.</param>
        public KeyListener(Keys keyToListen, Action commandOnKeyPress, bool triggerOnce = true)
        {
            key = keyToListen;
            command = commandOnKeyPress;
            singleTrigger = triggerOnce;
        }

        public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
        {
            // Get the current state of the key
            keyIsDown = keyboardState.IsKeyDown(key);

            // Check if the command can be run
            if (!singleTrigger && keyIsDown) { allowCommand = true; }
            else if (keyIsDown && commandSwitch == false) { allowCommand = true; commandSwitch = true; }
            else if (keyIsDown) { allowCommand = false; commandSwitch = true; }
            else { allowCommand = false; commandSwitch = false; }

            // If the command, can be run, run it
            if (allowCommand) command();
        }

        public override void Draw(SpriteBatch spriteBatch) { }
    }

    public class CheckBox : ScreenItem
    {
        public Text label;
        public ColouredRectangle outlineRectangle;
        public ColouredRectangle innerRectangle;
        public ColouredRectangle checkRectangle;
        public Color checkedColour;
        public Color hoverColour;
        public bool Checked { get; set; }

        public CheckBox(Text labelText, bool startValue, int borderSize, int gapSize, int boxSize, Vector2 startPos, Color borderColour, Color checkedButtonColour, Color checkHoverColour, Color backgroundColour, Screen screen) : base(screen)
        {
            label = labelText;
            Checked = startValue;

            checkedColour = checkedButtonColour;
            hoverColour = checkHoverColour;
            
            outlineRectangle = new ColouredRectangle(startPos, new Vector2(boxSize), borderColour);
            innerRectangle = new ColouredRectangle(new Vector2(startPos.X + borderSize, startPos.Y + borderSize), new Vector2(boxSize - (2 * borderSize)), backgroundColour);
            checkRectangle = new ColouredRectangle(new Vector2(startPos.X + borderSize + gapSize, startPos.Y + borderSize + gapSize), new Vector2(boxSize - (2 * borderSize) - (2 * gapSize)), checkedButtonColour);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            label.Draw(spriteBatch);
            outlineRectangle.Draw(spriteBatch);
            innerRectangle.Draw(spriteBatch);
            if (Checked) checkRectangle.Draw(spriteBatch);
        }

        bool checkSwitch = false;
        public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
        {
            label.Start = new Vector2(outlineRectangle.Left - label.Size.X - 5, outlineRectangle.CentreY - (label.Size.Y / 2));
            bool mouseInCheckBox = outlineRectangle.Left < mouseState.X && mouseState.X < outlineRectangle.Right && outlineRectangle.Top < mouseState.Y && mouseState.Y < outlineRectangle.Bottom;
            bool leftMouseDown = mouseState.LeftButton == ButtonState.Pressed;
            checkRectangle.Colour = mouseInCheckBox ? hoverColour : checkedColour;
            if (mouseInCheckBox && leftMouseDown && checkSwitch) { Checked ^= true; checkSwitch = false; }
            else if (!leftMouseDown) checkSwitch = true;
        }
    }

    public class Slider : ScreenItem
    {
        public bool isVisible = true;
        public Text text;
        Text valueText;
        public Vector2 start;
        Vector2 lineSize;
        public Vector2 size;

        float leftTextOffset;
        float rightTextOffset;

        Vector2 mousePos;
        bool leftButtonDown;

        ColouredRectangle sliderLineRectangle;
        ColouredRectangle sliderButtonRectangle;

        Color buttonColour;
        Color buttonHoverColour;
        Color buttonPressColour;
        Color lineColour;
        Color activeColour;

        bool clickedOn = false;
        bool clickedOff = false;

        float ratio;
        float min;
        float max;
        float range;
        float value;
        int valueRound;

        public float Ratio { get { return ratio; } }
        public float Value { get { return value; } }

        public Slider(Vector2 sliderStart, Vector2 sliderLineSize, Vector2 sliderButtonSize, float minValue, float maxValue, Color sliderButtonColour, Color sliderButtonHoverColour, Color sliderButtonPressColour, Color sliderLineColour, Text displayText = null, float initialValue = 0, int displayRound = 2, bool showValueOnRight = true, float textGapLeft = 0, float textGapRight = 0, Screen screen = null) : base(screen)
        {
            start = sliderStart;
            lineSize = sliderLineSize;
            size = sliderButtonSize;
            if (displayText != null) text = displayText;
            if (displayText != null && showValueOnRight) valueText = new Text(text.font);

            buttonColour = sliderButtonColour;
            buttonHoverColour = sliderButtonHoverColour;
            buttonPressColour = sliderButtonPressColour;
            lineColour = sliderLineColour;
            activeColour = buttonColour;

            leftTextOffset = textGapLeft;
            rightTextOffset = textGapRight;

            sliderLineRectangle = new ColouredRectangle(start, lineSize, lineColour);
            sliderButtonRectangle = new ColouredRectangle(new Vector2(0, 0), size, activeColour);
            sliderButtonRectangle.Centre = new Vector2(sliderLineRectangle.Left, sliderLineRectangle.CentreY);

            min = minValue;
            max = maxValue;
            range = max - min;
            SetSliderValue(initialValue);

            valueRound = displayRound;

            mousePos = new Vector2(0, 0);
            leftButtonDown = false;
        }

        public void SetSliderValue(float newValue)
        {
            if (newValue < min) newValue = min;
            else if (newValue > max) newValue = max;
            value = newValue;

            ratio = (newValue - min) / range;
            sliderButtonRectangle.CentreX = (ratio * sliderLineRectangle.Width) + sliderLineRectangle.Left;
        }

        public void SetSliderRatio(float newRatio)
        {
            ratio = newRatio;
            sliderButtonRectangle.CentreX = (newRatio * sliderLineRectangle.Width) + sliderLineRectangle.Left;

            value = min + (newRatio * range);
        }

        public void Show()
        {
            isVisible = true;
        }

        public void Hide()
        {
            isVisible = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (isVisible)
            {
                sliderLineRectangle.Draw(spriteBatch);
                sliderButtonRectangle.Draw(spriteBatch);
                if (text != null) text.Draw(spriteBatch);
                if (valueText != null) valueText.Draw(spriteBatch);
            }
        }

        public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
        {
            mousePos = new Vector2(mouseState.X, mouseState.Y);
            ButtonState leftButtonState = mouseState.LeftButton;
            if (leftButtonState == ButtonState.Pressed) leftButtonDown = true;
            else leftButtonDown = false;

            UpdateData();
            UpdatePosition(mousePos, leftButtonDown);
            UpdateColour(mousePos, leftButtonDown);
        }

        public void UpdateData()
        {
            ratio = (sliderButtonRectangle.CentreX - sliderLineRectangle.Left) / sliderLineRectangle.Width;
            value = min + (ratio * range);
        }

        public void UpdatePosition(Vector2 mousePos, bool leftButtonDown)
        {
            bool mouseInRectangleX = ((sliderButtonRectangle.Left < mousePos.X) && (mousePos.X < sliderButtonRectangle.Right));
            bool mouseInRectangleY = ((sliderButtonRectangle.Top < mousePos.Y) && (mousePos.Y < sliderButtonRectangle.Bottom));
            sliderButtonRectangle.CentreY = sliderLineRectangle.CentreY;
            if (mouseInRectangleX && mouseInRectangleY && leftButtonDown && !clickedOn) clickedOn = true;
            else if (leftButtonDown && !clickedOn) clickedOff = true;
            else if (!leftButtonDown && clickedOff) clickedOff = false;
            else if (clickedOn && !leftButtonDown) clickedOn = false;
            else if (clickedOn && !clickedOff) sliderButtonRectangle.CentreX = mousePos.X;
            if (sliderButtonRectangle.CentreX < sliderLineRectangle.Left) sliderButtonRectangle.CentreX = sliderLineRectangle.Left;
            else if (sliderButtonRectangle.CentreX > sliderLineRectangle.Right) sliderButtonRectangle.CentreX = sliderLineRectangle.Right;

            if (text != null) text.Start = new Vector2(sliderLineRectangle.Left - (text.Size.X + sliderButtonRectangle.Size.X + leftTextOffset), sliderLineRectangle.CentreY - (text.Size.Y / 2));
            if (valueText != null)
            {
                valueText.TextString = string.Format("{0}", Math.Round(Value, valueRound));
                valueText.Start = new Vector2(sliderLineRectangle.Right + sliderButtonRectangle.Size.X + rightTextOffset, sliderLineRectangle.CentreY - (valueText.Size.Y / 2));
            }
        }

        public void UpdateColour(Vector2 mousePos, bool leftButtonDown)
        {
            // is the mouse within the X and Y boundaries of the inner rectangle
            bool mouseInRectangleX = ((sliderButtonRectangle.Left < mousePos.X) && (mousePos.X < sliderButtonRectangle.Right));
            bool mouseInRectangleY = ((sliderButtonRectangle.Top < mousePos.Y) && (mousePos.Y < sliderButtonRectangle.Bottom));

            // Set the colour of the rectangle if the mouse is within the dimensions of the rectangle
            if (mouseInRectangleX && mouseInRectangleY)
            {
                if (leftButtonDown) sliderButtonRectangle.Colour = buttonHoverColour;
                else sliderButtonRectangle.Colour = buttonPressColour;
            }
            else sliderButtonRectangle.Colour = buttonColour;
        }
    }

    public class Score : ScreenItem
    {
        public Vector2 Position { get; private set; }
        private int Value { get; set; }
        public Text text { get; private set; }
        public string Label { get; set; }
        private readonly int initialScore;

        public Score(Vector2 startPos, SpriteFont font, Color textColour, string label = "Score: ", int startScore = 0, Screen screen = null) : base(screen)
        {
            Label = label;
            Position = startPos;
            Value = startScore;
            initialScore = startScore;
            text = new Text(font, textColour);
        }

        public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
        {
            text.TextString = string.Format("{0}{1}", Label, Value);
            text.Start = Position;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            text.Draw(spriteBatch);
        }

        public void Bump(int amount = 1)
        {
            Value += amount;
        }

        public void Set(int value)
        {
            Value = value;
        }

        public void ReSet()
        {
            Value = initialScore;
        }
    }

    public class HUD : ScreenItem
    {
        public Text GameInfoText { get; private set; }
        public Text FPSOutputText { get; private set; }
        public Text PlayerShootModeText { get; private set; }
        public Text PlayerHealthText { get; private set; }
        public Score PlayerScore { get; private set; }

        public HUD(Screen screen = null) : base(screen)
        {
            GameInfoText = new Text(string.Format("Shmup V{0}", Program.VERSION), Fonts.Arial, Color.LightCoral);
            FPSOutputText = new Text("FPS_COUNTER", Fonts.Arial, Color.White);
            PlayerShootModeText = new Text("PLAYER_SHOOT_MODE", Fonts.Arial, Color.White);
            PlayerHealthText = new Text("PLAYER_HEALTH", Fonts.Arial, Color.White);
            PlayerScore = new Score(new Vector2(200, 0), Fonts.Arial, Color.White);
        }

        public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
        {
            FPSOutputText.TextString = string.Format("FPS: {0}", Math.Round(Shmup.FrameCounter.CurrentFramesPerSecond), 2);
            FPSOutputText.Start = new Vector2(Shmup.Width - FPSOutputText.Size.X - 10, 0);

            switch (World.player.FireMode)
            {
                case Player.ShootMode.SemiAutomatic: PlayerShootModeText.TextString = "Semi-Automatic"; break;
                case Player.ShootMode.BurstFire: PlayerShootModeText.TextString = "Burst Fire"; break;
                case Player.ShootMode.Automatic: PlayerShootModeText.TextString = "Automatic"; break;
            }
            PlayerShootModeText.Start = new Vector2(Shmup.Width - PlayerShootModeText.Size.X, Shmup.Height - PlayerShootModeText.Size.Y - PlayerHealthText.Size.Y);
            for (int i = 0; i < World.fireModes.Length; i++) World.fireModes[i].position = new Vector2(Shmup.Width - (i * 5) - 5, PlayerShootModeText.Start.Y - 5);

            PlayerHealthText.TextString = string.Format("{0}", World.player.Health);
            PlayerHealthText.Start = new Vector2(Shmup.Width - PlayerHealthText.Size.X, Shmup.Height - PlayerHealthText.Size.Y);
            for (int i = 0; i < World.playerHealths.Length; i++) World.playerHealths[i].position = new Vector2(PlayerHealthText.Start.X - (i * 15) - 15, PlayerHealthText.Start.Y + World.playerHealths[i].offset.Y);
            PlayerScore.Set(Player.CurrentScore);
            PlayerScore.Update(gameTime, mouseState, keyboardState);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            GameInfoText.Draw(spriteBatch);
            FPSOutputText.Draw(spriteBatch);
            PlayerShootModeText.Draw(spriteBatch);
            PlayerHealthText.Draw(spriteBatch);
            PlayerScore.Draw(spriteBatch);
        }
    }
}
