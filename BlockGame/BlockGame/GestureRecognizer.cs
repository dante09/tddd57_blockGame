using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Kinect;

namespace BlockGame
{
    abstract class GestureRecognizer : GameComponent
    {
        protected int maxErrors;
        protected int amountOfErrors;
        protected GetSkeleton getSkel;
        protected int gestureKeptTime;

        public int holdFor
        {
            get;
            protected set;
        }
        public bool gestureComplete
        {
            get;
            protected set;
        }

        public GestureRecognizer(Game game,GetSkeleton skel,int errorCount,int holdFor) : base(game)
        {
            this.maxErrors = errorCount;
            this.holdFor = holdFor;
            getSkel = skel;
            Reset();
        }

        //For some reason this did not work as a ordinary getter for gesturekeptTime
        public int GetGestureKeptTime()
        {
            if (gestureKeptTime - holdFor / 5 < 0)
                return 0;
            return gestureKeptTime;
        }

        public bool GestureStarted()
        {
            return gestureKeptTime >= holdFor/5;
        }

        public void Reset()
        {
            amountOfErrors = 0;
            gestureKeptTime = 0;
            gestureComplete = false;
        }
    }
}
