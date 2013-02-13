using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        /// Whether or not the back buffer needs updating.
        /// </summary>
        private bool needToRedrawBackBuffer = true;

        public BlockCreationRenderer(Game game) : base(game)
        {}

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
 	         base.Draw(gameTime);
        }
    }
}
