using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace BlockGame
{
    class BlockCreationHumanPlayer : BlockCreationPlayer
    {
        public BlockCreationHumanPlayer()
            : base()
        {
        }

        override public PoseStatus GetBlock(Skeleton skel)
        {
            System.Diagnostics.Debug.Write("Player sees: ");
            for (int i = 0; i < shapeSelectionList.Count; i++)
                System.Diagnostics.Debug.Write(shapeSelectionList[i].ToString()+" ");
            System.Diagnostics.Debug.WriteLine("");
            return PoseHandler.Evaluate(skel, shapeSelectionList);
        }

        //PoseHandler evaluates the current pose with the help of the different Pose-classes to determine the currently performed pose.
        private static class PoseHandler
        {

            private static HashSet<Pose> poses;
            private static double[] features;
            private const double CONFIDENCE_THRESHOLD = 0.70;

            static PoseHandler()
            {
                poses = new HashSet<Pose>();
                poses.Add(new OPose());
                poses.Add(new LPose());
                poses.Add(new JPose());
                poses.Add(new TPose());
            }

            public static PoseStatus Evaluate(Skeleton skel, List<PoseType> selectionList)
            {
                //All features are extracted once, and the pose evaluators then use the ones they are interested in.
                features = new double[Enum.GetNames(typeof(Pose.Features)).Length];
                features[(int)Pose.Features.LEFT_ARM_ANGLE] = Pose.Angle(skel.Joints[JointType.ElbowLeft], skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.Spine]);
                features[(int)Pose.Features.RIGHT_ARM_ANGLE] = Pose.Angle(skel.Joints[JointType.ElbowRight], skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.Spine]);
                features[(int)Pose.Features.LEFT_ELBOW_ANGLE] = Pose.Angle(skel.Joints[JointType.ShoulderLeft], skel.Joints[JointType.ElbowLeft], skel.Joints[JointType.WristLeft]);
                features[(int)Pose.Features.RIGHT_ELBOW_ANGLE] = Pose.Angle(skel.Joints[JointType.ShoulderRight], skel.Joints[JointType.ElbowRight], skel.Joints[JointType.WristRight]);
                features[(int)Pose.Features.LEFT_WRIST_Y] = skel.Joints[JointType.WristLeft].Position.Y;
                features[(int)Pose.Features.RIGHT_WRIST_Y] = skel.Joints[JointType.WristRight].Position.Y;
                features[(int)Pose.Features.SHOULDER_CENTER_Y] = skel.Joints[JointType.ShoulderCenter].Position.Y;

                PoseType closestPose = PoseType.NO_POSE;
                double maxConfidence = CONFIDENCE_THRESHOLD;
                foreach (Pose pose in poses)
                {
                    if (selectionList.Contains(pose.poseType))
                    {
                        double tempConfidence = pose.Evaluate(features);
                        if (tempConfidence > CONFIDENCE_THRESHOLD)
                        {
                            maxConfidence = tempConfidence;
                            closestPose = pose.poseType;
                        }
                    }
                }
                return new PoseStatus(closestPose, maxConfidence, skel);
            }
        }
    }
}
