using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Kinect;
using System.ComponentModel;

namespace BlockGame
{
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
            LEFT_WRIST_Y = 4,
            [Description("right wrist y position")]
            RIGHT_WRIST_Y = 5,
            [Description("shoulder center y position")]
            SHOULDER_CENTER_Y = 6,
            [Description("head y position")]
            HEAD_Y = 7,
            [Description("left wrist x position")]
            LEFT_WRIST_X = 8,
            [Description("right wrist x position")]
            RIGHT_WRIST_X = 9
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

            double[] handOverShoulderValues = { features[(int)Features.LEFT_WRIST_Y], 
                                                features[(int)Features.RIGHT_WRIST_Y], 
                                                features[(int)Features.SHOULDER_CENTER_Y] };

            bool handOverShoulder = handOverShoulderValues[0] > handOverShoulderValues[2] ||
                handOverShoulderValues[1] > handOverShoulderValues[2];
             
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

    class JPose : Pose
    {
        public JPose()
            : base(PoseType.J)
        {
        }

        public override double Evaluate(double[] features)
        {
            double[] values = { features[(int)Features.LEFT_ELBOW_ANGLE], 
                                  features[(int)Features.RIGHT_ELBOW_ANGLE], 
                                  features[(int)Features.LEFT_ARM_ANGLE], 
                                  features[(int)Features.RIGHT_ARM_ANGLE] };

            double[] leftArmValues = { values[0], values[2] };
            double[] expectedLeftArmValues = { 3, 0.5 };
            bool leftArmDown = Normalize(leftArmValues, expectedLeftArmValues) > 0.75;

            double[] rightArmValues = { values[1], values[3] };
            double[] expectedRightArmValues = { 3, 1.35 };

            return (!leftArmDown ? 0 : Normalize(rightArmValues, expectedRightArmValues));
        }
    }

    class TPose : Pose
    {
        public TPose()
            : base(PoseType.T)
        {
        }

        public override double Evaluate(double[] features)
        {
            double[] values = { features[(int)Features.LEFT_ELBOW_ANGLE], 
                                  features[(int)Features.RIGHT_ELBOW_ANGLE], 
                                  features[(int)Features.LEFT_ARM_ANGLE], 
                                  features[(int)Features.RIGHT_ARM_ANGLE] };
            double[] expectedValues = { 3, 3, 1.35, 1.35 };

            return Normalize(values, expectedValues);
        }
    }

    class IPose : Pose
    {
        public IPose()
            : base(PoseType.I)
        {
        }

        public override double Evaluate(double[] features)
        {
            double[] values = { features[(int)Features.LEFT_WRIST_Y], 
                                  features[(int)Features.RIGHT_WRIST_Y], 
                                  features[(int)Features.HEAD_Y], 
                                  features[(int)Features.LEFT_WRIST_X], 
                                  features[(int)Features.RIGHT_WRIST_X]};

            bool handsOverHead = values[0] > values[2] &&
    values[1] > values[2];

            double handDistance = Math.Sqrt(Math.Pow(values[0] - values[1], 2) + Math.Pow(values[3] - values[4], 2));

            return (handsOverHead ? 1 - handDistance : 0);
        }
    }

    class SPose : Pose
    {
        public SPose()
            : base(PoseType.S)
        {
        }

        public override double Evaluate(double[] features)
        {
            double[] values = { features[(int)Features.LEFT_WRIST_Y], 
                                  features[(int)Features.RIGHT_WRIST_Y],
                                  features[(int)Features.SHOULDER_CENTER_Y],
                                  features[(int)Features.LEFT_ELBOW_ANGLE], 
                                  features[(int)Features.RIGHT_ELBOW_ANGLE]};

            bool handsOverAndUnderShoulders = values[0] > values[2] &&
    values[1] < values[2];

            double[] elbowValues = { values[3], values[4] };
            double[] expectedElbowValues = { 1.7, 1.7 };

            return (handsOverAndUnderShoulders ? Normalize(elbowValues, expectedElbowValues) : 0);
        }
    }

    class ZPose : Pose
    {
        public ZPose()
            : base(PoseType.Z)
        {
        }

        public override double Evaluate(double[] features)
        {
            double[] values = { features[(int)Features.LEFT_WRIST_Y], 
                                  features[(int)Features.RIGHT_WRIST_Y],
                                  features[(int)Features.SHOULDER_CENTER_Y],
                                  features[(int)Features.LEFT_ELBOW_ANGLE], 
                                  features[(int)Features.RIGHT_ELBOW_ANGLE]};

            bool handsOverAndUnderShoulders = values[0] < values[2] &&
    values[1] > values[2];

            double[] elbowValues = { values[3], values[4] };
            double[] expectedElbowValues = { 1.7, 1.7 };

            return (handsOverAndUnderShoulders ? Normalize(elbowValues, expectedElbowValues) : 0);
        }
    }
}