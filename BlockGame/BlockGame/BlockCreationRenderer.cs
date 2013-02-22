using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Kinect;
using System.Collections;

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
        public PoseStatus currentPoseStatus { private get; set; }
        public int poseKeptTime { private get; set; }
        private Color currentColor;
        private List<PoseType> shapeSelectionList;
        private SkeletonStreamRenderer skeletonStreamRenderer;
        private Random colorGenerator;
        private SpriteFont font;
        private Vector2 renderDimensions;

        public BlockCreationRenderer(Game game, List<PoseType> shapeSelectionList) : base(game)
        {
            skeletonStreamRenderer = new SkeletonStreamRenderer(game, this.SkeletonToDepthMap);
            this.shapeSelectionList = shapeSelectionList;
        }

        public override void Initialize()
        {
            base.Initialize();
            size = new Vector2(GraphicsDevice.Viewport.Width/2, GraphicsDevice.Viewport.Height);

            position = new Vector2(0, 0);
            colorGenerator = new Random();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            kinectDepthVisualizer = Game.Content.Load<Effect>("KinectDepthVisualizer");
            this.texture = Game.Content.Load<Texture2D>("Bone");
            this.font = Game.Content.Load<SpriteFont>("Segoe16");
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
                drawShapeSelectionList(spriteBatch);
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
            if (poseKeptTime == 0)
                currentColor = randomColor();
            currentColor.A = (byte)(255 * Math.Min((double)poseKeptTime / 2000, 1.0));
            //Length of one block unit
            double distance;
            float rotation;
            switch (currentPoseStatus.closestPose)
            {
                case PoseType.O:
                    DepthImagePoint oElbowLeft = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[0], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint oElbowRight = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[1], DepthImageFormat.Resolution640x480Fps30);

                    distance = Math.Sqrt(Math.Pow(oElbowLeft.Y - oElbowRight.Y, 2) + Math.Pow(oElbowLeft.X - oElbowRight.X, 2))/2;
                    rotation = (float)Math.Atan((double)(oElbowLeft.Y - oElbowRight.Y) / (double)(oElbowLeft.X - oElbowRight.X));
                    spriteBatch.Draw(
                        texture,
                        new Rectangle(oElbowLeft.X, oElbowLeft.Y, (int)distance*2, (int)distance*2),
                        null,
                        currentColor,
                        rotation,
                        new Vector2(0, 0),
                        SpriteEffects.None,
                        0);
                    break;
                case PoseType.L:
                    DepthImagePoint lWristLeft = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[0], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint lShoulderCenter = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[1], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint lSpine = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[2], DepthImageFormat.Resolution640x480Fps30);

                    distance = Math.Sqrt(Math.Pow(lWristLeft.Y - lShoulderCenter.Y, 2) + Math.Pow(lWristLeft.X - lShoulderCenter.X, 2)) / 2;
                    rotation = (float)-Math.Atan((double)(lShoulderCenter.X - lSpine.X) / (double)(lShoulderCenter.Y - lSpine.Y));
                    spriteBatch.Draw(
                        texture,
                        new Rectangle(lShoulderCenter.X - (int)(distance / 2), lShoulderCenter.Y - (int)(distance / 2), (int)distance, (int)(distance * 3)),
                        null,
                        currentColor,
                        rotation,
                        new Vector2(0, 0),
                        SpriteEffects.None,
                        0);
                    spriteBatch.Draw(
                        texture,
                        new Rectangle(lShoulderCenter.X - (int)(distance / 2), lShoulderCenter.Y - (int)(distance / 2), (int)distance, (int)distance),
                        null,
                        currentColor,
                        (float)(rotation + Math.PI / 2),
                        new Vector2(0, 0),
                        SpriteEffects.None,
                        0);
                    break;
                case PoseType.J:
                    DepthImagePoint jWristRight = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[0], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint jShoulderCenter = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[1], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint jSpine = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[2], DepthImageFormat.Resolution640x480Fps30);

                    distance = Math.Sqrt(Math.Pow(jWristRight.Y - jShoulderCenter.Y, 2) + Math.Pow(jWristRight.X - jShoulderCenter.X, 2)) / 2;
                    rotation = (float)-Math.Atan((double)(jShoulderCenter.X - jSpine.X) / (double)(jShoulderCenter.Y - jSpine.Y));
                    spriteBatch.Draw(
                        texture,
                        new Rectangle(jShoulderCenter.X + (int)(distance / 2), jShoulderCenter.Y - (int)(distance / 2), (int)distance, (int)distance),
                        null,
                        currentColor,
                        rotation,
                        new Vector2(0, 0),
                        SpriteEffects.None,
                        0);
                    spriteBatch.Draw(
                        texture,
                        new Rectangle(jShoulderCenter.X + (int)(distance / 2), jShoulderCenter.Y - (int)(distance / 2), (int)(distance * 3), (int)distance),
                        null,
                        currentColor,
                        (float)(rotation + Math.PI / 2),
                        new Vector2(0, 0),
                        SpriteEffects.None,
                        0);
                    break;
                case PoseType.T:
                    DepthImagePoint tWristLeft = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[0], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint tWristRight = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[1], DepthImageFormat.Resolution640x480Fps30);

                    distance = Math.Sqrt(Math.Pow(tWristLeft.Y - tWristRight.Y, 2) + Math.Pow(tWristLeft.X - tWristRight.X, 2)) / 5;
                    rotation = (float)-Math.Atan((double)(tWristLeft.X - tWristRight.X) / (double)(tWristLeft.Y - tWristRight.Y));
                    spriteBatch.Draw(
                        texture,
                        new Rectangle((int)(tWristLeft.X + 2 * distance), (int)(tWristLeft.Y + distance / 2), (int)distance, (int)distance),
                        null,
                        currentColor,
                        rotation,
                        new Vector2(0, 0),
                        SpriteEffects.None,
                        0);
                    spriteBatch.Draw(
                        texture,
                        new Rectangle((int)(tWristLeft.X + 2 * distance), (int)(tWristLeft.Y + distance / 2), (int)distance, (int)(distance * 2)),
                        null,
                        currentColor,
                        (float)(rotation + Math.PI/2),
                        new Vector2(0, 0),
                        SpriteEffects.None,
                        0);
                    spriteBatch.Draw(
                        texture,
                        new Rectangle((int)(tWristLeft.X + 2 * distance), (int)(tWristLeft.Y + distance / 2), (int)distance, (int)distance),
                        null,
                        currentColor,
                        (float)(rotation + Math.PI),
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

        private void drawShapeSelectionList(SpriteBatch spriteBatch)
        {
            //Bad code, but it solves the centering problem.
            renderDimensions.X = GraphicsDevice.Viewport.Width;
            renderDimensions.Y = GraphicsDevice.Viewport.Height;
            double height = 70;
            //Distance between shapes and between shapes and edge.
            double padding = 5;
            //This is the length of a drawing window for a single tetris block. Every block unit will be of length length/4.
            double length = height - 2 * padding;
            Vector2 position = new Vector2((float)padding, (float)padding);
            int count = 0;
            spriteBatch.Begin();
            foreach (PoseType p in shapeSelectionList)
            {
                System.Diagnostics.Debug.WriteLine(p.ToString());
                //Calculating the x-position is a bit tricky.
                position.X = (float)(renderDimensions.X/2 + (float)(count - (float)(shapeSelectionList.Count/2))*(length + padding) + padding/2);
                switch (p)
                {
                    case PoseType.O:
                        spriteBatch.Draw(
                            texture,
                            new Rectangle((int)(position.X+length/4), (int)(position.Y+length/4), (int)(length/2), (int)length/2),
                            null,
                            Color.Red,
                            0,
                            new Vector2(0, 0),
                            SpriteEffects.None,
                            0);
                        break;
                    case PoseType.L:
                        spriteBatch.Draw(
                            texture,
                            new Rectangle((int)(position.X + length / 4), (int)(position.Y + length / 8), (int)(length / 4), (int)length / 4),
                            null,
                            Color.Red,
                            0,
                            new Vector2(0, 0),
                            SpriteEffects.None,
                            0);
                        spriteBatch.Draw(
                            texture,
                            new Rectangle((int)(position.X + length / 2), (int)(position.Y + length / 8), (int)(length / 4), (int)length * 3/4),
                            null,
                            Color.Red,
                            0,
                            new Vector2(0, 0),
                            SpriteEffects.None,
                            0);
                        break;
                    case PoseType.J:
                        spriteBatch.Draw(
                            texture,
                            new Rectangle((int)(position.X + length / 2), (int)(position.Y + length / 8), (int)(length / 4), (int)length / 4),
                            null,
                            Color.Red,
                            0,
                            new Vector2(0, 0),
                            SpriteEffects.None,
                            0);
                        spriteBatch.Draw(
                            texture,
                            new Rectangle((int)(position.X + length / 4), (int)(position.Y + length / 8), (int)(length / 4), (int)length * 3 / 4),
                            null,
                            Color.Red,
                            0,
                            new Vector2(0, 0),
                            SpriteEffects.None,
                            0);
                        break;
                    case PoseType.T:
                        spriteBatch.Draw(
                            texture,
                            new Rectangle((int)(position.X + length * 3/8), (int)(position.Y + length / 4), (int)(length / 4), (int)length / 4),
                            null,
                            Color.Red,
                            0,
                            new Vector2(0, 0),
                            SpriteEffects.None,
                            0);
                        spriteBatch.Draw(
                            texture,
                            new Rectangle((int)(position.X + length / 8), (int)(position.Y + length / 2), (int)(length *  3/4), (int)length / 4),
                            null,
                            Color.Red,
                            0,
                            new Vector2(0, 0),
                            SpriteEffects.None,
                            0);
                        break;
                    case PoseType.NO_POSE:
                    default:
                        spriteBatch.DrawString(font, p.ToString(), position, Color.Black);
                        break;
                }

                count++;
            }
            spriteBatch.End();
        }

        //Color generation and management should be in MainGame
        private Color randomColor()
        {
            Color color = new Color();
            color.R = (byte)((50 + colorGenerator.Next(0,999) * 205) / 1000);
            color.G = (byte)((50 + colorGenerator.Next(0,999) * 205) / 1000);
            color.B = (byte)((50 + colorGenerator.Next(0,999) * 205) / 1000);
            color.A = 255;
            return color;
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
