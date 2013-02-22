using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Kinect;

namespace BlockGame
{
    class BlockCreationRenderer : DrawableGameComponent
    {

        /// <summary>
        /// The back buffer where the depth frame is scaled as requested by the Size.
        /// </summary>
        private RenderTarget2D backBuffer;

        /// <summary>
        /// The last frame of depth data.
        /// </summary>
        private short[] depthData;

        /// <summary>
        /// The depth frame as a texture.
        /// </summary>
        private Texture2D depthTexture;

        /// <summary>
        /// This Xna effect is used to convert the depth to RGB color information.
        /// </summary>
        private Effect kinectDepthVisualizer;

        /// <summary>
        /// The bone texture.
        /// </summary>
        private Texture2D texture;

        /// <summary>
        /// Whether or not the back buffer needs updating.
        /// </summary>
        private bool needToRedrawBackBuffer = true;

        private Vector2 position;
        private Vector2 size;
        public PoseStatus currentPose { private get; set; }
        public double shapeOpacityLevel { private get; set; }
        private SkeletonStreamRenderer skeletonStreamRenderer;

        public BlockCreationRenderer(Game game) : base(game)
        {
            skeletonStreamRenderer = new SkeletonStreamRenderer(game, this.SkeletonToDepthMap);
        }

        public override void Initialize()
        {
            base.Initialize();
            size = new Vector2(GraphicsDevice.Viewport.Width/2, GraphicsDevice.Viewport.Height);
            position = new Vector2(0, 0);

        }

        protected override void LoadContent()
        {
            base.LoadContent();
            kinectDepthVisualizer = Game.Content.Load<Effect>("KinectDepthVisualizer");
            this.texture = Game.Content.Load<Texture2D>("Bone");
        }

        public override void Update(GameTime gameTime)
        {
            KinectChooser chooser = (KinectChooser)this.Game.Services.GetService(typeof(KinectChooser));

            if (null == chooser.Sensor ||
                false == chooser.Sensor.IsRunning ||
                KinectStatus.Connected != chooser.Sensor.Status)
            {
                return;
            }

            using (var frame = chooser.Sensor.DepthStream.OpenNextFrame(0))
            {
                // Sometimes we get a null frame back if no data is ready
                if (null == frame)
                    return;
                // Reallocate values if necessary
                if (null == depthData || depthData.Length != frame.PixelDataLength)
                {
                    this.depthData = new short[frame.PixelDataLength];

                    this.depthTexture = new Texture2D(
                        Game.GraphicsDevice,
                        frame.Width,
                        frame.Height,
                        false,
                        SurfaceFormat.Bgra4444);

                    this.backBuffer = new RenderTarget2D(
                        Game.GraphicsDevice,
                        frame.Width,
                        frame.Height,
                        false,
                        SurfaceFormat.Color,
                        DepthFormat.None,
                        this.Game.GraphicsDevice.PresentationParameters.MultiSampleCount,
                        RenderTargetUsage.PreserveContents);
                }

                frame.CopyPixelDataTo(this.depthData);
                this.needToRedrawBackBuffer = true;
            }
        //    this.skeletonStreamRenderer.Update(gameTime);
            base.Update(gameTime);

        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            if (this.depthTexture == null)
            {
                return;
            }

            if (this.needToRedrawBackBuffer)
            {

                // Set the backbuffer and clear
                Game.GraphicsDevice.SetRenderTarget(this.backBuffer);
                Game.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

                this.depthTexture.SetData<short>(this.depthData);

                // Draw the depth image
                spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, kinectDepthVisualizer);
                spriteBatch.Draw(this.depthTexture, Vector2.Zero, Color.White);
                spriteBatch.End();

                drawShape(spriteBatch);
             //   this.skeletonStreamRenderer.Draw(gameTime);

                // Reset the render target and prepare to draw scaled image
                Game.GraphicsDevice.SetRenderTarget(null);

                // No need to re-render the back buffer until we get new data
                needToRedrawBackBuffer = false;
            }

            // Draw scaled image
            spriteBatch.Begin();
            spriteBatch.Draw(
                backBuffer,
                new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y),
                null,
                Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void drawShape(SpriteBatch spriteBatch)
        {
            CoordinateMapper coordinateMapper = ((KinectChooser)Game.Services.GetService(typeof(KinectChooser))).coordinateMapper;
            spriteBatch.Begin();
            Color color = Color.Chartreuse;
            color.A = (byte)shapeOpacityLevel;
            //Length of one block unit
            double distance;
            float rotation;
            switch (currentPose.closestPose)
            {
                case PoseType.O:
                    DepthImagePoint elbowLeft = coordinateMapper.MapSkeletonPointToDepthPoint(currentPose.pointsOfInterest[0], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint elbowRight = coordinateMapper.MapSkeletonPointToDepthPoint(currentPose.pointsOfInterest[1], DepthImageFormat.Resolution640x480Fps30);

                    distance = Math.Sqrt(Math.Pow(elbowLeft.Y - elbowRight.Y, 2) + Math.Pow(elbowLeft.X - elbowRight.X, 2))/2;
                    rotation = (float)Math.Atan((double)(elbowLeft.Y - elbowRight.Y) / (double)(elbowLeft.X - elbowRight.X));
                    spriteBatch.Draw(
                        texture,
                        new Rectangle(elbowLeft.X, elbowLeft.Y, (int)distance*2, (int)distance*2),
                        null,
                        color,
                        rotation,
                        new Vector2(0, 0),
                        SpriteEffects.None,
                        0);
                    break;
                case PoseType.L:
                    DepthImagePoint wristLeft = coordinateMapper.MapSkeletonPointToDepthPoint(currentPose.pointsOfInterest[0], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint shoulderCenter = coordinateMapper.MapSkeletonPointToDepthPoint(currentPose.pointsOfInterest[1], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint spine = coordinateMapper.MapSkeletonPointToDepthPoint(currentPose.pointsOfInterest[2], DepthImageFormat.Resolution640x480Fps30);

                    distance = Math.Sqrt(Math.Pow(wristLeft.Y - shoulderCenter.Y, 2) + Math.Pow(wristLeft.X - shoulderCenter.X, 2))/2;
                    rotation = (float)-Math.Atan((double)(shoulderCenter.X - spine.X) / (double)(shoulderCenter.Y - spine.Y));
                    spriteBatch.Draw(
                        texture,
                        new Rectangle(shoulderCenter.X - (int)(distance/2), shoulderCenter.Y - (int)(distance/2), (int)distance, (int)(distance*3)),
                        null,
                        color,
                        rotation,
                        new Vector2(0, 0),
                        SpriteEffects.None,
                        0);
                    spriteBatch.Draw(
                        texture,
                        new Rectangle(shoulderCenter.X - (int)(distance/2), shoulderCenter.Y - (int)(distance/2), (int)distance, (int)distance),
                        null,
                        color,
                        (float)(rotation+Math.PI/2),
                        new Vector2(0, 0),
                        SpriteEffects.None,
                        0);
                    break;
                case PoseType.NO_POSE:
                default:
                    break;
            }


            spriteBatch.End();


        }



        private Vector2 SkeletonToDepthMap(SkeletonPoint point)
        {
            KinectChooser Chooser = (KinectChooser)Game.Services.GetService(typeof(KinectChooser));
            if ((null != Chooser.Sensor) && (null != Chooser.Sensor.DepthStream))
            {
                // This is used to map a skeleton point to the depth image location
                var depthPt = Chooser.Sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(point, Chooser.Sensor.DepthStream.Format);
                return new Vector2(depthPt.X, depthPt.Y);
            }

            return Vector2.Zero;
        }


    }
}
