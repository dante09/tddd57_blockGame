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
        private int playerCheckInTime = 0;
        private int elapsedGameTime = 0;
        private int pauseTime = 0;
        //Splash screen and icons
        private Texture2D splashScreen;
        private Texture2D twoPlayers;

        private int width = 1000;

        private Random colorGenerator;

        private GameState gameState;
        private enum GameState
        {
            SHOWING_SPLASH_SCREEN,
            PLAYING_GAME,
            SHOWING_INSTRUCTIONS
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
            splashScreen = Content.Load<Texture2D>("tetris");
            twoPlayers = Content.Load<Texture2D>("two_players");

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
            nbrPlayers = 0;
            placingRenderer = new BlockPlacingRenderer(this, gameField);

            blockCreator = blockCreationPlayer;
            creationRenderer = new BlockCreationRenderer(this, blockCreator.shapeSelectionList);
            blockPlacer = blockPlacerPlayer;
            creationRenderer.currentColor = RandomColor();
            elapsedGameTime = 0;
            gameState = GameState.PLAYING_GAME;
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
                    Instructions();
                    break;
            }
            base.Update(gameTime);
        }

        private void ChooseNbrOfPlayers(GameTime gameTime)
        {
            Skeleton creatorPlayer = skeletonManager.creatorPlayer;
            Skeleton placerPlayer = skeletonManager.placerPlayer;
            int nbrPlayers = 0;

            if (placerPlayer != null && placerPlayer.Joints[JointType.WristLeft].Position.Y
                    > placerPlayer.Joints[JointType.ShoulderCenter].Position.Y)
                    nbrPlayers++;
            if (creatorPlayer != null && creatorPlayer.Joints[JointType.WristLeft].Position.Y
                    > creatorPlayer.Joints[JointType.ShoulderCenter].Position.Y)
                nbrPlayers++;

            if (nbrPlayers!=0 && this.nbrPlayers == nbrPlayers)
                playerCheckInTime += gameTime.ElapsedGameTime.Milliseconds;
            else
                playerCheckInTime = 0;

            this.nbrPlayers = nbrPlayers;
            //Change to reasonable time
            if (playerCheckInTime >= 5000)
            {
                //Just for testing
                if(nbrPlayers==2)
                    NewGame(new BlockCreationHumanPlayer(), new BlockPlacerHumanPlayer());
                else
                    NewGame(new BlockCreationComputerPlayer(), new BlockPlacerHumanPlayer());
                gameState = GameState.SHOWING_INSTRUCTIONS;
            }          
        }

        private void Instructions()
        {
            Components.Add(placingRenderer);
            Components.Add(creationRenderer);
            gameState = GameState.PLAYING_GAME;
        }

        private void TetrisGameLoop(GameTime gameTime)
        {
            Skeleton creatorPlayer = skeletonManager.creatorPlayer;
            Skeleton placerPlayer = skeletonManager.placerPlayer;
            if (!blockCreator.isHuman)
                creatorPlayer = placerPlayer;

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
                pauseTime += gameTime.ElapsedGameTime.Milliseconds / 1000;
            }

            if (gameField.gameOver || pauseTime >= 60)
            {
                gameState = GameState.SHOWING_SPLASH_SCREEN;
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

                if (nbrPlayers == 2)
                {
                    Color fadeIn = Color.LightGreen;
                    fadeIn.A = (byte)(playerCheckInTime * 255 / 5000);
                    spriteBatch.Begin();
                    spriteBatch.Draw(twoPlayers,new Rectangle(GraphicsDevice.Viewport.Width/2,GraphicsDevice.Viewport.Height/2,
                        100,100),fadeIn);
                    spriteBatch.End();
                }
                else if(nbrPlayers == 1)
                {
                    Color fadeIn = Color.MediumPurple;
                    fadeIn.A = (byte)(playerCheckInTime * 255 / 5000);
                    spriteBatch.Begin();
                    spriteBatch.Draw(twoPlayers, new Rectangle(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2,
                        100, 100), fadeIn);
                    spriteBatch.End();
                } 
            }
            else if (gameState == GameState.SHOWING_INSTRUCTIONS)
            {

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

    }
}
