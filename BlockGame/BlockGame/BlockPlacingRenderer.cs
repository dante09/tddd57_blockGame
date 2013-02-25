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
        private GameField gameField;
        private Texture2D texture;
        private Vector2 renderDimensions;
        public double animationFactor;

        /// <summary>
        /// Initializes a new instance of the ColorStreamRenderer class.
        /// </summary>
        /// <param name="game">The related game object.</param>
        public BlockPlacingRenderer(Game game, GameField gameField)
            : base(game)
        {
            this.gameField = gameField;
        }

        /// <summary>
        /// Initializes the necessary children.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            renderDimensions = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            size = new Vector2(renderDimensions.X, renderDimensions.Y);
            position = new Vector2(0, 0);

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
                if (colorData == null || colorData.Length != frame.PixelDataLength)
                {
                    colorData = new byte[frame.PixelDataLength];

                    colorTexture = new Texture2D(
                        this.Game.GraphicsDevice, 
                        frame.Width, 
                        frame.Height, 
                        false, 
                        SurfaceFormat.Color);

                    backBuffer = new RenderTarget2D(
                        this.Game.GraphicsDevice, 
                        frame.Width, 
                        frame.Height, 
                        false, 
                        SurfaceFormat.Color, 
                        DepthFormat.None,
                        this.Game.GraphicsDevice.PresentationParameters.MultiSampleCount, 
                        RenderTargetUsage.PreserveContents);            
                }
                
                frame.CopyPixelDataTo(colorData);
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

            if (needToRedrawBackBuffer)
            {
                // Set the backbuffer and clear
                Game.GraphicsDevice.SetRenderTarget(this.backBuffer);
                Game.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

                colorTexture.SetData<byte>(colorData);

                // Draw the color image
                spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, kinectColorVisualizer);
                spriteBatch.Draw(colorTexture, Vector2.Zero, Color.White);
                spriteBatch.End();

                // Reset the render target and prepare to draw scaled image
                Game.GraphicsDevice.SetRenderTargets(null);

                // No need to re-render the back buffer until we get new data
                needToRedrawBackBuffer = false;
            }

            // Draw the scaled texture
            spriteBatch.Begin();
            spriteBatch.Draw(
                backBuffer,
                new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y),
                null,
                Color.White);
            spriteBatch.End();

            DrawGameField(spriteBatch);
            DrawActiveBlock(spriteBatch);

            base.Draw(gameTime);
        }

        private void DrawGameField(SpriteBatch spriteBatch)
        {
            //Bad code, but it solves the centering problem.
            renderDimensions.X = GraphicsDevice.Viewport.Width/2;
            renderDimensions.Y = GraphicsDevice.Viewport.Height;
            spriteBatch.Begin();
            for (int x = 0; x < GameField.width; x++)
                for (int y = 0; y < GameField.height; y++)
                    if (gameField.field[x, y] == 1)
                        DrawBlock(spriteBatch, x, y, gameField.fieldColor[x, y]);
            spriteBatch.End();
        }

        private void DrawActiveBlock(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            for(int i=0; i<gameField.humanPosition.Length; i++)
                //Could check both x and y, but doesnt really need to.
                if(gameField.humanPosition[i].X != -1)
                    DrawBlock(spriteBatch, gameField.humanPosition[i].X, (double)gameField.humanPosition[i].Y + (animationFactor-1), gameField.humanColor);
            spriteBatch.End();
        }

        //Draws a block at the specified coordinates with the specified color.
        private void DrawBlock(SpriteBatch spriteBatch, double x, double y, Color color)
        {
            Vector2 size = new Vector2(renderDimensions.X / GameField.width, renderDimensions.Y / (GameField.height - GameField.invisibleRows));
            Vector2 position = new Vector2((float)(x * size.X + renderDimensions.X), (float)((y - GameField.invisibleRows) * size.Y));
            spriteBatch.Draw(
                texture,
                new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y),
                null,
                color,
                0,
                new Vector2(0, 0),
                SpriteEffects.None,
                0);
        }

        /// <summary>
        /// This method loads the Xna effect.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();
            this.texture = Game.Content.Load<Texture2D>("Bone");

            // This effect is necessary to remap the BGRX byte data we get
            // to the XNA color RGBA format.
            this.kinectColorVisualizer = Game.Content.Load<Effect>("KinectColorVisualizer");
        }
    }
}
