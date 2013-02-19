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
            double maxConfidence = 0.5;
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

        protected static double Angle(Joint a, Joint b, Joint c)
        {
            double[] ba = { a.Position.X - b.Position.X, a.Position.Y - b.Position.Y, a.Position.Z - b.Position.Z };
            double[] bc = { c.Position.X - b.Position.X, c.Position.Y - b.Position.Y, c.Position.Z - b.Position.Z };
            double absba = Math.Sqrt(ba[0] * ba[0] + ba[1] * ba[1] + ba[2] * ba[2]);
            double absbc = Math.Sqrt(bc[0] * bc[0] + bc[1] * bc[1] + bc[2] * bc[2]);

            return Math.Acos((ba[0] * bc[0] + ba[1] * bc[1] + ba[2] * bc[2]) / (absba * absbc));
        }
    }

    class SquarePose : Pose
    {

        public SquarePose() : base(PoseType.SQUARE)
        {
        }

        public override double Evaluate(Skeleton skel)
        {
            double leftElbowAngle = Angle(skel.Joints[JointType.ShoulderLeft], skel.Joints[JointType.ElbowLeft], skel.Joints[JointType.WristLeft]);
            double rightElbowAngle = Angle(skel.Joints[JointType.ShoulderRight], skel.Joints[JointType.ElbowRight], skel.Joints[JointType.WristRight]);
            double leftArmAngle = Angle(skel.Joints[JointType.ElbowLeft], skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.Spine]);
            double rightArmAngle = Angle(skel.Joints[JointType.ElbowRight], skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.Spine]);

            System.Diagnostics.Debug.WriteLine("leftElbow: " + leftElbowAngle + "\n" + "rightElbow: " + rightElbowAngle + "\n" + "leftArm: " + leftArmAngle + "\n" + "rightArm: " + rightArmAngle + "\n");

            return 0;
        }
    }
}