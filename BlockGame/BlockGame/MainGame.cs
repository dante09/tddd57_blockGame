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
        //Time before a block moves down one step in ms
        private int tickTime = 500;
        private int timeSinceLastTick = 0;

        private int width = 1000;
        //private readonly Rectangle viewPortRectangle;

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

            placingRenderer = new BlockPlacingRenderer(this);

            blockCreator = new BlockCreationHumanPlayer();
            creationRenderer = new BlockCreationRenderer(this, blockCreator.shapeSelectionList);

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
            blockPlacer = new BlockPlacerHumanPlayer();
            gameField = new GameField();
            Components.Add(placingRenderer);
            Components.Add(creationRenderer);
            Components.Add(skeletonManager);
            Services.AddService(typeof(SkeletonStreamManager), skeletonManager);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);

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

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (skeletonManager.creatorPlayer != null && !blockLockedIn)
            {
                PoseStatus currentStatus = blockCreator.GetBlock(skeletonManager.creatorPlayer);
                //System.Diagnostics.Debug.WriteLine(currentStatus);
                if (lastPose != PoseType.NO_POSE && currentStatus.closestPose == lastPose)
                    poseKeptTime += gameTime.ElapsedGameTime.Milliseconds;
                else
                    poseKeptTime = 0;
                creationRenderer.poseKeptTime = poseKeptTime;
                creationRenderer.currentPoseStatus = currentStatus;
                lastPose = currentStatus.closestPose;

                //If a pose has been kept for a certain amount of time 
                if (poseKeptTime >= 2000)
                {
                    blockLockedIn = true;
                    blockCreator.RemoveShape(currentStatus.closestPose);
                    gameField.LockShape(currentStatus.closestPose);
                }
            }

            if (skeletonManager.creatorPlayer != null)
            {
                timeSinceLastTick += gameTime.ElapsedGameTime.Milliseconds;
                PlayerMove move = blockPlacer.PlaceBlock(null,new Point(0,0));
                //gameField.MakeMove(move);
                if (timeSinceLastTick >= tickTime)
                {
                    if (gameField.MoveTimeStep())
                    {
                        blockLockedIn = false;
                    }
                    timeSinceLastTick = 0;
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
            GraphicsDevice.Clear(Color.White);
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
