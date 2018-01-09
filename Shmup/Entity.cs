using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Shmup
{
    /// <summary>
    /// The abstract class that represents an entity that can be updated and rendered
    /// </summary>
    public abstract class Entity
    {
        public Vector2 startPosition;
        public Vector2 position;
        public Vector2 drawPosition;
        public Vector2 size;
        public Vector2 offset;
        public float direction;
        public float drawDirection;
        public bool doDraw = true;
        protected Rectangle collisionRectangle;
        private Vector2[] collisionPoints;
        public SpriteSheet.Sprite Sprite { get; private set; }
        public Rectangle SpriteRectangle { get; private set; }
        public float Scale { get; set; }
        public Rectangle CollisionRectangle
        {
            get
            {
                return new Rectangle((int)position.X + collisionRectangle.X, (int)position.Y + collisionRectangle.Y, collisionRectangle.Width, collisionRectangle.Height);
            }
        }

        public Vector2[] CollisionPoints
        {
            get
            {
                Vector2[] points = new Vector2[collisionPoints.Length];
                for (int i = 0; i < collisionPoints.Length; i++) points[i] = position + offset + (Matrix.Matrix.Rotation(MathHelper.ToRadians(direction + 90)) * Matrix.Matrix2.FromVector2(collisionPoints[i] - (size / 2))).ToVector2();
                return points;
            }
        }

        /// <summary>
        /// Constuct the entity with its initial sprite
        /// </summary>
        /// <param name="sprite">The sprite to render</param>
        public Entity(SpriteSheet.Sprite sprite)
        {
            position = Vector2.Zero;
            startPosition = Vector2.Zero;
            drawPosition = Vector2.Zero;
            direction = 0;
            Sprite = sprite;
            Scale = 1;
            collisionRectangle = new Rectangle();
            SpriteRectangle = World.spriteSheet.GetSpriteRectangle(Sprite);
            collisionRectangle = World.spriteSheet.GetSpriteCollisionRectanlge(Sprite);
            collisionPoints = World.spriteSheet.GetSpriteCollisionPoints(Sprite);
            size = new Vector2(SpriteRectangle.Width, SpriteRectangle.Height);
            offset = new Vector2(SpriteRectangle.Width / 2, SpriteRectangle.Height / 2);
        }

        /// <summary>
        /// Constructs an entity
        /// </summary>
        /// <param name="sprite">The sprite to render</param>
        /// <param name="startPosition">The initial position of the entity</param>
        /// <param name="startDirection">The initial direction of the entity</param>
        public Entity(SpriteSheet.Sprite sprite, Vector2 startPosition, float startDirection)
        {
            position = startPosition;
            drawPosition = position;
            startPosition = position;
            direction = startDirection;
            Sprite = sprite;
            Scale = 1;
            SpriteRectangle = World.spriteSheet.GetSpriteRectangle(Sprite);
            collisionRectangle = World.spriteSheet.GetSpriteCollisionRectanlge(Sprite);
            collisionPoints = World.spriteSheet.GetSpriteCollisionPoints(Sprite);
            size = new Vector2(SpriteRectangle.Width, SpriteRectangle.Height);
            offset = new Vector2(SpriteRectangle.Width / 2, SpriteRectangle.Height / 2);
        }

        /// <summary>
        /// Set the entity's sprite
        /// </summary>
        /// <param name="sprite"></param>
        public void SetSprite(SpriteSheet.Sprite sprite)
        {
            Sprite = sprite;
            SpriteRectangle = World.spriteSheet.GetSpriteRectangle(Sprite);
            collisionRectangle = World.spriteSheet.GetSpriteCollisionRectanlge(Sprite);
            collisionPoints = World.spriteSheet.GetSpriteCollisionPoints(Sprite);
            size = new Vector2(SpriteRectangle.Width, SpriteRectangle.Height);
            offset = new Vector2(SpriteRectangle.Width / 2, SpriteRectangle.Height / 2);
        }

        /// <summary>
        /// Draw the entity
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="spriteSheet"></param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteSheet spriteSheet)
        {
            if (doDraw) spriteBatch.Draw(spriteSheet.Texture,
                drawPosition,
                SpriteRectangle,
                Color.White,
                MathHelper.ToRadians(drawDirection),
                offset, Scale, SpriteEffects.None, 1);
        }

        /// <summary>
        /// Check if a point intersects a rectangle
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <param name="box">The box to check for collision in</param>
        /// <returns>true if the point if the point intersects the box</returns>
        public static bool PointIntersects(Vector2 point, Rectangle box)
        {
            return box.Left < point.X && point.X < box.Right && box.Top < point.Y && point.Y < box.Bottom;
        }

        /// <summary>
        /// Update the entity's sprite
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update(GameTime gameTime);
    }

    /// <summary>
    /// A generic ship
    /// </summary>
    public abstract class Ship : Entity
    {
        public Ship(SpriteSheet.Sprite sprite) : base(sprite) { }
    }

    /// <summary>
    /// A player (soon to be changed to extend from type ship)
    /// </summary>
    public class Player : Entity
    {
        public const float ACCELERATION = 0.05f;
        public const float TURNRATE =     4.25f;
        public const float MAXVELOCITY =  5.00f;
        public const float MINVELOCITY =  0.00f;
        
        public static int CurrentScore { get; set; }
        public Laser[] Lasers { get; private set; }
        public Explosion[] Explosions { get; private set; }
        public bool doUpdate;
        public bool Alive { get; private set; }
        public ShootMode FireMode { get; private set; }
        public int Health { get; private set; }
        public float Velocity { get; private set; }

        /// <summary>
        /// Construct the player
        /// </summary>
        /// <param name="health">The maximum health of the player</param>
        public Player(int health) : base(SpriteSheet.Sprite.Ship)
        {
            Lasers = new Laser[SettingsManager.Current.maxLaserCount];
            for (int i = 0; i < Lasers.Length; i++) Lasers[i] = new Laser(SpriteSheet.Sprite.LaserGreen, 6f);
            FireMode = ShootMode.SemiAutomatic;
            Explosions = new Explosion[2];
            for (int i = 0; i < Explosions.Length; i++) Explosions[i] = new Explosion(SpriteSheet.ExplosionSprites);
            doUpdate = true;
            Alive = true;
            Health = health;
        }

        bool fireShot = false;
        bool fireSwitch = true;
        double shotTimer = 0;
        int shotCount = 0;
        bool allowShootModeChange = true;
        bool allowHit = true;
        double hitTimer = 0;
        Explosion activeExplosion;
        /// <summary>
        /// Update the player's state
        /// </summary>
        /// <param name="gameTime">The time information since the last update</param>
        public override void Update(GameTime gameTime)
        {
            if (doUpdate && Alive)
            {
                UpdateMovement(gameTime);
                UpdateInput(gameTime);
                UpdateLaser();
                UpdateHealth(gameTime);
            }
        }

        /// <summary>
        /// Move the player
        /// </summary>
        /// <param name="gameTime">The time information since the last update</param>
        public void UpdateMovement(GameTime gameTime)
        {
            if (Velocity > MAXVELOCITY) Velocity = MAXVELOCITY;
            else if (Velocity < MINVELOCITY) Velocity = MINVELOCITY;
            double timeDeltaMultiplier = gameTime.ElapsedGameTime.TotalSeconds * Constants.TIMEDELTACONSTANT;
            position.X += (float)(Velocity * timeDeltaMultiplier * Math.Cos(MathHelper.ToRadians(direction)));
            position.Y += (float)(Velocity * timeDeltaMultiplier * Math.Sin(MathHelper.ToRadians(direction)));
            drawPosition = position + offset;
            drawDirection = direction + 90;
            direction %= 360;
            drawDirection %= 360;

            if (position.X + offset.X < 0) position.X = Shmup.Width + offset.X;
            if (position.X - offset.X > Shmup.Width) position.X = 0 - offset.X;
            if (position.Y + offset.Y < 0) position.Y = Shmup.Height + offset.Y;
            if (position.Y - offset.Y > Shmup.Height) position.Y = 0 - offset.Y;
        }

        /// <summary>
        /// Handle user input
        /// </summary>
        /// <param name="gameTime">The time information since the last update</param>
        public void UpdateInput(GameTime gameTime)
        {
            float timeDeltaMultiplier = (float)((double)gameTime.ElapsedGameTime.TotalSeconds * (double)Constants.TIMEDELTACONSTANT);
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown((Keys)SettingsManager.Current.forwardThrustKey)) Velocity += ACCELERATION * timeDeltaMultiplier;
            if (keyboardState.IsKeyDown((Keys)SettingsManager.Current.backwardsThrustKey)) Velocity -= ACCELERATION * timeDeltaMultiplier;
            if (keyboardState.IsKeyDown((Keys)SettingsManager.Current.turnLeftKey)) direction -= TURNRATE * timeDeltaMultiplier;
            if (keyboardState.IsKeyDown((Keys)SettingsManager.Current.turnRightKey)) direction += TURNRATE * timeDeltaMultiplier;

            if (keyboardState.IsKeyDown((Keys)SettingsManager.Current.changeFireModeKey) && allowShootModeChange) ChangeShootMode();
            else if (!keyboardState.IsKeyDown((Keys)SettingsManager.Current.changeFireModeKey)) allowShootModeChange = true;
            else allowShootModeChange = false;

            fireShot = false;
            if (keyboardState.IsKeyDown((Keys)SettingsManager.Current.fireKey))
                switch (FireMode)
                {
                    case ShootMode.SemiAutomatic:
                        if (fireSwitch)
                        {
                            fireShot = true;
                            fireSwitch = false;
                        }
                        break;
                    case ShootMode.BurstFire:
                        shotTimer += gameTime.ElapsedGameTime.TotalSeconds;
                        if (shotTimer >= .05f && fireSwitch)
                        {
                            if (shotCount > 3) fireSwitch = false;
                            shotTimer = 0;
                            shotCount++;
                            fireShot = true;
                        }
                        break;
                    case ShootMode.Automatic:
                        shotTimer += gameTime.ElapsedGameTime.TotalSeconds;
                        if (shotTimer >= .2f)
                        {
                            shotTimer = 0;
                            fireShot = true;
                        }
                        break;
                }
            else
            {
                shotCount = 0;
                fireSwitch = true;
            }
        }

        /// <summary>
        /// Update the player's lasers
        /// </summary>
        public void UpdateLaser()
        {
            if (fireShot) foreach (Laser laser in Lasers)
                if (laser.canFire)
                {
                    laser.Fire(position + ((size - new Vector2(0, size.Y)) / 2), direction);
                    break;
                }
        }

        /// <summary>
        /// Update the player's health
        /// </summary>
        /// <param name="gameTime">The time information since the last update</param>
        public void UpdateHealth(GameTime gameTime)
        {
            if (registerHit && allowHit)
            {
                Health--;
                foreach (Explosion explosion in Explosions) if (explosion.CanExplode) { activeExplosion = explosion; break; }
                if (activeExplosion != null) activeExplosion.Explode();
                allowHit = false;
            }

            if (activeExplosion != null) activeExplosion.position = activeExplosion.drawPosition = position + offset;

            if (!allowHit)
            {
                if ((int)hitTimer % 100 == 0) doDraw ^= true;
                hitTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (hitTimer > 2000)
            {
                doDraw = true;
                allowHit = true;
                hitTimer = 0;
            }

            registerHit = false;

            if (Health <= 0)
            {
                Health = 0;
                activeExplosion.Scale = 2;
                Destroy();
            }
        }

        /// <summary>
        /// Change the player's shoot mode
        /// </summary>
        public void ChangeShootMode()
        {
            FireMode++;
            if (FireMode > ShootMode.Automatic) FireMode = ShootMode.SemiAutomatic;
            allowShootModeChange = false;
            fireShot = false;
            fireSwitch = true;
            shotTimer = 0;
            shotCount = 0;
        }

        /// <summary>
        /// Destroy (kill) the player
        /// </summary>
        public void Destroy()
        {
            doDraw = false;
            doUpdate = false;
            Alive = false;
        }
        bool registerHit = false;

        /// <summary>
        /// Register a player hit
        /// </summary>
        public void Hit()
        {
            registerHit = true;
        }

        /// <summary>
        /// Change the string representation of the player instance
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Player({0},{1},{2},{3},{4})", position, size, Velocity, direction, Health);
        }

        /// <summary>
        /// The shoot modes
        /// </summary>
        public enum ShootMode
        {
            SemiAutomatic,
            BurstFire,
            Automatic,
        }
    }

    /// <summary>
    /// An enemy object (to be changed to inherite from ship)
    /// </summary>
    public class Enemy : Entity
    {
        public bool FollowPlayer { get; set; }
        public Laser[] Lasers { get; private set; }
        public float Velocity { get; set; }
        public bool FollowingPlayer { get; private set; }
        public Explosion[] Explosions { get; private set; }
        public int Health { get; set; }
        private bool doUpdate;
        public bool Alive { get; private set; }
        public float PlayerDirection { get; set; }

        public RotatedRectangle rotatedRectangle { get; private set; }

        /// <summary>
        /// Construct the enemy
        /// </summary>
        /// <param name="sprite">The sprite to render</param>
        /// <param name="followPlayer">If the enemy should follow the player</param>
        /// <param name="health">The maximum health of the enemy</param>
        /// <param name="velocity">The maximum velocity of the enemy</param>
        public Enemy(SpriteSheet.Sprite sprite, bool followPlayer, int health, float velocity) : base(sprite)
        {
            FollowingPlayer = FollowPlayer = followPlayer;
            Explosions = new Explosion[2];
            for (int i = 0; i < Explosions.Length; i++) Explosions[i] = new Explosion(SpriteSheet.ExplosionSprites);
            Lasers = new Laser[10];
            for (int i = 0; i < Lasers.Length; i++) Lasers[i] = new Laser(SpriteSheet.Sprite.LaserRed, 6f);
            Health = health;
            doUpdate = true;
            Velocity = velocity;
            PlayerDirection = 0;

            Alive = true;

            rotatedRectangle = new RotatedRectangle(CollisionRectangle, MathHelper.ToRadians(drawDirection));
        }

        /// <summary>
        /// Construct the enemy
        /// </summary>
        /// <param name="sprite">The sprite to render</param>
        /// <param name="startPos">The initial position of the enemy</param>
        /// <param name="followPlayer">If the enemy should follow the player</param>
        /// <param name="health">The maximum health of the player</param>
        /// <param name="velocity">The maximum velocity of the player</param>
        public Enemy(SpriteSheet.Sprite sprite, Vector2 startPos, bool followPlayer, int health, float velocity) : base(sprite, startPos, 0)
        {
            FollowingPlayer = FollowPlayer = followPlayer;
            Explosions = new Explosion[2];
            for (int i = 0; i < Explosions.Length; i++) Explosions[i] = new Explosion(SpriteSheet.ExplosionSprites);
            Lasers = new Laser[10];
            for (int i = 0; i < Lasers.Length; i++) Lasers[i] = new Laser(SpriteSheet.Sprite.LaserRed, 6f);
            Health = health;
            doUpdate = true;
            Velocity = velocity;
            PlayerDirection = 0;

            Alive = true;

            rotatedRectangle = new RotatedRectangle(CollisionRectangle, MathHelper.ToRadians(drawDirection));
        }

        double turnCounter = 0;
        float turnTime = 0;
        int turnRate = 0;
        double breakTime = 0;
        double breakTimer = 0;
        bool allowHit = true;
        double hitTimer = 0;
        Explosion activeExplosion;
        /// <summary>
        /// Update the enemy's movement
        /// </summary>
        /// <param name="gameTime">A representation of the time since the last update</param>
        public override void Update(GameTime gameTime)
        {
            if (doUpdate)
            {
                UpdateMovement(gameTime);
                UpdateHit(gameTime);
                UpdateLasers(gameTime);
                UpdateHealth();
            }
        }

        double laserTimer = 0;
        /// <summary>
        /// Update the enemy's lasers
        /// </summary>
        /// <param name="gameTime">A representation of the time since the last update</param>
        public void UpdateLasers(GameTime gameTime)
        {
            laserTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (laserTimer > 1)
            {
                if (Math.Abs(PlayerDirection) - Math.Abs(direction) < 45) FireLaser();
                laserTimer = 0;
            }
        }
        
        /// <summary>
        /// Fire a laser
        /// </summary>
        public void FireLaser()
        {
            foreach (Laser laser in Lasers)
                if (laser.canFire)
                {
                    Matrix.Matrix mat = Matrix.Matrix.Rotation(MathHelper.ToRadians(direction));
                    Vector2 firePos = position + (mat * new Matrix.Matrix2(19, 0)).ToVector2();
                    laser.Fire(firePos, direction);
                    break;
                }
        }

        /// <summary>
        /// Update the enemy's movement
        /// </summary>
        /// <param name="gameTime">A representation of the time since the last update</param>
        public void UpdateMovement(GameTime gameTime)
        {
            float relitiveX = World.player.position.X + World.player.offset.X - position.X;
            float relitiveY = World.player.position.Y + World.player.offset.Y - position.Y;
            float relitiveMagnitude = (float)Math.Sqrt(Math.Pow(relitiveX, 2) + Math.Pow(relitiveY, 2));
            float tanDirection = MathHelper.ToDegrees((float)Math.Atan(relitiveY / relitiveX));
            PlayerDirection = relitiveX < 0 ? tanDirection + 180 : tanDirection;

            float timeDeltaMultiplier = (float)((double)gameTime.ElapsedGameTime.TotalSeconds * (double)Constants.TIMEDELTACONSTANT);

            if (FollowPlayer && FollowingPlayer)
            {
                turnCounter = 0;
                turnTime = 0;
                turnRate = 0;
                breakTime = 0;
                breakTimer = 0;
                if (direction < PlayerDirection) direction += timeDeltaMultiplier;
                else if (direction > PlayerDirection) direction -= timeDeltaMultiplier;
                if (relitiveMagnitude < 75)
                {
                    FollowingPlayer = false;
                    turnCounter = 0;
                    turnTime = 1;
                    turnRate = World.random.Next(0, 2) == 0 ? -1 : 1;
                    breakTime = turnTime + World.random.Next(2, 6);
                }
            }
            else if (FollowPlayer && !FollowingPlayer)
            {
                FollowingPlayer = breakTimer > breakTime;
                if (turnCounter < 2) direction += turnRate * timeDeltaMultiplier;
                turnCounter += gameTime.ElapsedGameTime.TotalSeconds;
                breakTimer += gameTime.ElapsedGameTime.TotalSeconds;
            }

            position.X += (float)(Velocity * timeDeltaMultiplier * Math.Cos(MathHelper.ToRadians(direction)));
            position.Y += (float)(Velocity * timeDeltaMultiplier * Math.Sin(MathHelper.ToRadians(direction)));
            drawPosition = position + offset;
            drawDirection = direction + 90;

            rotatedRectangle = new RotatedRectangle(CollisionRectangle, MathHelper.ToRadians(drawDirection));
        }

        /// <summary>
        /// Update the enemy's response to a hit
        /// </summary>
        /// <param name="gameTime">A representation of the time since the last update</param>
        public void UpdateHit(GameTime gameTime)
        {

            if (registerHit && allowHit)
            {
                Health--;
                foreach (Explosion explosion in Explosions) if (explosion.CanExplode) { activeExplosion = explosion; break; }
                if (activeExplosion != null) activeExplosion.Explode();
                allowHit = false;
            }

            if (activeExplosion != null) activeExplosion.position = activeExplosion.drawPosition = position + offset;

            if (!allowHit)
            {
                //if ((int)(1000 * hitTimer) % 100 == 0) doDraw ^= true;
                hitTimer += gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (hitTimer > 2)
            {
                doDraw = true;
                allowHit = true;
                hitTimer = 0;
            }

            registerHit = false;
        }

        bool registerHit = false;
        /// <summary>
        /// Register a hit
        /// </summary>
        public void Hit()
        {
            registerHit = true;
        }

        /// <summary>
        /// Update the enemy's health
        /// </summary>
        public void UpdateHealth()
        {
            if (Health <= 0)
            {
                Health = 0;
                activeExplosion.Scale = 2;
                Destroy();
            }
        }

        /// <summary>
        /// Destory (kill) the enemy
        /// </summary>
        public void Destroy()
        {
            doDraw = false;
            doUpdate = false;
            Alive = false;
        }
    }

    /// <summary>
    /// A class representing the asteroid entity
    /// </summary>
    public class Asteroid : Entity
    {
        public float Velocity { get; set; }
        public float clockwiseTurnRate;
        public bool doUpdate;
        public bool Alive { get; private set; }

        /// <summary>
        /// Construct the asteroid
        /// </summary>
        /// <param name="sprite">The sprite to render</param>
        /// <param name="startPosition">The initial position of the asteroid</param>
        /// <param name="startDirection">The initial direction of the asteroid</param>
        /// <param name="startVelocity">The initial velocity of the asteroid</param>
        /// <param name="turnRate">The turn rate of the asteroid</param>
        public Asteroid(SpriteSheet.Sprite sprite, Vector2 startPosition, float startDirection, float startVelocity, float turnRate = 1) : base((SpriteSheet.Sprite)sprite, startPosition, startDirection)
        {
            Velocity = startVelocity;
            clockwiseTurnRate = turnRate;
            doUpdate = true;
            Alive = true;
        }

        /// <summary>
        /// Destroy the asteroid
        /// </summary>
        public void Destroy()
        {
            doDraw = false;
            doUpdate = false;
            Alive = false;
        }

        /// <summary>
        /// Update the asteroid's state
        /// </summary>
        /// <param name="gameTime">A representation of the time since the last update</param>
        public override void Update(GameTime gameTime)
        {
            if (doUpdate && Alive)
            {
                float timeDeltaMultiplier = (float)((double)gameTime.ElapsedGameTime.TotalSeconds * (double)Constants.TIMEDELTACONSTANT);
                position.X += (float)(Velocity * timeDeltaMultiplier * Math.Cos(MathHelper.ToRadians(direction)));
                position.Y += (float)(Velocity * timeDeltaMultiplier * Math.Sin(MathHelper.ToRadians(direction)));

                drawPosition = position + offset;
                drawDirection += clockwiseTurnRate;
                direction %= 360;
                drawDirection %= 360;
                
                if (position.X + offset.X < 0) position.X = Shmup.Width + offset.X;
                if (position.X - offset.X > Shmup.Width) position.X = 0 - offset.X;
                if (position.Y + offset.Y < 0) position.Y = Shmup.Height + offset.Y;
                if (position.Y - offset.Y > Shmup.Height) position.Y = 0 - offset.Y;
                if (position.X + offset.X < 0 || position.X - offset.X > Shmup.Width || position.Y + offset.Y < 0 || position.Y - offset.Y > Shmup.Width) direction = World.random.Next(0, 360);
            }
        }
    }

    /// <summary>
    /// A class representing the laser entity
    /// </summary>
    public class Laser : Entity
    {
        public float Velocity { get; set; }

        public bool doUpdate;

        public bool canFire;

        /// <summary>
        /// Construct the laser
        /// </summary>
        /// <param name="sprite">The sprite to render</param>
        /// <param name="startVelocity">The initial velocity of the laser</param>
        public Laser(SpriteSheet.Sprite sprite, float startVelocity): base((SpriteSheet.Sprite)sprite)
        {
            Velocity = startVelocity;
            doDraw = false;
            doUpdate = false;
            canFire = true;
        }

        /// <summary>
        /// Fire the laser
        /// </summary>
        /// <param name="startPosition">The position to fire from</param>
        /// <param name="initialDirection">The direction to fire at</param>
        public void Fire(Vector2 startPosition, float initialDirection)
        {
            position = startPosition + new Vector2(0, 3);
            direction = initialDirection;
            doUpdate = true;
            doDraw = true;
            canFire = false;
        }
        
        /// <summary>
        /// Register a hit
        /// </summary>
        public void Hit()
        {
            doUpdate = false;
            doDraw = false;
            canFire = true;
            position = Vector2.Zero;
            direction = 0;
        }

        /// <summary>
        /// Update the movement of the laser
        /// </summary>
        /// <param name="gameTime">A representation of the time since the last update</param>
        public override void Update(GameTime gameTime)
        {
            if (doUpdate)
            {
                float timeDeltaMultiplier = (float)((double)gameTime.ElapsedGameTime.TotalSeconds * (double)Constants.TIMEDELTACONSTANT);
                position.X += (float)(Velocity * timeDeltaMultiplier * Math.Cos(MathHelper.ToRadians(direction)));
                position.Y += (float)(Velocity * timeDeltaMultiplier * Math.Sin(MathHelper.ToRadians(direction)));

                drawPosition = position + offset;
                drawDirection = direction + 90;
                direction %= 360;
                drawDirection %= 360;

                if (position.X + offset.X < 0 || position.X - offset.X > Shmup.Width || position.Y + offset.Y < 0 || position.Y - offset.Y > Shmup.Height)
                {
                    doUpdate = false;
                    doDraw = false;
                    canFire = true;
                    position = Vector2.Zero;
                    direction = 0;
                }
            }
        }
    }

    /// <summary>
    /// A class representation of the star object
    /// </summary>
    public class Star : Entity
    {
        /// <summary>
        /// Construct the star
        /// </summary>
        /// <param name="sprite">The sprite to render</param>
        /// <param name="startPosition">The position to render the star to</param>
        public Star(SpriteSheet.Sprite sprite, Vector2 startPosition) : base(sprite, startPosition, 0)
        { }

        /// <summary>
        /// Update the star's position
        /// </summary>
        /// <param name="gameTime">A representation of the time since the last update</param>
        public override void Update(GameTime gameTime)
        {
            drawPosition.Y = position.Y += (float)((double)gameTime.ElapsedGameTime.TotalSeconds * (double)Constants.TIMEDELTACONSTANT);
            if (position.Y - offset.Y > Shmup.Height) position.Y = 0 - offset.Y;
        }
    }

    /// <summary>
    /// A player health entity
    /// </summary>
    public class PlayerHealth : Entity
    {
        public int ID { get; private set; }

        /// <summary>
        /// Construct the player health entity
        /// </summary>
        /// <param name="sprite">The sprite to render to the screen</param>
        /// <param name="id">The id to give the player health</param>
        /// <param name="startPos">The start position of the player health</param>
        public PlayerHealth(SpriteSheet.Sprite sprite, int id, Vector2 startPos) : base(sprite, startPos, 0)
        {
            ID = id;
        }

        /// <summary>
        /// Update the state of the player health
        /// </summary>
        /// <param name="gameTime">A representation of the time since the last update</param>
        public override void Update(GameTime gameTime)
        {
            drawPosition = position;
            doDraw = World.player.Health > ID;
        }
    }

    /// <summary>
    /// An explosion entity
    /// </summary>
    public class Explosion : Entity
    {
        private int CurrentSprite { get; set; }
        private SpriteSheet.Sprite[] sprites;
        public bool Exploding { get; private set; }
        public bool CanExplode { get; private set; }
        public float Duration { get; set; }
        private float TimerLimit { get; set; }

        /// <summary>
        /// Construct the explosion object
        /// </summary>
        /// <param name="spriteOrder">The order of sprites to render for the explosion animation</param>
        /// <param name="explosionDuration">The time the explosion takes to complete</param>
        public Explosion(SpriteSheet.Sprite[] spriteOrder, float explosionDuration = 1000): base(spriteOrder[0], new Vector2(100, 100), 0)
        {
            CurrentSprite = 0;
            sprites = spriteOrder;
            doDraw = false;
            Exploding = false;
            CanExplode = true;
            Duration = explosionDuration;
        }

        /// <summary>
        /// Start the explosion
        /// </summary>
        public void Explode()
        {
            Exploding = true;
            CanExplode = false;
            doDraw = true;
            TimerLimit = Duration / 64;
        }

        double updateTimer = 0;
        /// <summary>
        /// Update the explosion's state
        /// </summary>
        /// <param name="gameTime">A representation of the time since the last update</param>
        public override void Update(GameTime gameTime)
        {
            if (Exploding)
            {
                updateTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (updateTimer > TimerLimit) { SetSprite(sprites[CurrentSprite]); updateTimer = 0; }

                if (CurrentSprite++ >= 63)
                {
                    Exploding = false;
                    doDraw = false;
                    CanExplode = true;
                    CurrentSprite = 0;
                }
            }
        }
    }

    /// <summary>
    /// The firemode entity
    /// </summary>
    public class FireMode : Entity
    {
        private int ID;

        /// <summary>
        /// Construct the firemode entity
        /// </summary>
        /// <param name="id">The identifier of this instance</param>
        /// <param name="startPos">The position to render the entity</param>
        public FireMode(int id, Vector2 startPos) : base(SpriteSheet.Sprite.LaserGreen, startPos, 0)
        {
            ID = id;
            doDraw = false;
            direction = 0;
            Scale = .5f;
        }

        /// <summary>
        /// Update the state of the firemode
        /// </summary>
        /// <param name="gameTime">A representation of the time since the last update</param>
        public override void Update(GameTime gameTime)
        {
            drawPosition = position;

            switch (World.player.FireMode)
            {
                case Player.ShootMode.SemiAutomatic:
                    if (ID == 0) doDraw = true;
                    else doDraw = false;
                    break;
                case Player.ShootMode.BurstFire:
                    if (ID < 5) doDraw = true;
                    else doDraw = false;
                    break;
                case Player.ShootMode.Automatic:
                    if (ID < 10) doDraw = true;
                    else doDraw = false;
                    break;
            }
        }
    }

    /// <summary>
    /// A rotatable rectangle
    /// </summary>
    public class RotatedRectangle
    {
        public Vector2 TopLeft { get; private set; }
        public Vector2 TopRight { get; private set; }
        public Vector2 BottomLeft { get; private set; }
        public Vector2 BottomRight { get; private set; }

        public RotatedRectangle() { }
        public RotatedRectangle(Rectangle rectangle, float rotation)
        {
            GetRectangle(rectangle, rotation);
        }
        public RotatedRectangle(Vector2 tl, Vector2 tr, Vector2 bl, Vector2 br, float rotation)
        {
            TopLeft = tl;
            TopRight = tr;
            BottomLeft = bl;
            BottomRight = br;
        }

        public bool Intersects(RotatedRectangle rectangle)
        {
            return false;
        }

        public RotatedRectangle GetRectangle(Rectangle rectangle, float rotation)
        {
            Matrix.Matrix rotationMatrix = Matrix.Matrix.Rotation(rotation);
            Vector2 centreOffset = new Vector2(rectangle.Width / 2, rectangle.Height / 2);
            Vector2 centre = new Vector2(rectangle.X, rectangle.Y) + centreOffset;
            TopLeft = (rotationMatrix * new Matrix.Matrix2(centre.X - rectangle.Left, centre.Y - rectangle.Top)).ToVector2();
            TopRight = (rotationMatrix * new Matrix.Matrix2(centre.X - rectangle.Right, centre.Y - rectangle.Top)).ToVector2();
            BottomLeft = (rotationMatrix * new Matrix.Matrix2(centre.X - rectangle.Left, centre.Y - rectangle.Bottom)).ToVector2();
            BottomRight = (rotationMatrix * new Matrix.Matrix2(centre.X - rectangle.Right, centre.Y - rectangle.Bottom)).ToVector2();
            return this;
        }

        public static RotatedRectangle operator +(RotatedRectangle left, Vector2 right)
        {
            left.TopLeft += right;
            left.TopRight += right;
            left.BottomLeft += right;
            left.BottomRight += right;
            return left;
        }

        public override string ToString()
        {
            return String.Format("RotatedRectangle({0}, {1}, {2}, {3})", TopLeft, TopRight, BottomLeft, BottomRight);
        }
    }

    /// <summary>
    /// A class to handle sprite information
    /// </summary>
    public class SpriteSheet
    {
        public Texture2D Texture { get; private set; }
        private Rectangle[] spriteRects;
        private Rectangle[] spriteCollisionRects;
        private List<Vector2>[] spriteCollisionPoints;

        /// <summary>
        /// Set the sprite information
        /// </summary>
        /// <param name="spriteSheetTexture"></param>
        public SpriteSheet(Texture2D spriteSheetTexture)
        {
            Texture = spriteSheetTexture;
            spriteRects = new Rectangle[Enum.GetNames(typeof(Sprite)).Length];
            spriteCollisionRects = new Rectangle[spriteRects.Length];
            spriteCollisionPoints = new List<Vector2>[spriteRects.Length];
            for (int i = 0; i < spriteCollisionRects.Length; i++) spriteCollisionPoints[i] = new List<Vector2>();

            spriteRects[(int)Sprite.Ship] = new Rectangle(72, 50, 41, 31);
            spriteCollisionPoints[(int)Sprite.Ship].Add(new Vector2(20, 0));
            spriteCollisionPoints[(int)Sprite.Ship].Add(new Vector2(2, 12));
            spriteCollisionPoints[(int)Sprite.Ship].Add(new Vector2(38, 12));
            spriteCollisionPoints[(int)Sprite.Ship].Add(new Vector2(20, 30));
            spriteCollisionPoints[(int)Sprite.Ship].Add(new Vector2(2, 25));
            spriteCollisionPoints[(int)Sprite.Ship].Add(new Vector2(38, 25));
            spriteCollisionPoints[(int)Sprite.Ship].Add(new Vector2(20, 12));

            spriteRects[(int)Sprite.Enemy] = new Rectangle(169, 50, 40, 32);
            spriteCollisionPoints[(int)Sprite.Enemy].Add(new Vector2(18, 0));
            spriteCollisionPoints[(int)Sprite.Enemy].Add(new Vector2(21, 0));
            spriteCollisionPoints[(int)Sprite.Enemy].Add(new Vector2(0, 24));
            spriteCollisionPoints[(int)Sprite.Enemy].Add(new Vector2(15, 30));
            spriteCollisionPoints[(int)Sprite.Enemy].Add(new Vector2(24, 30));
            spriteCollisionPoints[(int)Sprite.Enemy].Add(new Vector2(39, 23));
            spriteCollisionPoints[(int)Sprite.Enemy].Add(new Vector2(10, 13));
            spriteCollisionPoints[(int)Sprite.Enemy].Add(new Vector2(29, 13));
            spriteCollisionRects[(int)Sprite.Enemy] = new Rectangle(11, 11, 18, 16);

            spriteRects[(int)Sprite.Asteroid_01] = new Rectangle(380, 50, 41, 34);
            spriteCollisionRects[(int)Sprite.Asteroid_01] = new Rectangle(4, 4, 31, 27);

            spriteRects[(int)Sprite.Asteroid_02] = new Rectangle(424, 45, 48, 39);
            spriteCollisionRects[(int)Sprite.Asteroid_02] = new Rectangle(8, 3, 31, 30);

            spriteRects[(int)Sprite.Asteroid_03] = new Rectangle(380, 91, 36, 32);
            spriteCollisionRects[(int)Sprite.Asteroid_03] = new Rectangle(2, 4, 30, 26);

            spriteRects[(int)Sprite.Asteroid_04] = new Rectangle(433, 87, 40, 38);
            spriteCollisionRects[(int)Sprite.Asteroid_04] = new Rectangle(5, 4, 31, 33);

            spriteRects[(int)Sprite.Asteroid_05] = new Rectangle(477, 52, 18, 18);
            spriteCollisionRects[(int)Sprite.Asteroid_05] = new Rectangle(2, 2, 15, 15);

            spriteRects[(int)Sprite.Asteroid_06] = new Rectangle(503, 53, 18, 16);
            spriteCollisionRects[(int)Sprite.Asteroid_06] = new Rectangle(2, 2, 14, 12);

            spriteRects[(int)Sprite.LaserGreen] = new Rectangle(73, 202, 5, 24);
            spriteCollisionPoints[(int)Sprite.LaserGreen].Add(new Vector2(2, 11));

            spriteRects[(int)Sprite.LaserRed] = new Rectangle(73, 230, 5, 23);
            spriteCollisionPoints[(int)Sprite.LaserRed].Add(new Vector2(2, 11));

            spriteRects[(int)Sprite.LaserBlue] = new Rectangle(73, 258, 5, 23);
            spriteCollisionPoints[(int)Sprite.LaserBlue].Add(new Vector2(2, 11));

            spriteRects[(int)Sprite.Star_01] = new Rectangle(279, 72, 2, 2);
            spriteRects[(int)Sprite.Star_02] = new Rectangle(282, 72, 2, 2);
            spriteRects[(int)Sprite.Star_03] = new Rectangle(285, 72, 2, 2);
            spriteRects[(int)Sprite.Star_04] = new Rectangle(288, 72, 2, 2);
            spriteRects[(int)Sprite.Star_05] = new Rectangle(291, 72, 2, 2);

            spriteRects[(int)Sprite.SmallShip] = new Rectangle(220, 49, 15, 12);

            int currentSprite = (int)Sprite.Explosion_01;
            int xStart = 64;
            int yStart = 576;
            int xSize = 32;
            int ySize = 32;
            int x = xStart;
            int y = yStart;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    spriteRects[(int)currentSprite++] = new Rectangle(x, y, xSize, ySize);
                    x += xSize;
                }
                y += ySize;
                x = xStart;
            }
        }

        /// <summary>
        /// Gets a sprite's rectangle on the spritesheet
        /// </summary>
        /// <param name="sprite">The sprite to retrieve</param>
        /// <returns>The rectangle of the sprite on the sprite sheet</returns>
        public Rectangle GetSpriteRectangle(Sprite sprite)
        {
            return spriteRects[(int)sprite];
        }

        /// <summary>
        /// Gets a sprite's collision regtangle
        /// </summary>
        /// <param name="sprite">The sprite to retrieve</param>
        /// <returns>The collision rectangle of the sprite</returns>
        public Rectangle GetSpriteCollisionRectanlge(Sprite sprite)
        {
            return spriteCollisionRects[(int)sprite];
        }

        /// <summary>
        /// Gets a sprite's collision regtangle
        /// </summary>
        /// <param name="sprite">The sprite to retrieve</param>
        /// <returns>The collision points of the sprite</returns>
        public Vector2[] GetSpriteCollisionPoints(Sprite sprite)
        {
            return spriteCollisionPoints[(int)sprite].ToArray();
        }

        /// <summary>
        /// References to all the sprites
        /// </summary>
        public enum Sprite
        {
            Ship,
            Enemy,
            Asteroid_01,
            Asteroid_02,
            Asteroid_03,
            Asteroid_04,
            Asteroid_05,
            Asteroid_06,
            LaserGreen,
            LaserRed,
            LaserBlue,
            Star_01,
            Star_02,
            Star_03,
            Star_04,
            Star_05,
            SmallShip,
            Explosion_01,
            Explosion_02,
            Explosion_03,
            Explosion_04,
            Explosion_05,
            Explosion_06,
            Explosion_07,
            Explosion_08,
            Explosion_09,
            Explosion_10,
            Explosion_11,
            Explosion_12,
            Explosion_13,
            Explosion_14,
            Explosion_15,
            Explosion_16,
            Explosion_17,
            Explosion_18,
            Explosion_19,
            Explosion_20,
            Explosion_21,
            Explosion_22,
            Explosion_23,
            Explosion_24,
            Explosion_25,
            Explosion_26,
            Explosion_27,
            Explosion_28,
            Explosion_29,
            Explosion_30,
            Explosion_31,
            Explosion_32,
            Explosion_33,
            Explosion_34,
            Explosion_35,
            Explosion_36,
            Explosion_37,
            Explosion_38,
            Explosion_39,
            Explosion_40,
            Explosion_41,
            Explosion_42,
            Explosion_43,
            Explosion_44,
            Explosion_45,
            Explosion_46,
            Explosion_47,
            Explosion_48,
            Explosion_49,
            Explosion_50,
            Explosion_51,
            Explosion_52,
            Explosion_53,
            Explosion_54,
            Explosion_55,
            Explosion_56,
            Explosion_57,
            Explosion_58,
            Explosion_59,
            Explosion_60,
            Explosion_61,
            Explosion_62,
            Explosion_63,
            Explosion_64,
        }

        public static Sprite[] AsteroidSprites
        {
            get
            {
                SpriteSheet.Sprite[] asteroidSprites = new SpriteSheet.Sprite[6];
                for (int i = 0; i < asteroidSprites.Length; i++) asteroidSprites[i] = (SpriteSheet.Sprite)((int)SpriteSheet.Sprite.Asteroid_01 + i);
                return asteroidSprites;
            }
        }

        public static Sprite[] ExplosionSprites
        {
            get
            {
                SpriteSheet.Sprite[] explosionSprites = new SpriteSheet.Sprite[64];
                for (int i = 0; i < explosionSprites.Length; i++) explosionSprites[i] = (SpriteSheet.Sprite)((int)SpriteSheet.Sprite.Explosion_01 + i);
                return explosionSprites;
            }
        }

        public static Sprite[] StarSprites
        {
            get
            {
                SpriteSheet.Sprite[] starSprites = new SpriteSheet.Sprite[5];
                for (int i = 0; i < starSprites.Length; i++) starSprites[i] = (SpriteSheet.Sprite)((int)SpriteSheet.Sprite.Star_01 + i);
                return starSprites;
            }
        }
    }
}
