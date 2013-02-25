using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;

namespace BlockGame
{
    abstract class BlockPlacerPlayer : Player
    {

        public BlockPlacerPlayer()
        {
        }

        public abstract PlayerMove PlaceBlock(Point rightHand,Point leftHand,Point blockCenter);
    }

    public enum PlayerMove
    {
        ROTATE_LEFT,
        ROTATE_RIGHT,
        GO_DOWN,
        GO_RIGHT,
        GO_LEFT,
        NO_MOVE
    }
}
