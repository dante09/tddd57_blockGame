using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace BlockGame
{
    class LeftArmUp : GestureRecognizer
    {
        public LeftArmUp(Game game, Skeleton skel, int errorCount, int holdFor)
            : base(game, skel,errorCount,holdFor)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
