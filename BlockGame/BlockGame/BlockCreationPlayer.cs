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
        protected const int SHAPE_SELECTION_LIST_SIZE = 5;

        public BlockCreationPlayer()
        {
            shapeSelectionList = new List<PoseType>();
            shapeGenerator = new Random();
            //Populate the shapeSelectionList
            for (int i = 0; i < SHAPE_SELECTION_LIST_SIZE; i++)
                addShape();
        }

        public abstract PoseStatus GetBlock(Skeleton skel);

        public void replaceShape(PoseType poseType)
        {
            removeShape(poseType);
            addShape();
        }

        private void removeShape(PoseType poseType)
        {
            if (!shapeSelectionList.Contains(poseType))
                throw new System.InvalidOperationException("shapeSelectionList does not contain any " + poseType.ToString() + " PoseType.");
            shapeSelectionList.Remove(poseType);
        }

        protected void addShape()
        {
            if (shapeSelectionList.Count >= SHAPE_SELECTION_LIST_SIZE)
                throw new System.InvalidOperationException("Trying to insert shape while shapeSelectionList is full.");
            //Complicated expression to avoid the NO_POSE type.
            PoseType poseType = (PoseType)(1 + (shapeGenerator.Next(0, 999) * (Enum.GetNames(typeof(PoseType)).Length - 1)) / 1000);
            shapeSelectionList.Add(poseType);
        }
    }
}
