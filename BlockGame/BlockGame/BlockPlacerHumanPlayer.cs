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
            referencePosRight = new Queue<SkeletonPoint>();
            referencePosLeft = new Queue<SkeletonPoint>();
        }

        private Queue<SkeletonPoint> referencePosLeft;
        private Queue<SkeletonPoint> referencePosRight;
        //Used to make it hardewr to go left right after moving right
        private PlayerMove lastMove = PlayerMove.NO_MOVE;

        //For now this returns a gamefield, needs to be changed
        public override PlayerMove PlaceBlock(Skeleton skel)
        {
            SkeletonPoint leftHand = skel.Joints[JointType.HandLeft].Position;
            SkeletonPoint rightHand = skel.Joints[JointType.HandRight].Position;
            referencePosLeft.Enqueue(leftHand);
            referencePosRight.Enqueue(rightHand);
            //TODO: Might not work as intended, please test and discuss
            if (referencePosRight.Count > 7 && referencePosLeft.Count > 7)
            {
                SkeletonPoint leftRef = referencePosLeft.Dequeue();
                SkeletonPoint rightRef = referencePosRight.Dequeue();
                if (rightHand.X > rightRef.X + 0.40)
                {
                    referencePosRight.Clear();
                    referencePosLeft.Clear();
                    if (lastMove != PlayerMove.GO_LEFT)
                    {
                        lastMove = PlayerMove.GO_RIGHT;
                        return PlayerMove.GO_RIGHT;
                    }
                }
                if (rightHand.X < rightRef.X - 0.40)
                {
                    referencePosRight.Clear();
                    referencePosLeft.Clear();
                    if (lastMove != PlayerMove.GO_RIGHT)
                    {
                        lastMove = PlayerMove.GO_LEFT;
                        return PlayerMove.GO_LEFT;
                    }
                }
                if (leftHand.X > leftRef.X + 0.40)
                {
                    referencePosLeft.Clear();
                    referencePosRight.Clear();
                    if (lastMove != PlayerMove.ROTATE_LEFT)
                    {
                        lastMove = PlayerMove.ROTATE_RIGHT;
                        return PlayerMove.ROTATE_RIGHT;
                    }
                }
                if (leftHand.X < leftRef.X - 0.40)
                {
                    referencePosLeft.Clear();
                    referencePosRight.Clear();
                    if (lastMove != PlayerMove.ROTATE_RIGHT)
                    {
                        lastMove = PlayerMove.ROTATE_LEFT;
                        return PlayerMove.ROTATE_LEFT;
                    }
                }
                lastMove = PlayerMove.NO_MOVE;
                if (leftHand.Y < leftRef.Y - 0.4 && rightHand.Y < rightRef.Y - 0.4)
                {
                    referencePosRight.Clear();
                    referencePosLeft.Clear();
                    return PlayerMove.GO_DOWN;
                }
            }
            return PlayerMove.NO_MOVE;
        }
    }
}
