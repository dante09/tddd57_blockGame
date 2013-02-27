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
        public Color currentColor;
        private List<PoseType> shapeSelectionList;
        private SkeletonStreamRenderer skeletonStreamRenderer;
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
            renderDimensions = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            size = new Vector2(GraphicsDevice.Viewport.Width/2, GraphicsDevice.Viewport.Height);
            position = new Vector2(0, 0);
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
                    depthData = new short[frame.PixelDataLength];

                    depthTexture = new Texture2D(
                        Game.GraphicsDevice,
                        frame.Width,
                        frame.Height,
                        false,
                        SurfaceFormat.Bgra4444);

                    backBuffer = new RenderTarget2D(
                        Game.GraphicsDevice,
                        frame.Width/2,
                        frame.Height,
                        false,
                        SurfaceFormat.Color,
                        DepthFormat.None,
                        this.Game.GraphicsDevice.PresentationParameters.MultiSampleCount,
                        RenderTargetUsage.PreserveContents);
                }
                frame.CopyPixelDataTo(depthData);
                this.needToRedrawBackBuffer = true;
            }
        //    this.skeletonStreamRenderer.Update(gameTime);
            base.Update(gameTime);

        }

        public override void Draw(GameTime gameTime)
        {
            if (((KinectChooser)this.Game.Services.GetService(typeof(KinectChooser))).Sensor == null)
                return;
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

                depthTexture.SetData<short>(depthData);

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
            currentColor.A = (byte)(255 * Math.Min((double)poseKeptTime / 2000, 1.0));
            //Length of one block unit
            double distance;
            float rotation;
            double minDistance = 50;
            switch (currentPoseStatus.closestPose)
            {
                case PoseType.O:
                    DepthImagePoint oShoulderLeft = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[0], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint oShoulderRight = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[1], DepthImageFormat.Resolution640x480Fps30);

                    distance = Math.Sqrt(Math.Pow(oShoulderLeft.Y - oShoulderRight.Y, 2) + Math.Pow(oShoulderLeft.X - oShoulderRight.X, 2))/1.5;
                    distance = Math.Max(minDistance, distance);
                    rotation = (float)Math.Atan((double)(oShoulderLeft.Y - oShoulderRight.Y) / (double)(oShoulderLeft.X - oShoulderRight.X));

                    DrawRectangle(spriteBatch, oShoulderLeft.X-distance/3 , oShoulderLeft.Y - distance / 2, distance * 2, distance * 2, rotation, currentColor);
                    break;
                case PoseType.L:
                    DepthImagePoint lWristLeft = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[0], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint lShoulderCenter = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[1], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint lSpine = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[2], DepthImageFormat.Resolution640x480Fps30);

                    distance = Math.Sqrt(Math.Pow(lWristLeft.Y - lShoulderCenter.Y, 2) + Math.Pow(lWristLeft.X - lShoulderCenter.X, 2)) / 2;
                    distance = Math.Max(minDistance, distance);
                    rotation = (float)-Math.Atan((double)(lShoulderCenter.X - lSpine.X) / (double)(lShoulderCenter.Y - lSpine.Y));

                    DrawRectangle(spriteBatch, lShoulderCenter.X - distance / 2, lShoulderCenter.Y - distance / 2, distance, distance * 3, rotation, currentColor);
                    DrawRectangle(spriteBatch, lShoulderCenter.X - distance / 2, lShoulderCenter.Y - distance / 2, distance, distance, rotation + Math.PI / 2, currentColor);
                    break;
                case PoseType.J:
                    DepthImagePoint jWristRight = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[0], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint jShoulderCenter = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[1], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint jSpine = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[2], DepthImageFormat.Resolution640x480Fps30);

                    distance = Math.Sqrt(Math.Pow(jWristRight.Y - jShoulderCenter.Y, 2) + Math.Pow(jWristRight.X - jShoulderCenter.X, 2)) / 2;
                    distance = Math.Max(minDistance, distance);
                    rotation = (float)-Math.Atan((double)(jShoulderCenter.X - jSpine.X) / (double)(jShoulderCenter.Y - jSpine.Y));
                    
                    DrawRectangle(spriteBatch, jShoulderCenter.X + distance / 2, jShoulderCenter.Y - distance / 2, distance, distance, rotation, currentColor);
                    DrawRectangle(spriteBatch, jShoulderCenter.X + distance / 2, jShoulderCenter.Y - distance / 2, distance*3, distance, rotation + Math.PI / 2, currentColor);
                    break;
                case PoseType.T:
                    DepthImagePoint tWristLeft = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[0], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint tWristRight = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[1], DepthImageFormat.Resolution640x480Fps30);

                    distance = Math.Sqrt(Math.Pow(tWristLeft.Y - tWristRight.Y, 2) + Math.Pow(tWristLeft.X - tWristRight.X, 2)) / 5;
                    distance = Math.Max(minDistance, distance);
                    rotation = (float)Math.Atan((double)(tWristLeft.Y - tWristRight.Y) / (double)(tWristLeft.X - tWristRight.X));

                    DrawRectangle(spriteBatch, tWristLeft.X + 2 * distance, tWristLeft.Y + distance / 2, distance, distance, rotation, currentColor);
                    DrawRectangle(spriteBatch, tWristLeft.X + 2 * distance, tWristLeft.Y + distance / 2, distance, distance * 2, rotation - Math.PI / 2, currentColor);
                    DrawRectangle(spriteBatch, tWristLeft.X + 2 * distance, tWristLeft.Y + distance / 2, distance, distance, rotation - Math.PI, currentColor);
                    break;
                case PoseType.I:
                    DepthImagePoint iWristLeft = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[0], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint iWristRight = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[1], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint iSpine = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[2], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint iWrists = new DepthImagePoint();
                    iWrists.X = (iWristLeft.X + iWristRight.X) / 2;
                    iWrists.Y = (iWristLeft.Y + iWristRight.Y) / 2;
                    distance = Math.Sqrt(Math.Pow(iWrists.Y - iSpine.Y, 2) + Math.Pow(iWrists.X - iSpine.X, 2)) / 4;
                    distance = Math.Max(minDistance, distance);
                    rotation = (float)-Math.Atan((double)(iWrists.X - iSpine.X) / (double)(iWrists.Y - iSpine.Y));

                    DrawRectangle(spriteBatch, iWrists.X - distance / 2, iWrists.Y, distance, distance * 4, rotation, currentColor);
                    break;
                case PoseType.S:
                    DepthImagePoint sWristLeft = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[0], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint sShoulderCenter = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[1], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint sElbowLeft = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[2], DepthImageFormat.Resolution640x480Fps30);
                    distance = Math.Abs(sWristLeft.X - sShoulderCenter.X) / 2;
                    distance = Math.Max(minDistance, distance);
                    rotation = (float)Math.Atan((double)(sWristLeft.Y - sElbowLeft.Y) / (double)(sWristLeft.X - sElbowLeft.X));

                    DrawRectangle(spriteBatch, sWristLeft.X + distance / 2, sWristLeft.Y + distance / 2, distance, distance * 2, rotation - Math.PI/2, currentColor);
                    DrawRectangle(spriteBatch, sWristLeft.X + distance / 2, sWristLeft.Y + distance / 2, distance, distance, rotation, currentColor);
                    DrawRectangle(spriteBatch, sWristLeft.X + distance / 2, sWristLeft.Y + distance / 2, distance, distance, rotation + Math.PI/2, currentColor);
                    break;
                case PoseType.Z:
                    DepthImagePoint zWristRight = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[0], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint zShoulderCenter = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[1], DepthImageFormat.Resolution640x480Fps30);
                    DepthImagePoint zElbowRight = coordinateMapper.MapSkeletonPointToDepthPoint(currentPoseStatus.pointsOfInterest[2], DepthImageFormat.Resolution640x480Fps30);
                    distance = Math.Abs(zShoulderCenter.X - zWristRight.X) / 2;
                    distance = Math.Max(minDistance, distance);
                    rotation = (float)Math.Atan((double)(zWristRight.Y - zElbowRight.Y) / (double)(zWristRight.X - zElbowRight.X));

                    DrawRectangle(spriteBatch, zWristRight.X - distance / 2, zWristRight.Y + distance / 2, distance, distance, rotation+Math.PI/2, currentColor);
                    DrawRectangle(spriteBatch, zWristRight.X - distance / 2, zWristRight.Y + distance / 2, distance * 2, distance, rotation + Math.PI, currentColor);
                    DrawRectangle(spriteBatch, zWristRight.X - distance / 2, zWristRight.Y + distance / 2, distance, distance, rotation, currentColor);
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
            double height = 50;
            //Distance between shapes and between shapes and edge.
            double padding = 5;
            //This is the length of a drawing window for a single tetris block. Every block unit will be of length length/4.
            double length = height - 2 * padding;
            Vector2 position = new Vector2((float)padding, (float)padding);
            int count = 0;
            spriteBatch.Begin();
            foreach (PoseType p in shapeSelectionList)
            {
                //Calculating the x-position is a bit tricky.
                position.X = (float)(renderDimensions.X/2 + (float)(count - (float)(shapeSelectionList.Count/2))*(length + padding) + padding/2);
                switch (p)
                {
                    case PoseType.O:
                        DrawRectangle(spriteBatch, position.X + length / 4, position.Y + length / 4, length / 2, length / 2, 0, Color.Red);
                        break;
                    case PoseType.L:
                        DrawRectangle(spriteBatch, position.X + length / 4, position.Y + length / 8, length / 4, length / 4, 0, Color.Red);
                        DrawRectangle(spriteBatch, position.X + length / 2, position.Y + length / 8, length / 4, length * 3/4, 0, Color.Red);
                        break;
                    case PoseType.J:
                        DrawRectangle(spriteBatch, position.X + length / 2, position.Y + length / 8, length / 4, length / 4, 0, Color.Red);
                        DrawRectangle(spriteBatch, position.X + length / 4, position.Y + length / 8, length / 4, length * 3 / 4, 0, Color.Red);
                        break;
                    case PoseType.T:
                        DrawRectangle(spriteBatch, position.X + length * 3 / 8, position.Y + length / 2, length / 4, length / 4, 0, Color.Red);
                        DrawRectangle(spriteBatch, position.X + length / 8, position.Y + length / 4, length * 3 / 4, length / 4, 0, Color.Red);
                        break;
                    case PoseType.I:
                        DrawRectangle(spriteBatch, position.X + length * 3 / 8, position.Y, length / 4, length, 0, Color.Red);
                        break;
                    case PoseType.S:
                        DrawRectangle(spriteBatch, position.X + length / 4, position.Y + length / 8, length / 4, length / 2, 0, Color.Red);
                        DrawRectangle(spriteBatch, position.X + length / 2, position.Y + length * 3/8, length / 4, length / 2, 0, Color.Red);
                        break;
                    case PoseType.Z:
                        DrawRectangle(spriteBatch, position.X + length / 4, position.Y + length * 3 / 8, length / 4, length / 2, 0, Color.Red);
                        DrawRectangle(spriteBatch, position.X + length / 2, position.Y + length / 8, length / 4, length / 2, 0, Color.Red);
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

        private void DrawRectangle(SpriteBatch spriteBatch, double x, double y, double width, double height, double rotation, Color color)
        {
            spriteBatch.Draw(
                            texture,
                            new Rectangle((int)x, (int)y, (int)width, (int)height),
                            null,
                            color,
                            (float)rotation,
                            new Vector2(0, 0),
                            SpriteEffects.None,
                            0);
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
