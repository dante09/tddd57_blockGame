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
            referencePos = new Queue<Point>();
        }
        private PlayerMove lastMove = PlayerMove.NO_MOVE;
        private int moveTracker = 0;
        private Queue<Point> referencePos;

        //For now this returns a gamefield, needs to be changed
        public override PlayerMove PlaceBlock(Point rightHand,Point leftHand,Point blockCenter)
        {
            referencePos.Enqueue(rightHand);
            if (referencePos.Count < 10)
                return PlayerMove.NO_MOVE;
            Point point = referencePos.Dequeue();
            if (rightHand.X > point.X + 250)
            {
                referencePos.Clear();
                return PlayerMove.GO_RIGHT;
            }
            if (rightHand.X < point.X - 250)
            {
                referencePos.Clear();
                return PlayerMove.GO_LEFT;
            }
            //If hand is not far ahead of body, we don't need to make a move
            //if (rightHand.Position.Z > skel.Joints[JointType.ShoulderCenter].Position.Z+0.3)
            //     return PlayerMove.NO_MOVE;
            /*
            if (rightHand.X > blockCenter.X + 20)
            {
                if (lastMove == PlayerMove.GO_RIGHT && moveTracker > 10)
                {
                    lastMove = PlayerMove.GO_RIGHT;
                    moveTracker = 0;
                    return PlayerMove.GO_RIGHT;
                }
                else if (lastMove == PlayerMove.GO_RIGHT)
                {
                    moveTracker++;
                }
                else
                {
                    moveTracker = 0;
                }
                lastMove = PlayerMove.GO_RIGHT;
                return PlayerMove.NO_MOVE;
            }
            if (rightHand.X < blockCenter.X - 20)
            {
                if (lastMove == PlayerMove.GO_LEFT && moveTracker > 10)
                {
                    lastMove = PlayerMove.GO_LEFT;
                    moveTracker = 0;
                    return PlayerMove.GO_LEFT;
                }
                else if (lastMove == PlayerMove.GO_LEFT)
                {
                    moveTracker++;
                }
                else
                {
                    moveTracker = 0;
                }
                lastMove = PlayerMove.GO_LEFT;
                return PlayerMove.NO_MOVE;
            }
            */
            moveTracker = 0;
            lastMove = PlayerMove.NO_MOVE;
            return PlayerMove.NO_MOVE;
        }
    }
}
