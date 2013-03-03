using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using System.ComponentModel;

namespace BlockGame
{
    public delegate Skeleton GetSkeleton(); 
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MainGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private KinectChooser chooser;

        //Renderers
        private SkeletonStreamManager skeletonManager; 
        private BlockCreationRenderer creationRenderer;
        private BlockPlacingRenderer placingRenderer;
        //Players
        private BlockCreationPlayer blockCreator;
        private BlockPlacerPlayer blockPlacer;
        //Game field for tetris
        private GameField gameField;
        //Diffrent flags to keep record of the state we are in
        private PoseType lastPose = PoseType.NO_POSE;
        private int poseKeptTime = 0;
        private bool blockLockedIn = false;
        private int nbrPlayers = 0;
        //Timers
        private const int tickTime = 1000;
        private int timeSinceLastTick = 0;
        private int elapsedGameTime = 0;
        private double pauseTime = 0;
        private double gameOverTime = 0;
        //Splash screen and icons
        private Texture2D splashScreen;
        private Texture2D texture;
        private Texture2D instruction1A;
        private Texture2D instruction1B;
        private Texture2D instruction2A;
        private Texture2D instruction2B;
        private Texture2D gameOver;
        //GestureRecognizers
        private GestureRecognizer creatorRecognizer;
        private GestureRecognizer placerRecognizer;
        //Font
        private SpriteFont font;

        //Width of screen
        private int width = 1000;
        //Variable to randomize the colors of blocks
        private Random colorGenerator;
        //State of what we are showing on the screen
        private GameState gameState;
        private enum GameState
        {
            SHOWING_SPLASH_SCREEN,
            PLAYING_GAME,
            SHOWING_INSTRUCTIONS,
            GAME_OVER
        }

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            this.graphics.PreferredBackBufferWidth = width;
            this.graphics.PreferredBackBufferHeight = (int)((double)2/3*width);
            this.graphics.PreparingDeviceSettings += this.GraphicsDevicePreparingDeviceSettings;
            this.graphics.SynchronizeWithVerticalRetrace = true;
            //this.viewPortRectangle = new Rectangle(10, 80, width - 20, ((width - 2) / 4) * 3);
            Content.RootDirectory = "Content";

            chooser = new KinectChooser(this, ColorImageFormat.RgbResolution640x480Fps30, DepthImageFormat.Resolution640x480Fps30);
            Components.Add(this.chooser);
            Services.AddService(typeof(KinectChooser), this.chooser);

