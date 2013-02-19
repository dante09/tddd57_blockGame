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
                    pointsOfInterest[0] = skeleton.Joints[JointType.ElbowLeft].Position;
                    pointsOfInterest[1] = skeleton.Joints[JointType.ElbowRight].Position;
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
