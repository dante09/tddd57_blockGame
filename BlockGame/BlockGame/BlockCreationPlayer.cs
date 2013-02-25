using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;
using System.Collections;

namespace BlockGame
{
    abstract class BlockCreationPlayer : Player
    {
        public List<PoseType> shapeSelectionList { private set; get; }
        protected Random shapeGenerator;
        //The starting and maximum size of the shape selection list.
        protected const int MAX_LIST_SIZE = 3;
        //The minimum size of the shape selection list. When the list shrinks below this size, it will be repopulated.
        //0 < MIN_LIST_SIZE <= MAX_LIST_SIZE
        protected const int MIN_LIST_SIZE = 1;

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
                poses.Add(new JPose());
                poses.Add(new TPose());
                poses.Add(new IPose());
            }

            public static PoseStatus Evaluate(Skeleton skel)
            {
                //All features are extracted once, and the pose evaluators then use the ones they are interested in.
                features = new double[Enum.GetNames(typeof(Pose.Features)).Length];
                features[(int)Pose.Features.LEFT_ARM_ANGLE] = Pose.Angle(skel.Joints[JointType.ElbowLeft], skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.Spine]);
                features[(int)Pose.Features.RIGHT_ARM_ANGLE] = Pose.Angle(skel.Joints[JointType.ElbowRight], skel.Joints[JointType.ShoulderCenter], skel.Joints[JointType.Spine]);
                features[(int)Pose.Features.LEFT_ELBOW_ANGLE] = Pose.Angle(skel.Joints[JointType.ShoulderLeft], skel.Joints[JointType.ElbowLeft], skel.Joints[JointType.WristLeft]);
                features[(int)Pose.Features.RIGHT_ELBOW_ANGLE] = Pose.Angle(skel.Joints[JointType.ShoulderRight], skel.Joints[JointType.ElbowRight], skel.Joints[JointType.WristRight]);
                features[(int)Pose.Features.LEFT_WRIST_Y] = skel.Joints[JointType.WristLeft].Position.Y;
                features[(int)Pose.Features.RIGHT_WRIST_Y] = skel.Joints[JointType.WristRight].Position.Y;
                features[(int)Pose.Features.HEAD_Y] = skel.Joints[JointType.ShoulderCenter].Position.Y;

                PoseType closestPose = PoseType.NO_POSE;
                double maxConfidence = CONFIDENCE_THRESHOLD;
                foreach (Pose pose in poses)
                {
                    double tempConfidence = pose.Evaluate(features);
                    if (tempConfidence > CONFIDENCE_THRESHOLD)
                    {
                        maxConfidence = tempConfidence;
                        closestPose = pose.poseType;
                    }
                }

                return new PoseStatus(closestPose, maxConfidence, skel);
            }
        }

        public BlockCreationPlayer()
        {
            shapeSelectionList = new List<PoseType>();
            shapeGenerator = new Random();
            //Populate the shapeSelectionList
            for (int i = 0; i < MAX_LIST_SIZE; i++)
                AddShape();
        }

        public abstract PoseStatus GetBlock(Skeleton skel);

        protected void RepopulateList()
        {
            for (int i = shapeSelectionList.Count; i < MAX_LIST_SIZE; i++)
                AddShape();
        }

        public void RemoveShape(PoseType poseType)
        {
            if (!shapeSelectionList.Contains(poseType))
                throw new System.InvalidOperationException("shapeSelectionList does not contain any " + poseType.ToString() + " PoseType.");
            shapeSelectionList.Remove(poseType);
            if (shapeSelectionList.Count < MIN_LIST_SIZE)
                RepopulateList();
        }

        protected void AddShape()
        {
            if (shapeSelectionList.Count >= MAX_LIST_SIZE)
                throw new System.InvalidOperationException("Trying to insert shape while shapeSelectionList is full.");
            //Complicated expression to avoid the NO_POSE type.
            PoseType poseType = (PoseType)(1 + (shapeGenerator.Next(0, 999) * (Enum.GetNames(typeof(PoseType)).Length - 1)) / 1000);
            shapeSelectionList.Add(poseType);
        }
    }
}
