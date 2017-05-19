using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace _3DSnekUlt
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphicsDeviceManager;
        ColllisionDetector collisionDetector;
        InputManager inputManager;
        Player player;
        Bounds bounds;
        Vector3 foodLocation;
        VisualOutputManager visualOutputManager;
        PointSystem pointSystem;
        Random rand;
        double gameTickTimer;//used for determining when a game tick should happen
        double tickFrequency;
        int superFoodFrequency = 3;//change to 4?
        float enrageDuration = 4000f,
            elapsedEnrageTime;
        private int gridSpaceFactor,//the dist from one grid location to the next (displacement when player moves)
                    countDownToNextSuperFood;//how many more food spawns until the next super food spawn
        bool foodIsSuperFood,//if the current food piece will grant a boost
                gameNotOver;//if the game has been lost
        Song zavodIronFoundry;
        bool musicPlaying;

        public Game1()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);
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
            // TODO: Add your initialization logic here
            base.Initialize();
            rand = new Random();
            gridSpaceFactor = 140;
            player = new Player(gridSpaceFactor);
            collisionDetector = new ColllisionDetector(gridSpaceFactor);
            inputManager = new InputManager();
            bounds.set(13, -13, 13, -13);//boundaries of the map grid (might need to change these to 14 if it does not look right)
            setFoodPosition();
            pointSystem = new PointSystem();
            visualOutputManager = new VisualOutputManager(graphicsDeviceManager, GraphicsDevice, Content);
            gameTickTimer = 0;
            tickFrequency = 300;
            countDownToNextSuperFood = superFoodFrequency;
            foodIsSuperFood = false;
            gameNotOver = true;
            zavodIronFoundry = Content.Load<Song>("Music/12 ALEXANDER MOSOLOV - Zavod [Iron Foundry]");
            musicPlaying = false;   
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
        
            inputManager.handleCameraControl(visualOutputManager);
            progressTheMusic();
            if (gameNotOver)
            {
                gameTickTimer += gameTime.ElapsedGameTime.Milliseconds;//accumulate time until we can update again
                if (player.enraged)
                {
                    elapsedEnrageTime += gameTime.ElapsedGameTime.Milliseconds;
                    if(elapsedEnrageTime > enrageDuration)
                    {
                        tickFrequency = 300;
                        elapsedEnrageTime = 0;
                        player.enraged = false;
                    }
                }
                if (gameTickTimer > tickFrequency)//If it has been long enough since last update
                {
                    inputManager.handleMotionControl(player);//player must be holding key down in the appropriate cycle for it to be processed
                                                             //Move the snake and check for collisions
                    player.move();
                    if (collisionDetector.checkAgainstWalls(player, bounds))
                    {
                        Console.Out.WriteLine("Player hit wall and DIED");
                        gameNotOver = false;
                    }
                    if (collisionDetector.checkAgainstTail(player))//this check needs to happen before checking food collection so that adding first tail piece does not cause this to return true
                    {
                        Console.Out.WriteLine("Play ran into its tail");
                        gameNotOver = false;
                    }
                    if (collisionDetector.checkIfCollectingFood(player, foodLocation))
                    {
                        Console.Out.WriteLine("Collected food yo");
                        if (foodIsSuperFood)//if player just ate a super food
                        {
                            player.enraged = true;
                            foodIsSuperFood = false;
                            tickFrequency = 150;
                            visualOutputManager.startVideo();//play the crazy dog man video!
                        }
                        if(--countDownToNextSuperFood == 0)//if it is time for a super food to spawn
                        {
                            countDownToNextSuperFood = superFoodFrequency;//reset countdown
                            foodIsSuperFood = true;
                        }
                        setFoodPosition();
                        player.addTailPiece();
                    }

                    gameTickTimer = 0; //reset for timing the next gameTick
                }
            }
            else //if game is over
            {
                bool needToRestart;
                inputManager.handleGameOverMenuChoice(this, out needToRestart);
                if( needToRestart)
                {
                    this.Initialize();
                }
            }
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);//could replace with black when player has died lel

            if (gameNotOver)
            {
                visualOutputManager.draw(player, foodLocation, foodIsSuperFood);
            }
            else
            {
                visualOutputManager.drawGameOverMenu();
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// Set the new food to a valid position.
        /// </summary>
        private void setFoodPosition()
        {
            int newx, newz;
            newx = rand.Next(bounds.xmin, bounds.xmax + 1);
            newz = rand.Next(bounds.zmin, bounds.zmax + 1);
            Vector3 newFoodPosition = new Vector3(newx * gridSpaceFactor, 0, newz * gridSpaceFactor);

            while(!collisionDetector.validFoodPosition(player, newFoodPosition))//until the rng provides a valid food location (a bad implementation lel)
            {
                newx = rand.Next(bounds.xmin, bounds.xmax + 1);
                newz = rand.Next(bounds.zmin, bounds.zmax + 1);
                newFoodPosition = new Vector3(newx * gridSpaceFactor, 0, newz * gridSpaceFactor);
            }

            foodLocation = newFoodPosition;
        }

        /// <summary>
        /// Simple background music management
        /// </summary>
        private void progressTheMusic()
        {
            if (musicPlaying)
            {

            }
            else
            {
                MediaPlayer.Play(zavodIronFoundry);
                musicPlaying = true;
            }
        }
    }
}
