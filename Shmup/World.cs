using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Shmup
{
    /// <summary>
    /// A class that represents and manages the shmup world and its entites
    /// </summary>
    public class World
    {
        public bool debugPositions = false;

        public static Random random;

        public static SpriteSheet spriteSheet;

        public static Player player { get; private set; }
        public static List<Asteroid> asteroids { get; private set; }
        public static List<Star> Stars { get; set; }
        public static Explosion[] explosions { get; private set; }
        public static PlayerHealth[] playerHealths { get; private set; }
        public static FireMode[] fireModes { get; private set; }
        public static Enemy[] enemies { get; private set; }

        ColouredRectangle[] hitPointVisuals;

        private static List<Entity> Entities;

        /// <summary>
        /// Instantiate the world
        /// </summary>
        /// <param name="spriteSheetInstance">The current instance of the sprite sheet</param>
        /// <param name="starCount">The number of stars to render</param>
        /// <param name="maxLaserCount">The number of lasers to be created in memory</param>
        /// <param name="asteroidCount">The number of asteroids to be created</param>
        public World(SpriteSheet spriteSheetInstance, int starCount, int maxLaserCount, int asteroidCount)
        {
            spriteSheet = spriteSheetInstance;

            random = new Random();

            Entities = new List<Entity>();

            GenerateStars(starCount);

            player = new Player(Constants.PLAYER_MAX_HEALTH);
            foreach (Laser laser in player.Lasers) AddEntity(laser);
            AddEntity(player);

            foreach (Explosion explosion in player.Explosions) AddEntity(explosion);

            enemies = new Enemy[random.Next(2, 6)];
            for (int i = 0; i < enemies.Length; i++) enemies[i] =
                    new Enemy(SpriteSheet.Sprite.Enemy,
                    new Vector2(random.Next(0, Shmup.Width),
                    random.Next(0, Shmup.Height)), true,
                    random.Next(1, 5), 3);
            foreach (Enemy enemy in enemies) AddEntity(enemy);

            foreach (Enemy enemy in enemies)
            {
                foreach (Explosion explosion in enemy.Explosions) AddEntity(explosion);
                foreach (Laser laser in enemy.Lasers) AddEntity(laser);
            }

            playerHealths = new PlayerHealth[Constants.PLAYER_MAX_HEALTH];
            for (int i = 0; i < playerHealths.Length; i++) playerHealths[i] = new PlayerHealth(SpriteSheet.Sprite.SmallShip, i, new Vector2(Shmup.Width - (i * 15), Shmup.Height - spriteSheet.GetSpriteRectangle(SpriteSheet.Sprite.SmallShip).Height));
            foreach (PlayerHealth playerHealth in playerHealths) AddEntity(playerHealth);
            
            fireModes = new FireMode[10];
            for (int i = 0; i < fireModes.Length; i++) fireModes[i] = new FireMode(i, new Vector2());
            foreach (FireMode fireMode in fireModes) AddEntity(fireMode);

            asteroids = new List<Asteroid>();
            for (int i = 0; i < asteroidCount; i++) asteroids.Add(new Asteroid(SpriteSheet.AsteroidSprites[random.Next(SpriteSheet.AsteroidSprites.Length)], new Vector2(random.Next(0, Shmup.Width), random.Next(0, Shmup.Height)), random.Next(0, 360), (float)random.Next(5, 15) / 10, random.Next(-20, 21) / 10));
            AddEntities(asteroids);

            hitPointVisuals = new ColouredRectangle[player.CollisionPoints.Length + 1 + 4 * enemies.Length];
            for (int i = 0; i < hitPointVisuals.Length; i++) hitPointVisuals[i] = new ColouredRectangle(Vector2.Zero, new Vector2(5), Color.Green);
            hitPointVisuals[player.CollisionPoints.Length].Colour = Color.Red;
        }

        /// <summary>
        /// Generate the stars
        /// </summary>
        /// <param name="starCount">The number of stars to be created</param>
        public static void GenerateStars(int starCount)
        {
            if (Stars != null) foreach (Star star in Stars) RemoveEntity(star);
            Stars = new List<Star>();
            for (int i = 0; i < starCount; i++)
                Stars.Add(new Star(SpriteSheet.StarSprites[random.Next(SpriteSheet.StarSprites.Length)], new Vector2(random.Next(Shmup.Width), random.Next(Shmup.Height))));
            foreach (Star star in Stars) AddEntityAt(star, 0);
        }

        /// <summary>
        /// Run and handle the collision detection
        /// </summary>
        private void CollisionDetection()
        {
            foreach (Asteroid asteroid in asteroids)
            {
                if (asteroid.Alive)
                {
                    foreach (Vector2 point in player.CollisionPoints)
                        if (Entity.PointIntersects(point, asteroid.CollisionRectangle))
                        {
                            player.Hit();
                        }
                    foreach (Laser laser in player.Lasers) foreach (Vector2 point in laser.CollisionPoints)
                            if (Entity.PointIntersects(point, asteroid.CollisionRectangle))
                            {
                                laser.Hit();
                                asteroid.Destroy();
                                Player.CurrentScore++;
                            }
                }
            }

            foreach (Enemy enemy in enemies)
            {
                if (enemy.Alive)
                {
                    foreach (Vector2 point in player.CollisionPoints)
                    {
                        if (player.Alive)
                            if (Entity.PointIntersects(point, enemy.CollisionRectangle))
                            {
                                player.Hit();
                                enemy.Hit();
                            }
                    }

                    foreach (Laser laser in player.Lasers) foreach (Vector2 point in laser.CollisionPoints)
                            if (Entity.PointIntersects(point, enemy.CollisionRectangle))
                            {
                                laser.Hit();
                                enemy.Hit();
                                Player.CurrentScore++;
                            }
                }
            }
        }

        /// <summary>
        /// Kill the player
        /// </summary>
        public void KillPlayer()
        {
            player.Destroy();
        }
        
        /// <summary>
        /// Update the world and its entites
        /// </summary>
        private int flickerCount = 0;
        private bool debugSwitch = true;
        public void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.P) && debugSwitch) { debugPositions ^= true; debugSwitch = false; }
            else if (!Keyboard.GetState().IsKeyDown(Keys.P)) debugSwitch = true;
            if (0 == flickerCount++) Stars[random.Next(Stars.Count)].doDraw ^= true;
            flickerCount %= 20;
            foreach (Entity entity in Entities) entity.Update(gameTime);
            if (debugPositions)
            {
                int currentIndex = 0;
                for (int i = 0; i < player.CollisionPoints.Length; i++) hitPointVisuals[currentIndex].coordinates = player.CollisionPoints[currentIndex++];
                hitPointVisuals[currentIndex++].coordinates = player.position;
                for (int i = 0; i < enemies.Length; i++)
                {
                    RotatedRectangle newRectangle = (enemies[i].rotatedRectangle + enemies[i].position);
                    hitPointVisuals[currentIndex++].coordinates = newRectangle.TopLeft;
                    hitPointVisuals[currentIndex++].coordinates = newRectangle.TopRight;
                    hitPointVisuals[currentIndex++].coordinates = newRectangle.BottomLeft;
                    hitPointVisuals[currentIndex++].coordinates = newRectangle.BottomRight;
                }
            }
            CollisionDetection();
        }

        /// <summary>
        /// Draw the world and its entites
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (Entity entity in Entities) entity.Draw(gameTime, spriteBatch, spriteSheet);
            if (debugPositions) foreach (ColouredRectangle hitPointVisual in hitPointVisuals) hitPointVisual.Draw(spriteBatch);
        }

        #region Entity Management Utilities
        public static void AddEntityAt(Entity entity, int index)
        {
            Entities.Insert(index, entity);
        }
        public static void AddEntitiesAt(IEnumerable<Entity> entities, int index)
        {
            Entities.InsertRange(index, entities);
        }
        public static void AddEntities(IEnumerable<Entity> entities)
        {
            Entities.AddRange(entities);
        }
        public static void AddEntity(Entity entity)
        {
            Entities.Add(entity);
        }
        public static void RemoveEntity(Entity entity)
        {
            Entities.Remove(entity);
        }
        public static void RemoveEntityAt(int index)
        {
            Entities.RemoveAt(index);
        }
        #endregion
    }
}
