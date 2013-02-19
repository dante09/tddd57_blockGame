using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;
using System.ComponentModel;

namespace BlockGame
{
    public enum PoseType
    {
        [Description("No pose")]NO_POSE, [Description("Square")] SQUARE
    }
    public struct PoseStatus
    {   

        static SkeletonPoint JointToSkeletonPoint(Skeleton skel, JointType t)
        {
            SkeletonPoint point = new SkeletonPoint();
            point.X = skel.Joints[t].Position.X;
            point.Y = skel.Joints[t].Position.Y;
            point.Z = skel.Joints[t].Position.Z;
            return point;
        }

        public PoseType closestPose { get; private set; }
        public double confidenceLevel { get; private set; }
        public SkeletonPoint[] pointsOfInterest { get; private set; }

        public PoseStatus(PoseType closestPose, double confidenceLevel, Skeleton skeleton) : this()
        {
            this.closestPose = closestPose;
            this.confidenceLevel = confidenceLevel;
            switch (closestPose)
            {
                case PoseType.SQUARE:
                    pointsOfInterest = new SkeletonPoint[2];
                    pointsOfInterest[0] = JointToSkeletonPoint(skeleton, JointType.ElbowLeft);
                    pointsOfInterest[1] = JointToSkeletonPoint(skeleton, JointType.ElbowRight);
                    break;
                case PoseType.NO_POSE:
                default:
                    pointsOfInterest = new SkeletonPoint[0];
                    break;  
            }
        }

        public override string ToString()
        {
            return "{closestPose: " + closestPose.ToString() + ", confidenceLevel: " + confidenceLevel + "}";
        }
    }
}
