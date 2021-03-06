﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace BlockGame
{
    class BlockCreationHumanPlayer : BlockCreationPlayer
    {
        private PoseType currentPose = PoseType.NO_POSE;
        private int errorCount = 0;

        public BlockCreationHumanPlayer()
            : base()
        {
            isHuman = true;
        }

        override public PoseStatus GetBlock(Skeleton skel)
        {
            PoseStatus poseStatus = PoseHandler.Evaluate(skel, shapeSelectionList);

            if (currentPose != PoseType.NO_POSE && errorCount <= 15 && currentPose != poseStatus.closestPose
                &&shapeSelectionList.Contains(currentPose))
            {
                errorCount++;
                return new PoseStatus(currentPose, 1, skel);
            }
            else
            {
                errorCount = 0;
                currentPose = poseStatus.closestPose;
                return poseStatus;
            }
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
                poses.Add(new IPose());
                poses.Add(new SPose());
                poses.Add(new ZPose());
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
                features[(int)Pose.Features.HEAD_Y] = skel.Joints[JointType.Head].Position.Y;
                features[(int)Pose.Features.HEAD_X] = skel.Joints[JointType.Head].Position.X;
                features[(int)Pose.Features.SHOULDER_CENTER_Y] = skel.Joints[JointType.ShoulderCenter].Position.Y;
                features[(int)Pose.Features.LEFT_WRIST_X] = skel.Joints[JointType.WristLeft].Position.X;
                features[(int)Pose.Features.RIGHT_WRIST_X] = skel.Joints[JointType.WristRight].Position.X;

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
