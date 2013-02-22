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
       public override PlayerMove PlaceBlock(Skeleton skel,Point blockCenter)
        {
            if (skel == null)
                return PlayerMove.NO_MOVE;
           Joint rightHand = skel.Joints[JointType.HandRight];
           Joint leftHand = skel.Joints[JointType.HandLeft];
           
           //If hand is not far ahead of body, we don't need to make a move
           if (rightHand.Position.Z > skel.Joints[JointType.ShoulderCenter].Position.Z+0.3)
                return PlayerMove.NO_MOVE;
           if (rightHand.Position.X > blockCenter.X)
               return PlayerMove.GO_LEFT;
           if (rightHand.Position.X < blockCenter.X)
               return PlayerMove.GO_RIGHT;
           return PlayerMove.NO_MOVE;
        }
    }
}
