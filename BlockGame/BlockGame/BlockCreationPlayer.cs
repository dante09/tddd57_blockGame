using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace BlockGame
{
    abstract class BlockCreationPlayer : Player
    {
        public abstract PoseStatus GetBlock(Skeleton skel);
    }
}
