#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace OverheadShooter
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        SpriteFont gameFont;

        Vector2 playerPosition = new Vector2(100, 100);
        Vector2 enemyPosition = new Vector2(100, 100);

        Random random = new Random();
        SpriteBatch spriteBatch;

        GamePadState previousGamePadState = GamePad.GetState(PlayerIndex.One);
        #region ShipVariables
        GameObject ship = new GameObject();

        // ship upgrades
        int upgrade = 40;  //IDDQD
        int maxLevel = 5;
        int score = 0;
        int speed = 3; //controls ship speed. 2 is a tad slow but could be a good start.
        int bulletPower = 9;  //controls bullet speed.
        int laserPower = 16;
        int missilePower = 6;
        float missileAccel = .3f; //missile accelerates this speed towards its target
        int coolDownGun = 250; // CD for guns
        int coolDownLaser = 500; // CD for lasers
        int coolDownMissile = 800; //CD for missiles
        int level = 1; //ship level
        int nextLevel = 10;
        int spreadLvl = 3;  //Level of multishot
        int missileLvl = 1;
        int laserLvl = 1;
        int shieldLvl = 1;
        int currentWeapon = 1; // 1 is gun, 2 is laser, 3 is missile
        int invuln = 30; //invulnerability timer
        const int maxBullets = 200; // probally needs to go up. 
        GameObject[] bullets;


                
        const int maxLasers = 200; //varies on implementation
        GameObject[] lasers;


        const int maxMissiles = 30;
        GameObject[] missiles;
//        GameObject[] missileTarget; // used for old seeker missiles

        DateTime lastBulletTime;

        DateTime lastSpawnTime;
        int spawnIntensity = 10;  //interval in seconds to determin spawn rate




        const int maxShields = 240;
        int maxShieldStrength = 60;
        GameObject[] shields;
        int currentShield = 0;
        float shieldDamage = 0f;
        //float shieldRotation = 0f;
        #endregion
        bool debugState = false; //displays extra info


        string[] messageQueue; //these are for use in the PromptPlayer function
        int[] messageTicks;
        Vector2[] messagePos;
        const int maxMQueue = 100;

        DateTime gameStartTime = DateTime.Now;

        //        AudioEngine audioEngine;
        //        SoundBank soundBank;
        //        WaveBank waveBank;

        SpriteFont font;
        
        const int maxStars = 750;
        GameObject[] stars;


        #region BadGuyVariables
        Random r = new Random();

        int maxBadGuy = 128;
        int cubeNumber = 0;
        int maxCubes = 512;
        const int maxEnemyShields = 240;
        GameObject[] enemySquares;
        GameObject[] enemyCubePacks;
        GameObject[] enemyCubes;
        GameObject[] enemyChargers;
        GameObject[] enemyReflectors;
        GameObject[] enemyDodgers;
        GameObject[] enemySnakes;
        GameObject[] enemyCowards;
        GameObject[] enemyFood; //Don't shoot them!  Warrior shot the food!
        GameObject[] enemyPlaceholder1;
        GameObject[] enemyPlaceholder2;
        GameObject[] enemyPlaceholder3;
        GameObject[] enemyPlaceholder4;
        GameObject[] enemyShields;
        
        


        #endregion

        //Hud THings
        Rectangle viewportRect;
        Vector2 scoreDrawPoint = new Vector2(0.8f, 0.1f);

        //Camera Decleration
        Matrix cameraProjectionMatrix;  //feild of view
        Matrix cameraViewMatrix;        // look at matrix

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            gameFont = content.Load<SpriteFont>("gamefont");

            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            //Thread.Sleep(1000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);

            //sound crap.
            //audioEngine = new AudioEngine("Content\\Audio\\ProjectAudio.xap");
            //waveBank = new WaveBank(audioEngine, "Content\\Audio\\Wave Bank.xwb");
            //soundBank = new SoundBank(audioEngine, "Content\\Audio\\Sound Bank.xsb");
            #region GameObjects
            ship.model = content.Load<Model>(
                "Models\\Ship"); // loading the ship model
            ship.scale = 20.0f;
            ship.alive = true;
            ship.projType = 5;
            ship.special = 5; //hit points

            bullets = new GameObject[maxBullets]; // loads up the spread cannon.
            for (int i = 0; i < maxBullets; i++)
            {
                bullets[i] = new GameObject();
                bullets[i].model =
                    content.Load<Model>("Models\\bullet");
                bullets[i].scale = 3.0f;
                bullets[i].special = 0; //Doesn't currently use special, but that's ok
                bullets[i].projType = 1;
            }

            lasers = new GameObject[maxLasers]; // loads up the laser cannon.
            for (int i = 0; i < maxLasers; i++)
            {
                lasers[i] = new GameObject();
                lasers[i].model =
                    content.Load<Model>("Models\\laser");
                lasers[i].scale = 3.0f;
                lasers[i].rotation.X = MathHelper.PiOver2;
                lasers[i].special = 0;
                lasers[i].projType = 2;
            }


            missiles = new GameObject[maxMissiles];
            for (int i = 0; i < maxMissiles; i++)
            {
                missiles[i] = new GameObject();
                missiles[i].model =
                    content.Load<Model>("Models\\missile");
                missiles[i].scale = 6f;
                missiles[i].projType = 3;
            }
            shields = new GameObject[maxShields];
            for (int i = 0; i < maxShields; i++)
            {
                shields[i] = new GameObject();
                shields[i].model = content.Load<Model>("Models\\bullet"); //TODO: New graphic?
                shields[i].scale = 2.0f;
                shields[i].projType = 4;
            }
            #region enemydeclarations

            enemyShields = new GameObject[maxEnemyShields];
            for (int i = 0; i < maxEnemyShields; i++)
            {
                enemyShields[i] = new GameObject();
                enemyShields[i].model = content.Load<Model>(
                    "Models\\bullet");
                enemyShields[i].scale = 2f;
                
            }

            enemySquares = new GameObject[maxBadGuy];
            for (int i = 0; i < maxBadGuy; i++)
            {
                enemySquares[i] = new GameObject();
                enemySquares[i].model = content.Load<Model>(
                    "Models\\Diamond");
                enemySquares[i].scale = 7.0f;
            }

            enemyCubePacks = new GameObject[maxBadGuy];
            for (int i = 0; i < maxBadGuy; i++)
            {
                enemyCubePacks[i] = new GameObject();
                enemyCubePacks[i].model = content.Load<Model>(
                    "Models\\BevelCubes");
                enemyCubePacks[i].scale = 12.0f;
            }

            enemyCubes = new GameObject[maxCubes];
            for (int i = 0; i < maxCubes; i++)
            {
                enemyCubes[i] = new GameObject();
                enemyCubes[i].model = content.Load<Model>(
                    "Models\\BevelCube");
                enemyCubes[i].scale = 10.0f;
                enemyCubes[i].special = 0;
            }

            enemyChargers = new GameObject[maxBadGuy];
            for (int i = 0; i < maxBadGuy; i++)
            {
                enemyChargers[i] = new GameObject();
                enemyChargers[i].model = content.Load<Model>(
                    "Models\\LCharger");
                enemyChargers[i].scale = 10.0f;
                enemyChargers[i].special = 0;
                //enemyChargers[i].rotation.X = -90;
                //enemyChargers[i].rotation.Z = -45;
                enemyChargers[i].projType = i;
            }

            enemyReflectors = new GameObject[maxBadGuy];
            for (int i = 0; i < maxBadGuy; i++)
            {
                enemyReflectors[i] = new GameObject();
                enemyReflectors[i].model = content.Load<Model>("Models\\RingShape");
                enemyReflectors[i].scale = 5.0f;
                enemyReflectors[i].special = 0;
            }
            enemyDodgers= new GameObject[maxBadGuy];
            for (int i = 0; i < maxBadGuy; i++)
            {
                enemyDodgers[i]  = new GameObject();
                enemyDodgers[i].model = content.Load<Model>("Models\\RingShape");
                enemyDodgers[i].scale = 5.0f;
                enemyDodgers[i].special = 0;
            }
            enemySnakes= new GameObject[maxBadGuy];
            for (int i = 0; i < maxBadGuy; i++)
            {
                enemySnakes[i]  = new GameObject();
                enemySnakes[i].model = content.Load<Model>("Models\\RingShape");
                enemySnakes[i].scale = 5.0f;
                enemySnakes[i].special = 0;
            }
            enemyCowards= new GameObject[maxBadGuy];
            for (int i = 0; i < maxBadGuy; i++)
            {
                enemyCowards[i]  = new GameObject();
                enemyCowards[i].model = content.Load<Model>("Models\\RingShape");
                enemyCowards[i].scale = 5.0f;
                enemyCowards[i].special = 0;
            }
            enemyFood= new GameObject[maxBadGuy];
            for (int i = 0; i < maxBadGuy; i++)
            {
                enemyFood[i]  = new GameObject();
                enemyFood[i].model = content.Load<Model>("Models\\RingShape");
                enemyFood[i].scale = 5.0f;
                enemyFood[i].special = 0;
            }
            enemyPlaceholder1 = new GameObject[maxBadGuy];
            for (int i = 0; i < maxBadGuy; i++)
            {
                enemyPlaceholder1[i]  = new GameObject();
                enemyPlaceholder1[i].model = content.Load<Model>("Models\\RingShape");
                enemyPlaceholder1[i].scale = 5.0f;
                enemyPlaceholder1[i].special = 0;
            }
            enemyPlaceholder2 = new GameObject[maxBadGuy];
            for (int i = 0; i < maxBadGuy; i++)
            {
                enemyPlaceholder2[i] = new GameObject();
                enemyPlaceholder2[i].model = content.Load<Model>("Models\\RingShape");
                enemyPlaceholder2[i].scale = 5.0f;
                enemyPlaceholder2[i].special = 0;
            }
            enemyPlaceholder3 = new GameObject[maxBadGuy];
            for (int i = 0; i < maxBadGuy; i++)
            {
                enemyPlaceholder3[i] = new GameObject();
                enemyPlaceholder3[i].model = content.Load<Model>("Models\\RingShape");
                enemyPlaceholder3[i].scale = 5.0f;
                enemyPlaceholder3[i].special = 0;
            }
            enemyPlaceholder4 = new GameObject[maxBadGuy];
            for (int i = 0; i < maxBadGuy; i++)
            {
                enemyPlaceholder4[i] = new GameObject();
                enemyPlaceholder4[i].model = content.Load<Model>("Models\\RingShape");
                enemyPlaceholder4[i].scale = 5.0f;
                enemyPlaceholder4[i].special = 0;
            }

            #endregion


            stars = new GameObject[maxStars];
            for (int i = 0; i < maxStars; i++)
            {
                stars[i] = new GameObject();
                stars[i].model = content.Load<Model>(
                    "Models\\star");
            }
            #endregion

            messageQueue = new string[maxMQueue];
            messageTicks = new int[maxMQueue];
            messagePos = new Vector2[maxMQueue];
            for (int i = 0; i < maxMQueue; i++)
            {
                messageQueue[i] = "";
                messageTicks[i] = 0;
                messagePos[i] = new Vector2(0, 0);
            }



            font = content.Load<SpriteFont>("Fonts\\GameFont");
            // Necessary ? (vpRect)
            viewportRect = new Rectangle(0, 0,
                ScreenManager.GraphicsDevice.Viewport.Width,
                ScreenManager.GraphicsDevice.Viewport.Height);

            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                cameraViewMatrix = Matrix.CreateLookAt(  // positon, target, up.
                   new Vector3(ship.position.X, ship.position.Y, 1000.0f),
                   ship.position,
                   Vector3.Up);  // hey it works!

                // Lets the camera draw on your screen the 3d Objects.
                cameraProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(45.0f), // FOV in radians, how wide the camera sees.
                    ScreenManager.GraphicsDevice.Viewport.AspectRatio, // H + W of the screen.
                    1.0f,
                    10000.0f);  // near and far clipping planes.



                input();

                spawn(); //NEW spawn routine. should cover all spawns.

                #region Update BadGuys
                updateSquares();
                updateChargers();
                updateEnemyShields();
                updateCubePacks();
                updateCubes();
                updateReflectors();
                updateDodgers();
                updateSnakes();
                updateCowards();
                updateFood();
                updatePlaceholder1();
                updatePlaceholder2();
                updatePlaceholder3();
                updatePlaceholder4();

