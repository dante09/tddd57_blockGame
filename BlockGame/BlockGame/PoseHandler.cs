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
        private const double CONFIDENCE_THRESHOLD = 0.70;

        static PoseHandler()
        {
            poses = new HashSet<Pose>();
            poses.Add(new OPose());
            poses.Add(new LPose());
        }

        public static PoseStatus Evaluate(Skeleton skel)
        {
            PoseType closestPose = PoseType.NO_POSE;
            double maxConfidence = CONFIDENCE_THRESHOLD;
            foreach(Pose pose in poses)
            {
                double tempConfidence = pose.Evaluate(skel);
                if (tempConfidence > CONFIDENCE_THRESHOLD)
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

        protected static double Normalize(double[] values, double[] expectedValues)
        {
            double[] weights = new double[values.Length];
            for (int i = 0; i < weights.Length; i++)
                weights[i] = 1;
            return Normalize(values, expectedValues, weights);
        }

        // Returns a confidence value between 0 and 1, where 1 is maximum confidence.
        protected static double Normalize(double[] values, double[] expectedValues, double[] weigths)
        {
            double total = 0;
            double totalMaxValue = 0;
            double maxValue = Math.PI / 6;
            for (int i = 0; i < values.Length; i++)
            {
                //double tempValue = Math.Max(Math.Abs(expectedValues[i]-values[i]),
                //    Math.Abs(expectedValues[i]-values[i]-Math.PI));
                totalMaxValue += weigths[i] * maxValue;
                double tempValue = Math.Abs(expectedValues[i] - values[i]);
                total += weigths[i] * (tempValue > maxValue ? maxValue : tempValue);
            }
            return 1 - total / totalMaxValue;
        }

        protected static double Angle(Joint a, Joint b, Joint c)
        {
            double[] ba = { a.Position.X - b.Position.X, a.Position.Y - b.Position.Y};
            double[] bc = { c.Position.X - b.Position.X, c.Position.Y - b.Position.Y};
            double absba = Math.Sqrt(ba[0] * ba[0] + ba[1] * ba[1]);
            double absbc = Math.Sqrt(bc[0] * bc[0] + bc[1] * bc[1]);

            return Math.Acos((ba[0] * bc[0] + ba[1] * bc[1]) / (absba * absbc));
        }
    }

    class OPose : Pose
    {

        public OPose() : base(PoseType.O)
        {
        }

        public override double Evaluate(Skeleton skel)
        {
            double[] values = new double[4];
            //left elbow angle
            values[0] = Angle(skel.Joints[JointType.ShoulderLeft], skel.Joints[JointType.ElbowLeft], skel.Joints[JointType.WristLeft]);
            //right elbow angle
            values[1] = Angle(skel.Joints[JointType.ShoulderRight], skel.Joints[JointType.ElbowRight], skel.Joints[JointType.WristRight]);
            //left arm angle
            values[2] = Angle(skel.Joints[JointType.ElbowLeft], skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.Spine]);
            //right arm angle
            values[3] = Angle(skel.Joints[JointType.ElbowRight], skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.Spine]);

            double[] expectedValues = { 1.75, 1.75, 1.35, 1.35 };
            bool handOverShoulder = skel.Joints[JointType.WristLeft].Position.Y >skel.Joints[JointType.ShoulderCenter].Position.Y ||
                skel.Joints[JointType.WristRight].Position.Y >skel.Joints[JointType.ShoulderCenter].Position.Y;
            return (handOverShoulder ? 0 : Normalize(values, expectedValues));
        }
    }
    class LPose : Pose
    {
        public LPose()
            : base(PoseType.L)
        {
        }

        public override double Evaluate(Skeleton skel)
        {
            double[] values = new double[4];
            //left elbow angle
            values[0] = Angle(skel.Joints[JointType.ShoulderLeft], skel.Joints[JointType.ElbowLeft], skel.Joints[JointType.WristLeft]);
            //right elbow angle
            values[1] = Angle(skel.Joints[JointType.ShoulderRight], skel.Joints[JointType.ElbowRight], skel.Joints[JointType.WristRight]);
            //left arm angle
            values[2] = Angle(skel.Joints[JointType.ElbowLeft], skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.Spine]);
            //right arm angle
            values[3] = Angle(skel.Joints[JointType.ElbowRight], skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.Spine]);

            double[] rightArmValues = { values[1], values[3] };
            double[] expectedRightArmValues = { 3, 0.5 };
            bool rightArmDown = Normalize(rightArmValues, expectedRightArmValues) > 0.75;

            double[] leftArmValues = { values[0], values[2] };
            double[] expectedLeftArmValues = { 3, 1.35 };

            return (!rightArmDown ? 0 : Normalize(leftArmValues, expectedLeftArmValues));
        }
    }
}