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
        [Description("No pose")]
        NO_POSE = 0,
        [Description("O-pose")]
        O = 1,
        [Description("L-pose")]
        L = 2,
        [Description("J-pose")]
        J = 3,
        [Description("T-pose")]
        T = 4,
        [Description("I-pose")]
        I = 5,
        [Description("S-pose")]
        S = 6,
        [Description("Z-pose")]
        Z = 7
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
                case PoseType.O:
                    pointsOfInterest = new SkeletonPoint[2];
                    pointsOfInterest[0] = skeleton.Joints[JointType.ElbowLeft].Position;
                    pointsOfInterest[1] = skeleton.Joints[JointType.ElbowRight].Position;
                    break;
                case PoseType.L:
                    pointsOfInterest = new SkeletonPoint[3];
                    pointsOfInterest[0] = skeleton.Joints[JointType.WristLeft].Position;
                    pointsOfInterest[1] = skeleton.Joints[JointType.ShoulderCenter].Position;
                    pointsOfInterest[2] = skeleton.Joints[JointType.Spine].Position;
                    break;
                case PoseType.J:
                    pointsOfInterest = new SkeletonPoint[3];
                    pointsOfInterest[0] = skeleton.Joints[JointType.WristRight].Position;
                    pointsOfInterest[1] = skeleton.Joints[JointType.ShoulderCenter].Position;
                    pointsOfInterest[2] = skeleton.Joints[JointType.Spine].Position;
                    break;
                case PoseType.T:
                    pointsOfInterest = new SkeletonPoint[2];
                    pointsOfInterest[0] = skeleton.Joints[JointType.WristLeft].Position;
                    pointsOfInterest[1] = skeleton.Joints[JointType.WristRight].Position;
                    break;
                case PoseType.I:
                    pointsOfInterest = new SkeletonPoint[3];
                    pointsOfInterest[0] = skeleton.Joints[JointType.WristLeft].Position;
                    pointsOfInterest[1] = skeleton.Joints[JointType.WristRight].Position;
                    pointsOfInterest[2] = skeleton.Joints[JointType.Spine].Position;
                    break;
                case PoseType.S:
                    pointsOfInterest = new SkeletonPoint[2];
                    pointsOfInterest[0] = skeleton.Joints[JointType.WristLeft].Position;
                    pointsOfInterest[1] = skeleton.Joints[JointType.WristRight].Position;
                    break;
                case PoseType.Z:
                    pointsOfInterest = new SkeletonPoint[2];
                    pointsOfInterest[0] = skeleton.Joints[JointType.WristLeft].Position;
                    pointsOfInterest[1] = skeleton.Joints[JointType.WristRight].Position;
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
