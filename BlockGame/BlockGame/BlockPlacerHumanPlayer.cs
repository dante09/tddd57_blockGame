using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace BlockGame
{
    class BlockPlacerHumanPlayer : BlockPlacerPlayer
    {
       public  BlockPlacerHumanPlayer() : base()
       {
       }

       //For now this returns a gamefield, needs to be changed
       public override PlayerMove PlaceBlock(Skeleton skel)
        {
            return PlayerMove.NO_MOVE;
        }
    }
}
