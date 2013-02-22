using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlockGame
{
    class BlockPlacingRenderer : DrawableGameComponent
    {
        /// <summary>
        /// The last frame of color data.
        /// </summary>
        private byte[] colorData;

        /// <summary>
        /// The color frame as a texture.
        /// </summary>
        private Texture2D colorTexture;

        /// <summary>
        /// The back buffer where color frame is scaled as requested by the Size.
        /// </summary>
        private RenderTarget2D backBuffer;
        
        /// <summary>
        /// This Xna effect is used to swap the Red and Blue bytes of the color stream data.
        /// </summary>
        private Effect kinectColorVisualizer;

        /// <summary>
        /// Whether or not the back buffer needs updating.
        /// </summary>
        private bool needToRedrawBackBuffer = true;

        private Vector2 position;
        private Vector2 size;

        /// <summary>
        /// Initializes a new instance of the ColorStreamRenderer class.
        /// </summary>
        /// <param name="game">The related game object.</param>
        public BlockPlacingRenderer(Game game)
            : base(game)
        {
        }

        /// <summary>
        /// Initializes the necessary children.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            size = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height);
            position = new Vector2(GraphicsDevice.Viewport.Width / 2+5, 0);
        }

        /// <summary>
        /// The update method where the new color frame is retrieved.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            KinectChooser chooser = (KinectChooser)this.Game.Services.GetService(typeof(KinectChooser));

            // If the sensor is not found, not running, or not connected, stop now
            if (null == chooser.Sensor ||
                false == chooser.Sensor.IsRunning ||
                KinectStatus.Connected != chooser.Sensor.Status)
            {
                return;
            }

            using (var frame = chooser.Sensor.ColorStream.OpenNextFrame(0))
            {
                // Sometimes we get a null frame back if no data is ready
                if (frame == null)
                {
                    return;
                }
                // Reallocate values if necessary
                if (this.colorData == null || this.colorData.Length != frame.PixelDataLength)
                {
                    this.colorData = new byte[frame.PixelDataLength];

                    this.colorTexture = new Texture2D(
                        this.Game.GraphicsDevice, 
                        frame.Width, 
                        frame.Height, 
                        false, 
                        SurfaceFormat.Color);

                    this.backBuffer = new RenderTarget2D(
                        this.Game.GraphicsDevice, 
                        frame.Width, 
                        frame.Height, 
                        false, 
                        SurfaceFormat.Color, 
                        DepthFormat.None,
                        this.Game.GraphicsDevice.PresentationParameters.MultiSampleCount, 
                        RenderTargetUsage.PreserveContents);            
                }

                frame.CopyPixelDataTo(this.colorData);
                needToRedrawBackBuffer = true;
            }
        }

        /// <summary>
        /// This method renders the color and skeleton frame.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            // If we don't have the effect load, load it
            if (null == this.kinectColorVisualizer)
            {
                LoadContent();
            }

            // If we don't have a target, don't try to render
            if (null == colorTexture)
            {
                return;
            }

            if (this.needToRedrawBackBuffer)
            {
                // Set the backbuffer and clear
                Game.GraphicsDevice.SetRenderTarget(this.backBuffer);
                Game.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

                colorTexture.SetData<byte>(this.colorData);

                // Draw the color image
                spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, this.kinectColorVisualizer);
                spriteBatch.Draw(this.colorTexture, Vector2.Zero, Color.White);
                spriteBatch.End();

                // Reset the render target and prepare to draw scaled image
                this.Game.GraphicsDevice.SetRenderTargets(null);

                // No need to re-render the back buffer until we get new data
                this.needToRedrawBackBuffer = false;
            }

            // Draw the scaled texture
            spriteBatch.Begin();
            spriteBatch.Draw(
                this.backBuffer,
                new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y),
                null,
                Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// This method loads the Xna effect.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // This effect is necessary to remap the BGRX byte data we get
            // to the XNA color RGBA format.
            this.kinectColorVisualizer = Game.Content.Load<Effect>("KinectColorVisualizer");
        }
    }
}
