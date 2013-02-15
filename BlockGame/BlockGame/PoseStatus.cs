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

        static Vector2 JointToVector2(Skeleton skel, JointType t)
        {
            return new Vector2(skel.Joints[t].Position.X, skel.Joints[t].Position.Y);
        }

        PoseType closestPose;
        double confidenceLevel;
        Vector2[] pointsOfInterest;

        public PoseStatus(PoseType closestPose, double confidenceLevel, Skeleton skeleton)
        {
            this.closestPose = closestPose;
            this.confidenceLevel = confidenceLevel;
            switch (closestPose)
            {
                case PoseType.SQUARE:
                    pointsOfInterest = new Vector2[2];
                    pointsOfInterest[0] = JointToVector2(skeleton, JointType.ElbowLeft);
                    pointsOfInterest[0] = JointToVector2(skeleton, JointType.ElbowRight);
                    break;
                case PoseType.NO_POSE:
                default:
                    pointsOfInterest = new Vector2[0];
                    break;  
            }
        }

        public override string ToString()
        {
            return "{closestPose: " + closestPose.ToString() + ", confidenceLevel: " + confidenceLevel + "}";
        }
    }
}
