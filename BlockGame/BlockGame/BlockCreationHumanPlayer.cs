using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace BlockGame
{
    class BlockCreationHumanPlayer : BlockCreationPlayer
    {
        override public PoseStatus GetBlock(Skeleton skel)
        {
            return PoseHandler.Evaluate(skel);
        }
    }
}