            skeletonManager = new SkeletonStreamManager(this);
            gameField = new GameField();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            colorGenerator = new Random();
            gameState = GameState.SHOWING_SPLASH_SCREEN;
            Components.Add(skeletonManager);
            Services.AddService(typeof(SkeletonStreamManager), skeletonManager);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);

            font = Content.Load<SpriteFont>("Segoe16");

            //For now we always load all the images and nevr unload them. Could be optimized
            instruction1A = Content.Load<Texture2D>("instructions1A");
            instruction1B = Content.Load<Texture2D>("instructions1B");
            instruction2A = Content.Load<Texture2D>("instructions2A");
            instruction2B = Content.Load<Texture2D>("instructions2B");
            gameOver = Content.Load<Texture2D>("gameover");
            splashScreen = Content.Load<Texture2D>("titleScreen");
            texture = Content.Load<Texture2D>("Bone");

            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private void NewGame(BlockCreationPlayer blockCreationPlayer, BlockPlacerPlayer blockPlacerPlayer)
        {
            gameField.Clear();
            placingRenderer = new BlockPlacingRenderer(this, gameField);

            blockCreator = blockCreationPlayer;
            creationRenderer = new BlockCreationRenderer(this, blockCreator.shapeSelectionList);
            blockPlacer = blockPlacerPlayer;
            creationRenderer.currentColor = RandomColor();
            elapsedGameTime = 0;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (chooser.Sensor == null)
                return;
            switch (gameState)
            {
                case GameState.PLAYING_GAME:
                    TetrisGameLoop(gameTime);
                    break;
                case GameState.SHOWING_SPLASH_SCREEN:
                    ChooseNbrOfPlayers(gameTime);
                    break;
                case GameState.SHOWING_INSTRUCTIONS:
                    Instructions(gameTime);
                    break;
                case GameState.GAME_OVER:
                    gameOverTime += (double)gameTime.ElapsedGameTime.Milliseconds / 1000;
                    if (gameOverTime >= 10)
                    {
                        gameState = GameState.SHOWING_SPLASH_SCREEN;
                        gameOverTime = 0;
                        ChooseNbrOfPlayers(gameTime);
                    }
                    break;
            }
            base.Update(gameTime);
        }

        private void ChooseNbrOfPlayers(GameTime gameTime)
        {
            if (creatorRecognizer == null || placerRecognizer == null)
            {
                creatorRecognizer = new LeftArmUp(this, GetCreator, 10, 5000);
                placerRecognizer = new LeftArmUp(this, GetPlacer, 10, 5000);
                Components.Add(creatorRecognizer);
                Components.Add(placerRecognizer);
                System.Diagnostics.Debug.WriteLine("Setting up recognizers");
            }
            int nbrPlayers = 0;
            if (creatorRecognizer.GestureStarted())
                nbrPlayers++;
            if (placerRecognizer.GestureStarted())
                nbrPlayers++;

            if (creatorRecognizer.gestureComplete || placerRecognizer.gestureComplete)
            {
                System.Diagnostics.Debug.WriteLine("Pose complete");
                //Remove recongizers
                Components.Remove(creatorRecognizer);
                Components.Remove(placerRecognizer);
                creatorRecognizer = null;
                placerRecognizer = null;

                if (nbrPlayers == 2)
                    NewGame(new BlockCreationHumanPlayer(), new BlockPlacerHumanPlayer());
                else
                    NewGame(new BlockCreationComputerPlayer(), new BlockPlacerHumanPlayer());
                gameState = GameState.SHOWING_INSTRUCTIONS;
                Instructions(gameTime);
            }
            else if (this.nbrPlayers != 0 && this.nbrPlayers != nbrPlayers)
            {
                System.Diagnostics.Debug.WriteLine("Reset recognizers");
                creatorRecognizer.Reset();
                placerRecognizer.Reset();
            }
            this.nbrPlayers = nbrPlayers;
        }

        private void Instructions(GameTime gameTime)
        {
            if (placerRecognizer == null && this.nbrPlayers == 2)
            {
                creatorRecognizer = new RightArmUp(this, GetCreator, 10, 5000);
                placerRecognizer = new RightArmUp(this, GetPlacer, 10, 5000);
                Components.Add(creatorRecognizer);
                Components.Add(placerRecognizer);
            }
            else if (placerRecognizer == null)
            {
                placerRecognizer = new RightArmUp(this, GetPlacer, 10, 5000);
                Components.Add(placerRecognizer);
            }

            if ((creatorRecognizer==null || creatorRecognizer.gestureComplete) && placerRecognizer.gestureComplete)
            {
                //Remove recongizers
                Components.Remove(creatorRecognizer);
                Components.Remove(placerRecognizer);
                creatorRecognizer = null;
                placerRecognizer = null;

                Components.Add(placingRenderer);
                Components.Add(creationRenderer);
                gameState = GameState.PLAYING_GAME;
            }
        }

        private void TetrisGameLoop(GameTime gameTime)
        {
            Skeleton creatorPlayer = skeletonManager.creatorPlayer;
            Skeleton placerPlayer = skeletonManager.placerPlayer;
            if (!blockCreator.isHuman)
                creatorPlayer = placerPlayer;

            //Create block
            if (creatorPlayer != null && !blockLockedIn)
            {
                PoseStatus currentStatus = blockCreator.GetBlock(creatorPlayer);
                if (lastPose != PoseType.NO_POSE && currentStatus.closestPose == lastPose)
                {
                    poseKeptTime += gameTime.ElapsedGameTime.Milliseconds;
                }
                else if (poseKeptTime > 500)
                {
                    creationRenderer.currentColor = RandomColor();
                    poseKeptTime = 0;
                }
                else
                {
                    poseKeptTime = 0;
                }
                creationRenderer.poseKeptTime = poseKeptTime;
                creationRenderer.currentPoseStatus = currentStatus;
                lastPose = currentStatus.closestPose;

                //If a pose has been kept for a certain amount of time 
                if (poseKeptTime >= 2000)
                {
                    poseKeptTime = 0;
                    blockLockedIn = true;
                    blockCreator.RemoveShape(currentStatus.closestPose);
                    gameField.LockShape(currentStatus.closestPose, creationRenderer.currentColor);
                }
            }
            //Place block
            if (placerPlayer != null)
            {
                pauseTime = 0;
                elapsedGameTime += gameTime.ElapsedGameTime.Milliseconds;
                gameField.gameSpeed = 1 + 0.1 * (int)(elapsedGameTime / 60000);
                timeSinceLastTick += gameTime.ElapsedGameTime.Milliseconds;
                PlayerMove move = blockPlacer.PlaceBlock(placerPlayer);

                gameField.MakeMove(move);
                if (timeSinceLastTick >= tickTime / gameField.gameSpeed)
                {
                    if (gameField.MoveTimeStep())
                    {
                        //When releasing a locked block, generate a new color.
                        if (blockLockedIn)
                            creationRenderer.currentColor = RandomColor();
                        blockLockedIn = false;
                    }
                    timeSinceLastTick = 0;
                }
                placingRenderer.animationFactor = (double)timeSinceLastTick / (double)tickTime;
            }
            else
            {
                pauseTime += (double)gameTime.ElapsedGameTime.Milliseconds / 1000;
            }
            //If game is over, or noone has been playing for a while, quit
            if (gameField.gameOver || pauseTime >= 60)
            {
                gameState = GameState.GAME_OVER;
                nbrPlayers = 0;
                Components.Remove(placingRenderer);
                Components.Remove(creationRenderer);
            }
        }

        //Color generation for blocks
        private Color RandomColor()
        {
            Color color = new Color();
            color.R = (byte)((50 + colorGenerator.Next(0, 999) * 205) / 1000);
            color.G = (byte)((50 + colorGenerator.Next(0, 999) * 205) / 1000);
            color.B = (byte)((50 + colorGenerator.Next(0, 999) * 205) / 1000);
            color.A = 255;
            return color;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (chooser.Sensor == null)
                return;

            GraphicsDevice.Clear(Color.White);
            if (gameState==GameState.SHOWING_SPLASH_SCREEN)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(splashScreen,new Rectangle(0,0,GraphicsDevice.Viewport.Width,GraphicsDevice.Viewport.Height)
                    ,Color.White);
                spriteBatch.End();

                Color fadeIn = Color.Green;
                double fadeInFactor = (double)placerRecognizer.gestureKeptTime / (double)placerRecognizer.holdFor;
                fadeIn.A = (byte)(255 * fadeInFactor);
                if (nbrPlayers == 2)
                {
                    spriteBatch.Begin();
                    spriteBatch.Draw(texture, new Rectangle((int)(GraphicsDevice.Viewport.Width * 0.6288), (int)(GraphicsDevice.Viewport.Height * 0.8905),
                        (int)(GraphicsDevice.Viewport.Width * 0.1702 * fadeInFactor), 30), fadeIn);
                    spriteBatch.End();
                }
                else if(nbrPlayers == 1)
                {
                    spriteBatch.Begin();
                    spriteBatch.Draw(texture, new Rectangle((int)(GraphicsDevice.Viewport.Width * 0.1702), (int)(GraphicsDevice.Viewport.Height * 0.8905),
                        (int)(GraphicsDevice.Viewport.Width * 0.1702 * fadeInFactor), 30), fadeIn);
                    spriteBatch.End();
                } 
            }
            else if (gameState == GameState.SHOWING_INSTRUCTIONS)
            {
                if (nbrPlayers == 2)
                {
                    Color placerFadeIn = Color.Green;
                    Color creatorFadeIn = Color.Green;
                    double placerFadeInFactor = (double)placerRecognizer.gestureKeptTime / (double)placerRecognizer.holdFor;
                    double creatorFadeInFactor = (double)creatorRecognizer.gestureKeptTime / (double)creatorRecognizer.holdFor;
                    placerFadeIn.A = (byte)(255 * placerFadeInFactor);
                    creatorFadeIn.A = (byte)(255 * creatorFadeInFactor);
                    spriteBatch.Begin();
                    spriteBatch.Draw((gameTime.TotalGameTime.Seconds % 2 > 0 ? instruction2A : instruction2B),
                        new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
                        , Color.White);
                    if(creatorRecognizer.GestureStarted())
                        spriteBatch.Draw(texture, new Rectangle((int)(GraphicsDevice.Viewport.Width * 0.4143), (int)(GraphicsDevice.Viewport.Height * 0.9623),
                            (int)(GraphicsDevice.Viewport.Width * 0.13 * creatorFadeInFactor), 30), creatorFadeIn);
                    if (placerRecognizer.GestureStarted())
                        spriteBatch.Draw(texture, new Rectangle((int)(GraphicsDevice.Viewport.Width * (0.4143 + 0.13 * creatorFadeInFactor)), (int)(GraphicsDevice.Viewport.Height * 0.9623),
                            (int)(GraphicsDevice.Viewport.Width * 0.13 * placerFadeInFactor), 30), placerFadeIn);
                    spriteBatch.End();
                }
                else if(nbrPlayers == 1)
                {
                    Color fadeIn = Color.Green;
                    double placerFadeInFactor = (double)placerRecognizer.gestureKeptTime / (double)placerRecognizer.holdFor;
                    fadeIn.A = (byte)(255 * placerFadeInFactor);
                    System.Diagnostics.Debug.WriteLine("fadeIn.A: " + fadeIn.A);
                    spriteBatch.Begin();
                    
                    spriteBatch.Draw((gameTime.TotalGameTime.Seconds % 2 > 0 ? instruction1A : instruction1B),
                        new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
                        , Color.White);
                    if (placerRecognizer.GestureStarted())
                        spriteBatch.Draw(texture, new Rectangle((int)(GraphicsDevice.Viewport.Width * 0.4130), (int)(GraphicsDevice.Viewport.Height * 0.9623),
                            (int)(GraphicsDevice.Viewport.Width * 0.1831 * placerFadeInFactor), 30), fadeIn);
                    spriteBatch.End();
                }
            }
            else if (gameState == GameState.GAME_OVER)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(gameOver, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
                    , Color.White);
                spriteBatch.End();

                spriteBatch.Begin();
                spriteBatch.DrawString(font, "Du fick "+gameField.score +" apelsiner! ",
                    new Vector2(GraphicsDevice.Viewport.Width / 2 - 100, GraphicsDevice.Viewport.Height / 2), Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// This method ensures that we can render to the back buffer without
        /// losing the data we already had in our previous back buffer.  This
        /// is necessary for the SkeletonStreamRenderer.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="e">The event args.</param>
        private void GraphicsDevicePreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // This is necessary because we are rendering to back buffer/render targets and we need to preserve the data
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }

        public Skeleton GetCreator()
        {
            return skeletonManager.creatorPlayer;
        }

        public Skeleton GetPlacer()
        {
            return skeletonManager.placerPlayer;
        }
    }
}