#endregion

                updateBullet();
                spawnStars();
                updateShields();
                testCollision(ship);

                //audioEngine.Update();
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>

        void input()
        {

            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            ship.position.X = (gamePadState.ThumbSticks.Left.X * speed + ship.position.X);
            ship.position.Y = (gamePadState.ThumbSticks.Left.Y * speed + ship.position.Y);
            // added a max size of game area
            ship.position.X = MathHelper.Clamp(ship.position.X, -1000, 1000);
            ship.position.Y = MathHelper.Clamp(ship.position.Y, -1000, 1000);


            if (gamePadState.ThumbSticks.Right.Length() > 0.2)
            {
                ship.rotation.Z = (float)Math.Atan2(gamePadState.ThumbSticks.Right.Y,
                         gamePadState.ThumbSticks.Right.X) - MathHelper.PiOver2;
            }


            if (gamePadState.ThumbSticks.Right.Length() > 0.2)
            {
                FireWeapon();  // change ships rot.Z and shoots bullets in said direction.
            }
            #region UpgradePurchase
            if (upgrade > 0)
            {
                if (gamePadState.Buttons.A == ButtonState.Pressed && previousGamePadState.Buttons.A == ButtonState.Released && upgrade >= spreadLvl && spreadLvl < (maxLevel + 2))
                {
                    upgrade = upgrade - spreadLvl + 2;
                    spreadLvl = spreadLvl + 1;
                }
                if (gamePadState.Buttons.Y == ButtonState.Pressed && previousGamePadState.Buttons.Y == ButtonState.Released && upgrade >= laserLvl && laserLvl < maxLevel)
                {
                    upgrade = upgrade - laserLvl;
                    laserLvl = laserLvl + 1;
                    coolDownLaser -= 25;

                }
                if (gamePadState.Buttons.X == ButtonState.Pressed && previousGamePadState.Buttons.X == ButtonState.Released && upgrade >= shieldLvl && shieldLvl < maxLevel)
                {
                    upgrade -= shieldLvl;
                    shieldLvl++;
                    maxShieldStrength += 15;

                }
                if (gamePadState.Buttons.B == ButtonState.Pressed && previousGamePadState.Buttons.B == ButtonState.Released && upgrade >= missileLvl && missileLvl < maxLevel)
                {
                    upgrade = upgrade - missileLvl;
                    missileLvl = missileLvl + 1;
                }
            }
            #endregion
            #region WeaponSelect
            if (gamePadState.Buttons.RightShoulder == ButtonState.Pressed && previousGamePadState.Buttons.RightShoulder == ButtonState.Released)
            {
                if (currentWeapon < 3)
                {
                    currentWeapon ++ ;
                }
                else if (currentWeapon == 3)
                {
                    currentWeapon = 1;
                }
            }
            if (gamePadState.Buttons.LeftShoulder == ButtonState.Pressed && previousGamePadState.Buttons.LeftShoulder == ButtonState.Released)
            {
                if (currentWeapon > 1)
                {
                    currentWeapon -- ;
                }
                else if (currentWeapon == 1)
                {
                    currentWeapon = 3;
                }
            }

            if (gamePadState.DPad.Up == ButtonState.Pressed && previousGamePadState.DPad.Up == ButtonState.Released)
            {
                currentWeapon = 2;
                PromptPlayer("Lasers", 45, .5f, .2f);
            }
            if (gamePadState.DPad.Down == ButtonState.Pressed && previousGamePadState.DPad.Down == ButtonState.Released)
            {
                currentWeapon = 1;
                PromptPlayer("Guns", 45, .5f, .23f);
            }
            if (gamePadState.DPad.Right == ButtonState.Pressed && previousGamePadState.DPad.Right == ButtonState.Released)
            {
                currentWeapon = 3;
                PromptPlayer("Missiles", 45, .5f, .26f);
            }

            if (gamePadState.DPad.Left == ButtonState.Pressed && gamePadState.Buttons.X == ButtonState.Pressed)
            {
                DamageShields(-1f); //bookmark: temporary shield powerup
            }
            if (previousGamePadState.DPad.Left == ButtonState.Pressed && previousGamePadState.Buttons.X == ButtonState.Pressed)
            {
                if (gamePadState.DPad.Left == ButtonState.Released || gamePadState.Buttons.X == ButtonState.Released)
                {
                    PromptPlayer("Shield cheat!", 45, .4f, .20f);
                    PromptPlayer(shieldDamage.ToString(), 45, .4f, .23f);
                }
            }
            #endregion

            if (previousGamePadState.Triggers.Left > .2 && gamePadState.Triggers.Left <= .2)
            {
                speed = 3;
            }
            else
            {
                if (gamePadState.Triggers.Left > .2)
                {
                    AfterBurner();
                }
            }

            if (gamePadState.DPad.Left == ButtonState.Pressed && previousGamePadState.DPad.Left == ButtonState.Released)
            {
                if (debugState)
                {
                    debugState = false;
                }
                else
                {
                    debugState = true;
                    PromptPlayer("Debug enabled", 45, .5f, .3f);
                }
            }




            previousGamePadState = gamePadState;
        }
        #region ShootWeapons
        void FireWeapon()
        {
            if ((currentWeapon == 1)&& ((DateTime.Now - lastBulletTime).TotalMilliseconds > coolDownGun))
            {
                FireGuns();
                lastBulletTime = DateTime.Now;
            }
            else if ((currentWeapon == 2) && ((DateTime.Now - lastBulletTime).TotalMilliseconds > coolDownLaser))
            {
                FireLasers();
                lastBulletTime = DateTime.Now;
                
            }
            else if ((currentWeapon == 3) && (DateTime.Now - lastBulletTime).TotalMilliseconds > coolDownMissile)
            {
                FireMissiles();
                lastBulletTime = DateTime.Now;
            }
            else
            {
                //PromptPlayer("Error, CurrentWeapon:" + currentWeapon.ToString(), 120, .5f, .2f);
            }
        }

        void FireGuns()
        {
            

            for (int i = 0; i < spreadLvl; i++)
            {
                foreach (GameObject bullet in bullets)
                {
                    if ((!bullet.alive) && ((DateTime.Now - lastBulletTime).TotalMilliseconds > coolDownGun))// controls the max onscreen bullets.
                    {
                        //soundBank.PlayCue("Tank_Fire");
                        float angle = (float)Math.PI / -32 + i * (float)Math.PI / 16 / (spreadLvl - 1);

                        bullet.velocity = Vector3.Transform(GetBulletVelocity(), Matrix.CreateRotationZ(angle)) * bulletPower;
                        bullet.position = ship.position;
                        bullet.alive = true;
                        if (i == spreadLvl - 1)
                        {
                            lastBulletTime = DateTime.Now;
                        }
                        break;
                    }

                }
            }

        }

        void FireLasers()
        {

            for (int i = 0; i < laserLvl + 2; i++)
            {
                foreach (GameObject laser in lasers)
                {
                    if (laser.special == 0 && !laser.alive) // controls the max onscreen bullets.
                    {
                        //soundBank.PlayCue("Tank_Fire");

                        laser.velocity = Vector3.Normalize(Vector3.Transform(GetBulletVelocity(), Matrix.CreateRotationZ(0))) * laserPower;
                        laser.position = ship.position;
                        if (i == 0)
                        {
                            laser.alive = true;
                        }
                        laser.rotation.X = MathHelper.PiOver2 - ship.rotation.Z;  //bookmark laser rotation
                        laser.rotation.Y = MathHelper.PiOver2;
                        laser.special = i;
                        
                        break;
                    }

                }
            }

        }

        void FireMissiles ()
        {
            

            for (int i = 0; i < (missileLvl+3)/2; i++)
            {
                foreach (GameObject missile in missiles)
                {
                    if (!missile.alive)// controls the max onscreen missiles.
                    {
                        //soundBank.PlayCue("Tank_Fire");
                        float angle = (float)Math.PI / -32 + i * (float)Math.PI / 4 / (missileLvl + 1);

                        missile.velocity = Vector3.Normalize( Vector3.Transform(GetBulletVelocity(), Matrix.CreateRotationZ(angle))) * missilePower;
                        missile.position = ship.position;
                        missile.alive = true;
                        missile.scale = 6f; //reset in case of explosion?
                        missile.special = 240; //missile lives for 4 seconds
                        missile.model =
                    content.Load<Model>("Models\\missile");
                        
                        break;
                    }

                }
            }

        }


  
        void DetonateMissile(GameObject missile)
        {
            if (missile.special < 0)
            {
                missile.alive = true;
            }
            else if (missileLvl > 1)
            {
                missile.alive = true;
                missile.model =
                        content.Load<Model>("Models\\bullet"); //placeholder
                missile.special = -10;
                missile.velocity = Vector3.Zero;
            }
        }
        
        Vector3 GetBulletVelocity() // it worked yay. speed seems odd somethimes.
        {
            GamePadState gamepadstate = GamePad.GetState(PlayerIndex.One);
            return new Vector3((gamepadstate.ThumbSticks.Right.X),
                    (gamepadstate.ThumbSticks.Right.Y), 0);
        }
        #endregion
        void AfterBurner()
        {
            if (shieldDamage < maxShieldStrength)
            {
                speed = 6;
                DamageShields(.2f); //will need to change this.
            }
            else
            {
                speed = 3;
            }
        }

        void updateBullet()
        {
            
            foreach (GameObject bullet in bullets)
            {
                if (bullet.alive)
                {
                    bullet.position += bullet.velocity;

                    if (bullet.position.X > 1250 || bullet.position.X < -1250 || bullet.position.Y > 1250 || bullet.position.Y < -1250) 
                    {
                        bullet.alive = false;
                    }
                    else
                    {
                        testCollision(bullet);
                    }
                }
            }


            foreach (GameObject laser in lasers)
            {
                if (laser.special > 0)
                {

                    laser.special--;
                    if (laser.special == 0)
                    {
                        laser.alive = true;

                    }
                }
                if (laser.alive)
                {
                    laser.position += laser.velocity; 

                    


                    if (laser.position.X > 1250 || laser.position.X < -1250 || laser.position.Y > 1250 || laser.position.Y < -1250) 
                    {
                        laser.alive = false;
                    }
                    else
                    {
                        testCollision(laser);

                    }
                }
            }
            foreach (GameObject missile in missiles)
            {
                
                if (missile.alive)
                    if (missile.special < 0)
                    {
                        missile.special++;
                        if (missile.special == 0)
                        {
                            missile.alive = false;
                        }
                        else
                        {
                            missile.scale = 1.5f * (missile.special + 11);
                            if (missileLvl > 3)
                            {
                                missile.scale = 2.25f * (missile.special + 11);
                            }
                        }
                        
                        testCollision(missile);

                    }
                    else
                    {
                        {
                            missile.special--;
                            if (missile.special == 0)
                            {
                                if (missileLvl == 1)
                                {
                                    missile.alive = false;
                                }
                                else
                                {
                                    DetonateMissile(missile);
                                }
                            }

                            Vector3 interceptPos = GetNearestIntercept(missile.position, missile.velocity * 1.5f);



                            missile.velocity += Vector3.Normalize(interceptPos) * missileAccel;
                            missile.position += missile.velocity;




                            if (missile.position.X > 1500 || missile.position.X < -1500 || missile.position.Y > 1500 || missile.position.Y < -1500)
                            {
                                missile.alive = false;
                            }
                            else
                            {
                                testCollision(missile);
                            }
                        }
                    }
            }
            #region oldseekermissile
            /* for (int i = 0; i < maxMissiles; i++)
            {
                missiles[i].special--;
                if (missiles[i].special <= 0)
                {
                    missiles[i].alive = false;
                }
                missileTarget[i].alive = false;
                if (missiles[i].alive)
                {
                    int j = (int)missileTarget[i].special;
                    if (missileTarget[i].projType == 1)
                    {
                        if (enemySquares[j].alive)
                        {
                            missiles[i].velocity += Vector3.Normalize(enemySquares[j].position - missiles[i].position) * missileAccel;//physics accel
                            missiles[i].position += Vector3.Normalize(enemySquares[j].position - missiles[i].position) * missileHone * missileLvl;//direct accel
                            missileTarget[i].alive = true;
                            missileTarget[i].position = enemySquares[j].position;

                        }
                    }
                    else if (missileTarget[i].projType == 2)
                    {
                        if (enemyCubePacks[j].alive)
                        {
                            missiles[i].velocity += Vector3.Normalize(enemyCubePacks[j].position - missiles[i].position) * missileAccel;
                            missiles[i].position += Vector3.Normalize(enemyCubePacks[j].position - missiles[i].position) * missileHone * missileLvl;//direct accel
                            missileTarget[i].alive = true;
                            missileTarget[i].position = enemyCubePacks[j].position;
                            missileTarget[i].position.Z += 30f;
                        }
                    }

                    float missileSpeed;
                    Vector3 zerod = new Vector3(0, 0, 0);
                    Vector3.Distance(ref missiles[i].velocity, ref zerod, out missileSpeed);

                    if (missileSpeed > missileMaxSpeed)
                    {
                        missiles[i].velocity = missileMaxSpeed * Vector3.Normalize(missiles[i].velocity);
                    }
                    missiles[i].position += missiles[i].velocity;

                    if (missiles[i].position.X > 1050 || missiles[i].position.X < -1050 || missiles[i].position.Y > 1050 || missiles[i].position.Y < -1050)
                    {//Missiles can go beyond the base border
                        missiles[i].alive = false;
                    }
                    else
                    {
                        testCollision(missiles[i]);
                    }
                }
            }*/
#endregion
        }
        Vector3 GetNearestIntercept(Vector3 missilePos, Vector3 missileVel)
        {
            float shortTime = -1;
           /* float shortTime = MathHelper.Max(MathHelper.Max(
                TimeToIntercept(new Vector3(1100-missilePos.X, 1100-missilePos.Y, 0), missileVel),
                TimeToIntercept(new Vector3(1100-missilePos.X, -1100-missilePos.Y, 0), missileVel)),
                MathHelper.Max(TimeToIntercept(new Vector3(-1100-missilePos.X, 1100-missilePos.Y, 0), missileVel),
                TimeToIntercept(new Vector3(-1100-missilePos.X, -1100-missilePos.Y, 0), missileVel)));*/ 
            //checks all four corners for furthest distance
            Vector3 interPos = Vector3.Zero;
                //ship.rotation; //default shoots a missile straight
            
            
            foreach (GameObject square in enemySquares){
                if (square.alive){
                    float isOnscreenX = square.position.X - ship.position.X;//725
                    float isOnscreenY = square.position.Y - ship.position.Y;//410
                    isOnscreenY *= 1.78f; 
                    if (MathHelper.Max(MathHelper.Max(isOnscreenX, -1 * isOnscreenX), MathHelper.Max(isOnscreenY, isOnscreenY * -1)) < 800)
                    {
                        float time = TimeToIntercept(square.position - missilePos, square.velocity - missileVel);
                        if (time < shortTime || shortTime == -1)
                        {
                            shortTime = time;
                            interPos = square.position + time * square.velocity;
                        }
                    }
                }

            }
            foreach (GameObject cubePack in enemyCubePacks)
            {
                if (cubePack.alive)
                {
                    float isOnscreenX = cubePack.position.X - ship.position.X;
                    float isOnscreenY = cubePack.position.Y - ship.position.Y;
                    isOnscreenY *= 1.78f; 
                    if (MathHelper.Max(MathHelper.Max(isOnscreenX, -1 * isOnscreenX), MathHelper.Max(isOnscreenY, isOnscreenY * -1)) < 800)
                    {
                        float time = TimeToIntercept(cubePack.position - missilePos, cubePack.velocity - missileVel);
                        if (time < shortTime || shortTime == -1)
                        {
                            shortTime = time;
                            interPos = cubePack.position + time * cubePack.velocity;
                        }
                    }
                }

            }
            foreach (GameObject charger in enemyChargers)
            {
                if (charger.alive)
                {
                    float isOnscreenX = charger.position.X - ship.position.X;
                    float isOnscreenY = charger.position.Y - ship.position.Y;
                    isOnscreenY *= 1.78f;
                    if (MathHelper.Max(MathHelper.Max(isOnscreenX, -1 * isOnscreenX), MathHelper.Max(isOnscreenY, isOnscreenY * -1)) < 800)
                    {

                        if (MathHelper.Max(MathHelper.Max(isOnscreenX, -1 * isOnscreenX), MathHelper.Max(isOnscreenY, isOnscreenY * -1)) < 800)
                        {
                            float time = TimeToIntercept(charger.position - missilePos, charger.velocity - missileVel);
                            if (time < shortTime || shortTime == -1)
                            {
                                shortTime = time;
                                interPos = charger.position + time * charger.velocity;
                            }
                        }
                    }
                }

            }
            foreach (GameObject reflector in enemyReflectors)
            {
                if (reflector.alive)
                {
                    float isOnscreenX = reflector.position.X - ship.position.X;
                    float isOnscreenY = reflector.position.Y - ship.position.Y;
                    isOnscreenY *= 1.78f;
                    if (MathHelper.Max(MathHelper.Max(isOnscreenX, -1 * isOnscreenX), MathHelper.Max(isOnscreenY, isOnscreenY * -1)) < 800)
                    {
                        float time = TimeToIntercept(reflector.position - missilePos, reflector.velocity - missileVel);
                        if (time < shortTime || shortTime == -1)
                        {
                            shortTime = time;
                            interPos = reflector.position + time * reflector.velocity;
                        }
                    }
                }

            }
            foreach (GameObject dodger in enemyDodgers)
            {
                if (dodger.alive)
                {
                    float isOnscreenX = dodger.position.X - ship.position.X;
                    float isOnscreenY = dodger.position.Y - ship.position.Y;
                    isOnscreenY *= 1.78f;
                    if (MathHelper.Max(MathHelper.Max(isOnscreenX, -1 * isOnscreenX), MathHelper.Max(isOnscreenY, isOnscreenY * -1)) < 800)
                    {
                        float time = TimeToIntercept(dodger.position - missilePos, dodger.velocity - missileVel);
                        if (time < shortTime || shortTime == -1)
                        {
                            shortTime = time;
                            interPos = dodger.position + time * dodger.velocity;
                        }
                    }
                }
            }
            foreach (GameObject snake in enemySnakes)
            {
                if (snake.alive)
                {
                    float isOnscreenX = snake.position.X - ship.position.X;
                    float isOnscreenY = snake.position.Y - ship.position.Y;
                    isOnscreenY *= 1.78f;
                    if (MathHelper.Max(MathHelper.Max(isOnscreenX, -1 * isOnscreenX), MathHelper.Max(isOnscreenY, isOnscreenY * -1)) < 800)
                    {
                        float time = TimeToIntercept(snake.position - missilePos, snake.velocity - missileVel);
                        if (time < shortTime || shortTime == -1)
                        {
                            shortTime = time;
                            interPos = snake.position + time * snake.velocity;
                        }
                    }
                }
            }
            foreach (GameObject coward in enemyCowards)
            {
                float isOnscreenX = coward.position.X - ship.position.X;
                float isOnscreenY = coward.position.Y - ship.position.Y;
                    isOnscreenY *= 1.78f;
                    if (MathHelper.Max(MathHelper.Max(isOnscreenX, -1 * isOnscreenX), MathHelper.Max(isOnscreenY, isOnscreenY * -1)) < 800)
                    {
                        if (coward.alive)
                        {
                            float time = TimeToIntercept(coward.position - missilePos, coward.velocity - missileVel);
                            if (time < shortTime || shortTime == -1)
                            {
                                shortTime = time;
                                interPos = coward.position + time * coward.velocity;
                            }
                        }
                    }

            }
            foreach (GameObject placeholder in enemyPlaceholder1)
            {
                float isOnscreenX = placeholder.position.X - ship.position.X;
                float isOnscreenY = placeholder.position.Y - ship.position.Y;
                isOnscreenY *= 1.78f;
                if (MathHelper.Max(MathHelper.Max(isOnscreenX, -1 * isOnscreenX), MathHelper.Max(isOnscreenY, isOnscreenY * -1)) < 800)
                {
                    if (placeholder.alive)
                    {
                        float time = TimeToIntercept(placeholder.position - missilePos, placeholder.velocity - missileVel);
                        if (time < shortTime || shortTime == -1)
                        {
                            shortTime = time;
                            interPos = placeholder.position + time * placeholder.velocity;
                        }
                    }
                }

            }

            foreach (GameObject placeholder in enemyPlaceholder2)
            {
                float isOnscreenX = placeholder.position.X - ship.position.X;
                float isOnscreenY = placeholder.position.Y - ship.position.Y;
                isOnscreenY *= 1.78f;
                if (MathHelper.Max(MathHelper.Max(isOnscreenX, -1 * isOnscreenX), MathHelper.Max(isOnscreenY, isOnscreenY * -1)) < 800)
                {
                    if (placeholder.alive)
                    {
                        float time = TimeToIntercept(placeholder.position - missilePos, placeholder.velocity - missileVel);
                        if (time < shortTime || shortTime == -1)
                        {
                            shortTime = time;
                            interPos = placeholder.position + time * placeholder.velocity;
                        }
                    }
                }

            }
            foreach (GameObject placeholder in enemyPlaceholder3)
            {
                float isOnscreenX = placeholder.position.X - ship.position.X;
                float isOnscreenY = placeholder.position.Y - ship.position.Y;
                isOnscreenY *= 1.78f;
                if (MathHelper.Max(MathHelper.Max(isOnscreenX, -1 * isOnscreenX), MathHelper.Max(isOnscreenY, isOnscreenY * -1)) < 800)
                {
                    if (placeholder.alive)
                    {
                        float time = TimeToIntercept(placeholder.position - missilePos, placeholder.velocity - missileVel);
                        if (time < shortTime || shortTime == -1)
                        {
                            shortTime = time;
                            interPos = placeholder.position + time * placeholder.velocity;
                        }
                    }
                }

            }
            foreach (GameObject placeholder in enemyPlaceholder4)
            {
                float isOnscreenX = placeholder.position.X - ship.position.X;
                float isOnscreenY = placeholder.position.Y - ship.position.Y;
                isOnscreenY *= 1.78f;
                if (MathHelper.Max(MathHelper.Max(isOnscreenX, -1 * isOnscreenX), MathHelper.Max(isOnscreenY, isOnscreenY * -1)) < 800)
                {
                    if (placeholder.alive)
                    {
                        float time = TimeToIntercept(placeholder.position - missilePos, placeholder.velocity - missileVel);
                        if (time < shortTime || shortTime == -1)
                        {
                            shortTime = time;
                            interPos = placeholder.position + time * placeholder.velocity;
                        }
                    }
                }

            }
          

         
            

            //bookmark ADDITIONAL ENEMIES NEED TO BE ADDED HERE

            if (shortTime == -1)
            {
                return missileVel;
            }
            
            return interPos-missilePos;//returns a vector relative to the missile's current position
        }

        float TimeToIntercept(Vector3 tarPos, Vector3 tarVel)
        {
            
            int accelX = 1;
            int accelY = 1;
            if (tarPos.X < 0)
            {
                accelX = -1;
            }
            if (tarPos.Y < 0)
            {
                accelY = -1;
            }
            float timeX = 2 * tarVel.X / (accelX * missileAccel);
            float timeY = 2 * tarVel.Y / (accelY * missileAccel);
           
            float guess = timeX * timeX;
            guess += 8 * tarPos.X / (accelX * missileAccel);
            
            /*if (guess < 8 * tarPos.X / missileAccel)
            {
                guess += 8 * tarPos.X / missileAccel;
                
            }
            else
            {
                guess -= 8 * tarPos.X / missileAccel;
            }*/
            timeX += RoughSquareRoot(guess, guess, 10);

            guess = timeY * timeY;
            guess += 8 * tarPos.Y / (accelY * missileAccel);
            
            
            /*if (guess < 8 * tarPos.Y / missileAccel)
            {
                guess += 8 * tarPos.Y / missileAccel;
                
            }
            else
            {
                guess -= 8 * tarPos.Y / missileAccel;
            }*/
            timeY += RoughSquareRoot(guess, guess, 10);
            

            timeX = MathHelper.Max(timeX, -1 * timeX);
            timeY = MathHelper.Max(timeY, -1 * timeY);
            timeX = MathHelper.Max(timeX, timeY)/2;

            
            return timeX;
        }
        float RoughSquareRoot(float XtoRoot, float Guess, int i) //higher initial i means more precision, as does a better guess.
        {
            if (XtoRoot < 0)
            {
                PromptPlayer("Tried to squareroot " + XtoRoot.ToString(), 90, .5f, .4f);
                return Guess;
                
            }
            if (i == 0){
                return Guess;
            }
            return RoughSquareRoot(XtoRoot, (Guess + XtoRoot / Guess) / 2, i - 1);//recursion == fun!
        }
 
        void spawnStars()
        {
            foreach (GameObject star in stars)
            {
                if (!star.alive)
                {
                    star.alive = true;
                    star.position = new Vector3(
                        MathHelper.Lerp(-1000, 1000, (float)r.NextDouble()),
                        MathHelper.Lerp(-1000, 1000, (float)r.NextDouble()),
                        MathHelper.Lerp(-10, -100, (float)r.NextDouble())
                        );
                }
            }
        }
        #region spawning
        void spawn()
        {
            int waves = ((int)(DateTime.Now - gameStartTime).TotalMinutes + 2);
            int waveSize = 4;
            // Please do not modify my code if you do not understand it. Your changes are only better
            // IF we where not going to add more bad guys. Now I have to re do my work. 
            if ((DateTime.Now - lastSpawnTime).TotalSeconds > spawnIntensity) // spawnintensity currently starts at 10
            {
                for (int i = 0; i < waves; i++) // i is number of waves.
                {
                    float spawnSelector;
                    spawnSelector = MathHelper.Lerp(0, 3, (float)r.NextDouble());
                    if (spawnSelector > 0 && spawnSelector <= 1)
                    {
                        spawnSquares(waveSize);
                    }
                    else if (spawnSelector > 1 && spawnSelector <= 2)
                    {
                        spawnCubePacks(waveSize);
                    }
                    else if (spawnSelector > 2)
                    {
                        spawnChargers(waveSize);
                    }
                    // need to do something to increase spawn intensity.
                    // also wave size
                    //Todo add potential to spawn another set and determine how many spawn per set. probally a variable for I (max spawns?)
                    lastSpawnTime = DateTime.Now;
                }
            }
        }
        void spawnSquares(int spawnNum)
        {
            for (int i = 0; i < spawnNum; i++)
            {
                foreach (GameObject square in enemySquares)
                {
                    if (!square.alive)
                    {
                        square.alive = true;
                        square.special = 1;
                        square.position = spawnPosition();

                        break;
                    }

                }
            }
        }

        void spawnCubePacks(int spawnNum)
        {
            for (int i = 0; i < spawnNum; i++)
            {
                foreach (GameObject cubePack in enemyCubePacks)
                {
                    if (!cubePack.alive)
                    {
                        cubePack.alive = true;
                        cubePack.position = spawnPosition();
                        break;

                    }
                }
            }
        }
        void spawnChargers(int spawnNum)
        {
            for (int i = 0; i < spawnNum; i++)
            {
                foreach (GameObject charger in enemyChargers)
                {
                    if (!charger.alive)
                    {
                        charger.alive = true;
                        charger.special = 0;
                        charger.projType = 0;
                        charger.position = spawnPosition();
                        break;

                    }
                }
            }
        }

        void SpawnChargerShield(Vector3 pos, Vector3 vel, float special, int parent)
        {
            foreach (GameObject shield in enemyShields)
            {
                if (!shield.alive)
                {

                    
                    float angle = MathHelper.PiOver4;
                    angle = MathHelper.Lerp(-1 * angle, angle, (float) r.NextDouble());
                    
                    shield.velocity = vel;
                    shield.position = pos + Vector3.Transform(vel, Matrix.CreateRotationZ(angle)) * 4;
                    //shield.position = pos + 30 * vel;
                    
                    shield.alive = true;

                    shield.special = MathHelper.Lerp(0, 161 - special, (float) r.NextDouble());
                    shield.projType = parent;
                    
                    

                    break;
                }

            }
        }



        Vector3 spawnPosition()
        {

            float xLoc = MathHelper.Lerp(-1000f, 1000f, (float)r.NextDouble());
            float yLoc = MathHelper.Lerp(1000f, 1050f, (float)r.NextDouble());
            float side = MathHelper.Lerp(0f, 4f, (float)r.NextDouble());

            if (side > 2)
            {
                yLoc *= -1;
            }
            if (side > 3 || side < 1)
            {// swap xloc and yloc; discard side
                side = xLoc;
                xLoc = yLoc;
                yLoc = side;
            }
            return new Vector3(xLoc, yLoc, 0);
        }




        #endregion
        #region BadGuy AI
        void updateSquares()
        {



            foreach (GameObject square in enemySquares)
            {
                if (square.alive)
                {
                    
                    //square.special += .004f; //special is acceleration
                    square.special *= 1.001f;
                    square.special = MathHelper.Min(square.special, 6f);
                    square.velocity = Vector3.Normalize(ship.position - square.position) * (square.special);
                    square.position += square.velocity; 
                    square.rotation.X = square.rotation.X + 0.01f;
                    //square.rotation.Y = square.rotation.Y + 0.02f;
                }
            }
        }

        void updateCubePacks()
        {
            foreach (GameObject cubePack in enemyCubePacks)
            {
                if (cubePack.alive)
                {
                    // TODO CHANGE MOVEMENT STYLE
                    cubePack.velocity = Vector3.Normalize(ship.position - cubePack.position);
                    cubePack.position += cubePack.velocity;
                    
                }
            }
        }
        void updateCubes()
        {
            foreach (GameObject cube in enemyCubes)
            {
                if (cube.alive)
                {
                    cube.position = cube.position + cube.velocity;
                    if (cube.special < 60)
                    {
                        cube.scale = cube.special / 6.67f + 1f;
                    }
                    cube.special--;
                    if (cube.position.X > 1500 || cube.position.Y > 1500 || cube.position.X < -1500 || cube.position.Y < -1500)
                    {
                        cube.alive = false;
                    }
                    if (cube.special == 0)
                    {
                        cube.alive = false;
                    }
                }
            }
        }
        void updateChargers()
        {
            foreach (GameObject charger in enemyChargers)
            {
                if (charger.alive)
                {
                    
                    //If distance > X seek if distance < X charge? 
                    if (charger.special == 0)
                    {
                        charger.velocity = Vector3.Normalize(ship.position - charger.position) * 6;
                        charger.special = 1;
                        //charger.rotation.Z = (float)Math.Acos((double)charger.velocity.Y/6);
                        charger.rotation.Z = (float)Math.Atan2(charger.velocity.Y,
                         charger.velocity.X) - MathHelper.PiOver2;
                        //bookmark fix this now
                        

                    }
                    else if (charger.special < 160 && charger.special > 0)
                    {
                        if (charger.special < 10 || (charger.special % 5)==0)// using projtype to track cooldown on spawning shields
                        {
                            SpawnChargerShield(charger.position, charger.velocity, charger.special, charger.projType);
                        
                        }
                        
                        charger.special++;
                        
                        charger.position += charger.velocity;
                        //Possibly add some regard for proximity.
                    }
                    else
                    {
                        charger.special = 0;
                    }
                }
            }
        }

        void updateEnemyShields()
        {
            
            foreach (GameObject shield in enemyShields)
            {
                if (shield.alive)
                {
                    
                    shield.special--;
                    if (shield.special < 0)
                    {
                        if (shield.special < -20)
                        {
                            shield.alive = false;
                        }
                        else
                        {

                            shield.velocity = Vector3.Zero;
                        }

                    }
                    else
                    {
                        shield.position.Z = MathHelper.Lerp(-10f, 10f, (float)r.NextDouble());
                    }
                    shield.position += shield.velocity;
                }
            }
        }
                    
                    

        

        void updateReflectors() {
            foreach (GameObject reflector in enemyReflectors)
            {
                if (reflector.alive)
                {
                    reflector.position += reflector.velocity;
                }
            }
        }
        void updateDodgers() { 
            foreach (GameObject dodger in enemyDodgers)
            {
                if (dodger.alive)
                {
                    dodger.position += dodger.velocity;
                }
            }
        }
        void updateSnakes() { 
            foreach (GameObject snake in enemySnakes)
            {
                if (snake.alive)
                {
                    snake.position += snake.velocity;
                }
            }
        }
        void updateCowards() { 
            foreach (GameObject coward in enemyCowards)
            {
                if (coward.alive)
                {
                    coward.position += coward.velocity;
                }
            }
        }
        void updateFood() { 
            foreach (GameObject food in enemyFood)
            {
                if (food.alive)
                {
                    food.position += food.velocity;
                }
            }
        }
        void updatePlaceholder1() { 
            foreach (GameObject placeholder in enemyPlaceholder1)
            {
                if (placeholder.alive)
                {
                    placeholder.position += placeholder.velocity;
                }
            }
        }
        void updatePlaceholder2() {
            foreach (GameObject placeholder in enemyPlaceholder2)
            {
                if (placeholder.alive)
                {
                    placeholder.position += placeholder.velocity;
                }
            }
        }
        void updatePlaceholder3() {
            foreach (GameObject placeholder in enemyPlaceholder3)
            {
                if (placeholder.alive)
                {
                    placeholder.position += placeholder.velocity;
                }
            }
        }
        void updatePlaceholder4() {
            foreach (GameObject placeholder in enemyPlaceholder4)
            {
                if (placeholder.alive)
                {
                    placeholder.position += placeholder.velocity;
                }
            }
        }


        #endregion

        void updateShields()
        {
            if (invuln > 0)  //seems as good a place as any to time down invulnerability
            {
                invuln--;
            }
            currentShield++;
            //            shieldRotation++;
            if (currentShield == maxShields)
            {
                currentShield = 0;
            }
            float angle = currentShield * MathHelper.PiOver4 / -30;

            const int distance = 25;
            shields[currentShield].rotation = Vector3.Normalize(Vector3.Transform(new Vector3(1, 1, 0), Matrix.CreateRotationZ(angle)));
            

            //shields[currentShield].special = (20 + 10 * shieldLvl);
            shields[currentShield].special = MathHelper.Max(0f, maxShieldStrength - shieldDamage);
            if (shields[currentShield].special > 0)
            {
                shields[currentShield].alive = true;
            }
            if (shieldDamage > 0)
            {
                shieldDamage = MathHelper.Max(shieldDamage - .01f * (.75f + shieldLvl / 4), 0f);
            }

            foreach (GameObject shield in shields)
            {
                if (shield.alive)
                {
                    shield.position = ship.position + shield.rotation * distance;
                    shield.special--;
                    if (shield.special <= 0)
                    {
                        shield.alive = false;
                    }
                    //shield.scale = (3f + shield.special/60);
                    // scale this between 1f for trailing dots and 4f for starting dots
                    shield.scale = (1f + 3f * shield.special / MathHelper.Max(1f, maxShieldStrength - shieldDamage));
                    testCollision(shield);
                }

            }
        }

        void DamageShields(float damage)
        {
            shieldDamage += damage;
            shieldDamage = MathHelper.Min(shieldDamage, 10 + maxShieldStrength);
            foreach (GameObject shield in shields)
            {
                if (shield.alive)
                {
                    shield.special = MathHelper.Max(0f, shield.special - damage);
                }
            }
        }

        void ShipHit()  //Bookmark: Until we have life system in place
        {
            ship.alive = true;
            if (invuln == 0)
            {
                ship.special--; //lose 1 hp
                if (ship.special > 0)
                {
                    PromptPlayer("Warning! Only " + (ship.special * 20).ToString() + "% Hull remaining!", 30, .5f, .25f);
                    invuln = 30;
                }
                else
                {
                    ship.alive = false;
                }
            }
        }

        void testCollision(GameObject bullet)
        {
            BoundingSphere bulletsphere =
                bullet.model.Meshes[0].BoundingSphere;
            bulletsphere.Center = bullet.position;
            bulletsphere.Radius += bullet.scale;

            foreach (GameObject shield in enemyShields)
            {
                if (shield.alive)
                {
                    if (shield.special > 0)
                    {
                        BoundingSphere squaresphere =
                        shield.model.Meshes[0].BoundingSphere;
                        squaresphere.Center = new Vector3 (shield.position.X, shield.position.Y, 0);
                            
                        squaresphere.Radius += (shield.scale * 4);//shields detect at larger radius than they exist


                        if (squaresphere.Intersects(bulletsphere))
                        {
                            bullet.alive = false;
                            if (bullet.projType == 4) //projectile is shield
                            {

                                bullet.alive = true;
                                DamageShields(1f);
                                shield.alive = false;

                            }
                            else if (bullet.projType == 5) //projectile is SHIP!!
                            {
                                ShipHit();

                            }
                            else if (bullet.projType == 3)//projectile is missile
                            {
                                DetonateMissile(bullet);
                            }
                            break;
                        }
                    }
                }
            }

            foreach (GameObject square in enemySquares)
            {
                if (square.alive)
                {
                    BoundingSphere squaresphere =
                        square.model.Meshes[0].BoundingSphere;
                    squaresphere.Center = square.position;
                    squaresphere.Radius += (square.scale * 2);

                    if (squaresphere.Intersects(bulletsphere))
                    {
                        //play sound
                        score ++;
                        checkLevelUP();
                        bullet.alive = false;
                        if (bullet.projType == 4) //projectile is shield
                        {

                            bullet.alive = true;
                            DamageShields(10f);

                        }
                        else if (bullet.projType == 5) //projectile is SHIP!!
                        {
                            ShipHit();
                            score--;
                        }
                        else if (bullet.projType == 3)//projectile is missile
                        {
                            DetonateMissile(bullet);
                        }
                        square.alive = false;
                        break;
                    }
                }
            }
            foreach (GameObject cubePack in enemyCubePacks)
            {
                if (cubePack.alive)
                {
                    BoundingSphere cubePackssphere =
                        cubePack.model.Meshes[0].BoundingSphere;
                    cubePackssphere.Center = cubePack.position;
                    cubePackssphere.Radius += (cubePack.scale * 2);


                    if (cubePackssphere.Intersects(bulletsphere))
                    {
                        score += 3;
                        checkLevelUP();
                        bullet.alive = false;
                        cubePack.alive = false;
                        if (bullet.projType == 4) //projectile is shield
                        {
                            DamageShields(10f);
                            bullet.alive = true;
                        }
                        else if (bullet.projType == 3)//projectile is missile
                        {
                            DetonateMissile(bullet);
                        }
                        else if (bullet.projType == 5) //projectile is SHIP!!
                        {
                            score -= 3;
                            ShipHit();
                        }
                        
                        Vector3 scatterRot = new Vector3 (MathHelper.Lerp(0f, 1f, (float)r.NextDouble()),  
                            MathHelper.Lerp(0f, 1f, (float)r.NextDouble()), 0f);

                        for (int i = 0; i < 8; i++)
                        {
                            float angle = (float)(MathHelper.PiOver4 * i);
                            enemyCubes[cubeNumber].special = 600;
                            enemyCubes[cubeNumber].scale = 10.0f;
                            enemyCubes[cubeNumber].alive = true;
                            enemyCubes[cubeNumber].position = cubePack.position;

                            enemyCubes[cubeNumber].velocity = Vector3.Normalize(Vector3.Transform(scatterRot, Matrix.CreateRotationZ(angle)));
                            cubeNumber++;
                            if (cubeNumber == maxCubes)
                            {
                                cubeNumber = 0;
                            }
                        }
                        
                    }
                }
            }
            foreach (GameObject cube in enemyCubes)
            {
                if (cube.alive && bullet.alive)
                {
                    BoundingSphere cubesphere =
                        cube.model.Meshes[0].BoundingSphere;
                    cubesphere.Center = cube.position;
                    cubesphere.Radius += (cube.scale * 2);

                    if (cubesphere.Intersects(bulletsphere))
                    {
                        //SCORE
                        bullet.alive = false;
                        if (bullet.projType == 2) //projectile is laser
                        {
                            if (cube.special < 590) //lasers can't kill the cubes at the same time they spawn
                            {
                                cube.alive = false;
                            }
                            else
                            {
                                bullet.alive = true;
                            }

                        }
                        else if (bullet.projType == 3)//projectile is missile
                        {
                            DetonateMissile(bullet);
                        }
                        else if (bullet.projType == 4) //projectile is shield
                        {
                            bullet.alive = true;
                            if (cube.special < 595) //So shield doesn't kill all the cubes on spawn
                            {
                                cube.alive = false;
                                DamageShields(10f);
                            }
                        }
                        else if (bullet.projType == 5) //projectile is SHIP!!
                        {
                            ShipHit();
                            cube.alive = false;
                        }

                    }
                }
            }
            foreach (GameObject charger in enemyChargers)
            {
                if (charger.alive)
                {
                    BoundingSphere squaresphere =
                        charger.model.Meshes[0].BoundingSphere;
                    squaresphere.Center = charger.position;
                    squaresphere.Radius += (charger.scale * 2);

                    if (squaresphere.Intersects(bulletsphere))
                    {
                        //play sound
                        score++;
                        checkLevelUP();
                        bullet.alive = false;
                        if (bullet.projType == 4) //projectile is shield
                        {

                            bullet.alive = true;
                            DamageShields(10f);

                        }
                        else if (bullet.projType == 5) //projectile is SHIP!!
                        {
                            ShipHit();
                            score--;
                        }
                        else if (bullet.projType == 3)//projectile is missile
                        {
                            DetonateMissile(bullet);
                        }
                        charger.alive = false;
                        foreach (GameObject shield in enemyShields)
                        {
                            if (shield.projType == charger.projType)
                            {
                                if (shield.special >= 0)
                                {
                                    shield.alive = false;
                                }
                            }
                        }
                        break;
                    }
                }
            }
            foreach (GameObject reflector in enemyReflectors)
            {
                if (reflector.alive)
                {
                    BoundingSphere squaresphere =
                        reflector.model.Meshes[0].BoundingSphere;
                    squaresphere.Center = reflector.position;
                    squaresphere.Radius += (reflector.scale * 2);

                    if (squaresphere.Intersects(bulletsphere))
                    {
                        //play sound
                        score++;
                        checkLevelUP();
                        bullet.alive = false;
                        if (bullet.projType == 4) //projectile is shield
                        {

                            bullet.alive = true;
                            DamageShields(10f);

                        }
                        else if (bullet.projType == 5) //projectile is SHIP!!
                        {
                            ShipHit();
                            score--;
                        }
                        else if (bullet.projType == 3)//projectile is missile
                        {
                            DetonateMissile(bullet);
                        }
                        reflector.alive = false;
                        break;
                    }
                }
            }
            foreach (GameObject dodger in enemyDodgers)
            {
                if (dodger.alive)
                {
                    BoundingSphere squaresphere =
                        dodger.model.Meshes[0].BoundingSphere;
                    squaresphere.Center = dodger.position;
                    squaresphere.Radius += (dodger.scale * 2);

                    if (squaresphere.Intersects(bulletsphere))
                    {
                        //play sound
                        score++;
                        checkLevelUP();
                        bullet.alive = false;
                        if (bullet.projType == 4) //projectile is shield
                        {

                            bullet.alive = true;
                            DamageShields(10f);

                        }
                        else if (bullet.projType == 5) //projectile is SHIP!!
                        {
                            ShipHit();
                            score--;
                        }
                        else if (bullet.projType == 3)//projectile is missile
                        {
                            DetonateMissile(bullet);
                        }
                        dodger.alive = false;
                        break;
                    }
                }
            }
            foreach (GameObject snake in enemySnakes)
            {
                if (snake.alive)
                {
                    BoundingSphere squaresphere =
                        snake.model.Meshes[0].BoundingSphere;
                    squaresphere.Center = snake.position;
                    squaresphere.Radius += (snake.scale * 2);

                    if (squaresphere.Intersects(bulletsphere))
                    {
                        //play sound
                        score++;
                        checkLevelUP();
                        bullet.alive = false;
                        if (bullet.projType == 4) //projectile is shield
                        {

                            bullet.alive = true;
                            DamageShields(10f);

                        }
                        else if (bullet.projType == 5) //projectile is SHIP!!
                        {
                            ShipHit();
                            score--;
                        }
                        else if (bullet.projType == 3)//projectile is missile
                        {
                            DetonateMissile(bullet);
                        }
                        snake.alive = false;
                        break;
                    }
                }
            }
            foreach (GameObject coward in enemyCowards)
            {
                if (coward.alive)
                {
                    BoundingSphere squaresphere =
                        coward.model.Meshes[0].BoundingSphere;
                    squaresphere.Center = coward.position;
                    squaresphere.Radius += (coward.scale * 2);

                    if (squaresphere.Intersects(bulletsphere))
                    {
                        //play sound
                        score++;
                        checkLevelUP();
                        bullet.alive = false;
                        if (bullet.projType == 4) //projectile is shield
                        {

                            bullet.alive = true;
                            DamageShields(10f);

                        }
                        else if (bullet.projType == 5) //projectile is SHIP!!
                        {
                            ShipHit();
                            score--;
                        }
                        else if (bullet.projType == 3)//projectile is missile
                        {
                            DetonateMissile(bullet);
                        }
                        coward.alive = false;
                        break;
                    }
                }
            }
            foreach (GameObject food in enemyFood)
            {
                if (food.alive)
                {
                    BoundingSphere squaresphere =
                        food.model.Meshes[0].BoundingSphere;
                    squaresphere.Center = food.position;
                    squaresphere.Radius += (food.scale * 2);

                    if (squaresphere.Intersects(bulletsphere))
                    {
                        //play sound
                        score++;
                        checkLevelUP();
                        bullet.alive = false;
                        if (bullet.projType == 4) //projectile is shield
                        {

                            bullet.alive = true;
                            DamageShields(10f);

                        }
                        else if (bullet.projType == 5) //projectile is SHIP!!
                        {
                            ShipHit();
                            score--;
                        }
                        else if (bullet.projType == 3)//projectile is missile
                        {
                            DetonateMissile(bullet);
                        }
                        food.alive = false;
                        break;
                    }
                }
            }
            foreach (GameObject placeholder in enemyPlaceholder1)
            {
                if (placeholder.alive)
                {
                    BoundingSphere squaresphere =
                        placeholder.model.Meshes[0].BoundingSphere;
                    squaresphere.Center = placeholder.position;
                    squaresphere.Radius += (placeholder.scale * 2);

                    if (squaresphere.Intersects(bulletsphere))
                    {
                        //play sound
                        score++;
                        checkLevelUP();
                        bullet.alive = false;
                        if (bullet.projType == 4) //projectile is shield
                        {

                            bullet.alive = true;
                            DamageShields(10f);

                        }
                        else if (bullet.projType == 5) //projectile is SHIP!!
                        {
                            ShipHit();
                            score--;
                        }
                        else if (bullet.projType == 3)//projectile is missile
                        {
                            DetonateMissile(bullet);
                        }
                        placeholder.alive = false;
                        break;
                    }
                }
            }
            foreach (GameObject placeholder in enemyPlaceholder2)
            {
                if (placeholder.alive)
                {
                    BoundingSphere squaresphere =
                        placeholder.model.Meshes[0].BoundingSphere;
                    squaresphere.Center = placeholder.position;
                    squaresphere.Radius += (placeholder.scale * 2);

                    if (squaresphere.Intersects(bulletsphere))
                    {
                        //play sound
                        score++;
                        checkLevelUP();
                        bullet.alive = false;
                        if (bullet.projType == 4) //projectile is shield
                        {

                            bullet.alive = true;
                            DamageShields(10f);

                        }
                        else if (bullet.projType == 5) //projectile is SHIP!!
                        {
                            ShipHit();
                            score--;
                        }
                        else if (bullet.projType == 3)//projectile is missile
                        {
                            DetonateMissile(bullet);
                        }
                        placeholder.alive = false;
                        break;
                    }
                }
            }
            foreach (GameObject placeholder in enemyPlaceholder3)
            {
                if (placeholder.alive)
                {
                    BoundingSphere squaresphere =
                        placeholder.model.Meshes[0].BoundingSphere;
                    squaresphere.Center = placeholder.position;
                    squaresphere.Radius += (placeholder.scale * 2);

                    if (squaresphere.Intersects(bulletsphere))
                    {
                        //play sound
                        score++;
                        checkLevelUP();
                        bullet.alive = false;
                        if (bullet.projType == 4) //projectile is shield
                        {

                            bullet.alive = true;
                            DamageShields(10f);

                        }
                        else if (bullet.projType == 5) //projectile is SHIP!!
                        {
                            ShipHit();
                            score--;
                        }
                        else if (bullet.projType == 3)//projectile is missile
                        {
                            DetonateMissile(bullet);
                        }
                        placeholder.alive = false;
                        break;
                    }
                }
            }
            foreach (GameObject placeholder in enemyPlaceholder4)
            {
                if (placeholder.alive)
                {
                    BoundingSphere squaresphere =
                        placeholder.model.Meshes[0].BoundingSphere;
                    squaresphere.Center = placeholder.position;
                    squaresphere.Radius += (placeholder.scale * 2);

                    if (squaresphere.Intersects(bulletsphere))
                    {
                        //play sound
                        score++;
                        checkLevelUP();
                        bullet.alive = false;
                        if (bullet.projType == 4) //projectile is shield
                        {

                            bullet.alive = true;
                            DamageShields(10f);

                        }
                        else if (bullet.projType == 5) //projectile is SHIP!!
                        {
                            ShipHit();
                            score--;
                        }
                        else if (bullet.projType == 3)//projectile is missile
                        {
                            DetonateMissile(bullet);
                        }
                        placeholder.alive = false;
                        break;
                    }
                }
            }
        }


        void checkLevelUP()
        {
            if (score >= nextLevel)
            {
                level += 1;
                nextLevel = nextLevel * 2;
                upgrade = upgrade + 1;
                PromptPlayer("Level Up!", 180, .8f, .2f);


            }
        }
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
                // Otherwise move the player position.
                Vector2 movement = Vector2.Zero;

                if (keyboardState.IsKeyDown(Keys.Left))
                    movement.X--;

                if (keyboardState.IsKeyDown(Keys.Right))
                    movement.X++;

                if (keyboardState.IsKeyDown(Keys.Up))
                    movement.Y--;

                if (keyboardState.IsKeyDown(Keys.Down))
                    movement.Y++;

                Vector2 thumbstick = gamePadState.ThumbSticks.Left;

                movement.X += thumbstick.X;
                movement.Y -= thumbstick.Y;

                if (movement.Length() > 1)
                    movement.Normalize();

                playerPosition += movement * 2;
            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.Black);


            // TODO: Add your drawing code here
            DrawGameObject(ship); //draws the picture

            #region DrawGameObjects
            foreach (GameObject bullet in bullets)
            {
                if (bullet.alive)
                {
                    DrawGameObject(bullet);
                }
            }

            foreach (GameObject laser in lasers)
            {
                if (laser.alive)
                {
                    DrawGameObject(laser);
                }
            }
            foreach (GameObject missile in missiles)
            {
                if (missile.alive)
                {
                    DrawGameObject(missile);
                }
            }

            foreach (GameObject square in enemySquares)
            {
                if (square.alive)
                {
                    DrawGameObject(square);
                }
            }
            foreach (GameObject star in stars)
            {
                if (star.alive)
                {
                    DrawGameObject(star);
                }
            }
            foreach (GameObject cube in enemyCubes)
            {
                if (cube.alive)
                {
                    DrawGameObject(cube);
                }
            }
            foreach (GameObject charger in enemyChargers)
            {
                if (charger.alive)
                {
                    DrawGameObject(charger);
                }
            }
            foreach (GameObject cubePack in enemyCubePacks)
            {
                if (cubePack.alive)
                {
                    DrawGameObject(cubePack);
                }
            }
            foreach (GameObject reflector in enemyReflectors)
            {
                if (reflector.alive)
                {
                    DrawGameObject(reflector);
                }
            }
            foreach (GameObject dodger in enemyDodgers)
            {
                if (dodger.alive)
                {
                    DrawGameObject(dodger);
                }
            }
            foreach (GameObject snake in enemySnakes)
            {
                if (snake.alive)
                {
                    DrawGameObject(snake);
                }
            }
            foreach (GameObject coward in enemyCowards)
            {
                if (coward.alive)
                {
                    DrawGameObject(coward);
                }
            }
            foreach (GameObject food in enemyFood)
            {
                if (food.alive)
                {
                    DrawGameObject(food);
                }
            }
            foreach (GameObject placeholder in enemyPlaceholder1)
            {
                if (placeholder.alive)
                {
                    DrawGameObject(placeholder);
                }
            }
            foreach (GameObject placeholder in enemyPlaceholder2)
            {
                if (placeholder.alive)
                {
                    DrawGameObject(placeholder);
                }
            }
            foreach (GameObject placeholder in enemyPlaceholder3)
            {
                if (placeholder.alive)
                {
                    DrawGameObject(placeholder);
                }
            }
            foreach (GameObject placeholder in enemyPlaceholder4)
            {
                if (placeholder.alive)
                {
                    DrawGameObject(placeholder);
                }
            }
            foreach (GameObject shield in enemyShields)
            {
                if (shield.alive)
                {
                    DrawGameObject(shield);

                }
            }
            foreach (GameObject shield in shields)
            {
                if (shield.alive)
                {
                    DrawGameObject(shield);

                }
            }
            #endregion

            #region 2D UI
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred,
                SaveStateMode.SaveState);

            spriteBatch.DrawString(font,
                 "Guns: " + (spreadLvl - 2).ToString(),
                 new Vector2(0.06f * viewportRect.Width,
                 0.12f * viewportRect.Height),
                 Color.Green);
            spriteBatch.DrawString(font,
                "Laser: " + laserLvl.ToString(),
                new Vector2(0.06f * viewportRect.Width,
                0.02f * viewportRect.Height),
                Color.Yellow);
            spriteBatch.DrawString(font,
                "Missile: " + missileLvl.ToString(),
                new Vector2(0.11f * viewportRect.Width,
                0.07f * viewportRect.Height),
                Color.Red);

            spriteBatch.DrawString(font,
                "Shield: " + shieldLvl.ToString(),
                new Vector2(0.02f * viewportRect.Width,
                0.07f * viewportRect.Height),
                Color.Blue);
            spriteBatch.DrawString(font,
                "Upgrades: " + upgrade.ToString(),
                new Vector2(scoreDrawPoint.X * viewportRect.Width,
                scoreDrawPoint.Y * viewportRect.Height),
                Color.White);
            spriteBatch.DrawString(font,
                "Exp: " + score.ToString() + " / " + nextLevel.ToString(),
                new Vector2(0.8f * viewportRect.Width,
                0.15f * viewportRect.Height),
                Color.White);
            if (!ship.alive)
            {
                spriteBatch.DrawString(font,
                "Hull breached.  Game over.",
                new Vector2(0.5f * viewportRect.Width,
                0.5f * viewportRect.Height),
                Color.Red);
            }

            if (debugState)
            {
                spriteBatch.DrawString(font,
                 "Game Time: " + (DateTime.Now - gameStartTime).ToString(),
                 new Vector2(0.06f * viewportRect.Width,
                 0.18f * viewportRect.Height),
                 Color.White);
                spriteBatch.DrawString(font,
                 "Seconds: " + (DateTime.Now - gameStartTime).TotalSeconds.ToString(),
                 new Vector2(0.06f * viewportRect.Width,
                 0.21f * viewportRect.Height),
                 Color.White);
                spriteBatch.DrawString(font,
                 "Shields: " + (maxShieldStrength - shieldDamage).ToString(),
                 new Vector2(0.06f * viewportRect.Width,
                 0.24f * viewportRect.Height),
                 Color.White);
                spriteBatch.DrawString(font,
                "Pos: " + ship.position.ToString(),
                 new Vector2(0.06f * viewportRect.Width,
                 0.27f * viewportRect.Height),
                 Color.White);
               
            }


            for (int i = 0; i < maxMQueue; i++)
            {
                if (messageTicks[i] > 0)
                {

                    spriteBatch.DrawString(font,
                        messageQueue[i],
                        new Vector2(messagePos[i].X * viewportRect.Width,
                        messagePos[i].Y * viewportRect.Height),
                        Color.White);
                    messageTicks[i]--; // yeah, a little shady to do it here.

                }
            }
            #endregion

            //if(upgrade > 0)
            //{
            // Make this Move and possibly add a prompt (choose upgrade)
            //            spriteBatch.DrawString(font,
            //              "Level Up!", 
            //            new Vector2(0.5f * viewportRect.Width,
            //          0.15f * viewportRect.Height),
            //        Color.Gold);
            //}


            spriteBatch.End();
            base.Draw(gameTime);
            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }
        void PromptPlayer(string text, int ticks, float posX, float posY)
        {
            for (int i = 0; i < maxMQueue-1; i++)
            {
                if (messageTicks[i] <= 0 && ticks > 0)
                {
                    messageTicks[i] = ticks;
                    messageQueue[i] = text;
                    messagePos[i].X = posX;
                    messagePos[i].Y = posY;
                    ticks = 0;
                }
            }
            if (ticks > 0 && messageTicks[maxMQueue-1] == 0)
            {
                messageTicks[maxMQueue-1] = 180;
                messageQueue[maxMQueue-1] = "Message Queue Full" + text;
                messagePos[maxMQueue-1].X = .2f;
                messagePos[maxMQueue-1].Y = .5f;
            }

        }

        void DrawGameObject(GameObject gameobject) // draws shit. 
        {
            foreach (ModelMesh mesh in gameobject.model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    effect.World =  // TBH no fucking idea whats up here.
                        Matrix.CreateFromYawPitchRoll(
                        gameobject.rotation.Y,
                        gameobject.rotation.X,
                        gameobject.rotation.Z) *

                        Matrix.CreateScale(gameobject.scale) *

                        Matrix.CreateTranslation(gameobject.position);

                    effect.Projection = cameraProjectionMatrix;
                    effect.View = cameraViewMatrix;
                }
                mesh.Draw();
            }
        }

        #endregion
    }
}
