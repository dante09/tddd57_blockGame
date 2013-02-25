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
       public override PlayerMove PlaceBlock(Point rightHand,Point leftHand,Point blockCenter)
        {


           System.Diagnostics.Debug.WriteLine(rightHand.X + " " + blockCenter.X);
           //If hand is not far ahead of body, we don't need to make a move
           //if (rightHand.Position.Z > skel.Joints[JointType.ShoulderCenter].Position.Z+0.3)
           //     return PlayerMove.NO_MOVE;
           if (rightHand.X-60 > blockCenter.X)
               return PlayerMove.GO_LEFT;
           if (rightHand.X+60 < blockCenter.X)
               return PlayerMove.GO_RIGHT;
           return PlayerMove.NO_MOVE;
        }
    }
}
