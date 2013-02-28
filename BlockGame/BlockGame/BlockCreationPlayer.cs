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
        protected const int MAX_LIST_SIZE = 5;
        //The minimum size of the shape selection list. When the list shrinks below this size, it will be repopulated.
        //0 < MIN_LIST_SIZE <= MAX_LIST_SIZE
        protected const int MIN_LIST_SIZE = 1;

        public BlockCreationPlayer()
        {
            shapeSelectionList = new List<PoseType>();
            shapeGenerator = new Random();
            //Populate the shapeSelectionList
            for (int i = 0; i < MAX_LIST_SIZE-1; i++)
                AddShape();
            shapeSelectionList.Add(PoseType.O);
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
