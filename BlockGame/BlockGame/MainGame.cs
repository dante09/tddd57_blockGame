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

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            chooser = new KinectChooser(this, ColorImageFormat.RgbResolution640x480Fps30, DepthImageFormat.Resolution640x480Fps30);
            Components.Add(this.chooser);
            Services.AddService(typeof(KinectChooser), this.chooser);

            skeletonManager = new SkeletonStreamManager(this);
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
            if (skeletonManager.currentSkeleton != null&&!blockLockedIn)
            {
                PoseStatus currentStatus = blockCreator.GetBlock(skeletonManager.currentSkeleton);
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

            if (true/*skeletonManager.currentSkeleton != null*/)
            {
                timeSinceLastTick += gameTime.ElapsedGameTime.Milliseconds;
                PlayerMove move = blockPlacer.PlaceBlock(null);
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
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
