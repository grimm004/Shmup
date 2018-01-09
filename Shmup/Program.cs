using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Drawing;

namespace Shmup
{
    static class Program
    {
        public const string VERSION = "1.1.2";

        /// <summary>
        /// Application Entry Point
        /// </summary>
        static void Main(string[] args)
        {
            if (File.Exists(Constants.CONFIGFILE))
            {
                SettingsManager.LoadSettingsFile(Constants.CONFIGFILE);

                using (Shmup shmup = new Shmup())
                {
                    shmup.Run();
                }
            }
            else
            {
                DialogResult result = MessageBox.Show("Could not find config file. Create new one?", "Config Error", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    CreateDefaultSettingsFile();
                    MessageBox.Show("Config file has been created.", "Config File");
                }
            }
        }

        /// <summary>
        /// Create the default settings file
        /// </summary>
        /// <returns>true if successful</returns>
        static bool CreateDefaultSettingsFile()
        {
            Rectangle screenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;

            Settings settings = new Settings();
            settings.multiplayerServerHost = "127.0.0.1";
            settings.multiplayerServerPort = 8000;
            settings.targetFps = 60;
            settings.maxLaserCount = 50;
            settings.starCount = 2500;
            settings.fullScreen = false;
            settings.asteroidCount = 50;
            settings.width = screenBounds.Width / 2;
            settings.height = screenBounds.Height / 2;
            settings.isMouseVisible = true;
            settings.fireKey = Keys.Space;
            settings.changeFireModeKey = Keys.F;
            settings.forwardThrustKey = Keys.W;
            settings.backwardsThrustKey = Keys.S;
            settings.turnLeftKey = Keys.A;
            settings.turnRightKey = Keys.D;
            SettingsManager.Current = settings;
            SettingsManager.CreateSettingsFile(Constants.CONFIGFILE);
            return true;
        }
    }

    /// <summary>
    /// A class that contains some core constants for the game's mechanics
    /// </summary>
    public class Constants
    {
        // Updates per second to try and change timings for
        public const int MAXFPS = 300;
        public const float TIMEDELTACONSTANT = 75;

        public const int MAXBULLETS = 25;
        public const int MAXBULLETSPERSECOND = 2;

        public const int PLAYER_MAX_HEALTH = 10;

        // Config file name
        public const string CONFIGFILE = "Shmup\\config.xml";
    }

    /// <summary>
    /// A class that manages the settings
    /// </summary>
    public class SettingsManager
    {
        private static Settings settings;

        public static Settings Current { get { return settings; } set { settings = value; } }

        /// <summary>
        /// Creates the settings file
        /// </summary>
        /// <param name="fileName">The name of the file</param>
        public static void CreateSettingsFile(string fileName)
        {
            string path = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            XmlSerializer xmlSerializer = new XmlSerializer(settings.GetType());
            XmlWriterSettings xmlSettings = new XmlWriterSettings();

            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "\t";

            using (var writer = XmlWriter.Create(fileName, xmlSettings)) xmlSerializer.Serialize(writer, settings);
        }

        /// <summary>
        /// Load an xml-serialized settings file
        /// </summary>
        /// <param name="fileName"></param>
        public static void LoadSettingsFile(string fileName)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings));

            using (var reader = XmlReader.Create(fileName))
            {
                settings = (Settings)xmlSerializer.Deserialize(reader);
            }
        }
    }

    /// <summary>
    /// A class containing all the changeable settings
    /// </summary>
    public class Settings
    {
        public int width;
        public int height;
        public bool fullScreen;
        public int targetFps;
        public bool isMouseVisible;
        public string multiplayerServerHost;
        public int multiplayerServerPort;
        public int starCount;
        public int maxLaserCount;
        public int asteroidCount;
        public Keys fireKey;
        public Keys changeFireModeKey;
        public Keys forwardThrustKey;
        public Keys backwardsThrustKey;
        public Keys turnLeftKey;
        public Keys turnRightKey;
    }
}