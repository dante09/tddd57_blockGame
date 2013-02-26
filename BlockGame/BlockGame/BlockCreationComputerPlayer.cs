using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace BlockGame
{
    class BlockCreationComputerPlayer : BlockCreationPlayer
    {
        public BlockCreationComputerPlayer()
            : base()
        {
            isHuman = false;
        }

        override public PoseStatus GetBlock(Skeleton skel)
        {
            return new PoseStatus(shapeSelectionList.ElementAt(0),1,skel);
        }
    }
}
