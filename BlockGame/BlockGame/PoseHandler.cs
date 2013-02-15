using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Kinect;

namespace BlockGame
{
    public static class PoseHandler
    {

        private static HashSet<Pose> poses;

        static PoseHandler()
        {
            poses = new HashSet<Pose>();
            poses.Add(new SquarePose());
        }

        public static PoseStatus Evaluate(Skeleton skel)
        {
            double maxConfidence = Double.MinValue;
            PoseType closestPose = PoseType.NO_POSE;
            foreach(Pose pose in poses)
            {
                double tempConfidence = pose.Evaluate(skel);
                if (tempConfidence > maxConfidence)
                {
                    maxConfidence = tempConfidence;
                    closestPose = pose.poseType;
                }
            }

            return new PoseStatus(closestPose,maxConfidence,skel);
        }


    }

    abstract class Pose
    {
        public abstract double Evaluate(Skeleton skel);
        public PoseType poseType
        {
            get;
            protected set;
        }
        public Pose(PoseType poseType)
        {
            this.poseType = poseType;

        }
    }

    class SquarePose : Pose
    {

        public SquarePose() : base(PoseType.SQUARE)
        {
        }

        public override double Evaluate(Skeleton skel)
        {
            double headY = 0, rightHandY = 0;
            foreach (Joint joint in skel.Joints)
            {
                if (joint.JointType == JointType.Head)
                {
                    headY = joint.Position.Y;
                }
                else if (joint.JointType == JointType.HandRight)
                {
                    rightHandY = joint.Position.Y;
                }
            }
            return Math.Abs(headY - rightHandY);
        }
    }
}