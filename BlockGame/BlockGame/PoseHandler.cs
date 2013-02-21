using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Kinect;
using System.ComponentModel;

namespace BlockGame
{
    //PoseHandler evaluates the current pose with the help of the different Pose-classes to determine the currently performed pose.
    public static class PoseHandler
    {

        private static HashSet<Pose> poses;
        private static double[] features;
        private const double CONFIDENCE_THRESHOLD = 0.70;

        static PoseHandler()
        {
            poses = new HashSet<Pose>();
            poses.Add(new OPose());
            poses.Add(new LPose());
        }

        public static PoseStatus Evaluate(Skeleton skel)
        {
            //All features are extracted once, and the pose evaluators then use the ones they are interested in.
            features = new double[Enum.GetNames(typeof(Pose.Features)).Length];
            features[(int)Pose.Features.LEFT_ARM_ANGLE] = Pose.Angle(skel.Joints[JointType.ElbowLeft], skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.Spine]);
            features[(int)Pose.Features.RIGHT_ARM_ANGLE] = Pose.Angle(skel.Joints[JointType.ElbowRight], skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.Spine]);
            features[(int)Pose.Features.LEFT_ELBOW_ANGLE] = Pose.Angle(skel.Joints[JointType.ShoulderLeft], skel.Joints[JointType.ElbowLeft], skel.Joints[JointType.WristLeft]);
            features[(int)Pose.Features.RIGHT_ELBOW_ANGLE] = Pose.Angle(skel.Joints[JointType.ShoulderRight], skel.Joints[JointType.ElbowRight], skel.Joints[JointType.WristRight]);

            PoseType closestPose = PoseType.NO_POSE;
            double maxConfidence = CONFIDENCE_THRESHOLD;
            foreach(Pose pose in poses)
            {
                double tempConfidence = pose.Evaluate(features);
                if (tempConfidence > CONFIDENCE_THRESHOLD)
                {
                    maxConfidence = tempConfidence;
                    closestPose = pose.poseType;
                }
            }

            return new PoseStatus(closestPose,maxConfidence,skel);
        }
    }

    //Pose classes are used in PoseHandler to evaluate which pose is currently performed.
    abstract class Pose
    {
        public abstract double Evaluate(double[] features);

        //Enumeration of features used in pose evaluation
        public enum Features
        {
            [Description("left arm angle")]
            LEFT_ARM_ANGLE = 0,
            [Description("right arm angle")]
            RIGHT_ARM_ANGLE = 1,
            [Description("left elbow angle")]
            LEFT_ELBOW_ANGLE = 2,
            [Description("right elbow angle")]
            RIGHT_ELBOW_ANGLE = 3,
            [Description("left wrist y position")]
            LEFT_WRIST_Y = 3,
            [Description("right wrist y position")]
            RIGHT_WRIST_Y = 3,
            [Description("shoulder center y position")]
            SHOULDER_CENTER_Y = 3
        }

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

        // Returns a confidence value between 0 and 1, where 1 is maximum confidence, based on the differences between the actual values and the target values. Each value is weighted by its respective weight.
        protected static double Normalize(double[] values, double[] targetValues, double[] weigths)
        {
            double total = 0;
            double totalMaxValue = 0;
            double maxValue = Math.PI / 6;
            for (int i = 0; i < values.Length; i++)
            {
                totalMaxValue += weigths[i] * maxValue;
                double tempValue = Math.Abs(targetValues[i] - values[i]);
                total += weigths[i] * (tempValue > maxValue ? maxValue : tempValue);
            }
            return 1 - total / totalMaxValue;
        }

        //Computes the angle between the vectors b->a and b->c in the xy-plane.
        public static double Angle(Joint a, Joint b, Joint c)
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

        public override double Evaluate(double[] features)
        {
            double[] values = { features[(int)Features.LEFT_ELBOW_ANGLE], 
                                  features[(int)Features.RIGHT_ELBOW_ANGLE], 
                                  features[(int)Features.LEFT_ARM_ANGLE], 
                                  features[(int)Features.RIGHT_ARM_ANGLE] };
            double[] expectedValues = { 1.75, 1.75, 1.35, 1.35 };

            bool handOverShoulder = false;
            /*
            bool handOverShoulder = skel.Joints[JointType.WristLeft].Position.Y >skel.Joints[JointType.ShoulderCenter].Position.Y ||
                skel.Joints[JointType.WristRight].Position.Y >skel.Joints[JointType.ShoulderCenter].Position.Y;
             */
            return (handOverShoulder ? 0 : Normalize(values, expectedValues));
        }
    }

    class LPose : Pose
    {
        public LPose()
            : base(PoseType.L)
        {
        }

        public override double Evaluate(double[] features)
        {
            double[] values = { features[(int)Features.LEFT_ELBOW_ANGLE], 
                                  features[(int)Features.RIGHT_ELBOW_ANGLE], 
                                  features[(int)Features.LEFT_ARM_ANGLE], 
                                  features[(int)Features.RIGHT_ARM_ANGLE] };

            double[] rightArmValues = { values[1], values[3] };
            double[] expectedRightArmValues = { 3, 0.5 };
            bool rightArmDown = Normalize(rightArmValues, expectedRightArmValues) > 0.75;

            double[] leftArmValues = { values[0], values[2] };
            double[] expectedLeftArmValues = { 3, 1.35 };

            return (!rightArmDown ? 0 : Normalize(leftArmValues, expectedLeftArmValues));
        }
    }
}