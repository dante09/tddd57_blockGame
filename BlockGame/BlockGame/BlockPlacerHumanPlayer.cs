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

        //For now this returns a gamefield, needs to be changed
        public override PlayerMove PlaceBlock(Skeleton skel)
        {
            SkeletonPoint leftHand = skel.Joints[JointType.HandLeft].Position;
            SkeletonPoint rightHand = skel.Joints[JointType.HandRight].Position;
            referencePosLeft.Enqueue(leftHand);
            referencePosRight.Enqueue(rightHand);
            if (referencePosRight.Count > 9 && referencePosLeft.Count > 9)
            {
                SkeletonPoint leftRef = referencePosLeft.Dequeue();
                SkeletonPoint rightRef = referencePosRight.Dequeue();
                if (rightHand.X > rightRef.X + 0.4)
                {
                    referencePosRight.Clear();
                    return PlayerMove.GO_RIGHT;
                }
                if (rightHand.X < rightRef.X - 0.4)
                {
                    referencePosRight.Clear();
                    return PlayerMove.GO_LEFT;
                }
                if (leftHand.X > leftRef.X + 0.4)
                {
                    referencePosLeft.Clear();
                    return PlayerMove.ROTATE_RIGHT;
                }
                if (leftHand.X < leftRef.X - 0.4)
                {
                    referencePosLeft.Clear();
                    return PlayerMove.ROTATE_LEFT;
                }
            }
            return PlayerMove.NO_MOVE;
        }
    }
}
